
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System.Windows.Controls;
  using System;
  using System.Windows;
  using System.Windows.Input;

  #endregion

  #region BooleanValueEditor
  /// <summary>
  /// An editor for a Boolean Type
  /// </summary>
  public class BooleanValueEditor : PropertyEditor {
    private Boolean? currentValue;
    private CheckBox cbx;

    public BooleanValueEditor(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }

    protected override void initialize() {
      base.initialize();
      this.currentValue = this.Property.Value as Boolean?;
      this.Property.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(this.property_PropertyChanged);
      this.Property.ValueError += new EventHandler<ExceptionEventArgs>(this.property_ValueError);

      this.cbx = new CheckBox() {
        Visibility = Visibility.Visible,
        //Margin = new Thickness(1),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalContentAlignment = VerticalAlignment.Center,
        Margin = new Thickness(1, 4, 0, 0),
        IsChecked = this.currentValue,
        IsEnabled = this.Property.CanWrite,
        Cursor = Cursors.Hand
      };
      this.Content = this.cbx;
      this.cbx.Checked += new RoutedEventHandler(this.cbx_CheckboxChanged);
      this.cbx.Unchecked += new RoutedEventHandler(this.cbx_CheckboxChanged);
      //this.dtp.CalendarClosed += new RoutedEventHandler(dtp_CalendarClosed);
      //this.dtp.LostFocus += new RoutedEventHandler(dtp_LostFocus);
      //this.pnl.Children.Add(dtp);
      this.cbx.Focus();
    }

    void cbx_CheckboxChanged(Object sender, RoutedEventArgs e) {
      this.currentValue = (sender as CheckBox).IsChecked;
      this.Property.Value = this.currentValue;
    }

    void property_ValueError(Object sender, ExceptionEventArgs e) {
      MessageBox.Show(e.EventException.Message);
    }
    void property_PropertyChanged(Object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      if (e.PropertyName == "Value") {
        this.currentValue = this.Property.Value as Boolean?;
        this.cbx.IsChecked = this.currentValue;
      }
      if (e.PropertyName == "CanWrite") {
        this.cbx.IsEnabled = this.Property.CanWrite;
      }
    }

  }
  #endregion
}
