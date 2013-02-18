namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using Common.Types;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
#endif
  using XLRptParams;
	/// <summary>
	/// 
	/// </summary>
  public delegate void DlgOnProgressDataSource(XLRDataSource pSender, Decimal pPrgPrc);
  
  /// <summary>
  /// 
  /// </summary>
  public class XLRDataSource:DisposableObject{
		private readonly CXLReport _owner;
    private readonly CXLReportDSConfig _cfg;
    private CXLRDataSet _ds;

    /// <summary>
    /// 
    /// </summary>
    public event DlgOnProgressDataSource OnProgress;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pOwner"></param>
		/// <param name="cfg"></param>
		public XLRDataSource(CXLReport pOwner, CXLReportDSConfig cfg){
			this._owner = pOwner;
      this._cfg = cfg;

      this._ds = new CXLRDataSet(this);
      this._ds.OnProgress += this._doOnProgressDataSet;
    }

    protected override void doOnDispose() {
      this._ds.Dispose();
      this._ds = null;
    }

		/// <summary>
		/// 
		/// </summary>
		public CXLReport Owner{
			get{
				return _owner;
			}
		}

    /// <summary>
    /// 
    /// </summary>
    public CXLReportDSConfig Cfg {
      get {
        return this._cfg;
      }
    }

    private static void _setParam(Excel.Worksheet ws, String param, String value) {
      var v_buffStr = value;
      if(!String.IsNullOrEmpty(v_buffStr))
        try {
          ws.Cells.Replace(param, (String.IsNullOrEmpty(v_buffStr) ? String.Empty : (v_buffStr.Length > 256) ? v_buffStr.Substring(0, 255) : v_buffStr), Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        } catch(Exception ex) {
          throw new EBioException(String.Format("Ошибка при установке параметра в книге. Сообщение: {0}. Параметр: [{1}]=>[{2}]", ex.Message, param, value), ex);
        }
    }
    private void _applyParamsToWs(CXLReport rpt, Excel.Worksheet ws) {
      var v_rDef = this.Owner.RptDefinition;
      if (v_rDef.RptParams != null) {
        for (var i = 0; i < v_rDef.RptParams.Count; i++) {
          var v_prmName = v_rDef.RptParams[i].Name;
          var v_prmValue = v_rDef.RptParams.getReportParameter(rpt, v_prmName);
          _setParam(ws, "#" + v_prmName + "#", v_prmValue);
        }
      }
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("ThrowCode"), v_rDef.ThrowCode);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("FullCode"), v_rDef.FullCode);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("ShortCode"), v_rDef.ShortCode);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("Roles"), v_rDef.Roles);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("Title"), v_rDef.Title);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("Subject"), v_rDef.Subject);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("Autor"), v_rDef.Autor);
      //this.setParam(ws, CXLRDefinition.getParamMappingOfProp("DBConnStr"), vRDef.DBConnStr);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("SessionID"), v_rDef.SessionID);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("UserName"), v_rDef.UserName);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("RemoteIP"), v_rDef.RemoteIP);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("RptLocalPath"), v_rDef.RptLocalPath);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("TmpPath"), v_rDef.TmpPath);
      _setParam(ws, CXLRDefinition.getParamMappingOfProp("LogPath"), v_rDef.LogPath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    public void AssignOuterDSTable(DataTable table){
      this._cfg.outerDataTable = table;
    }

    private void _doOnProgressDataSet(CXLRDataSet sender, Decimal prgPrc){
      if(this.OnProgress != null)
        this.OnProgress(this, prgPrc);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wb"></param>
    /// <exception cref="Exception"></exception>
    public void Init(ref Excel.Workbook wb) {
      var v_dsRange = ExcelSrv.GetRangeByName(ref wb, this.Cfg.rangeName);
      try {
        if (v_dsRange == null)
          throw new Exception(String.Format("Именованный регион \"{0}\" не найден в шаблоне отчета!", this.Cfg.rangeName));
        this._ds.Init(v_dsRange);
      } finally {
        ExcelSrv.nar(ref v_dsRange);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wb"></param>
    /// <param name="timeout"></param>
    public void BuildReport(ref Excel.Workbook wb, Int32 timeout) {
      this.Owner.writeLogLine("\tbldr:ds:(" + this.Cfg.alias + ") : Старт...");
      var v_dsRange = ExcelSrv.GetRangeByName(ref wb, this.Cfg.rangeName);
      if(!this.Cfg.rangeName.Equals("none")) {
        this.Owner.writeLogLine("\tbldr:ds:(" + this.Cfg.alias + ") : this.FDS.RootGroup.BuildReport(" + v_dsRange + ")...");
        this._ds.RootGroup.BuildReport(v_dsRange);
        this.Owner.writeLogLine("\tbldr:ds:(" + this.Cfg.alias + ") : this.FDS.RootGroup.BuildReport(" + v_dsRange + ") - OK.");
      } else {
        var v_row = this._ds.GetSingleRow(timeout);
        if(v_row != null)
          for(var k = 1; k <= wb.Worksheets.Count; k++)
            foreach (var v_param in v_row)
              ((Excel.Worksheet)wb.Worksheets[k]).Cells.Replace("=" + this.Cfg.alias + "_" + v_param.Name, v_param.Value, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
      }
      for(var i = 1; i <= wb.Worksheets.Count; i++)
        this._applyParamsToWs(this.Owner, (Excel.Worksheet)wb.Worksheets[i]);

    }

    /// <summary>
    /// 
    /// </summary>
    public DataTable OuterDSTable {
      get {
        return this.Cfg.outerDataTable;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    public void PrepareData(Int32 timeout){
      if(!this.Cfg.rangeName.Equals("none")){
        this._ds.PrepareData(timeout);
			}
		}

    /// <summary>
    /// 
    /// </summary>
    public String DataFactoryTypeName {
      get {
        return this.Owner.RptDefinition.DataFactoryType;
      }
    }
	}
}
