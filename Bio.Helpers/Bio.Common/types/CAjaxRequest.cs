using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
#if !SILVERLIGHT
using System.Web;
#endif
using System.Collections.Specialized;
using System.Reflection;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;
using System.IO;

namespace Bio.Helpers.Common.Types {

  //public class CAjaxRequestEventArgs : CancelEventArgs {
  //  public CAjaxRequest request { get; set; }
  //}

  public class AjaxResponseEventArgs : EventArgs {
    public CAjaxRequest request { get; set; }
    public CAjaxResponse response { get; set; }
    public Stream stream { get; set; }
  }

  public class AjaxRequestEventArgs : CancelEventArgs {
    public CAjaxRequest request { get; set; }
  }

  public delegate void AjaxBeforeRequestDelegate(Object sender, AjaxRequestEventArgs args);
  public delegate void AjaxRequestDelegate(Object sender, AjaxResponseEventArgs args);
  
  /// <summary>
  /// Конфиг. запроса
  /// </summary>
  public class CAjaxRequest : ICloneable {
    private const String csQParamName = "rqpckt";

    /// <summary>
    /// количество секунд ожидания ответа сервера
    /// </summary>
    public Int32 timeout { get; set; }

    /// <summary>
    /// Параметры запроса. 
    /// Данные параметры будут переданы на сервер в виде параметров Http-запроса
    /// К ним добавится системный параметр "rqpckt", в котором будет содержаться сам запрос в виде json-строки,
    /// при этом сериализованный объект запроса не содержит атрибут "prms".
    /// </summary>
    public Params prms { get; set; }
    ///// <summary>
    ///// Ссылка на объект, вызвавший запрос
    ///// </summary>
    //public Object Sender { get; set; }

    /// <summary>
    /// Не показывать окна с ошибками
    /// </summary>
    public bool silent { get; set; }

    /// <summary>
    /// Запрос
    /// </summary>
    public String url { get; set; }

    public Object userToken { get; set; }


    public AjaxRequestDelegate callback { get; set; }

    //internal void buildFURL(String pServerUrl) {
    //  this.FURL = ajaxUTL.BuildURL(pServerUrl);
    //}
    public CAjaxRequest() {
      //this.async = true;
    }

    /// <summary>
    /// Сериализует объект в json-строку
    /// </summary>
    /// <returns></returns>
    public String Encode(JsonConverter[] converters) {
      return jsonUtl.encode(this, converters);
    }
    /// <summary>
    /// Десериализует объект из json-строки
    /// </summary>
    /// <param name="jsonString"></param>
    /// <param name="converters">Массив конверторов</param>
    /// <returns></returns>
    public static CAjaxRequest Decode(String jsonString, JsonConverter[] converters) {
      return jsonUtl.decode<CAjaxRequest>(jsonString, converters);
    }

    /// <summary>
    /// Создает подготовленную к отправке на сервер коллекцию параметров
    /// </summary>
    /// <param name="converters">Массив конверторов</param>
    /// <returns></returns>
    public Params BuildQParams(JsonConverter[] converters) {
      Params rslt = (this.prms == null) ? new Params() : (Params)this.prms.Clone();
      CAjaxRequest rq = this.Clone() as CAjaxRequest;
      rq.prms = null;
      String vJsonStr = rq.Encode(converters);
      rslt.Add(new Param() { Name = csQParamName, Value = vJsonStr });
      return rslt;
    }

    public static CAjaxRequest ExtractFromQParams(Params p_prms, JsonConverter[] converters) {
      var vJsonStr = Params.FindParamValue(p_prms, csQParamName) as String;
      var rslt = CAjaxRequest.Decode(vJsonStr, converters);
      return rslt;
    }

#if !SILVERLIGHT
    public static CAjaxRequest ExtractFromQParams(NameValueCollection prms, JsonConverter[] converters) {
      var v_prms = new Params(prms);
      return ExtractFromQParams(v_prms, converters);
    }
    public static CAjaxRequest ExtractFromQParams(NameValueCollection prms) {
      return ExtractFromQParams(prms, ajaxUTL.GetConverters());
    }
#endif

    protected virtual void copyThis(ref CAjaxRequest destObj) {
      //destObj.async = this.async;
      destObj.prms = (this.prms != null) ? (Params)this.prms.Clone() : null;
      destObj.silent = this.silent;
      destObj.url = this.url;
      destObj.timeout = this.timeout;
      destObj.callback = this.callback;
      destObj.userToken = this.userToken;
    }


    public static CAjaxRequest CreateObjOfAjaxRequest(Type rqType) {
      ConstructorInfo ci = rqType.GetConstructor(new Type[0]);
      CAjaxRequest vR = (CAjaxRequest)ci.Invoke(new Object[0]);
      return vR;
    }


    #region ICloneable Members

    public object Clone() {
      CAjaxRequest rslt = CreateObjOfAjaxRequest(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }

    #endregion
  }



}
