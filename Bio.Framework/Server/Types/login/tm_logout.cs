namespace Bio.Framework.Server {
  using System.Web;
  using Bio.Helpers.Common.Types;

  public class tm_logout:ABioHandlerSys {

    public tm_logout(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    protected override void doExecute() {
      this.BioSession.Cfg.dbSession.KillTransactions();
      throw new EBioLoggedOut();
    }
  }
}
