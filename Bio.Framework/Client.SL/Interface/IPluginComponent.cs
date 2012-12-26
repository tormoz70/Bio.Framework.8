namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.Windows;

  public interface IPluginComponent {
    IPlugin ownerPlugin { get; }
  }
}
