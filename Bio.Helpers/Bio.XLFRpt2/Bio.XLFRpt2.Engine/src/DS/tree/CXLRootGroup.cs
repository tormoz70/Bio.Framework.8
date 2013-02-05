namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
	using System.IO;
  using Bio.Helpers.Common.Types;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Collections.Generic;
#endif

	/// <summary>
	/// 
	/// </summary>
	/// 

	public class CXLRootGroup:CXLRGroup{
		//private
		private const String csFirstColID = "XLR_COL";
		private const String csFolmulaFieldID = "XLR_FORMULA";

		private CXLRColDefs FColDefs = null;
		private int FLeftColOffset = 0;
		private int FTopRowOffset = 0;
		private int FCurBufferRowIndex = -1;
		private int FFirstDetalsRow = -1;
    private String FGroupFieldNamesComma = null;
    private int FHeaderFormatsCount = 0;
    private Boolean FIsGroupDefined = false;
    //public
		//constructor

    public CXLRTemplateDef GRTTmplDef = null;

    private String getGroupFieldNamesComma() {
      String rslt = null;
      if((this.GRTTmplDef.HeaderFormats != null) && (this.GRTTmplDef.HeaderFormats.Count > 0)) {
        for(int i = 0; i < this.GRTTmplDef.HeaderFormats.Count; i++)
          if(this.GRTTmplDef.HeaderFormats[i] != null) {
            if(rslt == null)
              rslt = this.GRTTmplDef.HeaderFormats.getRangeName(i);
            else
              rslt += "," + this.GRTTmplDef.HeaderFormats.getRangeName(i);
          }
      }
      return rslt;
    }

    public CXLRootGroup(CXLRDataSet pOwner, Excel.Range pDSRange) : base(pOwner, null, 0) {
      this.GRTTmplDef = new CXLRTemplateDef(pDSRange);
      try {
        this.FHeaderFormatsCount = this.GRTTmplDef.HeaderFormats.Count;
        this.FLeftColOffset = pDSRange.Column;
        this.FTopRowOffset = pDSRange.Row;
        this.FColDefs = new CXLRColDefs();
        this.FLeftCol = 1;
        this.FTopRow = 1;
        this.parsColumns();
        this.GRTTmplDef.Check(this);
        if (this.ColDefs.HasTotals)
          this.FTotals = new CXLRTotals(this);
        this.FGroupFieldNamesComma = this.getGroupFieldNamesComma();

      } finally {
        this.GRTTmplDef.Dispose();
        this.GRTTmplDef = null;
      }
		}

    private String[] getGroupFieldNames(int pColIndx) {
      if((this.GRTTmplDef.HeaderFormats != null) && (this.GRTTmplDef.HeaderFormats.Count > 0)) {
        int vLocalIndex = pColIndx - 2;
        if(this.GRTTmplDef.HeaderFormats.Count >= vLocalIndex) {
          String[] vGrpFieldNames = new String[this.GRTTmplDef.HeaderFormats.Count - vLocalIndex];
          for(int i = vLocalIndex; i < this.GRTTmplDef.HeaderFormats.Count; i++)
            vGrpFieldNames[i - vLocalIndex] = this.GRTTmplDef.HeaderFormats.getRangeName(i);
          return vGrpFieldNames;
        }else
          return new String[0];
      } else
        return new String[0];
    }

    //private IDictionary<String, String> _ttlsFolmulas = new Dictionary<String, String>();

    private void parsColumns() {

      if(this.GRTTmplDef.DetailsRng == null)
        throw new EBioException("Для построения отчета небходимо пометить строку \"details\".");
      for(int i = 1; i <= this.GRTTmplDef.DetailsRng.Columns.Count; i++) {
        String vCurColDefVal = ExcelSrv.ExtractCellValue(this.GRTTmplDef.DetailsRng.Cells[1, i]);
        bool vIsSysCol = ((vCurColDefVal != null) && (vCurColDefVal.Length > 3) && vCurColDefVal.Substring(1, 3).Equals("sys"));
        String vCurColFldName = ExcelSrv.ExtractFieldName(vCurColDefVal);
        if(vCurColFldName == null) {
          if(i == 1)
            vCurColFldName = CXLRootGroup.csFirstColID;
          else
            vCurColFldName = CXLRootGroup.csFolmulaFieldID;
        }
        var vCurColTTLDefVal = String.Empty;
        var vTTLType = XLRTTLType.xttNototal;
        var vTTLCalcType = XLRTTLCalcType.xctFormula;
        if(this.GRTTmplDef.TotalsRng != null) {
          var v_cellVal = this.GRTTmplDef.TotalsRng.Cells[1, i];
          vCurColTTLDefVal = ExcelSrv.ExtractCellValue(v_cellVal);
          if(!String.IsNullOrEmpty(vCurColTTLDefVal)) {
            if (vCurColTTLDefVal.Equals("sum") || 
                vCurColTTLDefVal.Equals("cnt") || 
                vCurColTTLDefVal.Equals("avg") ||
                vCurColTTLDefVal.Equals("max") ||
                vCurColTTLDefVal.Equals("min") )
              vTTLType = XLRTTLType.xttSubtotal;
            else if (vCurColTTLDefVal.Equals("sum(sum)") || vCurColTTLDefVal.Equals("cnt(cnt)"))
              vTTLType = XLRTTLType.xttRuntotal;
            else if (vCurColTTLDefVal[0] == '=') {
              vTTLType = XLRTTLType.xttFormula;
              var fkey = "ttlsFolmula-" + i; // this._ttlsFolmulas.Count;
              //ExcelSrv.SetCellValue(v_cellVal, fkey);
              //this._ttlsFolmulas.Add(fkey, vCurColTTLDefVal);
              vCurColTTLDefVal = fkey;
            }
            
            if (vCurColTTLDefVal.Length > 2) {
              if (vCurColTTLDefVal.Substring(0, 3).Equals("sum"))
                vTTLCalcType = XLRTTLCalcType.xctSum;
              else if (vCurColTTLDefVal.Substring(0, 3).Equals("cnt"))
                vTTLCalcType = XLRTTLCalcType.xctCnt;
              else if (vCurColTTLDefVal.Substring(0, 3).Equals("avg"))
                vTTLCalcType = XLRTTLCalcType.xctAvg;
              else if (vCurColTTLDefVal.Substring(0, 3).Equals("max"))
                vTTLCalcType = XLRTTLCalcType.xctMax;
              else if (vCurColTTLDefVal.Substring(0, 3).Equals("min"))
                vTTLCalcType = XLRTTLCalcType.xctMin;
            }
          }
        }
        var vIsGrpFld = false;
        if(vCurColFldName != null)
          vIsGrpFld = this.GRTTmplDef.HeaderFormats.RangeExists(vCurColFldName);
        var vGroupFieldHasFooter = false;
        if(vCurColFldName != null)
          vGroupFieldHasFooter = this.GRTTmplDef.FooterFormats.RangeExists(vCurColFldName);
        if (!String.IsNullOrEmpty(vCurColTTLDefVal) && (vTTLCalcType == XLRTTLCalcType.xctFormula))
          vCurColDefVal = vCurColTTLDefVal;
        var vCol = this.FColDefs.Add(vTTLType, vTTLCalcType, vCurColFldName, vIsGrpFld, vGroupFieldHasFooter, vIsSysCol, vCurColDefVal);
        vCol.GroupFieldNames = this.getGroupFieldNames(vCol.ColIndex);
      }
    }

		public XmlDocument GetXmlDoc(){
			XmlDocument vXmlDoc = new XmlDocument();
			vXmlDoc.AppendChild(this.GetXml(vXmlDoc));
			return vXmlDoc;
		}

		public int FirstDetalsRow{
			get{
				return this.FFirstDetalsRow;
			}
			set{
				this.FFirstDetalsRow = value;
			}
		}

    public Object GetData(int indx, CXLRDataRow row) {
			String vFieldName = this.ColDefs[indx].FieldName;
			if((vFieldName != null) && (vFieldName.Equals("XLR_FORMULA")))
				return this.ColDefs[indx].Formula;
			else
				return row[vFieldName];
		}

    //public Object GetData(String pFieldName, CXLRDataRow row) {
    //  if(pFieldName != null)
    //    return this.Owner.DataFactory.ValueByName(pFieldName);
    //  else
    //    return null;
    //}

		public String GroupFieldNamesComma{
			get{
				return this.FGroupFieldNamesComma;
			}
		}

		public CXLRColDefs ColDefs{
			get{
				return FColDefs;
			}
		}

		public override XmlElement GetXml(XmlDocument pDoc){
			XmlElement vRslt = pDoc.CreateElement("root");
			vRslt.SetAttribute("LeftColOffset", this.LeftColOffset+"");
			vRslt.SetAttribute("TopRowOffset", this.TopRowOffset+"");
			if(this.FDetails != null)
				vRslt.AppendChild(this.FDetails.GetXml(pDoc));
			else{
				if(this.FChildGroups != null)
					vRslt.AppendChild(this.FChildGroups.GetXml(pDoc));
			}
			if(this.FTotals != null){
				vRslt.AppendChild(this.FTotals.GetXml(pDoc));
			}
			return vRslt;
		}

		public void AddRowToBuffer(Object[,] pBuffer, Object[] pRow, Object pFirstColValue){
			this.FCurBufferRowIndex++;
			for(int i=1; i<pRow.Length; i++)
				pBuffer[this.FCurBufferRowIndex, i] = pRow[i];
      if(pFirstColValue != null)
        pBuffer[this.FCurBufferRowIndex, 0] = pFirstColValue;
		}

    public void AddRowToBuffer(Object[,] buffer, Object[] row) {
      this.AddRowToBuffer(buffer, row, null);
    }

    public override void FillBuffer(Excel.Worksheet ws, Object[,] buffer) {
			if(this.FDetails != null)
        this.FDetails.FillBuffer(ws, buffer);
			else{
				if(this.FChildGroups != null)
          this.FChildGroups.FillBuffer(ws, buffer);
			}
			if(this.FTotals != null){
        this.FTotals.FillBuffer(ws, buffer);
			}
		}

		public override void AppliayFormat(Excel.Range pDSRange){
      //throw new bioEx.bioSysError("AppliayFormat!!!");
			if(this.FChildGroups != null)
        this.FChildGroups.AppliayFormat(pDSRange);
			//applyFmtToGrpHeaders();
			if(this.FTotals != null){
        this.FTotals.AppliayFormat(pDSRange);
			}
      if(this.GRTTmplDef.TotalsRng != null)
        this.GRTTmplDef.TotalsRng.Delete(Type.Missing);
      for (int i = 0; i < this.GRTTmplDef.HeaderFormats.Count; i++) {
        this.GRTTmplDef.HeaderFormats[i].Range.Delete(Type.Missing);
        this.FHeaderFormatsCount--;
      }
      this.GRTTmplDef.HeaderFormats.Clear();
      for(int i = 0; i < this.GRTTmplDef.FooterFormats.Count; i++)
        this.GRTTmplDef.FooterFormats[i].Range.Delete(Type.Missing);
      this.GRTTmplDef.FooterFormats.Clear();
		}

    public override void RefreshTTLFormula(Excel.Range pDSRange) {
      if (this.FChildGroups != null)
        this.FChildGroups.RefreshTTLFormula(pDSRange);
      if (this.FTotals != null) {
        this.FTotals.RefreshFormula(pDSRange);
      }
    }

    public override void DoOnFetch(long rownum, CXLRDataRow row) { 
      CXLRColDef vCol = this.ColDefs.GetByColIndex(this.LeftCol+1);
      String[] vGrps = vCol.GroupFieldNames;
			if(vGrps.Length == 0)
        this.GroupDetails(true).DoOnFetch(rownum, row);
			else
        this.LastChildGroup(true).DoOnFetch(rownum, row);
		}

		public int LeftColOffset{
			get{
				return this.FLeftColOffset;
			}
		}
    public int HeaderFormatsCount {
      get {
        return this.FHeaderFormatsCount;
      }
    }
		public override int TopRowOffset{
			get{
				return this.FTopRowOffset + this.FHeaderFormatsCount;
			}
		}

    private Excel.Range addFirstRow() {
      if ((this.GRTTmplDef != null) && (this.GRTTmplDef.DetailsRng != null)) {
        Excel.Range vNewRow = (Excel.Range)this.GRTTmplDef.DetailsRng.Rows[2, Type.Missing];
        vNewRow.EntireRow.Insert(Type.Missing, Type.Missing);
        vNewRow = (Excel.Range)this.GRTTmplDef.DetailsRng.Rows[2, Type.Missing];
        this.GRTTmplDef.DetailsRng.Copy(vNewRow);
        this.GRTTmplDef.DetailsRng = ExcelSrv.UnionRanges(this.GRTTmplDef.DetailsRng, vNewRow);
        return vNewRow;
      } else
        return null;
		}

		private Excel.Range addRow(Excel.Range vCurRow){
			vCurRow.EntireRow.Insert(Type.Missing, true);
			return vCurRow;
		}

    private Excel.Range findFooterRngByName(Excel.Range rng, String name) {
      for (int i = 1; i <= rng.Rows.Count; i++) {
        String footerDef = ((Excel.Range)rng.Rows[i].Cells[1, 1]).Value2.ToString();
        if (String.Equals(footerDef, CXLRTemplateDef.csGrpFooterDef + "=" + this.Owner.Owner.Cfg.alias + "_" + name, StringComparison.CurrentCultureIgnoreCase)) {
          return ExcelSrv.getRange(rng.Worksheet, rng.Rows[i].Cells[1, 1], rng.Rows[i].Cells[1, rng.Columns.Count]);
        }
      }
      return null;
    }

    public CXLReport OwnerReport {
      get {
        return this.Owner.Owner.Owner;
      }
    }

    private void insertRows(int rowCount) {
      if (rowCount > 0) {
        //Excel.Range vRow = this.GRTTmplDef.DetailsRng;
        if (rowCount > this.Owner.Owner.Cfg.maxRowsLimit)
          throw new Exception(String.Format("Ошибка при вставке строк. Данный отчет {0} может содержать не более {1} строк. Вы пытаетесть создать отчет, который содержит {2} строк.",
            this.OwnerReport.RptDefinition.TemplateFileName, this.Owner.Owner.Cfg.maxRowsLimit, rowCount));
        var vRow = this.addFirstRow();
        if (vRow != null)
          ExcelSrv.getRange(vRow.Worksheet, "IV" + vRow.Row, "IV" + (rowCount + vRow.Row - 2)).EntireRow.Insert(Excel.XlInsertShiftDirection.xlShiftDown, Type.Missing);
      }
		}

    private Excel.Range getFirstDetailsRow(Excel.Range pDetailsRng) {
			Excel.Range vRslt = null;
			if(this.FirstDetalsRow > 0)
        vRslt = (Excel.Range)pDetailsRng.Rows[this.FirstDetalsRow, Type.Missing];
			return vRslt;
		}

		private void refreshFormulaCols_sub(Excel.Range pSrcFormula, Excel.Range[] pRng){
			for(int i=0; i<pRng.Length; i++){
				Excel.Range vRng = pRng[i];
				pSrcFormula.Copy(vRng);
			}
		}

		private void refreshFormulaCols(Excel.Range lastRow){
			for(int i=1; i<this.ColDefs.Count; i++){
				if(this.ColDefs[i].FieldName.Equals(CXLRootGroup.csFolmulaFieldID)){
          var v_dtrl = this.GetDetailsRangesList();
          var vInsRange = ExcelSrv.ParsColRowRanges(lastRow.Worksheet, this.ColDefs[i].ColIndex, this.ColDefs[i].ColIndex, v_dtrl);
          //var vFrmla = (Excel.Range)pLastRow.Cells[1, this.ColDefs[i].ColIndex];
          if (vInsRange.Length > 0) {
            var vFrmla = (Excel.Range)vInsRange[0].Cells[1, 1];
            this.refreshFormulaCols_sub(vFrmla, vInsRange);
          }
				}
			}
    }

		private void saveBufferToTxt(Object[,] pBuffer, bool pForce){
      if(this.FOwner.Owner.Owner.RptDefinition.DebugIsOn || pForce) {
        String vSaveBufferPath = this.Owner.Owner.Owner.RptDefinition.LogPath+this.Owner.Owner.Owner.RptDefinition.ShortCode+"_insBuffer_"+this.Owner.Owner.Cfg.rangeName+".txt"; 
        if(File.Exists(vSaveBufferPath))
          File.Delete(vSaveBufferPath);
        for(int i=0; i<pBuffer.GetLength(0); i++){
          String vLine = null;
          for(int k=0; k<pBuffer.GetLength(1); k++)
            Bio.Helpers.Common.Utl.AddObjToLine(ref vLine, "\t", pBuffer[i, k]);
          Bio.Helpers.Common.Utl.AppendStringToFile(vSaveBufferPath, vLine, null);
        }
      }
		}

    public void BuildReport(Excel.Range dsRange) {
       var ownerRptDef = this.Owner.Owner.Owner.RptDefinition;
      this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : Старт...");

      this.GRTTmplDef = new CXLRTemplateDef(dsRange);
      this.FLeftColOffset = dsRange.Column;
      this.FTopRowOffset = dsRange.Row;
      try {
        this.FHeaderFormatsCount = this.GRTTmplDef.HeaderFormats.Count;
        this.FIsGroupDefined = this.FHeaderFormatsCount > 0;
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : this.insertRows(" + this.RowsCount + ")...");
        Boolean additionalRowInserted = (this.RowsCount == 1);
        this.insertRows(additionalRowInserted ? 2 : this.RowsCount);
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : this.insertRows(" + this.RowsCount + ") - OK.");
        //return;
        Object[,] vBuffer = new Object[this.RowsCount, this.ColDefs.Count];
        this.FCurBufferRowIndex = -1;
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : FillBuffer...");
        this.FillBuffer(dsRange.Worksheet, vBuffer);
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : FillBuffer - OK.");
        //throw new Exception("Test!!!");
        Excel.Range vInsRng = ExcelSrv.getRange(this.GRTTmplDef.DetailsRng.Worksheet,
                              this.GRTTmplDef.DetailsRng.Cells[1, 1],
                              this.GRTTmplDef.DetailsRng.Cells[this.GRTTmplDef.DetailsRng.Rows.Count, this.GRTTmplDef.DetailsRng.Columns.Count]);
        if (ownerRptDef.DebugIsOn) {
          this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : saveBufferToTxt...");
          this.saveBufferToTxt(vBuffer, false);
          this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : saveBufferToTxt - OK.");
        }
        try {
          String insRngCoord = vInsRng.Address;
          this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : vInsRng["+insRngCoord+"].Formula = vBuffer...");
          //vInsRng.CopyFromRecordset(vBuffer);

          if (ownerRptDef.DebugIsOn) {
            this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : saveGroupTree...");
            this.RootGroup.GetXmlDoc().Save(ownerRptDef.LogPath + ownerRptDef.ShortCode + ".DS_DATA." + this.FOwner.Owner.Cfg.rangeName + "_pre.xml");
            this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : saveGroupTree - OK.");
          }

          vInsRng.FormulaLocal = vBuffer;
          if (this.Owner.PrepareDataError != null) {
            Excel.Range vInsErrRng = ExcelSrv.getRange(this.GRTTmplDef.DetailsRng.Worksheet,
                                  this.GRTTmplDef.DetailsRng.Cells[this.GRTTmplDef.DetailsRng.Rows.Count+1, 1],
                                  this.GRTTmplDef.DetailsRng.Cells[this.GRTTmplDef.DetailsRng.Rows.Count+1, 1]);
            vInsErrRng.FormulaLocal = this.Owner.PrepareDataError.Message;
            vInsErrRng.Font.Color = ConsoleColor.Red;
          }
          this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : vInsRng.Formula = vBuffer - OK.");
        } catch (Exception ex) {
          if (!ownerRptDef.DebugIsOn) 
            this.saveBufferToTxt(vBuffer, true);
          throw new EBioException("Ошибка при вставке буфера. vInsRng.Length:(" + vInsRng.Rows.Count + "," + vInsRng.Columns.Count + "); vBuffer.Length:(" + vBuffer.GetLength(0) + "," + vBuffer.GetLength(1) + "); msg:" + ex.ToString(), ex);
        }
        Excel.Range vDelLast = (Excel.Range)this.GRTTmplDef.DetailsRng.Rows[this.GRTTmplDef.DetailsRng.Rows.Count, Type.Missing];
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : refreshFormulaCols...");
        this.refreshFormulaCols(vDelLast);
        this.RefreshTTLFormula(dsRange);
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : refreshFormulaCols - OK.");

        vDelLast.Delete(Type.Missing);
        if (additionalRowInserted) {
          vDelLast = (Excel.Range)this.GRTTmplDef.DetailsRng.Rows[this.GRTTmplDef.DetailsRng.Rows.Count, Type.Missing];
          vDelLast.Delete(Type.Missing);
        }
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : AppliayFormat...");
        this.AppliayFormat(dsRange);

        if (this.FIsGroupDefined)
          this.GroupChild(dsRange.Worksheet);
        //this.FDetails
        this.Owner.Owner.Owner.writeLogLine("\tbldr:root-grp:(" + this.Owner.Owner.Cfg.alias + ") : AppliayFormat - OK.");
        this.Owner.Owner.Charts.Build(this);

      } finally {
        this.GRTTmplDef.Dispose();
        this.GRTTmplDef = null;
      }
    }

		public Excel.Range[] GetGroupKeysRng(Excel.Worksheet pWS, CXLRColDef pGroupFieldDef){
      return ExcelSrv.ParsColRowRanges(pWS, pGroupFieldDef.ColIndex, pGroupFieldDef.ColIndex, this.GetKeysRangesList(pGroupFieldDef.FieldName));
		}

	}
}
