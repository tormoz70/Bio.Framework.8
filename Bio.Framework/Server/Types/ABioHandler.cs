namespace Bio.Framework.Server {

  using System;
  using System.IO;
  using System.Web;
  using System.Xml;
  using Packets;
  using Helpers.Common.Types;
  using Helpers.Common;

  /// <summary>
  /// Ѕазовый класс дл€ все получаемых системой сообщений        
  ///   - взимодействие с системо происходит посредством сообщений,
  ///   чтобы добавить в систему новое сообщение, надо создать клвсс - обработчик сообщени€,
  ///   который наследуетс€ от данного и зарегистрировать его в файле /ini/regmsgs.xml
  /// </summary>
  public abstract class ABioHandler {
    protected XmlDocument FBioDesc = null;
    protected AjaxRequest FBioRequest = null;
    public String RemoteIP { get; private set; }

    protected ABioHandler(HttpContext context, AjaxRequest request) {
      BioSession = null;
      this.RemoteIP = httpSrvUtl.detectRemoteIP(context);
      this.Context = context;
      this.FBioRequest = request;
      if(this.Context != null) {
        this.BioSession = (BioSession)this.Context.Session["BioSessIni"];
        if(this.BioSession == null) {
          this.BioSession = new BioSession(this.Context.Request.PhysicalApplicationPath, this.Context.Request.ApplicationPath);
          this.Context.Session["BioSessIni"] = this.BioSession;
        }
      }
      this.BioSession.Init(this);
      this.QParams = QueryParams.ParsQString(this.Context.Request);
    }

    public T BioRequest<T>() where T : AjaxRequest {
      return this.FBioRequest as T;
    }

    protected String getQParamValue(String pName, bool pMandatory) {
      var param = this.QParams.ParamByName(pName);
      if(pMandatory && (param == null))
        throw new EBioException("Ќе найден об€зательный параметр запроса [" + pName + "]!");
      return param != null ? param.ValueAsString() : null;
    }

    protected abstract void doExecute();
    public void DoExecute() {
      this.doExecute();
    }

    public static dom4cs getHandlersRegistry(HttpContext pContext) {
      var result = (dom4cs)pContext.Session["BioMsgsRegistry"];
      if(result == null) {
        var vHandlersRegFN = pContext.Request.PhysicalApplicationPath + "\\ini\\reghanlers.xml";
        if (!File.Exists(vHandlersRegFN))
          throw new EBioException("Ќе найден файл: реестр сообщений: " + vHandlersRegFN);
        result = dom4cs.OpenDocument(vHandlersRegFN);
        pContext.Session["BioMsgsRegistry"] = result;
      }
      return result;
    }

    public BioSession BioSession { get; private set; }

    public HttpContext Context { get; private set; }

    public QueryParams QParams { get; private set; }

    public String RawUrl {
      get {
        return this.Context.Request.RawUrl;
      }
    }


    public String RqstURL {
      get {
        return this.Context.Request.Path + "?" + this.Context.Request.QueryString;
      }
    }

    public Object GetHttpSessionObject(String pName) {
      if((this.Context != null) && (this.Context.Session != null))
        return this.Context.Session[pName];
      return null;
    }

    public void SetHttpSessionObject(String pName, Object pObject) {
      if((this.Context != null) && (this.Context.Session != null))
        this.Context.Session[pName] = pObject;
    }

    public void RemoveHttpSessionObject(String pName) {
      if((this.Context != null) && (this.Context.Session != null))
        this.Context.Session.Remove(pName);
    }

    public String SelfTypePathWithoutExt {
      get {
        return this.GetType().Namespace.Replace("Bio", "").Replace(".", "\\") + "\\" + this.GetType().Name;
      }
    }
    public String SelfTypeUrlPathWithoutExt {
      get {
        return this.GetType().Namespace.Replace("Bio", "").Replace(".", "/") + "/" + this.GetType().Name;
      }
    }
  }
}
