namespace Bio.Framework.Server {
  using System.Web;
  using Packets;
  using Helpers.Common.Types;

  public class tm_ping:ABioHandlerBio {

    public tm_ping(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    protected override void doExecute() {
      try {
        base.doExecute();
      } catch (EBioOk bex) {
        var rsp = new BioResponse() {
          Success = true,
          GCfg = new GlobalCfgPack {
            Debug = this.BioSession.Cfg.Debug
          },
          Ex = bex
        };
        this.Context.Response.Write(rsp.Encode());
      }
    }
  }
}
