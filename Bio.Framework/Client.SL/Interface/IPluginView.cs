namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.Windows;

  /// <summary>
  /// Интерфейс, который должны поддерживать все View плагинов, 
  /// </summary>
  public interface IPluginView : IPluginComponent {
    void Show(UIElement container);
    void ShowDialog(Action<Boolean?> callback);
    void ShowSelector(VSelection selection, SelectorCallback callback);
    void Close();
  }
}
