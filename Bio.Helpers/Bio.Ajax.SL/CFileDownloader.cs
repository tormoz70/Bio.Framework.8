namespace Bio.Helpers.Ajax {
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Text;
  using System.Xml;
  using System.Threading;
  using System.Net;
  using System.IO;
  using System.Windows.Browser;

  /// <summary>
  /// Загрузчик файлов
  /// </summary>
  public static class CFileDownloader {

#if !SILVERLIGHT
    /// <summary>
    /// Загружает файл (Асинхронно)
    /// </summary>
    /// <param name="pToFileName"></param>
    /// <param name="pURL"></param>
    /// <param name="pOnCmpltHndlr"></param>
    /// <param name="pOnPrgHndlr"></param>
    public static void loadFileAsync(String pToFileName, String pURL,
                                AsyncCompletedEventHandler pOnCmpltHndlr,
                                DownloadProgressChangedEventHandler pOnPrgHndlr) {

      WebClient wc = new WebClient();
      if(pOnPrgHndlr != null)
        wc.DownloadProgressChanged += pOnPrgHndlr;
      wc.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) => 
      {
        if(pOnCmpltHndlr != null)
          pOnCmpltHndlr(sender, e);
      });
      wc.Headers.Set(HttpRequestHeader.Cookie, ajaxUTL.csSessionIdName+"="+ajaxUTL.sessionID.Value);
      String vURL = pURL + "&pdummy=" + new Guid().ToString();
      wc.DownloadFileAsync(new Uri(vURL), pToFileName);
    }
#else
    /// <summary>
    /// Загружает файл (Асинхронно)
    /// </summary>
    /// <param name="saveToFileName"></param>
    /// <param name="url"></param>
    /// <param name="callback"></param>
    /// <param name="onProgress"></param>
    public static void loadFileAsync(String saveToFileName, String url,
                                     AsyncCompletedEventHandler callback,
                                     DownloadProgressChangedEventHandler onProgress) {
      WebClient wc = new WebClient();
      wc.OpenReadCompleted += new OpenReadCompletedEventHandler((Object sender, OpenReadCompletedEventArgs e) => {
        //throw new NotImplementedException();
        //FileStream  
        //File.WriteAllBytes();
      });
      if (onProgress != null)
        wc.DownloadProgressChanged += onProgress;
      wc.OpenReadAsync(new Uri(url, UriKind.RelativeOrAbsolute));
    }

    static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
      throw new NotImplementedException();
    }

#endif

#if !SILVERLIGHT
    /// <summary>
    /// Загружает файл (Синхронно)
    /// </summary>
    /// <param name="pToFileName"></param>
    /// <param name="pURL"></param>
    public static void loadFile(String pToFileName, String pURL) {
      //String vURL = bioUTL.utlSTD.BuildURL(pServerHost, "getCli");
      //TParams vURLParams = new TParams();
      //vURLParams.Add(utlSTD.QLOGIN_PARNAME, utlSTD.QLOGIN_PARVALUE);
      //vURLParams.Add(utlSTD.CLINAME_PARNAME, utlSTD.CLINAME_PARVALUE);
      //vURLParams.Add("module", pModulePath);
      //vURL = vURLParams.bldUrlParams(vURL);
      //if(wc == null) wc = new WebClient();
      //wc.DownloadFile(new Uri(vURL), pToFileName);
      WebClient wc = new WebClient();
      wc.Headers.Set(HttpRequestHeader.Cookie, ajaxUTL.csSessionIdName + "=" + ajaxUTL.sessionID.Value);
      String vURL = pURL + "&pdummy=" + new Guid().ToString();
      wc.DownloadFile(new Uri(vURL), pToFileName);
    }
#else
#endif

  }
}
