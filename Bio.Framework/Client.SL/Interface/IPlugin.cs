namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.Windows;

  /// <summary>
  /// Класс, описывающий параметры для события DataChanged.
  /// </summary>
  public class DataChangedEventArgs : EventArgs {
    /// <summary>
    /// Параметры, передаваемые в событие.
    /// </summary>
    public Params Params { get; private set; }
    /// <summary>
    /// Создаёт экземпляр класса, описывающего параметры, передаваемые в событие.
    /// </summary>
    /// <param name="pars">Параметры.</param>
    public DataChangedEventArgs(Params pars) {
      this.Params = pars;
    }

    /// <summary>
    /// Пустые параметры.
    /// </summary>
    public new static DataChangedEventArgs Empty = new DataChangedEventArgs(null);
  }

  /// <summary>
  /// Класс, описывающий параметры для события DataChanging.
  /// </summary>
  public class DataChangingCancelEventArgs : EventArgs {
    private bool _cancel;
    /// <summary>
    /// Признак отмены события.
    /// </summary>
    public bool Cancel {
      get { return this._cancel; }
      set { this._cancel = value || this._cancel; }
    }
    /// <summary>
    /// Параметры, передаваемые в событие.
    /// </summary>
    public Params Params { get; private set; }
    /// <summary>
    /// Создаёт экземпляр класса, описывающего параметры, передаваемые в событие.
    /// </summary>
    /// <param name="pars">Параметры.</param>
    /// <param name="cancel">Признак отмены события.</param>
    public DataChangingCancelEventArgs(Params pars, bool cancel) {
      this.Params = pars;
      this._cancel = cancel;
    }
  }

  /// <summary>
  /// Базовый интерфейс плагинов
  /// Все плагины в системе поддерживают данный интерфейс
  /// </summary>
  public interface IPlugin {  
    /// <summary>
    /// Заголовок
    /// </summary>
    String ViewTitle { get; }
    /// <summary>
    /// Основной контрол плагина
    /// </summary>
    IPluginView View { get; set; }
    /// <summary>
    /// Показать основной контрол плагина в контейнере
    /// </summary>
    /// <param name="container">Это может быть Panel или ContentControl</param>
    void Show(UIElement container);
    /// <summary>
    /// Показать основной контрол плагина в диалоговом окне
    /// </summary>
    /// <param name="callback"></param>
    void ShowDialog(Action<Boolean?> callback);

    void Close();

    /// <summary>
    /// Имя модуля
    /// </summary>
    String ModuleName { get; }

    /// <summary>
    /// Имя плагина совпадает с именем модуля
    /// </summary>
    String PluginName { get; }

    /// <summary>
    /// ID плагина. Может совпадать с именем модуля, а может и отличаться
    /// </summary>
    String PluginID { get; }


    /// <summary>
    /// Ссылка на владельца
    /// </summary>
    IPlugin Owner { get; }

    /// <summary>
    /// Ссылка на среду
    /// </summary>
    IEnvironment Env { get; }

    /// <summary>
    /// Параметры плагина
    /// </summary>
    Params Params { get; }

    /// <summary>
    /// Обновление данных извне
    /// </summary>
    /// <param name="prms">Вход. параметры</param>
    /// <param name="force">Обновить данные не смотря ни на что, по умолчанию false</param>
    void refreshData(Params prms, Boolean force);
    /// <summary>
    /// Обновление данных извне
    /// </summary>
    /// <param name="prms">Вход. параметры</param>
    void refreshData(Params prms);

    /// <summary>
    /// Событие используется, при необходимости оповестить внешние плагины об изменении данных.
    /// </summary>
    event EventHandler<DataChangedEventArgs> DataChanged;

    /// <summary>
    /// Событие используется, при необходимости оповестить внешние плагины перед изменением данных.
    /// </summary>
    event EventHandler<DataChangingCancelEventArgs> DataChanging;

}

  /// <summary>
  /// Дополнительный к базовому <seealso cref="IPlugin"/> интерфейс для плагинов с редактируемыми данными.
  /// </summary>
  public interface IPluginEditable {
    /// <summary>
    /// Признак того, есть ли в плагине несохранённые данные.
    /// </summary>
    Boolean IsDataChanged { get; }

    /// <summary>
    /// Сохранение изменённых данных всего плагина. С колбаком.
    /// </summary>
    Boolean SaveChanges(EventHandler<CJsonStoreAfterPostEventArgs> callback);

    /// <summary>
    /// Отмена изменений данных всего плагина.
    /// </summary>
    void CancelChanges();

  }
}
