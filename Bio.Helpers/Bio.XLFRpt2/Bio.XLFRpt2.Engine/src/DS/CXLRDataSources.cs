namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Xml;
	using System.Collections;
  using System.Data;
  using Bio.Helpers.Common.Types;
  using System.Linq;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
#endif

	/// <summary>
	/// 
	/// </summary>

	public class CXLRDataSources:DisposableObject{
//private
		private XmlElement FXMLDoc = null;
		private CXLReport FOwner = null;
    private List<CXLRDataSource> FDSs = null;
//public
		//constructor
		public CXLRDataSources(CXLReport pOwner, List<CXLReportDSConfig> dss){
      this.FOwner = pOwner;
      this.FDSs = new List<CXLRDataSource>();
			foreach(var dsCfg in dss){
        CXLRDataSource newDS = new CXLRDataSource(this.FOwner, dsCfg);
        newDS.OnProgress += new DlgOnProgressDataSource(this.FOwner.DoOnProgressDataSource);
        this.FDSs.Add(newDS);
			}
		}

		protected override void doOnDispose(){
			foreach(var ds in this.FDSs)
				ds.Dispose();
		}

		public int Count{
			get{
        return this.FDSs.Count;
			}
		}
		
    public CXLRDataSource this[int index]{
			get{
				if((index >= 0) &&(index < FDSs.Count))
					return this.FDSs[index];
				else
					return null;
			}
		}

    /// <summary>
    /// Для определенного в описании отчета источника данных переопределяет созданый вне набор данных
    /// </summary>
    /// <param name="pAlias"></param>
    /// <param name="pTable"></param>
    public void AssignOuterDSTable(String alias, DataTable table){
      CXLRDataSource vDS = this.FDSs.Where((c) => { return String.Equals(c.Cfg.alias, alias, StringComparison.CurrentCultureIgnoreCase); }).FirstOrDefault();
      //foreach (var ds in this.FDSs)
      //  if(ds.Cfg.alias.Equals(pAlias)){
      //    vDS = ds;
      //    break;
      //  }
      if(vDS == null)
        throw new EBioException("Источник данных " + alias + " не определен в отчете.");
      vDS.AssignOuterDSTable(table);
    }

    public void Init(ref Excel.Workbook wb) {
      foreach(var ds in this.FDSs)
        ds.Init(ref wb);
    }

    public void PrepareData(Int32 timeout) {
      foreach(var ds in this.FDSs)
        ds.PrepareData(timeout);
    }

    public void BuildReport(ref Excel.Workbook wb, Int32 timeout) {
      foreach(var ds in this.FDSs)
        ds.BuildReport(ref wb, timeout);
    }

    public void Add(CXLReportDSConfig cfg) {
      CXLRDataSource newDS = new CXLRDataSource(this.FOwner, cfg);
      newDS.OnProgress += new DlgOnProgressDataSource(this.FOwner.DoOnProgressDataSource);
      FDSs.Add(newDS);
    }
    //public void Add(String alias, String rangeName, String title, String factoryTypeName) {
    //  CXLRDataSource newDS = new CXLRDataSource(this.FOwner, alias, rangeName, title, factoryTypeName);
    //  newDS.OnProgress += new DlgOnProgressDataSource(this.FOwner.DoOnProgressDataSource);
    //  FDSs.Add(newDS);
    //}
    //public void Add(String alias, String rangeName, String title, DataTable pOuterDataTable) {
    //  CXLRDataSource newDS = new CXLRDataSource(this.FOwner, alias, rangeName, title, pOuterDataTable);
    //  newDS.OnProgress += new DlgOnProgressDataSource(this.FOwner.DoOnProgressDataSource);
    //  FDSs.Add(newDS);
    //}
  }
}
