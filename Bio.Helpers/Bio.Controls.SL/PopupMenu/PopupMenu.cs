// Copyright (c) 2009 Ziad Jeeroburkhan. All Rights Reserved.
// GNU General Public License version 2 (GPLv2)
// (http://sl4popupmenu.codeplex.com/license)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Reflection;

namespace Bio.Helpers.Controls.SL {
  public enum TriggerTypes { LeftClick, RightClick, Hover }

  public enum MenuOrientationTypes { Left, Top, Right, Bottom, Mouse }

  //[StyleTypedProperty(Property = "Style", StyleTargetType = typeof(PopupMenu))]
  public partial class PopupMenu : ContentControl {
    #region Properties

    public Key AccessKey { get; set; }

    public ModifierKeys AccessKeyModifier1 { get; set; }

    public ModifierKeys AccessKeyModifier2 { get; set; }

    public string AccessKeyElementName { get; set; }

    public FrameworkElement AccessKeyElement { get; set; }

    public KeyEventHandler AccesskeyPressed;

    public bool OpenOnAccessKeyPressed { get; set; }

    private bool _restoreFocusOnClose;

    private Key _submenuLaunchKey;
    /// <summary>
    /// The key used to open the submenu in the actual menu, if any, when the ItemsControl
    /// has focus. The default value used is the the cursor key matching the menu Orientation
    /// but it can be disabled by setting its value to Key.None.
    /// </summary>
    public Key SubmenuLaunchKey {
      get {
        return _submenuLaunchKey == Key.None ? (Key)((int)Key.Left + (int)Orientation) : _submenuLaunchKey;
      }
      set {
        _submenuLaunchKey = value;
      }
    }

    public PopupMenuItem ParentPopupMenuItem;

    public Popup OverlayPopup;

    public Canvas OverlayCanvas;


    public string HoverElements { get; set; }

    public string LeftClickElements { get; set; }

    public string RightClickElements { get; set; }

    /// <summary>
    /// All the elements in the visual tree hierarchy where the mouse was clicked or hovered.
    /// </summary>
    public IEnumerable<UIElement> VisualTreeElementsUnderMouse { get; set; }

    /// <summary>
    /// The type of the trigger element that fired up the menu
    /// </summary>
    public TriggerTypes ActualTriggerType;

    /// <summary>
    /// The trigger element that fired up the menu.
    /// </summary>
    public FrameworkElement ActualTriggerElement { get; set; }


    public bool VisualTreeGenerated { get; set; }

    /// <summary>
    /// This storyboard can be used to override the default fade in animation(applied to ContentRoot) when opening the menu.
    /// </summary>
    public Storyboard OpenAnimation { get; set; }

    public int OpenDelay { get; set; }

    public int OpenDuration { get; set; }

    public bool IsOpening { get; set; }

    /// <summary>
    /// This storyboard can be used to override the default fade out animation(applied to ContentRoot) when closing the menu.
    /// </summary>
    public Storyboard CloseAnimation { get; set; }

    public int CloseDelay { get; set; }

    public int CloseDuration { get; set; }

    public bool IsClosing { get; set; }

    private Timer _timerOpen;

    private Timer _timerClose;

    private bool _clickAlreadyHandledInMouseDownEvent;

    public bool FocusOnShow { get; set; }

    private bool CloseOnHoverOut { get; set; }

    protected Grid ParentOverlapGrid;

    public Brush ParentOverlapGridBrush { get; set; }

    public Thickness? ParentOverlapGridThickness { get; set; }


    public bool _allowPinnedState;

    private bool _isPinned;

    /// <summary>
    /// When true left click menus can only be closed by clicking back on their respective trigger element.
    /// For hover menus however a click on the hover element will toggle their 'pinned' state instead.
    ///
    /// Note that when _allowPinnedState is false for hover menu IsPinned will always return false.
    ///
    /// For pinned menus OverlayCanvas is collapsed thus allowing surrounding elements to respond
    /// to the mouse while the menu is open.
    /// </summary>
    public bool IsPinned {
      get {
        return _isPinned && (_allowPinnedState || ActualTriggerType != TriggerTypes.Hover);
      }
      set {
        _isPinned = value;
        if (OverlayCanvas != null)
          ExpandOverlayCanvas(!value);
      }
    }

    /// <summary>
    /// Stretch OverlayCanvas all over the application or collpase it around the Popup content
    /// </summary>
    /// <param name="value">When false OverlayCanvas is collapsed such that it no longer captures mouse clicks</param>
    private void ExpandOverlayCanvas(bool value) {
      OverlayCanvas.Width = value ? Application.Current.Host.Content.ActualWidth : double.NaN;
      OverlayCanvas.Height = value ? Application.Current.Host.Content.ActualHeight : double.NaN;
    }

    private bool _isModal;

    public bool IsModal {
      get { return _isModal; }
      set {
        _isModal = value;
        OverlayCanvas.Background = _isModal ? (ModalBackground ?? new SolidColorBrush(Color.FromArgb(100, 100, 100, 100))) : new SolidColorBrush(Colors.Transparent);
      }
    }

    public Brush ModalBackground { get; set; }

    /// <summary>
    /// This event is called when the menu is being opened.
    /// It is actually the only event where submenus can be safely added.
    /// </summary>
    public event RoutedEventHandler Opening;
    /// <summary>
    /// This event is called when the menu is open but is still at the initial phase of its storyboard.
    /// Addtion of submenu items is not supported here since references to those elements may have been
    /// broken by the layouting process.
    /// </summary>
    public event RoutedEventHandler Showing;
    /// <summary>
    /// This event is called when the storyboard animation used to display the menu has completed.
    /// </summary>
    public event RoutedEventHandler Shown;
    /// <summary>
    /// This event is called when the menu is closing.
    /// </summary>
    public event RoutedEventHandler Closing;

    public double OffsetX { get; set; }

    public double OffsetY { get; set; }

    public MenuOrientationTypes Orientation { get; set; }

    public bool PreserveItemContainerStyle { get; set; }

    public Effect ItemsControlEffect {
      set {
        this.Dispatcher.BeginInvoke(delegate {
          if (ItemsControl != null) {
            ItemsControl.Dispatcher.BeginInvoke(delegate {
              ItemsControl.Effect = value;
            });
          }
        });
      }
    }

    public bool EnableItemsShadowEffect { get; set; }

    /// <summary>
    /// Adds an effect to all items in the menu.
    /// </summary>
    public Effect ItemsEffect {
      set {
        this.Dispatcher.BeginInvoke(delegate {
          ItemsControl.Dispatcher.BeginInvoke(delegate {
            foreach (FrameworkElement item in ItemsControl.Items)
              foreach (FrameworkElement itemChild in item.GetVisualDescendantsAndSelf())
                if (value != null && itemChild.Effect == null && !(itemChild is Panel))
                  itemChild.Effect = value;
          });
        });
      }
    }

    bool _autoSelectItem = true;

    /// <summary>
    /// Automatically select ListBoxItem when hovered or clicked
    /// </summary>
    public bool AutoSelectItem {
      get { return _autoSelectItem; }
      set { _autoSelectItem = value; }
    }

    /// <summary>
    /// Set ActualTriggerElement as the selected items of the trigger element if the latter derives from the Selector class(e.g ListBoxes, ComboBoxes and DataGrids)
    /// </summary>
    public bool AutoMapTriggerElementToSelectableItem { get; set; }

    /// <summary>
    /// Switch to the parent for hover bounds info if the element under the mouse is a Textblock to avoid dealing with widths
    /// that can change with the text length.
    /// </summary>
    public bool AutoMapHoverBoundsToParent { get; set; }

    /// <summary>
    /// The content of the PopupMenu.
    /// Note that all content is moved into OverlayCanvas at runtime.
    /// </summary>
    public FrameworkElement ContentRoot {
      get {
        MoveContentToOverlayCanvasIfAny(this.Content);
        return (OverlayCanvas.Children[0]) as FrameworkElement;
      }
      set {
        MoveContentToOverlayCanvasIfAny(value);
      }
    }

    /// <summary>
    /// Gets or sets a reference to the ItemsControl(a ListBox typically) used to accomodate the menu items.
    /// </summary>
    public ItemsControl ItemsControl {
      get // Get the first child of ContentRoot
      {
        return ContentRoot.GetVisualDescendantsAndSelf().OfType<ItemsControl>().FirstOrDefault();
      }
      set // Make the ItemControl a child of ContentRoot
      {
        MoveContentToOverlayCanvasIfAny(value);
      }
    }

    private void MoveContentToOverlayCanvasIfAny(object content) {
      if (!DesignerProperties.IsInDesignTool && content != null && content is FrameworkElement)
        PopupMenuUtils.RelocateElement(OverlayCanvas, content as FrameworkElement, EnableItemsShadowEffect);
    }

    /// <summary>
    /// A readonly collection of items in the ItemsControl used by the menu.
    /// To modify the collection use the ItemsControl.Items property instead.
    /// To add menu or submenus items use the AddItem or AddSubMenu functions.
    /// </summary>
    public ItemCollection Items {
      get { return ItemsControl.Items; }
    }

    #endregion Properties

    #region Constructors

    public PopupMenu()
      : this(0, 0) { }

    public PopupMenu(double offsetX, double offsetY)
      : this(null, offsetX, offsetY) { }

    public PopupMenu(ItemsControl itemsControl)
      : this(itemsControl, 0, 0) { }


    #endregion Constructors

    public PopupMenu(ItemsControl itemsControl, double offsetX, double offsetY) {
      //this.DefaultStyleKey = typeof(PopupMenu);
      //this.ApplyTemplate();

      // Default values
      Orientation = MenuOrientationTypes.Bottom;
      AutoMapTriggerElementToSelectableItem = true;
      AutoMapHoverBoundsToParent = true;
      EnableItemsShadowEffect = true;
      CloseOnHoverOut = true;
      OpenDelay = 200;
      CloseDelay = 300;
      OpenDuration = 150;
      CloseDuration = 110;
      OffsetX = offsetX;
      OffsetY = offsetY;
      // Closes the menu after a period of time when mouse moves outside the menu

      //// To be able to capture mouse activity the canvas is stretched across the window when calling the Open method
      //// Note that the ability for the canvas to auto stretch is only enabled when its Background property contains a value(so be it as weird as it seems)
      OverlayCanvas = new Canvas { Background = new SolidColorBrush(Colors.Transparent) };// Color.FromArgb(30, 255, 0, 0));
      // Put OverlayCanvas on top of all other elements in the application using OverlayPopup
      OverlayPopup = new Popup { Child = OverlayCanvas };

      // The setter also moves the ItemsControl to OverlayCanvas
      ItemsControl = itemsControl ?? new ListBox { Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)) };

      if (!DesignerProperties.IsInDesignTool) {
        this.Visibility = Visibility.Collapsed;
        this.LayoutUpdated += PopupMenu_LayoutUpdated;
      }
    }

    /// <summary>
    /// Called when the template's tree is generated.
    /// </summary>
    //public override void OnApplyTemplate()
    //{
    //    base.OnApplyTemplate();
    //    OverlayPopup = GetTemplateChild("OverlayPopup") as Popup;
    //    OverlayCanvas = GetTemplateChild("OverlayCanvas") as Canvas;
    //    //ContentRoot = GetTemplateChild("ContentRoot") as Grid;
    //}

    void PopupMenu_LayoutUpdated(object sender, EventArgs e) {
      this.LayoutUpdated -= PopupMenu_LayoutUpdated;

      if (!DesignerProperties.IsInDesignTool) {
        if (this.Parent is PopupMenuItem)
          ParentPopupMenuItem = this.Parent as PopupMenuItem;

        AddOverlayCanvasEventHandlers();

        if (!string.IsNullOrEmpty(AccessKeyElementName))
          AccessKeyElement = PopupMenuUtils.FindApplicationElementByName(AccessKeyElementName, "shortcut key");

        // If the parent element is a PopupMenuItem then show the menu on the right, disable its 'close on click' behavior and set the parent as the hover trigger
        if (ParentPopupMenuItem != null) {
          Orientation = MenuOrientationTypes.Right;
          ParentPopupMenuItem.CloseOnClick = false;
          AddTrigger(TriggerTypes.Hover, ParentPopupMenuItem);
        }

        // Delay mouse event assignments as far as possible using the MouseMove event to be sure the specified controls have been instanciated
        AddMarkupAssignedMouseTriggers();

        // Add keyboard navigation triggers
        if (!PopupMenuManager.KeyboardCaptureOn) // Make sure this is called only once
				{
          PopupMenuManager.KeyboardCaptureOn = true;
          var rootVisual = Application.Current.RootVisual as FrameworkElement;
          rootVisual.KeyDown += PopupMenuManager.AppRoot_KeyDown;
          rootVisual.KeyUp += PopupMenuManager.AppRoot_KeyUp;
        }
        ItemsControl.KeyUp += ItemsControl_KeyUp;
      }

      if (!PreserveItemContainerStyle && ItemsControl is ListBox) {
        ListBox lb = (ItemsControl as ListBox);
        // If the ItemContainerStyle has a value then clone it for modification(as it cannot be modified directly) else create a new one
        Style style = lb.ItemContainerStyle != null ? new Style().BasedOn = lb.ItemContainerStyle : new Style(typeof(ListBoxItem));
        // Add a 'HorizontalContentAlignment = Stretch' setter if its not already there(note that "3" is the string value for HorizontalAlignment.Stretch)
        Setter setter = PopupMenuUtils.GetStyleSetter(style, ContentControl.HorizontalContentAlignmentProperty);
        if (setter == null || setter.Value.ToString() != "3") {
          setter = new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
          style.Setters.Add(setter);
          lb.ItemContainerStyle = style; // Add the horizontal stretch style to the listbox items
        }
      }
    }

    private void ItemsControl_KeyUp(object sender, KeyEventArgs e) {
      UIElement selectedItem = null;

      if (ItemsControl is Selector)
        selectedItem = (ItemsControl as Selector).SelectedItem as UIElement;

      if (selectedItem != null) {
        if (e.Key == Key.Enter) {
          if (selectedItem != null && selectedItem is PopupMenuItem)
            (selectedItem as PopupMenuItem).OnClick();
        } else if (e.Key == Key.Space) {
          if (selectedItem != null && selectedItem is PopupMenuItem)
            (selectedItem as PopupMenuItem).ToggleCheckedValue();
        } else if (e.Key == (Orientation == MenuOrientationTypes.Left ? Key.Left : Key.Right)) {
          var selectedItemContainer = PopupMenuUtils.GetContainer<ContentControl>(selectedItem as FrameworkElement);
          if (selectedItemContainer != null) {
            // Identify all navigable menus associated with the selected item
            var menuTriggersKeyedUp = PopupMenuManager.ApplicationMenus.Where(m =>
              (m.TriggerType == TriggerTypes.Hover || m.TriggerType == TriggerTypes.LeftClick)
              && (m.TriggerElement == selectedItem || m.TriggerElement == selectedItemContainer));

            if (menuTriggersKeyedUp.Count() > 0) {	// Open the associated menus
              foreach (var menuTrigger in menuTriggersKeyedUp)
                menuTrigger.PopupMenu.OpenNextTo(selectedItemContainer, menuTrigger.PopupMenu.Orientation, true, true);
            } else {
              PopupMenuManager.CloseHangingMenus(0, false, null);
            }
          }
        }
      } else // Handle access keys
			{
        foreach (var item in ItemsControl.Items.Where(i => i is HeaderedItemsControl)) {
          object header = (item as HeaderedItemsControl).Header;
          if (header is StackPanel && ((header as StackPanel).Tag ?? "").ToString().ToUpper() == e.Key.ToString()) {
            if (item is PopupMenuItem)
              (item as PopupMenuItem).OnClick();
            else
              (ItemsControl as Selector).SelectedItem = item;
          }
        }
      }

      if (e.Key == Key.Escape
        || selectedItem == null && (e.Key == Key.Up || e.Key == Key.Down) && e.Key != SubmenuLaunchKey
        || e.Key == (Orientation == MenuOrientationTypes.Left ? Key.Right : Key.Left))// e.Key == (Key)((int)Key.Left + (((int)Orientation) % 4)))
        PopupMenuManager.CloseTopMenu(e);

      //if (selectedItem != null)
      //{
      //    var child = (selectedItem as FrameworkElement).GetVisualDescendants().OfType<Control>().LastOrDefault();
      //    if (child != null)
      //        child.Focus();
      //}		}

      //if (Orientation == MenuOrientationTypes.Bottom && e.Key == Key.Up)
      //    if (ItemsControl is Selector && (ItemsControl as Selector).SelectedIndex == 0)
      //        CloseTopMenu(null);
    }

    private void AddMarkupAssignedMouseTriggers() {
      if (!string.IsNullOrEmpty(HoverElements))
        AddTrigger(TriggerTypes.Hover, HoverElements);

      if (!string.IsNullOrEmpty(RightClickElements))
        AddTrigger(TriggerTypes.RightClick, RightClickElements);

      if (!string.IsNullOrEmpty(LeftClickElements))
        AddTrigger(TriggerTypes.LeftClick, LeftClickElements);
    }

    public void AddLeftClickElements(params UIElement[] triggerElements) {
      AddTrigger(TriggerTypes.LeftClick, triggerElements);
    }

    public void AddHoverElements(params UIElement[] triggerElements) {
      AddTrigger(TriggerTypes.Hover, triggerElements);
    }

    public void AddRightClickElements(params UIElement[] triggerElements) {
      AddTrigger(TriggerTypes.RightClick, triggerElements);
    }

    public void AddTrigger(TriggerTypes triggerType, string triggerElementNames) {
      foreach (string elementName in triggerElementNames.Split(',')) {
        var elem = elementName == "{Parent}"
          ? PopupMenuUtils.GetContainer<UIElement>(this)
          : PopupMenuUtils.FindApplicationElementByName(elementName, "trigger type " + triggerType);

        if (elem != null)
          elem.Dispatcher.BeginInvoke(delegate {
            AddTrigger(triggerType, elem as UIElement);
          });
      }
    }

    public void AddTrigger(TriggerTypes triggerType, params UIElement[] triggerElements) {
      foreach (FrameworkElement triggerElement in triggerElements.Where(elem => elem != null)) {
        PopupMenuManager.RegisterMenu(triggerElement, triggerType, this); 

        if (triggerElement.Parent == null)
          triggerElement.LayoutUpdated += triggerElement_LayoutUpdated;
        else
          triggerElement_LayoutUpdated(triggerElement, null);
      }
    }

    private void triggerElement_LayoutUpdated(object sender, EventArgs e) {
      (sender as FrameworkElement).LayoutUpdated -= triggerElement_LayoutUpdated;

      foreach (var menu in PopupMenuManager.ApplicationMenus.Where(m => !m.IsTriggerAssigned && m.TriggerElement == sender)) {
        menu.IsTriggerAssigned = true;

        FrameworkElement triggerElement = menu.TriggerElement;

        //if (triggerElement is ContentControl)
        //{
        //    var contentControl = triggerElement as ContentControl;
        //    if (contentControl.Content is string && contentControl.Content.ToString().Contains('^'))
        //        contentControl.Content = PopupMenuUtils.GenerateStackPanelWithUnderlinedText(contentControl.Content as string, '^');
        //}

        menu.TriggerElement.Dispatcher.BeginInvoke(delegate {
          switch (menu.TriggerType) {
            case TriggerTypes.RightClick:

              triggerElement.KeyDown += triggerElement_KeyDown; // Enable keyboard navigation

              triggerElement.MouseRightButtonDown += (o, ev) => { ev.Handled = true; }; // Disable the silverlight context menu that comes by default

              triggerElement.MouseRightButtonUp += TriggerElement_RightClick;
              break;

            case TriggerTypes.LeftClick:
              triggerElement.KeyDown += triggerElement_KeyDown; // Enable keyboard navigation
              triggerElement.KeyUp += triggerElement_KeyUp;

              // The Click event is used for buttons since they do not seem to trigger the MouseLeftButton event
              if (triggerElement is ButtonBase) {
                (triggerElement as ButtonBase).Click += TriggerButton_Click;
              } else if (triggerElement is ListBoxItem || triggerElement is Selector) {																  // Objects like the ListBox, ComboBox, ListBoxItem and the ComboBoxItem do not 
                triggerElement.MouseLeftButtonUp += TriggerElement_LeftClick; // capture MouseLeftButtonDown in which case MouseLeftButtonUp is used instead
              } else {
                triggerElement.MouseLeftButtonDown += TriggerElement_LeftClick;
              }
              // Monitor hover events as well
              triggerElement.MouseEnter += leftClickTriggerElement_Hover;
              break;

            case TriggerTypes.Hover:
              triggerElement.KeyDown += triggerElement_KeyDown; // Enable keyboard navigation
              triggerElement.KeyUp += triggerElement_KeyUp;

              triggerElement.MouseEnter += TriggerElement_Hover;

              // Monitor left click events as well
              triggerElement.MouseLeftButtonUp += hoverTriggerElement_LeftClick;

              // If the trigger element has an access key and no tooltip then use it to display the access key
              triggerElement.MouseEnter += delegate {
                if (AccessKey != Key.None && ToolTipService.GetToolTip(triggerElement) == null)
                  ToolTipService.SetToolTip(triggerElement,
                    PopupMenuUtils.GetShortcutKeyDisplayText(AccessKey, AccessKeyModifier1, AccessKeyModifier2));
              };
              break;
          }
        });
      }
    }

    public void UnassignTriggerElement(FrameworkElement element) {
      foreach (var menu in PopupMenuManager.ApplicationMenus.Where(m => m.IsTriggerAssigned && m.TriggerElement == element)) {
        //menu.IsTriggerAssigned = true;

        FrameworkElement triggerElement = menu.TriggerElement;

        //if (triggerElement is ContentControl)
        //{
        //    var contentControl = triggerElement as ContentControl;
        //    if (contentControl.Content is string && contentControl.Content.ToString().Contains('^'))
        //        contentControl.Content = PopupMenuUtils.GenerateStackPanelWithUnderlinedText(contentControl.Content as string, '^');
        //}

        menu.TriggerElement.Dispatcher.BeginInvoke(delegate {
          switch (menu.TriggerType) {
            case TriggerTypes.RightClick:

              triggerElement.KeyDown -= triggerElement_KeyDown; // Enable keyboard navigation

              triggerElement.MouseRightButtonDown -= (o, ev) => { ev.Handled = true; }; // Disable the silverlight context menu that comes by default

              triggerElement.MouseRightButtonUp -= TriggerElement_RightClick;
              break;

            case TriggerTypes.LeftClick:
              triggerElement.KeyDown -= triggerElement_KeyDown; // Enable keyboard navigation
              triggerElement.KeyUp -= triggerElement_KeyUp;

              // The Click event is used for buttons since they do not seem to trigger the MouseLeftButton event
              if (triggerElement is ButtonBase) {
                (triggerElement as ButtonBase).Click -= TriggerButton_Click;
              } else if (triggerElement is ListBoxItem || triggerElement is Selector) {																  // Objects like the ListBox, ComboBox, ListBoxItem and the ComboBoxItem do not 
                triggerElement.MouseLeftButtonUp -= TriggerElement_LeftClick; // capture MouseLeftButtonDown in which case MouseLeftButtonUp is used instead
              } else {
                triggerElement.MouseLeftButtonDown -= TriggerElement_LeftClick;
              }
              // Monitor hover events as well
              triggerElement.MouseEnter -= leftClickTriggerElement_Hover;
              break;

            case TriggerTypes.Hover:
              triggerElement.KeyDown -= triggerElement_KeyDown; // Enable keyboard navigation
              triggerElement.KeyUp -= triggerElement_KeyUp;

              triggerElement.MouseEnter -= TriggerElement_Hover;

              // Monitor left click events as well
              triggerElement.MouseLeftButtonUp -= hoverTriggerElement_LeftClick;

              // If the trigger element has an access key and no tooltip then use it to display the access key
              triggerElement.MouseEnter -= delegate {
                if (AccessKey != Key.None && ToolTipService.GetToolTip(triggerElement) == null)
                  ToolTipService.SetToolTip(triggerElement,
                    PopupMenuUtils.GetShortcutKeyDisplayText(AccessKey, AccessKeyModifier1, AccessKeyModifier2));
              };
              break;
          }
        });
      }
    }

    private void triggerElement_KeyDown(object sender, KeyEventArgs e) {
      if (e.Key == SubmenuLaunchKey) {
        e.Handled = true;
        ItemsControl.Focus();
      }
    }

    // Enables keyboard navigation
    private void triggerElement_KeyUp(object sender, KeyEventArgs e) {
      if (PopupMenuManager.OpenMenus.Count == 0) {
        if (e.Key == SubmenuLaunchKey) {
          var triggerElement = (sender is Selector ? (sender as Selector).SelectedItem : sender) as FrameworkElement;
          OpenNextTo(triggerElement, Orientation, true, true);
        }
      }
    }

    // This click event is for hover menus only and is only called when IsPinned is true.
    // Otherwise OverlayCanvas would be stretched all over the application, during the MouseEnter
    // event, before the user has time to click on the element thus blocking out all click activities.
    private void hoverTriggerElement_LeftClick(object triggerElement, MouseButtonEventArgs e) {
      if (_clickAlreadyHandledInMouseDownEvent)
        _clickAlreadyHandledInMouseDownEvent = false;
      else
        if (_isPinned)
          ToggleAllowPinnedState();
    }

    // Whenever a left click menu is open enable neighbouring left click menus to be opened by just hovering on them
    private void leftClickTriggerElement_Hover(object triggerElement, MouseEventArgs e) {
      if (PopupMenuManager.NeighbouringLeftClickElementUnderMouse != null)
        if (PopupMenuManager.NeighbouringLeftClickElementUnderMouse.Parent == (triggerElement as FrameworkElement).Parent)
          TriggerElement_Hover(triggerElement, e);
    }

    public void TriggerElement_RightClick(object triggerElement, MouseButtonEventArgs e) {
      ActualTriggerType = TriggerTypes.RightClick;

      var element = GetItemUnderMouse(triggerElement as FrameworkElement, e, AutoMapTriggerElementToSelectableItem, AutoSelectItem)
        ?? triggerElement as FrameworkElement;
      Point mousePos = e.GetSafePosition(null);
      Open(mousePos, MenuOrientationTypes.Mouse, 0, element, FocusOnShow, e);
    }

    public void TriggerButton_Click(object triggerElement, RoutedEventArgs e) {
      ActualTriggerType = TriggerTypes.LeftClick;

      Point mousePos = PopupMenuUtils.GetAbsoluteElementPos(PopupMenuManager.ApplicationMenus.Count == 0, triggerElement as FrameworkElement);
      Open(mousePos, Orientation, 0, triggerElement as FrameworkElement, FocusOnShow, e as MouseButtonEventArgs);
    }

    public void TriggerElement_LeftClick(object triggerElement, MouseButtonEventArgs e) {
      ActualTriggerType = TriggerTypes.LeftClick;

      if (_clickAlreadyHandledInMouseDownEvent) {
        _clickAlreadyHandledInMouseDownEvent = false;
      } else {
        if (OverlayPopup.IsOpen) {
          this.Close();
        } else {
          var element = GetItemUnderMouse(triggerElement as FrameworkElement, e, AutoMapTriggerElementToSelectableItem, AutoSelectItem);
          if (element != null) {
            Point mousePos = PopupMenuUtils.GetAbsoluteElementPos(PopupMenuManager.ApplicationMenus.Count == 0, element);
            Open(mousePos, Orientation, 0, element, FocusOnShow, e);
          } else {
            ActualTriggerElement = triggerElement as FrameworkElement; // This would normally be set when calling the Open method
          }
        }
      }
    }

    public void TriggerElement_Hover(object triggerElement, MouseEventArgs e) {
      ActualTriggerType = TriggerTypes.Hover;

      var element = GetItemUnderMouse(triggerElement as FrameworkElement, e, AutoMapTriggerElementToSelectableItem, AutoSelectItem);
      if (element != null) {
        Point mousePos = PopupMenuUtils.GetAbsoluteElementPos(PopupMenuManager.ApplicationMenus.Count == 0, element);
        Open(mousePos, Orientation, OpenDelay, element, FocusOnShow, e);
      }
    }

    /// <summary>
    /// Adds event handlers to the transparent OverlayCanvas which stretches over the application.
    /// It ensures that the menu is closed upon mouse activity detection outside the menu itself
    /// without counting on the less dependable MouseOut event.
    /// All events here can only take place when the menu is open.
    /// </summary>
    private void AddOverlayCanvasEventHandlers() {
      OverlayCanvas.MouseMove += (sender, e) => {
        PopupMenuManager.CapturedMouseEventArgs = e;
        if (!IsModal && !IsPinned && ActualTriggerElement != null) {
          // Get all neighbouring left click trigger elements
          var neighbouringLeftClickElements = PopupMenuManager.ApplicationMenus.Where(elem =>
            elem.TriggerType == TriggerTypes.LeftClick
            && elem.TriggerElement != ActualTriggerElement
            && elem.TriggerElement.Parent == ActualTriggerElement.Parent).Select(elem => elem.TriggerElement);

          // Get the first neighbouring hover menu being hovered(this variable is later used in the leftClickTriggerElement_Hover event)
          PopupMenuManager.NeighbouringLeftClickElementUnderMouse = PopupMenuUtils.GetFirstElementUnderMouse(
            e, PopupMenuManager.ApplicationMenus.Count > 1, AutoMapHoverBoundsToParent, neighbouringLeftClickElements.ToArray());

          // If the mouse is on a neignbouring hover menu
          if (PopupMenuManager.NeighbouringLeftClickElementUnderMouse != null) {
            this.Close(0); // Close actual menu to give way to the neighbouring menu
          } else if (ActualTriggerType == TriggerTypes.Hover) {
            var menuElements = ContentRoot.GetVisualChildren().OfType<FrameworkElement>().ToList();
            menuElements.Add(ActualTriggerElement); // Include the trigger element in the hit test so that the menu is not closed when the mouse is on it

            // Determine whether mouse is either on the menu or on its trigger element
            // 'OpenMenus.Count > 1' helps determine if the the menu lies in another menu(or Popup control).
            if (!PopupMenuUtils.HitTestAny(e, PopupMenuManager.ApplicationMenus.Count > 1, AutoMapHoverBoundsToParent, false, menuElements.ToArray())) {
              var neighbouringHoverElements = PopupMenuManager.ApplicationMenus.Where(elem =>
                elem.TriggerType == TriggerTypes.Hover
                && elem.TriggerElement != ActualTriggerElement).Select(elem => elem.TriggerElement).ToArray();

              if (PopupMenuUtils.HitTestAny(e, PopupMenuManager.ApplicationMenus.Count > 1, neighbouringHoverElements)) {
                this.Close(0); // Close immediately when hovering another hover element
              } else if (IsOpening && CloseOnHoverOut) {
                this.Close(0); // Close immediately when hovering outside an opening menu before its OpenDelay time has elapsed
              } else if (!IsClosing) {
                _timerClose = new Timer(delegate {
                  OverlayPopup.Dispatcher.BeginInvoke(delegate {
                    PopupMenuManager.CloseHangingMenus(CloseDuration, true, PopupMenuManager.CapturedMouseEventArgs);
                  });
                }, null, CloseDelay, Timeout.Infinite);
              }
            }
          }
        }
      };

      OverlayCanvas.MouseRightButtonDown += (sender, e) => // Close the menu when a right click is made outside the popup
      {
        if (!IsModal && !PopupMenuUtils.HitTest(e, PopupMenuManager.ApplicationMenus.Count > 1, ContentRoot)) // Make sure the mouse is not on the menu itself
          this.Close(0);
        e.Handled = true; // Disable the default Silverlight context menu
      };

      OverlayCanvas.MouseLeftButtonDown += (sender, e) => // Close the menu when a left click is captured outside the popup
      {
        if (!IsModal) {
          PopupMenuManager.CloseHangingMenus(0, false, e);
          // Determine if the parent menu was clicked(through OverlayCanvas itself)
          bool triggerElementClicked = PopupMenuUtils.HitTestAny(e, PopupMenuManager.ApplicationMenus.Count > 1, AutoMapHoverBoundsToParent, false, ActualTriggerElement);

          // If the menu is a pinned hover menu and its trigger element was clicked
          if (triggerElementClicked && ActualTriggerType == TriggerTypes.Hover && _isPinned) {
            ToggleAllowPinnedState();
            _clickAlreadyHandledInMouseDownEvent = true;
          }
          // If outer canvas was clicked
          if (ActualTriggerType != TriggerTypes.Hover) {
            Close(0);
            // Avoid menu reopening via the MouseLeftButtonUp event if the trigger element itself was clicked
            _clickAlreadyHandledInMouseDownEvent = triggerElementClicked;
          }
        }
      };
    }

    private void ToggleAllowPinnedState() {
      _allowPinnedState = !_allowPinnedState;

      ExpandOverlayCanvas(!_allowPinnedState);

      if (!_allowPinnedState)
        this.Close(CloseDuration);
    }

    /// <summary>
    /// Get the item under the mouse
    /// </summary>
    /// <param name="senderElement">The clicked or hovered element</param>
    /// <param name="e"></param>
    /// <param name="returnSelectableItemIfAny">Return the clicked or hovered item inside the trigger element if the latter is a DataGrid, a ListBox or a TreeView</param>
    /// <param name="selectItemIfSelectable">Select the item if lies in a ListBox, Datagrid or TreeView</param>
    /// <returns>The item under the mouse</returns>
    private FrameworkElement GetItemUnderMouse(FrameworkElement senderElement, MouseEventArgs e, bool returnSelectableItemIfAny, bool selectItemIfSelectable) {
      VisualTreeElementsUnderMouse = VisualTreeHelper.FindElementsInHostCoordinates(
        e.GetPosition(Application.Current.RootVisual),
        senderElement.GetVisualAncestors().Last() as FrameworkElement);

      return PopupMenuUtils.GetItemUnderMouse(VisualTreeElementsUnderMouse, senderElement, returnSelectableItemIfAny, selectItemIfSelectable);
    }

    public void OpenNextTo(FrameworkElement triggerElement, MenuOrientationTypes orientation, bool focusOnShow, bool restoreFocusOnClose) {
      Point mousePos = PopupMenuUtils.GetAbsoluteElementPos(PopupMenuManager.ApplicationMenus.Count == 0, triggerElement);
      OverlayPopup.IsOpen = IsOpening = false; // Make sure code inside the Open method is not skipped if being called simultaneously
      this.Open(mousePos, orientation, 0, triggerElement, focusOnShow, null);
      _restoreFocusOnClose = restoreFocusOnClose;
    }

    public void Open(Point mousePos, MenuOrientationTypes orientation, int delay, FrameworkElement triggerElement, bool focusOnShow, MouseEventArgs e) {
      ActualTriggerElement = triggerElement;

      if (!OverlayPopup.IsOpen && !IsOpening) {

        if (this.ItemsControl.DataContext == null)
          this.ItemsControl.DataContext = this.DataContext;

        OverlayCanvas.Opacity = 0; // Make sure the root grid is hidden before it is correctly positioned

        IsOpening = true;

        if (Opening != null)
          Opening(triggerElement, e);

        OverlayPopup.IsOpen = true;
        //if (!OpenMenus.Contains(this))
        PopupMenuManager.OpenMenus.Insert(0, this); // Add the actual menu on top of the list of open menus

        if (ItemsControl is Selector)
          (ItemsControl as Selector).SelectedIndex = -1; // Reset selected item in menu

        if (ParentOverlapGridBrush != null)
          ShowParentOverlapGrid(triggerElement);

        ExpandOverlayCanvas(!IsPinned);

        // Start opening the menu after the period of time specified by the delay parameter in milliseconds
        _timerOpen = new Timer(delegate {
          IsOpening = false;

          OverlayPopup.Dispatcher.BeginInvoke(delegate() {
            // Make sure the menu is still open. This could not be the
            // case if the mouse was clicked outside the menu right away.
            if (OverlayPopup.IsOpen && triggerElement != null) {
              if (!VisualTreeGenerated) // Perform only on first load
							{
                // Force content layout update
                OverlayPopup.IsOpen = false;
                OverlayPopup.IsOpen = true;
              }

              if (Showing != null)
                Showing(triggerElement, e);

              SetMenuPosition(triggerElement, orientation, mousePos);

              Storyboard sbOpen = OpenAnimation ?? PopupMenuUtils.CreateStoryBoard(0, OpenDuration, OverlayCanvas, "UIElement.Opacity", 1, null);
              sbOpen.Begin();
              sbOpen.Completed += delegate {
                if (OverlayCanvas.Opacity == 0) // Just in case sbOpen fails to restore the opacity
                  OverlayCanvas.Opacity = 1;

                if (Shown != null)
                  Shown.BeginInvoke(triggerElement, e, null, null);

                // Make sure the menu stays in view after the open StoryBoard has completed
                SetMenuPosition(triggerElement, orientation, mousePos);
              };

              if (focusOnShow) {
                ItemsControl.Focus();
                _restoreFocusOnClose = true;
              }

              VisualTreeGenerated = true;
            }
          });
        }, null, delay, Timeout.Infinite);
      }
    }

    private void SetMenuPosition(FrameworkElement positioningElement, MenuOrientationTypes orientation, Point? mousePos) {
      Point ptMargin = mousePos.HasValue ? new Point(mousePos.Value.X + OffsetX, mousePos.Value.Y + OffsetY)
                         : new Point(Canvas.GetLeft(positioningElement), Canvas.GetTop(positioningElement));

      switch (orientation) {
        case MenuOrientationTypes.Right:
          ptMargin.X += positioningElement.ActualWidth - 1;
          break;
        case MenuOrientationTypes.Bottom:
          ptMargin.Y += positioningElement.ActualHeight;
          break;
        case MenuOrientationTypes.Left:
          ptMargin.X += -ContentRoot.ActualWidth - 1;
          break;
        case MenuOrientationTypes.Top:
          ptMargin.Y += -ContentRoot.ActualHeight;
          break;
      }

      // Make sure the menu stays within root layout bounds
      PopupMenuUtils.SetPosition(ContentRoot, ptMargin, true);
    }

    private void ShowParentOverlapGrid(FrameworkElement triggerElement) {
      if (ParentOverlapGrid == null) {
        ParentOverlapGrid = new Grid { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
        OverlayCanvas.Children.Add(ParentOverlapGrid);
      }

      switch (Orientation) {
        case MenuOrientationTypes.Bottom:
          if (!ParentOverlapGridThickness.HasValue)
            ParentOverlapGridThickness = new Thickness(0, 1, 0, 1);

          ParentOverlapGrid.Height = ParentOverlapGridThickness.Value.Top + ParentOverlapGridThickness.Value.Bottom;
          ParentOverlapGrid.Width = triggerElement.ActualWidth + ParentOverlapGridThickness.Value.Left + ParentOverlapGridThickness.Value.Right;
          break;
        case MenuOrientationTypes.Right:
          if (!ParentOverlapGridThickness.HasValue)
            ParentOverlapGridThickness = new Thickness(1, -1, 1, 3);

          ParentOverlapGrid.Height = triggerElement.ActualHeight + ParentOverlapGridThickness.Value.Top + ParentOverlapGridThickness.Value.Bottom;
          ParentOverlapGrid.Width = ParentOverlapGridThickness.Value.Left + ParentOverlapGridThickness.Value.Right;
          OffsetX = 2;
          break;
        case MenuOrientationTypes.Left:
          // To be implemented
          break;
        case MenuOrientationTypes.Top:
          // To be implemented
          break;
      }
      ParentOverlapGrid.Margin = new Thickness(-ParentOverlapGridThickness.Value.Left, -ParentOverlapGridThickness.Value.Top, 0, 0);
      ParentOverlapGrid.Background = ParentOverlapGridBrush;
    }

    public void Close() {
      Close(CloseDuration);
    }

    public void Close(int transitionTime) {
      IsOpening = false;
      IsClosing = true;

      PopupMenuManager.OpenMenus.Remove(this);

      if (_restoreFocusOnClose && ActualTriggerElement is Control)
        (ActualTriggerElement as Control).Focus();
      _restoreFocusOnClose = false;

      if (_timerOpen != null) {
        _timerOpen.Change(0, Timeout.Infinite);
        _timerOpen.Dispose();
        _timerOpen = null;
      }

      if (_timerClose != null) {
        _timerClose.Change(0, Timeout.Infinite);
        _timerClose.Dispose();
        _timerClose = null;
      }

      if (Closing != null)
        Closing(this, PopupMenuManager.CapturedMouseEventArgs);


      if (ActualTriggerElement != null) {
        if (AutoSelectItem) // Unselect items selected when the menu was open
				{
          if (ActualTriggerElement is ListBoxItem)
            (ActualTriggerElement as ListBoxItem).IsSelected = false;
          if (ActualTriggerElement is PopupMenuItem)
            (PopupMenuUtils.GetContainer<ListBoxItem>(ActualTriggerElement)).IsSelected = false;
        }
        ActualTriggerElement = null;
      }

      if (OpenAnimation != null)
        OpenAnimation.Stop();

      Storyboard sbClose = CloseAnimation ?? PopupMenuUtils.CreateStoryBoard(0, transitionTime, OverlayCanvas, "UIElement.Opacity", 0, null);
      sbClose.Begin();
      sbClose.Completed += delegate {
        OverlayPopup.IsOpen = false;
        IsClosing = false;
      };
    }
  }
}