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

    /// <summary>
    /// Выполняет action для каждого поля
    /// </summary>
    /// <param name="type"></param>
    /// <param name="action"></param>
    public static void ForEachFieldInfo(Type type, Action<FieldInfo> action) {
        foreach (var v in type.GetFields()) {
          if (action != null)
            action(v);
        }
    }
    /// <summary>
    /// Выполняет action для каждого свойства
    /// </summary>
    /// <param name="type"></param>
    /// <param name="action"></param>
    public static void ForEachPropertyInfo(Type type, Action<PropertyInfo> action) {
      foreach (var v in type.GetProperties()) {
        if (action != null)
          action(v);
      }
    }

    /// <summary>
    /// Возвращает FieldInfo для val
    /// </summary>
    /// <param name="value">Значение enum</param>
    /// <returns></returns>
    public static FieldInfo GetFieldInfo(Object value) {
      if (value != null) {
        var v_type = value.GetType();
        var v_str = Enum.GetName(v_type, value);
        var fia = v_type.GetFields();
        foreach (var f in fia) {
          if (String.Equals(f.Name, v_str, StringComparison.CurrentCulture))
            return f;
        }
        return null;
      }
      return null;
    }

    /// <summary>
    /// Возвращет строковое представление значения из enum 
    /// </summary>
    /// <param name="value">Значение enum</param>
    /// <param name="getFullName">Вернуть полное имя с типом. По умолчанию True.</param>
    /// <returns></returns>
    public static String NameOfValue(Object value, Boolean getFullName) {
      if (value != null) {
        var type = value.GetType();
        var fis = type.GetFields();
        foreach (var f in fis) {
          Object v_val = null;
          try {
            v_val = Enum.Parse(type, f.Name, false);
          } catch { }
          if (v_val == value)
            return ((getFullName) ? (type + ".") : "") + f.Name;
        }
        return null;
      }
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

    /// <summary>
    /// Возвращет значения атрибута Description
    /// </summary>
    /// <param name="value">Значение enum</param>
    /// <returns></returns>
    public static String GetFieldDesc(Object value) {
      var a = GetAttributeByValue<DescriptionAttribute>(value);
      return a.Description;
    }

    /// <summary>
    /// Возвращет FieldInfo поля типа T по значению атрибута Description поля
    /// </summary>
    /// <param name="attrValue"></param>
    /// <param name="compareParam"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static FieldInfo GetFieldByDescAttr<T>(String attrValue, StringComparison compareParam) {
      var fia = typeof(T).GetFields();
      foreach (var f in fia) {
        var a = GetAttributeByInfo<DescriptionAttribute>(f);
        if ((a != null) && String.Equals(a.Description, attrValue, compareParam))
          return f;
      }
      return null;
    }

    /// <summary>
    /// Возврящает атрибут типа T для поля или свойсва
    /// </summary>
    /// <param name="info">FieldInfo или PropertyInfo</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetAttributeByInfo<T>(ICustomAttributeProvider info) {
      var attrs = info.GetCustomAttributes(false);
      return attrs.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Возврящает атрибут типа T для значения emum
    /// </summary>
    /// <param name="value">Значение enum</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetAttributeByValue<T>(Object value) {
      if (value != null) {
        ICustomAttributeProvider v_fieldInfo = GetFieldInfo(value);
        var attrs = v_fieldInfo.GetCustomAttributes(false);
        return attrs.OfType<T>().FirstOrDefault();
      }
      return default(T);
    }

    /// <summary>
    /// Возврящает значение emum типа T по строковому имени значения
    /// </summary>
    /// <typeparam name="T">Тип параметра</typeparam>
    /// <param name="valueName">Значение параметра как строка</param>
    /// <param name="compareParam"></param>
    /// <returns></returns>
    public static T GetFieldValueByValueName<T>(String valueName, StringComparison compareParam) {
      var fia = typeof(T).GetFields();
      foreach (var f in fia) {
        var v_str = f.Name;
        if (String.Equals(v_str, valueName, compareParam))
          return (T)Enum.Parse(typeof(T), v_str, false);
      }

      return default(T);

    }

    /// <summary>
    /// Возврящает значение emum типа T по строковому значению атрибута Description
    /// </summary>
    /// <param name="attrValue"></param>
    /// <param name="compareParam"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetFieldValueByDescAttr<T>(String attrValue, StringComparison compareParam) {
      var fi = GetFieldByDescAttr<T>(attrValue, StringComparison.CurrentCulture);
      if (fi != null) {
        return (T)fi.GetValue(null);
      }
      return default(T);
    }

  }

  public class EnumWrapper {
    private String _name;
    public String Name { get { return this._name; } set { this._name = value; } }
    private Object _value;
    public Object Value { get { return this._value; } set { this._value = value; } }
  }
}
