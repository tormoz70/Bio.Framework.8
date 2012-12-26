using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  /// <summary>
  /// Атрибут описывает оператор для сравнения при локальной фильтрации.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field)]
  public class RequestTypeMappingAttribute : Attribute {
    private readonly string _typeMapping;

    public string Mapping {
      get { return this._typeMapping; }
    }

    public RequestTypeMappingAttribute(string typeMapping) {
      this._typeMapping = typeMapping;
    }
  }

  public class CBioRequestTyped : CAjaxRequest {

    /// <summary>
    /// Тип запроса
    /// </summary>
    public RequestType requestType { get; set; }

    public String getRequestTypeValueAsString() {
      return enumHelper.NameOfValue(this.requestType, false);
    }

    public static JsonConverter[] GetConverters() {
      return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    }

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      CBioRequestTyped dst = destObj as CBioRequestTyped;
      dst.requestType = this.requestType;
    }

  }


}
 