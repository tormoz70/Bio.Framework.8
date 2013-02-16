namespace Bio.Helpers.DOA {

  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Common.Types;
  using System.Threading;
  using System.Web;

  /// <summary>
  /// Делегат для управления запущенными запросами
  /// </summary>
  /// <param name="sqlCmd"></param>
  /// <param name="killQuery"></param>
  /// <param name="killSession"></param>
  /// <param name="ajaxTimeoutExceeded"></param>
  public delegate void DlgSQLGarbageMonitorCheckItem(SQLCmd sqlCmd, ref Boolean killQuery, ref Boolean killSession, Boolean ajaxTimeoutExceeded);
  /// <summary>
  /// Отследиваемый запрос
  /// </summary>
  public class SQLGarbageMonitorItem {
    /// <summary>
    /// Кол-во секунд, которые надо ждать выполнения запроса
    /// </summary>
    public int ajaxRequestTimeout = 0;
    /// <summary>
    /// Делегат для управления поведением монитора при проверке запроса
    /// </summary>
    public DlgSQLGarbageMonitorCheckItem checkItemProc = null;
    /// <summary>
    /// Дата/Время пегистрации
    /// </summary>
    public DateTime regTime = DateTime.Now;
    /// <summary>
    /// SQL запрос, которые необходимо отследить
    /// </summary>
    public SQLCmd sqlCmd = null;
  }

  /// <summary>
  /// Данный монитор отслеживает запросы к БД, которые зависли и убивает их
  /// </summary>
  //[Synchronization]
  public class SQLGarbageMonitor: DisposableObject {

    private const int _SLEEP_MONITOR = 5000;
    private const String _MONITOR_NAME = "BioSQLGarbageMonitor";
    private readonly List<SQLGarbageMonitorItem> _list;
    private readonly Thread _thread;

    /// <summary>
    /// Конструктор
    /// </summary>
    public SQLGarbageMonitor() {
      this._list = new List<SQLGarbageMonitorItem>();
      this._thread = new Thread(this.monitorList);
      this._thread.Start();
    }

    /// <summary>
    /// Последняя ошибка
    /// </summary>
    public Exception LastError { get; private set; }

    /// <summary>
    /// Возвращает монитор
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static SQLGarbageMonitor GetSQLGarbageMonitor(HttpContext context) {
      var v_monitor = context.Application.Get(_MONITOR_NAME) as SQLGarbageMonitor;
      if (v_monitor == null) {
        v_monitor = new SQLGarbageMonitor();
        context.Application.Add(_MONITOR_NAME, v_monitor);
      }
      return v_monitor;
    }

    protected override void doOnDispose() {
      base.doOnDispose();
      this._thread.Abort();
    }

    private SQLGarbageMonitorItem findItem(SQLCmd sqlCmd) {
      SQLGarbageMonitorItem[] v_list;
      lock (this._list) {
        v_list = this._list.ToArray<SQLGarbageMonitorItem>();
      }
      foreach (var v_item in v_list) {
        if (v_item.sqlCmd.Equals(sqlCmd))
          return v_item;
      }
      return null;
    }

    private Boolean isSQLCmdRegistred(SQLCmd sqlCmd) {
      SQLGarbageMonitorItem[] v_list;
      lock (this._list) {
        v_list = this._list.ToArray<SQLGarbageMonitorItem>();
      }
      foreach (var v_item in v_list) {
        if (v_item.sqlCmd.Equals(sqlCmd))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Регистрируе SQL запрос в мониторе для отслеживания времени выполнения
    /// </summary>
    /// <param name="sqlCmd"></param>
    /// <param name="checkItemProc"></param>
    /// <param name="ajaxRequestTimeout"></param>
    public void RegisterSQLCmd(SQLCmd sqlCmd, DlgSQLGarbageMonitorCheckItem checkItemProc, int ajaxRequestTimeout) {
      if(!this.isSQLCmdRegistred(sqlCmd)) {
        lock(this._list) {
          this._list.Add(new SQLGarbageMonitorItem { sqlCmd = sqlCmd, checkItemProc = checkItemProc, ajaxRequestTimeout = ajaxRequestTimeout });
          sqlCmd.garbageMonitor = this;
        }
      }
    }

    /// <summary>
    /// Удалить запрос из списка отслеживаемых
    /// </summary>
    /// <param name="item"></param>
    public void RemoveItem(SQLGarbageMonitorItem item) {
      if (item != null)
        lock (this._list) {
          this._list.Remove(item);
        }
    }

    /// <summary>
    /// Удалить запрос из списка отслеживаемых
    /// </summary>
    /// <param name="sqlCmd"></param>
    public void RemoveItem(SQLCmd sqlCmd) {
      var v_item = this.findItem(sqlCmd);
      this.RemoveItem(v_item);
    }

    private static int calcDueMSecs(TimeSpan pDue){
      return (pDue.Hours*3600 + pDue.Minutes*60 + pDue.Seconds) * 1000;
    }

    private void monitorList() {
      while (true) {
        try {
          SQLGarbageMonitorItem[] v_list;
          lock (this._list) {
            v_list = this._list.ToArray<SQLGarbageMonitorItem>();
          }
          foreach (var v_item in v_list) {
            var v_killQuery = false; var v_killSession = false;
            try {
              var v_ajaxTimeoutExceeded = false;
              if (v_item.ajaxRequestTimeout > 0) {
                var v_due = calcDueMSecs(DateTime.Now - v_item.regTime);
                v_ajaxTimeoutExceeded = v_due > v_item.ajaxRequestTimeout*1000;
              }
              v_item.checkItemProc(v_item.sqlCmd, ref v_killQuery, ref v_killSession, v_ajaxTimeoutExceeded);
              if (v_killQuery) {
                v_item.sqlCmd.Cancel();
                if (v_killSession)
                  v_item.sqlCmd.Connection.Close();
                this.RemoveItem(v_item);
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
        Thread.Sleep(_SLEEP_MONITOR);
      }
    }
  }
}
