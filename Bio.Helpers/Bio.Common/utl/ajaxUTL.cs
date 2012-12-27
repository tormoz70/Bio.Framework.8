namespace Bio.Helpers.Common {
  using System;
  using System.Text;
  using System.Net;
  using System.IO;
  using System.Threading;
  using System.Collections;
  using Bio.Helpers.Common;
  using Bio.Helpers.Common.Types;
  using System.ComponentModel;
  using System.Collections.Generic;
  using Newtonsoft.Json;
  using System.Reflection;
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
    public const String csSessionIdName = "ASP.NET_SessionId";
    /// <summary>
    /// Время ожидания ответа от сервера.
    /// </summary>
    public const int RequestTimeout = 60000;

    /// <summary>
    /// Добавить строку в лог
    /// </summary>
    /// <param name="pLine">Строка</param>
    /// <param name="pOnLogLine">Метод пишущий в лог</param>
    public static void addLogLine(String pLine, DOnLogLine pOnLogLine) {
#if DEBUG
      if(pOnLogLine != null) {
        String vThrName = Thread.CurrentThread.Name;
        pOnLogLine(String.Format("{0} : [{1}] -:- {2}", DateTime.Now, vThrName, pLine));
      }
#endif
    }

    /// <summary>
    /// Вытаскивает URL без параметров
    /// </summary>
    /// <param name="pURL"></param>
    /// <returns></returns>
    public static String extractBaseURL(String pURL) {
      String[] vPrts = Utl.SplitString(pURL, '?');
      if(vPrts.Length > 0)
        return vPrts[0];
      else
        return null;
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
    /// <param name="pOnLogLine">Метод пишущий лог</param>
    public static void getDataFromSrv(String url, WebProxy proxy, CParams prms, String userAgentName,
                                      ref String responseText, ref EBioException requestException,
                                      DOnLogLine pOnLogLine, int timeOut) {
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
        addLogLine("<request>: Host: " + vUri.Host, pOnLogLine);
        addLogLine("<request>: URL: " + url, pOnLogLine);
        FCli.Method = "POST";
        FCli.UserAgent = userAgentName;
        addLogLine("<request>: Method: " + FCli.Method, pOnLogLine);
        CParams vParams = (prms == null) ? new CParams() : prms;
        vParams.Add("ajaxrqtimeout", ""+FCli.Timeout);
        String vParamsToPost = vParams.bldUrlParams();

        //String vParamsToPost = ((pParams == null) || (pParams.Count == 0)) ? "emptypost=yes" : pParams.bldUrlParams();
        addLogLine("<request>: Params: " + vParamsToPost, pOnLogLine);
        addLogLine("<request>: " + csSessionIdName + ": " + ((sessionID != null) ? sessionID.Value : "<null>"), pOnLogLine);
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
          Cookie vSessIdCoo = vRsp.Cookies[csSessionIdName];
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
          addLogLine("<recived>: " + csSessionIdName + ": " + vSessionID, pOnLogLine);
          addLogLine("<recived>: Domain: " + vCooDom, pOnLogLine);
          addLogLine("<recived>: Path: " + vCooPath, pOnLogLine);
          Stream data = vRsp.GetResponseStream();
          StreamReader reader = new StreamReader(data, Encoding.UTF8);
          try {
            responseText = reader.ReadToEnd();
            addLogLine("<recived>: " + responseText, pOnLogLine);
          } catch (Exception ex) {
            requestException = new EBioException("Ошибка при получении ответа с сервера. Сообщение: " + ex.Message + "\n"
              + "Параметры: " + vUri.AbsoluteUri + "?" + vParamsToPost, ex);
            responseText = ex.ToString();
          } finally {
            data.Close();
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
    private static void _processResponseError(Exception e, CAjaxRequest request) {
      CAjaxResponse vResponse = new CAjaxResponse {
        ex = EBioException.CreateIfNotEBio(e),
        success = false
      };
      if (request.callback != null)
        request.callback(null, new AjaxResponseEventArgs { request = request, response = vResponse });
    }
    /// <summary>
    /// Выполняет синхронный запрос к серверу
    /// </summary>
    /// <param name="pURL">URL</param>
    /// <param name="pParams">Дополнительные параметры запроса</param>
    /// <param name="pUserAgentName">Название клиента</param>
    /// <param name="vResponseText">Ответ сервера</param>
    /// <param name="vRequestException">Ошибка, которая произошла при запросе</param>
    /// <param name="pOnLogLine">Метод пишущий лог</param>
    public static void getDataFromSrv(CAjaxRequest request) {
      try {
        WebClient v_client = new WebClient();
        v_client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        v_client.UploadStringCompleted += new UploadStringCompletedEventHandler((Object sender, UploadStringCompletedEventArgs args) => {
          if (args.Error == null) {
            Utl.UiThreadInvoke(new Action<Object, CAjaxRequest, UploadStringCompletedEventArgs>((s, r, a) => {
              try {
                JsonConverter[] c = ajaxUTL.getConvertersFromRequestType(r.GetType());
                String vResponseText = a.Result;
                CAjaxResponse vResponse = ajaxUTL.CreResponseObject(null, vResponseText, c);
                if (r.callback != null)
                  r.callback(s, new AjaxResponseEventArgs { request = r, response = vResponse });
              } catch (Exception ex) {
                _processResponseError(ex, r);
              }
            }), sender, (CAjaxRequest)args.UserState, args);
          }else
            _processResponseError(args.Error, (CAjaxRequest)args.UserState);
        });
        Uri uri = new Uri(request.url, UriKind.Relative);
        String vParamsToPost = prepareRequestParams(request);
        v_client.UploadStringAsync(uri, "POST", vParamsToPost, request);
      } catch (Exception e) {
        _processResponseError(e, request);
      }
    }

    public static String prepareRequestParams(CAjaxRequest request) {
      CParams vParams = request.BuildQParams(ajaxUTL.getConvertersFromRequestType(request.GetType()));
      //vParams.Add("ajaxrqtimeout", "" + request.timeout);
      return vParams.bldUrlParams();
    }

    
    public static void getFileFromSrv(CAjaxRequest request, String appendUrlParams) {
      Action<Exception> p_doOnErr = (e) => {
        CAjaxResponse vResponse = new CAjaxResponse {
          ex = EBioException.CreateIfNotEBio(e),
          success = false
        };
        if (request.callback != null)
          request.callback(null, new AjaxResponseEventArgs { request = request, response = vResponse });
      };
      try {
        WebClient cli = new WebClient();
        cli.OpenReadCompleted += new OpenReadCompletedEventHandler((Object sender, OpenReadCompletedEventArgs e) => {
          if (e.Error == null) {
            //if (callback != null)
            //  callback(e);
            if (request.callback != null) {
              CAjaxResponse vResponse = new CAjaxResponse {
                success = true
              };
              var a = new AjaxResponseEventArgs {
                request = request,
                response = vResponse, 
                stream = new MemoryStream()
              };
              e.Result.CopyTo(a.stream);
              //e.Result.Flush();
              e.Result.Close();
              request.callback(sender, a);
            }


          } else {
            p_doOnErr(e.Error);
          }
        });
        String vParamsToPost = prepareRequestParams(request);
        String v_appendUrlParams = String.IsNullOrEmpty(appendUrlParams) ? String.Empty : "&" + appendUrlParams;
        Uri uri = new Uri(request.url + "?" + vParamsToPost + v_appendUrlParams, UriKind.Relative);
        cli.OpenReadAsync(uri);
      } catch (Exception ex) {
        p_doOnErr(ex);
      }
    }
#endif

    public static JsonConverter[] GetConverters() {
      return new JsonConverter[] { new EBioExceptionConverter()/*, new CJsonStoreRowConverter()*/ };
    }

    /// <summary>
    /// Строит строку запроса URL
    /// </summary>
    /// <param name="pServerUrl">URL сервера до уровня приложения</param>
    /// <returns></returns>

    public static CAjaxResponse CreResponseObject(EBioException requestException, String responseText, JsonConverter[] converters) {
      CAjaxResponse vResponse = null;
      if (requestException == null) {
        try {
          vResponse = CAjaxResponse.Decode(responseText, converters);
          vResponse.responseText = responseText;
        } catch (Exception e) {
          vResponse = new CAjaxResponse() {
            ex = new EBioException("Ошибка при восстановлении объекта Response. Сообщение: " + e.Message + "\nResponseText: " + responseText, e),
            responseText = responseText,
            success = false
          };
        }
      } else {
        vResponse = new CAjaxResponse() {
          ex = requestException,
          responseText = responseText,
          //request = pRequest,
          success = false
        };
      }
      return vResponse;
    }

    public static JsonConverter[] getConvertersFromRequestType(Type rqType) {
      if (rqType == null)
        return null;
      JsonConverter[] converters = null;
      MethodInfo[] ms = rqType.GetMethods();
      MethodInfo getConvertersMethod = null;
      foreach (MethodInfo m in ms)
        if (((MethodInfo)m).IsStatic && ((MethodInfo)m).Name.Equals("GetConverters")) {
          getConvertersMethod = m;
          break;
        }
      if (getConvertersMethod != null)
        converters = (JsonConverter[])getConvertersMethod.Invoke(null, new Object[0]);
      if (converters == null) {
        var v_base_type = rqType.BaseType;
        if (v_base_type != null)
          converters = getConvertersFromRequestType(v_base_type);
      }
      return converters;
    }
  }
}
