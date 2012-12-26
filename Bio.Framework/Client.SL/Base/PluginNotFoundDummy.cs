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

namespace Bio.Framework.Client.SL {

  public class PluginNotFoundDummy : CPluginBase, IPlugin {

    public PluginNotFoundDummy(IPlugin owner, IEnvironment env, String module, String name, String id)
      : base(owner, env, module, name, id) {
    }

    public override string ViewTitle {
      get { return ""; }
    }

    private PluginNotFoundDummyView _view = null;
    public override IPluginView View {
      get {
        if (this._view == null) {
          this._view = new PluginNotFoundDummyView(this);
          //this._view.ownerPlugin = this;
        }
        return this._view; 
      }
      set {
        this._view = value as PluginNotFoundDummyView;
      }
    }
  }
}
