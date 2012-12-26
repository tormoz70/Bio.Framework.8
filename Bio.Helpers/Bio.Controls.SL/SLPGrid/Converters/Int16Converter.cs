using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class Int16Converter : BaseNumberConverter {
    // Methods
    internal override object FromString(string value, CultureInfo culture) {
      return short.Parse(value, culture);
    }

    internal override object FromString(string value, NumberFormatInfo formatInfo) {
      return short.Parse(value, NumberStyles.Integer, (IFormatProvider)formatInfo);
    }

    internal override object FromString(string value, int radix) {
      return Convert.ToInt16(value, radix);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      short num = (short)value;
      return num.ToString("G", (IFormatProvider)formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(short);
      }
    }
  }
}
