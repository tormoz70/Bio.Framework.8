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
  public class tmio_LongOp: ABioHandlerBio {
    private RmtClientRequest _request = null;

    public tmio_LongOp(HttpContext context, AjaxRequest request)
      : base(context, request) {
      this._request = request as RmtClientRequest;
    }

    protected override void doExecute() {
      base.doExecute();

      var vBldr = new RmtThreadHandler(
        this.BioSession,
        "application/octet-stream", //"application/vnd.ms-excel",
        "longop[" + this.bioCode + "]");
      vBldr.OnRunEvent += new RmtThreadOnRunEvent(this._doOnRunEvent);
      vBldr.DoExecute(this.BioRequest<RmtClientRequest>().cmd, this.bioParams);
    }

    private void _doOnRunEvent(RmtThreadHandler sender, out IRemoteProcInst instance) {
      var rqst = this.BioRequest<LongOpClientRequest>();
      instance = new CLongOpProc(this.BioSession.Cfg.dbSession, null, rqst, this._prepareCmdDelegate);
    }

    private IDbCommand _prepareCmdDelegate(IDbConnection conn, ref String currentSQL, ref Params currentParams) {
      IDbCommand stmt = null;
      XmlElement vDS = this.FBioDesc.DocumentElement;
      if (vDS != null) {
        CJSCursor vCursor = new CJSCursor(conn, vDS, this.bioCode);
        currentParams = (Params)this.bioParams.Clone();
        stmt = vCursor.DoPrepareCommand(currentParams, ref currentSQL, 0);
      }
      return stmt;
    }

  }
}
