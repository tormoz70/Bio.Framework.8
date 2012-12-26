namespace Bio.Framework.Client.SL {
  using System;
  using System.ComponentModel;
  using System.Globalization;
  using System.Reflection;

  /// <summary>
  /// Интерфейс предназначен для реализации собственных настроек для плагина.
  /// </summary>
  public interface IConfigurable {
    /// <summary>
    /// Конфиг плагина.
    /// </summary>
    IConfigRec Cfg { get; }

    /// <summary>
    /// Загружает параметр конфигурации из реестра.
    /// </summary>
    /// <param name="valName">Имя параметра.</param>
    /// <param name="defVal">Значение по умолчанию.</param>
    /// <returns>Значение параметра.</returns>
    T RetrieveValue<T>(String valName, T defVal);

    /// <summary>
    /// Сохраняет параметр конфигурации в реестр.
    /// </summary>
    /// <param name="valName">Имя параметра.</param>
    /// <param name="value">Значение параметра.</param>
    void StoreValue(String valName, Object value);

    /// <summary>
    /// Загружает все параметры конфигурации.
    /// </summary>
    void LoadCfg();

    /// <summary>
    /// Сохраняет все параметры конфигурации.
    /// </summary>
    void SaveCfg();
  }
}
