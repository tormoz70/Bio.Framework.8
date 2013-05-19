using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Framework.Packets;
using System.ComponentModel;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {

  /// <summary>
  /// Состояние соединения.
  /// </summary>
  public enum RequestState {
    [Description("-")]
    Idle,
    [Description("Запрос к серверу...")]
    Requesting,
    [Description("Запрос выполнен")]
    Requested,
    [Description("Ошибка")]
    Error,
    [Description("Запрос прерван")]
    Canceled
  };

  /// <summary>
  /// Состояние соединения.
  /// </summary>
  public enum ConnectionState {
    [Description("Соединение отсутствует")]
    Unconnected,
    [Description("Соединение...")]
    Connecting,
    [Description("Соединение установлено")]
    Connected,
    [Description("Закрытие соединения...")]
    Breaking,
    [Description("Соединение разорвано")]
    Breaked,
    [Description("Соединение отменено")]
    Canceled,
    [Description("Ошибка")]
    Error
  };

  public interface IAjaxMng {

    event EventHandler<AjaxStateChangedEventArgs> OnStateChanged;
    void Request(CBioRequest ajaxRequest);
    BioUser CurUsr { get; }
    IEnvironment Env { get; }



  }
}
