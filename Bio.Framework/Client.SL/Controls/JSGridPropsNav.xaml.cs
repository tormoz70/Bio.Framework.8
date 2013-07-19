using System.Windows.Controls;

namespace Bio.Framework.Client.SL {
  public partial class JSGridPropsNav {
    public JSGridPropsNav() {
      InitializeComponent();
    }



    public JSGridPropsNav(JSGridConfig cfg)
      : this() {
        this.DataContext = cfg;
    }

  }

}

