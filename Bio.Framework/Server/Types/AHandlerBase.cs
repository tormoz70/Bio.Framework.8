namespace Bio.Framework.Server {
  using System;
  using System.Xml;
  using System.Web;
  using System.Web.SessionState;
  using System.IO;
  using System.Threading;
  using System.Text;
  //using System.Diagnostics;

  using System.Diagnostics;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common;
  using Bio.Helpers.Common.Types;
  using System.Collections.Specialized;

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

    protected abstract void processAjaxRequest(HttpContext context, CAjaxRequest ajaxRequest);

    public virtual void ProcessRequest(HttpContext context) {
      RequestType v_cur_rqtp = RequestType.Unassigned;
      try {
        this.FLocalPath = context.Request.PhysicalApplicationPath;
        this.FAppURL = context.Request.ApplicationPath;
        context.Response.ContentType = SrvConst.HTML_CONTENT_TYPE;
        context.Response.BufferOutput = true;
        context.Response.ContentEncoding = Encoding.GetEncoding(Utl.SYS_ENCODING);
        CAjaxRequest ar = CAjaxRequest.ExtractFromQParams(context.Request.Params) as CAjaxRequest;
        if (ar is CBioRequestTyped)
          v_cur_rqtp = (ar as CBioRequestTyped).requestType;

        if (ar == null) {
          var v_rqType = context.Request.Params[csRequestTypeParamName];
          var v_bioCode = context.Request.Params[csBioCodeParamName];
          if (String.IsNullOrEmpty(v_rqType) || String.IsNullOrEmpty(v_bioCode))
            ar = new CAjaxRequest();
          else {
            ar = new CBioRequest {
              requestType = enumHelper.GetFieldValueByValueName<RequestType>(v_rqType, StringComparison.CurrentCulture),
              bioCode = v_bioCode,
              bioParams = new CParams(context.Request.Params)
            };
            (ar as CBioRequest).bioParams.Remove(csRequestTypeParamName);
            (ar as CBioRequest).bioParams.Remove(csBioCodeParamName);
          }
        }
        ar.prms = new CParams(context.Request.Params);

        this.processAjaxRequest(context, ar);
      } catch (ThreadAbortException) {
      } catch (EBioUnknownRequest bex) {
        context.Response.Write(new CBioResponse() { success = true, ex = bex }.Encode());
        context.Session.Abandon();
      } catch (EBioLoggedOut bex) {
        context.Response.Write(new CBioResponse() { success = true, ex = bex }.Encode());
        context.Session.Abandon();
      } catch (EBioRestartApp bex) {
        context.Response.Write(new CBioResponse() { success = true, ex = bex }.Encode());
      } catch (EBioException bex) {
        //if ((bex is EBioOk) && (v_cur_rqtp == RequestType.doPing))
          //Данный ответ - является нормальным на doPing если сессия уже существует
          //context.Response.Write(new CBioResponse() { success = true, ex = bex }.Encode());
        //else
          context.Response.Write(new CBioResponse() { success = false, ex = bex }.Encode());
      } catch (Exception bex) {
        EBioException ebioex = new EBioException("Непредвиденная ошибка на сервере.\nСообщение: " + bex.Message, bex);
        context.Response.Write(new CBioResponse() { success = false, ex = ebioex }.Encode());
      }
    }
  }
}