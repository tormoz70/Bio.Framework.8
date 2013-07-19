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
  public class RemoteProcessStatePack : ICloneable {
    public String BioCode { get; set; }
    /// <summary>
    /// Запущена
    /// </summary>
    public DateTime Started { get; set; }
    /// <summary>
    /// Прошло
    /// </summary>
    public TimeSpan Duration { get; set; }
    /// <summary>
    /// Состояние
    /// </summary>
    public RemoteProcState State { get; set; }
    /// <summary>
    /// Последняя ошибка
    /// </summary>
    public EBioException Ex { get; set; }

    public Boolean HasResultFile { get; set; }

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
    public Boolean IsRunning() {
      return rmtUtl.IsRunning(this.State);
    }

    public String StateDesc() {
      return enumHelper.GetFieldDesc(this.State);
    }

    /// <summary>
    /// Выполнен
    /// </summary>
    public Boolean IsFinished() {
      return rmtUtl.IsFinished(this.State);
    }

    public static RemoteProcessStatePack Decode(String pJsonString) {
      return jsonUtl.decode<RemoteProcessStatePack>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }


    #region ICloneable Members

    public static RemoteProcessStatePack CreateObjOfStatePack(Type rqType) {
      ConstructorInfo ci = rqType.GetConstructor(new Type[0]);
      RemoteProcessStatePack vR = (RemoteProcessStatePack)ci.Invoke(new Object[0]);
      return vR;
    }

    protected virtual void copyThis(ref RemoteProcessStatePack destObj) {
      destObj.BioCode = this.BioCode;
      destObj.Started = this.Started;
      destObj.Duration = this.Duration;
      destObj.State = this.State;
      destObj.HasResultFile = this.HasResultFile;
      destObj.Ex = (EBioException)this.Ex.Clone();
      destObj.pipe = this.pipe;
      destObj.sessionUID = this.sessionUID;
      destObj.owner = this.owner;
      destObj.lastPipedLines = (String[])this.lastPipedLines.Clone();
    }

    public object Clone() {
      RemoteProcessStatePack rslt = CreateObjOfStatePack(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }

    #endregion
  }

}
