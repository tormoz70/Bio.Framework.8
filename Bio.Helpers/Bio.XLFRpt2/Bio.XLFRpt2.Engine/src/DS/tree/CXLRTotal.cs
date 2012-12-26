namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
#endif

	/// <summary>
	/// 
	/// </summary>
	public abstract class CXLRTotal{
//private
    private const String csTotalSumFormat4Root = "—”ÃÃ≈—À»({0};\"=[1]\";{1})";
    private const String csTotalSumFormat4RootNoGrps = "—”ÃÃ({0})";

    private const String csTotalAvgFormat4Root = "—–«Õ¿◊≈—À»({0};\"=[1]\";{1})";
    private const String csTotalAvgFormat4RootNoGrps = "—–«Õ¿◊({0})";

    private const String csTotalCntFormat4Root = "—”ÃÃ≈—À»({0};\"=[1]\";{1})";
    private const String csTotalCntFormat4RootNoGrps = "◊—“–Œ ({0})";

    private const String csTotalSumFormat = "—”ÃÃ({0})";
    private const String csTotalSumFormatPart = "—”ÃÃ({0})";

    private const String csTotalAvgFormat = "—–«Õ¿◊({0})";
    private const String csTotalAvgFormatPart = "—–«Õ¿◊({0})";

    private const String csTotalMaxFormat = "Ã¿ —({0})";
    private const String csTotalMaxFormatPart = "Ã¿ —({0})";

    private const String csTotalMinFormat = "Ã»Õ({0})";
    private const String csTotalMinFormatPart = "Ã»Õ({0})";

    private const String csTotalCntFormat = "◊—“–Œ ({0})";
    private const String csTotalCntFormatPart = "—”ÃÃ({0})";
    
    private CXLRTotals FOwner = null;
		private CXLRColDef FCol;

//public
		//constructor
		public CXLRTotal(CXLRTotals pOwner, CXLRColDef pCol){
			FOwner = pOwner;
			FCol = pCol;
		}

		public CXLRTotals Owner{
			get{
				return this.FOwner;
			}
		}
		
		public CXLRColDef ColDef{
			get{
				return this.FCol;
			}
		}

		public abstract String GetRange();

		public XmlElement GetXml(XmlDocument pDoc){
			XmlElement vNewTTL = pDoc.CreateElement(this.GetType().Name);
			vNewTTL.SetAttribute("FieldName", this.ColDef.FieldName);
			vNewTTL.SetAttribute("ColIndex", this.ColDef.ColIndex+"");
			String vRRng = this.GetRange();
			vNewTTL.SetAttribute("Range", vRRng);
			String[] vRngs = CExcelSrv.ParsColRowRanges(this.ColDef.ColIndex, this.ColDef.ColIndex, vRRng);
			for(int i=0; i<vRngs.Length; i++){
				XmlElement vNewRng = pDoc.CreateElement("rng");
				vNewRng.SetAttribute("Range", vRngs[i]);
				vNewTTL.AppendChild(vNewRng);
			}
			return vNewTTL;
		}

    private String buildTTLsFormula(String pRRng) {
      String vResult = null;
      String[] vRngs = CExcelSrv.ParsColRowRanges(this.ColDef.ColIndex, this.ColDef.ColIndex, pRRng);
      for(int i = 0; i < vRngs.Length; i++) {
        String vTotalFormat = this.FCol.Formula; 
        String vTotalFormatPart = this.FCol.Formula; 

        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctSum) {
          vTotalFormat = CXLRTotal.csTotalSumFormat;
          vTotalFormatPart = CXLRTotal.csTotalSumFormatPart;
        }
        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctCnt) {
          if (this.Owner.Owner.ChildGroups(false) == null) {
            vTotalFormat = CXLRTotal.csTotalCntFormat;
            vTotalFormatPart = CXLRTotal.csTotalCntFormat;
          } else {
            vTotalFormat = CXLRTotal.csTotalCntFormatPart;
            vTotalFormatPart = CXLRTotal.csTotalCntFormatPart;
          }
        }
        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctMin) {
          vTotalFormat = CXLRTotal.csTotalMinFormat;
          vTotalFormatPart = CXLRTotal.csTotalMinFormatPart;
        }
        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctMax) {
          vTotalFormat = CXLRTotal.csTotalMaxFormat;
          vTotalFormatPart = CXLRTotal.csTotalMaxFormatPart;
        }
        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctAvg) {
          vTotalFormat = CXLRTotal.csTotalAvgFormat;
          vTotalFormatPart = CXLRTotal.csTotalAvgFormatPart;
        }

        if (vResult == null)
          vResult = String.Format(vTotalFormat, vRngs[i]);
        else {
          if (this.FCol.TTLCalcType == XLRTTLCalcType.xctMin)
            vResult = String.Format(csTotalMinFormat, vResult + ";" + String.Format(vTotalFormatPart, vRngs[i]));
          else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctMax)
            vResult = String.Format(csTotalMaxFormat, vResult + ";" + String.Format(vTotalFormatPart, vRngs[i]));
          else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctAvg)
            vResult = String.Format(csTotalAvgFormat, vResult + ";" + String.Format(vTotalFormatPart, vRngs[i]));
          else
            vResult += " + " + String.Format(vTotalFormatPart, vRngs[i]);
        }
      }
      if (!String.IsNullOrEmpty(vResult))
        return (vResult[0] != '=') ? "=" + vResult : vResult;
      else
        return "0";
    }

    private String buildTTLsFormula4Root(String rRng) {
      String vResult = null;
      String[] vRngs = null;
      if ((this.FCol.TTLCalcType == XLRTTLCalcType.xctMax) || (this.FCol.TTLCalcType == XLRTTLCalcType.xctMin))
        vRngs = CExcelSrv.ParsColRowRanges(this.ColDef.ColIndex, this.ColDef.ColIndex, rRng);
      else
        vRngs = CExcelSrv.ParsColRowRanges4Root(this.ColDef.ColIndex, this.ColDef.ColIndex, rRng);

      if((vRngs != null) && (vRngs.Length == 2) && ((vRngs[0] != null) && (vRngs[1] != null))) {
        // “ÛÚ ÒÚÓËÏ ÙÓÏÛÎ˚ ‰Îˇ ÛÒÎÓ‚Ì˚ı ÙÛÌÍˆËÈ
        String vTotalFormat4Root = this.FCol.Formula; 
        String vTotalFormat4RootNoGrps = this.FCol.Formula; 
        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctSum) {
          vTotalFormat4Root = csTotalSumFormat4Root;
          vTotalFormat4RootNoGrps = csTotalSumFormat4RootNoGrps;
        } else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctAvg) {
          vTotalFormat4Root = csTotalAvgFormat4Root;
          vTotalFormat4RootNoGrps = csTotalAvgFormat4RootNoGrps;
        } else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctCnt) {
          vTotalFormat4Root = csTotalCntFormat4Root;
          vTotalFormat4RootNoGrps = csTotalCntFormat4RootNoGrps;
        }
        if (this.Owner.Owner.HasChildGroups)
          vResult = String.Format(vTotalFormat4Root, vRngs);
        else {
          vResult = String.Format(vTotalFormat4RootNoGrps, vRngs[1]);
        }
      } else if ((vRngs != null) && (vRngs.Length == 1) && (vRngs[0] != null)) {
        // “ÛÚ ÒÚÓËÏ ÙÓÏÛÎ˚ ‰Îˇ ÙÛÌÍˆËÈ, Û ÍÓÚÓ˚ı ÌÂÚ ÛÒÎÓ‚Ì˚ı
        String vTotalFormat4Root = this.FCol.Formula; 
        if (this.FCol.TTLCalcType == XLRTTLCalcType.xctSum) {
          vTotalFormat4Root = csTotalSumFormat4Root;
        } else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctMax) {
          vTotalFormat4Root = csTotalMaxFormat;
        } else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctMin) {
          vTotalFormat4Root = csTotalMinFormat;
        } else if (this.FCol.TTLCalcType == XLRTTLCalcType.xctCnt) {
          vTotalFormat4Root = csTotalCntFormat;
        }
        vResult = String.Format(vTotalFormat4Root, vRngs);
      } else
        return "0";
      return (vResult[0] != '=') ? "=" + vResult : vResult;
    }

    public void FillBuffer(Excel.Worksheet ws, Object[] buffer) {
      String vRRng = this.GetRange();
      int vCurColIndex = this.ColDef.ColIndex - 1;
      if (this.Owner.Owner.IsRootGroup) 
        buffer[vCurColIndex] = this.buildTTLsFormula4Root(vRRng);
       else 
        buffer[vCurColIndex] = this.buildTTLsFormula(vRRng);
    }
	}
}
