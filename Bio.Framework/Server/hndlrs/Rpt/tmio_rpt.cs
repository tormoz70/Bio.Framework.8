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
  using Bio.Helpers.DOA;

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

      var v_action = String.Format("Запуск отчета \"{1}\". Параметры запуска: {0}", this.bioParams.ToString(), ((CXLReport)instance).RptDefinition.Title);

      CSQLCmd.ExecuteScript(
        this.BioSession.Cfg.dbSession,
        "begin givcadmin.utils.reg_usr_activity (" +
        " :p_usr_id," +
        " :p_iobj_cd," +
        " :p_iobj_uid," +
        " :p_action); end;",
        new CParams(
          new CParam("p_usr_id", this.BioSession.Cfg.CurUser.USR_UID),
          new CParam("p_iobj_cd", "XLR-BUILDER"),
          new CParam("p_iobj_uid", this.bioCode),
          new CParam("p_action", v_action)
        ),
        60
      );

    }

  }
}
