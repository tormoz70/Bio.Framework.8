namespace Bio.Helpers.Common.Types {

	using System;
	using System.Diagnostics;
  using System.Collections.Generic;
#if !SILVERLIGHT
  using System.Windows.Forms;
#endif
  using System.Text;
  using System.IO;

  public delegate void CLoggerEventHandler(Object sender, String logLine);
	public static class CLogger{
    private const String csDefaultLoggerName = "A6306937-46C7-46F0-B45B-174C9165478D";
    private static Dictionary<String, CLoggerItem> sFLoggers = null;

    static CLogger() {
      if (sFLoggers == null)
        sFLoggers = new Dictionary<String, CLoggerItem>();
		}

    /// <summary>
    /// Регистрация обработчика собития OnLog для Логгера "loggerName"
    /// </summary>
    /// <param name="loggerName">Если null, то для default-Логгера</param>
    /// <param name="eventHandler"></param>
    public static void RegisterLogger(String loggerName, CLoggerEventHandler eventHandler, Boolean insDateTimeMarker) {
      CLoggerItem vItem = null;
      String locLoggerName = String.IsNullOrEmpty(loggerName) ? csDefaultLoggerName : loggerName;
      if(sFLoggers.ContainsKey(locLoggerName))
        vItem = sFLoggers[locLoggerName];
      if(vItem == null){
        vItem = new CLoggerItem();
        sFLoggers[locLoggerName] = vItem;
      }
      vItem.FInsDateTimeMarker = insDateTimeMarker;
      vItem.OnLog -= eventHandler;
      vItem.OnLog += eventHandler;
    }

#if !SILVERLIGHT
    /// <summary>
    /// Регистрация Контрола logControl как получателя лога. При этом лог будет добавляться в поле Техт.
    /// </summary>
    /// <param name="loggerName">Если null, то для default-Логгера</param>
    /// <param name="logControl"></param>
    /// <param name="insDateTimeMarker">Если true, то в начале каждой строки буде вставляться метка [дата/время]</param>
    public static void RegisterLogger(String loggerName, Control logControl, Boolean insDateTimeMarker) {
      if (logControl != null) {
        CLoggerEventHandler eventHandler = new CLoggerEventHandler((sender, line) => {
          Control senderControl = logControl;
          if ((senderControl != null) && !senderControl.Disposing && !senderControl.IsDisposed && senderControl.IsHandleCreated) {
            Action d = new Action(() => { senderControl.Text = String.IsNullOrEmpty(senderControl.Text) ? line : senderControl.Text + "\n" + line; });
            if (senderControl.InvokeRequired) senderControl.Invoke(d); else d();
          }
        });
        RegisterLogger(loggerName, eventHandler, insDateTimeMarker);
      }
    }
#endif

    /// <summary>
    /// Регистрация Файла logFile как получателя лога.
    /// </summary>
    /// <param name="loggerName">Если null, то для default-Логгера</param>
    /// <param name="logControl"></param>
    /// <param name="insDateTimeMarker">Если true, то в начале каждой строки буде вставляться метка [дата/время]</param>
    public static void RegisterLogger(String loggerName, String logFile, Boolean insDateTimeMarker) {
      String vPath = Path.GetDirectoryName(logFile);
      if (!String.IsNullOrEmpty(vPath) && !Directory.Exists(vPath))
        Directory.CreateDirectory(vPath);
      CLoggerEventHandler eventHandler = new CLoggerEventHandler((sender, line) => {
        Utl.AppendStringToFile(logFile, line, Utl.DefaultEncoding);
      });
      RegisterLogger(loggerName, eventHandler, insDateTimeMarker);
    }

    public static void WriteLog(String loggerName, Object sender, String logLine) {
      CLoggerItem vItem = null;
      String locLoggerName = String.IsNullOrEmpty(loggerName) ? csDefaultLoggerName : loggerName;
      if (sFLoggers.ContainsKey(locLoggerName))
        vItem = sFLoggers[locLoggerName];
      if (vItem != null) {
        String locLogLine = logLine;
        if(vItem.FInsDateTimeMarker)
          locLogLine = "["+DateTime.Now+"] - "+locLogLine;
        vItem.DoOnLogEvent(sender, locLogLine);
      }
    }
    public static void WriteLog(String logLine) {
      WriteLog(null, null, logLine);
    }
  }
  class CLoggerItem {
    public Boolean FInsDateTimeMarker = false;
    public event CLoggerEventHandler OnLog;
    public void DoOnLogEvent(Object sender, String logLine) {
      if (this.OnLog != null)
        this.OnLog(sender, logLine);
    }
  }
}
