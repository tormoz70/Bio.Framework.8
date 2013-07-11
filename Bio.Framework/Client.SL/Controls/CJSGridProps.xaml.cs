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
  public partial class CJSGridProps : FloatableWindow {
    public CJSGridProps() {
      InitializeComponent();
    }

    private CJSGridConfig _cfg = null;
    public void ShowDialog(CJSGridConfig cfg) {
      base.ShowDialog();
      this._cfg = cfg;
      this.containerCols.Content = new CJSGridPropsCols(this._cfg);
      this.containerCols.UpdateLayout();
      this.containerNav.Content = new CJSGridPropsNav(this._cfg);
      this.containerNav.UpdateLayout();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

  }

  public class CJSGColumnCfg {
    internal CJSGridConfig _owner = null;
    public CJSGColumnCfg() {
    }


    public String FieldName { get; set; }
    public String Header { get; set; }
    public Int32 Index { get; set; }
    public Double Width { get; set; }
    public Boolean IsChecked { get; set; }

    public void MoveTo(Int32 index) {
      for (int i = index; i < this._owner.columnDefs.Count; i++) {
        var itm = this._owner.columnDefs[i];
        if (!itm.Equals(this))
          this._owner.columnDefs[i].Index = i + 1;
      }
      this.Index = index;
      this._owner.sort();
    }

    public void Move2Prev() {
      if (this.Index > 0) {
        var v_curIndx = this.Index;
        this.Index--;
        this._owner.columnDefs[v_curIndx - 1].Index++;
      }
      this._owner.sort();
    }
    public void Move2Next() {
      if (this.Index < this._owner.columnDefs.Count - 1) {
        var v_curIndx = this.Index;
        this.Index++;
        this._owner.columnDefs[v_curIndx + 1].Index--;
      }
      this._owner.sort();
    }
  }

  public class CJSGridConfig : CEditorBase {
    public String uid { get; set; }

    public List<CJSGColumnCfg> columnDefs { get; set; }
    private Int64? _pageSize = null;
    public Int64? pageSize { 
      get {
        return this._pageSize;
      }
      set {
        this._pageSize = value;
      }
    }

    public CJSGridConfig() {
      this.columnDefs = new List<CJSGColumnCfg>();
    }

    public CJSGColumnCfg add() {
      var v_item = new CJSGColumnCfg { 
        _owner = this
      };
      this.columnDefs.Add(v_item);
      return v_item;
    }

    public void sort() {
      if (this.columnDefs.Count > 0) {
        this.columnDefs.Sort((a, b) => {
          if (a.Index == b.Index)
            return 0;
          else if (a.Index > b.Index)
            return 1;
          else
            return -1;
        });
      }
    }

    public void store(String uid) {
      Utl.StoreUserObjectStrg(uid, this);
    }
    public static CJSGridConfig restore(String uid, CJSGridConfig defaultCfg) {
      CJSGridConfig rslt = Utl.RestoreUserObjectStrg<CJSGridConfig>(uid, defaultCfg);
      if (rslt != null) {
        rslt.uid = uid;
        foreach (var c in rslt.columnDefs)
          c._owner = rslt;
      }
      return rslt;
    }

    public void refresh(CJSGrid grid) {
      if ((grid.Columns != null) && (grid.Columns.Count > 0)) {
        //Utl.UiThreadInvoke(new Action<CJSGridConfig>((cfg) => {
        this.columnDefs.Clear();
        foreach (var c in grid.Columns) {
          var v_itm = this.add();
          v_itm.FieldName = ((DataGridBoundColumn)c).Binding.Path.Path;
          v_itm.Header = c.Header as String;
          v_itm.Width = c.Width.Value;
          v_itm.Index = c.DisplayIndex;
          v_itm.IsChecked = c.Visibility == Visibility.Visible;
        }
        this.sort();
        //}), this);
      }
      if (this.pageSize == null)
        this.pageSize = grid._jsClient.PageSize;
    }

  }


}

