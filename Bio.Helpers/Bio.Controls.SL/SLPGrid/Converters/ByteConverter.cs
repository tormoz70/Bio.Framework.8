using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class ByteConverter : BaseNumberConverter {
    // Methods
    internal override Object FromString(String value, CultureInfo culture) {
      return !String.IsNullOrEmpty(value) ? Byte.Parse(value, culture) : Byte.Parse("0", culture);
    }

    internal override Object FromString(String value, NumberFormatInfo formatInfo) {
      return !String.IsNullOrEmpty(value) ? Byte.Parse(value, NumberStyles.Integer, (IFormatProvider)formatInfo) : Byte.Parse("0", NumberStyles.Integer, (IFormatProvider)formatInfo);
    }

    internal override Object FromString(String value, Int32 radix) {
      return !String.IsNullOrEmpty(value) ? Convert.ToByte(value, radix) : Convert.ToByte("0", radix);
    }

    internal override String ToString(Object value, NumberFormatInfo formatInfo) {
      byte num = (Byte)value;
      return num.ToString("G", formatInfo);
    }

    // Properties
    internal override Type TargetType {
      get {
        return typeof(Byte);
      }
    }
  }
}
