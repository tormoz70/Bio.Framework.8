using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {

  public class PluginViewDialogButton {
    public String Name { get; set; }
    public String Caption { get; set; }
    public Boolean? IsDisabled { get; set; }
    public RoutedEventHandler OnClick { get; set; }
  }

  public interface ISelectorView {
    /// <summary>
    /// Значение первичного ключа ывбранной записи
    /// </summary>
    VSelection Selection { get; }
    void AddButtons(params PluginViewDialogButton[] buttons);
  }
}
