using System;
using Newtonsoft.Json;
using System.Reflection;

namespace Bio.Helpers.Common.Types {

  /// <summary>
  /// Ответ
  /// </summary>
  public class AjaxResponse: ICloneable {
    /// <summary>
    /// Ответ в текстовом вид, как он есть
    /// </summary>
    public String ResponseText { get; set; }

    /// <summary>
    /// Если при обращении к серверу произошла ошибка, тогда FALSE.
    /// </summary>
    public Boolean Success { get; set; }
    /// <summary>
    /// Ошибка при обращении к серверу, возникает на клиенте
    /// </summary>
    public EBioException Ex { get; set; }

    public static AjaxResponse Decode(String pJsonString, JsonConverter[] converters) {
      return jsonUtl.Decode(pJsonString, null, converters) as AjaxResponse;
    }

    public virtual String Encode(JsonConverter[] converters) {
      return jsonUtl.encode(this, converters);
    }
    public virtual String Encode() {
      return this.Encode(ajaxUTL.GetConverters());
    }

    protected virtual void copyThis(ref AjaxResponse destObj) {
      destObj.ResponseText = this.ResponseText;
      destObj.Success = this.Success;
      destObj.Ex = (this.Ex != null) ? (EBioException)this.Ex.Clone() : null;
    }

    public static AjaxResponse CreateObjOfAjaxResponse(Type rqType) {
      var ci = rqType.GetConstructor(new Type[0]);
      if (ci != null) {
        var vR = (AjaxResponse)ci.Invoke(new Object[0]);
        return vR;
      }
      return null;
    }

    public object Clone() {
      var rslt = CreateObjOfAjaxResponse(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }
  }
}
