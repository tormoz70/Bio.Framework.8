namespace Bio.Framework.Server {

  using System;
  using System.Web;
  using Helpers.Common.Types;
  using Packets;
  using Helpers.Common;

  public class ABioHandlerSys:ABioHandler {

    public ABioHandlerSys(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    public String bioCode {
      get {
        var brq = this.FBioRequest as BioRequest;
        return (brq != null) ? brq.BioCode : null;
      }
    }

    private Params _bioParams;
    protected Params bioParams {
      get {
        var brq = this.FBioRequest as BioRequest;
        if (brq != null)
          this._bioParams = brq.BioParams;
        if (this._bioParams == null)
          this._bioParams = new Params();
        return this._bioParams; 
      }
    }

    protected override void doExecute() {
      var rootDomain = SrvUtl.detectRootDomain(this.bioCode);
      var skipLogin = !String.IsNullOrEmpty(rootDomain) && rootDomain.ToLower().Equals("sys");
      this.BioSession.Login(skipLogin);
    }

    public void write_log(String category, String message, String forIoCode) {
      if (String.IsNullOrEmpty(forIoCode) || String.IsNullOrEmpty(this.bioCode) || this.bioCode.StartsWith(forIoCode)) {
        var vLogFileName = this.BioSession.Cfg.TmpPath + "" + category + ".log";
        Utl.AppendStringToFile(vLogFileName, String.Format("{0} : [{1}] : {2}", DateTime.Now, this.bioCode, message), null, true);
      }
    }
    public void write_log(String category, String message) {
      this.write_log(category, message, null);
    }
    public void write_log(String pMessage) {
      this.write_log("common", pMessage, null);
    }

  }
}
