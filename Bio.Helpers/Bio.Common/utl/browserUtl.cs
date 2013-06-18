using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;

namespace Bio.Helpers.Common {
  public class browserUtl {
    private static String csPxFmt = "{0}px";
    private static String csDivHtmlViewerID = "div_htmlViewer";
    public static void showHtml(String html, Point pos, Size size) {
      HtmlElement v_frame = HtmlPage.Document.GetElementById(csDivHtmlViewerID);
      if (v_frame == null) {
        v_frame = HtmlPage.Document.CreateElement("iframe");
        v_frame.SetAttribute("id", csDivHtmlViewerID);
        v_frame.SetStyleAttribute("position", "absolute");
        v_frame.SetStyleAttribute("visibility", "hidden");
        v_frame.SetProperty("src", "javascript:false");
        HtmlPage.Document.Body.AppendChild(v_frame);
        var v_cntWin = v_frame.GetProperty("contentWindow") as HtmlWindow;
        var v_doc = v_cntWin.GetProperty("document") as HtmlDocument;
        if (v_doc != null) {
          v_doc.Invoke("write", String.Format("<body>{0}</body>", html));
        }
      } else {
        var v_cntWin = v_frame.GetProperty("contentWindow") as HtmlWindow;
        var v_doc = v_cntWin.GetProperty("document") as HtmlDocument;
        v_doc.Body.SetProperty("innerHtml", html);
      }
      v_frame.SetStyleAttribute("width", String.Format(csPxFmt, size.Width));
      v_frame.SetStyleAttribute("height", String.Format(csPxFmt, size.Height));
      v_frame.SetStyleAttribute("top", String.Format(csPxFmt, pos.Y));
      v_frame.SetStyleAttribute("left", String.Format(csPxFmt, pos.X));
      v_frame.SetStyleAttribute("z-index", "12000");
      v_frame.SetStyleAttribute("visibility", "visible");
    }

    private static String csIfrmFileLoaderID = "ifrm_downloader";
    private static Action _loadFile_callback = null;
    /// <summary>
    /// Загружает файл (Синхронно)
    /// </summary>
    /// <param name="url"></param>
    /// <param name="callback"></param>
    public static void loadFile(String url, Action callback) {
      HtmlElement iframe = HtmlPage.Document.GetElementById(csIfrmFileLoaderID);
      if (iframe == null) {
        HtmlElement div = HtmlPage.Document.CreateElement("div");
        div.SetStyleAttribute("display", "none");
        div.SetProperty("innerHTML", String.Format("<iframe id='{0}' src='sys/dummy.htm'/>", csIfrmFileLoaderID));
        //div.SetProperty("innerHTML", String.Format("<iframe id='{0}' src='_blank'/>", csIfrmFileLoaderID));
        HtmlPage.Document.Body.AppendChild(div);
        iframe = HtmlPage.Document.GetElementById(csIfrmFileLoaderID);
        //iframe.AttachEvent("onload", new EventHandler((s, a) => { 
        //  if (_loadFile_callback != null) 
        //    _loadFile_callback(); 
        //}));
      }
      _loadFile_callback = callback;
      iframe.SetAttribute("src", url);
    }

    public static void showUrl(Uri url) {
      HtmlPage.Window.Navigate(url, "_blank", "toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,resizable=yes");
    }


    private static String csNullCookie = "nullCookie";
    public static void SetCookie(String key, String value) {
      if (!String.IsNullOrEmpty(key)) {
        var expireDate = DateTime.Now + TimeSpan.FromDays(30);
        value = (!String.IsNullOrEmpty(value) ? value : csNullCookie);
        value = HttpUtility.UrlEncode(value);
        var newCookie = key.Trim() + "=" + value + ";expires=" + expireDate.ToString("R");
        HtmlPage.Document.SetProperty("cookie", newCookie);
        var ttt = GetCookie(key);
      }
    }




    public static String GetCookie(String key) {
      String[] cookies = HtmlPage.Document.Cookies.Split(';');
      foreach (var cookie in cookies) {
        String[] keyValue = cookie.Split('=');
        if (keyValue.Length == 2) {
          if (keyValue[0].ToString().Trim() == key.Trim()) {
            var rslt = HttpUtility.UrlDecode(keyValue[1]);
            rslt = (String.Equals(rslt, csNullCookie) ? null : rslt);
            return rslt;
          }
        }
      }
      return null;
    }


  }
}
