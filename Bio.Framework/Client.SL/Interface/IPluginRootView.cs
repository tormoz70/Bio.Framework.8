namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.Windows;

  /// <summary>
  /// Интерфейс, который должны поддерживать View плагинна IPluginRoot, 
  /// </summary>
  public interface IPluginRootView : IPluginView {
    void showBusyIndicator(String msg);
    void hideBusyIndicator();
  }
}
