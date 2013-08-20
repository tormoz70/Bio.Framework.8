using System;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {

  public abstract class CPluginBase {
    
    protected CPluginBase(IPlugin owner, String module, String name, String id) {
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

    private Object _getMemberValue(MemberInfo mi, Type type) {
      var attr = Attribute.GetCustomAttribute(mi, typeof(InvisibleAttribute));
      if (attr != null && ((InvisibleAttribute)attr).Invisible)
        return null;
      attr = Attribute.GetCustomAttribute(mi, typeof(DefaultValueAttribute));
      var defval = attr != null ? ((DefaultValueAttribute)attr).Value : null;
      var val = this.RetrieveValue(mi.Name, defval);
      if (val != null) 
        val = Utl.Convert2Type(val, type);
      return val;
    }

    private void _setMemberValue(MemberInfo mi, Object val) {
      var attr = Attribute.GetCustomAttribute(mi, typeof(InvisibleAttribute));
      if (attr != null && ((InvisibleAttribute)attr).Invisible)
        return;
      this.StoreValue(mi.Name, val);
    }

    /// <summary>
    /// Конфигурация плагина
    /// </summary>
    public virtual IConfigRec Cfg {
      get {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Загружает все параметры конфигурации.
    /// </summary>
    public virtual void LoadCfg() {
      var cfg = (ConfigRec)this.Cfg;
      var fia = cfg.GetType().GetFields();
      foreach (var fi in fia) {
        try {
          var val = this._getMemberValue(fi, fi.FieldType);
          if (val != null)
            fi.SetValue(cfg, val);
        } catch (ArgumentException) {
        }
      }
      var pia = cfg.GetType().GetProperties();
      foreach (var pi in pia) {
        try {
          var val = this._getMemberValue(pi, pi.PropertyType);
          if (val != null)
            pi.SetValue(cfg, val, null);
        } catch (ArgumentException) {
        }
      }
    }

    /// <summary>
    /// Сохраняет все параметры конфигурации.
    /// </summary>
    public virtual void SaveCfg() {
      var cfg = (ConfigRec)this.Cfg;
      var fia = cfg.GetType().GetFields();
      foreach (var fi in fia) {
        var val = fi.GetValue(cfg);
        this._setMemberValue(fi, val);
      }
      var pia = cfg.GetType().GetProperties();
      foreach (var pi in pia) {
        var val = pi.GetValue(cfg, null);
        this._setMemberValue(pi, val);
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
