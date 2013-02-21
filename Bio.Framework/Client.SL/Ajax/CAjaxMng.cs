namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using System.Diagnostics;
  using Bio.Framework.Packets;
  using Bio.Helpers.Ajax;
  using System.Threading;
  using System.ComponentModel;
  using System.Collections.Generic;
  using System.Windows.Controls;
  using System.Windows;
  using Bio.Helpers.Controls.SL;

  /// <summary>
  /// Менеджер запросов к серверу
  /// </summary>
  public class CAjaxMng : IAjaxMng {
    private readonly Queue<CBioRequest> _queue = null;
    private readonly CAjaxCli _ajax = null;
    private readonly CAjaxLogin _loginPrc = null;

    
    /// <summary>
    /// Создаёт экземпляр класса.
    /// </summary>
    /// <param name="env">Ссылка на среду окружения</param>
    public CAjaxMng(IEnvironment env) {
      this.Env = env;
      this._queue = new Queue<CBioRequest>();
      this._ajax = new CAjaxCli(env.AppVersion, env.UserAgentName, env.AppTitle);
      this._loginPrc = new CAjaxLogin(this, this.doOnLogLine);
    }

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<AjaxStateChangedEventArgs> OnStateChanged;
    
    /// <summary>
    /// Вызывается каждый раз при изменении состояния соединения
    /// </summary>
    protected virtual void doOnStateChanged() {
      var evs = this.OnStateChanged;
      if (evs != null)
        evs(this, new AjaxStateChangedEventArgs { ConnectionState = this.ConnectionState, RequestState = this.RequestState });
    }

    /// <summary>
    /// Ajax - клиент
    /// </summary>
    public CAjaxCli AjaxCli {
      get { return this._ajax; }
    }

    private void doOnLogLine(String pLine) {
      try {
        //Trace.WriteLine(pLine);
      }catch(ObjectDisposedException){ }
    }


    /// <summary>
    /// Прервать запрос
    /// </summary>
    public void Abort() {
      this._ajax.Abort();
    }

    /// <summary>
    /// Последний пользователь успешно прошедший идентификацию на сервере
    /// </summary>
    public CBioUser CurUsr {
      get {
        return this._loginPrc.CurUsr;
      }
    }

    private void _processQueue() {
      if (this._queue.Count > 0) {
        var v_deferredRequest = this._queue.Dequeue();
        this.Request(v_deferredRequest);
      }
    }

    private CBioRequest _currentRequest = null;

    private void _processCallback(AjaxRequestDelegate callback, Object sender, AjaxResponseEventArgs args) {
      Utl.UiThreadInvoke(() => {
        if (callback != null) callback(sender, args);
        this._processQueue();
      });
    }

    private void _request(CBioRequest bioRequest) {
      //Debug.WriteLine("Request:0 - bioRequest.requestType[" + bioRequest.requestType + "]; prms:" + bioRequest.prms);
      if ((this.RequestState == RequestState.Requesting) && (this._currentRequest != bioRequest)) {
        this._queue.Enqueue(bioRequest);
        //Debug.WriteLine("Request:1 - _queue.Enqueue(); return;");
        return;
      }
      this.RequestState = RequestState.Requesting;
      this._currentRequest = bioRequest;

      if (bioRequest.url == null) {
        bioRequest.url = this.Env.ServerUrl; //Utl.BuildURL(this.ServerUrl, null);
      }
      var v_clbck = bioRequest.callback;
      bioRequest.callback = (sender, args) => {
          //Debug.WriteLine("Request:2 - bioRequest.callback - start");
          CBioResponse response = args.response as CBioResponse;
          if ((response != null) && !response.success) {
            //Debug.WriteLine("Request:3 - response.success:false");
            // Сервер вернул ошибку
            EBioLogin vBioLoginExcp = CAjaxLogin.decBioLoginExcp(response.ex);
            if (vBioLoginExcp != null) {
              //Debug.WriteLine("Request:4 - vBioLoginExcp:" + vBioLoginExcp.GetType().Name);
              // Попадаем сюда если сервер вернул сообщение связанное с логином (Например EBioStart-это значит, что соединение отсутствует и необходимо начать новую сессию)  
              // Сбрасываем параметры сессии
              ajaxUTL.sessionID = null;
              if (vBioLoginExcp is EBioStart) {
                this.ConnectionState = ConnectionState.Connecting;
                // - соединение отсутствует и сервер запросил параметры для новой сессии
                // - запускаем процедуру логина.
                //Debug.WriteLine("Request:5 - запускаем процедуру логина: this._loginPrc.processLogin");
                this._loginPrc.processLogin(vBioLoginExcp, args.request as CBioRequest, (e, r) => {
                  response.ex = e;
                  if (response.ex is EBioOk) {
                    //Debug.WriteLine("Request:6 - this._loginPrc.processLogin: response.ex == null");
                    // -- Новая сессия создана без ошибок
                    // устанавливаем состояние сессии
                    if (this.ConnectionState == ConnectionState.Connecting)
                      this.ConnectionState = ConnectionState.Connected;
                    else if (this.ConnectionState == ConnectionState.Breaking)
                      this.ConnectionState = ConnectionState.Breaked;
                    // тут необходимо запустить запрос, который был отправлен на сервер перед тем как сервер вернул запрос параметров новой сессии
                    //Debug.WriteLine("Request:7 - перезапускаем первоначальный запрос: this.Request(args.request as CBioRequest);");
                    this.Request(args.request as CBioRequest);
                  } else {
                    //Debug.WriteLine("Request:8 - this._loginPrc.processLogin: response.ex: " + response.ex.GetType().Name);
                    // -- При создании сессии произошла ошибка
                    if (response.ex is EBioCancel) {
                      // Пользователь нажал кнопку "Отмена" в окне ввода логина
                      this.ConnectionState = ConnectionState.Canceled;
                      this.RequestState = RequestState.Requested;
                      this._processCallback(v_clbck, this, args);
                    } else {
                      // Непредвиденная ошибка при проверке логина
                      this.ConnectionState = ConnectionState.Error;
                      this.RequestState = RequestState.Requested;
                      if (!args.request.silent) {
                        if (!(response.ex is EBioAutenticationError))
                          msgBx.showError(response.ex, "Ошибка обращения к серверу", () => this._processCallback(v_clbck, this, args));
                      }
                    }
                  }

                });
              } else {
                //Debug.WriteLine("Request:11 - Непредвиденная ошибка.");
                // Непредвиденная ошибка
                this.ConnectionState = ConnectionState.Error;
                this.RequestState = RequestState.Requested;
                if (!args.request.silent) {
                  msgBx.showError(response.ex, "Ошибка обращения к серверу", () => {
                    this._processCallback(v_clbck, this, args);
                  });
                } else
                  this._processCallback(v_clbck, this, args);
              }
            } else {
              //Debug.WriteLine("Request:12 - Непредвиденная ошибка.");
              //this.ConnectionState = ConnectionState.Unconnected;
              this.RequestState = RequestState.Error;
              if (!args.request.silent) {
                msgBx.showError(response.ex, "Ошибка обращения к серверу", () => this._processCallback(v_clbck, this, args));
              } else
                this._processCallback(v_clbck, this, args);
            }
          } else {
            if (args.response.ex != null) {
              if (args.response.ex is EBioLoggedOut) {
                //Debug.WriteLine("Request:9 - сессия завершена.");
                // - сервер завершил сессию
                this.ConnectionState = ConnectionState.Breaked;
              } else if (args.response.ex is EBioOk) {
                // Это ответ на запрос doPing когда сессия уже существует
                //Debug.WriteLine("Request:10 - сессия создана.");
                // ответ - заершающий создание сесии
                this._loginPrc.assignCurUser((args.response.ex as EBioOk).Usr);
                this.ConnectionState = ConnectionState.Connected;
                //Debug.WriteLine("Request:10-1 - this._queue.Dequeue();");
              }
            } 
            //Debug.WriteLine("Request:13 - Запрос выполнен.");
            this.RequestState = RequestState.Requested;
            this._processCallback(v_clbck, this, args);
            //Debug.WriteLine("Request:14 - this._queue.Dequeue();");
            
          }

        };

      this._ajax.Request(bioRequest, this.Env.ConfigRoot.RequestTimeout);
    }

    /// <summary>
    /// Выполнить запрос к серверу
    /// </summary>
    /// <param name="bioRequest">Конфигурация запроса</param>
    public void Request(CBioRequest bioRequest) {
      this._request(bioRequest);
    }
	
    public static String SessionId{
      get {
        if(ajaxUTL.sessionID != null)
          return ajaxUTL.sessionID.Value;
        return null;
      }
    }
    public static String SessionIdParName {
      get { return ajaxUTL.csSessionIdName; }
    }

    /// <summary>
    /// Возвращает ID сессии в формате cookie
    /// </summary>
    public static String SessionIdCookieStr {
      get {
        if(ajaxUTL.sessionID != null)
          return ajaxUTL.csSessionIdName+"="+ajaxUTL.sessionID.Value;
        return null;
      }
    }

    /// <summary>
    /// Заголовок Клиента
    /// </summary>
    public String UserAgentTitle {
      get {
        return this._ajax.UserAgentTitle;
      }
    }

    /// <summary>
    /// Текущая версия клиента
    /// </summary>
    public String UserAgentVersion {
      get {
        return this._ajax.UserAgentVersion;
      }
    }

    /// <summary>
    /// Заголовок клиента + версия
    /// </summary>
    public String UserAgentTitleAndVer {
      get {
        return String.Format("{0} v{1}", this.UserAgentTitle, this.UserAgentVersion);
      }
    }

    private ConnectionState _connState = ConnectionState.Unconnected;
    private RequestState _reqstState = RequestState.Idle;
    /// <summary>
    /// Состояние текущего соединения
    /// </summary>
    public ConnectionState ConnectionState {
      get {
        return this._connState;
      }
      private set {
        var v_stateChanged = this._connState != value;
        this._connState = value;
        if (v_stateChanged)
          this.doOnStateChanged();
      }
    }
    /// <summary>
    /// Состояние текущего соединения
    /// </summary>
    public RequestState RequestState {
      get {
        return this._reqstState;
      }
      private set {
        this._reqstState = value;
        this.doOnStateChanged();
      }
    }

    public IEnvironment Env { get; private set; }
  }
}
