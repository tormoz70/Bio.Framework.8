namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using Bio.Helpers.DOA;

  class CBioSelection {
    private bool FInvert = false;
    private CParams FPKs = null;

    public bool extractInvert(String pBioSelection) {
      bool vRslt = false;
      Regex vr = new Regex("\"invert\" *: *true|false\\b", RegexOptions.IgnoreCase);
      Match m = vr.Match(pBioSelection);
      if(m.Success) { 
        String[] vPrts = Utl.SplitString(m.Value, ':');
        if((vPrts.Length == 2) && (vPrts[1].Trim().ToUpper().Equals("TRUE")))
          vRslt = true;
      }
      return vRslt;
    }

    private void killTrailerChars(ref String vStr, char pTrailerCharBgn, char pTrailerCharEnd) {
      if(vStr[0] == pTrailerCharBgn) vStr = vStr.Remove(0, 1);
      if(vStr[vStr.Length - 1] == pTrailerCharEnd) vStr = vStr.Remove(vStr.Length - 1, 1);
    }


    private CParams extractPKItem(String pBioSelItem, CJSCursor pCur) {
      CParams vRslt = new CParams();
      String vBioSelItem = pBioSelItem;
      this.killTrailerChars(ref vBioSelItem, '(', ')');
      String[] vBioSelItemVals = Utl.SplitString(vBioSelItem, ")-(");
      for(int i = 0; i < vBioSelItemVals.Length; i++) {
        String vKey = "" + (i + 1);
        if(pCur.PKFields.ContainsKey(vKey)) {
          CField vPKFld = (CField)pCur.PKFields[vKey];
          vRslt.Add(vPKFld.FieldName, vBioSelItemVals[i], vPKFld);
        }
      }
      return vRslt;

    }

    private CParams extractPKS(String pBioSelection, CJSCursor pCur) {
      CParams vRslt = new CParams();
      String vRsltStr = null;
      Regex vr = new Regex("\"pks\" *: *.+}", RegexOptions.IgnoreCase);
      Match m = vr.Match(pBioSelection);
      if(m.Success) {
        String[] vPrts = Utl.SplitString(m.Value, ':');
        if(vPrts.Length == 2)
          vRsltStr = vPrts[1].Trim().Substring(0, vPrts[1].Length - 1);
      }
      if(vRsltStr != null) {
        killTrailerChars(ref vRsltStr, '[', ']');
        String[] vRows = Utl.SplitString(vRsltStr, ',');
        for(int i = 0; i < vRows.Length; i++ ) {
          String vStrItem = vRows[i];
          vStrItem = vStrItem.Trim();
          this.killTrailerChars(ref vStrItem, '"', '"');
          CParams vPKRow = this.extractPKItem(vStrItem, pCur);
          vRslt.Add("ROW_ID", vStrItem, vPKRow);
        }
      }
      return vRslt;
    }

    public bool Invert {
      get {
        return this.FInvert;
      }
    }

    public CParams PKs {
      get {
        return this.FPKs;
      }
    }

    public void PrepareSQL(String selection, CJSCursor cursor, ref String sql) {
      this.FInvert = this.extractInvert(selection);
      this.FPKs = this.extractPKS(selection, cursor);

      String vWhereSQL = null;
      String vInvertStr = "";
      if(this.Invert)
        vInvertStr = "not ";
      foreach(var pk in this.PKs) {
        String oneCond = null;
        foreach(var fldItem in ((CParams)pk.InnerObject)) {
          var fld = (CField)fldItem.InnerObject;
          Utl.AppendStr(ref oneCond, fld.FieldName + " = " + ":" + fld.FieldName + "", " and ");
          cursor.Params.Add(fld.FieldName, fldItem.Value);
        }
        if(vWhereSQL == null)
          vWhereSQL = " " + vInvertStr + "(" + oneCond + ")";
        else
          vWhereSQL += " and " + vInvertStr + "(" + oneCond + ")";
      }

      if(vWhereSQL != null)
        sql = String.Format("SELECT * FROM ({0}) WHERE {1}", sql, vWhereSQL);

    }
  }
}
