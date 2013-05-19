namespace Bio.Framework.Server {
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Generic;
  using System.Text;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;

  public enum BioDbSessionContextParams {
    [DbValue("SYS_CURUSERUID")]
    UserUID,
    [DbValue("SYS_CURUSERIP")]
    UserIP,
    [DbValue("SYS_CURUSERROLES")]
    UserRoles,
    [DbValue("SYS_CURODEPUID")]
    OrgID,
    [DbValue("SYS_TITLE")]
    AppTitle
  }

  public class BioCfgOra: BioCfgBase, IBioCfg {
    public const Int32 DEFAULT_SQL_TIMEOUT = 60 * 3;
    public const String BIO_LOGIN_PKG = "BIO_LOGIN";

    public BioCfgOra(String localPath, String appURL)
      : base(localPath, appURL) {
      this.dbSession = new DBSession(this.ConnectionString, this.WorkspaceSchema);
      ((DBSession)this.dbSession).BeforeDBConnectEvent += this._doOnBeforeDBConnect;
      ((DBSession)this.dbSession).AfterDBConnectEvent += this._doOnAfterDBConnect;
    }

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
      var v_prms = new Params();
      v_prms.Add("pUser", user);
      v_prms.Add("pSessionID", sessionID);
      v_prms.Add("pSessionRemoteIP", sessionRemoteIP);
      v_prms.Add("pSessionRemoteHost", sessionRemoteHost);
      v_prms.Add("pSessionRemoteClient", sessionRemoteClient);
      v_prms.Add("pStatus", this.decConStat(status));
      var v_sql = String.Format("begin {0}.reg_connection(:pUser, :pSessionID," +
                              ":pSessionRemoteIP, :pSessionRemoteHost, :pSessionRemoteClient, :pStatus); end;", BIO_LOGIN_PKG);
      SQLCmd.ExecuteScript(this.dbSession, v_sql, v_prms, DEFAULT_SQL_TIMEOUT);
    }

    public void regUser(BioUser pUser) {
      throw new NotImplementedException();
    }

    private void _doOnBeforeDBConnect(Object sender, DBConnBeforeEventArgs args) { }

    private String _getCtxParamName(BioDbSessionContextParams prm) {
      return enumHelper.GetAttributeByValue<DbValueAttribute>(prm).Value;
    }

    private void _doOnAfterDBConnect(Object sender, DBConnAfterEventArgs args) {
      var v_sqlParams = new Params();
      var v_sql = String.Format("begin {0}.set_context_value(:par_name, :par_value); end;", BIO_LOGIN_PKG);
      if (this.CurUser != null) {
        v_sqlParams.SetValue("par_name", this._getCtxParamName(BioDbSessionContextParams.UserUID));
        v_sqlParams.SetValue("par_value", this.CurUser.UID);
        SQLCmd.ExecuteScript(args.Connection, v_sql, v_sqlParams, DEFAULT_SQL_TIMEOUT);
        v_sqlParams.SetValue("par_name", this._getCtxParamName(BioDbSessionContextParams.UserIP));
        v_sqlParams.SetValue("par_value", this.CurUser.AddressIP);
        SQLCmd.ExecuteScript(args.Connection, v_sql, v_sqlParams, DEFAULT_SQL_TIMEOUT);
        v_sqlParams.SetValue("par_name", this._getCtxParamName(BioDbSessionContextParams.UserRoles));
        v_sqlParams.SetValue("par_value", this.CurUser.Roles);
        SQLCmd.ExecuteScript(args.Connection, v_sql, v_sqlParams, DEFAULT_SQL_TIMEOUT);
        v_sqlParams.SetValue("par_name", this._getCtxParamName(BioDbSessionContextParams.OrgID));
        v_sqlParams.SetValue("par_value", this.CurUser.OrgID);
        SQLCmd.ExecuteScript(args.Connection, v_sql, v_sqlParams, DEFAULT_SQL_TIMEOUT);
      }
      v_sqlParams.SetValue("par_name", this._getCtxParamName(BioDbSessionContextParams.AppTitle));
      v_sqlParams.SetValue("par_value", this.BioSysTitle);
      SQLCmd.ExecuteScript(args.Connection, v_sql, v_sqlParams, DEFAULT_SQL_TIMEOUT);
    }

    public override void Login(String login) {
      this.CurUser = null;

      var vSQL = String.Format("begin {0}.check_login(:login, :usr_uid); end;", BIO_LOGIN_PKG);

      var v_prms = new Params();
      v_prms.Add("login", login);
      v_prms.Add(new Param("usr_uid", (String)null, typeof(String), ParamDirection.Output));
      SQLCmd.ExecuteScript(this.dbSession, vSQL, v_prms, DEFAULT_SQL_TIMEOUT);
      var v_usrUID = Params.FindParamValue(v_prms, "usr_uid") as String;

      vSQL = String.Format("select * from table({0}.get_usr(:usr_uid))", BIO_LOGIN_PKG);
      var v_cur = SQLCursor.CreateAndOpenCursor(this.dbSession, vSQL, new Params(new Param("usr_uid", v_usrUID)), DEFAULT_SQL_TIMEOUT);
      try {
        if (v_cur.IsActive && v_cur.Next()) {
          this.CurUser = new BioUser();

          enumHelper.ForEachPropertyInfo(this.CurUser.GetType(), p => {
            var fld = enumHelper.GetAttributeByInfo<DbFieldAttribute>(p);
            if(fld != null)
              p.SetValue(this.CurUser, v_cur.GetOraValue(fld.Name), null);
          });

        } else {
          throw new EBioException { ErrorCode = 20401 };
        }
      } finally {
        v_cur.Close();
      }
    }

  }
}
