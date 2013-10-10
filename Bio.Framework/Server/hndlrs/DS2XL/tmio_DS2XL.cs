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
  /// ������ ���������� ��. ��������� �������� �������� ��� �������� ������ ���������
  /// ���������� �������� �� ����������� �������
  /// </summary>
  public class tmio_DS2XL: ABioHandlerBio {

    public tmio_DS2XL(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    private Excel.Range addFirstCol(ref Excel.Range pRng, Excel.Range vCurCol) {
      int vCurColN = vCurCol.Column;
      int vCurRowN = vCurCol.Row;
      int vCurRowsN = vCurCol.Rows.Count;
      vCurCol.Insert(Excel.XlInsertShiftDirection.xlShiftToRight, true);
      Excel.Range vNewCol = ExcelSrv.getRange(vCurCol.Worksheet, vCurCol.Worksheet.Cells[vCurRowN, vCurColN],
                                                         vCurCol.Worksheet.Cells[vCurRowN + vCurRowsN, vCurColN]);
      vCurCol.Copy(vNewCol);
      vCurCol.ColumnWidth = vNewCol.ColumnWidth;
      return vCurCol;
    }

    private void _initCol(Excel.Range col, CXLReportDSFieldDef fld) {
      var v_headerCell = (Excel.Range)col.Cells[1, 1];
      var v_dataCell = (Excel.Range)col.Cells[2, 1];

      //    Selection.NumberFormat = "0" -int
      //    Selection.NumberFormat = "@" -string
      //    Selection.NumberFormat = "0.000" -float
      //    Selection.NumberFormat = "#,##0.00$" -money
      v_dataCell.Formula = "=cdsRpt_" + fld.name;
      try {
        v_dataCell.NumberFormat = String.IsNullOrEmpty(fld.format) ? "@" : fld.format;
      } catch {
        throw this.creBioEx("�� ���������� ������ ������� " + fld.name + ", expXL_format=\"" + fld.format + "\".", null);
      }

      v_dataCell.HorizontalAlignment = ExcelSrv.ConvertAlignJS2XL(fld.align);

      v_dataCell.ColumnWidth = fld.width;
      v_headerCell.Formula = String.IsNullOrEmpty(fld.header) ? "<�����>" : fld.header.Replace("\n", String.Empty);
    }

    private void insertCols(ref Excel.Range rng, List<CXLReportDSFieldDef> fields) {
      Excel.Range vCol = rng;
      this._initCol(vCol, fields[0]);
      for(int i=1; i<fields.Count; i++) {
        vCol = this.addFirstCol(ref rng, vCol);
        this._initCol(vCol, fields[i]);
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

      var expTypeStr = this.getQParamValue("exptype", false);
      if (String.IsNullOrEmpty(expTypeStr))
        expTypeStr = "xls";
      var bldr = new RmtThreadHandler(
          this.BioSession,
          "application/octet-stream",
          "report[" + this.bioCode + "]");
        if (expTypeStr == "xls")
          bldr.OnRunEvent += this._doOnRunEventXls;
        else if (expTypeStr == "csv")
          bldr.OnRunEvent += this._doOnRunEventCsv;
        bldr.DoExecute(this.BioRequest<RmtClientRequest>().cmd, this.bioParams);
    }

    private void _doOnRunEventXls(RmtThreadHandler sender, out IRemoteProcInst instance) {
      var title = this.BioRequest<RmtClientRequest>().title;
      title = title != null ? title.Replace("<br>", "\n") : "";
      var io = this.BioSession.IObj_get(this.bioCode);
      if (io == null)
        throw new Exception(String.Format("Bio {0} �� ������!", this.bioCode));
      var defaultTmpl = this.BioSession.Cfg.IniPath + "iod\\bio2xl_default.xlsm";
      var customTmpl = io.ioTemplate2XL;
      if ((!File.Exists(defaultTmpl)) && (String.IsNullOrEmpty(customTmpl)))
        throw this.creBioEx("������ ��� �������� �� ������ � �������.", null);

      var isDefaultTempl = true;
      var seldTempl = defaultTmpl;
      if (!String.IsNullOrEmpty(customTmpl)) {
        seldTempl = customTmpl;
        isDefaultTempl = false;
      }

      var rptCfg = new CXLReportConfig();
      rptCfg.fullCode = io.bioCode;
      rptCfg.extAttrs.roles = "all";
      rptCfg.debug = false;
      rptCfg.extAttrs.liveScripts = false;
      rptCfg.templateAdv = seldTempl;
      rptCfg.title = title;
      rptCfg.filenameFmt = "{$code}_{$now}";
      rptCfg.dbSession = this.BioSession.Cfg.dbSession;
      rptCfg.extAttrs.sessionID = this.BioSession.CurSessionID;
      rptCfg.extAttrs.userUID = this.BioSession.Cfg.CurUser.UID;
      rptCfg.extAttrs.remoteIP = this.BioSession.CurSessionRemoteIP;
      rptCfg.extAttrs.workPath = this.BioSession.Cfg.WorkspacePath;
      foreach (var prm in this.bioParams)
        rptCfg.inPrms.Add((Param) prm.Clone());
      foreach (var prm in this.bioParams)
        rptCfg.rptPrms.Add(prm.Name, prm.Value, "str");
      rptCfg.debug = Xml.getAttribute<Boolean>(io.IniDocument.XmlDoc.DocumentElement, "debug", false);
      rptCfg.dss.Add(CXLReportDSConfig.DecodeFromBio(
        io.IniDocument.XmlDoc.DocumentElement,
        io.LocalPath, "cdsRpt", "mRng", rptCfg.title, null));
      instance = new CXLReport(io, rptCfg, this.Context);
      if (isDefaultTempl)
        (instance as CXLReport).OnPrepareTemplate += this.DoOnPrepareTemplate;

    }

    private void _doOnRunEventCsv(RmtThreadHandler sender, out IRemoteProcInst instance) {
      var ds = this.FBioDesc.DocumentElement;
      var conn = this.BioSession.Cfg.dbSession.GetConnection();
      var addHeaderStr = this.getQParamValue("addheader", false);
      var addHeader = !String.IsNullOrEmpty(addHeaderStr) && addHeaderStr.Equals("true");
      instance = new CCSVReport(
        this.bioCode,
        this.Context,
        ds,
        this.bioParams,
        conn,
        this.BioSession.Cfg.RptLogsPath,
        addHeader);
    }

  }
}
