namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;

  public class tm_ping:ABioHandlerBio {

    public tm_ping(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    protected override void doExecute() {
      try {
        base.doExecute();
      } catch (EBioOk bex) {
        var rsp = new CBioResponse() {
          success = true,
          gCfg = new CGlobalCfgPack {
            Debug = this.BioSession.Cfg.Debug
          },
          ex = bex
        };
        this.Context.Response.Write(rsp.Encode());
      }
    }
  }
}
