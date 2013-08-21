namespace Bio.Framework.Client.SL {
  using System;
  using Helpers.Common;
  using Helpers.Common.Types;
  using Packets;
  using System.Windows;

  /// <summary>
  /// Обработка запросов аутентикации
  /// </summary>
  public delegate AjaxResponse DlgDoContinueRequest(BioRequest pRequest);
  public delegate void DlgDoOnRequestLogin(Object pSender, ref String vLogin);
  public delegate void DlgDoOnLoginChecked(Object pSender, String pLogin, bool pLoginIsOk);
  internal class CAjaxLogin {

    private readonly AjaxMng _owner;
    private readonly String _curSrvInfo = null;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="owner">Ссылка на AjaxMng</param>
    public CAjaxLogin(AjaxMng owner) {
      this._owner = owner;
    }

    private void _postLogin(String pLogin, Action<BioResponse> callback) {
      try {
        var vLgnArr = Utl.SplitString(pLogin, '/');
        if (vLgnArr.Length != 2)
          throw new EBioBadLogin("Не верный формат данных аутентификации!");
        if (String.IsNullOrEmpty(vLgnArr[0]))
          throw new EBioBadUser("Имя пользователя не может быть пустым!");
        if (String.IsNullOrEmpty(vLgnArr[1]))
          throw new EBioBadPwd("Пароль не может быть пустым!");
      } catch (EBioException e) {
        var rslt = new BioResponse { 
          Ex = e,
          Success = false
        };
        if (callback != null) callback(rslt);
        return;
      }

      var rq = new BioLoginRequest {
        URL = BioEnvironment.Instance.ServerUrl,
        RequestType = RequestType.doPostLoginForm,
        login = pLogin,
        Timeout = BioEnvironment.Instance.ConfigRoot.RequestTimeout, //this._owner.AjaxCli.RequestTimeout,
        Callback = (s, a) => {
          if (callback != null)
            callback(a.Response as BioResponse);
        }
      };

      ajaxUTL.GetDataFromSrv(rq);
    }

    private static void _showLoginError(String pMsg, Action callback) {
      MessageBox.Show(pMsg, "Идентификация пользователя", MessageBoxButton.OK);
      if (callback != null)
        callback();
    }

    /// <summary>
    /// Восстанавливает ошибку типа ebio.login.EBioLogin из Json-строки
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns></returns>
    public static EBioLogin DecBioLoginExcp(Exception ex) {
      return (ex is EBioLogin) ? (EBioLogin)ex : null;
    }

    private void _getLogin(Action<String> callback) {
      if (this._owner.ConnectionState != ConnectionState.Canceled) {
        var frm = new FrmLoginBase();

        var lastUsrName = BioEnvironment.Instance.ConfigRoot.LastLoggedInUserName;
        frm.SetUsrName(lastUsrName);
        var saveUsrPwd = BioEnvironment.Instance.ConfigRoot.SavePassword;
        if (saveUsrPwd) {
          var lastUsrPwd = BioEnvironment.Instance.ConfigRoot.LastLoggedInUserPwd;
          frm.SetUsrPwd(lastUsrPwd);
        }
        frm.SetVerInfo(BioEnvironment.Instance.UserAgentTitleAndVer);
        try {
          frm.ShowM(callback);
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
        this._getLogin(newLogin => {
          if (newLogin == null) {
            // Если пользователь не ввел логин, тогда отбой
            if (callback != null) callback(new EBioCancel());
          } else {
            // Пользователь ввел логин
            var curUsrName = Utl.ExtractUsrNameFromLogin(newLogin);
            var curUsrPwd = Utl.ExtractUsrPwdFromLogin(newLogin);
            // Запускаем процедуру проверки логина 
            _postLogin(newLogin, r => {
              EBioException excp = DecBioLoginExcp(r.Ex);
              if (excp != null) {
                var bioOk = excp as EBioOk;
                if (bioOk != null) {
                  this.AssignCurUser(bioOk.Usr);
                  BioEnvironment.Instance.ConfigRoot.LastLoggedInUserName = curUsrName;
                  BioEnvironment.Instance.LastSuccessPwd = curUsrPwd;
                  if (BioEnvironment.Instance.ConfigRoot.SavePassword)
                    BioEnvironment.Instance.ConfigRoot.LastLoggedInUserPwd = curUsrPwd;
                  else
                    BioEnvironment.Instance.ConfigRoot.LastLoggedInUserPwd = String.Empty;
                  BioEnvironment.Instance.PluginRoot.Cfg.Store();
                  if (callback != null) callback(bioOk);
                } else if (excp is EBioAutenticationError) {
                  String errMsg = null;
                  if (excp is EBioBadUser)
                    errMsg = "Не верное имя пользователя [" + curUsrName + "] или пароль.";
                  else if (excp is EBioUncnfrmdUser)
                    errMsg = "Пользователь [" + curUsrName + "] не активирован.";
                  else if (excp is EBioBlockedUser)
                    errMsg = "Пользователь [" + curUsrName + "] заблокирован.";
                  else if (excp is EBioBadPwd)
                    errMsg = "Не верный пароль.";
                  _showLoginError(errMsg, () => {
                    this._processLogin(bioLoginExcp, callback);
                  });
                } else {
                  excp = new EBioException("Непредвиденная ошибка: " + r.Ex.Message);
                  if (callback != null) callback(excp);
                }
              } else {
                excp = EBioException.CreateIfNotEBio(r.Ex);
                if (callback != null) callback(excp);
              }
                
            });
          }
        });
      }
    }

    public void ProcessLogin(EBioLogin bioLoginExcp, BioRequest request, Action<EBioException, BioRequest> callback) {
      if (bioLoginExcp != null) {
        this._processLogin(bioLoginExcp, e => {
          if (callback != null) callback(e, request);
        });
      }
    }

    /// <summary>
    /// HostName сервера с которым установлено соединение
    /// </summary>
    public String CurSrvInfo {
      get {
        return this._curSrvInfo;
      }
    }

    /// <summary>
    /// Последний пользователь успешно прошедший идентификацию на сервере
    /// </summary>
    public BioUser CurUsr { get; private set; }

    public void AssignCurUser(BioUser usr) {
      this.CurUsr = (BioUser)usr.Clone();
    }
  }
}
