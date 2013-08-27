namespace Bio.Framework.Server {
  using System;
  using System.Web;
  using System.Web.SessionState;
  using System.Threading;
  using System.Text;
  //using System.Diagnostics;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common;
  using Bio.Helpers.Common.Types;

  /// <summary>
  /// Базовый обработчик, от него наследуются все
  /// </summary>
  public abstract class AHandlerBase : IHttpHandler, IRequiresSessionState {
    /// <summary>
    /// для get запросов из сторонних приложений
    /// </summary>
    private const String csRequestTypeParamName = "rqtp";
    private const String csBioCodeParamName = "rqbc";

    protected String FLocalPath = null;
    protected String FAppURL = null;


    public virtual bool IsReusable {
      get { return true; }
    }

    protected abstract void processAjaxRequest(HttpContext context, AjaxRequest ajaxRequest);

    public virtual void ProcessRequest(HttpContext context) {
      var curRqtp = RequestType.Unassigned;
      try {
        this.FLocalPath = context.Request.PhysicalApplicationPath;
        this.FAppURL = context.Request.ApplicationPath;
        context.Response.ContentType = SrvConst.HTML_CONTENT_TYPE;
        context.Response.BufferOutput = true;
        context.Response.ContentEncoding = Encoding.GetEncoding(Utl.SYS_ENCODING);
        var ar = AjaxRequest.ExtractFromQParams(context.Request.Params) as AjaxRequest;
        if (ar is BioRequestTyped)
          curRqtp = (ar as BioRequestTyped).RequestType;

        if (ar == null) {
          var rqType = context.Request.Params[csRequestTypeParamName];
          var bioCode = context.Request.Params[csBioCodeParamName];
          if (String.IsNullOrEmpty(rqType) || String.IsNullOrEmpty(bioCode))
            ar = new AjaxRequest();
          else {
            ar = new BioRequest {
              RequestType = enumHelper.GetFieldValueByValueName<RequestType>(rqType, StringComparison.CurrentCulture),
              BioCode = bioCode,
              BioParams = new Params(context.Request.Params)
            };
            (ar as BioRequest).BioParams.Remove(csRequestTypeParamName);
            (ar as BioRequest).BioParams.Remove(csBioCodeParamName);
          }
        }
        ar.Prms = new Params(context.Request.Params);

        this.processAjaxRequest(context, ar);
      } catch (ThreadAbortException) {
      } catch (EBioUnknownRequest bex) {
        context.Response.Write(new BioResponse { Success = true, Ex = bex }.Encode());
        context.Session.Abandon();
      } catch (EBioLoggedOut bex) {
        context.Response.Write(new BioResponse { Success = true, Ex = bex }.Encode());
        context.Session.Abandon();
      } catch (EBioRestartApp bex) {
        context.Response.Write(new BioResponse { Success = true, Ex = bex }.Encode());
      } catch (EBioException bex) {
        context.Response.Write(new BioResponse { Success = false, Ex = bex }.Encode());
      } catch (Exception bex) {
        var ebioex = new EBioException("Непредвиденная ошибка на сервере.\nСообщение: " + bex.Message, bex);
        context.Response.Write(new BioResponse { Success = false, Ex = ebioex }.Encode());
      }
    }
  }
}