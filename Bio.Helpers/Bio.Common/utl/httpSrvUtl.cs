using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;

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

  }
}
