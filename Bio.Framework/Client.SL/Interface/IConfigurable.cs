namespace Bio.Framework.Client.SL {
  using System;

  /// <summary>
  /// Интерфейс предназначен для реализации собственных настроек для плагина.
  /// </summary>
  public interface IConfigurable<out T> {
    /// <summary>
    /// Конфиг плагина.
    /// </summary>
    T Cfg { get; }

  }
}
