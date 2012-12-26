namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using Bio.Helpers.Common.Types;
  
  public class tm_login_post:ABioHandlerSys {

    public tm_login_post(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    protected override void doExecute() {
      this.BioSession.setLoginState(TBioLoginState.ssLogginIn);
      base.doExecute();
    }
  }
}
