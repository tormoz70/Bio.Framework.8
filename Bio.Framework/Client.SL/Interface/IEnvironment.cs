namespace Bio.Framework.Client.SL {
  using System;
  using System.Collections;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.ComponentModel;
  using System.Windows;
  using Bio.Helpers.Common.Types;

  public class LoadPluginCompletedEventArgs : AjaxResponseEventArgs {
    public IPlugin Plugin { get; set; }
  }

  public class AjaxStateChangedEventArgs : EventArgs {
    public ConnectionState ConnectionState { get; set; }
    public RequestState RequestState { get; set; }
  }

  /// <summary>
  /// Интерфейс среды обитания плагинов
  /// 
  /// </summary>
  public interface IEnvironment: IEnumerable {

    /// <summary>
    /// Инициализация атрибутов клиента
    /// </summary>
    /// <param name="pProducerCompany"></param>
    /// <param name="pAppName"></param>
    /// <param name="pAppTitle"></param>
    /// <param name="pAppVersion"></param>
    void setAppAttrs(String pProducerCompany, String pAppName, String pAppTitle, String pAppVersion);

    /// <summary>
    /// Название компании - производителя
    /// </summary>
    String ProducerCompany { get; }
    /// <summary>
    /// Название приложения
    /// </summary>
    String AppName { get; }
    /// <summary>
    /// Заголовок приложения
    /// </summary>
    String AppTitle { get; }
    /// <summary>
    /// Версия приложения
    /// </summary>
    String AppVersion { get; }
    /// <summary>
    /// URL для нестандартных запросов к серверу
    /// </summary>
    String ServerUrl { get; }

    event EventHandler<AjaxStateChangedEventArgs> OnStateChanged;

    /*
    /// <summary>
    /// Инициализация Ajax
    /// </summary>
    /// <param name="pServerUrl"></param>
    /// <param name="pRequestTimeout"></param>
    void InitAjax(String pServerUrl, int pRequestTimeout);
    */

    void LoadRootPlugin(UIElement container, String rootPluginName);
    void LoadPlugin(IPlugin ownerPlugin, String pluginName, String pluginID, Action<LoadPluginCompletedEventArgs> act);    

    /// <summary>
    /// Доступ по индексу к плагину, зарегестрированному в среде обитания
    /// Индекс плагина определяется порядком регистрации в среде обитания.
    /// </summary>
    /// <param name="pIndex"></param>
    /// <returns></returns>
    IPlugin this[int pIndex] { get; }
    /// <summary>
    /// Количество зарегистрированных плагинов
    /// </summary>
    Int32 PlgCount { get; }

    /// <summary>
    /// Возвращает имя клиента для параметра User-Agent в запросе к серверу
    /// </summary>
    String UserAgentName { get; }
    /// <summary>
    /// Заголовок клиента + Версия
    /// </summary>
    String UserAgentTitleAndVer { get; }

    /// <summary>
    /// AjaxMng
    /// </summary>
    IAjaxMng AjaxMng { get; }

    /// <summary>
    /// Ссылка на главное окно
    /// </summary>
    UserControl StartUpControl { get; }
    IPluginRoot PluginRoot { get; }

    ///// <summary>
    ///// Вызов стандартной, удаленной процедуры экспорта CGrid
    ///// </summary>
    ///// <param name="pOwnerPlg"></param>
    ///// <param name="pGrid"></param>
    ///// <param name="pExportTitle"></param>
    //void ExportGrid(IPlugin pOwnerPlg, CGrid pGrid, String pExportTitle);

    ConnectionState ConnectionState { get; }
    RequestState RequestState { get; }

    /// <summary>
    /// Посылает Ping-запрос на сервер. Если сессия на сервере не существует,
    /// тогда будет инициирован вход в систему.
    /// </summary>
    void Connect(AjaxRequestDelegate callback);

    /// <summary>
    /// Посылает на сервер запрос о завершении сессии
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="silent"></param>
    void Disconnect(AjaxRequestDelegate callback, Boolean silent);

    /// <summary>
    /// Посылает на сервер запрос о завершении сессии : Disconnect(null, true, true);
    /// </summary>
    void Disconnect();

    /// <summary>
    /// IsConnected
    /// </summary>
    Boolean IsConnected { get; }

    /// <summary>
    /// Отсоединение, соединение
    /// </summary>
    void Reconnect();

    IConfigRoot ConfigRoot { get; }

    String LastSuccessPwd { get; set; }

    void IncreaseISQuota(Action<Boolean> callback);

    void StoreUserObject(String objName, Object obj);
    T RestoreUserObject<T>(String objName, T defObj);

  }
}
