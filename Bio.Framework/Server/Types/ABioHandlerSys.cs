namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using System.Xml;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common;

  public class ABioHandlerSys:ABioHandler {

    public ABioHandlerSys(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    public String bioCode {
      get {
        CBioRequest brq = this.FBioRequest as CBioRequest;
        return (brq != null) ? brq.bioCode : null;
      }
    }

    private Params _bioParams = null;
    protected Params bioParams {
      get {
        CBioRequest brq = this.FBioRequest as CBioRequest;
        if (brq != null)
          this._bioParams = brq.bioParams;
        if (this._bioParams == null)
          this._bioParams = new Params();
        return this._bioParams; 
      }
    }

    //private Params parsIOParams(String pIOParams) {
    //  Params vResult = Params.Decode(pIOParams);
    //  //if(pIOParams != null) {
    //  //  LitJson_killd.JsonData vData = null;
    //  //  try {
    //  //    vData = LitJson_killd.JsonMapper.ToObject(pIOParams);
    //  //  } catch(LitJson_killd.JsonException ex) {
    //  //    throw new EBioException("Ошибочный формат параметров ИО. Текст: " + pIOParams, ex);
    //  //  }
    //  //  foreach(KeyValuePair<String, LitJson_killd.JsonData> vItem in vData) {
    //  //    Object vParVal = null;
    //  //    if(vItem.Value != null)
    //  //      if(vItem.Value.IsBoolean)
    //  //        vParVal = vItem.Value.ToBoolean(null);
    //  //      else if(vItem.Value.IsDouble)
    //  //        vParVal = vItem.Value.ToDouble(null);
    //  //      else if(vItem.Value.IsInt)
    //  //        vParVal = vItem.Value.ToInt32(null);
    //  //      else if(vItem.Value.IsLong)
    //  //        vParVal = vItem.Value.ToInt64(null);
    //  //      else
    //  //        vParVal = vItem.Value.ToString(null);

    //  //    vResult.Add(vItem.Key, vParVal);
    //  //  }
    //  //}
    //  //if(this.BioSession.CurUser != null) {
    //  //  vResult.Add(BioSession.csSYS_CURUSERUID_PARAM_NAME, this.BioSession.CurUser.USR_UID);
    //  //  vResult.Add(BioSession.csSYS_CURORGUID_PARAM_NAME, this.BioSession.CurUser.ORG_UID);
    //  //  vResult.Add(BioSession.csSYS_TITLE_PARAM_NAME, this.BioSession.BioSysTitle);
    //  //}
    //  return vResult;
    //}

    //protected override void doBeforeExecute() {
    //  //this.bioCode = this.getQParamValue("iocd", false);
    //  //this.FIOParamsStr = this.getQParamValue("ioprm", false);
    //  //this.FIOParams = this.parsIOParams(this.FIOParamsStr);
    //  //Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "curuser", this.BioSession.CurUserName);
    //  //if(this.BioSession.CurUser != null) {
    //  //  Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "curuser_uid", this.BioSession.CurUser.USR_UID);
    //  //  Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "curorg_uid", this.BioSession.CurUser.ORG_UID);
    //  //  Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "curorg_name", this.BioSession.CurUser.ORG_NAME);
    //  //}
    //  //Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "appurl", this.BioSession.AppURL);
    //  //Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "biosys_title", this.BioSession.BioSysTitle);
    //}

    protected override void doExecute() {
      //try {
      //  //bool vIsLoginPostMsg = this.GetType() == typeof(tm_login_post);
        String vRootDomain = SrvUtl.detectRootDomain(this.bioCode);
        Boolean vSkipLogin = !String.IsNullOrEmpty(vRootDomain) && vRootDomain.ToLower().Equals("sys");
        this.BioSession.login(vSkipLogin);
      //} catch(EBioStart) {
      //  //if(this.GetType().Equals(typeof(tm_ws)))
      //  //  throw new EBioRestartApp();
      //  //else
      //  throw;
      //}
    }

    public void write_log(String pCategory, String pMessage, String pForIOCode) {
      if (String.IsNullOrEmpty(pForIOCode) || String.IsNullOrEmpty(this.bioCode) || this.bioCode.StartsWith(pForIOCode)) {
        String vLogFileName = this.BioSession.Cfg.TmpPath + "" + pCategory + ".log";
        Utl.AppendStringToFile(vLogFileName, String.Format("{0} : [{1}] : {2}", DateTime.Now, this.bioCode, pMessage), null, true);
      }
    }
    public void write_log(String pCategory, String pMessage) {
      this.write_log(pCategory, pMessage, null);
    }
    public void write_log(String pMessage) {
      this.write_log("common", pMessage, null);
    }

  }
}
