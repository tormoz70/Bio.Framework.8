namespace Bio.Helpers.Ajax {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Net;
  using System.IO;
  using System.Threading;
#if !SILVERLIGHT
  using System.Web;
  using System.Windows.Forms;
#endif
  using System.ComponentModel;
  using Bio.Helpers.Common.Types;
  using System.Collections;
  using Newtonsoft.Json;
  using System.Reflection;
  using Bio.Helpers.Common;



  /// <summary>
  /// Аналог Ajax - клиента в JavaScript
  /// </summary>
  public class CAjaxCli : DisposableObject {

    private String FVersion = "1.0";
    private String FUserAgentName = "daAjax.CAjaxMng";
    private String FUserAgentTitle = "BioSys";
    private int FRequestTimeout = ajaxUTL.RequestTimeout;

    /// <summary>
    /// Текущая версия клиента
    /// </summary>
    public String UserAgentVersion {
      get {
        return this.FVersion;
      }
    }

    /// <summary>
    /// Имя Клиента
    /// </summary>
    public String UserAgentName {
      get {
        return this.FUserAgentName;
      }
    }

    /// <summary>
    /// Заголовок Клиента
    /// </summary>
    public String UserAgentTitle {
      get {
        return this.FUserAgentTitle;
      }
    }

    /// <summary>
    /// Кол-во секунд ожидания запроса от сервера
    /// </summary>
    //public int RequestTimeout {
    //  get { return this.FRequestTimeout; }
    //}

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pVersion"></param>
    /// <param name="pUserAgentName"></param>
    /// <param name="pUserAgentTitle"></param>
    /// <param name="pDOnLogLine">Процедура, которая получает отладочную инфу</param>
    public CAjaxCli(String version, String userAgentName, String userAgentTitle) {
      this.FVersion = version ?? this.FVersion;
      this.FUserAgentName = userAgentName ?? this.FUserAgentName;
      this.FUserAgentTitle = userAgentTitle ?? this.FUserAgentTitle;
      //this.FRequestTimeout = (requestTimeout > 0) ? requestTimeout : this.FRequestTimeout;
    }

    //private void _processEvents(CAjaxRequest ajaxRequest, CAjaxResponse ajaxResponse, AjaxRequestDelegate callback, Object usrToken) {

    //  if (callback != null) {
    //    AjaxResponseEventArgs args = new AjaxResponseEventArgs { request = ajaxRequest, response = ajaxResponse };
    //    callback(this, args);
    //  }

    //}

    /// <summary>
    /// Выполнить запрос 
    /// </summary>
    /// <param name="pCfg"></param>
    public void Request(CAjaxRequest ajaxRequest, Int32 requestTimeout) {
      /*Подготовка запроса*/
      //ajaxUTL.getDataFromSrv(ajaxRequest, 90, (rq, rsp, usrToken) => {
      //  this._processEvents(rq, rsp, callback, usrToken);
      //}, ajaxRequest.userToken);
      ajaxRequest.timeout = requestTimeout;
      ajaxUTL.getDataFromSrv(ajaxRequest);
    }

    /// <summary>
    /// Прервать запрос
    /// </summary>
    public void Abort() {
      //ajaxUTL.abortRequest();
    }

    protected override void doOnDispose() {
      this.Abort();
      base.doOnDispose();
    }


    /// <summary>
    /// HostName сервера с которым установлено соединение
    /// </summary>
    //public String CurSrvInfo{
    //  get {
    //    return this.FLoginPrc.CurSrvInfo;
    //  }
    //}


  }

  /// <summary>
  /// Настройки Proxy
  /// </summary>
  public class CDProxy {
    /// <summary>
    /// Адрес proxy-сенрвера
    /// </summary>
    public String Address { get; set; }
    /// <summary>
    /// Порт
    /// </summary>
    public String Port { get; set; }
    /// <summary>
    /// Пользователь
    /// </summary>
    public String UserName { get; set; }
    /// <summary>
    /// Пароль
    /// </summary>
    public String Password { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool BypassOnLocal { get; set; }
  }
}
