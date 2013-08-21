using System;

namespace Bio.Framework.Client.SL {

  public class PluginNotFoundDummy : PluginBase, IPlugin {

    public PluginNotFoundDummy(IPlugin owner, String module, String name, String id)
      : base(owner, module, name, id) {
    }

    public override string ViewTitle {
      get { return ""; }
    }

    private PluginNotFoundDummyView _view;
    public override IPluginView View {
      get {
        if (this._view == null) 
          this._view = new PluginNotFoundDummyView(this);
        return this._view; 
      }
      set {
        this._view = value as PluginNotFoundDummyView;
      }
    }
  }
}
