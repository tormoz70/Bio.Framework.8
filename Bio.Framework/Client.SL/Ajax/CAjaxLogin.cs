namespace Bio.Framework.Client.SL {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Net;
  using System.IO;
  using System.Threading;
  using Bio.Helpers.Common;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using Bio.Helpers.Ajax;
  using Newtonsoft.Json;
  using System.Windows.Threading;
  using System.Windows;
  using Bio.Helpers.Controls.SL;
  
  /// <summary>
  /// Обработка запросов аутентикации
  /// </summary>
  public delegate CAjaxResponse DlgDoContinueRequest(CBioRequest pRequest);
  public delegate void DlgDoOnRequestLogin(Object pSender, ref String vLogin);
  public delegate void DlgDoOnLoginChecked(Object pSender, String pLogin, bool pLoginIsOk);
  internal class CAjaxLogin {

    private CAjaxMng FOwner = null;
    private DOnLogLine FDOnLogLine = null;
    private String FCurVerInfo = null;
    private String FCurSrvInfo = null;
    private String FUserAgentName = null;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="owner">Ссылка на CAjaxMng</param>
    /// <param name="doOnLogLine">Процедура, которая получает отладочную инфу</param>
    public CAjaxLogin(CAjaxMng owner, DOnLogLine doOnLogLine) {
      this.FOwner = owner;
      this.FDOnLogLine = doOnLogLine;
    }

    private void postLogin(String pLogin, Action<CBioResponse> callback) {
      try {
        String[] vLgnArr = Utl.SplitString(pLogin, '/');
        if (vLgnArr.Length != 2)
          throw new EBioBadLogin("Не верный формат данных аутентификации!");
        if (String.IsNullOrEmpty(vLgnArr[0]))
          throw new EBioBadUser("Имя пользователя не может быть пустым!");
        if (String.IsNullOrEmpty(vLgnArr[1]))
          throw new EBioBadPwd("Пароль не может быть пустым!");
      } catch (EBioException e) {
        CBioResponse vRslt = new CBioResponse { 
          ex = e,
          success = false
        };
        if (callback != null) callback(vRslt);
        return;
      }

      CBioLoginRequest rq = new CBioLoginRequest() {
        url = this.FOwner.Env.ServerUrl,
        requestType = RequestType.doPostLoginForm,
        login = pLogin,
        timeout = this.FOwner.Env.ConfigRoot.RequestTimeout, //this.FOwner.AjaxCli.RequestTimeout,
        callback = (s, a) => {
          if (callback != null)
            callback(a.response as CBioResponse);
        }
      };

      ajaxUTL.getDataFromSrv(rq);
    }

    private void showLoginError(String pMsg, Action callback) {
      //msgBx.showError(pMsg, "Идентификация пользователя", callback);
      MessageBox.Show(pMsg, "Идентификация пользователя", MessageBoxButton.OK);
      if (callback != null)
        callback();
    }

    /// <summary>
    /// Восстанавливает ошибку типа ebio.login.EBioLogin из Json-строки
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns></returns>
    public static EBioLogin decBioLoginExcp(Exception ex) {
      return (ex != null) && (ex is EBioLogin) ? (EBioLogin)ex : null;
    }

    private void _getLogin(Action<String> callback) {
      if (this.FOwner.ConnectionState != ConnectionState.Canceled) {
        FrmLoginBase vFrm = new FrmLoginBase();
        //Deployment.Current.Dispatcher.BeginInvoke(new Action(() => {
        //  vFrm = new FrmLoginBase();
        //}));
        
        String vLastUsrName = (this.FOwner.Env != null) ? this.FOwner.Env.ConfigRoot.LastLoggedInUserName : null;
        vFrm.SetUsrName(vLastUsrName);
        Boolean vSaveUsrPwd = (this.FOwner.Env != null) ? this.FOwner.Env.ConfigRoot.SavePassword : false;
        if (vSaveUsrPwd) {
          String vLastUsrPwd = (this.FOwner.Env != null) ? this.FOwner.Env.ConfigRoot.LastLoggedInUserPwd : null;
          vFrm.SetUsrPwd(vLastUsrPwd);
        }
        vFrm.SetVerInfo((this.FOwner.Env != null) ? this.FOwner.Env.UserAgentTitleAndVer : this.FOwner.UserAgentTitleAndVer);
        try {
          vFrm.ShowM(callback);
        } catch (ObjectDisposedException) {
          if (callback != null) callback(null);
        }
      } else {
        if (callback != null) callback(null);
      }
    }

    private void _processLogin(EBioLogin bioLoginExcp, Action<EBioException> callback) {
      if(bioLoginExcp is EBioLoggedOut) {
        this.CurUsr = null;
        if (callback != null) callback(bioLoginExcp);
      } else {
        // Запрашиваем ввод логина
        this._getLogin((newLogin) => {
          if (newLogin == null) {
            // Если пользователь не ввел логин, тогда отбой
            if (callback != null) callback(new EBioCancel());
          } else {
            // Пользователь ввел логин
            String vCurUsrName = Utl.extractUsrNameFromLogin(newLogin);
            String vCurUsrPwd = Utl.extractUsrPwdFromLogin(newLogin);
            // Запускаем процедуру проверки логина 
            postLogin(newLogin, (r) => {
              EBioException vExcp = decBioLoginExcp(r.ex);
              if (vExcp != null) {
                if (vExcp is EBioOk) {
                  this.assignCurUser(((EBioOk)vExcp).Usr);
                  this.FOwner.Env.ConfigRoot.LastLoggedInUserName = vCurUsrName;
                  this.FOwner.Env.LastSuccessPwd = vCurUsrPwd;
                  if (this.FOwner.Env.ConfigRoot.SavePassword)
                    this.FOwner.Env.ConfigRoot.LastLoggedInUserPwd = vCurUsrPwd;
                  else
                    this.FOwner.Env.ConfigRoot.LastLoggedInUserPwd = String.Empty;
                  (this.FOwner.Env.PluginRoot as IConfigurable).SaveCfg();
                  if (callback != null) callback(vExcp);
                } else if (vExcp is EBioAutenticationError) {
                  String v_errMsg = null;
                  if (vExcp is EBioBadUser)
                    v_errMsg = "Не верное имя пользователя [" + vCurUsrName + "] или пароль.";
                  else if (vExcp is EBioUncnfrmdUser)
                    v_errMsg = "Пользователь [" + vCurUsrName + "] не активирован.";
                  else if (vExcp is EBioBlockedUser)
                    v_errMsg = "Пользователь [" + vCurUsrName + "] заблокирован.";
                  else if (vExcp is EBioBadPwd)
                    v_errMsg = "Не верный пароль.";
                  this.showLoginError(v_errMsg, () => {
                    this._processLogin(bioLoginExcp, callback);
                  });
                } else {
                  vExcp = new EBioException("Непредвиденная ошибка: " + r.ex.Message);
                  if (callback != null) callback(vExcp);
                }
              } else {
                vExcp = EBioException.CreateIfNotEBio(r.ex);
                if (callback != null) callback(vExcp);
              }
                
            });
          }
        });
      }
    }

    public void processLogin(EBioLogin bioLoginExcp, CBioRequest request, Action<EBioException, CBioRequest> callback) {
      if (bioLoginExcp != null) {
        this._processLogin(bioLoginExcp, (EBioException e) => {
          if (callback != null) callback(e, request);
        });
      }
    }

    /// <summary>
    /// HostName сервера с которым установлено соединение
    /// </summary>
    public String CurSrvInfo {
      get {
        return this.FCurSrvInfo;
      }
    }

    /// <summary>
    /// Последний пользователь успешно прошедший идентификацию на сервере
    /// </summary>
    public CBioUser CurUsr { get; private set; }

    public void assignCurUser(CBioUser usr) {
      this.CurUsr = (CBioUser)usr.Clone();
    }
  }
}
