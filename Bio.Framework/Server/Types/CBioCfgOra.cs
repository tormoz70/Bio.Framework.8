namespace Bio.Framework.Server {
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Generic;
  using System.Text;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;
  
  public class CBioCfgOra: CBioCfgBase, IBioCfg {
    public static Int32 ciDefaultSQLTimeout = 60 * 3;
    public static String csBIO_LOGIN_PKG = "BIO_LOGIN";
    public static String csSYS_CurUserUID_PARAM_NAME = "SYS_CURUSERUID";
    public static String csSYS_CurUserIP_PARAM_NAME = "SYS_CURUSERIP";
    public static String csSYS_CurUserRoles_PARAM_NAME = "SYS_CURUSERROLES";
    public static String csSYS_CurODepUID_PARAM_NAME = "SYS_CURODEPUID";
    public static String csSYS_TITLE_PARAM_NAME = "SYS_TITLE";

    public CBioCfgOra(String localPath, String appURL)
      : base(localPath, appURL) {
      //CDBFactory.OnAfterDBConnect += this.doOnAfterDBConnect;
      this.dbSession = new DBSession(this.ConnectionString);
      ((DBSession)this.dbSession).BeforeDBConnectEvent += this.doOnBeforeDBConnect;
      ((DBSession)this.dbSession).AfterDBConnectEvent += this.doOnAfterDBConnect;
    }

    #region IBioCfg Members

    public IDBSession dbSession { get; private set; }

    private String decConStat(TRemoteConnectionStatus status) {
      return enumHelper.GetFieldDesc(status);
    }

    public void regConnect(String user,
                           String sessionID,
                           String sessionRemoteIP,
                           String sessionRemoteHost,
                           String sessionRemoteClient,
                           TRemoteConnectionStatus status) {
      CParams vPrms = new CParams();
      vPrms.Add("pUser", user);
      vPrms.Add("pSessionID", sessionID);
      vPrms.Add("pSessionRemoteIP", sessionRemoteIP);
      vPrms.Add("pSessionRemoteHost", sessionRemoteHost);
      vPrms.Add("pSessionRemoteClient", sessionRemoteClient);
      vPrms.Add("pStatus", this.decConStat(status));
      String vSQL = String.Format("begin {0}.reg_connection(:pUser, :pSessionID," +
                              ":pSessionRemoteIP, :pSessionRemoteHost, :pSessionRemoteClient, :pStatus); end;", csBIO_LOGIN_PKG);
      SQLCmd.ExecuteScript(this.dbSession, vSQL, vPrms, ciDefaultSQLTimeout);
    }

    public void regUser(CBioUser pUser) {
      throw new NotImplementedException();
    }

    private void doOnBeforeDBConnect(Object sender, DBConnBeforeEventArgs args) { }

    private void doOnAfterDBConnect(Object sender, DBConnAfterEventArgs args) {
      CParams vSQLParams = new CParams();
      String vSQL = String.Format("begin {0}.set_context_value(:par_name, :par_value); end;", csBIO_LOGIN_PKG);
      if (this.CurUser != null) {
        vSQLParams.SetValue("par_name", csSYS_CurUserUID_PARAM_NAME);
        vSQLParams.SetValue("par_value", this.CurUser.USR_UID);
        SQLCmd.ExecuteScript(args.Connection, vSQL, vSQLParams, ciDefaultSQLTimeout);
        vSQLParams.SetValue("par_name", csSYS_CurUserIP_PARAM_NAME);
        vSQLParams.SetValue("par_value", this.CurUser.USR_IP);
        SQLCmd.ExecuteScript(args.Connection, vSQL, vSQLParams, ciDefaultSQLTimeout);
        vSQLParams.SetValue("par_name", csSYS_CurUserRoles_PARAM_NAME);
        vSQLParams.SetValue("par_value", this.CurUser.Role);
        SQLCmd.ExecuteScript(args.Connection, vSQL, vSQLParams, ciDefaultSQLTimeout);
        vSQLParams.SetValue("par_name", csSYS_CurODepUID_PARAM_NAME);
        vSQLParams.SetValue("par_value", this.CurUser.ODEP_UID);
        SQLCmd.ExecuteScript(args.Connection, vSQL, vSQLParams, ciDefaultSQLTimeout);
      }
      vSQLParams.SetValue("par_name", csSYS_TITLE_PARAM_NAME);
      vSQLParams.SetValue("par_value", this.BioSysTitle);
      SQLCmd.ExecuteScript(args.Connection, vSQL, vSQLParams, ciDefaultSQLTimeout);
    }

    public override void Login(String login) {
      this.CurUser = null;

      var vSQL = String.Format("begin {0}.check_login(:login, :usr_uid, :role_uid); end;", csBIO_LOGIN_PKG);

      var v_prms = new CParams();
      v_prms.Add("login", login);
      v_prms.Add(new CParam("usr_uid", (String)null, typeof(String), ParamDirection.Output));
      v_prms.Add(new CParam("role_uid", (String)null, typeof(String), ParamDirection.Output));
      SQLCmd.ExecuteScript(this.dbSession, vSQL, v_prms, ciDefaultSQLTimeout);
      String v_usr_uid = CParams.FindParamValue(v_prms, "usr_uid") as String;
      String v_role_uid = CParams.FindParamValue(v_prms, "role_uid") as String;

      vSQL = String.Format("select * from table({0}.get_usr(:usr_uid))", csBIO_LOGIN_PKG);
      SQLCursor vCur = SQLCursor.creAndOpenCursor(this.dbSession, vSQL, new CParams(new CParam("usr_uid", v_usr_uid)), ciDefaultSQLTimeout);
      try {
        if (vCur.IsActive && vCur.Next()) {
          this.CurUser = new CBioUser();

          this.CurUser.USR_UID = vCur.getOraValueAsString("usr_uid");
          this.CurUser.ODEP_UID = vCur.getOraValueAsString("odep_uid");
          this.CurUser.ODEP_NAME = vCur.getOraValueAsString("odep_name");
          this.CurUser.ODEP_DESC = vCur.getOraValueAsString("odep_desc");
          this.CurUser.ODepPath = vCur.getOraValueAsString("odep_path");
          this.CurUser.ODepUidPathStr = vCur.getOraValueAsString("odep_uid_path");
          this.CurUser.parsODepUidPath(this.CurUser.ODepUidPathStr);
          this.CurUser.Role = vCur.getOraValueAsString("usr_roles");
          this.CurUser.parsGrants(vCur.getOraValueAsString("usr_grants"));
          this.CurUser.USR_NAME = vCur.getOraValueAsString("usr_name");
          this.CurUser.USR_FAM = vCur.getOraValueAsString("usr_fam");
          this.CurUser.USR_FNAME = vCur.getOraValueAsString("usr_fname");
          this.CurUser.USR_SNAME = vCur.getOraValueAsString("usr_sname");
          this.CurUser.REG = vCur.getOraValueAsDateTime("reg_date");
          this.CurUser.EMAIL = vCur.getOraValueAsString("email_addr");
          this.CurUser.PHONE = vCur.getOraValueAsString("usr_phone");
        } else {
          throw new EBioException { ErrorCode = 20401 };
        }
      } finally {
        vCur.Close();
      }
    }

    #endregion
  }
}
