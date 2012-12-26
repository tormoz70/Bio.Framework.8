namespace Bio.Framework.Packets {
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Text;
  using System.ComponentModel;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;
  using System.Reflection;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  /// <summary>
  /// Информация о состояние выполняемой операции
  /// </summary>
  public class CRemoteProcessStatePack : ICloneable {
    public String bioCode { get; set; }
    /// <summary>
    /// Запущена
    /// </summary>
    public DateTime started { get; set; }
    /// <summary>
    /// Прошло
    /// </summary>
    public TimeSpan duration { get; set; }
    /// <summary>
    /// Состояние
    /// </summary>
    public RemoteProcState state { get; set; }
    /// <summary>
    /// Последняя ошибка
    /// </summary>
    public EBioException ex { get; set; }

    public Boolean hasResultFile { get; set; }

    #region Для долгих операций
    /// <summary>
    /// Имя трубы
    /// </summary>
    public String pipe { get; set; }
    /// <summary>
    /// UID сессии
    /// </summary>
    public String sessionUID { get; set; }
    /// <summary>
    /// Владелец-пользователь, запустивший процесс
    /// </summary>
    public String owner { get; set; }
    /// <summary>
    /// Последнее сообщение считанное PipeReader'ом
    /// </summary>
    public String[] lastPipedLines { get; set; }
    #endregion
    /// <summary>
    /// Выполняется
    /// </summary>
    public Boolean isRunning() {
      return rmtUtl.isRunning(this.state);
    }

    public String stateDesc() {
      return enumHelper.GetFieldDesc(this.state);
    }

    /// <summary>
    /// Выполнен
    /// </summary>
    public Boolean isFinished() {
      return rmtUtl.isFinished(this.state);
    }

    public static CRemoteProcessStatePack Decode(String pJsonString) {
      return jsonUtl.decode<CRemoteProcessStatePack>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }


    #region ICloneable Members

    public static CRemoteProcessStatePack CreateObjOfStatePack(Type rqType) {
      ConstructorInfo ci = rqType.GetConstructor(new Type[0]);
      CRemoteProcessStatePack vR = (CRemoteProcessStatePack)ci.Invoke(new Object[0]);
      return vR;
    }

    protected virtual void copyThis(ref CRemoteProcessStatePack destObj) {
      destObj.bioCode = this.bioCode;
      destObj.started = this.started;
      destObj.duration = this.duration;
      destObj.state = this.state;
      destObj.hasResultFile = this.hasResultFile;
      destObj.ex = (EBioException)this.ex.Clone();
      destObj.pipe = this.pipe;
      destObj.sessionUID = this.sessionUID;
      destObj.owner = this.owner;
      destObj.lastPipedLines = (String[])this.lastPipedLines.Clone();
    }

    public object Clone() {
      CRemoteProcessStatePack rslt = CreateObjOfStatePack(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }

    #endregion
  }

}
