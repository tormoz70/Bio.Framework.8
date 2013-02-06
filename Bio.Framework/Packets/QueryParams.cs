namespace Bio.Framework.Packets {
#if !SILVERLIGHT
  using System;
  using System.Web;
  using System.Collections;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  public class QueryParams : Params {

    public static QueryParams ParsQString(String pRawUrl) {
      QueryParams vResult = new QueryParams();
      int vPStrt = pRawUrl.IndexOf("?");
      String vParsString = null;
      if (vPStrt >= 0)
        vParsString = HttpUtility.UrlDecode(pRawUrl.Substring(vPStrt + 1));
      else
        vParsString = null;

      if (vParsString != null)
        vParsString = vParsString.Replace("&amp;", "&");

      if (vParsString != null) {
        String[] pars = Bio.Helpers.Common.Utl.SplitString(vParsString, '&');
        for (int i = 0; i < pars.Length; i++) {
          String pName = null;
          String pValue = null;
          if (pars[i].IndexOf("=") >= 0) {
            pName = pars[i].Substring(0, pars[i].IndexOf("="));
            pValue = pars[i].Substring(pars[i].IndexOf("=") + 1);
          } else
            pName = pars[i];

          if (pName != null)
            vResult.Add(pName, pValue);
        }
      }
      return vResult;
    }

    public static QueryParams ParsQString(HttpRequest request) {
      QueryParams vResult = ParsQString(request.RawUrl);
      foreach (String vPrmName in request.Params.AllKeys) {
        if (vResult.ParamByName(vPrmName) == null) {
          vResult.Add(vPrmName, request.Params[vPrmName]);
        }
      }
      return vResult;
    }

  }
#endif

}