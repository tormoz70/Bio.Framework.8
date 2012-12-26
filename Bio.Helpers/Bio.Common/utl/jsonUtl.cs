namespace Bio.Helpers.Common {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Newtonsoft.Json;
  //using Bio.Packets.Ex;
  using Newtonsoft.Json.Converters;
  using System.Reflection;
  using Newtonsoft.Json.Serialization;
  using Bio.Helpers.Common.Types;
  using System.ComponentModel;
  using System.Runtime.Serialization.Formatters;
//#if SILVERLIGHT
//  using System.Runtime.Serialization.Formatters;
//#endif

  /*public enum FTypeMap {
    [Description("unknown")]
    ftUnknown = 0x0000,
    [Description("string")]
    ftString = 0x0001,
    [Description("float")]
    ftFloat = 0x0002,
    [Description("int")]
    ftInt = 0x0004,
    [Description("boolean")]
    ftBoolean = 0x0008,
    [Description("date")]
    ftDate = 0x0010,
    [Description("clob")]
    ftClob = 0x0020,
    [Description("object")]
    ftObject = 0x0040,
    [Description("blob")]
    ftBlob = 0x0080,
    [Description("currency")]
    ftCurrency = 0x0100
  };*/

  public enum CJSAlignment {
    [Description("left")]
    Left = 0x0001,
    [Description("center")]
    Center = 0x0002,
    [Description("right")]
    Right = 0x0004,
    [Description("stretch")]
    Stretch = 0x0008
  };

  public class jsonUtl {
    public const string TypePropertyName = "$type";
    public const string ArryValuesPropertyName = "$values";

    public static JsonConverter[] MergeConverters(JsonConverter[] c1, JsonConverter[] c2) {
      if ((c1 != null) && (c2 != null)) {
        var rslt = new List<JsonConverter>();
        rslt.AddRange(c1);
        foreach (var c in c2) {
          if (rslt.FirstOrDefault(n => { return n.GetType().Equals(c.GetType()); }) == null)
            rslt.Add(c);
        }
        return rslt.ToArray();
      } else if ((c1 != null) && (c2 == null))
        return c1;
      else if ((c1 == null) && (c2 != null))
        return c2;
      else
        return null;
    }

    //public static Object Decode(String jsonString) {
    //  if (!String.IsNullOrEmpty(jsonString)) {
    //    JsonSerializerSettings st = new JsonSerializerSettings() {
    //      ContractResolver = new CContractResolver(),
    //      TypeNameHandling = TypeNameHandling.Objects | TypeNameHandling.Arrays,
    //      Converters = new JsonConverter[] { new EBioExceptionConverter(), new CJsonStoreRowConverter() }
    //    };
    //    return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, null, st);
    //  } else
    //    return null;
    //}

    public static Object Decode(String pJsonString, Type ptype, JsonConverter[] coverters) {
      JsonConverter[] v_coverters = coverters; // MergeConverters(coverters, new JsonConverter[] { new LocDateTimeConverter() });
      if (!String.IsNullOrEmpty(pJsonString)) {
        JsonSerializerSettings st = new JsonSerializerSettings() {
          ContractResolver = new CContractResolver(),
          TypeNameHandling = TypeNameHandling.Objects,
          Converters = v_coverters
        };
        return JsonConvert.DeserializeObject(pJsonString, ptype, st);
      } else
        return null;
    }

    public static T decode<T>(String jsonString) {
      return decode<T>(jsonString, new JsonConverter[0]);
    }

    public static T decode<T>(String jsonString, JsonConverter[] coverters) {
      JsonConverter[] v_coverters = coverters; // MergeConverters(coverters, new JsonConverter[] { new LocDateTimeConverter() });
      if (!String.IsNullOrEmpty(jsonString)) {
        JsonSerializerSettings st = new JsonSerializerSettings() {
          ContractResolver = new CContractResolver(),
          TypeNameHandling = TypeNameHandling.Objects,
          Converters = v_coverters
        };
        return JsonConvert.DeserializeObject<T>(jsonString, st);
      } else
        return default(T);
    }

    public static String encode(Object obj, JsonConverter[] coverters) {
      JsonConverter[] v_coverters = coverters; // MergeConverters(coverters, new JsonConverter[] { new LocDateTimeConverter() });
      JsonSerializerSettings st = new JsonSerializerSettings() {
        ContractResolver = new CContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Objects | TypeNameHandling.Arrays,
        Converters = v_coverters,
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
      };
      //"Newtonsoft.Json.Utilities.ReflectionUtils.GetTypeName
      var v_rslt = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, st);
      return v_rslt;
    }

    public static CFieldType detectFieldType(String typeName) {
      CFieldType rslt = ftypeHelper.ConvertStrToFType(typeName);
      //rslt = (rslt == FTypeMap.Unknown) ? FTypeMap.String : rslt;
      return rslt;
    }

    public static CJSAlignment detectAlignment(CFieldType fldType, String alignName) {
      if (!String.IsNullOrEmpty(alignName)) {
        CJSAlignment rslt = enumHelper.GetFieldValueByDescAttr<CJSAlignment>(alignName, StringComparison.CurrentCulture);
        return rslt;
      } else {
        if (new CFieldType[] {CFieldType.Currency, CFieldType.Int, CFieldType.Currency}.Contains(fldType))
          return CJSAlignment.Right;
        else
          return CJSAlignment.Left;
      }
    }


  }

  public class CContractResolver : DefaultContractResolver {

    public CContractResolver() :base(true) { 
    }

    #region IContractResolver Members

    public override JsonContract ResolveContract(Type type) {
      if (type.Equals(typeof(EBioException)) || type.IsSubclassOf(typeof(EBioException))) {
        JsonContract contract = new JsonObjectContract(type);
        //#if !SILVERLIGHT
        //        contract.DefaultCreator = new Func<EBioException>(() => {
        //          return EBioException.CreateEBioEx(type, null);
        //        });
        //#else
        contract.DefaultCreator = new Func<Object>(() => {
          return EBioException.CreateEBioEx(type, null);
        });
        //#endif
        return contract;
      } else if (type.Equals(typeof(CAjaxRequest)) || type.IsSubclassOf(typeof(CAjaxRequest))) {
        JsonContract contract = base.ResolveContract(type);
        JsonProperty c = (contract as JsonObjectContract).Properties.GetProperty("callback", StringComparison.CurrentCulture);
        if (c != null)
          c.ShouldSerialize = new Predicate<Object>((o) => { return false; });
        c = (contract as JsonObjectContract).Properties.GetProperty("userToken", StringComparison.CurrentCulture);
        if (c != null)
          c.ShouldSerialize = new Predicate<Object>((o) => { return false; });
        //JsonContract contract = new Newtonsoft.Json.Serialization.JsonObjectContract(type);
        //contract.
        return contract;
      } else if (type.Equals(typeof(Byte[]))) {
        return new JsonArrayContract(typeof(Byte[]));
      } else {
        return base.ResolveContract(type);
      }
    }

    #endregion
  }


}
