using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Helpers.Controls.SL;

namespace Bio.Framework.Client.SL {
  public partial class FrmPromptIncreaseQuotaIS : ChildWindow {
    public FrmPromptIncreaseQuotaIS() {
      InitializeComponent();
    }

    private Int64 _newQuota;
    public void ShowM(Int64 newQuota, Action<Boolean> callback) {
      this._newQuota = newQuota;
      this.Closed += new EventHandler((Object sender, EventArgs e) => {
        if (callback != null) callback(this.DialogResult.HasValue && this.DialogResult.Value);
      });
      this.Show();
      this.Focus();
    }

    private void ChildWindow_KeyDown(Object sender, KeyEventArgs e) {
      if(e.Key == Key.Enter)
        this.DialogResult = true;
      else if(e.Key == Key.Escape)
        this.DialogResult = false;
    }

    private void ChildWindow_GotFocus(object sender, RoutedEventArgs e) {
      this.btnOK.Focus();
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication()) {
        long newSpace = this._newQuota;
        try {
          isf.IncreaseQuotaTo(newSpace);
          this.DialogResult = true;
        } catch (Exception ex) {
          msgBx.showError(ex.ToString(), "Ошибка расширения локального хранилища", null);
        }
      }
    }
    

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
    }

  }
}

