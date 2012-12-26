using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class DoubleConverter : BaseNumberConverter {
    // Methods
    internal override Object FromString(String value, CultureInfo culture) {
      return !String.IsNullOrEmpty(value) ? Double.Parse(value, culture) : Double.Parse("0", culture);
    }

    internal override Object FromString(String value, NumberFormatInfo formatInfo) {
      return !String.IsNullOrEmpty(value) ? Double.Parse(value, NumberStyles.Float, (IFormatProvider)formatInfo) : Double.Parse("0", NumberStyles.Float, (IFormatProvider)formatInfo);
    }

    internal override Object FromString(String value, int radix) {
      return !String.IsNullOrEmpty(value) ? Convert.ToDouble(value, CultureInfo.CurrentCulture) : Convert.ToDouble(0, CultureInfo.CurrentCulture);
    }

    internal override String ToString(Object value, NumberFormatInfo formatInfo) {
      Double num = (Double)value;
      return num.ToString("R", formatInfo);
    }

    // Properties
    internal override Boolean AllowHex {
      get {
        return false;
      }
    }

    internal override Type TargetType {
      get {
        return typeof(Double);
      }
    }
  }
}
