namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Framework.Packets;
  using System.ComponentModel;
  using Bio.Helpers.Common.Types;
  using System.IO;
  using Bio.Helpers.Common;

  /// <summary>
  /// Предоставляет возможность запускать на сервере долгоиграющие процессы.
  /// </summary>
  public class LongOpClient:RmtClientBase {

    public String PipeName { get; set; }
    public String SessionUID { get; set; }


    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="ajaxMng">ссылка на AjaxMng</param>
    /// <param name="bioCode">Код</param>
    /// <param name="title">Заголовок</param>
    public LongOpClient(IAjaxMng ajaxMng, String bioCode, String title)
      : base(ajaxMng, bioCode, title) {
      this.requestType = RequestType.srvLongOp;
    }

    public LongOpClient()
      : this(null, null, null) {
    }

    internal override RmtClientRequest createRequest(RmtClientRequestCmd cmd, Params bioParams, Boolean silent, AjaxRequestDelegate callback) {
      var rslt = this.creRequestOfClient<LongOpClientRequest>(cmd, bioParams, silent, callback);
      rslt.Pipe = this.PipeName;
      rslt.SessionUID = this.SessionUID;
      return rslt;
    }

    protected override void doOnRmtProcRunnedSuccess(BioResponse response) {
      if (response.RmtStatePacket != null)
        this.SessionUID = response.RmtStatePacket.sessionUID;
    }

  }
}
