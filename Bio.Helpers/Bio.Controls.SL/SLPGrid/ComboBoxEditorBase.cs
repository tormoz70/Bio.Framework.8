
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Input;
  using System.Linq;
using Bio.Helpers.Common;
  using System.Collections;
  #endregion

  #region ComboBoxEditorBase
  /// <summary>
  /// An editor for a Boolean Type
  /// </summary>
  public abstract class ComboBoxEditorBase : PropertyEditor {
    #region Fields
    //Object currentValue;
    //Boolean showingCBO;
    //StackPanel pnl;
    //protected TextBox txt;
    protected ComboBox cbo;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="label"></param>
    /// <param name="property"></param>
    public ComboBoxEditorBase(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }
    #endregion

    protected override void initialize() {
      base.initialize();
      this.Property.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.property_PropertyChanged);
      //this.Property.ValueError += new EventHandler<ExceptionEventArgs>(this.property_ValueError);

      this.cbo = new ComboBox() {
        Visibility = Visibility.Visible,
        Margin = new Thickness(0),
        Cursor = Cursors.Hand,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsEnabled = this.Property.CanWrite,
        SelectedValuePath = "Value",
        DisplayMemberPath = "Name"

      };
      this.cbo.SelectionChanged += new SelectionChangedEventHandler(this.cbo_SelectionChanged);
      this._setCboSelectedItem(this.Property.Value);

      this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
      this.Content = this.cbo;
    }

    private Object _itemByValue(Object value) {
      if((value != null) && (this.cbo.Items.Count > 0)){
        //this.cbo.SelectedItem = this.currentValue;
        if (this.cbo.Items.First() is EnumWrapper) {
          var rslt = this.cbo.Items.Cast<EnumWrapper>().FirstOrDefault((itm) => {
            return value.Equals(itm.Value); 
          });
          return rslt;
        } else
          return this.cbo.Items.First();
      }else
        return null;
    }
    private Object _valueOfItem(Object item) {
      if ((item != null) && (item is EnumWrapper))
        return (item as EnumWrapper).Value;
      else
        return item;
    }

    #region Methods
    protected virtual void LoadItems(IEnumerable items) {
      Boolean v_selectionChangedEnabled = this._selectionChangedEnabled;
      this._selectionChangedEnabled = false;
      try {
        //this.cbo.ItemsSource = items;//.Cast<EnumWrapper>();
        this.cbo.Items.Clear();
        foreach (var item in items)
          this.cbo.Items.Add(new EnumWrapper {
            Name = (item as EnumWrapper).Name,
            Value = (item as EnumWrapper).Value
          });
      } finally {
        this._selectionChangedEnabled = v_selectionChangedEnabled;
      }
    }
    #endregion

    protected virtual Boolean setCboSelectedItem(Object value) {
      Object v_foundVal = this._itemByValue(value);
      if (v_foundVal != null) {
        this.cbo.SelectedValue = value;
        return true;
      } else
        return false;
    }

    private void _setCboSelectedItem(Object value) {
      this._selectionChangedEnabled = false;
      try {
        this.setCboSelectedItem(value);
      } finally {
        this._selectionChangedEnabled = true;
      }
    }

    #region Event Handlers
    private void property_ValueError(Object sender, ExceptionEventArgs e) {
      MessageBox.Show(e.EventException.Message);
    }

    private Boolean _propertyChangedEnabled = true;
    private void property_PropertyChanged(Object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      if (this._propertyChangedEnabled) {
        if (e.PropertyName == "Value") {
          //this.currentValue = this.Property.Value;
          this._setCboSelectedItem(this.Property.Value);
          //this.cbo.SelectedItem = this._itemByValue(this.currentValue);
        }
        if (e.PropertyName == "CanWrite") {
          this.cbo.IsEnabled = this.Property.CanWrite;
        }
      }
    }


    private Boolean _selectionChangedEnabled = true;
    private void cbo_SelectionChanged(Object sender, SelectionChangedEventArgs e) {
      if (this._selectionChangedEnabled) {
        Object v_currentValue = this._valueOfItem(e.AddedItems.Cast<Object>().FirstOrDefault());
        this._propertyChangedEnabled = false;
        try {
          this.Property.Value = v_currentValue;
          this._setCboSelectedItem(this.Property.Value);
          //this.cbo.UpdateLayout();
        } finally {
          this._propertyChangedEnabled = true;
        }
      }
    }
    //private void cbo_DropDownOpened(Object sender, EventArgs e) {
    //  this.showingCBO = true;
    //}
    //private void cbo_LostFocus(Object sender, RoutedEventArgs e) {
    //  if (this.cbo.IsDropDownOpen)
    //    return;
    //  //ShowTextBox();
    //}
    #endregion
  }
  #endregion
}
