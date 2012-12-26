using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Bio.Helpers.Controls.SL.SLPropertyGrid;
using System.Threading;
using System.Windows.Input;
using Bio.Helpers.Common.Types;
using System.ComponentModel;
using Bio.Helpers.Common;

namespace Bio.Framework.Client.SL.JSPropertyGrid {

  /// <summary>
  /// Контрол позволяет вызывать плагин ISelector для выбора значения.
  /// </summary>
  public class SelectValueEditor : ASelectorValueControl {
    
    //private SelectValueEditorAttribute _attrs = null;

    /// <summary>
    /// Данный конструктор используется в динамических PropertyGrid, на событии OnCustomEditor
    /// </summary>
    /// <param name="label"></param>
    /// <param name="property"></param>
    /// <param name="attrs"></param>
    public SelectValueEditor(PropertyGridLabel label, PropertyItem property, SelectValueEditorAttribute attrs)
      : base(label, property) {
      var v_attrs = attrs ?? this.Property.GetAttribute<SelectValueEditorAttribute>();
      this.SelectorPlugin = v_attrs.SelectorPluginName;
      this.DisplayField = v_attrs.DisplayFieldName;
      this.ValueField = v_attrs.ValueFieldName;
      this.IsMultiselector = v_attrs.isMultiselector;
      
    }

    /// <summary>
    /// Данный конструктор используется в статических PropertyGrid
    /// </summary>
    /// <param name="label"></param>
    /// <param name="property"></param>
    public SelectValueEditor(PropertyGridLabel label, PropertyItem property)
      : this(label, property, null) {
    }

    protected override void _onSelectionChanged() {
      this._disablePropertyChanged();
      try {
        this.Property.SetSelection(this._selection);
        this._txt.Text = this._selection.Display;
      } finally {
        this._enablePropertyChanged();
      }
    }

    /// <summary>
    /// Инициализация контрола
    /// </summary>
    protected override void initialize() {
      base.initialize();
      this._ownerPlugin = (this.Property.Owner as CJSPropertyGrid).OwnerPlugin;
      if (this.Property.PropertyType == typeof(Char)) {
        if ((char)this.Property.Value == '\0')
          this.Property.Value = "";
      }

      this.Property.PropertyChanged += new PropertyChangedEventHandler(property_PropertyChanged);
      this.Property.ValueError += new EventHandler<ExceptionEventArgs>(property_ValueError);

      this._txt = new TextBox();
      this._txt.IsReadOnly = true; //!this.Property.CanWrite;
      this._txt.Foreground = this.Property.CanWrite ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);
      this._txt.BorderThickness = new Thickness(1);
      this._txt.Margin = new Thickness(1);
      this._txt.VerticalAlignment = VerticalAlignment.Stretch;
      this._txt.VerticalContentAlignment = VerticalAlignment.Center;
      this._txt.HorizontalAlignment = HorizontalAlignment.Stretch;

      this._btn = new Button();
      this._btn.IsEnabled = this.Property.CanWrite;
      this._btn.Content = "...";
      this._btn.VerticalAlignment = VerticalAlignment.Stretch;
      this._btn.HorizontalAlignment = HorizontalAlignment.Stretch;
      this._btn.Cursor = Cursors.Hand;
      this._btn.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
        this._showSelector();
      });

      var grd = new Grid();
      grd.HorizontalAlignment = HorizontalAlignment.Stretch;
      grd.VerticalAlignment = VerticalAlignment.Stretch;
      grd.ColumnDefinitions.Add(new ColumnDefinition());
      grd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30D) });
      grd.Children.Add(_txt); Grid.SetColumn(_txt, 0);
      grd.Children.Add(_btn); Grid.SetColumn(_btn, 1);

      this.Content = grd;
      this.GotFocus += new RoutedEventHandler(StringValueEditor_GotFocus);

      this._get_selector((selector) => {
        if (this.IsMultiselector) {
          this._selection = new VMultiSelection {
            Value = this.Property.Value
          };
        } else {
          this._selection = new VSingleSelection {
            ValueField = this.ValueField,
            Value = this.Property.Value,
            DisplayField = this.DisplayField,
            Display = null
          };
        }
        this._txt.Text = VSelection.csNotSeldText;
        if (!this._selection.IsEmpty())
          this._setSelection(this._selection);
      });

    }

    void property_ValueError(object sender, ExceptionEventArgs e) {
      Utl.UiThreadInvoke(() => {
        MessageBox.Show(e.EventException.Message);
      });
    }

    private Boolean _propertyChangedDisabled = false;
    private void _disablePropertyChanged() { this._propertyChangedDisabled = true; }
    private void _enablePropertyChanged() { this._propertyChangedDisabled = false; }
    void property_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (!this._propertyChangedDisabled) {
        if (e.PropertyName == "Value") {
          if (this.Property.Value != null) {
            this._selection.Value = this.Property.Value;
            this._setSelection(this._selection);
          }
        }

        if (e.PropertyName == "CanWrite") {
          this._btn.IsEnabled = this.Property.CanWrite;
          this._txt.Foreground = this.Property.CanWrite ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);
        }
      }
    }

    void StringValueEditor_GotFocus(object sender, RoutedEventArgs e) {
      this._btn.Focus();
    }


    public override object Value {
      get {
        return this._selection.Value;
      }
      set {
      }
    }
  }
}
