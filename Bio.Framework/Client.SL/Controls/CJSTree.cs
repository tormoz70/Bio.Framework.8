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
using Bio.Helpers.Common.Types;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Globalization;
using Bio.Helpers.Common;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Bio.Framework.Packets;
using System.Collections;

namespace Bio.Framework.Client.SL {

  //public delegate void CJSTreeLoadChildrenDelegate(CJSTreeItemBase item);
  public class CJSTree : ContentControl, IDataControl {
    //private CJsonStoreClient _jsClient = null;


    public CJSTree() {
      this.DefaultStyleKey = typeof(CJSTree);
    }

    private CTreeView _treeView;
    public CTreeView TreeView {
      get {
        return this._treeView;
      }
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      if (this._treeView != null) {
        this._treeView.SelectedItemChanged -= this.onSelectedItemChanged;
        this._treeView.ContainerExpanded -= this.onContainerExpanded;
        this._treeView.ContainerCollapsed -= this.onContainerCollapsed;
      }
      this._treeView = this.GetTemplateChild("treeView") as CTreeView;
      if (this._treeView != null) {
        this._treeView.SelectedItemChanged += this.onSelectedItemChanged;
        this._treeView.ContainerExpanded += this.onContainerExpanded;
        this._treeView.ContainerCollapsed += this.onContainerCollapsed;
      }
    }

    //public static DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CJSTree), new PropertyMetadata(null));
    //public DataTemplate ItemTemplate {
    //  get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
    //  set { this.SetValue(ItemTemplateProperty, value); }
    //}

    public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CJSTree), new PropertyMetadata(null));
    public IEnumerable ItemsSource {
      get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
      set { this.SetValue(ItemsSourceProperty, value); }
    }

    public event RoutedPropertyChangedEventHandler<Object> SelectedItemChanged;
    private void onSelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e) {
      RoutedPropertyChangedEventHandler<Object> handler = this.SelectedItemChanged;
      if (handler != null) {
        handler(sender, e);
      }
    }

    public event RoutedEventHandler ContainerExpanded;
    private void onContainerExpanded(Object sender, RoutedEventArgs e) {
      RoutedEventHandler handler = this.ContainerExpanded;
      if (handler != null) {
        handler(sender, e);
      }
    }
    public event RoutedEventHandler ContainerCollapsed;
    private void onContainerCollapsed(Object sender, RoutedEventArgs e) {
      RoutedEventHandler handler = this.ContainerCollapsed;
      if (handler != null) {
        handler(sender, e);
      }
    }

    public event EventHandler<BeforeLoadItemChildrenEventArgs> BeforeLoadItemChildren;
    internal void processBeforeLoadItemChildren(CJSTreeItemBase sender, BeforeLoadItemChildrenEventArgs args) {
      EventHandler<BeforeLoadItemChildrenEventArgs> eve = this.BeforeLoadItemChildren;
      if (eve != null) {
        eve(sender, args);
      }
    }



    public Object SelectedItem {
      get {
        return this._treeView.SelectedItem;
      }
    }

    public Boolean SelectItem(Object item) { 
      return this._treeView.SelectItem(item);
    }

    public String SelectedPath {
      get {
        String v_rslt = "/ " + this.ItemsSource.Cast<CJSTreeItemBase>().FirstOrDefault().Name;
        var v_path = this._treeView.GetSelectedPath();
        foreach (var itm in v_path)
          if ((itm.Key as CJSTreeItemBase).Parent != null)
            v_rslt = v_rslt + ((v_rslt.Trim().Length == 1) ? String.Empty : " / ") + (itm.Key as CJSTreeItemBase).Name;
        return v_rslt;
      }
    }

    #region IDataControl Members

    public void ClearData() {

      CJSTreeItemBase v_rootItem = null;
      foreach (var itm in this.ItemsSource) {
        v_rootItem = itm as CJSTreeItemBase;
        break;
      }
      if (v_rootItem != null) {
          v_rootItem.Items.Clear();
      }
    }

    #endregion
  }


  public class BeforeLoadItemChildrenEventArgs : CancelEventArgs {
    public Params Params { get; set; }
  }


}
