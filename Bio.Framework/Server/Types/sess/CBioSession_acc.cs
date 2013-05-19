namespace Bio.Framework.Server {
  using System;
  using System.Xml;
  using System.Web;
  using System.Web.SessionState;
  using System.IO;
  using System.Threading;
  using System.Text;

  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common;


  public enum TBioLoginState { ssNew, ssLogginIn, ssLoggedOn, ssLoggedOff, ssLoginCanceled, ssLoginError };
  public partial class BioSession :DisposableObject {

    public IBioCfg Cfg { get; private set; }

    private TBioLoginState FLoginState = TBioLoginState.ssNew;
    public TBioLoginState LoginState {
      get {
        return this.FLoginState;
      }
    }

    private String FRqstURL = null;

    public void setLoginState(TBioLoginState pState) {
      this.FLoginState = pState;
    }

    private void regConn(String pUser, TRemoteConnectionStatus pState) {
      this.Cfg.regConnect(pUser,
                          this.CurSessionID,
                          this.CurSessionRemoteIP,
                          this.CurSessionRemoteHostName,
                          this.CurSessionRemoteAgent,
                          pState);
    }

    private void chekLogin(String login) {
      try {
        this.Cfg.Login(login);
        this.Cfg.CurUser.AddressIP = this.CurSessionRemoteIP;
      } catch (EBioException ex) {
        switch (ex.ErrorCode) {
          case 20401: throw new EBioBadUser();
          case 20402: throw new EBioBlockedUser();
          case 20403: throw new EBioUncnfrmdUser();
          case 20404: throw new EBioBadUser();
          default: throw ex;
        }
      }
    }

    private void _initHttpContext() {
      if (this.FLoginState == TBioLoginState.ssLoggedOn) { 
        Params v_prms = null;
        var brq = this.CurBioHandler.bioRequest<CBioRequest>();
        if (brq != null)
          v_prms = brq.bioParams;
        if (v_prms != null) {
          v_prms.SetValue(enumHelper.GetAttributeByValue<DbValueAttribute>(BioDbSessionContextParams.UserUID).Value, this.Cfg.CurUser.UID);
          v_prms.SetValue(enumHelper.GetAttributeByValue<DbValueAttribute>(BioDbSessionContextParams.UserIP).Value, this.Cfg.CurUser.AddressIP);
          v_prms.SetValue(enumHelper.GetAttributeByValue<DbValueAttribute>(BioDbSessionContextParams.UserRoles).Value, this.Cfg.CurUser.Roles);
          v_prms.SetValue(enumHelper.GetAttributeByValue<DbValueAttribute>(BioDbSessionContextParams.OrgID).Value, this.Cfg.CurUser.OrgID);
          v_prms.SetValue(enumHelper.GetAttributeByValue<DbValueAttribute>(BioDbSessionContextParams.AppTitle).Value, this.Cfg.BioSysTitle);
        }
      }
    }

    public void login(Boolean pSkipLogin) {
      if(!pSkipLogin) {

        if(this.FLoginState != TBioLoginState.ssLoggedOn) {
          var vQLoginPrm = this.CurBioHandler.QParams.ParamByName(Utl.QLOGIN_PARNAME, true);
          if (vQLoginPrm != null) {
            this.chekLogin(vQLoginPrm.ValueAsString());
            this.FLoginState = TBioLoginState.ssLoggedOn;
            this.regConn(this.Cfg.CurUser.Login, TRemoteConnectionStatus.rcsOk);
          } else {
            var vHLoginPrm = this.CurBioHandler.QParams.ParamByName(Utl.HASHLOGIN_PARNAME, true);
            if (vHLoginPrm != null) {
              this.chekLogin(vHLoginPrm.ValueAsString());
              this.FLoginState = TBioLoginState.ssLoggedOn;
              this.regConn(this.Cfg.CurUser.Login, TRemoteConnectionStatus.rcsOk);
            }
          }
        }
        if(this.FLoginState != TBioLoginState.ssLoggedOn) {
          try {
            //this.Cfg.CurUser = null;
            if(this.CurBioHandler.GetType() == typeof(tm_login_post))
              this.FLoginState = TBioLoginState.ssLogginIn;
            if(this.FLoginState != TBioLoginState.ssLogginIn) {
              this.regConn("mrnemo", TRemoteConnectionStatus.rcsTry);
              throw new EBioStart();
            }
            //Param vCurLoginPrm = this.CurBioMsg.QParams.ParamByName(Bio.Common.Utl.FLOGIN_PARNAME);
            //String vCurLogin = (vCurLoginPrm != null) ? vCurLoginPrm.ValueAsString() : null;
            CBioLoginRequest rq = this.CurBioHandler.bioRequest<CBioLoginRequest>();
            String vCurLogin = (rq != null) ? rq.login : null;
            if (vCurLogin == null) {
              this.regConn("mrnemo", TRemoteConnectionStatus.rcsTry);
              throw new EBioStart();
            }
            this.chekLogin(vCurLogin);

            if(this.FLoginState == TBioLoginState.ssLogginIn) {
              this.regConn(this.Cfg.CurUser.Login, TRemoteConnectionStatus.rcsOk);
              throw new EBioOk(this.Cfg.CurUser);
            }

          } catch(EBioError ex) {
            this.FLoginState = TBioLoginState.ssLoginError;
            throw ex;
          } catch(EBioStart ex) {
            this.FLoginState = TBioLoginState.ssLogginIn;
            this.FRqstURL = this.CurBioHandler.Context.Request.Path + "?" + this.CurBioHandler.Context.Request.QueryString;
            throw ex;
          } catch(EBioOk ex) {
            this.FLoginState = TBioLoginState.ssLoggedOn;
            throw ex;
          }
        } else if (this.FLoginState == TBioLoginState.ssLoggedOn) {
          if (this.CurBioHandler is tm_ping)
            throw new EBioOk(this.Cfg.CurUser);
        }
      }
      this._initHttpContext();

    }

    protected override void doOnDispose() {
      base.doOnDispose();
      this.Cfg.dbSession.KillTransactions();
    }
  }
}
