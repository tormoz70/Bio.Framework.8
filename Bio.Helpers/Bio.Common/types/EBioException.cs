namespace Bio.Helpers.Common.Types {
	using System;
  using System.IO;
  using System.Xml;
  using System.Text;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Runtime.Serialization;
  using System.Security.Permissions;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Utilities;
  using System.Runtime.Serialization.Formatters;

  public class EBioException:Exception, ICloneable {
    public Int32 ErrorCode { get; set; }
    public String ApplicationErrorMessage { get; internal set; }
    public String ServerTrace { get; internal set; }

    #region Constructors

    public EBioException()
      : base() {
    }
    public EBioException(String pMsg)
      : this(pMsg, null) {
    }
    public EBioException(String pMsg, Exception pInnerExeption)
      : base(pMsg, pInnerExeption) {
      Int32 errCode = 0;
      String errMsg = null;
      Utl.ExtractOracleApplicationError(pMsg, out errCode, out errMsg);
      this.ErrorCode = errCode;
      this.ApplicationErrorMessage = errMsg;
    }

    //protected EBioException(SerializationInfo info, StreamingContext context) : base (info, context) { }

    /// <summary>
    /// Проверяет pInnerExeption и если это не EBioException, то создает его
    /// </summary>
    /// <param name="pInnerExeption"></param>
    /// <returns></returns>
    public static EBioException CreateIfNotEBio(Exception pInnerExeption) {
      if(pInnerExeption is EBioException)
        return (EBioException)pInnerExeption;
      else
        return new EBioException(pInnerExeption.Message, pInnerExeption);
    }

    #endregion
    private String restoredStackTrace = null;
    public new String StackTrace {
      get {
        String baseStackTrace = (this.restoredStackTrace != null) ? this.restoredStackTrace : base.StackTrace;
        return ((this.InnerException != null) ? this.InnerException.StackTrace + "\n" + baseStackTrace : baseStackTrace) +
          (!String.IsNullOrEmpty(this.ServerTrace) ? "\nServerTrace: " + this.ServerTrace : null);
      }

      internal set {
        this.restoredStackTrace = value;
      }
    }

    public static EBioException Decode(String pJsonString) {
      return jsonUtl.decode<EBioException>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

#if !SILVERLIGHT
    protected virtual XmlDocument encode2xml(/*String pAppURL*/) {
      XmlDocument vDoc = new XmlDocument();
      Type vTP = this.GetType();
      XmlElement vRoot = (XmlElement)vDoc.AppendChild(vDoc.CreateElement("ebio"));
      vRoot.AppendChild(vDoc.CreateElement("namespace")).InnerText = vTP.Namespace;
      vRoot.AppendChild(vDoc.CreateElement("type")).InnerText = vTP.Name;
      //if(pAppURL != null)
      //  vRoot.AppendChild(vDoc.CreateElement("appurl")).InnerText = pAppURL;
      if(this.Message != null)
        vRoot.AppendChild(vDoc.CreateElement("message")).AppendChild(vDoc.CreateCDataSection(this.Message));
      if(this.StackTrace != null)
        vRoot.AppendChild(vDoc.CreateElement("trace")).AppendChild(vDoc.CreateCDataSection(this.StackTrace));
      if(this.ApplicationErrorMessage != null)
        vRoot.AppendChild(vDoc.CreateElement("app_err_message")).AppendChild(vDoc.CreateCDataSection(this.ApplicationErrorMessage));
      return vDoc;
    }
#endif

    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }

    public void writeToStream(Stream pStreem, Encoding pEncoding) {
      String vLine = this.Encode();
      using(StreamWriter vWR = new StreamWriter(pStreem, pEncoding)) {
        vWR.WriteLine(vLine);
      }
    }

    public void markAsServerException() {
      this.ServerTrace = this.restoredStackTrace;
      this.restoredStackTrace = null;
    }

    public override string ToString() {
      return "[" + this.GetType().Name + "] : " + (!String.IsNullOrEmpty(this.Message) ? "Сообщение: " + this.Message : null) + "\n" +
        (!String.IsNullOrEmpty(this.StackTrace) ? "StackTrace: " + this.StackTrace : null);
    }

    public static EBioException CreateEBioEx(Type objectType, String msg) {
      Type[] tps = (msg == null) ? new Type[0] : new Type[] { typeof(string) };
      Object[] prms = (msg == null) ? new Object[0] : new Object[] { msg };
      ConstructorInfo ci = objectType.GetConstructor(tps);
      EBioException vEx = (EBioException)ci.Invoke(prms);
      return vEx;
    }


    #region ICloneable Members

    public object Clone() {
      EBioException rslt = EBioException.CreateEBioEx(this.GetType(), this.Message);
      if (this.Data != null)
        foreach (Object key in this.Data.Keys) {
          Object val = this.Data[key];
          rslt.Data.Add(key, (val is ICloneable) ? (val as ICloneable).Clone() : val);
        }
      rslt.StackTrace = this.StackTrace;
      return rslt;
    }

    #endregion
  }

  public class EBioUnknownRequest : EBioException {
    public EBioUnknownRequest()
      : base() {
    }
    public EBioUnknownRequest(String msg)
      : base(msg) {
    }
    public EBioUnknownRequest(Exception innerExeption)
      : base(innerExeption.Message, innerExeption) {
    }
  }

  public class EBioDBConnectionError : EBioException {
    public EBioDBConnectionError()
      : base() {
    }
    public EBioDBConnectionError(String msg)
      : base(msg) {
    }
    public EBioDBConnectionError(String msg, Exception innerExeption)
      : base(msg, innerExeption) {
    }
  }

  public class EBioDBAccessError : EBioException {
    public EBioDBAccessError()
      : base() {
    }
    public EBioDBAccessError(String msg)
      : base(msg) {
    }
    public EBioDBAccessError(String msg, Exception innerExeption)
      : base(msg, innerExeption) {
    }
  }

  public class EBioUnknownException : EBioException {
    public EBioUnknownException()
      : base() {
    }
    public EBioUnknownException(String msg)
      : base(msg) {
    }
    public EBioUnknownException(String msg, Exception innerExeption)
      : base(msg, innerExeption) {
    }
  }

  public class EBioExceptionConverter : EBioExceptionConverterBase<EBioException> {
  }

  public class EBioExceptionConverterBase<T> : CustomCreationConverter<T> {

    public override T Create(Type objectType) {
      ConstructorInfo ci = objectType.GetConstructor(new Type[] { });
      T vEx = (T)ci.Invoke(new Object[] { });
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
      //return base.ReadJson(reader, objectType, existingValue, serializer);
      Type rType = typeof(T);
      EBioException rslt = null;
      ////if (reader.Depth == 1) {
      reader.Read();
      String typeName = null;
      String msg = null;
      String stackTrace = null;
      String applicationErrorMessage = null;
      BioUser vUsr = null;
      while (reader.TokenType != JsonToken.EndObject) {

        if (String.Equals((String)reader.Value, jsonUtl.TypePropertyName)) {
          reader.Read();
          typeName = (String)reader.Value;
        }
        if (String.Equals((String)reader.Value, "Message")) {
          reader.Read();
          msg = (String)reader.Value;
        }
        if (String.Equals((String)reader.Value, "StackTrace")) {
          reader.Read();
          stackTrace = (String)reader.Value;
        }
        if (String.Equals((String)reader.Value, "ApplicationErrorMessage")) {
          reader.Read();
          applicationErrorMessage = (String)reader.Value;
        }
        if (String.Equals((String)reader.Value, "Usr")) {
          reader.Read();
          vUsr = serializer.Deserialize<BioUser>(reader);
        }
        reader.Read();
      }
      if (typeName != null)
        rType = Type.GetType(typeName);
      rslt = EBioException.CreateEBioEx(rType, msg);
      rslt.StackTrace = stackTrace;
      rslt.ApplicationErrorMessage = applicationErrorMessage;
      if (rslt is EBioOk)
        ((EBioOk)rslt).Usr = vUsr;
      return rslt;
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer) {
      serializer.TypeNameHandling = TypeNameHandling.Objects;
      serializer.NullValueHandling = NullValueHandling.Ignore;
      //base.WriteJson(writer, value, serializer);
      var ex = value as EBioException;
      writer.WriteStartObject();
      writer.WritePropertyName(jsonUtl.TypePropertyName);
      var tp = ex.GetType();
      writer.WriteValue(ReflectionUtils.GetTypeName(tp, FormatterAssemblyStyle.Full));
      writer.WritePropertyName("Message");
      writer.WriteValue(ex.Message);

      if (ex.StackTrace != null) {
        writer.WritePropertyName("StackTrace");
        writer.WriteValue(ex.StackTrace);
      }
      if (ex.ApplicationErrorMessage != null) {
        writer.WritePropertyName("ApplicationErrorMessage");
        writer.WriteValue(ex.ApplicationErrorMessage);
      }
      if (ex is EBioOk) {
        writer.WritePropertyName("Usr");
        serializer.Serialize(writer, ((EBioOk)ex).Usr);
        //String vUsrJSON = ((EBioOk)ex).Usr.Encode();
        //writer.WritePropertyName("Usr");
        //writer.WriteValue(vUsrJSON);
      }
      

      writer.WriteEndObject();
    }
  }
}