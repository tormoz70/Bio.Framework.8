namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
	using System.IO;
  using Bio.Helpers.Common.Types;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
#endif

	/// <summary>
	/// 
	/// </summary>
	/// 

	public class CXLRTemplateDef:DisposableObject{
    //private
    public const String csGrpHeaderDef = "groupH";
    public const String csGrpFooterDef = "groupF";
    public const String csDetailsDef = "details";
    public const String csTotalsDef = "totals";

    private CXLRRngList _headerFormats = new CXLRRngList();
    private CXLRRngList _footerFormats = new CXLRRngList();
    private Excel.Range _detailsRng = null;
    private Excel.Range _totalsRng = null;

		//constructor
    public CXLRTemplateDef(Excel.Range dsRange){
      for(int i = 1; i <= dsRange.Rows.Count; i++) {
        Excel.Range vCurRow = (Excel.Range)dsRange.Rows[i, Type.Missing];
        String vCurDefValue = ExcelSrv.ExtractCellValue(vCurRow.Cells[1, 1]);
        if (!String.IsNullOrEmpty(vCurDefValue) && vCurDefValue.StartsWith(csGrpHeaderDef)) {
          String vFldName = ExcelSrv.ExtractFieldName(vCurDefValue);
          this._headerFormats.Add(vFldName, vCurRow);
        } else
          if(!String.IsNullOrEmpty(vCurDefValue) && vCurDefValue.StartsWith(csGrpFooterDef)) {
            String vFldName = ExcelSrv.ExtractFieldName(vCurDefValue);
            this._footerFormats.Add(vFldName, vCurRow);
          } else
            if(!String.IsNullOrEmpty(vCurDefValue) && (vCurDefValue.Equals(csDetailsDef))) {
              this._detailsRng = vCurRow;
            } else
              if(!String.IsNullOrEmpty(vCurDefValue) && (vCurDefValue.Equals(csTotalsDef))) {
                this._totalsRng = vCurRow;
              }
      }
    }

    public void Check(CXLRootGroup rootGroup) {
      for(int i = 0; i < this._headerFormats.Count; i++) {
        String vFldName = this._headerFormats.getRangeName(i);
        CXLRColDef vColDef = rootGroup.ColDefs[vFldName];
        if(vColDef == null)
          throw new EBioException("Ошибка в описании шаблона. Заголовок группы привязан к полю [" + vFldName + "]. Данное поле должно быть обязательно объявлено в области [details].");
      }
      for(int i = 0; i < this._footerFormats.Count; i++) {
        String vFldName = this._footerFormats.getRangeName(i);
        CXLRColDef vColDef = rootGroup.ColDefs[vFldName];
        if(vColDef == null)
          throw new EBioException("Ошибка в описании шаблона. Итог группы привязан к полю [" + vFldName + "]. Данное поле должно быть обязательно объявлено в области [details].");
      }
    }

    protected override void doOnDispose() {
      foreach (var fmt in this._headerFormats)
        ExcelSrv.nar(ref fmt.Range);
      foreach (var fmt in this._footerFormats)
        ExcelSrv.nar(ref fmt.Range);

      this._headerFormats.Clear();
      this._headerFormats = null;
      this._footerFormats.Clear();
      this._footerFormats = null;
      ExcelSrv.nar(ref this._detailsRng);
      ExcelSrv.nar(ref this._totalsRng);
    }

    public CXLRRngList HeaderFormats {
      get { return this._headerFormats; }
    }
    public CXLRRngList FooterFormats {
      get { return this._footerFormats; }
    }
    public Excel.Range DetailsRng {
      get { return this._detailsRng; }
      set { this._detailsRng = value; }
    }
    public Excel.Range TotalsRng {
      get { return this._totalsRng; }
      set { this._totalsRng = value; }
    }

	}
}
