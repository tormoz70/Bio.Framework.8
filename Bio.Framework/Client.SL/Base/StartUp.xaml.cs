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
using Bio.Helpers.Common;
using System.ComponentModel;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Controls.SL;

namespace Bio.Framework.Client.SL {
  public partial class StartUp : UserControl {
    public IEnvironment Env = null;
    public StartUp() {
      InitializeComponent();
      this.Env = new CEnvironment(this);
      Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(Current_UnhandledException);
    }

    void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
//#if !DEBUG
      if (!e.Handled) {
        e.Handled = true;
        msgBx.showError(EBioException.CreateIfNotEBio(e.ExceptionObject), "Непредвиденная ошибка!", null);
      }
//#endif
    }

    public String RootPluginName { get; set; }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
      if (!Utl.DesignTime) {
        //System.Diagnostics.Debug.WriteLine("Utl.DesignTime = false!");
        this.Env.LoadRootPlugin(this.contentControlPart, this.RootPluginName);
        //this.contentControlPart.Content = new PluginNotFoundDummyView();
      }
    }
  }
}
