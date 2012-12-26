using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bio.Helpers.Controls.SL {
  public class CTextEditField : Control {
    public CTextEditField() {
      this.DefaultStyleKey = typeof(CTextEditField);
    }
    public static DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(String), typeof(CTextEditField), new PropertyMetadata("Caption"));
    public String Caption {
      get { return (String)this.GetValue(CaptionProperty); }
      set { this.SetValue(CaptionProperty, value); }
    }
    public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(CTextEditField), new PropertyMetadata("Value"));
    public String Text {
      get { return (String)this.GetValue(TextProperty); }
      set { this.SetValue(TextProperty, value); }
    }
  }
}