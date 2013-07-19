namespace Bio.Framework.Packets {

  using System;
  using Helpers.Common.Types;
  using System.ComponentModel;

#if !SILVERLIGHT
  using System.Data;
  using System.Windows.Forms;
#endif

  public class JsonStoreRequest : BioSQLRequest {
    public CJsonStoreData Packet { get; set; }
    public CJsonStoreSort Sort { get; set; }
    public CJsonStoreFilter Filter { get; set; }

    public JsonStoreRequest() {
      this.RequestType = RequestType.DS;
    }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as JsonStoreRequest;
      if (dst != null) {
        dst.Packet = (this.Packet != null) ? (CJsonStoreData) this.Packet.Clone() : null;
        dst.Sort = (this.Sort != null) ? (CJsonStoreSort) this.Sort.Clone() : null;
        dst.Filter = (this.Filter != null) ? (CJsonStoreFilter) this.Filter.Clone() : null;
      }
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
    public JsonStoreRequest request { get; set; }

  }


  /// <summary>
  /// Класс, описывающий данные для событий загрузки данных.
  /// </summary>
  public class CJsonStoreAfterLoadEventArgs : EventArgs {
    public Boolean PreserveCurrent { get; set; }
    public AjaxResponse response { get; set; }
  }
  /// <summary>
  /// Класс, описывающий данные для событий сохранения данных.
  /// </summary>
  public class CJsonStoreAfterPostEventArgs : EventArgs {
    public AjaxResponse response { get; set; }
  }

  /// <summary>
  /// Класс, описывающий данные для событий навигации по набору данных.
  /// </summary>
  public class CJsonStoreAfterNavigateEventArgs : EventArgs {
    public AjaxResponse response { get; set; }
    public JsonStoreRow row { get; set; }
  }

}
