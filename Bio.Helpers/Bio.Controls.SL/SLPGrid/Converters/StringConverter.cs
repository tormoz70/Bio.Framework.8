using System;
using System.ComponentModel;
using System.Globalization;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class StringConverter : TypeConverter {
    // Methods
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
      return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
      if (value is string) {
        return (string)value;
      }
      if (value == null) {
        return "";
      }
      return base.ConvertFrom(context, culture, value);
    }
  }
}
