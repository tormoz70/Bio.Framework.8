using System;
using System.Windows;
using System.IO;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;

namespace Bio.Helpers.Controls.SL {
  public partial class msgBx {
    public msgBx() {
      InitializeComponent();
    }

    private void OKButton_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

    /// <summary>
    /// Выводит окно сообщения об ошибке
    /// </summary>
    /// <param name="errorText">Текст ошибки или Json-строка объекта ошибки</param>
    /// <param name="title">Заголовок</param>
    /// <param name="callback"></param>
    public static void ShowError(String errorText, String title, Action callback) {
      EBioException ex = null;
      try {
        ex = EBioException.Decode(errorText);
      } catch (Exception) { }
      if (ex != null) {
        ShowError(ex, title, callback);
      } else {
        _showMsg(errorText, title, callback);
      }
    }

    private const Boolean CB_FORCE_DEBUG_MODE = false;

    public static String FormatError(EBioException error) {
      if (error != null) {
        var showDebug = BioGlobal.Debug || BioGlobal.CurUsrIsDebugger || !BioGlobal.CurSessionIsLoggedOn;
        var stringWriter = new StringWriter();
        var msg = error.ApplicationErrorMessage;
        if (String.IsNullOrEmpty(msg))
          msg = "Произошла непредвиденная ошибка! Просим извинения за причиненные неудобства.\n" +
                  "Попробуйте перезапустить браузер и повторить операцию через несколько минут.";
        else
          msg = "Ошибка приложения:\n\t" + msg;
        stringWriter.WriteLine(msg);
        stringWriter.WriteLine(new String('*', 280));
//#if DEBUG
        if (showDebug || CB_FORCE_DEBUG_MODE) {
          if (error.Message != null) {
            stringWriter.WriteLine("Ошибка системы [" + error.GetType().FullName + 
              ((error.InnerException != null) ? "("+error.InnerException.GetType().FullName+")" : null)
              + "]:");
            stringWriter.WriteLine("\t" + error.Message);
            if(error.InnerException != null)
              stringWriter.WriteLine("\t\t" + error.InnerException.Message);
            stringWriter.WriteLine(new String('*', 280));
          }
          if (error.StackTrace != null) {
            stringWriter.WriteLine("Trace:");
            stringWriter.WriteLine("\t" + error.StackTrace);
            stringWriter.WriteLine(new String('*', 280));
          }
          if (error.ServerTrace != null) {
            stringWriter.WriteLine("ServerTrace:");
            stringWriter.WriteLine("\t" + error.ServerTrace);
            stringWriter.WriteLine(new String('*', 280));
          }
        }
//#endif
        return stringWriter.ToString();
      }
      return null;
    }

    /// <summary>
    /// Выводит окно сообщения об ошибке
    /// </summary>
    /// <param name="error">Класс ошибки</param>
    /// <param name="title">Заголовок</param>
    /// <param name="callback"></param>
    public static void ShowError(EBioException error, String title, Action callback) {
      var s = FormatError(error);
      _showMsg(s, title, callback);
    }

    /// <summary>
    /// Выводит окно сообщения. Не зависит от потоков.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="title">Заголовок</param>
    /// <param name="callback"></param>
    private static void _showMsg(String text, String title, Action callback) {
      if (!Utl.IsUiThread)
        Utl.UiThreadInvoke(() => {
          _showMsg(text, title, callback);
        });
      else {
        var win = new msgBx();
        win.Title = title;
        win.txtMsg.Text = text;
        win.Closed += (sender, e) => {
          if (callback != null) callback();
        };
        win.ShowDialog();
      }
    }

  }
}

