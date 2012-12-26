namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.XLFRpt2.Engine;
  using System.Threading;
  using Bio.Helpers.Common;

  /// <summary>
  /// Обработчик запроса внутренностей компонента Rpt
  /// </summary>
  public class tmio_rpt: ABioHandlerBio {
    private CRmtClientRequest _request = null;

    public tmio_rpt(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
        this._request = pRequest as CRmtClientRequest;
    }

    protected override void doExecute() {
      base.doExecute();

      var vBldr = new CRmtThreadHandler(
        this.BioSession,
        "application/octet-stream", //"application/vnd.ms-excel",
        "report[" + this.bioCode + "]");
        vBldr.OnRunEvent += new CRmtThreadOnRunEvent(this.doOnRunEventXLS);
      vBldr.doExecute(this.bioRequest<CRmtClientRequest>().cmd, this.bioParams);
    }

    private void doOnRunEventXLS(CRmtThreadHandler sender, ref IRemoteProcInst instance) {
      CXLReportConfig rptCfg = CXLReportConfig.LoadFromFile(
        null,
        this.bioCode,
        Utl.NormalizeDir(this.BioSession.Cfg.IniPath) + "rpts\\",
        Utl.NormalizeDir(this.BioSession.Cfg.WorkSpacePath) + "rpts\\",
        this.BioSession.Cfg.dbSession,
        this.BioSession.CurSessionID,
        this.BioSession.Cfg.CurUser.USR_NAME,
        this.BioSession.CurSessionRemoteIP,
        this.bioParams,
        !this.BioSession.Cfg.Debug
      );
      instance = new CXLReport(null, rptCfg, this.Context);
    }

  }
}
