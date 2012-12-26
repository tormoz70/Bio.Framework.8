using System;
using System.ComponentModel;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid.Converters {
  public class TypeConverterHelper {
    public static TypeConverter GetConverter(Type type) {
      TypeConverter converter = null;
      converter = GetCoreConverterFromCoreType(type);
      if (converter == null)
        converter = GetCoreConverterFromCustomType(type);
      return converter;
    }

    private static TypeConverter GetCoreConverterFromCoreType(Type type) {
      Type v_type = Nullable.GetUnderlyingType(type);
      Boolean v_isNullable = v_type != null;
      v_type = v_type ?? type;

      TypeConverter converter = null;
      if (v_type == typeof(Int32)) {
        return new Int32Converter();
      }
      if (v_type == typeof(Int16)) {
        return new Int16Converter();
      }
      if (v_type == typeof(Int64)) {
        return new Int64Converter();
      }
      if (v_type == typeof(UInt32)) {
        return new UInt32Converter();
      }
      if (v_type == typeof(UInt16)) {
        return new UInt16Converter();
      }
      if (v_type == typeof(UInt64)) {
        return new UInt64Converter();
      }
      if (v_type == typeof(Boolean)) {
        return new BooleanConverter();
      }
      if (v_type == typeof(Double)) {
        return new DoubleConverter();
      }
      if (v_type == typeof(float)) {
        return new SingleConverter();
      }
      if (v_type == typeof(Byte)) {
        return new ByteConverter();
      }
      if (v_type == typeof(SByte)) {
        return new SByteConverter();
      }
      if (v_type == typeof(Char)) {
        return new CharConverter();
      }
      if (v_type == typeof(Decimal)) {
        return new DecimalConverter();
      }
      if (v_type == typeof(TimeSpan)) {
        return new TimeSpanConverter();
      }
      if (v_type == typeof(Guid)) {
        return new GuidConverter();
      }
      if (v_type == typeof(String)) {
        return new StringConverter();
      }
      //if (type == typeof(CultureInfo))
      //{
      //    return new CultureInfoConverter();
      //}
      //if (type == typeof(Type))
      //{
      //    return new TypeTypeConverter();
      //}
      //if (type == typeof(DateTime))
      //{
      //    return new DateTimeConverter2();
      //}
      //if (ReflectionHelper.IsNullableType(type))
      //{
      //    converter = new NullableConverter(type);
      //}
      return converter;
    }


    private static TypeConverter GetCoreConverterFromCustomType(Type type) {
      Type v_type = Nullable.GetUnderlyingType(type);
      Boolean v_isNullable = v_type != null;
      v_type = v_type ?? type;
      TypeConverter converter = null;
      //if (type.IsEnum)
      //{
      //    return new EnumConverter(type);
      //}
      if (typeof(Int32).IsAssignableFrom(v_type)) {
        return new Int32Converter();
      }
      if (typeof(Int16).IsAssignableFrom(v_type)) {
        return new Int16Converter();
      }
      if (typeof(Int64).IsAssignableFrom(v_type)) {
        return new Int64Converter();
      }
      if (typeof(UInt32).IsAssignableFrom(v_type)) {
        return new UInt32Converter();
      }
      if (typeof(UInt16).IsAssignableFrom(v_type)) {
        return new UInt16Converter();
      }
      if (typeof(UInt64).IsAssignableFrom(v_type)) {
        return new UInt64Converter();
      }
      if (typeof(Boolean).IsAssignableFrom(v_type)) {
        return new BooleanConverter();
      }
      if (typeof(Double).IsAssignableFrom(v_type)) {
        return new DoubleConverter();
      }
      if (typeof(float).IsAssignableFrom(v_type)) {
        return new SingleConverter();
      }
      if (typeof(Byte).IsAssignableFrom(v_type)) {
        return new ByteConverter();
      }
      if (typeof(SByte).IsAssignableFrom(v_type)) {
        return new SByteConverter();
      }
      if (typeof(Char).IsAssignableFrom(v_type)) {
        return new CharConverter();
      }
      if (typeof(Decimal).IsAssignableFrom(v_type)) {
        return new DecimalConverter();
      }
      if (typeof(TimeSpan).IsAssignableFrom(v_type)) {
        return new TimeSpanConverter();
      }
      if (typeof(Guid).IsAssignableFrom(v_type)) {
        return new GuidConverter();
      }
      if (typeof(String).IsAssignableFrom(v_type)) {
        return new StringConverter();
      }
      //if (typeof(CultureInfo).IsAssignableFrom(type))
      //{
      //    return new CultureInfoConverter();
      //}
      //if (typeof(Type).IsAssignableFrom(type))
      //{
      //    return new TypeTypeConverter();
      //}
      //if (typeof(DateTime).IsAssignableFrom(type))
      //{
      //    converter = new DateTimeConverter2();
      //}
      return converter;
    }







  }
}
