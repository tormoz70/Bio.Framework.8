using System;
using System.Windows;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {

  public abstract class PluginBase : IPlugin {
    
    protected PluginBase(IPlugin owner, String module, String name, String id) {
      this.Params = new Params();

      this.Owner = owner;
      this.ModuleName = module;
      this.PluginName = name;
      this.PluginID = id;
    }

    public abstract String ViewTitle { get; }
    public abstract IPluginView View { get; set; }
    public void Show(UIElement container) {
      if (this.View != null) {
        this.View.Show(container);
      }
    }

    public void ShowDialog(Action<Boolean?> callback) {
      if (this.View != null) {
        this.View.ShowDialog(callback);
      }
    }

    public void Close() {
      if (this.View != null) {
        this.View.Close();
      }
    }

    #region IPlugin Members

    public String ModuleName { get; private set; }

    public String PluginName { get; private set; }

    public String PluginID { get; private set; }

    public IPlugin Owner { get; private set; }

    /// <summary>
    /// Параметры плагина
    /// </summary>
    public Params Params { get; private set; }

    public void RefreshData(Params prms, Boolean force) {
      throw new NotImplementedException();
    }

    public void RefreshData(Params prms) {
      throw new NotImplementedException();
    }

    //public event EventHandler<DataChangedEventArgs> DataChanged;

    //public event EventHandler<DataChangingCancelEventArgs> DataChanging;

    private String _genGlobalStoreValueName(String name) {
      return this.GetType().Name + "::" + name;
    }

    /// <summary>
    /// Загружает параметр конфигурации из реестра.
    /// </summary>
    /// <param name="name">Имя параметра.</param>
    /// <param name="defVal">Значение по умолчанию.</param>
    /// <returns>Значение параметра.</returns>
    public virtual T RetrieveValue<T>(String name, T defVal) {
      T rslt = BioEnvironment.Instance.RestoreUserObject(this._genGlobalStoreValueName(name), defVal);
      return rslt; 
    }

    /// <summary>
    /// Сохраняет параметр конфигурации в реестр.
    /// </summary>
    /// <param name="name">Имя параметра.</param>
    /// <param name="value">Значение параметра.</param>
    public virtual void StoreValue(String name, Object value) {
      BioEnvironment.Instance.StoreUserObject(this._genGlobalStoreValueName(name), value);
    }

    #endregion

    #region ISelector Members

    private const String CS_ERR_SELECTOR_REALISATION = "Для реализации функции выбора необходимо, чтобы View реализовывал интерфейс ISelectorView.";

    public virtual void Select(VSelection selection, SelectorCallback callback) {
      var v_sel_view = this.View as ISelectorView;
      if (v_sel_view != null) {
        this.View.ShowSelector(selection, callback);
      } else
        throw new NotImplementedException(CS_ERR_SELECTOR_REALISATION);
      
    }

    public virtual void GetSelection(VSingleSelection selection, SelectorCallback callback) {
      throw new NotImplementedException();
    }

    #endregion

  }
}
