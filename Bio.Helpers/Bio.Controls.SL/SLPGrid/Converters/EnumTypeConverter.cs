using System;
using System.Globalization;
using System.Windows.Data;
using Bio.Helpers.Common;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class EnumTypeConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return enumHelper.GetValues(value.GetType());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotSupportedException();
    }

  }
}
