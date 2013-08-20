using System;
using System.Windows;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Controls.SL;

namespace Bio.Framework.Client.SL {
  public partial class StartUp {
    public StartUp() {
      InitializeComponent();
      BioEnvironment.Init(this);
      Application.Current.UnhandledException += Current_UnhandledException;
    }

    void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
//#if !DEBUG
      if (!e.Handled) {
        e.Handled = true;
        msgBx.ShowError(EBioException.CreateIfNotEBio(e.ExceptionObject), "Непредвиденная ошибка!", null);
      }
//#endif
    }

    public String RootPluginName { get; set; }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
      if (!Utl.DesignTime) {
        //System.Diagnostics.Debug.WriteLine("Utl.DesignTime = false!");
        BioEnvironment.Instance.LoadRootPlugin(this.contentControlPart, this.RootPluginName);
        //this.contentControlPart.Content = new PluginNotFoundDummyView();
      }
    }
  }
}
