namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using Bio.Helpers.Common.Types;

  public class tm_logout:ABioHandlerSys {

    public tm_logout(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    protected override void doExecute() {
      //this.BioSession.setLoginState(TBioLoginState.ssLogginIn);
      //base.doExecute();
      //this.Context.Session.Abandon();
      //this.Context.Response.Redirect(this.Context.Request.ApplicationPath + "/srv.aspx");
      this.BioSession.Cfg.dbSession.KillTransactions();
      throw new EBioLoggedOut();
    }
  }
}
