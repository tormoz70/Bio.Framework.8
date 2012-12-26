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

namespace Bio.Framework.Client.SL {

  public class CTreeView : TreeView {
    
    protected override bool IsItemItsOwnContainerOverride(object item) {
      return item is CTreeView;
    }

    protected override DependencyObject GetContainerForItemOverride() {
      return new CTreeViewItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, Object item) {
      CTreeViewItem treeViewItemExtended = (CTreeViewItem)element;
      treeViewItemExtended.ParentTreeView = this;
      if (item is CJSTreeItemBase)
        (item as CJSTreeItemBase).Container = (CTreeViewItem)element;
      base.PrepareContainerForItemOverride(element, item);
    }

    public event RoutedEventHandler ContainerExpanded;
    public event RoutedEventHandler ContainerCollapsed;

    //public event EventHandler<BeforeLoadItemChildrenEventArgs> BeforeLoadItemChildren;


    internal void InvokeContainerExpanded(object sender, RoutedEventArgs e) {
      RoutedEventHandler expanded = this.ContainerExpanded;
      if (expanded != null) expanded(sender, e);
    }

    internal void InvokeContainerCollapsed(object sender, RoutedEventArgs e) {
      RoutedEventHandler collapsed = this.ContainerCollapsed;
      if (collapsed != null) collapsed(sender, e);
    }

    //internal void InvokeBeforeLoadItemChildren(object sender, BeforeLoadItemChildrenEventArgs e) {
    //  EventHandler<BeforeLoadItemChildrenEventArgs> eve = this.BeforeLoadItemChildren;
    //  if (eve != null) eve(sender, e);
    //}
  }
  public class CTreeViewItem : TreeViewItem {

    public event EventHandler<BeforeLoadItemChildrenEventArgs> BeforeLoadChildren;

    public CTreeViewItem() {
      this.Expanded += new RoutedEventHandler(CTreeViewItem_Expanded);
      this.Collapsed += new RoutedEventHandler(CTreeViewItem_Collapsed);
      //this.BeforeLoadChildren += new EventHandler<BeforeLoadItemChildrenEventArgs>(CTreeViewItem_BeforeLoadChildren);
    }

    //void CTreeViewItem_BeforeLoadChildren(object sender, BeforeLoadItemChildrenEventArgs e) {
    //  this.ParentTreeView.InvokeBeforeLoadItemChildren(sender, e);
    //}

    void CTreeViewItem_Collapsed(object sender, RoutedEventArgs e) {
      this.ParentTreeView.InvokeContainerCollapsed(sender, e);
    }

    void CTreeViewItem_Expanded(object sender, RoutedEventArgs e) {
      this.ParentTreeView.InvokeContainerExpanded(sender, e);
    }


    protected override bool IsItemItsOwnContainerOverride(object item) {
      return item is CTreeView;
    }

    protected override DependencyObject GetContainerForItemOverride() {
      return new CTreeViewItem();
    }

    public CTreeView ParentTreeView { set; get; }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
      CTreeViewItem treeViewItemExtended = (CTreeViewItem)element;
      treeViewItemExtended.ParentTreeView = this.ParentTreeView;
      if (item is CJSTreeItemBase)
        (item as CJSTreeItemBase).Container = (CTreeViewItem)element;
      base.PrepareContainerForItemOverride(element, item);
    }

  }
}
