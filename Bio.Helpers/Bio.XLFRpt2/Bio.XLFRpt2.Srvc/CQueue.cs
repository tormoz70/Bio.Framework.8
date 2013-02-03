namespace Bio.Helpers.XLFRpt2.Srvc {
	using System;
	using System.IO;
	using System.Xml;
	using System.Threading;
  using Engine;
  using System.Collections.Generic;
  using Common.Types;
	using Common;

  public enum QueueCmd {
    Break = 1,
    Restart = 2
  };

  public class CXLRptItem {
    public String uid { get; set; }
    public String code { get; set; }
    public CParams prms { get; set; }
    public String usr { get; set; }
  }

  public abstract class CQueue {
    internal static String csInternalUserName = "XLR.Queue";
    internal static CQueue instOfQueue = null;
    protected CConfigSys cfg = null;
    private readonly CBackgroundThread _thread;
    private readonly Object _opener;
    private readonly Dictionary<String, IRemoteProcInst> _runningReports;

    private void log_msg(String msg) {
      if (this.cfg.msgLogWriter != null)
        this.cfg.msgLogWriter(msg);
    }
    private void log_err(Exception ex) {
      if (this.cfg.errLogWriter != null)
        this.cfg.errLogWriter(ex);
    }

    protected CQueue(Object opener, CConfigSys cfg) {
      instOfQueue = this;
      this.cfg = cfg;
      this._opener = opener;
      this._runningReports = new Dictionary<String, IRemoteProcInst>();
      String v_srvName = null;
      if (this._opener is Service)
        v_srvName = (this._opener as Service).ServiceName;
      this._thread = new CBackgroundThread(
        v_srvName,
        this.cfg.adminEmail,
        this.cfg.smtp,
        this.cfg.errLogWriter, 
        this._processQueue);
		}

    internal static CQueue creQueue(Object opener, CConfigSys cfg) {
      CQueue v_rslt = null;
      var v_type = Type.GetType(cfg.queueImplementationType);
      if (v_type != null) {
        var v_parTypes = new[] { typeof(Object), typeof(CConfigSys)};
        var v_parVals = new[] { opener, cfg };
        var v_ci = v_type.GetConstructor(v_parTypes);
        if (v_ci != null) v_rslt = (CQueue)v_ci.Invoke(v_parVals);
      }
      return v_rslt;
    }


    protected abstract String doOnAdd(String rptCode, String sessionID, String userUID, String remoteIP, CParams prms, ThreadPriority pPriority);
    protected abstract void doOnGetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref Byte[] buff);
    protected abstract void doOnBreakReportInst(String rptUID, String userUID, String remoteIP);
    protected abstract void doOnRestartReportInst(String rptUID, String userUID, String remoteIP);
    protected abstract XmlDocument doOnGetQueue(String userUID, String remoteIP);
    protected abstract String doOnGetUsrRoles(String userUID);
    protected abstract String doOnCheckUsrLogin(String usr, String pwd);
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
    /// <param name="rptCode"></param>
    /// <param name="sessionID">ID сессии</param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP">Адрес с которого вошел пользователь</param>
    /// <param name="prms">Параметры отчета</param>
    /// <param name="pPriority"></param>
    public String Add(String rptCode, String sessionID, String userUID, String remoteIP, CParams prms, ThreadPriority pPriority){
      this.log_msg(String.Format("Добавление отчета в очередь \"{0}\", sessID:\"{1}\", ip:\"{2}\", usr:\"{3}\"...", rptCode, sessionID, remoteIP, userUID));
      var v_rptUID = this.doOnAdd(rptCode, sessionID, userUID, remoteIP, prms, pPriority);
      this.log_msg(String.Format("Отчет \"{0}\" добавлен в очередь пользователем \"{1}\".", rptCode, userUID));
      return v_rptUID;
    }
    public void GetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref byte[] buff) {
      this.doOnGetReportResult(rptUID, userUID, remoteIP, ref fileName, ref buff);
    }
    public void Break(String rptUID, String userUID, String remoteIP) {
      this.log_msg(String.Format("Останов отчета rptUID:\"{0}\", ip:\"{1}\", usr:\"{2}\"...", rptUID, remoteIP, userUID));
      this.doOnBreakReportInst(rptUID, userUID, remoteIP);
      this.log_msg(String.Format("Отчет rptUID:\"{0}\" остановлен пользователем \"{1}\".", rptUID, userUID));
    }
    public void Restart(String rptUID, String userUID, String remoteIP) {
      this.log_msg(String.Format("Останов отчета rptUID:\"{0}\", ip:\"{1}\", usr:\"{2}\"...", rptUID, remoteIP, userUID));
      this.doOnRestartReportInst(rptUID, userUID, remoteIP);
      this.log_msg(String.Format("Отчет rptUID:\"{0}\" остановлен пользователем \"{1}\".", rptUID, userUID));
    }
    public void Drop(String rptUID, String userUID, String remoteIP) {
      this.log_msg(String.Format("Удаление отчета rptUID:\"{0}\", ip:\"{1}\", usr:\"{2}\"...", rptUID, remoteIP, userUID));
      this.doOnDropReportInst(rptUID, userUID, remoteIP);
      this.log_msg(String.Format("Отчет rptUID:\"{0}\" удален пользователем \"{1}\".", rptUID, userUID));
    }
    public XmlDocument GetQueue(String userUID, String remoteIP) { 
      return doOnGetQueue(userUID, remoteIP);
    }
    public XmlDocument GetRptTreeNode(String userUID, String remoteIP, String folderCode) { 
      return this._getRptTreeNode(userUID, remoteIP, this.cfg.rootRptPath, folderCode);
    }
    public String CheckUsrLogin(String usr, String pwd) {
      return this.doOnCheckUsrLogin(usr, pwd);
    }

    public void Start() {
      this._stopRequsted = false;
      this._thread.start();
    }

    private Boolean _stopRequsted;
    public void Stop() {
      this._stopRequsted = true;
    }

    protected void remove_from_running(String rptUID) {
      if (!String.IsNullOrEmpty(rptUID) && this._runningReports.ContainsKey(rptUID.ToUpper())) {
        lock (this._runningReports) {
          if (this._runningReports.ContainsKey(rptUID.ToUpper())) {
            var v_rpt = this._runningReports[rptUID.ToUpper()];
            this._runningReports.Remove(rptUID.ToUpper());
            if (v_rpt.IsRunning)
              v_rpt.Abort(null);
            var v_t = new Thread(() => {
              Thread.Sleep(10 * 1000);
              v_rpt.Dispose();
            });
            v_t.Start();
          }
        }
      }
    }

    protected void add_to_running(String rptUID, String rptCode, CParams prms, String usrUID, Int32 priority) {
      if (!this._runningReports.ContainsKey(rptUID)) {
        CXLReport v_rptBuilder;
        lock (this._runningReports) {
          this.log_msg(String.Format("Запуск отчета \"{0}\", UID:\"{1}\"...", rptCode, rptUID));
          try {
            var v_rptCfg = CXLReportConfig.LoadFromFile(
              rptUID,
              rptCode,
              this.cfg.rootRptPath,
              this.cfg.workPath,
              this.cfg.connStr,
              null,
              usrUID,
              null,
              prms,
              !this.cfg.debugEnabled
            );
            v_rptBuilder = new CXLReport(v_rptCfg);
            v_rptBuilder.OnChangeState += rptBuilder_OnChangeState;
            v_rptBuilder.OnTerminatingBuild += rptBuilder_OnAfterBuild;
            v_rptBuilder.OnError += rptBuilder_OnError;
            this._runningReports.Add(v_rptBuilder.UID, v_rptBuilder);
          } catch (Exception v_ex) {
            var v_errMsg = String.Format("При инициализации отчета \"{0}\":\"{1}\".\n{2}", rptCode, rptUID, Utl.buildErrorLogMsg(v_ex, DateTime.Now));
            this.doOnAddQueueState(rptUID, RemoteProcState.Error, v_errMsg, csInternalUserName, Utl.getLocalIP());
            throw;
          }
        }
        v_rptBuilder.Run((ThreadPriority)priority);
      }
    }

    void rptBuilder_OnError(object pOpener, CXLReport pReport, Exception pEx) {
      this.log_msg(String.Format("В процессе построения отчета \"{0}\", UID:\"{1}\" - произошла ошибка.", pReport.RptDefinition.FullCode, pReport.UID));
      this.log_err(pEx);
    }

    void rptBuilder_OnAfterBuild(object opener, CXLReport report) {
      var v_uid = report.UID;
      var v_fullCode = report.FullCode;
      var v_state = report.State;
      var v_debug = report.RptDefinition.DebugIsOn;
      var v_rptUID = report.UID.ToUpper();
      var v_rptFileName = report.LastResultFile;
      this.remove_from_running(v_rptUID);
      if (report.State == RemoteProcState.Done) {
        this.doOnAddReportResult2DB(v_rptUID, v_rptFileName);
        if (!v_debug)
          File.Delete(v_rptFileName);
      }
      var v_descState = enumHelper.GetFieldDesc(v_state);
      this.log_msg(String.Format("Построение отчета \"{0}\", UID:\"{1}\" - завершено. Состояние : \"{2}\".",
        v_fullCode, v_uid, v_descState));
      if ((v_state == RemoteProcState.Done) && v_debug)
        this.log_msg(String.Format("\t - результат: \"{0}\"", v_rptFileName));
    }

    private void rptBuilder_OnChangeState(Object opener, CXLReport report, string text) {
      this.doOnAddQueueState(report.UID, report.State, text, csInternalUserName, Utl.getLocalIP());
    }

    private void markBadReports() {
      lock (this._runningReports) {
        var v_rpts = this.doOnGetReportsRunning();
        foreach (var v_itm in v_rpts) {
          if (!this._runningReports.ContainsKey(v_itm.uid))
            this.doOnAddQueueState(v_itm.uid, RemoteProcState.Error, "Неизвестная ошибка! Выполнение отчета было прервано по неизвестным причинам!", csInternalUserName, Utl.getLocalIP());
        }
      }
    }

    private void breakBreakingReports() {
      lock (this._runningReports) {
        var v_rpts = this.doOnGetReportsBreaking();
        foreach (var v_itm in v_rpts) {
          if (this._runningReports.ContainsKey(v_itm.uid)) {
            this._runningReports[v_itm.uid].Abort(null);
          }
          this.doOnMarkRQCmdState(v_itm.uid, QueueCmd.Break);
        }
      }
    }

    private void restartRestartingReports() {
      var v_rpts = this.doOnGetReportsRestarting();
      lock (this._runningReports) {
        foreach (var v_itm in v_rpts) {
          if (this._runningReports.ContainsKey(v_itm.uid)) {
            var v_itm1 = v_itm;
            this._runningReports[v_itm.uid].Abort(() => {
              this.doOnAddQueueState(v_itm1.uid, RemoteProcState.Redy, null, csInternalUserName, Utl.getLocalIP());
              this.doOnMarkRQCmdState(v_itm1.uid, QueueCmd.Restart);
            });
          } else {
            this.doOnAddQueueState(v_itm.uid, RemoteProcState.Redy, null, csInternalUserName, Utl.getLocalIP());
            this.doOnMarkRQCmdState(v_itm.uid, QueueCmd.Restart);
          }
        }
      }
    }

    private String _lastErrorText;
    private void _processQueue() {
      try {
        this.markBadReports();
        this.breakBreakingReports();
        this.restartRestartingReports();
        lock (this._runningReports) {
          var v_rpts = this.doOnGetReportsReady();
          while (this._runningReports.Count < this.cfg.poolSize) {
            if (this._stopRequsted)
              break;
            if (v_rpts.Count > 0) {
              var v_item = v_rpts.Dequeue();
              this.add_to_running(v_item.uid, v_item.code, v_item.prms, v_item.usr, 0);
            } else
              break;
          }
          if (this._stopRequsted) { 
            Thread.CurrentThread.Abort();
          }
        }
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception v_ex) {
        var v_curErrorText = v_ex.Message;
        if (!String.Equals(v_curErrorText, this._lastErrorText, StringComparison.CurrentCulture)) {
          this.log_err(v_ex);
          this._lastErrorText = v_curErrorText;
        }
      }
    }

    /// <summary>
    /// Формируем ветку дерева отчетов
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    /// <param name="rootPath"></param>
    /// <param name="folderCode"></param>
    /// <returns></returns>
    protected XmlDocument _getRptTreeNode(String userUID, String remoteIP, String rootPath, String folderCode) {
      var v_usrRoles = this.doOnGetUsrRoles(userUID);
      return CXLRptTreeNav.buildRptTreeNav(rootPath, folderCode, v_usrRoles);
    }

	}
}
