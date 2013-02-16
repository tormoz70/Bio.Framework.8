namespace Bio.Framework.Server {

  using System;

  using System.Data;
  using System.Data.Common;
  //using Oracle.DataAccess.Client;

  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Text;
  using System.IO;
  using System.Xml;
  //using CrystalDecisions.CrystalReports.Engine;
  //using CrystalDecisions.Shared;

  //internal struct CSelection {
  //  public bool selall;
  //  public LitJson_killd.JsonData items;
  //}

  //class CRptBuilderPF {
  //  private BioSession FSess = null;
  //  private String FRptCode = null;
  //  private String FRptFile = null;
  //  private String FTmpFileName = null;
  //  private String FRptFullPath_ws = null;
  //  private String FRptDonePath = null;
  //  private String FRptTmpPath = null;
  //  private String FRptLogPath = null;
  //  //private ExportFormatType FExpType = ExportFormatType.PortableDocFormat;

  //  public CRptBuilderPF(BioSession pSess, String pRptCode) {
  //    this.FSess = pSess;
  //    this.FRptCode = pRptCode;
  //    this.FRptFile = this.FSess.IniPath + this.FRptCode.Replace(".", "\\") + ".xml";
  //    this.FRptFullPath_ws = this.FSess.RptLogsPath + Bio.Common.Utl.genIOLocalPath(this.FRptCode);
  //    this.FRptDonePath = Path.GetDirectoryName(this.FRptFullPath_ws) + "\\done\\"; Directory.CreateDirectory(this.FRptDonePath);
  //    this.FRptTmpPath = Path.GetDirectoryName(this.FRptFullPath_ws) + "\\tmp\\";
  //    this.FRptLogPath = Path.GetDirectoryName(this.FRptFullPath_ws) + "\\log\\";
  //  }

  //  public String ContentType{
  //    get{
  //      return "text/plain";
  //    }
  //  }

  //  public String FileExt {
  //    get {
  //      return "DGV";
  //    }
  //  }

  //  //private ParameterFieldDefinition findParamInRpt(ReportDocument pRpt, String pPrmName) {
  //  //  foreach(ParameterFieldDefinition vItem in pRpt.DataDefinition.ParameterFields) {
  //  //    if(vItem.Name.ToUpper().Equals(pPrmName.ToUpper()))
  //  //      return vItem;
  //  //  }
  //  //  return null;
  //  //}

  //  public void doBuild(String pSelection) {
  //    this.FTmpFileName = null;
  //    if(!File.Exists(this.FRptFile))
  //      throw new EBioException("Не найден файл отчета " + this.FRptFile);
  //    XmlDocument vRptDoc = dom4cs.OpenDocument(this.FRptFile).XmlDoc;

  //    XmlElement vSQL = (XmlElement)vRptDoc.DocumentElement.SelectSingleNode("dss/ds/sql");
  //    String vSQLText = vSQL.InnerText;
      
  //    /*ReportDocument rptDoc = new ReportDocument();
  //    rptDoc.Load(this.FRptFile);

  //    if(rptTitle != null)
  //      rptDoc.SummaryInfo.ReportTitle = rptTitle;
  //    if(rptAutor != null)
  //      rptDoc.SummaryInfo.ReportAuthor = rptAutor;
  //    if(rptSubject != null)
  //      rptDoc.SummaryInfo.ReportSubject = rptSubject;
  //    if(rptComments != null)
  //      rptDoc.SummaryInfo.ReportComments = rptComments;*/

  //    //Password=j12;Persist Security Info=True;User ID=DRPT;Data Source=NDOM_GP
  //    /*StringDictionary vConnStrItems = utlSTD.parsConnectionStr(pConnStr);

  //    TableLogOnInfo logOnInfo = new TableLogOnInfo();
  //    logOnInfo.ConnectionInfo.ServerName = "";
  //    logOnInfo.ConnectionInfo.ServerName = vConnStrItems["Data Source"];
  //    logOnInfo.ConnectionInfo.UserID = "BP0";//vConnStrItems["User ID"];
  //    logOnInfo.ConnectionInfo.Password = vConnStrItems["Password"];
  //    foreach(Table table in rptDoc.Database.Tables)
  //      table.ApplyLogOnInfo(logOnInfo);
  //    */
      

  //    //this.FTmpFileName = this.FRptDonePath + Path.GetFileNameWithoutExtension(this.FRptFile) + "." + this.FileExt;
  //    this.FTmpFileName = this.FRptDonePath + "68"+DateTime.Now.ToString("ddMM") + "." + this.FileExt;
  //    if(File.Exists(this.FTmpFileName))
  //      File.Delete(this.FTmpFileName);

  //    //******************************************************************************************************			
  //    /*if(pParams != null) {
  //      foreach(Param vPrm in pParams) {
  //        ParameterFieldDefinition paramField = this.findParamInRpt(rptDoc, vPrm.Name);
  //        if(paramField != null) {
  //          ParameterDiscreteValue paramValue = new ParameterDiscreteValue();
  //          ParameterValues currentValues = new ParameterValues();
  //          paramValue.Value = vPrm.Value;
  //          paramValue.Kind = DiscreteOrRangeKind.DiscreteValue;
  //          currentValues.Clear();
  //          currentValues.Add(paramValue);
  //          try {
  //            rptDoc.DataDefinition.ParameterFields[vPrm.Name].ApplyCurrentValues(currentValues);
  //          }
  //          catch (Exception e) {
  //            throw e;
  //          }
  //        }
  //      }
  //    } */
  //    //******************************************************************************************************			
  //    CSelection vSelection = LitJson_killd.JsonMapper.ToObject<CSelection>(pSelection);
  //    int vSelAll = (vSelection.selall) ? 1: 0;
  //    String vSelectionList0 = vSelection.items.ToJson();
  //    String vSelectionList = vSelectionList0;
  //    if(!String.IsNullOrEmpty(vSelectionList) && (vSelectionList.Length > 3)) {
  //      vSelectionList = vSelectionList.Remove(0, 3);
  //      vSelectionList = vSelectionList.Remove(vSelectionList.Length - 3, 3);
  //      vSelectionList = vSelectionList.Replace(")\",\"(", ";");
  //    } else
  //      vSelectionList = String.Empty;
  //    String vConnStr = this.FSess.detectConnStr(vRptDoc.DocumentElement);
  //    //OracleConnection vConn = new OracleConnection(vConnStr);
  //    DbConnection vConn = Bio.DOA.CDBFactory.CreateConnection();
  //    vConn.ConnectionString = vConnStr;
  //    vConn.Open();
  //    try{
  //      CSQLCmd vCmd = new CSQLCmd(vConn);
  //      vCmd.Init(vSQLText, new Params());
  //      vCmd.Params.Add("p_selection", vSelectionList);
  //      vCmd.Params.Add("p_invert", vSelAll);
  //        vCmd.Open();
  //        try {
  //          while(vCmd.Next()) {
  //            String vLine = (String)vCmd.RowValues["F_LINE"];
  //            Bio.Common.Utl.AppendStringToFile(this.FTmpFileName, vLine, Encoding.GetEncoding("windows-1251"));
  //            //rptDoc.Refresh();не надо рефрешить! иначе снова просит параметры передать типа
  //            //rptDoc.Export();
  //          }
  //        } finally {
  //          vCmd.Close();
  //        }
  //    }finally{
  //      vConn.Close();
  //      vConn.Dispose();
  //    }

  //  }

  //  public String resultFileName {
  //    get {
  //      return this.FTmpFileName;
  //    }
  //  }
  //}
}
