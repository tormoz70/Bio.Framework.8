namespace Bio.Framework.Server {
	using System;
	using System.Web;
	using System.IO;
  using Bio.Helpers.Common.Types;

  public class Handler:AHandlerBase {
    protected override void processAjaxRequest(HttpContext context, CAjaxRequest ajaxRequest) {
      ABioHandler vMsg = CBioHandlerFactory.CreateBioHandler(context, ajaxRequest);
      if(vMsg != null)
        vMsg.DoExecute();
    }
	}
}