using System;

namespace Bio.Framework.Client.SL {

  public abstract class PluginConfigurable<TConfigType> : PluginBase, IConfigurable<TConfigType> where TConfigType : ConfigRec, new() {

    protected PluginConfigurable(IPlugin owner, String module, String name, String id)
      : base(owner, module, name, id) {
    }

    private TConfigType _config;



    /// <summary>
    /// Конфигурация плагина 
    /// </summary>
    public TConfigType Cfg {
      get {
        if (this._config == null) {
          this._config = ConfigRec.Restore<TConfigType>();
          if (this._config == null)
            this._config = new TConfigType();
        }
        return this._config;
      }
    }


  }
}
