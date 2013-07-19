namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using System.Data.Common;
  using Bio.Helpers.Common.Types;
  using System.Data;
  using Bio.Framework.Packets;

  /// <summary>
  /// Обработчик запроса внутренностей компонента LongOp
  /// </summary>
  public class tmio_DSFetch : ABioHandlerBio {
    private RmtClientRequest _request = null;

    public tmio_DSFetch(HttpContext context, AjaxRequest request)
      : base(context, request) {
      this._request = request as RmtClientRequest;
    }

    protected override void doExecute() {
      base.doExecute();

      var vBldr = new CRmtThreadHandler(
        this.BioSession,
        "application/octet-stream", //"application/vnd.ms-excel",
        "dsfetch[" + this.bioCode + "]");
      vBldr.OnRunEvent += this._doOnRunEvent;
      vBldr.DoExecute(this.BioRequest<RmtClientRequest>().cmd, this.bioParams);
    }

    private void _doOnRunEvent(CRmtThreadHandler sender, ref IRemoteProcInst instance) {
      if (instance == null) throw new ArgumentNullException("instance");
      var rqst = this.BioRequest<CDSFetchClientRequest>();
      var cursorDS = this.FBioDesc.DocumentElement;
      var execIo = CIObject.CreateIObject(rqst.execBioCode, this.BioSession);
      var execDS = execIo.IniDocument.XmlDoc.DocumentElement;
      instance = new CDSFetchProc(
        this.BioSession.Cfg.dbSession, null, rqst, cursorDS, execDS);
    }

  }
}
