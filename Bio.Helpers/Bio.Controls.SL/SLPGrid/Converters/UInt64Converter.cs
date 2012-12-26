using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class UInt64Converter : BaseNumberConverter {
    // Methods
    internal override object FromString(string value, CultureInfo culture) {
      return ulong.Parse(value, culture);
    }

    internal override object FromString(string value, NumberFormatInfo formatInfo) {
      return ulong.Parse(value, NumberStyles.Integer, formatInfo);
    }

    internal override object FromString(string value, int radix) {
      return Convert.ToUInt64(value, radix);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      ulong num = (ulong)value;
      return num.ToString("G", formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(ulong);
      }
    }
  }
}
