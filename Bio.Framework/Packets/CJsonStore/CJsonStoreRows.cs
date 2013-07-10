using System;
using System.Collections.Generic;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {
  public enum CJsonStoreRowChangeType {
    //[Description("n")]
    Unchanged = 0,
    //[Description("a")]
    Added = 1,
    //[Description("u")]
    Modified = 2,
    //[Description("d")]
    Deleted = 3
  };
  public class CJsonStoreRow : ICloneable {
    public CJsonStoreRow() { this.Values = new List<Object>(); }
    public String InternalROWUID { get; set; }
    public CJsonStoreRowChangeType ChangeType { get; set; }
    public List<Object> Values { get; set; }
    public object Clone() {
      var newRow = new CJsonStoreRow { InternalROWUID = this.InternalROWUID, ChangeType = this.ChangeType };
      foreach (var val in this.Values) {
        newRow.Values.Add((val is ICloneable) ? (val as ICloneable).Clone() : val);
      }
      return newRow;
    }
  }

  public class CJsonStoreRows : List<CJsonStoreRow>, ICloneable {
    public void CopyFrom(CJsonStoreRows rows) {
      this.Clear();
      foreach (var row in rows) {
        this.Add((CJsonStoreRow)row.Clone());
      }
    }
    
    public object Clone() {
      var rslt = new CJsonStoreRows();
      rslt.CopyFrom(this);
      return rslt;
    }
  }

  /*t12
  public class CJsonStoreRowConverter : CustomCreationConverter<CJsonStoreRow> {

    public override CJsonStoreRow Create(Type objectType) {
      ConstructorInfo ci = objectType.GetConstructor(new Type[] { });
      CJsonStoreRow vEx = (CJsonStoreRow)ci.Invoke(new Object[] { });
      return vEx;
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

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      CJsonStoreRow rslt = new CJsonStoreRow();
      reader.Read();
      List<Object> data = null;
      CJsonStoreRowChangeType changeType = CJsonStoreRowChangeType.Unchanged;
      while (reader.TokenType != JsonToken.EndObject) {
        if (String.Equals((String)reader.Value, jsonUtl.ArryValuesPropertyName)) {
          reader.Read();
          data = serializer.Deserialize<List<Object>>(reader);
        }
        if (String.Equals((String)reader.Value, "changeType")) {
          reader.Read();
          changeType = enumHelper.GetFieldValueByDescAttr<CJsonStoreRowChangeType>((String)reader.Value, StringComparison.CurrentCulture);
        }
        reader.Read();
      }
      rslt.changeType = changeType;
      foreach (Object val in data)
        rslt.Add(val);
      return rslt;
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) {
      serializer.TypeNameHandling = TypeNameHandling.Objects;
      serializer.NullValueHandling = NullValueHandling.Ignore;
      CJsonStoreRow rowData = value as CJsonStoreRow;
      writer.WriteStartObject();
      //writer.WritePropertyName(jsonUtl.TypePropertyName);
      //Type tp = rowData.GetType();
      //writer.WriteValue(tp.AssemblyQualifiedName);
      writer.WritePropertyName(jsonUtl.ArryValuesPropertyName);
      String rowDataJson = JsonConvert.SerializeObject(rowData);
      writer.WriteRawValue(rowDataJson);
      writer.WritePropertyName("changeType");
      writer.WriteValue(enumHelper.GetAttributeByValue<DescriptionAttribute>(rowData.changeType).Description);
      writer.WriteEndObject();
    }
  }
   */
}
