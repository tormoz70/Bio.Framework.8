using System;

namespace Bio.Framework.Client.SL {
  public class PluginRootViewBase : PluginViewBase {
    public PluginRootViewBase() {
    }
    public PluginRootViewBase(IPluginRoot owner)
      : base(owner) {
        this.OnShow += CPluginRootViewBase_OnShow;
    }

    void CPluginRootViewBase_OnShow(object sender, EventArgs e) {
    }

  }
}
