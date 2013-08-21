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
  public partial class PluginNotFoundDummyView : PluginViewBase, IPluginView {
    public PluginNotFoundDummyView() {
      InitializeComponent();
    }
    public PluginNotFoundDummyView(IPlugin owner)
      : base(owner) {
      InitializeComponent();
    }
    
    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
      if (this.ownerPlugin != null)
        this.laTitle.Content = String.Format("Модуль \"{0}\" не найден на сервере.", this.ownerPlugin.ModuleName);
    }
  }
}
