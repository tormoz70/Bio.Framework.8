using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class Int64Converter : BaseNumberConverter {
    // Methods
    internal override object FromString(string value, CultureInfo culture) {
      return long.Parse(value, culture);
    }

    internal override object FromString(string value, NumberFormatInfo formatInfo) {
      return long.Parse(value, NumberStyles.Integer, formatInfo);
    }

    internal override object FromString(string value, int radix) {
      return Convert.ToInt64(value, radix);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      long num = (long)value;
      return num.ToString("G", formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(long);
      }
    }
  }
}
