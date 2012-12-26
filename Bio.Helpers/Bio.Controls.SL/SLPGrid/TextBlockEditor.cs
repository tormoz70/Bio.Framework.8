using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class TextBlockEditor : PropertyEditor {
    TextBox txt;

    public TextBlockEditor(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }

    protected override void initialize() {
      base.initialize();
      if (this.Property.PropertyType == typeof(Char)) {
        if ((char)this.Property.Value == '\0')
          this.Property.Value = "";
      }

      this.Property.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(property_PropertyChanged);
      this.Property.ValueError += new EventHandler<ExceptionEventArgs>(property_ValueError);


      txt = new TextBox();
      //txt.Height = 20;
      if (null != this.Property.Value)
        txt.Text = this.Property.Value.ToString();
      txt.IsReadOnly = true;
      txt.TextWrapping = TextWrapping.Wrap;
      //txt.Foreground = this.Property.CanWrite ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);
      txt.BorderThickness = new Thickness(1);
      txt.Margin = new Thickness(1);
      txt.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
      txt.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;
      txt.MaxHeight = 150;
      txt.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      txt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

      //if (this.Property.CanWrite)
      //  txt.TextChanged += new TextChangedEventHandler(Control_TextChanged);

      this.Content = txt;
      this.GotFocus += new RoutedEventHandler(StringValueEditor_GotFocus);
    }

    void property_ValueError(object sender, ExceptionEventArgs e) {
      MessageBox.Show(e.EventException.Message);
    }
    void property_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
      if (e.PropertyName == "Value") {
        if (this.Property.Value != null)
          txt.Text = this.Property.Value.ToString();
        else
          txt.Text = string.Empty;
      }

      if (e.PropertyName == "CanWrite") {
        //if (!this.Property.CanWrite)
        //  txt.TextChanged -= new TextChangedEventHandler(Control_TextChanged);
        //else
        //  txt.TextChanged += new TextChangedEventHandler(Control_TextChanged);
        //txt.IsReadOnly = !this.Property.CanWrite;
        //txt.Foreground = this.Property.CanWrite ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);
      }
    }

    void StringValueEditor_GotFocus(object sender, RoutedEventArgs e) {
      this.txt.Focus();
    }

    //void Control_TextChanged(object sender, TextChangedEventArgs e) {
    //  if (this.Property.CanWrite)
    //    this.Property.Value = txt.Text;
    //}
  }
}
