using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bio.Helpers.Common {

  public enum RemoteProcState {
    [Description("Готов к запуску")]
    Redy =      0,
    [Description("Запускается...")]
    Starting =  1,
    [Description("Выполняется...")]
    Running =   2,
    [Description("Выполнен")]
    Done =      3,
    [Description("Останов...")]
    Breaking =  4,
    [Description("Остановлен")]
    Breaked =   5,
    [Description("Ошибка")]
    Error =     6,
    [Description("В очереди...")]
    Waiting =   7,
    [Description("Удален")]
    Disposed =  8
  };

  public static class rmtUtl {
    /// <summary>
    /// Выполняется
    /// </summary>
    public static Boolean isRunning(RemoteProcState state) {
      switch (state) {
        case RemoteProcState.Breaked:
        case RemoteProcState.Done:
        case RemoteProcState.Error:
        case RemoteProcState.Redy:
          return false;
        default:
          return true;
      }
    }

    /// <summary>
    /// Выполнен
    /// </summary>
    public static Boolean isFinished(RemoteProcState state) {
      switch (state) {
        case RemoteProcState.Breaked:
        case RemoteProcState.Done:
        case RemoteProcState.Error:
          return true;
        default:
          return false;
      }
    }
  }
}
