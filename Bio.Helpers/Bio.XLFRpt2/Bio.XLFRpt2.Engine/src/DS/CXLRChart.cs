namespace Bio.Helpers.XLFRpt2.Engine {

  using System;
  using System.Xml;
  using System.Xml.XPath;
  using System.Collections;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
#endif
  using Bio.Helpers.XLFRpt2.Engine.XLRptParams;

  /// <summary>
  /// 
  /// </summary>

  public class CXLRChart {
    private CXLRCharts FOwner = null;
    private String FTemplName = null;
    private String FName = null;
    private String FRngX = null;
    private String FRngY = null;
    private String FRngZ = null;

    public CXLRChart(CXLRCharts pOwner, XmlElement pChart) {
      //this.FOwner = pOwner;
      //this.FTemplName = pChart.GetAttribute("template");
      //this.FName = pChart.GetAttribute("name");
      //this.FRngX = pChart.GetAttribute("rngX");
      //this.FRngY = pChart.GetAttribute("rngY");
      //if((pChart.HasAttribute("rngZ")) && (!pChart.GetAttribute("rngZ").Equals("")))
      //  this.FRngZ = pChart.GetAttribute("rngZ");
    }

    public String Name {
      get {
        return this.FName;
      }
    }

    public String TemplName {
      get {
        return this.FTemplName;
      }
    }

    public String DataName {
      get {
        return this.FTemplName + "_data";
      }
    }

    private Excel.Range getCurColValuesRange(Excel.Worksheet ws, CXLRColDef colDef, String rRng) {
      //if((rRng != null) && (!rRng.Equals(""))) {
      //  String[] vRngsX = CExcelSrv.ParsColRowRanges(colDef.ColIndex, colDef.ColIndex, rRng);
      //  String vRngX = vRngsX[0];
      //  return CExcelSrv.getRange(ws, vRngX, Type.Missing);
      //} else
        return null;
    }

    private void dropGrDataWS(Excel.Workbook wb) {
      //Excel.Worksheet vCurGrDataWS = CExcelSrv.FindWS(this.DataName, wb);
      //if(vCurGrDataWS != null) {
      //  vCurGrDataWS.Delete();
      //}
    }

    private Excel.Worksheet clearGrDataWS(Excel.Workbook wb) {
      this.dropGrDataWS(wb);
      Excel.Worksheet vRslt = null;
      //vRslt = (Excel.Worksheet)wb.Worksheets.Add(Type.Missing, Type.Missing, 1, Type.Missing);
      //vRslt.Name = this.DataName;
      return vRslt;
    }

    private Excel.Worksheet addGrDataWS(Excel.Workbook wb) {
      Excel.Worksheet vCurGrDataWS = null;
      //vCurGrDataWS = CExcelSrv.FindWS(this.DataName, wb);
      //if(vCurGrDataWS != null)
      //  vCurGrDataWS.Cells.ClearContents();
      //else
      //  vCurGrDataWS = (Excel.Worksheet)wb.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
      //vCurGrDataWS.Name = this.DataName;
      //vCurGrDataWS.Visible = Excel.XlSheetVisibility.xlSheetHidden;
      return vCurGrDataWS;
    }

    private CXLReport ownerRpt { get { return this.FOwner.Owner.Owner; } }

    private void applyParamsTo(Excel.Chart actChrt) {
      //if(actChrt.HasTitle) {
      //  String vChartTitle = actChrt.ChartTitle.Text;
      //  CXLRDefinition vRDef = this.FOwner.Owner.Owner.RptDefinition;
      //  for(int i = 0; i < vRDef.RptParams.Count; i++) {
      //    String vPrmName = vRDef.RptParams[i].Name;
      //    String vPrmValue = vRDef.RptParams.getReportParameter(this.ownerRpt, vPrmName);
      //    vChartTitle = vChartTitle.Replace("#" + vPrmName + "#", vPrmValue);
      //  }
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("ThrowCode"), vRDef.ThrowCode);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("FullCode"), vRDef.FullCode);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("ShortCode"), vRDef.ShortCode);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("Roles"), vRDef.Roles);
      //  //vChartTitle = vChartTitle.Replace(TXLRDefinition.csRptIconParamID, vRDef.Roles);
      //  //vChartTitle = vChartTitle.Replace(TXLRDefinition.csRptHrefParamID, vRDef.Href);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("AuTitletor"), vRDef.Title);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("Subject"), vRDef.Subject);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("Autor"), vRDef.Autor);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("DBConnStr"), vRDef.ConnStr);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("SessionID"), vRDef.SessionID);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("UserName"), vRDef.UserName);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("RemoteIP"), vRDef.RemoteIP);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("RptRootTreePath"), vRDef.RptRootTreePath);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("RptDefFileName"), vRDef.RptDefFileName);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("RptWorkPath"), vRDef.RptWorkPath);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("RptLocalPath"), vRDef.RptLocalPath);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("TmpPath"), vRDef.TmpPath);
      //  vChartTitle = vChartTitle.Replace(CXLRDefinition.getParamMappingOfProp("LogPath"), vRDef.LogPath);
      //  actChrt.ChartTitle.Text = vChartTitle;
      //}
    }

    private void addXRngToGrData(Excel.Worksheet grWS, Excel.Worksheet dsWS, Excel.Range xRng, CXLRootGroup rootGrp, CXLRColDef xColDef, CXLRColDef yColDef, CXLRColDef zColDef) {
      //Excel.Range vXRng = CExcelSrv.getRange(grWS, grWS.Cells[1, 2], grWS.Cells[1, xRng.Areas.Count + 1]);
      //for (int i = 1; i <= xRng.Areas.Count; i++) {
      //  Excel.Range vCur = (Excel.Range)xRng.Areas.get_Item(i);
      //  String vLocAddr = ((Excel.Range)vCur.Cells[1, 1]).get_Address(Type.Missing, Type.Missing, Excel.XlReferenceStyle.xlA1, Type.Missing, Type.Missing);
      //  ((Excel.Range)vXRng.Cells[1, i]).Formula = "=" + this.FOwner.Owner.Cfg.wsName + "!" + vLocAddr;
      //}
      //String vVal = ((Excel.Range)vXRng.Cells[1, 1]).Value2.ToString();
      //CXLRGroup vCurGroup = rootGrp.FindGroup(xColDef.FieldName, vVal);
      //Excel.Range[] vDetails = CExcelSrv.ParsColRowRanges(dsWS, yColDef.ColIndex, zColDef.ColIndex, vCurGroup.GetDetailsRangesList());
      //vDetails[0].Copy(grWS.Cells[2, 1]);
    }

    private void prepareData(CXLRootGroup pRoot) {
      //Excel.Worksheet vDSWS = pRoot.GRTTmplDef.DetailsRng.Worksheet;
      //Excel.Workbook vWB = pRoot.GRTTmplDef.DetailsRng.Application.ActiveWorkbook;
      //Excel.Chart vActChrt = CExcelSrv.FindChartTempl(this.TemplName, vWB);
      //Excel.Range vX = CExcelSrv.GetRangeByName(vWB, this.FRngX);
      //Excel.Range vY = CExcelSrv.GetRangeByName(vWB, this.FRngY);
      //Excel.Range vZ = CExcelSrv.GetRangeByName(vWB, this.FRngZ);
      //if(vActChrt != null) {
      //  if(vX == null)
      //    throw new EBioException("Ошибка. " + this.TemplName + " - Объявлена область данных " + this.FRngX + ", которая не существует в шаблоне.");
      //  if(vY == null)
      //    throw new EBioException("Ошибка. " + this.TemplName + " - Объявлена область данных " + this.FRngY + ", которая не существует в шаблоне.");
      //  if((this.FRngZ != null) && (vZ == null))
      //    throw new EBioException("Ошибка. " + this.TemplName + " - Объявлена область данных " + this.FRngZ + ", которая не существует в шаблоне.");

      //  int vXCol = vX.Column - pRoot.LeftColOffset;
      //  CXLRColDef vXColDef = pRoot.ColDefs[vXCol];
      //  int vYCol = vY.Column - pRoot.LeftColOffset;
      //  CXLRColDef vYColDef = pRoot.ColDefs[vYCol];

      //  CXLRColDef vZColDef = null;
      //  if(vZ != null) {
      //    int vZCol = vZ.Column - pRoot.LeftColOffset;
      //    vZColDef = pRoot.ColDefs[vZCol];
      //  }

      //  // I - 2D or 3D и нет групп в размерности
      //  if((vXColDef != null) && (!vXColDef.IsGroupField) && (vYColDef != null) && (!vYColDef.IsGroupField)) {
      //    this.dropGrDataWS(vWB);
      //    String vRRng = pRoot.GetDetailsRangesList();
      //    Excel.Range vXDataRng = getCurColValuesRange(vDSWS, vXColDef, vRRng);
      //    Excel.Range vYDataRng = getCurColValuesRange(vDSWS, vYColDef, vRRng);
      //    Excel.Range vZDataRng = null;
      //    if(this.FRngZ != null)
      //      vZDataRng = getCurColValuesRange(vDSWS, vZColDef, vRRng);
      //    if((vXDataRng != null) && (vYDataRng != null)) {
      //      vXDataRng = CExcelSrv.UnionRanges(vXDataRng, (Excel.Range)vX.Cells[1, 1]);
      //      vYDataRng = CExcelSrv.UnionRanges(vYDataRng, (Excel.Range)vY.Cells[1, 1]);
      //      if(vZDataRng != null)
      //        vZDataRng = CExcelSrv.UnionRanges(vZDataRng, (Excel.Range)vZ.Cells[1, 1]);
      //      Excel.Range vDt = CExcelSrv.UnionRanges(vXDataRng, vYDataRng);
      //      if(vZDataRng != null)
      //        vDt = CExcelSrv.UnionRanges(vDt, vZDataRng);

      //      vActChrt.SetSourceData(vDt, Excel.XlRowCol.xlColumns);
      //    }
      //  } else
      //    // II - Есть группировка только по Х
      //    if((vXColDef.IsGroupField) && (!vYColDef.IsGroupField)) {
      //      Excel.Worksheet vGrDataWS = this.clearGrDataWS(vWB);
      //      if(vZColDef == null)
      //        throw new EBioException("Необходимо определить Z-набор данных.");
      //      if(vZColDef.IsGroupField)
      //        throw new EBioException("Z-набор данных не может быть определен на группе.");
      //      Excel.Range[] vXKeys = pRoot.GetGroupKeysRng(vDSWS, vXColDef);
      //      for(int i = 0; i < vXKeys.Length; i++)
      //        addXRngToGrData(vGrDataWS, vDSWS, vXKeys[i], pRoot, vXColDef, vYColDef, vZColDef);
      //    }

      //  vActChrt.Name = this.Name;
      //  this.applyParamsTo(vActChrt);
      //}
    }

    public void Build(CXLRootGroup pRoot) {
      this.prepareData(pRoot);
    }

  }


}
