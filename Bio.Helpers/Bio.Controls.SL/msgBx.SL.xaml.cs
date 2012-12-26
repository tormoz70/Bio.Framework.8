using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;

namespace Bio.Helpers.Controls.SL {
  public partial class msgBx : FloatableWindow {
    public msgBx() {
      InitializeComponent();
    }

    private void OKButton_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

    //public static void showMsgModal(String pText, String pTitle) {
    //    msgBx vWin = new msgBx();
    //    vWin.Title = pTitle;
    //    vWin.txtMsg.Text = pText;
    //    vWin.ShowMod();
    //}

    /// <summary>
    /// Выводит окно сообщения об ошибке
    /// </summary>
    /// <param name="pErrorText">Текст ошибки или Json-строка объекта ошибки</param>
    /// <param name="pTitle">Заголовок</param>
    public static void showError(String pErrorText, String pTitle, Action callback) {
      EBioException vEx = null;
      try {
        vEx = EBioException.Decode(pErrorText);
      } catch (Exception) { }
      if (vEx != null) {
        showError(vEx, pTitle, callback);
      } else {
        showMsg(pErrorText, pTitle, callback);
      }
    }

    private static Boolean cbForceDebugMode = false;
    public static String formatError(EBioException error) {
      if (error != null) {
        var v_show_debug = BioGlobal.Debug || BioGlobal.CurUsrIsDebugger || !BioGlobal.CurSessionIsLoggedOn;
        StringWriter vS = new StringWriter();
        var v_msg = error.ApplicationErrorMessage;
        if (String.IsNullOrEmpty(v_msg))
          v_msg = "Произошла непредвиденная ошибка! Просим извинения за причиненные неудобства.\n" +
                  "Попробуйте перезапустить браузер и повторить операцию через несколько минут.";
        else
          v_msg = "Ошибка приложения:\n\t" + v_msg;
        vS.WriteLine(v_msg);
        vS.WriteLine(new String('*', 280));
//#if DEBUG
        if (v_show_debug || cbForceDebugMode) {
          if (error.Message != null) {
            vS.WriteLine("Ошибка системы [" + error.GetType().FullName + 
              ((error.InnerException != null) ? "("+error.InnerException.GetType().FullName+")" : null)
              + "]:");
            vS.WriteLine("\t" + error.Message);
            if(error.InnerException != null)
              vS.WriteLine("\t\t" + error.InnerException.Message);
            vS.WriteLine(new String('*', 280));
          }
          if (error.StackTrace != null) {
            vS.WriteLine("Trace:");
            vS.WriteLine("\t" + error.StackTrace);
            vS.WriteLine(new String('*', 280));
          }
          if (error.ServerTrace != null) {
            vS.WriteLine("ServerTrace:");
            vS.WriteLine("\t" + error.ServerTrace);
            vS.WriteLine(new String('*', 280));
          }
        }
//#endif
        return vS.ToString();
      } else
        return null;
    }
    /// <summary>
    /// Выводит окно сообщения об ошибке
    /// </summary>
    /// <param name="error">Класс ошибки</param>
    /// <param name="title">Заголовок</param>
    public static void showError(EBioException error, String title, Action callback) {
      String vS = formatError(error);
      showMsg(vS, title, callback);
    }

    /// <summary>
    /// Выводит окно сообщения. Не зависит от потоков.
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="title">Заголовок</param>
    private static void showMsg(String text, String title, Action callback) {
      if (!Utl.IsUiThread)
        Utl.UiThreadInvoke(() => {
          showMsg(text, title, callback);
        });
      else {
        msgBx vWin = new msgBx();
        vWin.Title = title;
        vWin.txtMsg.Text = text;
        vWin.Closed += new EventHandler((Object sender, EventArgs e) => {
          if (callback != null) callback();
        });
        vWin.ShowDialog();
      }
    }

  }
}

