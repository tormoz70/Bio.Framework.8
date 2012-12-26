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
  /// Глобальные настройки с сервера. То что прописано в config.xml
  /// </summary>
  public class CGlobalCfgPack : ICloneable {
    /// <summary>
    /// Система находится в режиме отладки
    /// </summary>
    public Boolean Debug { get; set; }

    public static CGlobalCfgPack Decode(String pJsonString) {
      return jsonUtl.decode<CGlobalCfgPack>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }


    #region ICloneable Members

    protected virtual void copyThis(ref CGlobalCfgPack destObj) {
      destObj.Debug = this.Debug;
    }

    public object Clone() {
      CGlobalCfgPack rslt = new CGlobalCfgPack();
      this.copyThis(ref rslt);
      return rslt;
    }

    #endregion
  }

}
