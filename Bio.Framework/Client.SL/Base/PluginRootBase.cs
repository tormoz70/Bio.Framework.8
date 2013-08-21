using System;

namespace Bio.Framework.Client.SL {

  public abstract class PluginRootBase : PluginConfigurable<ConfigRoot> {
    protected PluginRootBase(IPlugin owner, String module, String name, String id)
      : base(owner, module, name, id) { }

    #region IPluginRoot Members


    public abstract string ProducerCompany { get; }

    public abstract string AppName { get; }

    public abstract string AppTitle { get; }

    public abstract string AppVersion { get; }

    #endregion
  }
}
