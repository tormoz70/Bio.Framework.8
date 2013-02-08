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
  public class CLongOpClient:CRmtClientBase {

    public String PipeName { get; set; }
    public String SessionUID { get; set; }


    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="ajaxMng">ссылка на AjaxMng</param>
    /// <param name="bioCode">Код</param>
    /// <param name="title">Заголовок</param>
    public CLongOpClient(IAjaxMng ajaxMng, String bioCode, String title)
      : base(ajaxMng, bioCode, title) {
      this._requestType = RequestType.srvLongOp;
    }

    public CLongOpClient()
      : this(null, null, null) {
    }

    internal override CRmtClientRequest createRequest(RmtClientRequestCmd cmd, Params bioParams, Boolean silent, AjaxRequestDelegate callback) {
      var rslt = this.creRequestOfClient<CLongOpClientRequest>(cmd, bioParams, silent, callback);
      rslt.pipe = this.PipeName;
      rslt.sessionUID = this.SessionUID;
      return rslt;
    }

    protected override void doOnRmtProcRunnedSuccess(CBioResponse response) {
      if (response.rmtStatePacket != null)
        this.SessionUID = response.rmtStatePacket.sessionUID;
    }

  }
}
