using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;

namespace Bio.Helpers.Common.Types {

  /// <summary>
  /// Ответ
  /// </summary>
  public class CAjaxResponse: ICloneable {
    /// <summary>
    /// Ответ в текстовом вид, как он есть
    /// </summary>
    public String responseText { get; set; }

    /// <summary>
    /// Если при обращении к серверу произошла ошибка, тогда FALSE.
    /// </summary>
    public Boolean success { get; set; }
    /// <summary>
    /// Ошибка при обращении к серверу, возникает на клиенте
    /// </summary>
    public EBioException ex { get; set; }

    public static CAjaxResponse Decode(String pJsonString, JsonConverter[] converters) {
      return jsonUtl.Decode(pJsonString, null, converters) as CAjaxResponse;
    }

    public String Encode(JsonConverter[] converters) {
      return jsonUtl.encode(this, converters);
    }
    public String Encode() {
      return this.Encode(ajaxUTL.GetConverters());
    }

    //protected static T CopyObj<T>(T obj) where T : CAjaxResponse, new() {
    //  T rslt = new T() {
    //    responseText = obj.responseText,
    //    success = obj.success,
    //    ex = (EBioException)obj.ex.Clone()
    //  };

    //  if (typeof(T) == typeof(CBioResponse)) {
    //    CBioResponse frm = obj as CBioResponse; CBioResponse dst = rslt as CBioResponse;
    //    dst.transactionID = frm.transactionID;
    //    dst.bioParams = (CParams)frm.bioParams.Clone();
    //    dst.rptStatePacket = (CXLRptStatePack)frm.rptStatePacket;
    //  }

    //  if (typeof(T) == typeof(CJsonStoreResponse)) {
    //    CJsonStoreResponse frm = obj as CJsonStoreResponse; CJsonStoreResponse dst = rslt as CJsonStoreResponse;
    //    //dst.nav = (frm.nav != null) ? (CJsonStoreNav)frm.nav.Clone() : null;
    //    dst.packet = (frm.packet != null) ? (CJsonStoreData)frm.packet.Clone() : null;
    //    dst.cmd = frm.cmd;
    //    dst.sort = (frm.sort != null) ? (CJsonStoreSort)frm.sort.Clone() : null;
    //    dst.transactionID = frm.transactionID;
    //  }

    //  return rslt;
    //}

    protected virtual void copyThis(ref CAjaxResponse destObj) {
      destObj.responseText = this.responseText;
      destObj.success = this.success;
      destObj.ex = (this.ex != null) ? (EBioException)this.ex.Clone() : null;
    }

    public static CAjaxResponse CreateObjOfAjaxResponse(Type rqType) {
      ConstructorInfo ci = rqType.GetConstructor(new Type[0]);
      CAjaxResponse vR = (CAjaxResponse)ci.Invoke(new Object[0]);
      return vR;
    }

    public object Clone() {
      CAjaxResponse rslt = CreateObjOfAjaxResponse(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }
  }
}
