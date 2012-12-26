using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using Bio.Helpers.Common;
using System.Windows.Input;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class DateTimeValueEditor : PropertyEditor {
    #region Fields
    //private DateTime? currentValue;
    private StackPanel pnl;
    private DatePicker dtp;
    private ComboBox cbxHour;
    private ComboBox cbxMin;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="label"></param>
    /// <param name="property"></param>
    public DateTimeValueEditor(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }

    private void _init_cmbxs() {
      var hSource = new List<String>();
      var mSource = new List<String>();
      for (int i = 0; i < 24; i++)
        hSource.Add(String.Format("{0:00}", i));
      this.cbxHour.ItemsSource = hSource;
      for (int i = 0; i < 6; i++)
        mSource.Add(String.Format("{0:00}", i * 10));
      this.cbxMin.ItemsSource = mSource;
    }

    protected override void initialize() {
      base.initialize();
      //this.currentValue = this.Property.Value as DateTime?;
      this.Property.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.property_PropertyChanged);
      this.Property.ValueError += new EventHandler<ExceptionEventArgs>(this.property_ValueError);

      this.pnl = new StackPanel { 
        Orientation = System.Windows.Controls.Orientation.Horizontal,
        FlowDirection = System.Windows.FlowDirection.LeftToRight
      };
      this.Content = pnl;

      this.dtp = new DatePicker {
        Visibility = Visibility.Visible,
        Margin = new Thickness(1, 0, 0, 0),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        Width = 120,
        IsEnabled = this.Property.CanWrite
      };
      this.pnl.Children.Add(this.dtp);
      this.cbxHour = new ComboBox {
        Visibility = Visibility.Visible,
        Margin = new Thickness(2,0,0,0),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        Width = 50,
        IsEnabled = this.Property.CanWrite,
        Cursor = Cursors.Hand
      };
      this.pnl.Children.Add(this.cbxHour);
      this.cbxMin = new ComboBox {
        Visibility = Visibility.Visible,
        Margin = new Thickness(2, 0, 0, 0),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        Width = 50,
        IsEnabled = this.Property.CanWrite,
        Cursor = Cursors.Hand
      };
      this.pnl.Children.Add(this.cbxMin);
      this._init_cmbxs();

      this.refreshControlsValue();

      this.dtp.LostFocus += new RoutedEventHandler(this.dtp_LostFocus);
      this.dtp.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(this.dtp_SelectedDateChanged);
      this.cbxHour.SelectionChanged += new SelectionChangedEventHandler(this.cbxTime_SelectionChanged);
      this.cbxMin.SelectionChanged += new SelectionChangedEventHandler(this.cbxTime_SelectionChanged);
      this.dtp.Focus();
    }

    #endregion

    #region Overrides
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnGotFocus(RoutedEventArgs e) {
      Debug.WriteLine("DateTimeValueEditor : OnGotFocus");
      base.OnGotFocus(e);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLostFocus(RoutedEventArgs e) {
      Debug.WriteLine("DateTimeValueEditor : OnLostFocus");
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
        if (this.Property.Value != null) {
          var v_date = (DateTime)this.Property.Value;
          var vHH = v_date.ToString("HH");
          var vMI = v_date.ToString("mm").Substring(0, 1) + "0";
          this.cbxHour.SelectedItem = vHH;
          this.cbxMin.SelectedItem = vMI;
        }
      } finally {
        this._enableCnrlsChangedEvents();
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
          this.cbxHour.IsEnabled = this.Property.CanWrite;
          this.cbxMin.IsEnabled = this.Property.CanWrite;
        }
      }
    }

    private void refreshPropertyValue() {
      if ((this.dtp.SelectedDate != null) && 
            (this.cbxHour.SelectedValue != null) && 
              (this.cbxMin.SelectedValue != null)) {
        var v_redy_date = (DateTime)this.dtp.SelectedDate;
        var v_redy_date_str = v_redy_date.ToString("yyyy.MM.dd") + " " +
            this.cbxHour.SelectedValue + ":" + this.cbxMin.SelectedValue + ":00";
        this._disablePropertyChangedEvents();
        try {
          this.Property.Value = Utl.Convert2Type<DateTime>(v_redy_date_str);
        } finally {
          this._enablePropertyChangedEvents();
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
    void cbxTime_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (this._cnrlsChangedEnabled)
        this.refreshPropertyValue();
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
