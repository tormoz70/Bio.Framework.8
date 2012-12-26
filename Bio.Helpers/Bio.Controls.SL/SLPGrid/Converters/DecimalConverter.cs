using System;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class DecimalConverter : BaseNumberConverter {

    internal override object FromString(String value, CultureInfo culture) {
      return !String.IsNullOrEmpty(value) ? Decimal.Parse(value, culture) : Convert.ToDecimal(0, CultureInfo.CurrentCulture);
    }

    internal override object FromString(String value, NumberFormatInfo formatInfo) {
      return !String.IsNullOrEmpty(value) ? Decimal.Parse(value, NumberStyles.Float, formatInfo) : Convert.ToDecimal(0, formatInfo);
    }

    internal override object FromString(String value, Int32 radix) {
      return !String.IsNullOrEmpty(value) ? Convert.ToDecimal(value, CultureInfo.CurrentCulture) : Convert.ToDecimal(0, CultureInfo.CurrentCulture);
    }

    internal override string ToString(object value, NumberFormatInfo formatInfo) {
      Decimal num = (Decimal)value;
      return num.ToString("G", formatInfo);
    }

    // Properties
    internal override bool AllowHex {
      get {
        return false;
      }
    }

    internal override Type TargetType {
      get {
        return typeof(Decimal);
      }
    }
  }
}
