namespace Bio.Framework.Server {

  using System;
  using System.IO;
  using System.Web;
  using System.Collections.Generic;
  using System.Xml;
  using System.Text;
  using System.Reflection;

  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  /// <summary>
  /// Ѕазовый класс дл€ все получаемых системой сообщений        
  ///   - взимодействие с системо происходит посредством сообщений,
  ///   чтобы добавить в систему новое сообщение, надо создать клвсс - обработчик сообщени€,
  ///   который наследуетс€ от данного и зарегистрировать его в файле /ini/regmsgs.xml
  /// </summary>
  public abstract class ABioHandler {

    private HttpContext FContext = null;
    private CQueryParams FQParams = null;
    private CBioSession FBioSession = null;
    protected XmlDocument FBioDesc = null;
    protected CAjaxRequest FBioRequest = null;
    public String RemoteIP { get; private set; }
    //protected XMLResponse FXMLResponse = null;

    public ABioHandler(HttpContext context, CAjaxRequest request) {
      this.RemoteIP = httpSrvUtl.detectRemoteIP(context);
      this.FContext = context;
      this.FBioRequest = request;
      if(this.FContext != null) {
        this.FBioSession = (CBioSession)this.FContext.Session["BioSessIni"];
        if(this.FBioSession == null) {
          this.FBioSession = new CBioSession(this.FContext.Request.PhysicalApplicationPath, this.FContext.Request.ApplicationPath);
          this.FContext.Session["BioSessIni"] = this.FBioSession;
        }
      }
      this.FBioSession.Init(this);
      //this.FXMLResponse = new XMLResponse(this);
      this.FQParams = CQueryParams.ParsQString(this.FContext.Request);
    }

    public T bioRequest<T>() where T : CAjaxRequest {
      return this.FBioRequest as T;
    }

    protected String getQParamValue(String pName, bool pMandatory) {
      CParam vParam = this.QParams.ParamByName(pName);
      if(pMandatory && (vParam == null))
        throw new EBioException("Ќе найден об€зательный параметр запроса [" + pName + "]!");
      if(vParam != null)
        return vParam.ValueAsString();
      else
        return null;
    }

    //protected String buildBackResponseXSL() {
    //  return this.BioSession.LocalPath + "src" + this.selfTypePathWithoutExt + ".xsl";
    //}

    //protected void sendBackResponseStd() {
    //  this.sendBackResponseStd(false);
    //}
    //protected void sendBackResponseStd(bool pDoNotTransform) {
    //  if(pDoNotTransform)
    //    this.FXMLResponse.Send(this.Context.Response);
    //  else {
    //    String vXSL_fileName = this.buildBackResponseXSL();
    //    this.FXMLResponse.Send(this.Context.Response, vXSL_fileName);
    //  }
    //}

    //protected abstract void doBeforeExecute();
    protected abstract void doExecute();
    public void DoExecute() {
      //this.doBeforeExecute();
      this.doExecute();
    }

    public static dom4cs getHandlersRegistry(HttpContext pContext) {
      dom4cs vResult = (dom4cs)pContext.Session["BioMsgsRegistry"];
      if(vResult == null) {
        String vHandlersRegFN = pContext.Request.PhysicalApplicationPath + "\\ini\\reghanlers.xml";
        if (!File.Exists(vHandlersRegFN))
          throw new EBioException("Ќе найден файл: реестр сообщений: " + vHandlersRegFN);
        vResult = dom4cs.OpenDocument(vHandlersRegFN);
        pContext.Session["BioMsgsRegistry"] = vResult;
      }
      return vResult;
    }

    public CBioSession BioSession {
      get {
        return this.FBioSession;
      }
    }

    public HttpContext Context {
      get {
        return this.FContext;
      }
    }

    public CQueryParams QParams {
      get {
        return this.FQParams;
      }
    }

    public String RawUrl {
      get {
        return this.FContext.Request.RawUrl;
      }
    }


    public String RqstURL {
      get {
        return this.FContext.Request.Path + "?" + this.FContext.Request.QueryString;
      }
    }

    public Object GetHttpSessionObject(String pName) {
      if((this.FContext != null) && (this.FContext.Session != null))
        return this.FContext.Session[pName];
      else
        return null;
    }

    public void SetHttpSessionObject(String pName, Object pObject) {
      if((this.FContext != null) && (this.FContext.Session != null))
        this.FContext.Session[pName] = pObject;
    }

    public void RemoveHttpSessionObject(String pName) {
      if((this.FContext != null) && (this.FContext.Session != null))
        this.FContext.Session.Remove(pName);
    }

    public String selfTypePathWithoutExt {
      get {
        return this.GetType().Namespace.Replace("Bio", "").Replace(".", "\\") + "\\" + this.GetType().Name;
      }
    }
    public String selfTypeUrlPathWithoutExt {
      get {
        return this.GetType().Namespace.Replace("Bio", "").Replace(".", "/") + "/" + this.GetType().Name;
      }
    }
  }
}
