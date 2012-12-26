namespace Bio.Framework.Server {
  using System;
  using System.IO;
  using System.Web;
  using System.Xml;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Collections;
  using System.Reflection;
  using Bio.Helpers.Common;

  public class SrvUtl {

    public static String bldiniFileName(String pIniPath, String pIOCode) {
      String fLocIOPath = pIOCode.Replace(".", "\\");
      return pIniPath + fLocIOPath + ".xml";
    }

    public static String bldIOPathUrl(String pAppURL, String pIOCode) {
      String vIOCode = pIOCode;
      Regex vr = new Regex("[.]\\w+$", RegexOptions.IgnoreCase);
      vIOCode = vr.Replace(vIOCode, "");
      String vResult = pAppURL + "/ini/" + vIOCode.Replace(".", "/") + "/";
      return vResult;
    }

    public static String detectRootDomain(String pIOCode) {
      if(!String.IsNullOrEmpty(pIOCode)) {
        String[] vIOCodeParts = Utl.SplitString(pIOCode, '.');
        if(vIOCodeParts.Length > 0)
          return vIOCodeParts[0];
      }
      return null;
    }

    public static String bldIOFileUrl(String pAppURL, String pIOCode, String pExt) {
      return pAppURL + "/ini/" + pIOCode.Replace(".", "/") + pExt;
    }

  }
}