﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Bio.Helpers.Common.Types;
using System.ComponentModel;
using System.Collections;
using PropertyMetadata = System.Windows.PropertyMetadata;

namespace Bio.Framework.Client.SL {

  public class JSTree : ContentControl, IDataControl {

    public JSTree() {
      this.DefaultStyleKey = typeof(JSTree);
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
        this._treeView.SelectedItemChanged -= this._onSelectedItemChanged;
        this._treeView.ContainerExpanded -= this._onContainerExpanded;
        this._treeView.ContainerCollapsed -= this._onContainerCollapsed;
      }
      this._treeView = this.GetTemplateChild("treeView") as CTreeView;
      if (this._treeView != null) {
        this._treeView.SelectedItemChanged += this._onSelectedItemChanged;
        this._treeView.ContainerExpanded += this._onContainerExpanded;
        this._treeView.ContainerCollapsed += this._onContainerCollapsed;
      }
    }

    //public static DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(JSTree), new PropertyMetadata(null));
    //public DataTemplate ItemTemplate {
    //  get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
    //  set { this.SetValue(ItemTemplateProperty, value); }
    //}

    private Boolean _eventSelectedItemChangedEnabled = true;
    public Boolean EventSelectedItemChangedEnabled {
      get { return this._eventSelectedItemChangedEnabled; }
    }

    public void EventSelectedItemChangedEnable() {
      this._eventSelectedItemChangedEnabled = true;
    }
    public void EventSelectedItemChangedDisable() {
      this._eventSelectedItemChangedEnabled = false;
    }

    public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(JSTree), new PropertyMetadata(null));
    public IEnumerable ItemsSource {
      get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
      set { this.SetValue(ItemsSourceProperty, value); }
    }

    public event RoutedPropertyChangedEventHandler<Object> SelectedItemChanged;
    private void _onSelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e) {
      if (this.EventSelectedItemChangedEnabled) {
        var handler = this.SelectedItemChanged;
        if (handler != null)
          handler(sender, e);
      }
    }

    public event RoutedEventHandler ContainerExpanded;
    private void _onContainerExpanded(Object sender, RoutedEventArgs e) {
      var handler = this.ContainerExpanded;
      if (handler != null)
        handler(sender, e);
    }

    public event RoutedEventHandler ContainerCollapsed;
    private void _onContainerCollapsed(Object sender, RoutedEventArgs e) {
      var handler = this.ContainerCollapsed;
      if (handler != null)
        handler(sender, e);
    }

    public event EventHandler<BeforeLoadItemChildrenEventArgs> BeforeLoadItemChildren;
    internal void processBeforeLoadItemChildren(JSTreeItemBase sender, BeforeLoadItemChildrenEventArgs args) {
      var eve = this.BeforeLoadItemChildren;
      if (eve != null) {
        eve(sender, args);
      }
    }

    public void UpdateInnerTreeView() {
      this._treeView.UpdateLayout();
    }

    public Object SelectedItem {
      get {
        return this._treeView.SelectedItem;
      }
    }



    public Boolean SelectItem(Object item) {
      var treeItem = item as JSTreeItemBase;
      if (treeItem != null && treeItem.Container != null) {
        treeItem.Container.IsSelected = true;
        treeItem.Container.IsExpanded = true;
        return true;
      }
      return false;
    }

    public Boolean ExpandItem(Object item) {
      var treeItem = item as JSTreeItemBase;
      if (treeItem != null && treeItem.Container != null) {
        treeItem.Container.IsExpanded = true;
        return true;
      }
      return false;
    }

    public String SelectedPath {
      get {
        String v_rslt = "/ " + this.ItemsSource.Cast<JSTreeItemBase>().FirstOrDefault().Name;
        var v_path = this._treeView.GetSelectedPath();
        foreach (var itm in v_path)
          if ((itm.Key as JSTreeItemBase).Parent != null)
            v_rslt = v_rslt + ((v_rslt.Trim().Length == 1) ? String.Empty : " / ") + (itm.Key as JSTreeItemBase).Name;
        return v_rslt;
      }
    }

    #region IDataControl Members

    public void ClearData() {

      JSTreeItemBase v_rootItem = null;
      foreach (var itm in this.ItemsSource) {
        v_rootItem = itm as JSTreeItemBase;
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
