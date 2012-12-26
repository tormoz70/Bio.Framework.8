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
    private CRmtClientRequest _request = null;

    public tmio_DSFetch(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
      this._request = pRequest as CRmtClientRequest;
    }

    protected override void doExecute() {
      base.doExecute();

      var vBldr = new CRmtThreadHandler(
        this.BioSession,
        "application/octet-stream", //"application/vnd.ms-excel",
        "dsfetch[" + this.bioCode + "]");
      vBldr.OnRunEvent += new CRmtThreadOnRunEvent(this._doOnRunEvent);
      vBldr.doExecute(this.bioRequest<CRmtClientRequest>().cmd, this.bioParams);
    }

    private void _doOnRunEvent(CRmtThreadHandler sender, ref IRemoteProcInst instance) {
      var rqst = this.bioRequest<CDSFetchClientRequest>();
      var v_cursorDS = this.FBioDesc.DocumentElement;
      var v_execIO = CIObject.CreateIObject(rqst.execBioCode, this.BioSession);
      var v_execDS = v_execIO.IniDocument.XmlDoc.DocumentElement;
      instance = new CDSFetchProc(
        this.BioSession.Cfg.dbSession, null, rqst, v_cursorDS, v_execDS);
    }

  }
}
