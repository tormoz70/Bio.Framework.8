namespace Bio.Framework.Packets {
  using System;
  using Newtonsoft.Json;
  using Helpers.Common.Types;
  using Helpers.Common;

  /// <summary>
  /// Глобальные настройки с сервера. То что прописано в config.xml
  /// </summary>
  public class GlobalCfgPack : ICloneable {
    /// <summary>
    /// Система находится в режиме отладки
    /// </summary>
    public Boolean Debug { get; set; }

    public static GlobalCfgPack Decode(String pJsonString) {
      return jsonUtl.decode<GlobalCfgPack>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }


    #region ICloneable Members

    protected virtual void copyThis(ref GlobalCfgPack destObj) {
      destObj.Debug = this.Debug;
    }

    public object Clone() {
      GlobalCfgPack rslt = new GlobalCfgPack();
      this.copyThis(ref rslt);
      return rslt;
    }

    #endregion
  }

}
