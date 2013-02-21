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

    private readonly String _version = "1.0";
    private readonly String _userAgentName = "daAjax.CAjaxMng";
    private readonly String _userAgentTitle = "BioSys";

    /// <summary>
    /// Текущая версия клиента
    /// </summary>
    public String UserAgentVersion {
      get {
        return this._version;
      }
    }

    /// <summary>
    /// Имя Клиента
    /// </summary>
    public String UserAgentName {
      get {
        return this._userAgentName;
      }
    }

    /// <summary>
    /// Заголовок Клиента
    /// </summary>
    public String UserAgentTitle {
      get {
        return this._userAgentTitle;
      }
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="version"></param>
    /// <param name="userAgentName"></param>
    /// <param name="userAgentTitle"></param>
    public CAjaxCli(String version, String userAgentName, String userAgentTitle) {
      this._version = version ?? this._version;
      this._userAgentName = userAgentName ?? this._userAgentName;
      this._userAgentTitle = userAgentTitle ?? this._userAgentTitle;
    }

    /// <summary>
    /// Выполнить запрос 
    /// </summary>
    public void Request(CAjaxRequest ajaxRequest, Int32 requestTimeout) {
      /*Подготовка запроса*/
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
