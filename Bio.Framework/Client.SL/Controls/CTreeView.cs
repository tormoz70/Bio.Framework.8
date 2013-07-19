using System;
using System.Windows;
using System.Windows.Controls;

namespace Bio.Framework.Client.SL {

  public class CTreeView : TreeView {
    
    protected override bool IsItemItsOwnContainerOverride(object item) {
      return item is CTreeView;
    }

    protected override DependencyObject GetContainerForItemOverride() {
      return new CTreeViewItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, Object item) {
      var treeViewItemExtended = (CTreeViewItem)element;
      treeViewItemExtended.ParentTreeView = this;
      if (item is JSTreeItemBase)
        (item as JSTreeItemBase).Container = (CTreeViewItem)element;
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

  }
  public class CTreeViewItem : TreeViewItem {


    public CTreeViewItem() {
      this.Expanded += CTreeViewItem_Expanded;
      this.Collapsed += CTreeViewItem_Collapsed;
    }

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
      var treeViewItemExtended = (CTreeViewItem)element;
      treeViewItemExtended.ParentTreeView = this.ParentTreeView;
      if (item is JSTreeItemBase)
        (item as JSTreeItemBase).Container = (CTreeViewItem)element;
      base.PrepareContainerForItemOverride(element, item);
    }

  }
}
