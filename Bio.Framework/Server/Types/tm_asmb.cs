namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using System.Web.Configuration;
  using System.Reflection;
  using Bio.Helpers.Common;
  using System.IO;
  using System.Threading;

  public class tm_asmb:ABioHandlerSys {

    public tm_asmb(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private void sendFileToClient(String modulePath) {
      this.Context.Response.ClearContent();
      this.Context.Response.ClearHeaders();
      this.Context.Response.ContentType = "application/octet-stream";
      String vRemoteFName = Path.GetFileName(modulePath);
      this.Context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + vRemoteFName + "\"");
      this.Context.Response.WriteFile(modulePath);
      this.Context.Response.Flush();
      this.Context.Response.Close();
    }


    protected override void doExecute() {
      //Thread.Sleep(5000);
      //base.doExecute();
      String moduleName = Params.FindParamValue(this.bioParams, "moduleName") as String;
      String v_fileName = Utl.NormalizeDir(this.BioSession.Cfg.LocalPath) + @"ClientBin\" + moduleName;
      String getModule = Params.FindParamValue(this.bioParams, "getModule") as String;
      if (String.Equals(getModule, "1")) {
        this.sendFileToClient(v_fileName);
      } else {
        Assembly ai = Assembly.LoadFile(v_fileName);
        this.bioParams.SetValue("moduleVersion", ai.GetName().Version.ToString());
        this.Context.Response.Write(new CBioResponse() { success = true, bioParams = this.bioParams }.Encode());
      }
    }
  }
}
