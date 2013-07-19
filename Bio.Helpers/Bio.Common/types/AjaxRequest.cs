using System;
using System.Collections;
using System.ComponentModel;
using Newtonsoft.Json;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif
using System.IO;

namespace Bio.Helpers.Common.Types {

  public class AjaxResponseEventArgs : EventArgs {
    public AjaxRequest Request { get; set; }
    public AjaxResponse Response { get; set; }
    public Stream Stream { get; set; }
  }

  public class JsonStoreDSLoadedEventArgs : EventArgs {
    public IEnumerable DS { get; set; }
    public AjaxRequest Request { get; set; }
    public AjaxResponse Response { get; set; }
  }

  public class AjaxRequestEventArgs : CancelEventArgs {
    public AjaxRequest Request { get; set; }
  }

  public delegate void AjaxBeforeRequestDelegate(Object sender, AjaxRequestEventArgs args);
  public delegate void AjaxRequestDelegate(Object sender, AjaxResponseEventArgs args);
  
  /// <summary>
  /// Конфиг. запроса
  /// </summary>
  public class AjaxRequest : ICloneable {
    private const String CS_Q_PARAM_NAME = "rqpckt";

    /// <summary>
    /// количество секунд ожидания ответа сервера
    /// </summary>
    public Int32 Timeout { get; set; }

    /// <summary>
    /// Параметры запроса. 
    /// Данные параметры будут переданы на сервер в виде параметров Http-запроса
    /// К ним добавится системный параметр "rqpckt", в котором будет содержаться сам запрос в виде json-строки,
    /// при этом сериализованный объект запроса не содержит атрибут "prms".
    /// </summary>
    public Params Prms { get; set; }

    /// <summary>
    /// Не показывать окна с ошибками
    /// </summary>
    public Boolean Silent { get; set; }

    /// <summary>
    /// Запрос
    /// </summary>
    public String URL { get; set; }

    public Object UserToken { get; set; }

    [JsonIgnore]
    public AjaxRequestDelegate Callback { get; set; }

    public AjaxRequest() {
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
    public static AjaxRequest Decode(String jsonString, JsonConverter[] converters) {
      return jsonUtl.decode<AjaxRequest>(jsonString, converters);
    }

    /// <summary>
    /// Создает подготовленную к отправке на сервер коллекцию параметров
    /// </summary>
    /// <param name="converters">Массив конверторов</param>
    /// <returns></returns>
    public Params BuildQParams(JsonConverter[] converters) {
      var rslt = (this.Prms == null) ? new Params() : (Params)this.Prms.Clone();
      var rq = this.Clone() as AjaxRequest;
      String vJsonStr = null;
      if (rq != null) {
        rq.Prms = null;
        vJsonStr = rq.Encode(converters);
      }
      rslt.Add(new Param { Name = CS_Q_PARAM_NAME, Value = vJsonStr });
      return rslt;
    }

    public static AjaxRequest ExtractFromQParams(Params prms, JsonConverter[] converters) {
      var vJsonStr = Params.FindParamValue(prms, CS_Q_PARAM_NAME) as String;
      var rslt = Decode(vJsonStr, converters);
      return rslt;
    }

#if !SILVERLIGHT
    public static AjaxRequest ExtractFromQParams(NameValueCollection prms, JsonConverter[] converters) {
      var v_prms = new Params(prms);
      return ExtractFromQParams(v_prms, converters);
    }
    public static AjaxRequest ExtractFromQParams(NameValueCollection prms) {
      return ExtractFromQParams(prms, ajaxUTL.GetConverters());
    }
#endif

    protected virtual void copyThis(ref AjaxRequest destObj) {
      destObj.Prms = (this.Prms != null) ? (Params)this.Prms.Clone() : null;
      destObj.Silent = this.Silent;
      destObj.URL = this.URL;
      destObj.Timeout = this.Timeout;
      destObj.Callback = this.Callback;
      destObj.UserToken = this.UserToken;
    }


    public static AjaxRequest CreateObjOfAjaxRequest(Type rqType) {
      var ci = rqType.GetConstructor(new Type[0]);
      var vR = (AjaxRequest)ci.Invoke(new Object[0]);
      return vR;
    }


    #region ICloneable Members

    public object Clone() {
      AjaxRequest rslt = CreateObjOfAjaxRequest(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }

    #endregion
  }



}
