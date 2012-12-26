namespace Bio.Helpers.Common.Types {
#if !SILVERLIGHT
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Drawing;
  using System.Reflection;
  using System.Windows.Forms;
  using System.IO;
  using Bio.Helpers.Common.Types;

  public partial class msgBx:Form {
    /// <summary>
    /// Конструктор
    /// </summary>
    public msgBx() {
      InitializeComponent();
    }

    /// <summary>
    /// SafeThread call of ShowDialog
    /// </summary>
    /// <param name="pOwner"></param>
    private void showDlgTS(Control pOwner) {
      if ((pOwner != null) && pOwner.InvokeRequired) {
        try {
          pOwner.Invoke(new Action<Control>(showDlgTS), new Object[] { pOwner });
        } catch (ObjectDisposedException) { }
      } else {
        this.ShowDialog(pOwner);
      }
    }

    /// <summary>
    /// Выводит окно сообщения об ошибке
    /// </summary>
    /// <param name="pErrorText">Текст ошибки или Json-строка объекта ошибки</param>
    /// <param name="pTitle">Заголовок</param>
    /// <param name="pOwner">Ссылка на окно-родитель</param>
    public static void showError(String pErrorText, String pTitle, Control pOwner) {
      EBioException vEx = EBioException.Decode(pErrorText);
      if(vEx != null) {
        showError(vEx, pTitle, pOwner);
      } else {
        showModal(pErrorText, pTitle, pOwner);
      }
    }


    /// <summary>
    /// Выводит окно сообщения об ошибке
    /// </summary>
    /// <param name="pError">Класс ошибки</param>
    /// <param name="pTitle">Заголовок</param>
    /// <param name="pOwner">Ссылка на окно-родитель</param>
    public static void showError(EBioException pError, String pTitle, Control pOwner) {
      if(pError != null) {
        StringWriter vS = new StringWriter();
        if(pError.ApplicationErrorMessage != null) {
          vS.WriteLine("Ошибка приложения:");
          vS.WriteLine("\t" + pError.ApplicationErrorMessage);
          vS.WriteLine(new String('*', 280));
        }
#if DEBUG
        if(pError.Message != null) {
          vS.WriteLine("Ошибка системы:");
          vS.WriteLine("\t" + pError.Message);
          vS.WriteLine(new String('*', 280));
        }
        if(pError.StackTrace != null) {
          vS.WriteLine("Trace:");
          vS.WriteLine("\t" + pError.StackTrace);
          vS.WriteLine(new String('*', 280));
        }
        if(pError.ServerTrace != null) {
          vS.WriteLine("ServerTrace:");
          vS.WriteLine("\t" + pError.ServerTrace);
          vS.WriteLine(new String('*', 280));
        }
#endif
        showModal(vS.ToString(), pTitle, pOwner);
      }
    }

    /// <summary>
    /// Выводит окно сообщения. Не зависит от потоков.
    /// </summary>
    /// <param name="pText">Текст сообщения</param>
    /// <param name="pTitle">Заголовок</param>
    /// <param name="pOwner">Ссылка на окно-родитель</param>
    private static void showModal(String pText, String pTitle, Control pOwner) {
      msgBx vWin = new msgBx();
      vWin.Text = pTitle;
      vWin.rtxtText.Text = pText;
      if (pOwner != null) {
        try {
          vWin.showDlgTS(pOwner);
        } catch (ObjectDisposedException) {
          vWin.showDlgTS(null);
        }
      } else {
        vWin.showDlgTS(null);
        //CMsgs.showModalMsg(pText, pTitle);
      }
    }

  }
#endif
}
