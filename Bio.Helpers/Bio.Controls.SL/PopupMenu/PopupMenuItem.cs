// Copyright (c) 2009 Ziad Jeeroburkhan. All Rights Reserved.
// GNU General Public License version 2 (GPLv2)
// (http://sl4popupmenu.codeplex.com/license)

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bio.Helpers.Controls.SL {
  [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
  [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
  [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]
  [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
  [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PopupMenuItem))]
  public class PopupMenuItem : HeaderedItemsControl//, INotifyPropertyChanged
  {
    public Key ShortcutKey { get; set; }

    public ModifierKeys ShortcutKeyModifier1 { get; set; }

    public ModifierKeys ShortcutKeyModifier2 { get; set; }

    public bool DisplayShortcutKey { get; set; }

    public string Shortcut {

      get {
        return PopupMenuUtils.GetShortcutKeyDisplayText(ShortcutKey, ShortcutKeyModifier1, ShortcutKeyModifier2);
      }
      set {
        var keys = value.Replace("Ctrl", "Control").Split('+').Reverse().ToArray();
        for (int i = 0; i < keys.Length; i++)
          switch (i) {
            case 0:
              ShortcutKey = (Key)Enum.Parse(typeof(Key), keys[i], true);
              break;
            case 1:
              ShortcutKeyModifier2 = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[i], true);
              break;
            case 2:
              ShortcutKeyModifier1 = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), keys[i], true);
              break;
          }
      }
    }

    public event RoutedEventHandler Click;

    private bool _isLoaded = false;
    private bool _isFocused = false;

    public bool IsThreeState { get; set; }

    public string ImageCheckedPath { get; set; }

    public string ImageUncheckedPath { get; set; }

    public string ImageIntermediatePath { get; set; }

    internal static DependencyProperty Register<T>(string name, T defaultValue, PropertyChangedCallback propertyChangedCallback) {
      return DependencyProperty.Register(name, typeof(T), typeof(PopupMenuItem), new PropertyMetadata(defaultValue, propertyChangedCallback));
    }

    #region Commanding

    public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

    public static readonly DependencyProperty CommandProperty = Register<ICommand>("Command", null, (d, e) => {
      ((PopupMenuItem)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
    });

    private void OnCommandChanged(ICommand oldValue, ICommand newValue) {
      if (null != oldValue)
        oldValue.CanExecuteChanged -= HandleCanExecuteChanged;
      if (null != newValue)
        newValue.CanExecuteChanged += HandleCanExecuteChanged;
      UpdateIsEnabled();
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the Command property of a PopupMenuItem.
    /// </summary>
    public object CommandParameter { get { return (object)GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }

    public static readonly DependencyProperty CommandParameterProperty = Register<object>("CommandParameter", null, (d, e) => {
      ((PopupMenuItem)d).UpdateIsEnabled();
    });

    /// <summary>
    /// Handles the CanExecuteChanged event of the Command property.
    /// </summary>
    private void HandleCanExecuteChanged(object sender, EventArgs e) {
      UpdateIsEnabled();
    }

    /// <summary>
    /// Updates the IsEnabled property.
    /// </summary>
    private void UpdateIsEnabled() {
      IsEnabled = Command == null || Command.CanExecute(CommandParameter);
      ChangeVisualState(true);
    }

    #endregion Commanding

    // All credits to Locksley for his thread safe implementation of the IsChecked property.
    #region IsChecked

    public event EventHandler Checked,
                  Indeterminate,
                  Unchecked;

    /// <summary>
    /// Displays the checked state by showing or hiding the image in the left margin accordingly.
    /// The menu item can provide a three value state(Checked, Intermediate and Unchecked) by setting the IsThreeState property to true.
    /// </summary>
    public bool? IsChecked {
      get { return (bool?)GetValue(IsCheckedProperty); }
      set { SetValue(IsCheckedProperty, value); }
    }

    public static readonly DependencyProperty IsCheckedProperty = Register<bool?>("IsChecked", false, (d, e) => {
      var pmi = (PopupMenuItem)d;
      // Store the EventHandler into a local temp variable for thread-safety
      EventHandler temp;
      if (!pmi.IsChecked.HasValue) {
        pmi.SetImagePath(pmi.ImageIntermediatePath, true);
        temp = Interlocked.CompareExchange(ref pmi.Indeterminate, null, null);
      } else if (pmi.IsChecked.Value) {
        pmi.SetImagePath(pmi.ImageCheckedPath, true);
        temp = Interlocked.CompareExchange(ref pmi.Checked, null, null);
      } else {
        pmi.SetImagePath(pmi.ImageUncheckedPath, true);
        temp = Interlocked.CompareExchange(ref pmi.Unchecked, null, null);
      }

      if (temp != null)
        temp(pmi, EventArgs.Empty);
    });

    public void SetImagePath(string imagePath, bool makeTransparentOnEmptyPath) {
      ImageLeftOpacity = string.IsNullOrEmpty(imagePath) && makeTransparentOnEmptyPath ? 0 : 1;
      ImagePath = imagePath;
    }

    #endregion IsChecked

    public bool CloseOnClick {
      get { return (bool)GetValue(CloseOnClickProperty); }
      set { SetValue(CloseOnClickProperty, value); }
    }

    public static readonly DependencyProperty CloseOnClickProperty
      = Register<bool>("CloseOnClick", true, null);

    public new bool IsEnabled {
      get { return (bool)GetValue(IsEnabledProperty); }
      set {
        SetValue(IsEnabledProperty, value);
        CloseOnClick = value;
        ChangeVisualState(true);
      }
    }

    public bool IsVisible {
      get { return (bool)GetValue(IsVisibleProperty); }
      set { SetValue(IsVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsVisibleProperty
      = Register<bool>("IsVisible", true, (d, e) => {
      var pmi = (PopupMenuItem)d;
      // The container is not accessible in the setter especially when the value is being applied from a xaml property
      // In this case the value is assigned before the visual tree is created
      if (pmi._isLoaded)
        pmi.UpdateContainerVisibility();

      pmi.IsEnabled = pmi.IsVisible;
      // else this is handled in OnApplyTemplate instead anyway
    });

    public ImageSource ImageSource {
      get { return (ImageSource)GetValue(ImageSourceProperty); }
      set { SetValue(ImageSourceProperty, value); }
    }

    public static readonly DependencyProperty ImageSourceProperty
      = Register<ImageSource>("ImageSource", null, (d, e) => {
      ((PopupMenuItem)d).ImageSource = e.NewValue as ImageSource;
    });

    public object ContentLeft {
      get { return (ImageSource)GetValue(ContentLeftProperty); }
      set { SetValue(ContentLeftProperty, value); }
    }

    public static readonly DependencyProperty ContentLeftProperty
      = Register<object>("ContentLeft", null, null);

    public string ImagePath {
      get { return (string)GetValue(ImagePathProperty); }
      set { SetValue(ImagePathProperty, value); }
    }

    public static readonly DependencyProperty ImagePathProperty
      = Register<string>("ImagePath", null, (d, e) => {
      ((PopupMenuItem)d).ImageSource = new BitmapImage(new Uri((e.NewValue ?? "").ToString(), UriKind.RelativeOrAbsolute));
    });

    public ImageSource ImageSourceForRightMargin {
      get { return (ImageSource)GetValue(ImageSourceForRightMarginProperty); }
      set { SetValue(ImageSourceForRightMarginProperty, value); }
    }

    public static readonly DependencyProperty ImageSourceForRightMarginProperty
      = Register<ImageSource>("ImageSourceForRightMargin", null, (d, e) => {
      ((PopupMenuItem)d).ImageSourceForRightMargin = e.NewValue as ImageSource;
    });

    public object ContentForRightMargin {
      get { return (ImageSource)GetValue(ContentForRightMarginProperty); }
      set { SetValue(ContentForRightMarginProperty, value); }
    }

    public static readonly DependencyProperty ContentForRightMarginProperty
      = Register<object>("ContentForRightMargin", null, null);

    public string ImagePathForRightMargin {
      get { return (string)GetValue(ImagePathForRightMarginProperty); }
      set { SetValue(ImagePathForRightMarginProperty, value); }
    }

    public static readonly DependencyProperty ImagePathForRightMarginProperty
      = Register<string>("ImagePathForRightMargin", null, (d, e) => {
      ((PopupMenuItem)d).ImageSourceForRightMargin
        = new BitmapImage(new Uri(e.NewValue.ToString(), UriKind.RelativeOrAbsolute));
    });

    public object Tooltip {
      get { return (string)GetValue(TooltipProperty); }
      set { SetValue(TooltipProperty, value); }
    }

    public static readonly DependencyProperty TooltipProperty
      = Register<object>("Tooltip", null, (d, e) => {
      ToolTipService.SetToolTip((PopupMenuItem)d, e.NewValue);
    });

    public Visibility VerticalSeparatorVisibility {
      get { return (Visibility)GetValue(VerticalSeparatorVisibilityProperty); }
      set { SetValue(VerticalSeparatorVisibilityProperty, value); }
    }

    public static readonly DependencyProperty VerticalSeparatorVisibilityProperty
      = Register<Visibility>("VerticalSeparatorVisibility", Visibility.Visible, null);

    public double VerticalSeparatorWidth {
      get { return (double)GetValue(VerticalSeparatorWidthProperty); }
      set { SetValue(VerticalSeparatorWidthProperty, value); }
    }

    public static readonly DependencyProperty VerticalSeparatorWidthProperty
      = Register<double>("VerticalSeparatorWidth", 2, null);

    /// <summary>
    /// The vertical and horizontal and vertical separator start color
    /// </summary>
    public Color SeparatorStartColor {
      get { return (Color)GetValue(SeparatorStartColorProperty); }
      set { SetValue(SeparatorStartColorProperty, value); }
    }

    public static readonly DependencyProperty SeparatorStartColorProperty
      = Register<Color>("SeparatorStartColor", Color.FromArgb(55, 55, 55, 55), (d, e) => {
      var pmi = (PopupMenuItem)d;
      pmi.VerticalSeparatorFill = PopupMenuUtils.MakeColorGradient((Color)e.NewValue, pmi.SeparatorEndColor, 0);
    });

    /// <summary>
    /// The vertical and horizontal and vertical separator end color
    /// </summary>
    public Color SeparatorEndColor {
      get { return (Color)GetValue(SeparatorEndColorProperty); }
      set { SetValue(SeparatorEndColorProperty, value); }
    }

    public static readonly DependencyProperty SeparatorEndColorProperty
      = Register<Color>("SeparatorEndColor", Color.FromArgb(55, 255, 255, 255), (d, e) => {
      var pmi = (PopupMenuItem)d;
      pmi.VerticalSeparatorFill = PopupMenuUtils.MakeColorGradient(pmi.SeparatorStartColor, (Color)e.NewValue, 0);
    });

    public Brush VerticalSeparatorFill {
      get { return (Brush)GetValue(VerticalSeparatorFillProperty); }
      set { SetValue(VerticalSeparatorFillProperty, value); }
    }

    public static readonly DependencyProperty VerticalSeparatorFillProperty
      = Register<Brush>("VerticalSeparatorFill", null, null);

    public bool ShowLeftMargin {
      get { return (bool)GetValue(ShowLeftMarginProperty); }
      set { SetValue(ShowLeftMarginProperty, value); }
    }

    public static readonly DependencyProperty ShowLeftMarginProperty
      = Register<bool>("ShowLeftMargin", true, (d, e) => {
      var pmi = (PopupMenuItem)d;
      pmi.ImageLeftVisibility =
      pmi.VerticalSeparatorVisibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    });

    public double ImageLeftOpacity {
      get { return (double)GetValue(ImageLeftOpacityProperty); }
      set { SetValue(ImageLeftOpacityProperty, value); }
    }

    public static readonly DependencyProperty ImageLeftOpacityProperty
      = Register<double>("ImageLeftOpacity", 1, null);

    public Visibility ImageLeftVisibility {
      get { return (Visibility)GetValue(ImageLeftVisibilityProperty); }
      set { SetValue(ImageLeftVisibilityProperty, value); }
    }

    public static readonly DependencyProperty ImageLeftVisibilityProperty
      = Register<Visibility>("ImageLeftVisibility", Visibility.Visible, null);

    public double ImageLeftMinWidth {
      get { return (double)GetValue(ImageLeftMinWidthProperty); }
      set { SetValue(ImageLeftMinWidthProperty, value); }
    }

    public static readonly DependencyProperty ImageLeftMinWidthProperty
      = Register<double>("ImageLeftMinWidth", 16, null);

    public double ImageRightMinWidth {
      get { return (double)GetValue(ImageRightMinWidthProperty); }
      set { SetValue(ImageRightMinWidthProperty, value); }
    }

    public static readonly DependencyProperty ImageRightMinWidthProperty
      = Register<double>("ImageRightMinWidth", 16, null);

    #region PopupMenuHorizontalSeparator

    public Visibility HorizontalSeparatorVisibility {
      get { return (Visibility)GetValue(HorizontalSeparatorVisibilityProperty); }
      set { SetValue(HorizontalSeparatorVisibilityProperty, value); }
    }

    public static readonly DependencyProperty HorizontalSeparatorVisibilityProperty
      = Register<Visibility>("HorizontalSeparatorVisibility", Visibility.Collapsed, null);

    public Brush HorizontalSeparatorBrush {
      get { return (Brush)GetValue(HorizontalSeparatorBrushProperty); }
      set { SetValue(HorizontalSeparatorBrushProperty, value); }
    }

    public static readonly DependencyProperty HorizontalSeparatorBrushProperty
      = Register<Brush>("HorizontalSeparatorBrush", null, null);

    public double HorizontalSeparatorHeight {
      get { return (double)GetValue(HorizontalSeparatorHeightProperty); }
      set { SetValue(HorizontalSeparatorHeightProperty, value); }
    }

    public static readonly DependencyProperty HorizontalSeparatorHeightProperty
      = Register<double>("HorizontalSeparatorHeight", 2, null);

    #endregion PopupMenuHorizontalSeparator

    public PopupMenuItem() :
      this(null, null, true) { }

    public PopupMenuItem(string iconUrl, object header, params UIElement[] items)
      : this(iconUrl, header, true, items) { }

    public PopupMenuItem(string iconUrl, string header, string tag, bool useItemTemplate)
      : this(iconUrl, useItemTemplate, new TextBlock() { Text = header, Tag = tag }) { }

    public PopupMenuItem(string iconUrl, object header, bool useDefaultTemplate, params UIElement[] items)
      : base() {
      this.DefaultStyleKey = typeof(PopupMenuItem);

      DisplayShortcutKey = true;

      VerticalSeparatorFill = PopupMenuUtils.MakeColorGradient(SeparatorStartColor, SeparatorEndColor, 0);

      ShowLeftMargin = useDefaultTemplate;
      ImagePath = iconUrl;


      this.Header = header;


      // Add custom elements if any
      if (items != null)
        foreach (FrameworkElement element in items.Where(el => el != null)) {
          if (element.Parent is Panel)
            (element.Parent as Panel).Children.Remove(element);
          this.Items.Add(element);
        }

      this.Dispatcher.BeginInvoke(delegate {
        if (ShortcutKey != Key.None) {
          Application.Current.RootVisual.KeyDown += (o, e) => {
            if (PopupMenuUtils.IsKeyPressed(ShortcutKey, ShortcutKeyModifier1, ShortcutKeyModifier2, e)) {
              e.Handled = true;
              OnClick();
            }
          };

          if (DisplayShortcutKey)
            this.ContentForRightMargin = "  " + PopupMenuUtils.GetShortcutKeyDisplayText(
              ShortcutKey, ShortcutKeyModifier1, ShortcutKeyModifier2);
        }
      });
      this.Loaded += PopupMenuItem_Loaded;
    }


    void PopupMenuItem_Loaded(object sender, EventArgs e) {
      _isLoaded = true;

      this.Loaded -= PopupMenuItem_Loaded; // Prevents the method from being called each time the menu is opened

      if ((Header == null) && this.Items.Count > 0) {
        object element = this.Items.First();
        this.Items.Remove(element);
        // Use first child as header if the latter is not assigned
        Header = element;
        //this.UpdateLayout();
      }

      if (Header is string && Header.ToString().Contains('^'))
        Header = PopupMenuUtils.GenerateStackPanelWithUnderlinedText(Header.ToString(), '^');

    }

    private void UpdateContainerVisibility() {
      // The container is only accessible when the menu item is loaded.
      // So setting a value to the IsVisible property only works after that.
      var container = PopupMenuUtils.GetContainer<FrameworkElement>(this);
      container.Visibility = (bool)this.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public void ToggleCheckedValue() {
      if (!IsThreeState) {
        IsChecked = !IsChecked ?? true; // null -> true
      } else {
        if (IsChecked.HasValue)
          IsChecked = IsChecked.Value ? null as bool? : true; // false -> true, true -> null
        else
          IsChecked = false; // null -> false
      }
    }

    /// <summary>
    /// Called when the template's tree is generated.
    /// </summary>
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      ChangeVisualState(false);
      UpdateContainerVisibility();
    }

    protected override void OnGotFocus(RoutedEventArgs e) {
      base.OnGotFocus(e);
      _isFocused = true;
      ChangeVisualState(true);
    }

    protected override void OnLostFocus(RoutedEventArgs e) {
      base.OnLostFocus(e);
      _isFocused = false;
      ChangeVisualState(true);
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
      base.OnMouseEnter(e);
      ChangeVisualState(true);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
      base.OnMouseLeave(e);
      ChangeVisualState(true);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
      if (!e.Handled) {
        OnClick();
        e.Handled = true;
      }
      base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
      if (!e.Handled) {
        OnClick();
        e.Handled = true;
      }
      base.OnMouseRightButtonDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (!e.Handled && (Key.Enter == e.Key)) {
        OnClick();
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }

    public virtual void OnClick() {
      if (!string.IsNullOrEmpty(ImageCheckedPath + ImageIntermediatePath + ImageCheckedPath))
        ToggleCheckedValue();

      if (IsEnabled) {
        if (PopupMenuManager.ItemClicked != null)
          PopupMenuManager.ItemClicked(this, new RoutedEventArgs());

        if (Click != null)
          Click(this, new RoutedEventArgs());

        if (Command != null && Command.CanExecute(CommandParameter))
          Command.Execute(CommandParameter);
      }

      if (CloseOnClick)
        PopupMenuManager.CloseHangingMenus(0, false, null);
    }

    /// <summary>
    /// Changes to the correct visual state(s) for the control.
    /// </summary>
    /// <param name="useTransitions">True to use transitions; otherwise false.</param>
    protected virtual void ChangeVisualState(bool useTransitions) {
      VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", useTransitions);
      VisualStateManager.GoToState(this, IsEnabled && _isFocused ? "Focused" : "Unfocused", useTransitions);
    }
  }
}