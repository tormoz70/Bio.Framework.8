namespace Bio.Helpers.DOA {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
using System.ComponentModel;
  using Bio.Helpers.Common.Types;
  using System.Threading;
  using System.Web;
  using System.Runtime.Remoting.Contexts;
  using Bio.Helpers.Common;

  public delegate void DlgSQLGarbageMonitorCheckItem(CSQLCmd vSQLCmd, ref Boolean killQuery, ref Boolean killSession, Boolean ajaxTimeoutExceeded);
  public class CSQLGarbageMonitorItem {
    public DateTime RegTime = DateTime.Now;
    public CSQLCmd SQLCmd = null;
    public DlgSQLGarbageMonitorCheckItem CheckItemProc = null;
    public int AjaxRequestTimeout = 0;
  }

  /// <summary>
  /// Данный монитор отслеживает запросы к БД, которые зависли и убивает их
  /// </summary>
  //[Synchronization]
  public class CSQLGarbageMonitor: CDisposableObject {

    private const int ciSleepMonitor = 5000;
    private List<CSQLGarbageMonitorItem> FList = null;
    private Thread FThread = null;

    public CSQLGarbageMonitor() {
      this.FList = new List<CSQLGarbageMonitorItem>();
      //this.FTimer = new Timer(this.monitorList, null, 500, 5000);
      this.FThread = new Thread(new ThreadStart(this.monitorList));
      this.FThread.Start();
      //this.FTimer.WorkerSupportsCancellation = true;
      //this.FTimer. += this.monitorList;
      //this.FBackgroundWorker.RunWorkerAsync();
    }

    private const String csSQLGarbageMonitorNAME = "BioSQLGarbageMonitor";
    public static CSQLGarbageMonitor GetSQLGarbageMonitor(HttpContext context) {
      var vMonitor = context.Application.Get(csSQLGarbageMonitorNAME) as CSQLGarbageMonitor;
      if (vMonitor == null) {
        vMonitor = new CSQLGarbageMonitor();
        context.Application.Add(csSQLGarbageMonitorNAME, vMonitor);
      }
      return vMonitor;
    }

    protected override void OnDispose() {
      base.OnDispose();
      this.FThread.Abort();
      //this.FBackgroundWorker.CancelAsync();
    }



    private CSQLGarbageMonitorItem findItem(CSQLCmd pSQLCmd) {
      CSQLGarbageMonitorItem[] vList = null;
      lock (this.FList) {
        vList = this.FList.ToArray<CSQLGarbageMonitorItem>();
      }
      foreach (CSQLGarbageMonitorItem vItem in vList) {
        if (vItem.SQLCmd.Equals(pSQLCmd))
          return vItem;
      }
      return null;
    }

    private Boolean isSQLCmdRegistred(CSQLCmd pSQLCmd) {
      CSQLGarbageMonitorItem[] vList = null;
      lock (this.FList) {
        vList = this.FList.ToArray<CSQLGarbageMonitorItem>();
      }
      foreach (CSQLGarbageMonitorItem vItem in vList) {
        if (vItem.SQLCmd.Equals(pSQLCmd))
          return true;
      }
      return false;
    }

    public void RegisterSQLCmd(CSQLCmd pSQLCmd, DlgSQLGarbageMonitorCheckItem pCheckItemProc, int pAjaxRequestTimeout) {
      if(!this.isSQLCmdRegistred(pSQLCmd)) {
        lock(this.FList) {
          this.FList.Add(new CSQLGarbageMonitorItem() { SQLCmd = pSQLCmd, CheckItemProc = pCheckItemProc, AjaxRequestTimeout = pAjaxRequestTimeout });
          pSQLCmd.garbageMonitor = this;
        }
      }
    }

    public Exception LastError { get; private set; }

    public void RemoveItem(CSQLGarbageMonitorItem pItem) {
      if (pItem != null)
        lock (this.FList) {
          this.FList.Remove(pItem);
        }
    }

    public void RemoveItem(CSQLCmd pSQLCmd) {
      CSQLGarbageMonitorItem vItem = this.findItem(pSQLCmd);
      this.RemoveItem(vItem);
    }

    private int calcDueMSecs(TimeSpan pDue){
      return (pDue.Hours*3600 + pDue.Minutes*60 + pDue.Seconds) * 1000;
    }

    private void monitorList() {
      while (true) {
        //if(this.FBackgroundWorker.CancellationPending) break;
        try {
          CSQLGarbageMonitorItem[] vList = null;
          lock (this.FList) {
            vList = this.FList.ToArray<CSQLGarbageMonitorItem>();
          }
          //Utl.AppendStringToFile(@"d:\we\tmp\garbageMonitor_sys.log", "Ping-start, vList.Length:" + vList.Length, null);
          foreach (CSQLGarbageMonitorItem vItem in vList) {
            Boolean killQuery = false; Boolean killSession = false;
            try {
              //Utl.AppendStringToFile(@"e:\we\tmp\garbageMonitor_sys.log", "CheckItemProc, vItem:" + (vItem.SQLCmd as CSQLCursorBio).IOCode, null);
              Boolean vAjaxTimeoutExceeded = false;
              if (vItem.AjaxRequestTimeout > 0) {
                int vDue = this.calcDueMSecs(DateTime.Now - vItem.RegTime);
                vAjaxTimeoutExceeded = vDue > vItem.AjaxRequestTimeout*1000;
              }
              vItem.CheckItemProc(vItem.SQLCmd, ref killQuery, ref killSession, vAjaxTimeoutExceeded);
              //Utl.AppendStringToFile(@"e:\we\tmp\garbageMonitor_sys.log", "CheckItemProc, killQuery:" + killQuery, null);
              if (killQuery) {
                vItem.SQLCmd.Cancel();
                //Utl.AppendStringToFile(@"e:\we\tmp\garbageMonitor.log", "SQLCmd.Canceled", null);
                if (killSession)
                  vItem.SQLCmd.Connection.Close();
                this.RemoveItem(vItem);
              }
            } catch (ThreadAbortException) {
              throw;
            } catch (Exception ex) {
              this.LastError = ex;
            }
          }
        } catch (ThreadAbortException) {
          break;
        } catch (Exception ex) {
          this.LastError = ex;
        }
        Thread.Sleep(ciSleepMonitor);
      }
    }
  }
}
