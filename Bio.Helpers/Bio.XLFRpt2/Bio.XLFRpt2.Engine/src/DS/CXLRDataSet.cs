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

    private XLRDataSource FOwner = null;
    private CXLRootGroup FRootGroup;

#pragma warning disable 0067
    public event DlgOnProgressDataSet OnProgress;
#pragma warning restore 0067

    public CXLRDataSet(XLRDataSource pOwner){
			this.FOwner = pOwner;
    }

    protected override void doOnDispose() {
      if(this.FRootGroup != null) {
        this.FRootGroup.Dispose();
        this.FRootGroup = null;
      }
    }

		public XLRDataSource Owner{
			get{
				return FOwner;
			}
		}

    public CXLRootGroup RootGroup{
      get{
        return this.FRootGroup;
      }
    }

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
        
        for (int i = 0; i < this.Owner.OuterDSTable.Rows.Count; i++) {
          this.RootGroup.DoOnFetch(i, convertDataRow(this.Owner.OuterDSTable.Rows[i]));
        }
        this.Owner.Owner.writeLogLine("\tds:(" + this.Owner.Cfg.alias + ") : Заполнение структуры отчета из OuterDSTable - ОК.");
      }

    }


		public Params GetSingleRow(Int32 timeout){
			Params vResult = null;
      CXLRDataFactory v_dataFactory = this.Owner.Owner.DataFactory;//CXLRDataFactory.createDataFactory(this.Owner.Cfg, this.Owner.DataFactoryTypeName);
      try {

        v_dataFactory.Open(this.Owner.Owner.currentDbConnection, this.Owner.Cfg, timeout);
        if (v_dataFactory.Next()) {
          vResult = new Params();
          for (int i = 0; i < v_dataFactory.ColCount; i++) {
            vResult.Add(v_dataFactory.ColName(i), v_dataFactory.Values[i].ToString());
          }
        }

      }finally{
      }
      return vResult;
		}

	}
}
