namespace Bio.Framework.Client.SL {
  using System;

  /// <summary>
  /// Интерфейс root-плагинов
  /// </summary>
  public interface IPluginRoot: IPlugin {
    IConfigRoot CfgRoot { get; }
    /// <summary>
    /// Название компании - производителя
    /// </summary>
    String ProducerCompany { get; }
    /// <summary>
    /// Название приложения
    /// </summary>
    String AppName { get; }
    /// <summary>
    /// Заголовок приложения
    /// </summary>
    String AppTitle { get; }
    /// <summary>
    /// Версия приложения
    /// </summary>
    String AppVersion { get; }

  }

}
