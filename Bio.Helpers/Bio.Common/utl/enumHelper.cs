using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Collections.ObjectModel;


namespace Bio.Helpers.Common {
  public static class enumHelper {
#if SILVERLIGHT
    private static Dictionary<Type, IEnumerable> _enumCache = new Dictionary<Type, IEnumerable>();

    public static IEnumerable<T> GetValues<T>() {
      Type enumType = typeof(T);

      if (!enumType.IsEnum) {
        throw new ArgumentException("Type '" + enumType.Name + "' is not an enum");
      }

      List<T> values = new List<T>();

      var fields = from field in enumType.GetFields()
                   where field.IsLiteral
                   select field;

      foreach (FieldInfo field in fields) {
        object value = field.GetValue(enumType);
        values.Add((T)value);
      }

      return values;
    }
    public static IEnumerable GetValues(Type enumType) {
      if (!enumType.IsEnum)
        throw new ArgumentException("Type '" + enumType.Name + "' is not an enum");

      List<Object> values = new List<Object>();

      var fields = from field in enumType.GetFields()
                   where field.IsLiteral
                   select field;

      foreach (FieldInfo field in fields) {
        Object value = field.GetValue(enumType);
        values.Add(value);
      }

      return values;
    }
    public static IEnumerable GetValuesWrapped(Type enumType) {
      if (!enumType.IsEnum) {
        throw new ArgumentException("Type '" + enumType.Name + "' is not an enum");
      }
      if (_enumCache.ContainsKey(enumType))
        return _enumCache[enumType];

      List<EnumWrapper> values = new List<EnumWrapper>();

      var fields = from field in enumType.GetFields()
                   where field.IsLiteral
                   select field;

      foreach (FieldInfo field in fields) {
        Object value = field.GetValue(enumType);
        //values.Add(value);
        values.Add(new EnumWrapper { Name = enumHelper.GetAttributeByValue<HeaderContentAttribute>(value).Text, Value = value });
      }
      //EnumWrapper[] ret = values.ToArray();
      _enumCache.Add(enumType, values);
      return values;
    }

    public static EnumWrapper GetValueWrapped(Object o) {
      Type enumType = o.GetType();
      if (!enumType.IsEnum) {
        throw new ArgumentException("Type '" + enumType.Name + "' is not an enum");
      }

      IEnumerable values = GetValuesWrapped(enumType);
      EnumWrapper v = values.Cast<EnumWrapper>().FirstOrDefault(ew => ew.Value.Equals(o));
      return v;
    }

    //public static FieldInfo GetFieldInfo<T>(T val) {
    //  String valStr = Enum.GetName(typeof(T), val);
    //  FieldInfo[] fia = typeof(T).GetFields();
    //  foreach (FieldInfo f in fia) {
    //    String fValStr = f.Name;
    //    if (String.Equals(fValStr, valStr, StringComparison.CurrentCulture))
    //      return f;
    //  }
    //  return null;
    //}
#endif

    public static FieldInfo GetFieldInfo(Object val) {
      if (val != null) {
        Type valType = val.GetType();
        String valStr = Enum.GetName(valType, val);
        FieldInfo[] fia = valType.GetFields();
        foreach (FieldInfo f in fia) {
          String fValStr = f.Name;
          if (String.Equals(fValStr, valStr, StringComparison.CurrentCulture))
            return f;
        }
        return null;
      } else
        return null;
    }
    /// <summary>
    /// Возвращет строковое представление значения из enum 
    /// </summary>
    /// <param name="value">Значение enum</param>
    /// <param name="type">Тип enum</param>
    /// <param name="getFullName">Вернуть полное имя с типом. По умолчанию True.</param>
    /// <returns></returns>
    public static String NameOfValue(Object value, Boolean getFullName) {
      if (value != null) {
        Type type = value.GetType();
        FieldInfo[] fis = type.GetFields();
        foreach (FieldInfo f in fis) {
          Object vVal = null;
          try {
            vVal = Enum.Parse(type, f.Name, false);
          } catch { }
          if (vVal == value)
            return ((getFullName) ? (type + ".") : "") + f.Name;
        }
        return null;
      }else
        return null;
    }

    /// <summary>
    /// Возвращет строковое представление значения из enum 
    /// </summary>
    /// <param name="value">Значение enum</param>
    /// <returns></returns>
    public static String NameOfValue(Object value) {
      return NameOfValue(value, true);
    }

    public static String GetFieldDesc(Object value) {
      DescriptionAttribute a = GetAttributeByValue<DescriptionAttribute>(value);
      return a.Description;
    }

    public static FieldInfo GetFieldByDescAttr<T>(String attrValue, StringComparison compareParam) {
      FieldInfo[] fia = typeof(T).GetFields();
      foreach (FieldInfo f in fia) {
        DescriptionAttribute a = GetAttributeByField<DescriptionAttribute>(f);
        if ((a != null) && String.Equals(a.Description, attrValue, compareParam))
          return f;
      }
      return null;
    }

    public static T GetAttributeByField<T>(ICustomAttributeProvider fieldInfo) {
      Object[] attrs = fieldInfo.GetCustomAttributes(false);
      return attrs.OfType<T>().FirstOrDefault();
    }

    public static T GetAttributeByValue<T>(Object value) {
      if (value != null) {
        Type valType = value.GetType();
        ICustomAttributeProvider fieldInfo = GetFieldInfo(value);
        Object[] attrs = fieldInfo.GetCustomAttributes(false);
        return attrs.OfType<T>().FirstOrDefault();
      } else
        return default(T);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    /// <param name="valueName">Значение параметра как строка</param>
    /// <returns></returns>
    public static T GetFieldValueByValueName<T>(String valueName, StringComparison compareParam) {
      FieldInfo[] fia = typeof(T).GetFields();
      foreach (FieldInfo f in fia) {
        String valStr = f.Name;
        if (String.Equals(valStr, valueName, compareParam))
          return (T)Enum.Parse(typeof(T), valStr, false);
      }

      return default(T);

    }

    public static T GetFieldValueByDescAttr<T>(String attrValue, StringComparison compareParam) {
      FieldInfo fi = GetFieldByDescAttr<T>(attrValue, StringComparison.CurrentCulture);
      if (fi != null) {
        return (T)fi.GetValue(null);
      } else
        return default(T);
    }

  }

  public class EnumWrapper {
    private String _name = null;
    public String Name { get { return this._name; } set { this._name = value; } }
    private Object _value = null;
    public Object Value { get { return this._value; } set { this._value = value; } }
  }
}
