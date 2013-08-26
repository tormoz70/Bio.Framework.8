namespace Bio.Framework.Server {
	using System;
	using System.Web;
	using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  public class Handler:AHandlerBase {
    protected override void processAjaxRequest(HttpContext context, AjaxRequest ajaxRequest) {
      var vMsg = CBioHandlerFactory.CreateBioHandler(context, ajaxRequest);

      if (vMsg.BioSession.Cfg.Debug) {
        var headersFN = vMsg.BioSession.Cfg.WorkspacePath + "\\debug\\hdrs(dir)_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        httpSrvUtl.saveHeaders(headersFN, context);
      }

      vMsg.DoExecute();
    }
	}
}