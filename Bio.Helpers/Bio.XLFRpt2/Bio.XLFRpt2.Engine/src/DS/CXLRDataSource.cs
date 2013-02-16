namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
	using System.IO;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
#endif
  using Bio.Helpers.XLFRpt2.Engine.XLRptParams;
	/// <summary>
	/// 
	/// </summary>
  public delegate void DlgOnProgressDataSource(CXLRDataSource pSender, Decimal pPrgPrc);
  
  public class CXLRDataSource:DisposableObject{
//private
		private CXLReport _owner = null;
    private CXLReportDSConfig _cfg = null;
		private CXLRCharts _charts = null;
    private CXLRDataSet _ds = null;

//public
    public event DlgOnProgressDataSource OnProgress;

		//constructor
		public CXLRDataSource(CXLReport pOwner, CXLReportDSConfig cfg){
			this._owner = pOwner;
      this._cfg = cfg;
      this._charts = new CXLRCharts(this);
      if (this._cfg.outerDataTable == null) {
        this._charts.Pars(null /*(XmlElement)pDSDefinition.SelectSingleNode("charts")*/);
      } else {
        this._charts.Pars(null);
      }

      this._ds = new CXLRDataSet(this);
      this._ds.OnProgress += new DlgOnProgressDataSet(this.doOnProgressDataSet);
    }

    protected override void doOnDispose() {
      this._ds.Dispose();
      this._ds = null;
      this._charts.Dispose();
    }

		public CXLReport Owner{
			get{
				return _owner;
			}
		}

		public CXLRCharts Charts{
			get{
				return this._charts;
			}
		}

    public CXLReportDSConfig Cfg {
      get {
        return this._cfg;
      }
    }

    private void setParam(Excel.Worksheet pWS, String pParam, String pValue) {
      String vBuffStr = pValue;
      if(!String.IsNullOrEmpty(vBuffStr))
        try {
          pWS.Cells.Replace(pParam, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        } catch(Exception ex) {
          throw new EBioException(String.Format("Ошибка при установке параметра в книге. Сообщение: {0}. Параметр: [{1}]=>[{2}]", ex.Message, pParam, pValue), ex);
        }
    }
    private void applyParamsToWS(CXLReport rpt, Excel.Worksheet ws) {
      CXLRDefinition vRDef = this.Owner.RptDefinition;
      if (vRDef.RptParams != null) {
        for (int i = 0; i < vRDef.RptParams.Count; i++) {
          String vPrmName = vRDef.RptParams[i].Name;
          String vPrmValue = vRDef.RptParams.getReportParameter(rpt, vPrmName);
          this.setParam(ws, "#" + vPrmName + "#", vPrmValue);
        }
      }
      String vBuffStr = vRDef.FullCode;
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("ThrowCode"), vRDef.ThrowCode);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("FullCode"), vRDef.FullCode);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("ShortCode"), vRDef.ShortCode);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("Roles"), vRDef.Roles);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("Title"), vRDef.Title);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("Subject"), vRDef.Subject);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("Autor"), vRDef.Autor);
      //this.setParam(pWS, CXLRDefinition.getParamMappingOfProp("DBConnStr"), vRDef.DBConnStr);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("SessionID"), vRDef.SessionID);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("UserName"), vRDef.UserName);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("RemoteIP"), vRDef.RemoteIP);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("RptLocalPath"), vRDef.RptLocalPath);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("TmpPath"), vRDef.TmpPath);
      this.setParam(ws, CXLRDefinition.getParamMappingOfProp("LogPath"), vRDef.LogPath);
    }

    public void AssignOuterDSTable(DataTable table){
      this._cfg.outerDataTable = table;
    }

    private void doOnProgressDataSet(CXLRDataSet sender, Decimal prgPrc){
      if(this.OnProgress != null)
        this.OnProgress(this, prgPrc);
    }

    public void Init(ref Excel.Workbook wb) {
      Excel.Range vDSRange = ExcelSrv.GetRangeByName(ref wb, this.Cfg.rangeName);
      try {
        if (vDSRange == null)
          throw new Exception(String.Format("Именованный регион \"{0}\" не найден в шаблоне отчета!", this.Cfg.rangeName));
        this._ds.Init(vDSRange);
      } finally {
        ExcelSrv.nar(ref vDSRange);
      }
    }

    public void BuildReport(ref Excel.Workbook wb, Int32 timeout) {
      this.Owner.writeLogLine("\tbldr:ds:(" + this.Cfg.alias + ") : Старт...");
      var vDSRange = ExcelSrv.GetRangeByName(ref wb, this.Cfg.rangeName);
      if(!this.Cfg.rangeName.Equals("none")) {
        this.Owner.writeLogLine("\tbldr:ds:(" + this.Cfg.alias + ") : this.FDS.RootGroup.BuildReport(" + vDSRange + ")...");
        this._ds.RootGroup.BuildReport(vDSRange);
        this.Owner.writeLogLine("\tbldr:ds:(" + this.Cfg.alias + ") : this.FDS.RootGroup.BuildReport(" + vDSRange + ") - OK.");
      } else {
        var vRow = this._ds.GetSingleRow(timeout);
        if(vRow != null)
          for(int k = 1; k <= wb.Worksheets.Count; k++)
            for(int i = 0; i < vRow.Count; i++)
              ((Excel.Worksheet)wb.Worksheets[k]).Cells.Replace("=" + this.Cfg.alias + "_" + vRow[i].Name, vRow[i].Value, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
      }
      for(int i = 1; i <= wb.Worksheets.Count; i++)
        this.applyParamsToWS(this.Owner, (Excel.Worksheet)wb.Worksheets[i]);

    }

    public DataTable OuterDSTable {
      get {
        return this.Cfg.outerDataTable;
      }
    }

    public void PrepareData(Int32 timeout){
      if(!this.Cfg.rangeName.Equals("none")){
        this._ds.PrepareData(timeout);
			}
		}

    public String DataFactoryTypeName {
      get {
        return /*this.Cfg.dataFactoryTypeName ?? */this.Owner.RptDefinition.DataFactoryType;
      }
    }
	}
}
