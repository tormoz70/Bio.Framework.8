using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Reflection;
#if !SILVERLIGHT
using System.Text;
#endif

namespace Bio.Helpers.Common {
  using Types;
  using Newtonsoft.Json;
  /// <summary>
  /// Делегат добавления строки в лог
  /// </summary>
  /// <param name="pLine"></param>
  public delegate void DOnLogLine(String pLine);
  
  /// <summary>
  /// Аналог Ajax - клиента в JavaScript
  /// </summary>
  public class ajaxUTL {
    
    /// <summary>
    /// ID текущей HTTP-сессии
    /// </summary>
    public static Cookie sessionID;
    /// <summary>
    /// Имя cookie HTTP-сессии
    /// </summary>
    public const String CS_SESSION_ID_NAME = "ASP.NET_SessionId";
    /// <summary>
    /// Время ожидания ответа от сервера.
    /// </summary>
    public const int RequestTimeout = 60000;

    /// <summary>
    /// Добавить строку в лог
    /// </summary>
    /// <param name="pLine">Строка</param>
    /// <param name="pOnLogLine">Метод пишущий в лог</param>
    public static void AddLogLine(String pLine, DOnLogLine pOnLogLine) {
#if DEBUG
      if(pOnLogLine != null) {
        var thrName = Thread.CurrentThread.Name;
        pOnLogLine(String.Format("{0} : [{1}] -:- {2}", DateTime.Now, thrName, pLine));
      }
#endif
    }

    /// <summary>
    /// Вытаскивает URL без параметров
    /// </summary>
    /// <param name="pURL"></param>
    /// <returns></returns>
    public static String ExtractBaseURL(String pURL) {
      var prts = Utl.SplitString(pURL, '?');
      return prts.Length > 0 ? prts[0] : null;
    }


#if !SILVERLIGHT
    private static Mutex syncObj4WebRequest = new Mutex();
    private static HttpWebRequest FCli = null;

    /// <summary>
    /// Выполняет синхронный запрос к серверу
    /// </summary>
    /// <param name="url">URL</param>
    /// <param name="proxy"></param>
    /// <param name="prms">Дополнительные параметры запроса</param>
    /// <param name="userAgentName">Название клиента</param>
    /// <param name="responseText">Ответ сервера</param>
    /// <param name="requestException">Ошибка, которая произошла при запросе</param>
    /// <param name="onLogLine">Метод пишущий лог</param>
    /// <param name="timeOut">Сек</param>
    public static void getDataFromSrv(String url, WebProxy proxy, Params prms, String userAgentName,
                                      ref String responseText, ref EBioException requestException,
                                      DOnLogLine onLogLine, int timeOut) {
      syncObj4WebRequest.WaitOne();
      try {
        responseText = null;
        Uri vUri = null;
        try {
          vUri = new Uri(url);
        } catch (Exception ex) {
          requestException = new EBioException("Строка URL [" + url + "] имеет некорректный формат. Сообщение: " + ex.Message, ex);
          responseText = ex.ToString();
          return;
        }

        FCli = (HttpWebRequest)WebRequest.Create(vUri);
        FCli.Timeout = (timeOut <= 0) ? RequestTimeout : timeOut;
        if (proxy != null)
          FCli.Proxy = proxy;
        FCli.CookieContainer = new CookieContainer();
        if (sessionID != null) {
          FCli.CookieContainer.Add(sessionID);
        }
        AddLogLine("<request>: Host: " + vUri.Host, onLogLine);
        AddLogLine("<request>: URL: " + url, onLogLine);
        FCli.Method = "POST";
        FCli.UserAgent = userAgentName;
        AddLogLine("<request>: Method: " + FCli.Method, onLogLine);
        var vParams = (prms == null) ? new Params() : prms;
        vParams.Add("ajaxrqtimeout", ""+FCli.Timeout);
        var vParamsToPost = vParams.bldUrlParams();

        //String vParamsToPost = ((pParams == null) || (pParams.Count == 0)) ? "emptypost=yes" : pParams.bldUrlParams();
        AddLogLine("<request>: Params: " + vParamsToPost, onLogLine);
        AddLogLine("<request>: " + CS_SESSION_ID_NAME + ": " + ((sessionID != null) ? sessionID.Value : "<null>"), onLogLine);
        //if(vParamsToPost != null) {
        byte[] postArray = Encoding.UTF8.GetBytes(vParamsToPost);
        FCli.ContentType = "application/x-www-form-urlencoded";
        FCli.ContentLength = postArray.Length;
        try {
          Stream postStream = FCli.GetRequestStream();
          try {
            postStream.Write(postArray, 0, postArray.Length);
          } finally {
            if (postStream != null)
              postStream.Close();
          }
        } catch (Exception ex) {
          requestException = new EBioException("Ошибка при обрщении к серверу. Сообщение: " + ex.Message, ex);
          responseText = ex.ToString();
        }
        //}
        DateTime vStartTimeRequest = DateTime.Now;
        HttpWebResponse vRsp = null;
        if (requestException == null) {
          try {
            vRsp = (HttpWebResponse)FCli.GetResponse();
          } catch (Exception ex) {
            requestException = new EBioException("Ошибка при получении ответа с сервера. Сообщение: " + ex.Message + "\n"
              + "Параметры: " + vUri.AbsoluteUri + "?" + vParamsToPost, ex);
            responseText = ex.ToString();
          }
        }
        if((vRsp != null) && FCli.HaveResponse) {
          Cookie vSessIdCoo = vRsp.Cookies[CS_SESSION_ID_NAME];
          if (vSessIdCoo != null) {
            sessionID = vSessIdCoo;
          }
          String vSessionID = null;
          String vCooDom = null;
          String vCooPath = null;
          if (vSessIdCoo != null) {
            vSessionID = (sessionID != null) ? sessionID.Value : null;
            vCooDom = (sessionID != null) ? sessionID.Domain : "<null>";
            vCooPath = (sessionID != null) ? sessionID.Path : "<null>";
          }
          AddLogLine("<recived>: " + CS_SESSION_ID_NAME + ": " + vSessionID, onLogLine);
          AddLogLine("<recived>: Domain: " + vCooDom, onLogLine);
          AddLogLine("<recived>: Path: " + vCooPath, onLogLine);
          var data = vRsp.GetResponseStream();
          var reader = new StreamReader(data, Encoding.UTF8);
          try {
            responseText = reader.ReadToEnd();
            AddLogLine("<recived>: " + responseText, onLogLine);
          } catch (Exception ex) {
            requestException = new EBioException("Ошибка при получении ответа с сервера. Сообщение: " + ex.Message + "\n"
              + "Параметры: " + vUri.AbsoluteUri + "?" + vParamsToPost, ex);
            responseText = ex.ToString();
          } finally {
            if (data != null) data.Close();
            reader.Close();
          }
          if (String.IsNullOrEmpty(responseText))
            requestException = new EBioException("Сервер вернул пустой ответ!");
        }
      } finally {
        FCli = null;
        syncObj4WebRequest.ReleaseMutex();
      }
    }

    /// <summary>
    /// Прервать запрос
    /// </summary>
    public static void abortRequest() {
      if(FCli != null)
        FCli.Abort();
    }

#else
    private static void _processResponseError(Exception e, AjaxRequest request) {
      var response = new AjaxResponse {
        Ex = EBioException.CreateIfNotEBio(e),
        Success = false
      };
      if (request.Callback != null)
        request.Callback(null, new AjaxResponseEventArgs { Request = request, Response = response });
    }
    /// <summary>
    /// Выполняет синхронный запрос к серверу
    /// </summary>
    /// <param name="request">URL</param>
    public static void GetDataFromSrv(AjaxRequest request) {
      try {
        var client = new WebClient();
        client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        client.UploadStringCompleted += (sender, args) => {
          if (args.Error == null) {
            Utl.UiThreadInvoke(new Action<Object, AjaxRequest, UploadStringCompletedEventArgs>((s, r, a) => {
              try {
                var c = GetConvertersFromRequestType(r.GetType());
                var responseText = a.Result;
                var response = CreResponseObject(null, responseText, c);
                if (r.Callback != null)
                  r.Callback(s, new AjaxResponseEventArgs { Request = r, Response = response });
              } catch (Exception ex) {
                _processResponseError(ex, r);
              }
            }), sender, (AjaxRequest)args.UserState, args);
          }else
            _processResponseError(args.Error, (AjaxRequest)args.UserState);
        };
        var uri = new Uri(request.URL, UriKind.Relative);
        var paramsToPost = PrepareRequestParams(request);
        client.UploadStringAsync(uri, "POST", paramsToPost, request);
      } catch (Exception e) {
        _processResponseError(e, request);
      }
    }

    public static String PrepareRequestParams(AjaxRequest request) {
      var qParams = request.BuildQParams(GetConvertersFromRequestType(request.GetType()));
      return qParams.bldUrlParams();
    }

    
    public static void GetFileFromSrv(AjaxRequest request, String appendUrlParams) {
      Action<Exception> doOnErr = e => {
        var response = new AjaxResponse {
          Ex = EBioException.CreateIfNotEBio(e),
          Success = false
        };
        if (request.Callback != null)
          request.Callback(null, new AjaxResponseEventArgs { Request = request, Response = response });
      };
      try {
        var cli = new WebClient();
        cli.OpenReadCompleted += (sender, e) => {
          if (e.Error == null) {
            if (request.Callback != null) {
              var response = new AjaxResponse {
                Success = true
              };
              var a = new AjaxResponseEventArgs {
                Request = request,
                Response = response, 
                Stream = new MemoryStream()
              };
              e.Result.CopyTo(a.Stream);
              e.Result.Close();
              request.Callback(sender, a);
            }


          } else {
            doOnErr(e.Error);
          }
        };
        var paramsToPost = PrepareRequestParams(request);
        var urlParams = String.IsNullOrEmpty(appendUrlParams) ? String.Empty : "&" + appendUrlParams;
        var uri = new Uri(request.URL + "?" + paramsToPost + urlParams, UriKind.Relative);
        cli.OpenReadAsync(uri);
      } catch (Exception ex) {
        doOnErr(ex);
      }
    }
#endif

    public static JsonConverter[] GetConverters() {
      return new JsonConverter[] { new EBioExceptionConverter()/*, new CJsonStoreRowConverter()*/ };
    }

    /// <summary>
    /// Строит строку запроса URL
    /// </summary>
    /// <param name="requestException"></param>
    /// <param name="responseText"></param>
    /// <param name="converters"></param>
    /// <returns></returns>
    public static AjaxResponse CreResponseObject(EBioException requestException, String responseText, JsonConverter[] converters) {
      AjaxResponse response;
      if (requestException == null) {
        try {
          response = AjaxResponse.Decode(responseText, converters);
          response.ResponseText = responseText;
        } catch (Exception e) {
          response = new AjaxResponse {
            Ex = new EBioException("Ошибка при восстановлении объекта Response. Сообщение: " + e.Message + "\nResponseText: " + responseText, e),
            ResponseText = responseText,
            Success = false
          };
        }
      } else {
        response = new AjaxResponse {
          Ex = requestException,
          ResponseText = responseText,
          //request = pRequest,
          Success = false
        };
      }
      return response;
    }

    public static JsonConverter[] GetConvertersFromRequestType(Type rqType) {
      if (rqType == null)
        return null;
      JsonConverter[] converters = null;
      MethodInfo[] ms = rqType.GetMethods();
      MethodInfo getConvertersMethod = null;
      foreach (var m in ms)
        if (m.IsStatic && m.Name.Equals("GetConverters")) {
          getConvertersMethod = m;
          break;
        }
      if (getConvertersMethod != null)
        converters = (JsonConverter[])getConvertersMethod.Invoke(null, new Object[0]);
      if (converters == null) {
        var v_base_type = rqType.BaseType;
        if (v_base_type != null)
          converters = GetConvertersFromRequestType(v_base_type);
      }
      return converters;
    }
  }
}
