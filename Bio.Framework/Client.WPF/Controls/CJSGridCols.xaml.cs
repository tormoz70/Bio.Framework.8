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
  public partial class CJSGridCols : FloatableWindow {
    public CJSGridCols() {
      InitializeComponent();
    }

    private CJSGCfg _cfg = null;
    public void ShowDialog(CJSGCfg cfg) {
      base.ShowDialog();
      this._cfg = cfg;
      this.lbxList.ItemsSource = this._cfg.Items;
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

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
    }

    private void btnOK_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }



  }

  public class CJSGColumnCfg {
    internal CJSGCfg _owner = null;
    public CJSGColumnCfg() {
    }


    public String FieldName { get; set; }
    public String Header { get; set; }
    public Int32 Index { get; set; }
    public Double Width { get; set; }
    public Boolean IsChecked { get; set; }

    public void MoveTo(Int32 index) {
      for (int i = index; i < this._owner.Items.Count; i++) {
        var itm = this._owner.Items[i];
        if (!itm.Equals(this))
          this._owner.Items[i].Index = i + 1;
      }
      this.Index = index;
      this._owner.Sort();
    }

    public void Move2Prev() { 
      if(this.Index > 0){
        var v_curIndx = this.Index;
        this.Index--;
        this._owner.Items[v_curIndx - 1].Index++;
      }
      this._owner.Sort();
    }
    public void Move2Next() {
      if (this.Index < this._owner.Items.Count - 1) {
        var v_curIndx = this.Index;
        this.Index++;
        this._owner.Items[v_curIndx + 1].Index--;
      }
      this._owner.Sort();
    }
  }
  public class CJSGCfg {
    public SortableObservableCollection<CJSGColumnCfg> Items { get; set; }

    public CJSGCfg() {
      this.Items = new SortableObservableCollection<CJSGColumnCfg>();
    }

    public CJSGColumnCfg Add() {
      var v_item = new CJSGColumnCfg { 
        _owner = this
      };
      this.Items.Add(v_item);
      return v_item;
    }

    public void Sort() {
      this.Items.Sort("Index");
    }

    public void Store(String name) {
      Utl.StoreUserObject(name, this);
    }
    public static CJSGCfg Restore(String name, CJSGCfg defaultCfg) {
      CJSGCfg rslt = Utl.RestoreUserObject<CJSGCfg>(name, defaultCfg);
      foreach (var c in rslt.Items)
        c._owner = rslt;
      return rslt;
    }

  }


}

