namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Text;
  using System.IO;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  //using CrystalDecisions.CrystalReports.Engine;
  //using CrystalDecisions.Shared;

  class CRptBuilderCR {
    private CBioSession FSess = null;
    private String FRptCode = null;
    private String FRptFile = null;
    private String FTmpFileName = null;
    private String FRptFullPath_ws = null;
    private String FRptDonePath = null;
    private String FRptTmpPath = null;
    private String FRptLogPath = null;
    //private ExportFormatType FExpType = ExportFormatType.PortableDocFormat;

    public CRptBuilderCR(CBioSession pSess, String pRptCode) {
      this.FSess = pSess;
      this.FRptCode = pRptCode;
      this.FRptFile = this.FSess.Cfg.IniPath + this.FRptCode.Replace(".", "\\") + ".rpt";
      this.FRptFullPath_ws = this.FSess.Cfg.RptLogsPath + Utl.GenBioLocalPath(this.FRptCode);
      this.FRptDonePath = Path.GetDirectoryName(this.FRptFullPath_ws) + "\\done\\"; Directory.CreateDirectory(this.FRptDonePath);
      this.FRptTmpPath = Path.GetDirectoryName(this.FRptFullPath_ws) + "\\tmp\\";
      this.FRptLogPath = Path.GetDirectoryName(this.FRptFullPath_ws) + "\\log\\";
    }

    //public ExportFormatType ExpType(string pExportType) {
    //  switch (pExportType)
    //  {
    //    case "PDF":
    //      this.FExpType = ExportFormatType.PortableDocFormat;
    //      return this.FExpType;
    //    case "RPT":
    //      this.FExpType = ExportFormatType.CrystalReport;
    //      return this.FExpType;
    //    case "XLS":
    //      this.FExpType = ExportFormatType.Excel;
    //      return this.FExpType;
    //    case "DOC":
    //      this.FExpType = ExportFormatType.WordForWindows;
    //      return this.FExpType;
    //    case "RTF":
    //      this.FExpType = ExportFormatType.RichText;
    //      return this.FExpType;
    //    default:
    //      this.FExpType = ExportFormatType.PortableDocFormat;
    //      return this.FExpType;
    //  }
    //}

    public String ContentType{
      get{
        //switch(this.FExpType) { 
        //  case ExportFormatType.Excel:
        //    return "application/x-msexcel";
        //  case ExportFormatType.PortableDocFormat:
        //    return "application/pdf";
        //  case ExportFormatType.RichText:
        //    return "application/msword";
        //  case ExportFormatType.WordForWindows:
        //    return "application/msword";
        //  default:
        //    return "application/octet-stream";
        //} 
        return null;
      }
    }

    public String FileExt {
      get {
        //switch(this.FExpType) {
        //  case ExportFormatType.Excel:
        //  return "xls";
        //  case ExportFormatType.PortableDocFormat:
        //  return "pdf";
        //  case ExportFormatType.RichText:
        //  return "rtf";
        //  case ExportFormatType.WordForWindows:
        //  return "doc";
        //  case ExportFormatType.CrystalReport:
        //  return "rpt";
        //  default:
        //  return "000";
        //}
        return null;
      }
    }

    //private ParameterFieldDefinition findParamInRpt(ReportDocument pRpt, String pPrmName) {
    //  foreach(ParameterFieldDefinition vItem in pRpt.DataDefinition.ParameterFields) {
    //    if(vItem.Name.ToUpper().Equals(pPrmName.ToUpper()))
    //      return vItem;
    //  }
    //  return null;
    //}

    public void doBuild(CParams pParams) {
      //this.FTmpFileName = null;
      //if(!File.Exists(this.FRptFile))
      //  throw new EBioException("Не найден файл отчета " + this.FRptFile);

      //String rptAutor = null;
      //String rptTitle = null;
      //String rptSubject = null;
      //String rptComments = null;

      //ReportDocument rptDoc = new ReportDocument();
      //rptDoc.Load(this.FRptFile);

      //if(rptTitle != null)
      //  rptDoc.SummaryInfo.ReportTitle = rptTitle;
      //if(rptAutor != null)
      //  rptDoc.SummaryInfo.ReportAuthor = rptAutor;
      //if(rptSubject != null)
      //  rptDoc.SummaryInfo.ReportSubject = rptSubject;
      //if(rptComments != null)
      //  rptDoc.SummaryInfo.ReportComments = rptComments;

      ////Password=j12;Persist Security Info=True;User ID=DRPT;Data Source=NDOM_GP
      //String vConnStr = this.FSess.detectConnStr(null);
      //StringDictionary vConnStrItems = Bio.Common.Utl.parsConnectionStr(vConnStr);

      //TableLogOnInfo logOnInfo = new TableLogOnInfo();
      //logOnInfo.ConnectionInfo.ServerName = "";
      //logOnInfo.ConnectionInfo.ServerName = vConnStrItems["Data Source"];
      //logOnInfo.ConnectionInfo.UserID = vConnStrItems["User ID"];
      //logOnInfo.ConnectionInfo.Password = vConnStrItems["Password"];
      //foreach(Table table in rptDoc.Database.Tables)
      //  table.ApplyLogOnInfo(logOnInfo);
      ///*foreach(IConnectionInfo conn in rptDoc.DataSourceConnections) {
      //  conn.SetConnection(vConnStrItems["Data Source"], "", vConnStrItems["User ID"], vConnStrItems["Password"]);
      //} */

      

      //ExportOptions oExp = rptDoc.ExportOptions;
      //oExp.DestinationOptions = new DiskFileDestinationOptions();
      //oExp.ExportDestinationType = ExportDestinationType.DiskFile;
      //try {
      //  oExp.ExportFormatType = this.ExpType(pParams["ExportType"].Value);
      //  pParams.Remove("ExportType");
      //}
      //catch (Exception) {
      //  oExp.ExportFormatType = this.ExpType("");
      //}
      //this.FTmpFileName = this.FRptDonePath + Path.GetFileNameWithoutExtension(this.FRptFile) + "." + this.FileExt;
      //((DiskFileDestinationOptions)oExp.DestinationOptions).DiskFileName = this.FTmpFileName;

      ////******************************************************************************************************			
      //if(pParams != null) {
      //  foreach(CParam vPrm in pParams) {
      //    ParameterFieldDefinition paramField = this.findParamInRpt(rptDoc, vPrm.Name);
      //    if(paramField != null) {
      //      ParameterDiscreteValue paramValue = new ParameterDiscreteValue();
      //      ParameterValues currentValues = new ParameterValues();
      //      paramValue.Value = vPrm.Value;
      //      paramValue.Kind = DiscreteOrRangeKind.DiscreteValue;
      //      currentValues.Clear();
      //      currentValues.Add(paramValue);
      //      try {
      //        rptDoc.DataDefinition.ParameterFields[vPrm.Name].ApplyCurrentValues(currentValues);
      //      }
      //      catch (Exception e) {
      //        throw e;
      //      }
      //    }
      //  }
      //}
      ////******************************************************************************************************			

      //try {
      // // rptDoc.Refresh();не надо рефрешить! иначе снова просит параметры передать типа
      //  rptDoc.Export();
      //} catch(Exception ex) {
      //  throw new EBioException("[" + vConnStrItems["User ID"] + "/" + vConnStrItems["Password"] + "@" + vConnStrItems["Data Source"] + "]; "+ex.Message);
      //}

      ///*pPrms.response.WriteFile(tmpName);
      //pPrms.response.Flush();
      //pPrms.response.Close();
      //File.Delete(tmpName);*/

    }

    public String resultFileName {
      get {
        return this.FTmpFileName;
      }
    }
  }
}
