namespace Bio.Helpers.XLFRpt2.Srvc {
	using System;
	using System.IO;
	using System.Xml;
	using System.Web;
	using System.Collections;
  using System.Threading;
  using Bio.Helpers.XLFRpt2.Engine;
  using System.Collections.Generic;
  using Bio.Helpers.Common.Types;
  using System.Runtime.Remoting.Channels.Ipc;
  using System.Runtime.Remoting.Channels;
  using System.Runtime.Remoting;
  using System.Runtime.Remoting.Channels.Tcp;
  using System.Security.Principal;
  using System.Runtime.Serialization.Formatters;
  using System.Reflection;
  using Bio.Helpers.Common;
  using System.Net.NetworkInformation;

  public enum QueueCmd {
    Break = 1,
    Restart = 2
  };

  //public delegate void XLRQueueOnRemoveEventHandler(Object pOpener, IXLReportInst pReport);
  public class CXLRptItem {
    public String uid { get; set; }
    public String code { get; set; }
    public CParams prms { get; set; }
    public String usr { get; set; }
  }
  public abstract class CQueue {
    internal static String csInternalUserName = "XLR.Queue";
    internal static CQueue instOfQueue = null;
    protected CConfigSys _cfg = null;
    private CBackgroundThread1 _thread = null;
    private Object FOpener = null;
    private Dictionary<String, IRemoteProcInst> FRunningReports = null;
    private CQueueRemoteControl _remoteControl = null;

    private void log_msg(String msg) {
      if (this._cfg.msgLogWriter != null)
        this._cfg.msgLogWriter(msg);
    }
    private void log_err(Exception ex) {
      if (this._cfg.errLogWriter != null)
        this._cfg.errLogWriter(ex);
    }

    public CQueue(Object opener, CConfigSys cfg) {
      instOfQueue = this;
      this._cfg = cfg;
      this.FOpener = opener;
      this.FRunningReports = new Dictionary<String, IRemoteProcInst>();
      String v_srv_name = null;
      if (this.FOpener is Service)
        v_srv_name = (this.FOpener as Service).ServiceName;
      this._thread = new CBackgroundThread1(
        v_srv_name,
        this._cfg.adminEmail,
        this._cfg.smtp,
        this._cfg.errLogWriter, 
        this.processQueue);
		}

    internal static CQueue creQueue(Object opener, CConfigSys cfg) {
      CQueue rslt = null;
      Type vType = Type.GetType(cfg.queueImplementationType);
      if (vType != null) {
        Type[] parTypes = new Type[] { typeof(Object), typeof(CConfigSys)};
        Object[] parVals = new Object[] { opener, cfg };
        ConstructorInfo ci = vType.GetConstructor(parTypes);
        rslt = (CQueue)ci.Invoke(parVals);
      }
      return rslt;
    }


    protected abstract String doOnAdd(String rptCode, String sessionID, String userUID, String remoteIP, CParams prms, ThreadPriority pPriority);
    protected abstract void doOnGetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref Byte[] buff);
    protected abstract void doOnBreakReportInst(String rptUID, String userUID, String remoteIP);
    protected abstract void doOnRestartReportInst(String rptUID, String userUID, String remoteIP);
    protected abstract XmlDocument doOnGetQueue(String userUID, String remoteIP);
    protected abstract String doOnGetUsrRoles(String userUID);
    protected abstract String doOnCheckUsrLogin(String usr, String pwd);
    //protected abstract XmlDocument doOnGetRptTreeNode(String userUID, String remoteIP, String startPath);
    protected abstract void doOnAddQueueState(String rptUID, RemoteProcState newState, String newStateDesc, String userUID, String remoteIP);
    protected abstract void doOnDropReportInst(String rptUID, String userUID, String remoteIP);
    protected abstract Queue<CXLRptItem> doOnGetReportsReady();
    protected abstract Queue<CXLRptItem> doOnGetReportsRunning();
    protected abstract Queue<CXLRptItem> doOnGetReportsBreaking();
    protected abstract Queue<CXLRptItem> doOnGetReportsRestarting();
    protected abstract void doOnAddReportResult2DB(String rptUID, String fileName);
    protected abstract void doOnMarkRQCmdState(String rptUID, QueueCmd cmd);
    /// <summary>
    /// Добавляет отчет в очередь
    /// </summary>
    /// <param name="pReportCode">Код отчета</param>
    /// <param name="sessionID">ID сессии</param>
    /// <param name="userName">Имя пользователя</param>
    /// <param name="remoteIP">Адрес с которого вошел пользователь</param>
    /// <param name="prms">Параметры отчета</param>
    /// <param name="pPriority"></param>
    public String Add(String rptCode, String sessionID, String userUID, String remoteIP, CParams prms, ThreadPriority pPriority){
      this.log_msg(String.Format("Добавление отчета в очередь \"{0}\", sessID:\"{1}\", ip:\"{2}\", usr:\"{3}\"...", rptCode, sessionID, remoteIP, userUID));
      String rptUID = this.doOnAdd(rptCode, sessionID, userUID, remoteIP, prms, pPriority);
      this.log_msg(String.Format("Отчет \"{0}\" добавлен в очередь пользователем \"{1}\".", rptCode, userUID));
      return rptUID;
    }
    public void GetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref byte[] buff) {
      this.doOnGetReportResult(rptUID, userUID, remoteIP, ref fileName, ref buff);
    }
    public void Break(String rptUID, String userUID, String remoteIP) {
      this.log_msg(String.Format("Останов отчета rptUID:\"{0}\", ip:\"{2}\", usr:\"{3}\"...", rptUID, remoteIP, userUID));
      this.doOnBreakReportInst(rptUID, userUID, remoteIP);
      this.log_msg(String.Format("Отчет rptUID:\"{0}\" остановлен пользователем \"{1}\".", rptUID, userUID));
    }
    public void Restart(String rptUID, String userUID, String remoteIP) {
      this.log_msg(String.Format("Останов отчета rptUID:\"{0}\", ip:\"{2}\", usr:\"{3}\"...", rptUID, remoteIP, userUID));
      this.doOnRestartReportInst(rptUID, userUID, remoteIP);
      this.log_msg(String.Format("Отчет rptUID:\"{0}\" остановлен пользователем \"{1}\".", rptUID, userUID));
    }
    public void Drop(String rptUID, String userUID, String remoteIP) {
      this.log_msg(String.Format("Удаление отчета rptUID:\"{0}\", ip:\"{2}\", usr:\"{3}\"...", rptUID, remoteIP, userUID));
      this.doOnDropReportInst(rptUID, userUID, remoteIP);
      this.log_msg(String.Format("Отчет rptUID:\"{0}\" удален пользователем \"{1}\".", rptUID, userUID));
    }
    public XmlDocument GetQueue(String userUID, String remoteIP) { 
      return doOnGetQueue(userUID, remoteIP);
    }
    public XmlDocument GetRptTreeNode(String userUID, String remoteIP, String folderCode) { 
      return this._getRptTreeNode(userUID, remoteIP, this._cfg.rootRptPath, folderCode);
    }
    public String CheckUsrLogin(String usr, String pwd) {
      return this.doOnCheckUsrLogin(usr, pwd);
    }

    public void Start() {
      this._stopRequsted = false;
      this._thread.start();
    }

    private Boolean _stopRequsted = false;
    public void Stop() {
      //this._thread.stop();
      this._stopRequsted = true;
    }

    protected void remove_from_running(String rptUID) {
      if (!String.IsNullOrEmpty(rptUID) && this.FRunningReports.ContainsKey(rptUID.ToUpper())) {
        lock (this.FRunningReports) {
          if (this.FRunningReports.ContainsKey(rptUID.ToUpper())) {
            IRemoteProcInst v_rpt = this.FRunningReports[rptUID.ToUpper()];
            this.FRunningReports.Remove(rptUID.ToUpper());
            if (v_rpt.IsRunning)
              v_rpt.Abort(null);
            Thread t = new Thread(new ThreadStart(() => {
              Thread.Sleep(10 * 1000);
              v_rpt.Dispose();
            }));
            t.Start();
          }
        }
      }
    }

    protected void add_to_running(String rptUID, String rptCode, CParams prms, String usrUID, Int32 priority) {
      if (!this.FRunningReports.ContainsKey(rptUID)) {
        CXLReport rptBuilder = null;
        lock (this.FRunningReports) {
          this.log_msg(String.Format("Запуск отчета \"{0}\", UID:\"{1}\"...", rptCode, rptUID));
          try {
            CXLReportConfig rptCfg = CXLReportConfig.LoadFromFile(
              rptUID,
              rptCode,
              this._cfg.rootRptPath,
              this._cfg.workPath,
              this._cfg.connStr,
              null,
              usrUID,
              null,
              prms,
              !this._cfg.debugEnabled
            );
            rptBuilder = new CXLReport(rptCfg);
            rptBuilder.OnChangeState += new DlgXLReportOnChangeState(rptBuilder_OnChangeState);
            rptBuilder.OnTerminatingBuild += new DlgXLReportOnTerminatingBuild(rptBuilder_OnAfterBuild);
            rptBuilder.OnError += new DlgXLReportOnError(rptBuilder_OnError);
            this.FRunningReports.Add(rptBuilder.UID, rptBuilder);
          } catch (Exception ex) {
            String errMsg = String.Format("При инициализации отчета \"{0}\":\"{1}\".\n{2}", rptCode, rptUID, Utl.buildErrorLogMsg(ex, DateTime.Now));
            this.doOnAddQueueState(rptUID, RemoteProcState.Error, errMsg, csInternalUserName, Utl.getLocalIP());
            throw ex;
          }
          //this.log_msg(String.Format("Отчет \"{0}\" добавлен в очередь пользователем \"{1}\".", rptCode, userUID));
        }
        if (rptBuilder != null) {
          rptBuilder.Run((ThreadPriority)priority);
        }
      }
    }

    void rptBuilder_OnError(object pOpener, CXLReport pReport, Exception pEx) {
      //throw new NotImplementedException();
      this.log_msg(String.Format("В процессе построения отчета \"{0}\", UID:\"{1}\" - произошла ошибка.", pReport.RptDefinition.FullCode, pReport.UID));
      this.log_err(pEx);
    }

    void rptBuilder_OnAfterBuild(object opener, CXLReport report) {
      //throw new NotImplementedException();
      String v_uid = report.UID;
      String v_full_code = report.FullCode;
      RemoteProcState v_state = report.State;
      Boolean vDebug = report.RptDefinition.DebugIsOn;
      String vUID = report.UID.ToUpper();
      String rptFileName = report.LastResultFile;
      this.remove_from_running(vUID);
      if (report.State == RemoteProcState.Done) {
        this.doOnAddReportResult2DB(vUID, rptFileName);
        if (!vDebug)
          File.Delete(rptFileName);
      }
      String vDescState = enumHelper.GetFieldDesc(v_state);
      this.log_msg(String.Format("Построение отчета \"{0}\", UID:\"{1}\" - завершено. Состояние : \"{2}\".",
        v_full_code, v_uid, vDescState));
      if ((v_state == RemoteProcState.Done) && vDebug)
        this.log_msg(String.Format("\t - результат: \"{0}\"", rptFileName));
    }

    private void rptBuilder_OnChangeState(object pOpener, CXLReport pReport, string pText) {
      this.doOnAddQueueState(pReport.UID, pReport.State, pText, csInternalUserName, Utl.getLocalIP());
    }

    private void markBadReports() {
      lock (this.FRunningReports) {
        Queue<CXLRptItem> v_rpts = this.doOnGetReportsRunning();
        foreach (CXLRptItem itm in v_rpts) {
          if (!this.FRunningReports.ContainsKey(itm.uid))
            this.doOnAddQueueState(itm.uid, RemoteProcState.Error, "Неизвестная ошибка! Выполнение отчета было прервано по неизвестным причинам!", csInternalUserName, Utl.getLocalIP());
        }
      }
    }

    private void breakBreakingReports() {
      lock (this.FRunningReports) {
        Queue<CXLRptItem> v_rpts = this.doOnGetReportsBreaking();
        foreach (CXLRptItem itm in v_rpts) {
          if (this.FRunningReports.ContainsKey(itm.uid)) {
            this.FRunningReports[itm.uid].Abort(null);
          }
          this.doOnMarkRQCmdState(itm.uid, QueueCmd.Break);
        }
      }
    }

    private void restartRestartingReports() {
      Queue<CXLRptItem> v_rpts = this.doOnGetReportsRestarting();
      lock (this.FRunningReports) {
        foreach (CXLRptItem itm in v_rpts) {
          if (this.FRunningReports.ContainsKey(itm.uid)) {
            this.FRunningReports[itm.uid].Abort(() => {
              this.doOnAddQueueState(itm.uid, RemoteProcState.Redy, null, csInternalUserName, Utl.getLocalIP());
              this.doOnMarkRQCmdState(itm.uid, QueueCmd.Restart);
            });
          } else {
            this.doOnAddQueueState(itm.uid, RemoteProcState.Redy, null, csInternalUserName, Utl.getLocalIP());
            this.doOnMarkRQCmdState(itm.uid, QueueCmd.Restart);
          }
        }
      }
    }

    //private void removeFinishedReportsFromPool() {
    //  lock (this.FRunningReports) {
    //    CXLReport[] rpts = new CXLReport[this.FRunningReports.Count];
    //    this.FRunningReports.Values.CopyTo(rpts, 0);
    //    foreach (CXLReport itm in rpts) {
    //      if (itm.IsFinished){
    //        this.remove_from_running(itm.RptUID);
    //      }
    //    }
    //  }
    //}

    private String FLastErrorText = null;
    private void processQueue() {
      //Thread.Sleep(30 * 1000);
      try {
        this.markBadReports();
        this.breakBreakingReports();
        this.restartRestartingReports();
        //this.removeFinishedReportsFromPool();
        lock (this.FRunningReports) {
          Queue<CXLRptItem> v_rpts = this.doOnGetReportsReady();
          while (this.FRunningReports.Count < this._cfg.poolSize) {
            if (this._stopRequsted)
              break;
            if (v_rpts.Count > 0) {
              CXLRptItem v_item = v_rpts.Dequeue();
              this.add_to_running(v_item.uid, v_item.code, v_item.prms, v_item.usr, 0);
            } else
              break;
          }
          if (this._stopRequsted) { 
            //foreach(var rpt in this.FRunningReports)
            //  rpt.Value.Abort();
            Thread.CurrentThread.Abort();
          }
        }
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        String curErrorText = ex.Message;
        if (!String.Equals(curErrorText, this.FLastErrorText, StringComparison.CurrentCulture)) {
          this.log_err(ex);
          this.FLastErrorText = curErrorText;
        }
      }
    }

    /// <summary>
    /// Формируем ветку дерева отчетов
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    /// <param name="startPath"></param>
    /// <returns></returns>
    protected XmlDocument _getRptTreeNode(String userUID, String remoteIP, String rootPath, String folderCode) {
      String usrRoles = this.doOnGetUsrRoles(userUID);
      return CXLRptTreeNav.buildRptTreeNav(rootPath, folderCode, usrRoles);
    }

	}
}
