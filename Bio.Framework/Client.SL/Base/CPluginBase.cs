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
using System.Reflection;
using System.ComponentModel;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;

namespace Bio.Framework.Client.SL {

  public abstract class CPluginBase {

    public CPluginBase(IPlugin owner, IEnvironment env, String module, String name, String id) {
      this.Params = new Params();

      this.Owner = owner;
      this.Env = env;
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

    private Object GetMemberValue(MemberInfo mi, Type type) {
      Attribute attr = Attribute.GetCustomAttribute(mi, typeof(InvisibleAttribute));
      if (attr != null && ((InvisibleAttribute)attr).Invisible)
        return null;
      attr = Attribute.GetCustomAttribute(mi, typeof(DefaultValueAttribute));
      Object defval = attr != null ? ((DefaultValueAttribute)attr).Value : null;
      Object val = this.RetrieveValue(mi.Name, defval);
      if (val != null) {
        //if (val.GetType().Equals(typeof(System.String)) && !type.Equals(typeof(System.String))) {
        //  TypeConverter tc = TypeDescriptor.GetConverter(type);
        //  if (tc != null && tc.CanConvertFrom(typeof(System.String)))
        //    val = tc.ConvertFromString((String)val);
        //}
        val = Utl.Convert2Type(val, type);
      }
      return val;
    }

    private void SetMemberValue(MemberInfo mi, Object val) {
      Attribute attr = Attribute.GetCustomAttribute(mi, typeof(InvisibleAttribute));
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
      ConfigRec cfg = this.Cfg as ConfigRec;
      FieldInfo[] fia = cfg.GetType().GetFields();
      foreach (FieldInfo fi in fia) {
        try {
          Object val = this.GetMemberValue(fi, fi.FieldType);
          if (val != null)
            fi.SetValue(cfg, val);
        } catch (ArgumentException) {
        }
      }
      PropertyInfo[] pia = cfg.GetType().GetProperties();
      foreach (PropertyInfo pi in pia) {
        try {
          Object val = this.GetMemberValue(pi, pi.PropertyType);
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
      var cfg = this.Cfg as ConfigRec;
      var fia = cfg.GetType().GetFields();
      foreach (FieldInfo fi in fia) {
        var val = fi.GetValue(cfg);
        this.SetMemberValue(fi, val);
      }
      var pia = cfg.GetType().GetProperties();
      foreach (PropertyInfo pi in pia) {
        var val = pi.GetValue(cfg, null);
        this.SetMemberValue(pi, val);
      }
    }

    #region IPlugin Members

    public String ModuleName { get; private set; }

    public String PluginName { get; private set; }

    public String PluginID { get; private set; }

    public IPlugin Owner { get; private set; }

    public IEnvironment Env { get; private set; }

    /// <summary>
    /// Параметры плагина
    /// </summary>
    public Params Params { get; private set; }

    public void refreshData(Params prms, Boolean force) {
      throw new NotImplementedException();
    }

    public void refreshData(Params prms) {
      throw new NotImplementedException();
    }

    public event EventHandler<DataChangedEventArgs> DataChanged;

    public event EventHandler<DataChangingCancelEventArgs> DataChanging;

    private String genGlobalStoreValueName(String name) {
      return this.GetType().Name + "::" + name;
    }

    /// <summary>
    /// Загружает параметр конфигурации из реестра.
    /// </summary>
    /// <param name="name">Имя параметра.</param>
    /// <param name="defVal">Значение по умолчанию.</param>
    /// <returns>Значение параметра.</returns>
    public virtual T RetrieveValue<T>(String name, T defVal) {
      T rslt = this.Env.RestoreUserObject<T>(this.genGlobalStoreValueName(name), defVal);
      return rslt; 
    }

    /// <summary>
    /// Сохраняет параметр конфигурации в реестр.
    /// </summary>
    /// <param name="name">Имя параметра.</param>
    /// <param name="value">Значение параметра.</param>
    public virtual void StoreValue(String name, Object value) {
      this.Env.StoreUserObject(this.genGlobalStoreValueName(name), value);
    }

    #endregion

    #region ISelector Members

    private const String csErrSelectorRealisation = "Для реализации функции выбора необходимо, чтобы View реализовывал интерфейс ISelectorView.";

    public virtual void Select(VSelection selection, SelectorCallback callback) {
      var v_sel_view = this.View as ISelectorView;
      if (v_sel_view != null) {
        this.View.ShowSelector(selection, callback);
      } else
        new NotImplementedException(csErrSelectorRealisation);
      
    }

    public virtual void GetSelection(VSingleSelection selection, SelectorCallback callback) {
      throw new NotImplementedException();
    }

    #endregion

  }
}
