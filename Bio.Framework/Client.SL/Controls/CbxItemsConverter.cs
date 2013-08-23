using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace Bio.Framework.Client.SL {
  public class CbxItemsConverter : CustomCreationConverter<CbxItems> {
    public override CbxItems Create(Type objectType) {
      return new CbxItems();
    }

    public override bool CanConvert(Type objectType) {
      return objectType == typeof(CbxItems);
    }

    public override bool CanWrite {
      get {
        return true;
      }
    }
    public override bool CanRead {
      get {
        return true;
      }
    }

    private void _writeDS(JsonWriter writer, Object ds) {
      writer.WriteStartObject();
      var generic = ds.GetType().GetGenericTypeDefinition();
      var specific = generic.MakeGenericType(typeof(CRTObject));
      writer.WritePropertyName(jsonUtl.TypePropertyName);
      writer.WriteValue(ReflectionUtils.GetTypeName(specific, FormatterAssemblyStyle.Full));
      writer.WritePropertyName(jsonUtl.ArryValuesPropertyName);
      writer.WriteStartArray();
      var collection = (IEnumerable<CRTObject>)ds;
      foreach (var item in collection) {
        writer.WriteStartObject();
        writer.WritePropertyName(jsonUtl.TypePropertyName);
        var tp = typeof(CRTObject);
        writer.WriteValue(ReflectionUtils.GetTypeName(tp, FormatterAssemblyStyle.Full));
        var props = item.GetType().GetProperties();
        foreach (var p in props) {
          if (p.Name != "Item") {
            writer.WritePropertyName(p.Name);
            writer.WriteValue(p.GetValue(item, null));
          }
        }
        writer.WriteEndObject();
      }
      writer.WriteEndArray();
      writer.WriteEndObject();
    }

    public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer) {
      serializer.TypeNameHandling = TypeNameHandling.Objects;
      serializer.NullValueHandling = NullValueHandling.Ignore;
      writer.WriteStartObject();
      writer.WritePropertyName(jsonUtl.TypePropertyName);
      writer.WriteValue(ReflectionUtils.GetTypeName(value.GetType(), FormatterAssemblyStyle.Full));

      writer.WritePropertyName("metadata");
      serializer.Serialize(writer, ((CbxItems)value).metadata);
      writer.WritePropertyName("ds");
      this._writeDS(writer, ((CbxItems)value).ds);
      writer.WriteEndObject();
    }

    private static CRTObject _restoreCRTObject(JsonReader reader, Type itemType) {
      var result = TypeFactory.CreateInstance(itemType);
      if (result != null) {
        reader.Read();
        if (reader.TokenType == JsonToken.StartObject) {
          while (reader.TokenType != JsonToken.EndObject) {
            if (reader.TokenType == JsonToken.PropertyName) {
              var propertyName = (String) reader.Value;
              var propertyInfo = itemType.GetProperty(propertyName);
              reader.Read(); // goto propertyValue
              if (propertyInfo != null) {
                var propertyValue = Utl.Convert2Type(reader.Value, propertyInfo.PropertyType);
                propertyInfo.SetValue(result, propertyValue, null);
              }
            }
            reader.Read();
          }
        }
      }
      return result;
    }

    private static IEnumerable<CRTObject> _restoreDS(JsonReader reader, Type itemType) {
      IEnumerable<CRTObject> result = null;
      reader.Read();
      if (reader.TokenType == JsonToken.StartObject) {
        reader.Read();
        if (String.Equals((String) reader.Value, jsonUtl.TypePropertyName)) {
          reader.Read();
          var collectionTypeName = (String) reader.Value;
          var collectionType = Type.GetType(collectionTypeName);
          if (collectionType != null && collectionType.IsGenericType) collectionType = collectionType.GetGenericTypeDefinition();
          if (collectionType != null) collectionType = collectionType.MakeGenericType(itemType);
          if (collectionType == null)
            throw new Exception("Коллекция имеет не верный тип элемента!");
          result = Activator.CreateInstance(collectionType) as IEnumerable<CRTObject>;
        }
        if (result == null)
          return null;
        reader.Read();
        if (String.Equals((String)reader.Value, jsonUtl.ArryValuesPropertyName)) {
          reader.Read();
          if (reader.TokenType == JsonToken.StartArray) {
            while (reader.TokenType != JsonToken.EndArray) {
              var item = _restoreCRTObject(reader, itemType);
              if (item != null)
                ((IList) result).Add(item);
            }
          }
        }
        reader.Read();
        if (reader.TokenType == JsonToken.EndObject)
          return result;
      }
      return null;
    }

    private readonly TypeFactory _typeFactory = new TypeFactory();
    private Type _creRowType(List<JsonStoreMetadataFieldDef> fieldDefs) {
      var propDefs = JsonStoreClient.GeneratePropertyDefs(fieldDefs);
      return this._typeFactory.CreateType(propDefs);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      var rslt = new CbxItems();
      reader.Read();
      while (reader.TokenType != JsonToken.EndObject) {
        if (String.Equals((String)reader.Value, jsonUtl.TypePropertyName)) {
          reader.Read();
          var typeName = (String)reader.Value;
        }
        if (String.Equals((String)reader.Value, "metadata")) {
          reader.Read();
          rslt.metadata = serializer.Deserialize<JsonStoreMetadata>(reader);
        }
        if (String.Equals((String)reader.Value, "ds")) {
          var rowType = this._creRowType(rslt.metadata.Fields);
          rslt.ds = _restoreDS(reader, rowType);
        }
        
        reader.Read();
      }
      return rslt;
    }
  }
}