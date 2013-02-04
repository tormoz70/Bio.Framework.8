namespace Bio.Framework.Server {
	using System;
	using System.Xml;
	using System.Web;
  using System.Web.SessionState;
	using System.IO;
  using System.Threading;
  using System.Text;
  using System.Collections;

  using Bio.Helpers.DOA;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  public partial class CBioSession {

    public DateTime CreationDT { get; private set; }

    //private String FCurSessionRemoteIP = null;
    //private String FCurSessionID = null;
    //private String FCurSessionRemoteHostName = null;
    //private String FCurSessionRemoteAgent = null;

    //private ABioHandler FCurBioMsg = null;

    private Hashtable FIOS = null;
    //private DBSession FDBSession = null;

    public CBioSession(String localPath, String appURL) {
			this.CreationDT = DateTime.Now;
      this.FIOS = new Hashtable();
      this.Cfg = new CBioCfgOra(localPath, appURL);
      //this.FDBSession = new DBSession(this.Cfg.ConnectionString, null, this.Cfg.doOnAfterDBConnect);

    }

    public String CurSessionRemoteIP { get; private set; }
    public String CurSessionID { get; private set; }
    public String CurSessionRemoteHostName { get; private set; }
    public String CurSessionRemoteAgent { get; private set; }

    public ABioHandler CurBioHandler { get; private set; }

    public String CurBioCode {
      get {
        if (this.CurBioHandler is ABioHandlerSys)
          return (this.CurBioHandler as ABioHandlerSys).bioCode;
        else
          return null;
      }
    }

    public CIObject CurBiObject {
      get {
        var curBioCode = this.CurBioCode;
        if (!String.IsNullOrEmpty(curBioCode))
          return this.IObj_get(curBioCode);
        else
          return null;
      }
    }

    public CIObject IObj_get(String bioCode) {
      return (CIObject)this.FIOS[bioCode];
    }

    public void IObj_set(String bioCode, CIObject iObj) {
      this.FIOS[bioCode] = iObj;
    }
    
    public void Init(ABioHandler pBioMsg) {
      this.CurBioHandler = pBioMsg;
      this.CurSessionRemoteIP = this.CurBioHandler.RemoteIP;
      this.CurSessionID = this.CurBioHandler.Context.Session.SessionID;
      this.CurSessionRemoteHostName = (this.CurBioHandler.Context.Request.UserHostName == "::1") ? "127.0.0.1" : this.CurBioHandler.Context.Request.UserHostName;
      this.CurSessionRemoteAgent = this.CurBioHandler.Context.Request.UserAgent;
    }

    public void DoFinalize() {
      if (Directory.Exists(this.Cfg.TmpPath + this.CurSessionID)) {
        String[] tmps = Directory.GetFiles(this.Cfg.TmpPath + this.CurSessionID);
        for(int i = 0; i < tmps.Length; i++)
          File.Delete(tmps[i]);
        Directory.Delete(this.Cfg.TmpPath + this.CurSessionID, true);
      }
    }

    public String CreationDTStr {
      get {
        return this.CreationDT.ToString("yyyy.MM.dd hh:mm:ss");
      }
    }

    //public DBSession DBSess {
    //  get {
    //    return this.FDBSession;
    //  }
    //}

    //public String prepareConnStr(XmlElement pDSNode) {
    //  String vConn = null;
    //  if(pDSNode != null && pDSNode.HasAttribute("connid"))
    //    vConn = pDSNode.GetAttribute("connid");
    //  vConn = (String.IsNullOrEmpty(vConn)) ? this.CurUser.ConnectionString : this.Cfg.getDBConnStr(vConn);
    //  CParams vCntxEnv = new CParams();
    //  vCntxEnv.SetValue(csSYS_CURUSERUID_PARAM_NAME, this.CurUser.USR_UID);
    //  vCntxEnv.SetValue(csSYS_CURODEPUID_PARAM_NAME, this.CurUser.ODEP_UID);
    //  vCntxEnv.SetValue(csSYS_TITLE_PARAM_NAME, this.BioSysTitle);
    //  String vJStr = CDBFactory.csBIO_CNTXT_ENV_NAME+"="+vCntxEnv.Encode();
    //  Utl.appendStr(ref vConn, vJStr, ";");
    //  return vConn;
    //}

  }
}
