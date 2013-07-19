using System.Windows;

namespace Bio.Framework.Client.SL {
  public partial class JSGridPropsCols {
    public JSGridPropsCols() {
      InitializeComponent();
    }

    public JSGridPropsCols(JSGridConfig cfg)
      : this() {
      this.lbxList.ItemsSource = cfg.ColumnDefs;
    }

    private void btnMoveUp_Click(object sender, RoutedEventArgs e) {
      var v_cur = this.lbxList.SelectedItem as JSGColumnCfg;
      if (v_cur != null) {
        v_cur.Move2Prev();
        this.lbxList.SelectedItem = v_cur;
      }
    }

    private void btnMoveDown_Click(object sender, RoutedEventArgs e) {
      var v_cur = this.lbxList.SelectedItem as JSGColumnCfg;
      if (v_cur != null) {
        v_cur.Move2Next();
        this.lbxList.SelectedItem = v_cur;
      }
    }

  }

}

