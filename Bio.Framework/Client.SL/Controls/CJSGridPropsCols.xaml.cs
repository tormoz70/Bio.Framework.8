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
using Bio.Framework.Client.SL;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;
using Bio.Helpers.Controls.SL;
using System.Collections;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Bio.Framework.Client.SL {
  public partial class CJSGridPropsCols : UserControl {
    public CJSGridPropsCols() {
      InitializeComponent();
    }

    public CJSGridPropsCols(CJSGridConfig cfg)
      : this() {
      this.lbxList.ItemsSource = cfg.columnDefs;
    }

    private void btnMoveUp_Click(object sender, RoutedEventArgs e) {
      var v_cur = this.lbxList.SelectedItem as CJSGColumnCfg;
      if (v_cur != null) {
        v_cur.Move2Prev();
        this.lbxList.SelectedItem = v_cur;
      }
    }

    private void btnMoveDown_Click(object sender, RoutedEventArgs e) {
      var v_cur = this.lbxList.SelectedItem as CJSGColumnCfg;
      if (v_cur != null) {
        v_cur.Move2Next();
        this.lbxList.SelectedItem = v_cur;
      }
    }

  }

}

