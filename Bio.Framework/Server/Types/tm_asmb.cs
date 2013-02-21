namespace Bio.Framework.Server {

  using System;
  using System.Web;
  using Packets;
  using Helpers.Common.Types;
  using System.Reflection;
  using Helpers.Common;
  using System.IO;

  public class tm_asmb:ABioHandlerSys {

    public tm_asmb(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private void _sendFileToClient(String modulePath) {
      this.Context.Response.ClearContent();
      this.Context.Response.ClearHeaders();
      this.Context.Response.ContentType = "application/octet-stream";
      var v_vRemoteFName = Path.GetFileName(modulePath);
      this.Context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + v_vRemoteFName + "\"");
      this.Context.Response.WriteFile(modulePath);
      this.Context.Response.Flush();
      this.Context.Response.Close();
    }


    protected override void doExecute() {
      //Thread.Sleep(5000);
      //base.doExecute();
      var v_moduleName = Params.FindParamValue(this.bioParams, "moduleName") as String;
      var v_fileName = Utl.NormalizeDir(this.BioSession.Cfg.LocalPath) + @"ClientBin\" + v_moduleName;
      var v_getModule = Params.FindParamValue(this.bioParams, "getModule") as String;
      if (String.Equals(v_getModule, "1")) {
        this._sendFileToClient(v_fileName);
      } else {
        var ai = Assembly.LoadFile(v_fileName);
        this.bioParams.SetValue("moduleVersion", ai.GetName().Version.ToString());
        this.Context.Response.Write(new CBioResponse { success = true, bioParams = this.bioParams }.Encode());
      }
    }
  }
}
