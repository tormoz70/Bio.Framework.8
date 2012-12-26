using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class Int32Converter : BaseNumberConverter {
    // Methods
    internal override object FromString(string value, CultureInfo culture) {
      return int.Parse(value, culture);
    }

    internal override object FromString(string value, NumberFormatInfo formatInfo) {
      return int.Parse(value, NumberStyles.Integer, formatInfo);
    }

    internal override object FromString(string value, int radix) {
      return Convert.ToInt32(value, radix);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      int num = (int)value;
      return num.ToString("G", formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(int);
      }
    }
  }
}
