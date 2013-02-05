namespace Bio.Framework.Server {
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using System.Globalization;
  using System.Threading;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using Bio.Helpers.XLFRpt2.Engine;
  using Bio.Framework.Packets;

  /// <summary>
  /// Генератор CSV файлов
  /// </summary>
  public class CCSVReport:DisposableObject, IRemoteProcInst {

    private String _bioCode;
    private HttpContext _context;
    private XmlElement _ds;
    //private CParams FCols;
    private IDbConnection _conn;
    private CParams _bioParams;
    private String _rptLogsPath;
    private Boolean _headerIsOn;

    public CCSVReport(
        String bioCode,
        HttpContext context, 
        XmlElement ds, 
        //CParams pCols, 
        CParams bioParams,
        IDbConnection conn,
        String rptLogsPath,
        Boolean headerIsOn) {
      this._bioCode = bioCode;
      this._context = context;
      this._ds = ds;
      //this.FCols = pCols;
      this._bioParams = bioParams;
      this._conn = conn;
      this._rptLogsPath = rptLogsPath;
      this._headerIsOn = headerIsOn;
      String vRptFullPath_ws = this._rptLogsPath + Utl.GenBioLocalPath(this._bioCode);
      String vRptDonePath = Path.GetDirectoryName(vRptFullPath_ws) + "\\done\\";
      if(!Directory.Exists(vRptDonePath))
        Directory.CreateDirectory(vRptDonePath);
      this._lastReportResultFile = vRptDonePath + this._bioCode + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
      this._lastErrorFile = Path.GetDirectoryName(vRptFullPath_ws) + this._bioCode + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".err";
    }

    private RemoteProcState _state = RemoteProcState.Redy;
    private Thread _thread = null;
    private String _lastErrorFile = null;
    private String _lastReportResultFile = null;
    private DateTime _started;

    private XmlElement findFldNodeByName(XmlElement pFlds, String pFldName) {
      foreach(XmlElement vFld in pFlds.ChildNodes) {
        String vFldNAme = vFld.GetAttribute("name");
        if(String.Equals(vFldNAme, pFldName, StringComparison.CurrentCultureIgnoreCase))
          return vFld;
      }
      return null;
    }
    private const String csEXPDATAROWNUMBER_COL_NAME = "EXPDATAROWNUMBER";
    private void prepareDS() {
      XmlElement vFldsRoot = (XmlElement)this._ds.SelectSingleNode("fields");
      XmlNodeList vFields = vFldsRoot.SelectNodes("field");
      XmlElement vFldDATARNUM = findFldNodeByName(vFldsRoot, csEXPDATAROWNUMBER_COL_NAME);
      if (vFldDATARNUM == null) {
        vFldDATARNUM = (XmlElement)vFldsRoot.AppendChild(vFldsRoot.OwnerDocument.CreateElement("field"));
        vFldDATARNUM.SetAttribute("name", csEXPDATAROWNUMBER_COL_NAME);
        vFldDATARNUM.SetAttribute("type", "int");
      }
    }

    private void bldCSV(CXLReportDSConfig dsCfg, CXLRDataFactory tbl, String fileName, Boolean addHeader, CParams colHeaders) {
      String vCSVDelimiter = ";";
      Encoding vEnc = Encoding.GetEncoding(1251);
      if (File.Exists(fileName))
        File.Delete(fileName);
      String vLine = null;
      if (addHeader) {
        if (colHeaders != null) {
          for (int j = 0; j < colHeaders.Count; j++) {
            //String vColHeader = (String)CParams.FindParamValue(pColHeaders, pTbl.GetCoulumnName(j));
            String vColHeader = colHeaders[j].ValueAsString();
            if (!String.IsNullOrEmpty(vColHeader))
              Utl.AppendStr(ref vLine, vColHeader, vCSVDelimiter);
          }
          Utl.AppendStringToFile(fileName, vLine, vEnc);
        }
      }
      var conn = tbl.openDbConnection(dsCfg.owner);
      try {
        tbl.Open(conn, dsCfg, 120);
        while (tbl.Next()) {
          vLine = null;
          for (int j = 0; j < colHeaders.Count; j++) {
            if (!String.Equals(colHeaders[j].Name, csEXPDATAROWNUMBER_COL_NAME, StringComparison.CurrentCultureIgnoreCase)) {
              Object vValueObj = tbl.ValueByName(colHeaders[j].Name);
              String vValue = null;
              if (vValueObj is DateTime)
                vValue = ((DateTime)vValueObj).ToString(CultureInfo.GetCultureInfo("ru-RU"));
              else
                vValue = (vValueObj != null) ? vValueObj.ToString() : null;
              if ((vValueObj is String) && !String.IsNullOrEmpty(vValue) && (vValue.IndexOf(vCSVDelimiter) >= 0)) {
                vValue = "\"" + vValue.Replace("\"", "\"\"") + "\"";
              }
              Utl.AppendStr(ref vLine, vValue, vCSVDelimiter);
            }
          }
          Utl.AppendStringToFile(fileName, vLine, vEnc);
        }
      } finally {
        conn.Close();
      }
    }

    private DataTable _prepareDSTable(String bioCode, CParams prms, IDbConnection conn) {
      DataTable vTable = new DataTable("DSTable");
      CJsonStoreRequestGet request = null; // (CExParams)CParams.FindParamValue(vPrms, "ExParams");
      CJSCursor vCursor = null;// new CJSCursor(vConn, vDSDefNode, bioCode);
      vCursor.Init(request);

      try {
        vCursor.Open(120);

        try {
          bool vIsFirstRow = true;
          Int64 vRowCount = 0;
          while (vCursor.Next()) {
            if (vIsFirstRow) {
              DataColumn vNewColmn = vTable.Columns.Add();
              vNewColmn.ColumnName = csEXPDATAROWNUMBER_COL_NAME;
              vNewColmn.DataType = typeof(Int64);
              for (int i = 0; i < vCursor.DataReader.FieldCount; i++) {
                vNewColmn = vTable.Columns.Add();
                vNewColmn.ColumnName = vCursor.DataReader.GetName(i);
                vNewColmn.DataType = vCursor.DataReader.GetFieldType(i);
              }
              vIsFirstRow = false;
            }
            Object[] vVals = new Object[vCursor.DataReader.FieldCount];
            vCursor.DataReader.GetValues(vVals);
            Object[] vVals1 = new Object[vVals.Length + 1];
            vVals.CopyTo(vVals1, 1);
            vRowCount++;
            vVals1[0] = vRowCount;
            vTable.Rows.Add(vVals1);
          }
        } catch (ThreadAbortException) {
          throw;
        } catch (Exception ex) {
          throw new EBioException("Ошибка при считывании данных из БД. Сообщение: " + ex.ToString(), ex);
        }
      } finally {
        if (vCursor != null)
          vCursor.Close();
        conn.Close();
      }
      return vTable;
    }

    private void buildFile() {
      try {
        this._state = RemoteProcState.Running;
        this._started = DateTime.Now;
        //this.prepareDS();
        //DbConnection vConn = this.CreOraConn(vDS);
        //CExParams vExParams = new CExParams(this._context.Request.Params);
        this._bioParams.SetValue("hide_deleted1", 1);
        this._bioParams.SetValue("hide_deleted2", 1);
        CParams vPrms = new CParams(
          new CParam("IOCode", this._bioCode),
          new CParam("DSDefNode", this._ds),
          new CParam("IOParams", this._bioParams),
          //new CParam("ExParams", vExParams),
          new CParam("DbConn", this._conn)
          );
        //CXLRDataFactory vTbl = new CXLRDataFactory(null, vPrms);
        //vTbl.PrepareDSTable();
        //this.bldCSV(vTbl, this._lastReportResultFile, this._headerIsOn, this.FCols);
      } catch (ThreadAbortException ex) {
        Utl.SaveStringToFile(this._lastErrorFile, ex.ToString(), null);
        this._state = RemoteProcState.Breaked;
      } catch (Exception ex) {
        Utl.SaveStringToFile(this._lastErrorFile, ex.ToString(), null);
        this._state = RemoteProcState.Error;
      } finally {
        if (this._state != RemoteProcState.Error) {
          if (this._state != RemoteProcState.Breaked)
            this._state = RemoteProcState.Done;
        }
      }
    }

    #region IReportInst Members

    public void Abort(Action callback) {
      if((this._thread != null) && (this._thread.IsAlive))
        this._thread.Abort();
      if (callback != null)
        callback();
    }

    public new void Dispose() {
      base.Dispose();
    }

    public Boolean IsRunning {
      get { 
        return rmtUtl.isRunning(this.State); 
      }
    }

    public String LastResultFile {
      get { return this._lastReportResultFile; }
    }

    public RemoteProcState State {
      get { return this._state; }
    }

    public DateTime Started {
      get { return this._started; }
    }
    public TimeSpan Duration {
      get {
        return Utl.Duration(this.Started);
      }
    }

    public String UID {
      get { return null; }
    }

    public String GetXmlDesc() {
      return null;  /*"<report" +
         " guid=\"" + this.RptUID + "\"" +
         " remote_ip=\"" + this.RptDefinition.RemoteIP + "\"" +
         " user=\"" + this.RptDefinition.UserName + "\"" +
         " code=\"" + this.RptDefinition.ShortCode + "\"" +
         " state=\"" + this.State + "\"" +
         " state_cp=\"" + this.StateDesc + "\">" +
         "<title>[" + this.RptDefinition.FullCode + "] " + this.RptDefinition.Title + "</title>" +
         "<conn_str><![CDATA[" + this.RptDefinition.DBConnStr + "]]></conn_str>" +
          this.RptDefinition.InParams.AsXMLText() +
         "</report>";*/
    }

    public void Run(ThreadPriority priority) {
      if (this.State == RemoteProcState.Redy) {
        this._thread = new Thread(new ThreadStart(this.buildFile));
        this._thread.Priority = ThreadPriority.Normal;
        this._thread.Start();
      }
    }
    public void Run() {
      this.Run(ThreadPriority.Normal);
    }

    #endregion


    #region IRemoteProcInst Members

    public string Owner {
      get { throw new NotImplementedException(); }
    }

    public EBioException LastError {
      get {
        EBioException ebioex = null;
        if (File.Exists(this._lastErrorFile)) {
          String vErrText = null;
          StrFile.LoadStringFromFile(this._lastErrorFile, ref vErrText, null);
          ebioex = new EBioException(vErrText);
        }

        return ebioex;
      }
    }

    public string Pipe {
      get { throw new NotImplementedException(); }
    }

    public void pushPipedLine(string[] pipedLines) {
      throw new NotImplementedException();
    }

    public string[] popPipedLines() {
      throw new NotImplementedException();
    }

    #endregion
  }
}
