using System.Configuration;
using System.Web.Configuration;

namespace Bio.Framework.Server {
	using System;
	using System.Web;
	using System.Web.SessionState;
	using System.IO;
  using System.Threading;
  using System.Collections;
  using System.Diagnostics;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

	public class Global : System.Web.HttpApplication{

		protected void Application_Start(Object sender, EventArgs e){

    }

    //private static String csMainModuleName = "Ekb.Start.xap";
		protected void Session_Start(Object sender, EventArgs e) {
		  String v_MainModuleName = null;
      //Configuration rootWebConfig = WebConfigurationManager.OpenWebConfiguration(null);
      var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
      if (webConfig.AppSettings.Settings.Count > 0) {
        var customSetting = webConfig.AppSettings.Settings["startModuleName"];
        if (customSetting != null)
          v_MainModuleName = customSetting.Value;
        else
          throw new Exception("No startModuleName application string in WebConfiguration!");
      }


      var v_mainModuleFileName = Utl.NormalizeDir(this.Request.PhysicalApplicationPath) + "\\ClientBin\\" + v_MainModuleName;
      String v_mainModuleChangedTimeStamp = "none";
      if(File.Exists(v_mainModuleFileName)){
        var fi = new FileInfo(v_mainModuleFileName);
        v_mainModuleChangedTimeStamp = fi.LastWriteTime.ToString("yyyyMMdd-HHmmss");
      }
      this.Application.Set("gvStartupName", v_MainModuleName); // + "?timestamp=" + v_mainModuleChangedTimeStamp);
			ArrayList vSessions = null;
			if(this.Application.Get("bioSessionArray") != null)
				vSessions = (ArrayList)this.Application.Get("bioSessionArray");
			if(vSessions == null){
				vSessions = new ArrayList();
				this.Application.Set("bioSessionArray", vSessions);
			}
			vSessions.Add(this.Session);
		}

		protected void Application_BeginRequest(Object sender, EventArgs e){}

		protected void Application_EndRequest(Object sender, EventArgs e){}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e){}

		protected void Application_Error(Object sender, EventArgs e){}

		protected void Session_End(Object sender, EventArgs e){
      try {
        BioSession bioSession = (BioSession)this.Session["BioSessIni"];
        if(bioSession != null) {
          bioSession.DoFinalize();
          if ((bioSession.Cfg != null) && (bioSession.Cfg.CurUser != null)) {
            bioSession.Cfg.regConnect(bioSession.Cfg.CurUser.Login,
                                      bioSession.CurSessionID,
                                      bioSession.CurSessionRemoteIP,
                                      bioSession.CurSessionRemoteHostName,
                                      bioSession.CurSessionRemoteAgent,
                                      TRemoteConnectionStatus.rcsOff);
          }
        }

        ArrayList vSessions = null;
        if(this.Application.Get("bioSessionArray") != null)
          vSessions = (ArrayList)this.Application.Get("bioSessionArray");
        if(vSessions != null)
          vSessions.Remove(this.Session);
      } catch(ThreadAbortException) {
      } catch(Exception ex) {
        CEventLog vEve = new CEventLog(null, null);
        vEve.WriteEvent(ex.ToString());
      }
		}

		protected void Application_End(Object sender, EventArgs e){
      //TXLRQueue vRptQueue = (TXLRQueue)this.Application.Get("GXLRQueue");
      //if(vRptQueue != null){
      //  if(vRptQueue.Count > 0){
      //    vRptQueue.GetReport(0).;
      //  }
      //}
    }
			
	}
}

