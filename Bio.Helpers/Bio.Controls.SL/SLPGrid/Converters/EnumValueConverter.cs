using System;
using System.Globalization;
using System.Windows.Data;
using Bio.Helpers.Common;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class EnumValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return enumHelper.GetValueWrapped(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return ((EnumWrapper)value).Value;
    }
  }
}
