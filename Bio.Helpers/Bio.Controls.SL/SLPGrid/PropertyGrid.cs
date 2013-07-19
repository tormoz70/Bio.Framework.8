
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;
  using System.Reflection;
  using System.Windows;
  using System.Windows.Browser;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using Bio.Helpers.Common.Types;

  #endregion

  public class RequiredPropsIsNullException : Exception {
    public RequiredPropsIsNullException(List<PropertyItem> badProps) {
      this.BadProps = badProps;
    }
    public List<PropertyItem> BadProps { get; private set; }
  }

  public class OnCustomEditorEventArgs : EventArgs {
    public PropertyGridLabel Label { get; set; }
    public PropertyEditor Editor { get; set; }
    public OnCustomEditorEventArgs()
      : base() {
    }
  }

  public class OnPreparePropertyEventArgs : EventArgs {
    public Boolean Browsable { get; set; }
    public OnPreparePropertyEventArgs()
      : base() {
    }
  }

  public delegate void OnCustomEditorEventHandler(PropertyItem sender, OnCustomEditorEventArgs args);

  public delegate void OnPreparePropertyEventHandler(PropertyInfo sender, OnPreparePropertyEventArgs args);

  #region PropertyGrid
  /// <summary>
  /// PropertyGrid
  /// </summary>
  public partial class PropertyGrid : ContentControl {
    #region Fields

    //#99B4D1
    //internal static Color backgroundColor = Color.FromArgb(127, 153, 180, 209);

    //#E9ECFA
    internal static Color backgroundColor = Color.FromArgb(255, 233, 236, 250);
    internal static Color backgroundColorFocused = Color.FromArgb(255, 94, 170, 255);

    static Type thisType = typeof(PropertyGrid);

    private PropertyEditor _selectedEditor;
    private ScrollViewer _layoutRoot;
    private Grid _mainGrid;
    private Boolean _loaded = false;
    private Boolean _resetLoadedObject;

    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public PropertyGrid() {
      base.DefaultStyleKey = typeof(PropertyGrid);
      this._props = new List<PropertyItem>();
      this.Loaded += new RoutedEventHandler(PropertyGrid_Loaded);
    }
    #endregion

    #region Properties

    #region SelectedObject

    public static readonly DependencyProperty SelectedObjectProperty =
      DependencyProperty.Register("SelectedObject", typeof(object), thisType, new System.Windows.PropertyMetadata(null, OnSelectedObjectChanged));

    public object SelectedObject {
      get { return base.GetValue(SelectedObjectProperty); }
      set { base.SetValue(SelectedObjectProperty, value); }
    }

    private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      PropertyGrid propertyGrid = d as PropertyGrid;
      if (propertyGrid != null) {
        if (!propertyGrid._loaded)
          propertyGrid._resetLoadedObject = true;
        else if (null != e.NewValue)
          propertyGrid._resetObject(e.NewValue);
        else
          propertyGrid._resetMainGrid();
      }
    }
    #endregion

    #region Default LabelWidth
    /// <summary>
    /// The DefaultLabelWidth DependencyProperty
    /// </summary>
    public static readonly DependencyProperty DefaultLabelWidthProperty =
      DependencyProperty.Register("DefaultLabelWidth", typeof(GridLength), thisType, new System.Windows.PropertyMetadata(new GridLength(75)));
    /// <summary>
    /// Gets or sets the Default Width for the labels
    /// </summary>
    public GridLength DefaultLabelWidth {
      get { return (GridLength)base.GetValue(DefaultLabelWidthProperty); }
      set { base.SetValue(DefaultLabelWidthProperty, value); }
    }
    #endregion


    #region Grid BorderBrush

    public static readonly DependencyProperty GridBorderBrushProperty =
      DependencyProperty.Register("GridBorderBrush", typeof(Brush), thisType, new System.Windows.PropertyMetadata(new SolidColorBrush(Colors.LightGray), _onGridBorderBrushChanged));

    /// <summary>
    /// Gets or sets the Border Brush of the Property Grid
    /// </summary>
    public Brush GridBorderBrush {
      get { return (Brush)base.GetValue(GridBorderBrushProperty); }
      set { base.SetValue(GridBorderBrushProperty, value); }
    }

    private static void _onGridBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      PropertyGrid propertyGrid = d as PropertyGrid;
      if (propertyGrid != null && null != propertyGrid._layoutRoot && null != e.NewValue)
        propertyGrid._layoutRoot.BorderBrush = (SolidColorBrush)e.NewValue;
    }

    #endregion

    #region Grid BorderThickness

    public static readonly DependencyProperty GridBorderThicknessProperty =
      DependencyProperty.Register("GridBorderThickness", typeof(Thickness), thisType, new System.Windows.PropertyMetadata(new Thickness(1), _onGridBorderThicknessChanged));

    /// <summary>
    /// Gets or sets the Border Thickness of the Property Grid
    /// </summary>
    public Thickness GridBorderThickness {
      get { return (Thickness)base.GetValue(GridBorderThicknessProperty); }
      set { base.SetValue(GridBorderThicknessProperty, value); }
    }


    private static void _onGridBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      PropertyGrid propertyGrid = d as PropertyGrid;
      if (propertyGrid != null && null != propertyGrid._layoutRoot && null != e.NewValue)
        propertyGrid._layoutRoot.BorderThickness = (Thickness)e.NewValue;
    }

    #endregion

    #endregion

    #region Overrides
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      this.IsTabStop = false;
      this._layoutRoot = (ScrollViewer)this.GetTemplateChild("LayoutRoot");
      this._layoutRoot.IsTabStop = false;
      this._mainGrid = (Grid)this.GetTemplateChild("MainGrid");

      _loaded = true;

      if (_resetLoadedObject) {
        _resetLoadedObject = false;
        this._resetObject(this.SelectedObject);
      }
    }
    #endregion

    #region Methods

    private List<PropertyItem> _props = null;

    public PropertyItem ProprtyByName(String name) {
      var rslt = this._props.Where((p) => {
        return String.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase);
      }).FirstOrDefault();
      return rslt;
    }

    public event OnPreparePropertyEventHandler OnPrepareProperty; 
    internal void doOnPrepareProperty(PropertyInfo propInfo, OnPreparePropertyEventArgs args){
      var eve = this.OnPrepareProperty;
      if(eve != null)
        eve(propInfo, args);
    }
    private int _setObject(Object obj) {
      int rowCount = -1;

      // Parse the objects properties
      this._props = PropertyGrid.ParseObject(this, obj);

      #region Render the Grid

      var categories = (from p in this._props
                        orderby p.Category
                        select p.Category).Distinct();

      foreach (string category in categories) {

        this._addHeaderRow(category, ref rowCount);

        var items = from p in this._props
                    where p.Category == category
                    //orderby p.Name
                    select p;

        foreach (var item in items)
          this._addPropertyRow(item, ref rowCount);

      }
      #endregion

      return rowCount++;

    }

    private void _resetObject(object obj) {
      this._resetMainGrid();

      int rowCount = this._setObject(obj);

      if (rowCount > 0)
        AddGridSplitter(rowCount);
    }
    private void _resetMainGrid() {
      this._mainGrid.Children.Clear();
      this._mainGrid.RowDefinitions.Clear();
    }
    private void _addHeaderRow(string category, ref int rowIndex) {
      rowIndex++;
      this._mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21) });

      #region Column 0 - Margin

      Border brd = GetCategoryMargin(category, GetHideImage(Visibility.Visible), GetShowImage(Visibility.Collapsed));
      this._mainGrid.Children.Add(brd);
      Grid.SetRow(brd, rowIndex);
      Grid.SetColumn(brd, 0);

      #endregion

      #region Column 1 & 2 - Category Header

      brd = GetCategoryHeader(category);
      this._mainGrid.Children.Add(brd);
      Grid.SetRow(brd, rowIndex);
      Grid.SetColumn(brd, 1);
      Grid.SetColumnSpan(brd, 2);

      #endregion
    }

    public event OnCustomEditorEventHandler OnCustomEditor;
    private void _doOnCustomEditor(PropertyGridLabel label, PropertyItem item, ref PropertyEditor editor) {
      var eve = this.OnCustomEditor;
      if (eve != null) {
        var args = new OnCustomEditorEventArgs() { Label = label, Editor = editor };
        eve(item, args);
        editor = args.Editor;
      }
    }

    public event EventHandler<PropertyChangingEventArgsEx> PropertyChanging;
    private void _doOnPropertyChanging(Object sender, PropertyChangingEventArgsEx args) {
      var eve = this.PropertyChanging;
      if (eve != null)
        eve(sender, args);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void _doOnPropertyChanged(Object sender, PropertyChangedEventArgs args) {
      var eve = this.PropertyChanged;
      if (eve != null)
        eve(sender, args);
    }

    private void _addPropertyRow(PropertyItem item, ref int rowIndex) {

      #region Create Display Objects
      PropertyGridLabel label = (item.Hidden) ? null : CreateLabel(item.Name, item.DisplayName);
      PropertyEditor editor = PropertyEditor.GetEditor(label, item);
      this._doOnCustomEditor(label, item, ref editor);
      if (!editor.Initialized) {
        editor.Initialize();
      }


      editor.IsTabStop = false;
      if (null == editor)
        return;
      editor.GotFocus += new RoutedEventHandler(this.Editor_GotFocus);
      #endregion

      rowIndex++;
      this._mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
      String tagValue = item.Category;

      #region Column 0 - Margin
      Border brd = GetItemMargin(tagValue);
      this._mainGrid.Children.Add(brd);
      Grid.SetRow(brd, rowIndex);
      Grid.SetColumn(brd, 0);
      #endregion

      if (label != null) {
        #region Column 1 - Label
        brd = GetItemLabel(label, tagValue);
        this._mainGrid.Children.Add(brd);
        Grid.SetRow(brd, rowIndex);
        Grid.SetColumn(brd, 1);
        #endregion

        #region Column 2 - Editor
        brd = GetItemEditor(editor, tagValue);
        this._mainGrid.Children.Add(brd);
        Grid.SetRow(brd, rowIndex);
        Grid.SetColumn(brd, 2);
        #endregion
      } else {
        #region Column 1 - Editor
        brd = GetItemEditor(editor, tagValue);
        this._mainGrid.Children.Add(brd);
        Grid.SetRow(brd, rowIndex);
        Grid.SetColumn(brd, 1);
        Grid.SetColumnSpan(brd, 2);
        #endregion
      }


      item.PropertyChanging += new EventHandler<PropertyChangingEventArgsEx>(item_PropertyChanging);
      item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
    }

    void item_PropertyChanged(Object sender, PropertyChangedEventArgs args) {
      this._doOnPropertyChanged(sender, args);
    }

    void item_PropertyChanging(Object sender, PropertyChangingEventArgsEx args) {
      this._doOnPropertyChanging(sender, args);
    }

    void AddGridSplitter(int rowCount) {
      GridSplitter gsp = new GridSplitter() {
        IsTabStop = false,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Stretch,
        Background = new SolidColorBrush(Colors.Transparent),
        ShowsPreview = false,
        Width = 2
      };
      Grid.SetColumn(gsp, 2);
      Grid.SetRowSpan(gsp, rowCount);
      Canvas.SetZIndex(gsp, 1);
      _mainGrid.Children.Add(gsp);

    }
    void ToggleCategoryVisible(bool show, string tagValue) {
      foreach (FrameworkElement element in this._mainGrid.Children) {
        object value = element.Tag;
        if (null != value) {
          string tag = (string)value;
          if (tagValue == tag)
            element.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
      }
    }
    void AttachWheelEvents() {
      if (HtmlPage.IsEnabled) {
        HtmlPage.Window.AttachEvent("DOMMouseScroll", OnMouseWheel);
        HtmlPage.Window.AttachEvent("onmousewheel", OnMouseWheel);
        HtmlPage.Document.AttachEvent("onmousewheel", OnMouseWheel);
      }
    }
    void DetachWheelEvents() {
      if (HtmlPage.IsEnabled) {
        HtmlPage.Window.DetachEvent("DOMMouseScroll", OnMouseWheel);
        HtmlPage.Window.DetachEvent("onmousewheel", OnMouseWheel);
        HtmlPage.Document.DetachEvent("onmousewheel", OnMouseWheel);
      }
    }
    Image GetHideImage(Visibility visibility) {
      Image img = GetImage("/Bio.Helpers.Controls.SL;component/SLPGrid/Assets/minus.png");
      img.Visibility = visibility;
      img.MouseLeftButtonUp += new MouseButtonEventHandler(this.CategoryHide_MouseLeftButtonUp);
      img.Cursor = System.Windows.Input.Cursors.Hand;
      return img;
    }
    Image GetShowImage(Visibility visibility) {
      Image img = GetImage("/Bio.Helpers.Controls.SL;component/SLPGrid/Assets/plus.png");
      img.Visibility = visibility;
      img.MouseLeftButtonUp += new MouseButtonEventHandler(this.CategoryShow_MouseLeftButtonUp);
      img.Cursor = System.Windows.Input.Cursors.Hand;
      return img;
    }

    static List<PropertyItem> ParseObject(PropertyGrid grid, Object objItem) {
      if (null == objItem)
        return new List<PropertyItem>();

      List<PropertyItem> pc = new List<PropertyItem>();
      Type t = objItem.GetType();
      var props = t.GetProperties();

      foreach (PropertyInfo pinfo in props) {
        Boolean isBrowsable = true;
        BrowsableAttribute b = PropertyItem.GetAttribute<BrowsableAttribute>(pinfo);
        if (b != null)
          isBrowsable = b.Browsable;

        var args = new OnPreparePropertyEventArgs { Browsable = true };
        grid.doOnPrepareProperty(pinfo, args);
        isBrowsable = isBrowsable && args.Browsable;

        if (isBrowsable) {
          var readOnly = false;
          var required = false;

          // SL3 Only
          var attrRO = PropertyItem.GetAttribute<ReadOnlyAttribute>(pinfo);
          if (attrRO != null)
            readOnly = attrRO.IsReadOnly;
          var attrRQ = PropertyItem.GetAttribute<RequiredAttribute>(pinfo);
          if (attrRQ != null)
            required = attrRQ.IsRequired;


          try {
            Object value = pinfo.GetValue(objItem, null);
            PropertyItem prop = new PropertyItem(grid, objItem, value, pinfo, readOnly, required);
            pc.Add(prop);
          } catch { }
        }
      }

      return pc;
    }
    static PropertyGridLabel CreateLabel(String name, String displayName) {
      var txt = new TextBlock() {
        Text = displayName,
        Margin = new Thickness(0)
      };
      return new PropertyGridLabel() {
        Name = Guid.NewGuid().ToString("N"),
        Content = txt
      };
    }
    static Border GetCategoryMargin(string tagValue, Image hide, Image show) {
      StackPanel stp = new StackPanel() {
        Name = Guid.NewGuid().ToString("N"),
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
      };
      stp.Tag = tagValue;
      stp.Children.Add(hide);
      stp.Children.Add(show);

      Border brd = new Border() { Background = new SolidColorBrush(backgroundColor) };
      brd.Child = stp;

      return brd;
    }
    static Border GetCategoryHeader(string category) {
      TextBlock txt = new TextBlock() {
        Name = Guid.NewGuid().ToString("N"),
        Text = category,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        Foreground = new SolidColorBrush(Colors.Gray),
        Margin = new Thickness(3, 0, 0, 0),
        FontWeight = FontWeights.Bold,
        FontFamily = new FontFamily("Arial Narrow")
      };
      //Expander expdr = new Expander {
      //  Name = Guid.NewGuid().ToString("N"),
      //  Content = category,
      //  VerticalAlignment = VerticalAlignment.Stretch,
      //  HorizontalAlignment = HorizontalAlignment.Stretch,
      //  Foreground = new SolidColorBrush(Colors.Gray),
      //  Margin = new Thickness(3, 0, 0, 0),
      //  FontWeight = FontWeights.Bold,
      //  ExpandDirection = ExpandDirection.Down
      //};
      Border brd = new Border();
      brd.Background = new SolidColorBrush(backgroundColor);
      brd.Child = txt;
      Canvas.SetZIndex(brd, 1);
      return brd;
    }
    static Border GetItemMargin(string tagValue) {
      return new Border() {
        Name = Guid.NewGuid().ToString("N"),
        Margin = new Thickness(0),
        BorderThickness = new Thickness(0),
        Background = new SolidColorBrush(backgroundColor),
        Tag = tagValue,
        Height = 25
      };
    }
    static Border GetItemLabel(PropertyGridLabel label, string tagValue) {
      return new Border() {
        Name = Guid.NewGuid().ToString("N"),
        Margin = new Thickness(0),
        BorderBrush = new SolidColorBrush(backgroundColor),
        BorderThickness = new Thickness(0, 0, 1, 1),
        Child = label,
        Tag = tagValue
      };
    }
    static Border GetItemEditor(PropertyEditor editor, string tagValue) {
      Border brd = new Border() {
        Name = Guid.NewGuid().ToString("N"),
        Margin = new Thickness(1, 0, 0, 0),
        BorderThickness = new Thickness(0, 0, 0, 1),
        BorderBrush = new SolidColorBrush(backgroundColor)
      };
      brd.Child = editor;
      brd.Tag = tagValue;
      return brd;
    }
    static Image GetImage(string imageUri) {
      //
      Image img = new Image() {
        Name = Guid.NewGuid().ToString("N"),
        Source = new BitmapImage(new Uri(imageUri, UriKind.Relative)),
        Height = 9,
        Width = 9,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center
      };
      return img;
    }

    public void CheckRequiredProps() {
      var v_badProps = new List<PropertyItem>();
      foreach (var p in this._props) {
        if (p.Required && (p.Value == null))
          v_badProps.Add(p);
      }
      if (v_badProps.Count > 0)
        throw new RequiredPropsIsNullException(v_badProps);
    }

    #endregion

    #region Event Handlers
    private void PropertyGrid_Loaded(object sender, RoutedEventArgs e) {
      this.MouseEnter += new MouseEventHandler(PropertyGrid_MouseEnter);
      this.MouseLeave += new MouseEventHandler(PropertyGrid_MouseLeave);
    }
    private void Editor_GotFocus(object sender, RoutedEventArgs e) {
      if (null != _selectedEditor)
        _selectedEditor.IsSelected = false;
      _selectedEditor = sender as PropertyEditor;
      if (null != _selectedEditor) {
        _selectedEditor.IsSelected = true;

        //double editorX = ((UIElement)selectedEditor.Parent).RenderTransformOrigin.X;
        //Debug.WriteLine("editorX: " + editorX.ToString());
        //double editorY = ((UIElement)selectedEditor.Parent).RenderTransformOrigin.Y;
        //Debug.WriteLine("editorY: " + editorY.ToString());

        //double thisWidth = this.RenderSize.Width;
        //Debug.WriteLine("thisWidth: " + thisWidth.ToString());
        //double thisHeight = this.RenderSize.Height;
        //Debug.WriteLine("thisHeight: " + thisHeight.ToString());

      }
    }
    private void PropertyGrid_MouseEnter(object sender, MouseEventArgs e) {
      this.AttachWheelEvents();
    }
    private void PropertyGrid_MouseLeave(object sender, MouseEventArgs e) {
      this.DetachWheelEvents();
    }
    private void OnMouseWheel(object sender, HtmlEventArgs args) {
      double mouseDelta = 0;
      ScriptObject e = args.EventObject;

      // Mozilla and Safari    
      if (e.GetProperty("detail") != null) {
        mouseDelta = ((double)e.GetProperty("detail"));
      }

        // IE and Opera    
      else if (e.GetProperty("wheelDelta") != null)
        mouseDelta = ((double)e.GetProperty("wheelDelta"));

      mouseDelta = Math.Sign(mouseDelta);
      mouseDelta = mouseDelta * -1;
      mouseDelta = mouseDelta * 40; // Just a guess at an acceleration
      mouseDelta = this._layoutRoot.VerticalOffset + mouseDelta;
      this._layoutRoot.ScrollToVerticalOffset(mouseDelta);
    }
    private void CategoryHide_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      FrameworkElement ctl = sender as FrameworkElement;
      Panel stp = ctl.Parent as Panel;
      String tagValue = (string)stp.Tag;
      stp.Children[0].Visibility = Visibility.Collapsed;
      stp.Children[1].Visibility = Visibility.Visible;
      this.Dispatcher.BeginInvoke(delegate() {
        ToggleCategoryVisible(false, tagValue);
      });
    }
    private void CategoryShow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      FrameworkElement ctl = sender as FrameworkElement;
      Panel stp = ctl.Parent as Panel;
      string tagValue = (string)stp.Tag;
      stp.Children[0].Visibility = Visibility.Visible;
      stp.Children[1].Visibility = Visibility.Collapsed;
      this.Dispatcher.BeginInvoke(delegate() {
        ToggleCategoryVisible(true, tagValue);
      });
    }
    #endregion
  }
  #endregion
}
