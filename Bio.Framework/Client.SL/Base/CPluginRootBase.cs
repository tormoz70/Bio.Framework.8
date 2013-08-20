using System;

namespace Bio.Framework.Client.SL {

  public abstract class CPluginRootBase : CPluginBase {
    protected CPluginRootBase(IPlugin owner, String module, String name, String id)
      : base(owner, module, name, id) { }


    #region IPluginRoot Members

    //public IConfigRoot Cfg {
    //  get { throw new NotImplementedException(); }
    //}

    protected IConfigRoot FCfgRec = null;
    /// <summary>
    /// Конфигурация плагина
    /// </summary>
    public override IConfigRec Cfg {
      get {
        if (this.FCfgRec == null) {
          this.FCfgRec = new ConfigRoot();
          this.LoadCfg();

        }
        return this.FCfgRec;
      }
    }

    public IConfigRoot CfgRoot {
      get {
        return this.Cfg as IConfigRoot;
      }
    }
    
    #endregion

    #region IPluginRoot Members


    public abstract string ProducerCompany { get; }

    public abstract string AppName { get; }

    public abstract string AppTitle { get; }

    public abstract string AppVersion { get; }

    #endregion
  }
}
