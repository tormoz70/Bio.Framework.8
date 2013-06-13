using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Bio.Framework.Client.SL {
  public class CPluginRootViewBase : CPluginViewBase {
    public CPluginRootViewBase()
      : base() {
    }
    public CPluginRootViewBase(IPluginRoot owner)
      : base(owner) {
        this.OnShow += new EventHandler(CPluginRootViewBase_OnShow);
    }

    void CPluginRootViewBase_OnShow(object sender, EventArgs e) {
    }

  }
}
