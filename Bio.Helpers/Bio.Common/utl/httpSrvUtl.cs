using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.IO;

namespace Bio.Helpers.Common {
  public static class httpSrvUtl {
    public static String findHeader(String keyName, NameValueCollection coll) {
      String[] keys = coll.AllKeys;
      foreach (String key in keys) {
        if (String.Equals(key, keyName, StringComparison.CurrentCulture)) {
          return coll.Get(key);
        }
      }
      return null;
    }
    public static String detectRemoteIP(HttpContext context) {
      String v_remoteAddr = context.Request.UserHostAddress;
      String realRemoteAddr = findHeader("X-Real-IP", context.Request.Headers);
      if (!String.IsNullOrEmpty(realRemoteAddr))
        v_remoteAddr = realRemoteAddr;
      v_remoteAddr = (v_remoteAddr == "::1") ? "127.0.0.1" : v_remoteAddr;
      return v_remoteAddr;
    }

    public static void saveHeaders(String fileName, HttpContext context) {
      if (!Directory.Exists(Path.GetDirectoryName(fileName)))
        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      if (File.Exists(fileName))
        File.Delete(fileName);

      Utl.AppendStringToFile(fileName, "QueryString: " + context.Request.QueryString, Encoding.Default);

      Utl.AppendStringToFile(fileName, "*** Headers ***".PadRight(80, '*'), Encoding.Default);
      foreach (var vKey in context.Request.Headers.AllKeys) {
        Utl.AppendStringToFile(fileName, vKey + ":", Encoding.Default);
        int indx = 0;
        foreach (var vVal in context.Request.Headers.GetValues(vKey)) {
          Utl.AppendStringToFile(fileName, "\t\t" + indx + ": " + vVal, Encoding.Default);
          indx++;
        }
      }
      Utl.AppendStringToFile(fileName, "*** Form ***".PadRight(80, '*'), Encoding.Default);
      foreach (var vKey in context.Request.Form.AllKeys) {
        Utl.AppendStringToFile(fileName, vKey + ":", Encoding.Default);
        int indx = 0;
        foreach (var vVal in context.Request.Form.GetValues(vKey)) {
          Utl.AppendStringToFile(fileName, "\t\t" + indx + ": " + vVal, Encoding.Default);
          indx++;
        }
      }
    }

  }
}
