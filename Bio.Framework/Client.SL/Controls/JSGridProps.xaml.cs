using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Bio.Helpers.Common;

namespace Bio.Framework.Client.SL {
  public partial class JSGridProps {
    public JSGridProps() {
      InitializeComponent();
    }

    private JSGridConfig _cfg;
    public void ShowDialog(JSGridConfig cfg) {
      base.ShowDialog();
      this._cfg = cfg;
      this.containerCols.Content = new JSGridPropsCols(this._cfg);
      this.containerCols.UpdateLayout();
      this.containerNav.Content = new JSGridPropsNav(this._cfg);
      this.containerNav.UpdateLayout();
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

  }

  public class JSGColumnCfg {
    internal JSGridConfig owner = null;
    public JSGColumnCfg() {
    }


    public String FieldName { get; set; }
    public String Header { get; set; }
    public Int32 Index { get; set; }
    public Double Width { get; set; }
    public Boolean IsChecked { get; set; }

    public void MoveTo(Int32 index) {
      for (int i = index; i < this.owner.ColumnDefs.Count; i++) {
        var itm = this.owner.ColumnDefs[i];
        if (!itm.Equals(this))
          this.owner.ColumnDefs[i].Index = i + 1;
      }
      this.Index = index;
      this.owner.Sort();
    }

    public void Move2Prev() {
      if (this.Index > 0) {
        var curIndx = this.Index;
        this.Index--;
        this.owner.ColumnDefs[curIndx - 1].Index++;
      }
      this.owner.Sort();
    }
    public void Move2Next() {
      if (this.Index < this.owner.ColumnDefs.Count - 1) {
        var curIndx = this.Index;
        this.Index++;
        this.owner.ColumnDefs[curIndx + 1].Index--;
      }
      this.owner.Sort();
    }
  }

  public class JSGridConfig : EditorBase {
    public String UID { get; set; }

    public List<JSGColumnCfg> ColumnDefs { get; set; }
    public long? PageSize { get; set; }

    public JSGridConfig() {
      this.ColumnDefs = new List<JSGColumnCfg>();
    }

    public JSGColumnCfg Add() {
      var v_item = new JSGColumnCfg { 
        owner = this
      };
      this.ColumnDefs.Add(v_item);
      return v_item;
    }

    public void Sort() {
      if (this.ColumnDefs.Count > 0) {
        this.ColumnDefs.Sort((a, b) => {
          if (a.Index == b.Index)
            return 0;
          if (a.Index > b.Index)
            return 1;
          return -1;
        });
      }
    }

    public void Store() {
      Utl.StoreUserObjectStrg(this.UID, this);
    }
    public static JSGridConfig Restore(String uid, JSGridConfig defaultCfg) {
      var rslt = Utl.RestoreUserObjectStrg<JSGridConfig>(uid, defaultCfg);
      if (rslt != null) {
        rslt.UID = uid;
        foreach (var c in rslt.ColumnDefs)
          c.owner = rslt;
      }
      return rslt;
    }

    public void Refresh(JSGrid grid) {
      if ((grid.Columns != null) && (grid.Columns.Count > 0)) {
        //Utl.UiThreadInvoke(new Action<JSGridConfig>((cfg) => {
        this.ColumnDefs.Clear();
        foreach (var c in grid.Columns) {
          var v_itm = this.Add();
          v_itm.FieldName = ((DataGridBoundColumn)c).Binding.Path.Path;
          v_itm.Header = c.Header as String;
          v_itm.Width = c.Width.Value;
          v_itm.Index = c.DisplayIndex;
          v_itm.IsChecked = c.Visibility == Visibility.Visible;
        }
        this.Sort();
        //}), this);
      }
      if (this.PageSize == null)
        this.PageSize = grid.PageSize;
    }

  }


}

