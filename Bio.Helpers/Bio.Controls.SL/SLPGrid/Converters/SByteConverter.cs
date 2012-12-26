using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class SByteConverter : BaseNumberConverter {
    // Methods
    internal override object FromString(string value, CultureInfo culture) {
      return sbyte.Parse(value, culture);
    }

    internal override object FromString(string value, NumberFormatInfo formatInfo) {
      return sbyte.Parse(value, NumberStyles.Integer, (IFormatProvider)formatInfo);
    }

    internal override object FromString(string value, int radix) {
      return Convert.ToSByte(value, radix);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      sbyte num = (sbyte)value;
      return num.ToString("G", (IFormatProvider)formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(sbyte);
      }
    }
  }
}
