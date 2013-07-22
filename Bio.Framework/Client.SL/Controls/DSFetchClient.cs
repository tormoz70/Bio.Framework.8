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
  public class DSFetchClient:RmtClientBase {
    public String Selection { get; set; }
    public Params ExecBioParams { get; set; }
    public String ExecBioCode { get; set; }

    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="ajaxMng">ссылка на AjaxMng</param>
    /// <param name="bioCode">Код объекта - описание курсора</param>
    /// <param name="execBioCode">Код объекта - описание скрипта</param>
    /// <param name="title">Заголовок</param>
    public DSFetchClient(IAjaxMng ajaxMng, String bioCode, String execBioCode, String title)
      : base(ajaxMng, bioCode, title) {
      this.requestType = RequestType.DSFetch;
      this.ExecBioCode = execBioCode;
    }

    public DSFetchClient()
      : this(null, null, null, null) {
    }

    internal override RmtClientRequest createRequest(RmtClientRequestCmd cmd, Params bioParams, Boolean silent, AjaxRequestDelegate callback) {
      var rslt = this.creRequestOfClient<DSFetchClientRequest>(cmd, bioParams, silent, callback);
      rslt.ExecBioCode = this.ExecBioCode;
      return rslt;
    }

    protected override void doOnRmtProcRunnedSuccess(BioResponse response) {
      //if (response.rmtStatePacket != null)
      //  this.SessionUID = response.rmtStatePacket.sessionUID;
    }

    protected override void doOnRmtProcBeforeRun(RmtClientRequest request) {
      var v_request = request as DSFetchClientRequest;
      v_request.Selection = this.Selection;
      //v_request.execBioParams = this.ExecBioParams;
    }

    public void runProc(Params bioParams, String selection, AjaxRequestDelegate callback) {
      this.Selection = selection;
      //this.ExecBioParams = execBioParams;
      this.RunProc(bioParams, callback);
    }
  }
}
