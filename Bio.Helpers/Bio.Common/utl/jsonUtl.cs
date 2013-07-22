using Bio.Helpers.Common;

namespace Bio.Helpers.Common {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using System.Reflection;
  using Newtonsoft.Json.Serialization;
  using Bio.Helpers.Common.Types;
  using System.ComponentModel;
  using System.Runtime.Serialization.Formatters;

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

    public static Object Decode(String pJsonString, Type ptype, JsonConverter[] coverters) {
      JsonConverter[] v_coverters = coverters;
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
      JsonConverter[] v_coverters = coverters;
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
      JsonConverter[] v_coverters = coverters;
      JsonSerializerSettings st = new JsonSerializerSettings() {
        ContractResolver = new CContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Objects | TypeNameHandling.Arrays,
        Converters = v_coverters,
        TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
      };
      var v_rslt = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, st);
      return v_rslt;
    }

    public static JSFieldType detectFieldType(String typeName) {
      JSFieldType rslt = ftypeHelper.ConvertStrToFieldType(typeName);
      return rslt;
    }

    public static CJSAlignment detectAlignment(JSFieldType fldType, String alignName) {
      if (!String.IsNullOrEmpty(alignName)) {
        CJSAlignment rslt = enumHelper.GetFieldValueByDescAttr<CJSAlignment>(alignName, StringComparison.CurrentCulture);
        return rslt;
      } else {
        if (new JSFieldType[] {JSFieldType.Currency, JSFieldType.Int, JSFieldType.Currency}.Contains(fldType))
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
        contract.DefaultCreator = new Func<Object>(() => {
          return EBioException.CreateEBioEx(type, null);
        });
        return contract;
      } else if (type.Equals(typeof(AjaxRequest)) || type.IsSubclassOf(typeof(AjaxRequest))) {
        JsonContract contract = base.ResolveContract(type);
        JsonProperty c = (contract as JsonObjectContract).Properties.GetProperty("callback", StringComparison.CurrentCulture);
        if (c != null)
          c.ShouldSerialize = new Predicate<Object>((o) => { return false; });
        c = (contract as JsonObjectContract).Properties.GetProperty("userToken", StringComparison.CurrentCulture);
        if (c != null)
          c.ShouldSerialize = new Predicate<Object>((o) => { return false; });
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
