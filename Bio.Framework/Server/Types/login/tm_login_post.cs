namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using Bio.Helpers.Common.Types;
  
  public class tm_login_post:ABioHandlerSys {

    public tm_login_post(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    protected override void doExecute() {
      this.BioSession.SetLoginState(TBioLoginState.ssLogginIn);
      base.doExecute();
    }
  }
}
