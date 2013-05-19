namespace Bio.Framework.Server {

  using System;

  using System.Data;
  using System.Data.Common;
  //using Oracle.DataAccess.Client;

  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using System.Globalization;
  using Excel = Microsoft.Office.Interop.Excel;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using Bio.Helpers.XLFRpt2.Engine;
  using System.Collections.Generic;
  using System.Threading;
  using Bio.Helpers.Common;

  /// <summary>
  /// Данный обработчик вх. сообщений является шаблоном для создания нового сообщения
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_DS2XL: ABioHandlerBio {

    public tmio_DS2XL(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private Excel.Range addFirstCol(ref Excel.Range pRng, Excel.Range vCurCol) {
      int vCurColN = vCurCol.Column;
      int vCurRowN = vCurCol.Row;
      int vCurRowsN = vCurCol.Rows.Count;
      vCurCol.Insert(Excel.XlInsertShiftDirection.xlShiftToRight, true);
      Excel.Range vNewCol = ExcelSrv.getRange(vCurCol.Worksheet, vCurCol.Worksheet.Cells[vCurRowN, vCurColN],
                                                         vCurCol.Worksheet.Cells[vCurRowN + vCurRowsN, vCurColN]);
      vCurCol.Copy(vNewCol);
      //pRng = XLRUtils.UnionRanges(pRng, vNewCol);
      vCurCol.ColumnWidth = vNewCol.ColumnWidth;
      return vCurCol;
    }

    private void initCol(Excel.Range col, CXLReportDSFieldDef fld) {
      Excel.Range v_headerCell = (Excel.Range)col.Cells[1, 1];
      Excel.Range v_dataCell = (Excel.Range)col.Cells[2, 1];

      //    Selection.NumberFormat = "0" -int
      //    Selection.NumberFormat = "@" -string
      //    Selection.NumberFormat = "0.000" -float
      //    Selection.NumberFormat = "#,##0.00$" -money
      v_dataCell.Formula = "=cdsRpt_" + fld.name;
      try {
        v_dataCell.NumberFormat = String.IsNullOrEmpty(fld.format) ? "@" : fld.format;
      } catch {
        throw this.creBioEx("Не допустимый формат столбца " + fld.name + ", expXL_format=\"" + fld.format + "\".", null);
      }

      v_dataCell.HorizontalAlignment = ExcelSrv.ConvertAlignJS2XL(fld.align);

      v_dataCell.ColumnWidth = fld.width;
      v_headerCell.Formula = String.IsNullOrEmpty(fld.header) ? "<пусто>" : fld.header.Replace("\n", String.Empty);
    }

    private void insertCols(ref Excel.Range rng, List<CXLReportDSFieldDef> fields) {
      Excel.Range vCol = rng;
      this.initCol(vCol, fields[0]);
      for(int i=1; i<fields.Count; i++) {
        vCol = this.addFirstCol(ref rng, vCol);
        this.initCol(vCol, fields[i]);
      }
    }

    private void DoOnPrepareTemplate(Object opener, ref Excel.Workbook wb, CXLReport report) {
        Excel.Range vRng = ExcelSrv.GetRangeByName(ref wb, "mRng");
        if (vRng != null) {
          Excel.Worksheet vWS = vRng.Worksheet;
          vRng = ExcelSrv.getRange(vWS, vWS.Cells[vRng.Row - 1, 2], vWS.Cells[vRng.Row + vRng.Rows.Count, 2]);
          var dsCfg = report.RptDefinition.dsCfgByRangeName("mRng");
          var colHeadersFromCli = Params.FindParamValue<Dictionary<String, String>>(report.RptDefinition.InParams, "dataGridOnClientHeaders");
          if (colHeadersFromCli != null) {
            foreach (var fd in dsCfg.fieldDefs) {
              String colHeader;
              if (colHeadersFromCli.TryGetValue(fd.name.ToUpper(), out colHeader))
                fd.header = colHeader;
            }
          }
          this.insertCols(ref vRng, dsCfg.fieldDefs);
        }
    }

   protected override void doExecute() {
      base.doExecute();

      String vExpTypeStr = this.getQParamValue("exptype", false);
      if (String.IsNullOrEmpty(vExpTypeStr))
        vExpTypeStr = "xls";
      var vBldr = new CRmtThreadHandler(
          this.BioSession,
          "application/octet-stream",
          "report[" + this.bioCode + "]");
        if (vExpTypeStr == "xls")
          vBldr.OnRunEvent += new CRmtThreadOnRunEvent(this.doOnRunEventXLS);
        else if (vExpTypeStr == "csv")
          vBldr.OnRunEvent += new CRmtThreadOnRunEvent(this.doOnRunEventCSV);
        vBldr.doExecute(this.bioRequest<CRmtClientRequest>().cmd, this.bioParams);
    }

    private void doOnRunEventXLS(CRmtThreadHandler sender, ref IRemoteProcInst instance) {
      String vTitle = this.bioRequest<CRmtClientRequest>().title;
      if (vTitle != null)
        vTitle = vTitle.Replace("<br>", "\n");
      else
        vTitle = "";
      CIObject vIO = this.BioSession.IObj_get(this.bioCode);
      if(vIO != null) {
        String vDefaultTmpl = this.BioSession.Cfg.IniPath + "iod\\bio2xl_default.xlsm";
        String vCustomTmpl = vIO.ioTemplate2XL;
        if((!File.Exists(vDefaultTmpl)) && (String.IsNullOrEmpty(vCustomTmpl)))
          throw this.creBioEx("Шаблон для экспорта не найден в системе.", null);

        Boolean vIsDefaultTempl = true;
        String vSeldTempl = vDefaultTmpl;
        if(!String.IsNullOrEmpty(vCustomTmpl)) {
          vSeldTempl = vCustomTmpl;
          vIsDefaultTempl = false;
        }

        var v_rptCfg = new CXLReportConfig();
        v_rptCfg.fullCode = vIO.bioCode;
        v_rptCfg.extAttrs.roles = "all";
        v_rptCfg.debug = false;
        v_rptCfg.extAttrs.liveScripts = false;
        v_rptCfg.templateAdv = vSeldTempl;
        v_rptCfg.title = vTitle;
        v_rptCfg.filenameFmt = "{$code}_{$now}";
        v_rptCfg.dbSession = this.BioSession.Cfg.dbSession;
        v_rptCfg.extAttrs.sessionID = this.BioSession.CurSessionID;
        v_rptCfg.extAttrs.userUID = this.BioSession.Cfg.CurUser.UID;
        v_rptCfg.extAttrs.remoteIP = this.BioSession.CurSessionRemoteIP;
        v_rptCfg.extAttrs.workPath = this.BioSession.Cfg.WorkspacePath; //this.BioSession.Cfg.IniPath; //vIO.LocalPath;
        //rptCfg.extAttrs.
        foreach (var v_prm in this.bioParams)
          v_rptCfg.inPrms.Add((Param)v_prm.Clone());
        v_rptCfg.debug = Xml.getAttribute<Boolean>(vIO.IniDocument.XmlDoc.DocumentElement, "debug", false);
        v_rptCfg.dss.Add(CXLReportDSConfig.DecodeFromBio(
          vIO.IniDocument.XmlDoc.DocumentElement,
          vIO.LocalPath, "cdsRpt", "mRng", v_rptCfg.title, null));
        instance = new CXLReport(vIO, v_rptCfg, this.Context);
        if(vIsDefaultTempl)
          (instance as CXLReport).OnPrepareTemplate += new DlgXLReportOnPrepareTemplate(this.DoOnPrepareTemplate);
      }
    }

    private void doOnRunEventCSV(CRmtThreadHandler sender, ref IRemoteProcInst instance) {
      XmlElement vDS = this.FBioDesc.DocumentElement;
      IDbConnection vConn = this.BioSession.Cfg.dbSession.GetConnection();
      //String vColsList = this.getQParamValue("cols", false);
      String vAddHeaderStr = this.getQParamValue("addheader", false);
      Boolean vAddHeader = !String.IsNullOrEmpty(vAddHeaderStr) && vAddHeaderStr.Equals("true");
      instance = new CCSVReport(
        this.bioCode,
        this.Context,
        vDS,
        //nm,
        //vCols,
        this.bioParams,
        vConn,
        this.BioSession.Cfg.RptLogsPath,
        vAddHeader);
    }

  }
}
