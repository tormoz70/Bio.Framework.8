namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
	using System.IO;
  using System.Reflection;
  using System.Linq;
  using Bio.Helpers.Common.Types;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Collections.Generic;
#endif

	/// <summary>
	/// 
	/// </summary>
  public delegate void DlgOnProgressDataSet(CXLRDataSet pSender, Decimal pPrgPrc);

  public class CXLRDataSet:DisposableObject{
//private
    private CXLRDataSource FOwner = null;
    //private CXLRDTblFactory FDSTable = null;
    //private CXLRDataFactory FDataFactory = null;
    private CXLRootGroup FRootGroup;

//public
    public event DlgOnProgressDataSet OnProgress;
    //constructor
		public CXLRDataSet(CXLRDataSource pOwner){
			this.FOwner = pOwner;
    }

    protected override void doOnDispose() {
      if(this.FRootGroup != null) {
        this.FRootGroup.Dispose();
        this.FRootGroup = null;
      }
    }

    //public CXLRDataFactory DataFactory { get; private set; }

		public CXLRDataSource Owner{
			get{
				return FOwner;
			}
		}

    public CXLRootGroup RootGroup{
      get{
        return this.FRootGroup;
      }
    }

    //private String getSQL(){
    //  String vRslt = this.FOwner.SQL;
    //  /*if((vRslt != null) && (vRslt.Length > 0)){
    //    for(int i=0; i<this.FOwner.Owner.RptDefinition.InParams.Count; i++)
    //      vRslt = vRslt.Replace("#"+this.FOwner.Owner.RptDefinition.InParams[i].Name+"#", this.FOwner.Owner.RptDefinition.InParams[i].Value);
    //    //SQLCSProcessor.ProcessSQLCSFunction(ref vRslt);
    //  }else
    //    vRslt = null;*/
    //  return vRslt;
    //}


    public void Init(Excel.Range dsRange) {
      this.FRootGroup = new CXLRootGroup(this, dsRange);
    }

    CXLRDataRow convertDataRow(DataRow row) {
      CXLRDataRow rslt = new CXLRDataRow();
      foreach (DataColumn col in row.Table.Columns)
        rslt.Add(col.ColumnName, row[col.ColumnName]);
      return rslt;
    }

    public EBioException PrepareDataError { get; private set; }

    public void PrepareData(Int32 timeout) {

      if (this.Owner.OuterDSTable == null) {
        /* Создание DataFactory */
        this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Создание DataFactory...");
        CXLRDataFactory v_dataFactory = this.Owner.Owner.DataFactory;//CXLRDataFactory.createDataFactory(this.Owner.Cfg, this.Owner.DataFactoryTypeName);
        this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Создание DataFactory - OK.");
        try {

          /* Инициализация набора данных */
          this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Инициализация набора данных DataFactory...");
          if (this.Owner.Owner.RptDefinition.DBConnEnabled) {
            this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Параметры: " + this.Owner.Owner.RptDefinition.InParams);
            v_dataFactory.Open(this.Owner.Owner.currentDbConnection, this.Owner.Cfg, timeout);
          } else
            return;
          this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Инициализация набора данных DataFactory - ОК.");
          this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Заполнение структуры отчета из DataFactory...");
          long rnum = 0;
          while (v_dataFactory.Next()) {
            rnum++;
            this.RootGroup.DoOnFetch(rnum, v_dataFactory.CurentRowExt);
            if ((this.Owner.Cfg.maxExpRows > 0) && (rnum >= this.Owner.Cfg.maxExpRows)) {
              throw new EBioTooManyRows("... Внимание! Достигнут максимальный размер набора данных для экспорта в MS Excel - " + this.Owner.Cfg.maxExpRows + " записей.");
            }
          }
          this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Заполнение структуры отчета из DataFactory - ОК.");
        } catch(EBioTooManyRows ex){
          this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Заполнение структуры отчета из DataFactory - "+ex.Message);
          this.PrepareDataError = ex;
        } finally {
          if (v_dataFactory != null)
            v_dataFactory.Dispose();
          v_dataFactory = null;
        }
      } else {
        this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Заполнение структуры отчета из OuterDSTable...");
        //long rnum = 0;
        for (int i = 0; i < this.Owner.OuterDSTable.Rows.Count; i++) {
          //rnum++;
          this.RootGroup.DoOnFetch(i, convertDataRow(this.Owner.OuterDSTable.Rows[i]));
        }
        this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Заполнение структуры отчета из OuterDSTable - ОК.");
      }

      //if (this.Owner.Owner.RptDefinition.DebugIsOn) {
      //  this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.DSAlias + ") : this.RootGroup.GetXmlDoc().Save...");
      //  this.RootGroup.GetXmlDoc().Save(this.FOwner.Owner.RptDefinition.LogPath + this.FOwner.Owner.RptDefinition.ShrtCode + ".DS_DATA." + this.FOwner.DSRangeName + ".xml");
      //  this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.DSAlias + ") : this.RootGroup.GetXmlDoc().Save - OK.");
      //}

      //this.RootGroup.BuildReport();
      //this.Owner.Charts.Build(this.RootGroup);
    }


		public CParams GetSingleRow(Int32 timeout){
			CParams vResult = null;
      CXLRDataFactory v_dataFactory = this.Owner.Owner.DataFactory;//CXLRDataFactory.createDataFactory(this.Owner.Cfg, this.Owner.DataFactoryTypeName);
      try {

        //if(pOuterDSTable == null){
        //  String vSQL = this.getSQL();
        //  if(vSQL != null)
        //    this.FDSTable.PrepareDSTable(vSQL);
        //}else
        //  this.FDSTable.PrepareDSTable(pOuterDSTable);
        v_dataFactory.Open(this.Owner.Owner.currentDbConnection, this.Owner.Cfg, timeout);
        if (v_dataFactory.Next()) {
          vResult = new CParams();
          for (int i = 0; i < v_dataFactory.ColCount; i++) {
            vResult.Add(v_dataFactory.ColName(i), v_dataFactory.Values[i].ToString());
          }
        }

      }finally{
        //if (v_dataFactory != null)
        //v_dataFactory.Dispose();
        //v_dataFactory = null;
      }
      return vResult;
		}

	}
}
