namespace Bio.Helpers.XLFRpt2.Engine {
	
	using System;
	using System.Xml;
	using System.Web;
	using System.IO;
	using System.Threading;
  using System.Data;
  using Bio.Helpers.Common.Types;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
using Bio.Helpers.Common;
#endif

  public partial class ExcelSrv:DisposableObject {

		public const String xlrModuleName = "XLRTmpModule";
    public const char csRowRangesListDelimeter = ';';
    public const char csRowRangetDelimeter = ':';
    public const int ciMaxUnionRangesCount = 30;

    public static String ExtractFieldName(String value) {
      String vFieldName = null;
      if((value != null) && (value.IndexOf("_") >= 0) && (value.IndexOf("_") < (value.Length - 1)))
        vFieldName = value.Substring(value.IndexOf("_") + 1);
      return vFieldName;
    }
    public static String CellA1Str(int row, int col) {
      String vResult = null;
      if((row > 0) && (col > 0)) {
        String vBase = "";
        if(col > 26)
          vBase = "" + (char)((byte)('A') + (col - 1) / 26 - 1);
        vResult = vBase + (char)((byte)('A') + Bio.Helpers.Common.Utl.Mod((col - 1), 26)) + ("" + row);
      }
      return vResult;
    }
    public static String[] ParsColRowRanges(int colBegin, int colEnd, String rowRangesList) {
      String[] vRslt = new String[0];
      if((rowRangesList != null) && (!rowRangesList.Equals(""))) {
        String[] vRowRanges = Bio.Helpers.Common.Utl.SplitString(rowRangesList, ExcelSrv.csRowRangesListDelimeter);
        int vRangsCount = ExcelSrv.CalcRangesGroupCount(vRowRanges.Length);
        vRslt = new String[vRangsCount];
        int vCurResultIndex = 0;
        for(int i = 0; i < vRowRanges.Length; i++) {
          if(i >= (ExcelSrv.ciMaxUnionRangesCount * (vCurResultIndex + 1))) vCurResultIndex++;
          String vRng = ExcelSrv.BuldRngFromCoord(colBegin, colEnd, vRowRanges[i]);
          if(vRslt[vCurResultIndex] == null)
            vRslt[vCurResultIndex] = vRng;
          else
            vRslt[vCurResultIndex] += new String(ExcelSrv.csRowRangesListDelimeter, 1) + vRng;
        }
      }
      return vRslt;
    }

    public static Excel.Range getRange(Excel.Worksheet ws, Object cell1, Object cell2) {
#if OFFICE12
      return ws.Range[cell1, cell2];
#else
      ws.get_Range(cell1, cell2);
#endif
    }

    public static Excel.Range[] ParsColRowRanges(Excel.Worksheet ws, int colBegin, int colEnd, String rowRangesList) {
      if((rowRangesList != null) && (!rowRangesList.Equals(""))) {
        String[] vRowRanges = Bio.Helpers.Common.Utl.SplitString(rowRangesList, ';');
        int vRangsCount = ExcelSrv.CalcRangesGroupCount(vRowRanges.Length);
        Excel.Range[] vRslt = new Excel.Range[vRangsCount];
        int vCurResultIndex = 0;
        for(int i = 0; i < vRowRanges.Length; i++) {
          if(i >= (ExcelSrv.ciMaxUnionRangesCount * (vCurResultIndex + 1))) vCurResultIndex++;
          String[] vCurRangeArr = Bio.Helpers.Common.Utl.SplitString(vRowRanges[i], ':');
          int vStrtRow = Int32.Parse(vCurRangeArr[0]);
          int vEndRow = Int32.Parse(vCurRangeArr[1]);
          Object vLTCel = ws.Cells[vStrtRow, colBegin];
          Object vRBCel = ws.Cells[vEndRow, colEnd];
          Excel.Range vCurRng = null;
          if ((vStrtRow == vEndRow) && (colBegin == colEnd))
            vCurRng = (Excel.Range)vLTCel;
          else
            vCurRng = getRange(ws, vLTCel, vRBCel);
          if (vRslt[vCurResultIndex] == null)
            vRslt[vCurResultIndex] = vCurRng;
          else
            vRslt[vCurResultIndex] = ExcelSrv.UnionRanges(vRslt[vCurResultIndex], vCurRng);
        }
        //vRslt[vCurResultIndex].Formula = "FTW:"+vRslt[vCurResultIndex].Count;
        return vRslt;
      } else
        return new Excel.Range[0];
    }

    public static int CalcRangesGroupCount(int rangLength) {
      int vRangsCount = Decimal.ToInt32((new Decimal(rangLength) / new Decimal(ExcelSrv.ciMaxUnionRangesCount)));
      if(ExcelSrv.ciMaxUnionRangesCount * vRangsCount < rangLength)
        vRangsCount++;
      return vRangsCount;
    }
    public static String[] ParsColRowRanges4Root(int colBegin, int colEnd, String rowRangesList) {
      if((rowRangesList != null) && (!rowRangesList.Equals(""))) {
        String[] vRowRanges = Bio.Helpers.Common.Utl.SplitString(rowRangesList, ExcelSrv.csRowRangesListDelimeter);
        String vRngFirst = vRowRanges[0];
        String vRngLast = vRowRanges[vRowRanges.Length - 1];
        String[] vRngFirstArr = Bio.Helpers.Common.Utl.SplitString(vRngFirst, ExcelSrv.csRowRangetDelimeter);
        String[] vRngLastArr = Bio.Helpers.Common.Utl.SplitString(vRngLast, ExcelSrv.csRowRangetDelimeter);
        String vRngCommon = null;
        if((vRngFirstArr.Length == 2) && (vRngLastArr.Length == 2)) {
          vRngCommon = vRngFirstArr[0] + ExcelSrv.csRowRangetDelimeter + vRngLastArr[1];
        }
        if(vRngCommon != null) {
          String[] vRslt = new String[2];
          vRslt[0] = ExcelSrv.BuldRngFromCoord(1, 1, vRngCommon);
          vRslt[1] = ExcelSrv.BuldRngFromCoord(colBegin, colEnd, vRngCommon);
          return vRslt;
        }
      }
      return null;
    }
    public static String BuldRngFromCoord(int colBegin, int colEnd, String rng) {
      String vRslt = null;
      String[] vCurRangeArr = Bio.Helpers.Common.Utl.SplitString(rng, ExcelSrv.csRowRangetDelimeter);
      String vLTCel = null;
      String vRBCel = null;
      try {
        int vStrtRow = Int32.Parse(vCurRangeArr[0]);
        int vEndRow = Int32.Parse(vCurRangeArr[1]);
        vLTCel = ExcelSrv.CellA1Str(vStrtRow, colBegin);
        vRBCel = ExcelSrv.CellA1Str(vEndRow, colEnd);
      } catch(Exception ex) {
        vLTCel = "err:" + ex.ToString();
        vRBCel = "err:" + ex.ToString();
      }
      if(!vLTCel.Equals(vRBCel))
        vRslt = vLTCel + new String(ExcelSrv.csRowRangetDelimeter, 1) + vRBCel;
      else
        vRslt = vLTCel;
      return vRslt;
    }

    public static String ExtractCellValue(Object cell) {
      String vResult = null;
      if(cell != null) {
        vResult = (String)((Excel.Range)cell).Formula;
      }
      if(vResult.Equals(""))
        vResult = null;
      return vResult;
    }

    public static void SetCellValue(Object cell, Object value) {
      if ((cell != null) && (cell is Excel.Range))
        ((Excel.Range)cell).Formula = value;
    }

    public static String RangeToStr(Excel.Range rng) { 
      String vRslt = null;
      String vLTCel = null;
      String vRBCel = null;
      try {
        vLTCel = ExcelSrv.CellA1Str(rng.Row, rng.Column);
        vRBCel = ExcelSrv.CellA1Str(rng.Row + rng.Rows.Count - 1, rng.Column + rng.Columns.Count - 1);
      } catch(Exception ex) {
        vLTCel = "err:" + ex.ToString();
        vRBCel = "err:" + ex.ToString();
      }
      if(!vLTCel.Equals(vRBCel))
        vRslt = vLTCel + new String(ExcelSrv.csRowRangetDelimeter, 1) + vRBCel;
      else
        vRslt = vLTCel;
      return vRslt;
    }

    public static Excel.Range StrToRange(String rng, Excel.Worksheet ws) {
      return getRange(ws, rng, Type.Missing);
    }

    public static void DeleteRange(String pRng, Excel.Worksheet pWS) {
      Excel.Range vRnd = ExcelSrv.StrToRange(pRng, pWS);
      vRnd.Delete(Type.Missing);
    }

    //public static void FreeNames(ref Excel.Workbook wb) {
    //  for (int i = 1; i <= wb.Names.Count; i++) {
    //    var vXName = wb.Names.Item(i, Type.Missing, Type.Missing);
    //    ExcelSrv.nar(ref vXName);
    //  }
    //}

    public static Excel.Range GetRangeByName(ref Excel.Workbook wb, String rangeName) {
      String vName = "";
      for (int i = 1; i <= wb.Names.Count; i++) {
        var vXName = wb.Names.Item(i, Type.Missing, Type.Missing);
        try {
          vName = vXName.Name;
          if (vName.Equals(rangeName)) {
            return vXName.RefersToRange;
            //break;
          }
        } finally {
          ExcelSrv.nar(ref vXName);
        }
      }
      return null;
    }

    public static Excel.Range UnionRanges(Excel.Range rng1, Excel.Range rng2) {
      if(rng1 != null) {
        if(rng2 != null)
          return rng1.Application.Union(rng1, rng2,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        else
          return rng1;
      } else {
        if(rng2 != null)
          return rng2;
        else
          return null;
      }
    }

    public static Excel.Chart FindChartTempl(String name, Excel.Workbook wb) {
      if(wb != null) {
        for(int i = 1; i <= wb.Charts.Count; i++) {
          String vName = ((Excel.Chart)wb.Charts[i]).Name;
          if(vName.Equals(name))
            return (Excel.Chart)wb.Charts[i];
        }
      }
      return null;
    }

    public static Excel.Worksheet FindWS(String name, Excel.Workbook wb) {
      if(wb != null) {
        for(int i = 1; i <= wb.Worksheets.Count; i++) {
          String vName = ((Excel.Worksheet)wb.Worksheets[i]).Name;
          if(vName.Equals(name))
            return (Excel.Worksheet)wb.Worksheets[i];
        }
      }
      return null;
    }

    public static Excel.XlHAlign ConvertAlignJS2XL(CJSAlignment align) {
      switch (align) {
        case CJSAlignment.Left: return Excel.XlHAlign.xlHAlignLeft;
        case CJSAlignment.Right: return Excel.XlHAlign.xlHAlignRight;
        case CJSAlignment.Center: return Excel.XlHAlign.xlHAlignCenter;
        case CJSAlignment.Stretch: return Excel.XlHAlign.xlHAlignJustify;
        default: return Excel.XlHAlign.xlHAlignLeft;
      }
    }

	}
}
