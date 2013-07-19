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
    private RmtClientRequest _request = null;

    public tmio_rpt(HttpContext context, AjaxRequest request)
      : base(context, request) {
        this._request = request as RmtClientRequest;
    }

    protected override void doExecute() {
      base.doExecute();

      var vBldr = new CRmtThreadHandler(
        this.BioSession,
        "application/octet-stream", //"application/vnd.ms-excel",
        "report[" + this.bioCode + "]");
        vBldr.OnRunEvent += this._doOnRunEventXls;
      vBldr.DoExecute(this.BioRequest<RmtClientRequest>().cmd, this.bioParams);
    }

    private void _doOnRunEventXls(CRmtThreadHandler sender, ref IRemoteProcInst instance) {
      var rptCfg = CXLReportConfig.LoadFromFile(
        null,
        this.bioCode,
        Utl.NormalizeDir(this.BioSession.Cfg.IniPath) + "rpts\\",
        Utl.NormalizeDir(this.BioSession.Cfg.WorkspacePath) + "rpts\\",
        this.BioSession.Cfg.dbSession,
        this.BioSession.CurSessionID,
        this.BioSession.Cfg.CurUser.Login,
        this.BioSession.CurSessionRemoteIP,
        this.bioParams,
        !this.BioSession.Cfg.Debug
      );
      instance = new CXLReport(null, rptCfg, this.Context);

      var v_action = String.Format("Запуск отчета \"{1}\". Параметры запуска: {0}", this.bioParams.ToString(), ((CXLReport)instance).RptDefinition.Title);

      SQLCmd.ExecuteScript(
        this.BioSession.Cfg.dbSession,
        "begin givcadmin.utils.reg_usr_activity (" +
        " :p_usr_id," +
        " :p_iobj_cd," +
        " :p_iobj_uid," +
        " :p_action); end;",
        new Params(
          new Param("p_usr_id", this.BioSession.Cfg.CurUser.UID),
          new Param("p_iobj_cd", "XLR-BUILDER"),
          new Param("p_iobj_uid", this.bioCode),
          new Param("p_action", v_action)
        ),
        60
      );

    }

  }
}
