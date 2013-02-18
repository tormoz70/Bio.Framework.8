namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
  using Common.Types;
  using System.Linq;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
#endif

	/// <summary>
	/// 
	/// </summary>

	public class XLRDataSources:DisposableObject{
	  private readonly CXLReport _owner;
    private readonly List<XLRDataSource> _dss;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="dss"></param>
		public XLRDataSources(CXLReport owner, List<CXLReportDSConfig> dss){
      this._owner = owner;
      this._dss = new List<XLRDataSource>();
			foreach(var dsCfg in dss){
        var v_newDS = new XLRDataSource(this._owner, dsCfg);
        v_newDS.OnProgress += this._owner.DoOnProgressDataSource;
        this._dss.Add(v_newDS);
			}
		}

		protected override void doOnDispose(){
			foreach(var ds in this._dss)
				ds.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		public int Count {
		  get{
        return this._dss.Count;
			}
		}

	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="index"></param>
	  public XLRDataSource this[int index]{
			get {
			  if((index >= 0) &&(index < _dss.Count))
					return this._dss[index];
			  return null;
			}
	  }

    /// <summary>
    /// Для определенного в описании отчета источника данных переопределяет созданый вне набор данных
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="table"></param>
    public void AssignOuterDSTable(String alias, DataTable table){
      var v_ds = this._dss.Where(c => { return String.Equals(c.Cfg.alias, alias, StringComparison.CurrentCultureIgnoreCase); }).FirstOrDefault();
      if(v_ds == null)
        throw new EBioException("Источник данных " + alias + " не определен в отчете.");
      v_ds.AssignOuterDSTable(table);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wb"></param>
    public void Init(ref Excel.Workbook wb) {
      foreach(var ds in this._dss)
        ds.Init(ref wb);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    public void PrepareData(Int32 timeout) {
      foreach(var ds in this._dss)
        ds.PrepareData(timeout);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wb"></param>
    /// <param name="timeout"></param>
    public void BuildReport(ref Excel.Workbook wb, Int32 timeout) {
      foreach(var ds in this._dss)
        ds.BuildReport(ref wb, timeout);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    public void Add(CXLReportDSConfig cfg) {
      var v_newDS = new XLRDataSource(this._owner, cfg);
      v_newDS.OnProgress += this._owner.DoOnProgressDataSource;
      _dss.Add(v_newDS);
    }
  }
}
