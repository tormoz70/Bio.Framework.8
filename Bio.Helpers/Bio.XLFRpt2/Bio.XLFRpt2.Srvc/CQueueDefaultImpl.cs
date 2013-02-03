using System;
using System.Collections.Generic;
using System.Text;
using Bio.Helpers.Common.Types;
using System.Threading;
using Bio.Helpers.Common;
using System.Xml;
using Bio.Helpers.DOA;
using System.IO;
using System.Data;

namespace Bio.Helpers.XLFRpt2.Srvc {

  class CQueueDefaultImpl : CQueue {
    public CQueueDefaultImpl(Object pOpener, CConfigSys cfg) :
      base(pOpener, cfg) { }

    private static String _paramTypeEncode(Type paramType) {
      var v_prmTypeStr = "A";
      if (paramType != null) {
        if (Utl.TypeIsNumeric(paramType))
          v_prmTypeStr = "N";
        else if (paramType == typeof(DateTime))
          v_prmTypeStr = "D";
      }
      return v_prmTypeStr;
    }

    private static Object _paramDecode(String paramType, String paramValue) {
      Object v_prmValue = paramValue;
      if (paramType != null) {
        if (paramType.Equals("N"))
          v_prmValue = Utl.Convert2Type<Decimal>(paramValue);
        else if (paramType.Equals("D"))
          v_prmValue = Utl.Convert2Type<DateTime>(paramValue);
      }
      return v_prmValue;
    }

    private static void _addRptParams(IDbConnection conn, String rptUID, CParams prms, String userUID, String remoteIP) {
      var v_prms = new CParams();
      v_prms.SetValue("p_rpt_uid", rptUID);
      var v_sql = "begin xlr.clear_rparams(:p_rpt_uid); end;";
      SQLCmd.ExecuteScript(conn, v_sql, v_prms, 120);
      v_sql = "begin xlr.add_rparam(:p_rpt_uid, :p_prm_name, :p_prm_type, :p_prm_val, :p_usr_uid, :p_remote_ip); end;";
      foreach (var v_prm in prms) {
        v_prms.SetValue("p_prm_name", v_prm.Name);
        String v_prmValue;
        var v_prmTypeStr = "A";
        if (v_prm.Value != null) {
          var v_prmType = v_prm.ParamType ?? v_prm.Value.GetType();
          if (Utl.TypeIsNumeric(v_prmType)) {
            v_prmTypeStr = "N";
            v_prmValue = "" + v_prm.Value;
            v_prmValue = v_prmValue.Replace(",", ".");
          } else if (v_prmType == typeof(DateTime)) {
            v_prmTypeStr = "D";
            v_prmValue = ((DateTime)v_prm.Value).ToString("yyyy.MM.dd HH:mm:ss");
          } else
            v_prmValue = "" + v_prm.Value;
        } else
          continue;
        v_prms.SetValue("p_prm_type", v_prmTypeStr);
        v_prms.SetValue("p_prm_val", v_prmValue);
        v_prms.SetValue("p_usr_uid", userUID);
        v_prms.SetValue("p_remote_ip", remoteIP);
        SQLCmd.ExecuteScript(conn, v_sql, v_prms, 120);
      }
    }

    /// <summary>
    /// Тут добавляем отчет в "очередь", которая обрабатывается построителем отчетов
    /// </summary>
    /// <param name="rptCode"></param>
    /// <param name="sessionID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    /// <param name="prms"></param>
    /// <param name="pPriority"></param>
    /// <returns></returns>
    protected override String doOnAdd(String rptCode, String sessionID, String userUID, String remoteIP, CParams prms, ThreadPriority pPriority) {
      String v_rptUID;
      var v_conn = this.cfg.dbSession.GetConnection();
      try {
        const string sql = "begin xlr.add_rpt(:p_rpt_uid, :p_rpt_code, :p_rpt_prms, :p_rpt_desc, :p_usr_uid, :p_remote_ip); end;";
        var v_prms = new CParams();
        v_prms.Add(new CParam("p_rpt_uid", null, typeof(String), ParamDirection.InputOutput));
        v_prms.Add("p_rpt_code", rptCode);
        v_prms.Add("p_rpt_desc", null);
        v_prms.Add("p_usr_uid", userUID);
        v_prms.Add("p_remote_ip", remoteIP);
        SQLCmd.ExecuteScript(v_conn, sql, v_prms, 120);
        v_rptUID = v_prms.ValAsStrByName("p_rpt_uid", true);
        _addRptParams(v_conn, v_rptUID, prms, userUID, remoteIP);
      } finally {
        v_conn.Close();
      }
      return v_rptUID;
    }

    /// <summary>
    /// Тут вытаскиваем из очереди файл готового отчета
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    /// <param name="fileName"></param>
    /// <param name="buff"></param>
    protected override void doOnGetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref Byte[] buff) {
      const string sql = "SELECT a.rpt_result_fn, a.rpt_result_len, a.rpt_result" +
                         " FROM rpt$queue_rslts a"+
                         " WHERE a.rpt_uid = :rpt_uid";
      var v_prms = new CParams();
      v_prms.Add("rpt_uid", rptUID);
      var v_cur = new SQLCmd(this.cfg.dbSession);
      v_cur.Init(sql, v_prms);
      v_cur.Open(120);
      try {
        if (v_cur.Next()) {
          fileName = (String)v_cur.DataReader.GetValue(0);
          var v_szAct = v_cur.DataReader.GetDecimal(1);
          var v_szActInt = (int)v_szAct;
          buff = new byte[v_szActInt];
          v_cur.DataReader.GetBytes(2, 0, buff, 0, v_szActInt);
        }
      } finally {
        v_cur.Close();
      }
    }

    /// <summary>
    /// Тут засовываем в очередь файл готового отчета
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="fileName"></param>
    protected override void doOnAddReportResult2DB(String rptUID, String fileName) {
      const string sql = "begin xlr.save_rpt_file(:p_rpt_uid, :p_file_name, :p_file); end;";
      var v_prms = new CParams();
      v_prms.Add("p_rpt_uid", rptUID);
      v_prms.Add("p_file_name", Path.GetFileName(fileName));
      byte[] v_buffer = null;
      Utl.ReadBinFileInBuffer(fileName, ref v_buffer);
      v_prms.Add("p_file", v_buffer);
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
    }

    /// <summary>
    /// Останавливаем выполняющийся отчет
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnBreakReportInst(String rptUID, String userUID, String remoteIP) {
      const string sql = "begin xlr.break_rpt(:p_rpt_uid, :p_usr_uid, :p_remote_ip); end;";
      var v_prms = new CParams();
      v_prms.Add("p_rpt_uid", rptUID);
      v_prms.Add("p_usr_uid", userUID);
      v_prms.Add("p_remote_ip", remoteIP);
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
    }

    /// <summary>
    /// Перезапуск отчета
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnRestartReportInst(String rptUID, String userUID, String remoteIP) {
      const string sql = "begin xlr.restart_rpt(:p_rpt_uid, :p_usr_uid, :p_remote_ip); end;";
      var v_prms = new CParams();
      v_prms.Add("p_rpt_uid", rptUID);
      v_prms.Add("p_usr_uid", userUID);
      v_prms.Add("p_remote_ip", remoteIP);
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
    }

    /// <summary>
    /// Формируем список отчетов в очереди
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    /// <returns></returns>
    protected override XmlDocument doOnGetQueue(String userUID, String remoteIP) {
      var v_rslt = dom4cs.NewDocument("queue").XmlDoc;
      if (v_rslt.DocumentElement != null) {
        v_rslt.DocumentElement.SetAttribute("usr", userUID);
        v_rslt.DocumentElement.SetAttribute("remote_ip", remoteIP);
        v_rslt.DocumentElement.SetAttribute("count", "0");
      }
      const string sql = "select * from table(xlr.rqueue_xml(null,:p_usr_uid))";
      var v_prms = new CParams();
      v_prms.Add("p_usr_uid", userUID);
      var v_cur = new SQLCmd(this.cfg.dbSession);
      v_cur.Init(sql, v_prms);
      v_cur.Open(120);
      var v_sb = new StringBuilder();
      try{
        var v_cnt = 0;
        while (v_cur.Next()) {
          v_sb.AppendLine(v_cur.DataReader.GetString(0));
          v_cnt++;
        }
        if (v_sb.Length > 0) {
          if (v_rslt.DocumentElement != null) {
            v_rslt.DocumentElement.InnerXml = v_sb.ToString();
            v_rslt.DocumentElement.SetAttribute("count", "" + v_cnt);
          }
        }
      } finally {
        v_cur.Close();
      }
      return v_rslt;
    }

    /// <summary>
    /// Добавляем состояние к отчету
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="newState"></param>
    /// <param name="newStateDesc"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnAddQueueState(String rptUID, RemoteProcState newState, String newStateDesc, String userUID, String remoteIP) {
      const string sql = "begin xlr.set_rpt_state(:p_rpt_uid, :p_rpt_state, :p_state_desc, :p_usr_uid, :p_remote_ip); end;";
      var v_prms = new CParams();
      v_prms.Add("p_rpt_uid", rptUID);
      v_prms.Add("p_rpt_state", (int)newState);
      v_prms.Add("p_state_desc", newStateDesc);
      v_prms.Add("p_usr_uid", userUID);
      v_prms.Add("p_remote_ip", remoteIP);
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
    }

    /// <summary>
    /// Удаляем отчет из очереди
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnDropReportInst(String rptUID, String userUID, String remoteIP) {
      throw new NotImplementedException();
    }

    private CParams _getRptParams(String uid) {
      var v_rslt = new CParams();
      var v_cur = new SQLCmd(this.cfg.dbSession);
      var v_prms = new CParams(new CParam("rpt_uid", uid));
      v_cur.Init("select prm_name, prm_type, prm_val from rpt$rparams where rpt_uid = :rpt_uid", v_prms);
      v_cur.Open(120);
      try {
        while (v_cur.Next()) {
          var v_prmName = v_cur.DataReader.GetValue(0) as String;
          var v_prmType = v_cur.DataReader.GetValue(1) as String;
          var v_prmVal = v_cur.DataReader.GetValue(2) as String;
          var v_prm = new CParam();
          v_prm.Name = v_prmName;
          v_prm.Value = _paramDecode(v_prmType, v_prmVal);
          v_rslt.Add(v_prm);
        }
      } finally {
        v_cur.Close();
      }

      return v_rslt;
    }

    private Queue<CXLRptItem> _getReportsList(String viewName) {
      var v_rslt = new Queue<CXLRptItem>();
      var v_cur = new SQLCmd(this.cfg.dbSession);
      var v_sql = String.Format("SELECT a.rpt_uid, a.rpt_code, a.usr_uid FROM {0} a", viewName);
      v_cur.Init(v_sql, null);
      v_cur.Open(120);
      try {
        while (v_cur.Next()) {
          var v_uid = v_cur.DataReader.GetValue(0) as String;
          var v_code = v_cur.DataReader.GetValue(1) as String;
          var v_usr = v_cur.DataReader.GetValue(2) as String;

          v_rslt.Enqueue(new CXLRptItem {
            uid = v_uid,
            code = v_code,
            usr = v_usr,
            prms = this._getRptParams(v_uid)
          });
        }
      } finally {
        v_cur.Close();
      }

      return v_rslt;
    }

    /// <summary>
    /// Возвращаем список отчетов готовых к запуску
    /// </summary>
    /// <returns></returns>
    protected override Queue<CXLRptItem> doOnGetReportsReady() {
      return this._getReportsList("RPT$QUEUE_READY");
    }
    /// <summary>
    /// Возвращаем список запущеных отчетов
    /// </summary>
    /// <returns></returns>
    protected override Queue<CXLRptItem> doOnGetReportsRunning() {
      return this._getReportsList("RPT$QUEUE_RUNNING");
    }
    /// <summary>
    /// Возвращаем список отчетов для которых запрошен останов
    /// </summary>
    /// <returns></returns>
    protected override Queue<CXLRptItem> doOnGetReportsBreaking() {
      return this._getReportsList("RPT$QUEUE_BREAKING");
    }

    /// <summary>
    /// Возвращаем список отчетов для которых запрошен перезапуск
    /// </summary>
    /// <returns></returns>
    protected override Queue<CXLRptItem> doOnGetReportsRestarting() {
      return this._getReportsList("RPT$QUEUE_RESTARTING");
    }

    /// <summary>
    /// Возвращаем список ролей пользователя - строка перечень через ";"
    /// </summary>
    /// <param name="userUID"></param>
    /// <returns></returns>
    protected override String doOnGetUsrRoles(String userUID) {
      const string sql = "begin :rslt := xlr.usr_roles(:p_usr_uid); end;";
      var v_prms = new CParams();
      v_prms.Add("p_usr_uid", userUID);
      v_prms.Add(new CParam("rslt", null, typeof(String), 1000, ParamDirection.InputOutput));
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
      return v_prms.ValAsStrByName("rslt", true);
    }

    /// <summary>
    /// Проверка логина
    /// </summary>
    /// <param name="usr"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    protected override String doOnCheckUsrLogin(String usr, String pwd) {
      const string sql = "begin :rslt := xlr.check_usr_login(:p_usr, :p_pwd); end;";
      var v_prms = new CParams();
      v_prms.Add("p_usr", usr);
      v_prms.Add("p_pwd", pwd);
      v_prms.Add(new CParam("rslt", null, typeof(String), 1000, ParamDirection.InputOutput));
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
      return v_prms.ValAsStrByName("rslt", true);
    }

    /// <summary>
    /// Добавляем состояние к отчету
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="cmd"></param>
    protected override void doOnMarkRQCmdState(String rptUID, QueueCmd cmd) {
      //throw new NotImplementedException();
      const string sql = "begin xlr.mark_cmd_done(:p_rpt_uid, :p_cmd); end;";
      var v_prms = new CParams();
      v_prms.Add("p_rpt_uid", rptUID);
      v_prms.Add("p_cmd", (int)cmd);
      SQLCmd.ExecuteScript(this.cfg.dbSession, sql, v_prms, 120);
    }

  }
}
