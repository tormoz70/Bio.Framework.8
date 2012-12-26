using System;
using System.Collections.Generic;
using System.Linq;
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

    private String _paramTypeEncode(Type paramType) {
      String v_prm_type_str = "A";
      if (paramType != null) {
        if (Utl.typeIsNumeric(paramType))
          v_prm_type_str = "N";
        else if (paramType.Equals(typeof(DateTime)))
          v_prm_type_str = "D";
      }
      return v_prm_type_str;
    }

    private Object _paramDecode(String paramType, String paramValue) {
      Object v_prm_value = paramValue;
      if (paramType != null) {
        if (paramType.Equals("N"))
          v_prm_value = Utl.Convert2Type<Decimal>(paramValue);
        else if (paramType.Equals("D"))
          v_prm_value = Utl.Convert2Type<DateTime>(paramValue);
      }
      return v_prm_value;
    }

    private void addRptParams(IDbConnection conn, String rptUID, CParams prms, String userUID, String remoteIP) {
      CParams vPrms = new CParams();
      vPrms.SetValue("p_rpt_uid", rptUID);
      String vSQL = "begin xlr.clear_rparams(:p_rpt_uid); end;";
      CSQLCmd.ExecuteScript(conn, vSQL, vPrms, 120);
      vSQL = "begin xlr.add_rparam(:p_rpt_uid, :p_prm_name, :p_prm_type, :p_prm_val, :p_usr_uid, :p_remote_ip); end;";
      foreach (var prm in prms) {
        vPrms.SetValue("p_prm_name", prm.Name);
        String v_prm_value = null;
        String v_prm_type_str = "A";
        if (prm.Value != null) {
          Type v_prm_type = prm.ParamType ?? prm.Value.GetType();
          if (Utl.typeIsNumeric(v_prm_type)) {
            v_prm_type_str = "N";
            v_prm_value = "" + prm.Value;
            v_prm_value.Replace(",", ".");
          } else if (v_prm_type.Equals(typeof(DateTime))) {
            v_prm_type_str = "D";
            v_prm_value = ((DateTime)prm.Value).ToString("yyyy.MM.dd HH:mm:ss");
          } else
            v_prm_value = "" + prm.Value;
        } else
          continue;
        vPrms.SetValue("p_prm_type", v_prm_type_str);
        vPrms.SetValue("p_prm_val", v_prm_value);
        vPrms.SetValue("p_usr_uid", userUID);
        vPrms.SetValue("p_remote_ip", remoteIP);
        CSQLCmd.ExecuteScript(conn, vSQL, vPrms, 120);
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
      String v_rptUID = null;
      IDbConnection conn = this._cfg.dbSession.GetConnection();
      try {
        //throw new NotImplementedException();
        String vSQL = "begin xlr.add_rpt(:p_rpt_uid, :p_rpt_code, :p_rpt_prms, :p_rpt_desc, :p_usr_uid, :p_remote_ip); end;";
        CParams vPrms = new CParams();
        vPrms.Add(new CParam("p_rpt_uid", null, typeof(String), ParamDirection.InputOutput));
        vPrms.Add("p_rpt_code", rptCode);
        vPrms.Add("p_rpt_desc", null);
        vPrms.Add("p_usr_uid", userUID);
        vPrms.Add("p_remote_ip", remoteIP);
        CSQLCmd.ExecuteScript(conn, vSQL, vPrms, 120);
        v_rptUID = vPrms.ValAsStrByName("p_rpt_uid", true);
        this.addRptParams(conn, v_rptUID, prms, userUID, remoteIP);
      } finally {
        conn.Close();
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
      //throw new NotImplementedException();
      String vSQL = "SELECT a.rpt_result_fn, a.rpt_result_len, a.rpt_result" +
                    " FROM rpt$queue_rslts a"+
                    " WHERE a.rpt_uid = :rpt_uid";
      CParams vPrms = new CParams();
      vPrms.Add("rpt_uid", rptUID);
      CSQLCmd vCur = new CSQLCmd(this._cfg.dbSession);
      vCur.Init(vSQL, vPrms);
      vCur.Open(120);
      try {
        if (vCur.Next()) {
          fileName = (String)vCur.DataReader.GetValue(0);
          Decimal vSzAct = vCur.DataReader.GetDecimal(1);
          int vSzActInt = (int)vSzAct;
          buff = new byte[vSzActInt];
          long vSz = vCur.DataReader.GetBytes(2, 0, buff, 0, vSzActInt);
        }
      } finally {
        vCur.Close();
      }
    }

    /// <summary>
    /// Тут засовываем в очередь файл готового отчета
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="fileName"></param>
    protected override void doOnAddReportResult2DB(String rptUID, String fileName) {
      String vSQL = "begin xlr.save_rpt_file(:p_rpt_uid, :p_file_name, :p_file); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_rpt_uid", rptUID);
      vPrms.Add("p_file_name", Path.GetFileName(fileName));
      byte[] vBuffer = null;
      Utl.ReadBinFileInBuffer(ref vBuffer, fileName);
      vPrms.Add("p_file", vBuffer);
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
    }

    /// <summary>
    /// Останавливаем выполняющийся отчет
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnBreakReportInst(String rptUID, String userUID, String remoteIP) {
      String vSQL = "begin xlr.break_rpt(:p_rpt_uid, :p_usr_uid, :p_remote_ip); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_rpt_uid", rptUID);
      vPrms.Add("p_usr_uid", userUID);
      vPrms.Add("p_remote_ip", remoteIP);
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
    }

    /// <summary>
    /// Перезапуск отчета
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnRestartReportInst(String rptUID, String userUID, String remoteIP) {
      String vSQL = "begin xlr.restart_rpt(:p_rpt_uid, :p_usr_uid, :p_remote_ip); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_rpt_uid", rptUID);
      vPrms.Add("p_usr_uid", userUID);
      vPrms.Add("p_remote_ip", remoteIP);
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
    }

    /// <summary>
    /// Формируем список отчетов в очереди
    /// </summary>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    /// <returns></returns>
    protected override XmlDocument doOnGetQueue(String userUID, String remoteIP) {
      XmlDocument rslt = dom4cs.NewDocument("queue").XmlDoc;
      rslt.DocumentElement.SetAttribute("usr", userUID);
      rslt.DocumentElement.SetAttribute("remote_ip", remoteIP);
      rslt.DocumentElement.SetAttribute("count", "0");
      //throw new NotImplementedException();
      String vSQL = "select * from table(xlr.rqueue_xml(null,:p_usr_uid))";
      CParams vPrms = new CParams();
      vPrms.Add("p_usr_uid", userUID);
      CSQLCmd vCur = new CSQLCmd(this._cfg.dbSession);
      vCur.Init(vSQL, vPrms);
      vCur.Open(120);
      StringBuilder sb = new StringBuilder();
      try{
        int cnt = 0;
        while (vCur.Next()) {
          sb.AppendLine(vCur.DataReader.GetString(0));
          cnt++;
        }
        if (sb.Length > 0) {
          rslt.DocumentElement.InnerXml = sb.ToString();
          rslt.DocumentElement.SetAttribute("count", "" + cnt);
        }
      } finally {
        vCur.Close();
      }
      return rslt;
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
      //throw new NotImplementedException();
      String vSQL = "begin xlr.set_rpt_state(:p_rpt_uid, :p_rpt_state, :p_state_desc, :p_usr_uid, :p_remote_ip); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_rpt_uid", rptUID);
      vPrms.Add("p_rpt_state", (int)newState);
      vPrms.Add("p_state_desc", newStateDesc);
      vPrms.Add("p_usr_uid", userUID);
      vPrms.Add("p_remote_ip", remoteIP);
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
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
      CParams rslt = new CParams();
      CSQLCmd vCur = new CSQLCmd(this._cfg.dbSession);
      CParams qPrms = new CParams(new CParam("rpt_uid", uid));
      vCur.Init("select prm_name, prm_type, prm_val from rpt$rparams where rpt_uid = :rpt_uid", qPrms);
      vCur.Open(120);
      try {
        while (vCur.Next()) {
          var v_prm_name = vCur.DataReader.GetValue(0) as String;
          var v_prm_type = vCur.DataReader.GetValue(1) as String;
          var v_prm_val = vCur.DataReader.GetValue(2) as String;
          CParam prm = new CParam();
          prm.Name = v_prm_name;
          prm.Value = this._paramDecode(v_prm_type, v_prm_val);
          rslt.Add(prm);
        }
      } finally {
        vCur.Close();
      }

      return rslt;
    }

    private Queue<CXLRptItem> _getReportsList(String viewName) {
      Queue<CXLRptItem> rslt = new Queue<CXLRptItem>();
      CSQLCmd vCur = new CSQLCmd(this._cfg.dbSession);
      String v_sql = String.Format("SELECT a.rpt_uid, a.rpt_code, a.usr_uid FROM {0} a", viewName);
      vCur.Init(v_sql, null);
      vCur.Open(120);
      try {
        while (vCur.Next()) {
          var v_uid = vCur.DataReader.GetValue(0) as String;
          var v_code = vCur.DataReader.GetValue(1) as String;
          var v_usr = vCur.DataReader.GetValue(2) as String;

          rslt.Enqueue(new CXLRptItem() {
            uid = v_uid,
            code = v_code,
            usr = v_usr,
            prms = this._getRptParams(v_uid)
          });
        }
      } finally {
        vCur.Close();
      }

      return rslt;
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
      String vSQL = "begin :rslt := xlr.usr_roles(:p_usr_uid); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_usr_uid", userUID);
      vPrms.Add(new CParam("rslt", null, typeof(String), 1000, ParamDirection.InputOutput));
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
      return vPrms.ValAsStrByName("rslt", true);
    }

    /// <summary>
    /// Проверка логина
    /// </summary>
    /// <param name="usr"></param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    protected override String doOnCheckUsrLogin(String usr, String pwd) {
      String vSQL = "begin :rslt := xlr.check_usr_login(:p_usr, :p_pwd); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_usr", usr);
      vPrms.Add("p_pwd", pwd);
      vPrms.Add(new CParam("rslt", null, typeof(String), 1000, ParamDirection.InputOutput));
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
      return vPrms.ValAsStrByName("rslt", true);
    }

    /// <summary>
    /// Добавляем состояние к отчету
    /// </summary>
    /// <param name="rptUID"></param>
    /// <param name="newState"></param>
    /// <param name="newStateDesc"></param>
    /// <param name="userUID"></param>
    /// <param name="remoteIP"></param>
    protected override void doOnMarkRQCmdState(String rptUID, QueueCmd cmd) {
      //throw new NotImplementedException();
      String vSQL = "begin xlr.mark_cmd_done(:p_rpt_uid, :p_cmd); end;";
      CParams vPrms = new CParams();
      vPrms.Add("p_rpt_uid", rptUID);
      vPrms.Add("p_cmd", (int)cmd);
      CSQLCmd.ExecuteScript(this._cfg.dbSession, vSQL, vPrms, 120);
    }

  }
}
