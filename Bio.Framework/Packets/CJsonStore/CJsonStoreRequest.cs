using System;
namespace Bio.Framework.Packets {

  using System;
  using Bio.Helpers.Common.Types;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json;
  using System.Collections.Generic;
  using System.Xml;
  using System.ComponentModel;
  using System.Reflection;
  using Bio.Helpers.Common;
#if !SILVERLIGHT
  using System.Data;
  using System.Windows.Forms;
#endif

  public class CJsonStoreRequest : CBioSQLRequest {
    public CJsonStoreData packet { get; set; }
    public CJsonStoreSort sort { get; set; }
    public CJsonStoreFilter filter { get; set; }

    public CJsonStoreRequest() {
      this.requestType = Packets.RequestType.DS;
    }

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as CJsonStoreRequest;
      dst.packet = (this.packet != null) ? (CJsonStoreData)this.packet.Clone() : null;
      dst.sort = (this.sort != null) ? (CJsonStoreSort)this.sort.Clone() : null;
      dst.filter = (this.filter != null) ? (CJsonStoreFilter)this.filter.Clone() : null;
    }

  }

  /// <summary>
  /// Параметры события BeforeLoad
  /// </summary>
  public class CJsonStoreBeforeLoadEventArgs : CancelEventArgs {
    public CJsonStoreBeforeLoadEventArgs() {
      this.waitText = "Загрузка...";
    }
    public String waitText { get; set; }
    public CJsonStoreRequest request { get; set; }

  }


  /// <summary>
  /// Класс, описывающий данные для событий загрузки данных.
  /// </summary>
  public class CJsonStoreAfterLoadEventArgs : EventArgs {
    public Boolean PreserveCurrent { get; set; }
    public CAjaxResponse response { get; set; }
  }
  /// <summary>
  /// Класс, описывающий данные для событий сохранения данных.
  /// </summary>
  public class CJsonStoreAfterPostEventArgs : EventArgs {
    public CAjaxResponse response { get; set; }
  }

  /// <summary>
  /// Класс, описывающий данные для событий навигации по набору данных.
  /// </summary>
  public class CJsonStoreAfterNavigateEventArgs : EventArgs {
    public CAjaxResponse response { get; set; }
    public CJsonStoreRow row { get; set; }
  }

}
