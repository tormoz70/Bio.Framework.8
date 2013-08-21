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

namespace Bio.Framework.Client.SL {
  public partial class FrmLoginBase : ChildWindow {
    public FrmLoginBase() {
      InitializeComponent();
    }

    public void SetUsrName(String usrName) {
      this.tbxUsrName.Text = String.IsNullOrEmpty(usrName) ? "noname" : usrName; 
    }
    public void SetUsrPwd(String usrPwd) {
      this.tbxUsrPwd.Password = String.IsNullOrEmpty(usrPwd) ? String.Empty : usrPwd;
    }
    public void SetVerInfo(String versionInfo) { 
      this.laVerInfo.Text = versionInfo; 
    }
    //public void SetSrvInfo(String serverUrl) { this.laSrvInfo.Text = serverUrl; }



    public void ShowM(Action<String> callback) {
      //Deployment.Current.Dispatcher.BeginInvoke(new Action(() => {
        this.Closed += new EventHandler((Object sender, EventArgs e) => {
          if (this.DialogResult == true) {
            if (callback != null) callback(this.tbxUsrName.Text + "/" + this.tbxUsrPwd.Password);
          } else {
            if (callback != null) callback(null);
          }
        });
        this.Show();
        this.Focus();
      //}));
    }

    private void ChildWindow_KeyDown(Object sender, KeyEventArgs e) {
      if(e.Key == Key.Enter)
        this.DialogResult = true;
      else if(e.Key == Key.Escape)
        this.DialogResult = false;
    }

    private Boolean fFocused = false;
    private void ChildWindow_GotFocus(object sender, RoutedEventArgs e) {
      if (!this.fFocused) {
        this.fFocused = true;
        if (String.IsNullOrEmpty(this.tbxUsrName.Text))
          this.tbxUsrName.Focus();
        else if (String.IsNullOrEmpty(this.tbxUsrPwd.Password))
          this.tbxUsrPwd.Focus();
        else
          this.btnOK.Focus();
      }
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
    }

    private void tbxUsrPwd_GotFocus(object sender, RoutedEventArgs e) {
      this.tbxUsrPwd.SelectAll();
    }

    private void tbxUsrName_GotFocus(object sender, RoutedEventArgs e) {
      this.tbxUsrName.SelectAll();
    }


  }
}

