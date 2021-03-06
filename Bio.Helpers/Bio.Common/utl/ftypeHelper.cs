﻿using System;
using System.Linq;

namespace Bio.Helpers.Common {

  internal class MappingAttribute : Attribute {

    public MappingAttribute(String xmlName, Type toType, Type[] fromTypes) {
      this.XmlName = xmlName;
      this.ToNetType = toType;
      this.FromNetTypes = fromTypes;
    }
    public String XmlName { get; private set; }
    public Type ToNetType { get; private set; }
    public Type[] FromNetTypes { get; private set; }
  }

  /// <summary>
  /// Тип поля данных
  /// </summary>
  public enum JSFieldType {

    [Mapping("string", typeof(String), null)]
    String      = 0x0000,

    [Mapping("float", typeof(Decimal), new Type[] { typeof(Decimal), typeof(Double), typeof(float) })]
    Float       = 0x0001,

    [Mapping("int", typeof(Int64), new Type[] { typeof(Int16), typeof(Int32), typeof(Int64) })]
    Int         = 0x0002,

    [Mapping("boolean", typeof(Boolean), null)]
    Boolean     = 0x0003,

    [Mapping("date", typeof(DateTime?), new Type[] { typeof(DateTime), typeof(DateTimeOffset), typeof(DateTime?), typeof(DateTimeOffset?) })]
    Date        = 0x0004,
    [Mapping("dateUTC", typeof(DateTime?), null)]
    DateUTC     = 0x0005,

    [Mapping("clob", typeof(String), null)]
    Clob        = 0x0006,

    [Mapping("object", typeof(Object), null)]
    Object      = 0x0007,

    [Mapping("blob", typeof(Byte[]), null)]
    Blob        = 0x0008,

    [Mapping("currency", typeof(Decimal), null)]
    Currency    = 0x0009

  };

  public enum FieldEncoding {
    UTF8 = 0,
    WINDOWS1251 = 1
  }


  public static class ftypeHelper {

    /// <summary>
    /// Преобразует имя типа в объект типа System.Type.
    /// </summary>
    /// <param name="xmlName">Имя типа прописанное в \ini\iod\store.xsd.</param>
    /// <returns>Тип соответствующий входному имени типа.</returns>
    /// <exception cref="ArgumentException">Возбуждается, если тип не может быть преобразован.</exception>
    public static Type ConvertStrToType(String xmlName) {
      if (!String.IsNullOrEmpty(xmlName)) {
        foreach(var fi in typeof(JSFieldType).GetFields()) {
          var attr = enumHelper.GetAttributeByInfo<MappingAttribute>(fi);
          if((attr != null) && (attr.XmlName.Equals(xmlName)))
            return attr.ToNetType;
        }
      }
      throw new ArgumentException("Невозможно преобразовать тип \"" + xmlName + "\".", "xmlName");
    }

    /// <summary>
    /// Преобразует объект типа System.Type в строковое имя.
    /// </summary>
    /// <param name="type">Преобразуемый тип.</param>
    /// <returns>Строковое соответствие типа.</returns>
    /// <exception cref="ArgumentException">Возбуждается, если тип не может быть преобразован.</exception>
    public static String ConvertTypeToStr(Type type) {

      if (type != null) {
        foreach (var fi in typeof(JSFieldType).GetFields()) {
          var attr = enumHelper.GetAttributeByInfo<MappingAttribute>(fi);
          if (attr != null) {
            if (((attr.FromNetTypes != null) && attr.FromNetTypes.Any(t => t.Equals(type))) ||
                ((attr.FromNetTypes == null) && (attr.ToNetType.Equals(type))))
              return attr.XmlName;
          }
        }
      }
      throw new ArgumentException("Невозможно преобразовать тип \"" + type + "\".", "type");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static JSFieldType ConvertStrToFieldType(String xmlName) {
      if (!String.IsNullOrEmpty(xmlName)) {
        foreach (var fi in typeof(JSFieldType).GetFields()) {
          var attr = enumHelper.GetAttributeByInfo<MappingAttribute>(fi);
          if ((attr != null) && (attr.XmlName.Equals(xmlName)))
            return (JSFieldType)fi.GetValue(null);
        }
      }
      throw new ArgumentException("Невозможно преобразовать тип \"" + xmlName + "\".", "xmlName");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static FieldEncoding ConvertStrToFieldEncoding(String xmlName) {
      if (!String.IsNullOrEmpty(xmlName)) {
        if(String.Equals(xmlName, Utl.ENC_UTF8, StringComparison.OrdinalIgnoreCase))
          return FieldEncoding.UTF8;
        if (String.Equals(xmlName, Utl.ENC_WINDOWS1251, StringComparison.OrdinalIgnoreCase))
          return FieldEncoding.WINDOWS1251;
      }
      throw new ArgumentException("Невозможно преобразовать имя кодировки в кодировку \"" + xmlName + "\".", "xmlName");
    }

    /// <summary>
    /// Преобразует тип FTypeMap в тип System.Type.
    /// </summary>
    /// <param name="type">Имя типа.</param>
    /// <returns>Объект, соответствующий входному имени типа.</returns>
    /// <exception cref="ArgumentException">Возбуждается, если тип не может быть преобразован.</exception>
    public static Type ConvertFTypeToType(JSFieldType type) {
      return enumHelper.GetAttributeByValue<MappingAttribute>(type).ToNetType;
    }

    /// <summary>
    /// Преобразует тип System.Type в тип FTypeMap.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static JSFieldType ConvertTypeToFType(Type type) {
      return ConvertStrToFieldType(ConvertTypeToStr(type));
    }

  }

}
