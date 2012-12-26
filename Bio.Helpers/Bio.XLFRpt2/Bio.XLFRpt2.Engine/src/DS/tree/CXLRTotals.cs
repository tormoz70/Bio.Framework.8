namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Collections.Generic;
#endif

	/// <summary>
	/// 
	/// </summary>
	public class CXLRTotals{
//private
		private CXLRGroup FOwner = null;
    private List<CXLRTotal> FTotals = null;

		private void addTotalSub(CXLRColDef pCol){
			CXLRTotal vNewTTL = new CXLRTotalSub(this, pCol);
			FTotals.Add(vNewTTL);
		}
		private void addTotalRun(CXLRColDef pCol){
			CXLRTotal vNewTTL = new CXLRTotalRun(this, pCol);
			FTotals.Add(vNewTTL);
		}

//public
		//constructor
		public CXLRTotals(CXLRGroup pOwner){
			FOwner = pOwner;
			FOwner.DoOnChildRowsInsert(1);
			FTotals = new List<CXLRTotal>();
			for(int i=0; i<this.FOwner.RootGroup.ColDefs.Count; i++){
				CXLRColDef vCurCol = this.FOwner.RootGroup.ColDefs[i];
				switch(vCurCol.TTLType){
					case XLRTTLType.xttRuntotal:
						this.addTotalRun(vCurCol); break;
					case XLRTTLType.xttSubtotal:
          case XLRTTLType.xttFormula:
            this.addTotalSub(vCurCol); break;
				}
			}
		}

		public CXLRGroup Owner{
			get{
				return this.FOwner;
			}
		}

		public XmlElement GetXml(XmlDocument pDoc){
			XmlElement vRslt = pDoc.CreateElement("totals");
			foreach(var vCurTTL in this.FTotals){
				XmlElement vNewTTL = vCurTTL.GetXml(pDoc);
				vRslt.AppendChild(vNewTTL);
			}
			return vRslt;
		}

    private void fillTotalRowData(Excel.Range rngSrc, Object[] buffer) {
      if (rngSrc != null) {
        for (int i = 0; i < buffer.Length; i++)
          buffer[i] = CExcelSrv.ExtractCellValue(rngSrc.Cells[1, i + 1]);
      }
    }
		public void FillBuffer(Excel.Worksheet ws, Object[,] buffer){
			Object[] v_grpTotal = new Object[this.Owner.RootGroup.ColDefs.Count];
      if (this.Owner.IsRootGroup)
        this.fillTotalRowData(this.Owner.RootGroup.GRTTmplDef.TotalsRng, v_grpTotal);
      else {
        Excel.Range vRngSrc = this.Owner.RootGroup.GRTTmplDef.FooterFormats[this.Owner.GroupKeyField];
        this.fillTotalRowData(vRngSrc, v_grpTotal);
      }
      foreach (var ttl in this.FTotals)
        ttl.FillBuffer(ws, v_grpTotal);
      String vGroupLevelLabel = "["+this.Owner.GroupLevel+"]";
      this.Owner.RootGroup.AddRowToBuffer(buffer, v_grpTotal, vGroupLevelLabel);
		}

    public void AppliayFormat(Excel.Range dsRange) {
      Excel.Range vRngSrc = null;
			if(!this.Owner.IsRootGroup)
				vRngSrc = this.Owner.RootGroup.GRTTmplDef.FooterFormats[this.Owner.GroupKeyField];
			else
        vRngSrc = this.Owner.RootGroup.GRTTmplDef.TotalsRng;
			if(vRngSrc != null){
				int vTtlRowNum = this.Owner.BottomRow;
        Excel.Range vDetailsRng = this.Owner.RootGroup.GRTTmplDef.DetailsRng;
        Excel.Range vRngDst = CExcelSrv.getRange(vDetailsRng.Worksheet, vDetailsRng.Cells[vTtlRowNum, 1], vDetailsRng.Cells[vTtlRowNum, vDetailsRng.Columns.Count]);
				vRngSrc.Copy(Type.Missing);
				vRngDst.PasteSpecial(Excel.XlPasteType.xlPasteFormats, Excel.XlPasteSpecialOperation.xlPasteSpecialOperationNone, false, false);
				// Копируем значения из формата totals для колонок, которые не являются тоталами.
				for(int i=2; i<=vRngDst.Columns.Count; i++){
          if(this.Owner.RootGroup.ColDefs[i-1] != null){
            if (this.Owner.RootGroup.ColDefs[i-1].TTLType == XLRTTLType.xttNototal)
              ((Excel.Range)vRngSrc.Cells[1, i]).Copy(vRngDst.Cells[1, i]);
          }
				}
			}
		}

    public void RefreshFormula(Excel.Range dsRange) {
      Excel.Range vRngSrc = this.Owner.RootGroup.GRTTmplDef.TotalsRng;
      if (vRngSrc != null) {
        var vTtlRowNum = this.Owner.BottomRow;
        var vDetailsRng = this.Owner.RootGroup.GRTTmplDef.DetailsRng;
        var vRngDst = CExcelSrv.getRange(vDetailsRng.Worksheet, vDetailsRng.Cells[vTtlRowNum, 1], vDetailsRng.Cells[vTtlRowNum, vDetailsRng.Columns.Count]);
        for (int i = 2; i <= vRngDst.Columns.Count; i++) {
          if (this.Owner.RootGroup.ColDefs[i - 1] != null) {
            if ((this.Owner.RootGroup.ColDefs[i - 1].TTLType == XLRTTLType.xttFormula) &&
              (this.Owner.RootGroup.ColDefs[i-1].TTLCalcType == XLRTTLCalcType.xctFormula)){
              var v_srcCell = (Excel.Range)vRngSrc.Cells[1, i];
              var v_dstCell = (Excel.Range)vRngDst.Cells[1, i];
              v_srcCell.Copy(v_dstCell);
            }
          }
        }
      }
    }

	}
}
