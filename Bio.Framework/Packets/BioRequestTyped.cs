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

  public class BioRequestTyped : AjaxRequest {

    /// <summary>
    /// Тип запроса
    /// </summary>
    public RequestType RequestType { get; set; }

    public String GetRequestTypeValueAsString() {
      return enumHelper.NameOfValue(this.RequestType, false);
    }

    public static JsonConverter[] GetConverters() {
      return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      BioRequestTyped dst = destObj as BioRequestTyped;
      dst.RequestType = this.RequestType;
    }

  }


}
 