using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Bio.Helpers.Controls.SL {
  //public class PopupImplementationTest : Canvas
  //{
  //    public EventHandler Opened { get; set; }

  //    public PopupImplementationTest()
  //    {
  //        this.SetValue(Canvas.ZIndexProperty, 1000000);
  //        IsOpen = false;
  //    }

  //    public bool IsOpen
  //    {
  //        get
  //        {
  //            return this.Visibility == Visibility.Visible;
  //        }
  //        set
  //        {
  //            this.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
  //            if (this.Visibility == Visibility.Visible)
  //                if(Opened != null)
  //                    Opened.BeginInvoke(this, new EventArgs(), null, null);
  //        }
  //    }
  //    //this.Loaded += delegate
  //    //{
  //    //    //if (Application.Current.RootVisual is UserControl)
  //    //    //    (Application.Current.RootVisual as UserControl).Content = (OverlayPopup);
  //    //    //else
  //    //    if (this.GetVisualAncestors().OfType<Panel>().Count() > 0)
  //    //        (this.GetVisualAncestors().OfType<Panel>().Last()).Children.Add(OverlayPopup);
  //    //    else if (this.GetVisualAncestors().OfType<ContentControl>().Count() > 0)
  //    //        (this.GetVisualAncestors().OfType<ContentControl>().Last()).Content = (OverlayPopup);
  //    //    //else
  //    //    //    OverlayPopup = OverlayPopup;
  //    //    OverlayPopup.Children.Add(OverlayCanvas);
  //    //};
  //}

  //public class ThemeChangeCommand : ICommand
  //{
  //    public bool CanExecute(object parameter)
  //    {
  //        return true;
  //    }

  //    public event EventHandler CanExecuteChanged;

  //    public void Execute(object parameter)
  //    {
  //        Theme themeContainer =
  //              (Theme)((FrameworkElement)Application.Current.RootVisual).FindName("ThemeContainer");

  //        string themeName = parameter as string;


  //        if (themeName == null)
  //        {
  //            themeContainer.ThemeUri = null;
  //        }
  //        else
  //        {
  //            themeContainer.ThemeUri =
  //                 new Uri("/System.Windows.Controls.Theming." + themeName +
  //                 ";component/Theme.xaml", UriKind.RelativeOrAbsolute);
  //        }

  //        if (CanExecuteChanged != null)
  //            CanExecuteChanged(this, new EventArgs());
  //    }
  //}

  public partial class PopupMenu {

    #region AddItem

    public PopupMenuItem AddItem(FrameworkElement item) {
      return InsertItem(-1, null, item, null, null, null);
    }

    public PopupMenuItem AddItem(string header, RoutedEventHandler clickHandler) {
      return InsertItem(-1, null, header, null, null, null, clickHandler);
    }

    public PopupMenuItem AddItem(FrameworkElement item, RoutedEventHandler clickHandler) {
      return InsertItem(-1, item, clickHandler);
    }

    public PopupMenuItem AddItem(bool showLeftMargin, FrameworkElement item, RoutedEventHandler clickHandler) {
      return InsertItem(-1, showLeftMargin, item, clickHandler);
    }

    public PopupMenuItem AddItem(string iconUrl, FrameworkElement item, RoutedEventHandler clickHandler) {
      return InsertItem(-1, iconUrl, item, null, null, clickHandler);
    }

    public PopupMenuItem AddItem(string iconUrl, string header, string tag, RoutedEventHandler clickHandler) {
      return InsertItem(-1, iconUrl, new TextBlock() { Text = header, Tag = tag }, null, null, clickHandler);
    }

    public PopupMenuItem AddItem(string leftIconUrl, string header, string rightIconUrl, string tag, RoutedEventHandler clickHandler) {
      return InsertItem(-1, leftIconUrl, new TextBlock() { Text = header, Tag = tag }, rightIconUrl, null, clickHandler);
    }

    public PopupMenuItem AddItem(string leftIconUrl, string header, string rightIconUrl, string tag, string name, RoutedEventHandler clickHandler) {
      return InsertItem(-1, leftIconUrl, new TextBlock() { Text = header, Tag = tag }, rightIconUrl, name, clickHandler);
    }

    #endregion AddItem

    #region InsertItem

    public PopupMenuItem InsertItem(int index, FrameworkElement item) {
      return InsertItem(index, item, null);
    }

    public PopupMenuItem InsertItem(int index, string header, RoutedEventHandler leftClickHandler) {
      return InsertItem(index, null, new TextBlock() { Text = header }, null, null, leftClickHandler);
    }

    public PopupMenuItem InsertItem(int index, FrameworkElement item, RoutedEventHandler leftClickHandler) {
      return InsertItem(index, null, item, null, null, leftClickHandler);
    }

    public PopupMenuItem InsertItem(int index, bool showLeftMargin, FrameworkElement item, RoutedEventHandler leftClickHandler) {
      PopupMenuItem pmi = InsertItem(index, null, item, null, null, leftClickHandler);
      pmi.ShowLeftMargin = showLeftMargin;
      return pmi;
    }

    public PopupMenuItem InsertItem(int index, string leftIconUrl, FrameworkElement item, string tag, RoutedEventHandler leftClickHandler) {
      return InsertItem(index, leftIconUrl, item, null, null, leftClickHandler);
    }

    public PopupMenuItem InsertItem(int index, string leftIconUrl, string header, string rightIconUrl, string tag, RoutedEventHandler leftClickHandler) {
      return InsertItem(index, leftIconUrl, new TextBlock() { Text = header, Tag = tag }, rightIconUrl, null, leftClickHandler);
    }

    public PopupMenuItem InsertItem(int index, string leftIconUrl, string header, string rightIconUrl, string tag, string name, RoutedEventHandler leftClickHandler) {
      return InsertItem(index, leftIconUrl, new TextBlock() { Text = header, Tag = tag }, rightIconUrl, name, leftClickHandler);
    }

    #endregion InsertItem

    public PopupMenuItem AddSubMenu(PopupMenu subMenu, string header, string rightIconUrl, string tag, string name, bool closeOnClick, RoutedEventHandler clickHandler) {
      return InsertSubMenu(-1, subMenu, header, rightIconUrl, tag, name, closeOnClick, clickHandler);
    }

    public PopupMenuItem InsertSubMenu(int index, PopupMenu subMenu, string header, string rightIconUrl, string tag, string name, bool closeOnClick, RoutedEventHandler clickHandler) {
      PopupMenuItem pmi = InsertItem(index, null, new TextBlock() { Text = header, Tag = tag }, rightIconUrl, name, null);
      pmi.CloseOnClick = closeOnClick;
      subMenu.Orientation = MenuOrientationTypes.Right;
      subMenu.AddTrigger(TriggerTypes.Hover, pmi);
      return pmi;
    }

    public PopupMenuItem InsertItem(int index, string leftIconUrl, FrameworkElement item, string rightIconUrl, string name, RoutedEventHandler clickHandler) {
      if (item.Parent != null)
        (item.Parent as Panel).Children.Remove(item);

      PopupMenuItem popupMenuItem = item is PopupMenuItem ? item as PopupMenuItem : new PopupMenuItem(leftIconUrl, item);

      if (clickHandler != null)
        popupMenuItem.Click += clickHandler;

      if (rightIconUrl != null)
        popupMenuItem.ImagePathForRightMargin = rightIconUrl;

      if (name != null) {
        if (ItemsControl.Items.OfType<FrameworkElement>().Where(i => i.Name == name).Count() == 0)
          popupMenuItem.Name = name;
        else
          throw new ArgumentException("An item named " + name + " already exists in the PopupMenu " + this.Name);
      }

      ItemsControl.Items.Insert(index == -1 ? ItemsControl.Items.Count : index, popupMenuItem);

      return popupMenuItem;
    }

    public void RemoveAt(int index) {
      ItemsControl.Items.RemoveAt(index);
    }

    public void Remove(ContentControl item) {
      ItemsControl.Items.Remove(item);
    }

    public PopupMenuSeparator AddSeparator() {
      return this.AddSeparator(null);
    }

    public PopupMenuSeparator AddSeparator(string tag) {
      var v_rslt = new PopupMenuSeparator { Tag = tag };
      this.ItemsControl.Items.Add(v_rslt);
      return v_rslt;
    }

    public void InsertSeparator(int index, string tag) {
      ItemsControl.Items.Insert(index, new PopupMenuSeparator { Tag = tag });
    }

    public ContentControl GetContentControlItem(int index) {
      return (ContentControl)(ItemsControl.ItemContainerGenerator.ContainerFromItem(ItemsControl.Items[index]));
    }

    public PopupMenuItem PopupMenuItem(string name) {
      return GetItem<PopupMenuItem>(name);
    }

    public PopupMenuItem PopupMenuItem(int index) {
      return GetItem<PopupMenuItem>(index);
    }

    /// <summary>
    /// Get the object of type T in the menu control by index position.
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <param name="index">The object  index</param>
    public T GetItem<T>(int index) where T : class {
      T item = (ItemsControl.Items[index] as FrameworkElement).GetVisualDescendantsAndSelf()
        .Where(i => i is T)
        .Select(i => i as T).FirstOrDefault();

      if (item == default(T))
        throw new Exception(string.Format("{0} at item {1} is not of type {2}", ItemsControl.Items[index].GetType(), index, typeof(T).ToString()));
      else
        return item;
    }

    public T GetChildItem<T>(UIElement element) {
      foreach (object item in (element as UIElement).GetVisualChildren()) {
        if (item != null && item is T)
          return (T)item;
      }
      return default(T);
    }

    /// <summary>
    /// Get the last clicked element associated with any of the PopupMenu control trigger events.
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    public T GetClickedElement<T>() {
      return GetElement<T>(VisualTreeElementsUnderMouse);
    }

    private static T GetElement<T>(IEnumerable<UIElement> elements) {
      return elements == null ? default(T) : elements.OfType<T>().FirstOrDefault();
    }

    private static T GetElement<T>(IEnumerable<UIElement> elements, int index) {
      foreach (object elem in elements.OfType<T>())
        if (index-- <= 0)
          return (T)elem;
      return default(T);
    }

    /// <summary>
    /// Find a ContentControl having elements with a specific tag value.
    /// This method only works after the visual tree has been created.
    /// </summary>
    public ContentControl FindItemContainerByTag(object tag) {
      return FindItemContainersByTag(tag).FirstOrDefault();
    }

    /// <summary>
    /// Find a list of ContentControls having elements with a specific tag value.
    /// This method only works after the visual tree has been created.
    /// </summary>
    public List<ContentControl> FindItemContainersByTag(object tag) {
      return FindItemsByTag<FrameworkElement>(tag)
        .Select(e => PopupMenuUtils.GetContainer<ContentControl>(e)).ToList();
    }

    public T FindItemByTag<T>(object tag) where T : class {
      return FindItemsByTag<T>(tag).FirstOrDefault();
    }

    /// <summary>
    /// Find a list of ContentControls having elements with any of the tags specified.
    /// This method only works after the visual tree has been created.
    /// </summary>
    /// <param name="tags">A comma delimited list of tags that will be used as identifier.</param>
    public List<T> FindItemsByTag<T>(params object[] tags) where T : class {
      List<T> elements = new List<T>();
      foreach (object tag in tags) {
        //foreach (FrameworkElement item in Items)
        //    foreach (T element in item.GetVisualChildrenAndSelf().OfType<T>())
        //        if ((element as FrameworkElement).Tag != null && (element as FrameworkElement).Tag.Equals(tag))
        //            elements.Add(element as T);
        // If no element was found search all the visual tree instead(only works after the latter has been created)
        //if (elements.Count == 0)
        elements = ItemsControl.GetVisualDescendantsAndSelf().OfType<T>()
          .Where(i => i is FrameworkElement
            && (i as FrameworkElement).Tag != null
            && (i as FrameworkElement).Tag.Equals(tag))
          .Select(i => i as T).ToList();
      }
      return elements;
    }

    /// <summary>
    /// Find a list of ContentControls having elements with tags matching a regex pattern
    /// This method only works after the visual tree has been created.
    /// </summary>
    /// <param name="regexPattern">The regex pattern to match the element name</param>
    public List<T> FindItemsByTag<T>(string regexPattern) where T : class {
      List<T> elements = new List<T>();
      elements = ItemsControl.GetVisualDescendantsAndSelf().OfType<T>()
        .Where(i => (new Regex(regexPattern).IsMatch((i as FrameworkElement).Tag.ToString() ?? "")))
        .Select(i => i as T).ToList();

      return elements;
    }

    /// <summary>
    /// Find the ContentControl containing a control with a name matching a regex pattern.
    /// This method only works after the visual tree has been created.
    /// </summary>
    /// <param name="regexPattern">The regex pattern to match the element name</param>
    public ContentControl FindItemContainerByName(string regexPattern) {
      return FindItemsByName<FrameworkElement>(regexPattern).Select(e => PopupMenuUtils.GetContainer<ContentControl>(e)).FirstOrDefault();
    }

    /// <summary>
    /// Find the ContentControls containing controls with names matching a regex pattern.
    /// This method only works after the visual tree has been created.
    /// </summary>
    /// <param name="regexPattern">The regex pattern to match the element names</param>
    public List<ContentControl> FindItemContainersByName(string regexPattern) {
      return FindItemsByName<FrameworkElement>(regexPattern).Select(e => PopupMenuUtils.GetContainer<ContentControl>(e)).ToList();
    }

    public T FindItemByName<T>(string regexPattern) where T : class {
      return FindItemsByName<T>(regexPattern).FirstOrDefault();
    }

    public List<T> FindItemsByName<T>(string regexPattern) where T : class {
      List<T> elements = new List<T>();
      //foreach (FrameworkElement item in Items)
      //    foreach (T element in item.GetVisualChildrenAndSelf().OfType<T>())
      //        if ((new Regex(regexPattern).IsMatch((element as FrameworkElement).Name ?? "")))
      //            elements.Add(element as T);

      //// If no element was found search all the visual tree instead(only works after the latter has been created)
      //if (elements.Count == 0)
      elements = ItemsControl.GetVisualDescendantsAndSelf().OfType<T>()
        .Where(i => (new Regex(regexPattern).IsMatch((i as FrameworkElement).Name ?? "")))
        .Select(i => i as T).ToList();

      return elements;
    }

    public T GetItem<T>(string name) {
      return Items.GetVisualDescendantsAndSelf()
        .Where(i => i is T && (i as FrameworkElement).Name == name)
        .Select(i => (T)(i as object)).First();
    }

    public void SetVisibilityByTag(string tag, bool visible) {
      foreach (ContentControl item in FindItemContainersByTag(tag))
        if (item != null)
          item.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

  }
}
