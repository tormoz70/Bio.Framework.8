using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class DateValueEditor : PropertyEditor {
    #region Fields
    //DateTime? currentValue;
    //Boolean showingDTP;
    //StackPanel pnl;
    //protected TextBox txt;
    protected DatePicker dtp;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="label"></param>
    /// <param name="property"></param>
    public DateValueEditor(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }
    protected override void initialize() {
      base.initialize();
      //this.currentValue = this.Property.Value as DateTime?;
      this.Property.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.property_PropertyChanged);
      this.Property.ValueError += new EventHandler<ExceptionEventArgs>(this.property_ValueError);

      //this.pnl = new StackPanel();
      //this.Content = pnl;

      this.dtp = new DatePicker() {
        Visibility = Visibility.Visible,
        Margin = new Thickness(1),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        //SelectedDate = this.currentValue,
        IsEnabled = this.Property.CanWrite
      };
      this.Content = this.dtp;
      this.refreshControlsValue();

      this.dtp.LostFocus += new RoutedEventHandler(this.dtp_LostFocus);
      this.dtp.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(this.dtp_SelectedDateChanged);
      this.dtp.Focus();
    }

    #endregion

    #region Overrides
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnGotFocus(RoutedEventArgs e) {
      Debug.WriteLine("DateValueEditor : OnGotFocus");
      base.OnGotFocus(e);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLostFocus(RoutedEventArgs e) {
      Debug.WriteLine("DateValueEditor : OnLostFocus");
      base.OnLostFocus(e);
    }
    #endregion

    #region Methods
    #endregion

    #region Event Handlers
    void property_ValueError(Object sender, ExceptionEventArgs e) {
      MessageBox.Show(e.EventException.Message);
    }

    private void refreshControlsValue() {
      this._disableCnrlsChangedEvents();
      try {
        this.dtp.SelectedDate = (this.Property.Value != null) ? (DateTime?)this.Property.Value : null;
      } finally {
        this._enableCnrlsChangedEvents();
      }
    }

    private void refreshPropertyValue() {
      this._disablePropertyChangedEvents();
      try {
        this.Property.Value = this.dtp.SelectedDate;
      } finally {
        this._enablePropertyChangedEvents();
      }
    }

    private Boolean _propertyChangedEnabled = true;
    private void _disablePropertyChangedEvents() { this._propertyChangedEnabled = false; }
    private void _enablePropertyChangedEvents() { this._propertyChangedEnabled = true; }
    void property_PropertyChanged(Object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      if (this._propertyChangedEnabled) {
        if (e.PropertyName == "Value") {
          this.refreshControlsValue();
        }
        if (e.PropertyName == "CanWrite") {
          this.dtp.IsEnabled = this.Property.CanWrite;
        }
      }
    }
    private Boolean _cnrlsChangedEnabled = true;
    private void _disableCnrlsChangedEvents() { this._cnrlsChangedEnabled = false; }
    private void _enableCnrlsChangedEvents() { this._cnrlsChangedEnabled = true; }
    void dtp_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
      if (this._cnrlsChangedEnabled) {
        this.refreshPropertyValue();
      }
    }
    void dtp_CalendarOpened(object sender, RoutedEventArgs e) {
    }
    void dtp_CalendarClosed(object sender, RoutedEventArgs e) {
    }
    void dtp_LostFocus(object sender, RoutedEventArgs e) {
      //this.currentValue = this.dtp.SelectedDate;
      //this.Property.Value = this.currentValue;
      //if (this.dtp.IsDropDownOpen)
      //  return;
    }
    #endregion
  }
}
