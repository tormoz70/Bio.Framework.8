using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class UInt32Converter : BaseNumberConverter {
    // Methods
    internal override object FromString(string value, CultureInfo culture) {
      return uint.Parse(value, culture);
    }

    internal override object FromString(string value, NumberFormatInfo formatInfo) {
      return uint.Parse(value, NumberStyles.Integer, formatInfo);
    }

    internal override object FromString(string value, int radix) {
      return Convert.ToUInt32(value, radix);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      uint num = (uint)value;
      return num.ToString("G", formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(uint);
      }
    }
  }
}
