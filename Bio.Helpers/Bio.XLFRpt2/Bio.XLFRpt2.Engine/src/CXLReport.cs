namespace Bio.Helpers.XLFRpt2.Engine {

  using System;
  using System.Xml;
  using System.Web;
  using System.IO;
  using System.Threading;
  using System.Data;
  using Bio.Helpers.Common.Types;
  using System.ComponentModel;
  using Bio.Helpers.Common;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Text;
  using System.Collections.Generic;
#endif
  using Bio.Helpers.XLFRpt2.Engine.XLRptParams;

  public delegate void DlgXLReportOnPrepareTemplate(Object opener, ref Excel.Workbook wb, CXLReport report);
  public delegate void DlgXLReportOnBeforeBuild(Object opener, CXLReport report);
  public delegate void DlgXLReportOnChangeState(Object opener, CXLReport report, String text);
  public delegate void DlgXLReportOnStartBuild(Object opener, CXLReport report);
  public delegate void DlgXLReportOnAfterBuild(Object opener, CXLReport report);
  public delegate void DlgXLReportOnTerminatingBuild(Object opener, CXLReport report);
  public delegate void DlgXLReportOnError(Object opener, CXLReport report, Exception ex);
  public delegate void DlgXLReportOnBeforeDispose(CXLReport report);

  public class CXLReport : DisposableObject, IRemoteProcInst {
    //private
    private Object FOpener = null;
    private ExcelSrv FExcelSrv = null;
    private CXLRDefinition FRptDefinition = null;
    private HttpContext FHttpContext = null;
    private RemoteProcState FState = RemoteProcState.Redy;
    private Thread FReportThread = null;
    //private TXLReport FPrevReport = null;
    private bool FNoData = false;
    //private TXLRParams FOuterDSS = null;
    private String FProgress = "[0 %]";
    //private TimeSpan FLastDuration = TimeSpan.MinValue;
    private String FLastErrorFile = null;
    private XLRDataSources FDataSources = null;
    private String FLastReportResultFile = null;
    private CXLRSQLScript FCurrentSQLScript = null;

    private event DlgXLReportOnPrepareTemplate FOnPrepareTemplate;

    //public
    public event DlgXLReportOnBeforeBuild OnBeforeBuild;
    public event DlgXLReportOnStartBuild OnStartBuild;
    public event DlgXLReportOnChangeState OnChangeState;
    public event DlgXLReportOnAfterBuild OnAfterBuild;
    public event DlgXLReportOnError OnError;
    public event DlgXLReportOnBeforeDispose OnBeforeDispose;
    public event DlgXLReportOnTerminatingBuild OnTerminatingBuild;

    public ExcelSrv ExcelSrv {
      get { return this.FExcelSrv; }
    }



    public Object Opener {
      get {
        return this.FOpener;
      }
    }

    public DlgXLReportOnPrepareTemplate OnPrepareTemplate {
      get {
        return this.FOnPrepareTemplate;
      }
      set {
        this.FOnPrepareTemplate += value;
      }
    }
    private Boolean FExcelSrvIsOutter = false;

    //constructor
    public CXLReport(Object opener, CXLReportConfig cfg, HttpContext httpContext, Excel.Application excelInst) {
      this.State = RemoteProcState.Redy;
      this.FOpener = opener;
      this.FRptDefinition = new CXLRDefinition(this, cfg);
      this.FHttpContext = httpContext;
      this.FDataSources = new XLRDataSources(this, cfg.dss);
      this.FExcelSrvIsOutter = (excelInst != null);
      this.FExcelSrv = new ExcelSrv(excelInst);
      this.FLastReportResultFile = this.FRptDefinition.GetNewTempFileName();
    }

    public CXLReport(Object opener, CXLReportConfig cfg, HttpContext httpContext) : this(opener, cfg, httpContext, null) { }
    public CXLReport(Object opener, CXLReportConfig cfg) : this(opener, cfg, null, null) { }
    public CXLReport(CXLReportConfig cfg) : this(null, cfg, null, null) { }

    //destructor
    protected override void doOnDispose() {
      if (this.OnBeforeDispose != null)
        this.OnBeforeDispose(this);
      if (this.FRptDefinition != null) {
        this.FRptDefinition.Dispose();
        this.FRptDefinition = null;
      }
      this.FHttpContext = null;
      if (this.FDataSources != null) {
        this.FDataSources.Dispose();
        this.FDataSources = null;
      }
      if (this.FExcelSrvIsOutter && (this.FExcelSrv != null)) {
        this.FExcelSrv.Dispose();
        this.FExcelSrv = null;
      }
    }

    public XLRDataSources DataSources {
      get {
        return this.FDataSources;
      }
    }

    public void AddOuterDSS(String pAlias, DataTable pTable) {
      this.FDataSources.AssignOuterDSTable(pAlias, pTable);
    }

    public String FullCode {
      get {
        return this.RptDefinition.FullCode;
      }
    }

    public CXLRDefinition RptDefinition {
      get {
        return this.FRptDefinition;
      }
    }

    public void WriteTrace(String pGroup, String pLine) {
      if (this.FHttpContext != null)
        this.FHttpContext.Trace.Write(pGroup, pLine);
    }

    private const String csCOMExceptionID_1 = "No more results.";
    private const String csCOMExceptionID_2 = "Operation unavailable";
    private const String csCOMExceptionID_3 = "Операция недоступна";

    public String Progress {
      get {
        lock (this) {
          return this.FProgress;
        }
      }
    }


    public void DoOnProgressDataSource(XLRDataSource pSender, Decimal pPrgPrc) {
      this.FProgress = "" + pSender.Cfg.wsName + " [" + String.Format("{0:##0.0}", pPrgPrc) + " %]";
    }

    private void prepareData(Int32 timeout) {
      this.FDataSources.PrepareData(timeout);
    }

    private DateTime fLastTPoint = DateTime.MinValue;
    internal void writeLogLine(String msg){
      if(this.RptDefinition.DebugIsOn){
        String v_ip = this.RptDefinition.RemoteIP;
        DateTime v_cur_point = DateTime.Now;
        TimeSpan v_dure = v_cur_point - (this.fLastTPoint == DateTime.MinValue ? v_cur_point : this.fLastTPoint);
        this.fLastTPoint = v_cur_point;
        String v_tpoint = v_cur_point.ToString("yyyy.MM.dd HH:mm:ss");
        String v_duration = String.Format("{0}:{1}:{2}.{3}", 
                            v_dure.Hours.ToString("00"), v_dure.Minutes.ToString("00"), 
                            v_dure.Seconds.ToString("00"), v_dure.Milliseconds.ToString("000"));
        String v_msg = String.Format("{0} [{1}]:({2}):{3}", v_tpoint, v_duration, v_ip, msg);
        Utl.AppendStringToFile(this.RptDefinition.LogFileName, v_msg, Encoding.Default);
      }
    }

    private Action _afterAbortCallback = null;

    private IDbConnection _conn = null;
    public IDbConnection currentDbConnection { get { return this._conn; } }

    private void _initDBConnection() {
      if (this._conn != null)
        this._conn.Close();
      this._conn = null;
      Boolean connIsNeeded = !String.IsNullOrEmpty(this.RptDefinition.ConnStr);
      foreach (var ds in this.RptDefinition.RptCfg.dss)
        connIsNeeded = connIsNeeded || !String.IsNullOrEmpty(ds.sql);
      if (connIsNeeded) {
        this._conn = this.DataFactory.openDbConnection(this.RptDefinition.RptCfg);
      }
    }

    private void bldReportExec() {
      try {
        DateTime v_start_point = DateTime.Now;
        try {
          this.writeLogLine("Запуск...");
          lock (this) {
            this.Started = DateTime.Now;
            this.FNoData = false;//this.State == RemoteProcState.NoData;
            this.State = RemoteProcState.Running;
            if (this.OnBeforeBuild != null)
              this.OnBeforeBuild(this.FOpener, this);
          }
          if (!this.FNoData) {
            this._initDBConnection();
            try {
              // Запуск подготовительного скрипта
              if (this.RptDefinition.SQLScriptBefore != null) {
                this.writeLogLine("Запуск подготовительного скрипта...");
                this.FCurrentSQLScript = new CXLRSQLScript(this);
                String vPrdFileName = this.RptDefinition.LogPath + "sqlScriptBefore_prd.sql";
                try {
                  this.FCurrentSQLScript.Run(this.RptDefinition.SQLScriptBefore, vPrdFileName);
                } finally {
                  this.FCurrentSQLScript = null;
                }
                this.writeLogLine("Запуск подготовительного скрипта - OK.");
              }

              this.writeLogLine("Запуск OnStartBuild...");
              if (this.OnStartBuild != null)
                this.OnStartBuild(this.FOpener, this);
              this.writeLogLine("Запуск OnStartBuild - OK.");


              /* Инициализация отчета. Загрузка и считывание настроек из шаблона */
              this.writeLogLine("Запуск Do_init_report...");
              this.FExcelSrv.initReport(this);
              this.writeLogLine("Запуск Do_init_report - OK.");

              this.writeLogLine("Запуск prepareData...");
              this.prepareData(60 * 10);
              this.writeLogLine("Запуск prepareData - OK.");

              this.writeLogLine("Запуск Do_build_report...");
              this.FExcelSrv.buildReport(this, 60 * 5);
              this.writeLogLine("Запуск Do_build_report - OK.");

              this.writeLogLine("Запуск Do_finalize_report...");
              this.FExcelSrv.finalizeReport(this);
              this.writeLogLine("Запуск Do_finalize_report - OK.");

              this.FProgress = null;

            } finally {
              if (this._conn != null) {
                this._conn.Close();
                this._conn = null;
              }
            }

          }
        //} catch (ThreadAbortException ex) {
        //  this.FLastErrorFile = this.RptDefinition.LogPath + this.RptDefinition.ShortCode + ".err";
        //  this.writeLogLine("Ошибка: " + ex.ToString());
        //  Utl.SaveStringToFile(this.FLastErrorFile, ex.ToString(), null);
        //  this.State = RemoteProcState.Breaked;
        //} catch (EBioSQLBreaked ex) {
        //  this.FLastErrorFile = this.RptDefinition.LogPath + this.RptDefinition.ShortCode + ".err";
        //  this.writeLogLine("Ошибка: " + ex.ToString());
        //  Utl.SaveStringToFile(this.FLastErrorFile, ex.ToString(), null);
        //  this.State = RemoteProcState.Breaked;
        } catch (Exception ex) {
          this.FLastErrorFile = this.RptDefinition.LogPath + this.RptDefinition.ShortCode + ".err";
          this.writeLogLine("Ошибка: " + ex.ToString());
          Utl.SaveStringToFile(this.FLastErrorFile, ex.ToString(), null);
          if ((ex is ThreadAbortException) || (ex is EBioSQLBreaked))
            this.State = RemoteProcState.Breaked;
          else {
            this.State = RemoteProcState.Error;
            if (this.OnError != null)
              this.OnError(this.FOpener, this, ex);
          }
        } finally {
          if (this.State != RemoteProcState.Error) {
            if (this.State != RemoteProcState.Breaked)
              this.State = RemoteProcState.Done;
            try {
              this.writeLogLine("Запуск OnAfterBuild...");
              if (this.OnAfterBuild != null)
                this.OnAfterBuild(this.FOpener, this);
              this.writeLogLine("Запуск OnAfterBuild - OK.");
            } catch (Exception ex) {
              this.FLastErrorFile = this.RptDefinition.LogPath + this.RptDefinition.ShortCode + "_run_aftr_bld_event.err";
              this.writeLogLine("Ошибка: " + ex.ToString());
              Utl.SaveStringToFile(this.FLastErrorFile, ex.ToString(), null);
              this.State = RemoteProcState.Error;
            }
          }
          this._proccessAferAborCallback();
        }

        // Запуск завершающего скрипта
        if (this.RptDefinition.SQLScriptAfter != null) {
          this._initDBConnection();
          try {
            this.writeLogLine("Запуск завершающего скрипта...");
            this.FCurrentSQLScript = new CXLRSQLScript(this);
            String vPrdFileName = this.RptDefinition.LogPath + "sqlScriptAfter_prd.sql";
            try {
              this.FCurrentSQLScript.Run(this.RptDefinition.SQLScriptAfter, vPrdFileName);
            } finally {
              this.FCurrentSQLScript = null;
            }
            this.writeLogLine("Запуск завершающего скрипта - OK.");

          } finally {
            if (this._conn != null) {
              this._conn.Close();
              this._conn = null;
            }
          }
        }
      } finally {
        var eve = this.OnTerminatingBuild;
        if (eve != null)
          eve(this.FOpener, this);
      }
    }

    private void _proccessAferAborCallback() {
      if ((this._afterAbortCallback != null) && (this.State == RemoteProcState.Breaked)) {
        try {
          this.writeLogLine("Запуск afterAbortCallback...");
          this._afterAbortCallback();
          this._afterAbortCallback = null;
          this.writeLogLine("Запуск afterAbortCallback - OK.");
        } catch (Exception ex) {
          this.FLastErrorFile = this.RptDefinition.LogPath + this.RptDefinition.ShortCode + "_afterAbortCallback.err";
          this.writeLogLine("Ошибка: " + ex.ToString());
          Utl.SaveStringToFile(this.FLastErrorFile, ex.ToString(), null);
          this.State = RemoteProcState.Error;
        }
      }
    }

    /// <summary>
    /// Описание состояние процесса выполнения отчета
    /// </summary>
    public String StateDesc {
      get {
        return enumHelper.GetAttributeByValue<DescriptionAttribute>(this.State).Description;
      }
    }

    /// <summary>
    /// Строит отчет в том же потоке
    /// </summary>
    /// <returns>Путь к файлу - результату</returns>
    public String BuildReportSync() {
      this.bldReportExec();
      return this.FLastReportResultFile;
    }

    /// <summary>
    /// Установить приоритет, если отчет строится в отдельном потоке
    /// </summary>
    /// <param name="pPriority"></param>
    public void SetPriority(ThreadPriority pPriority) {
      if ((this.FReportThread != null) && (this.FReportThread.IsAlive))
        this.FReportThread.Priority = pPriority;
    }

    /// <summary>
    /// Создает описание отчета на основании переданных параметров
    /// </summary>
    /// <param name="rpt_title"></param>
    /// <param name="rpt_desc"></param>
    /// <param name="rpt_template"></param>
    /// <param name="rpt_params"></param>
    /// <param name="dataFactoryTypeName"></param>
    /// <param name="reportResultFileName"></param>
    /// <param name="dss"></param>
    /// <param name="connStr"></param>
    /// <param name="debug"></param>
    /// <returns></returns>
    public static String BuildReportConfigXml1(
        String rpt_title,
        String rpt_desc,
        String rpt_template,
        Params rpt_params,
        String dataFactoryTypeName,
        String reportResultFileName,
        List<CXLReportDSConfig> dss,
        String connStr,
        Boolean debug
      ) {
      String rpt_code = Path.GetFileNameWithoutExtension(rpt_template).Replace("(rpt)", null);
      dom4cs rptDocs = dom4cs.NewDocument("report");
      rptDocs.XmlDoc.DocumentElement.SetAttribute("full_code", rpt_code);
      rptDocs.XmlDoc.DocumentElement.SetAttribute("roles", "all");
      rptDocs.XmlDoc.DocumentElement.SetAttribute("debug", (debug ? "true" : "false"));
      rptDocs.XmlDoc.DocumentElement.SetAttribute("liveScripts", "false");
      if (!String.IsNullOrEmpty(dataFactoryTypeName))
        rptDocs.XmlDoc.DocumentElement.SetAttribute("defaultTableFactory", dataFactoryTypeName);

      StringWriter vApndXML = new StringWriter();

      vApndXML.WriteLine("<connstr><![CDATA[" + connStr + "]]></connstr>");
      vApndXML.WriteLine("<adv_template>" + rpt_template + "</adv_template>");
      vApndXML.WriteLine("<icon/>");
      vApndXML.WriteLine("<href/>");
      vApndXML.WriteLine("<filename_fmt><![CDATA[" + reportResultFileName + "]]></filename_fmt>");
      vApndXML.WriteLine("<title><![CDATA["+rpt_title+"]]></title>");
      vApndXML.WriteLine("<subject><![CDATA["+rpt_desc+"]]></subject>");
      vApndXML.WriteLine("<autor><![CDATA[]]></autor>");
      vApndXML.WriteLine("<macroBefore/>");
      vApndXML.WriteLine("<macroAfter/>");
      if (rpt_params != null) {
        vApndXML.WriteLine("<params>");
        foreach (Param vPrm in rpt_params)
          vApndXML.WriteLine("<param name=\"" + vPrm.Name + "\" type=\"notsql\">" + vPrm.Value + "</param>");
        vApndXML.WriteLine("</params>");
      } else
        vApndXML.WriteLine("<params/>");
      if (dss != null) {
        vApndXML.WriteLine("<dss>");
        foreach (var p in dss) {
          vApndXML.WriteLine("<ds alias=\"cds" + p.alias + "\" range=\"rng" + p.rangeName + "\" enabled=\"true\">");
          vApndXML.WriteLine("<sql commandType=\""+enumHelper.NameOfValue(p.commandType, false)+"\"><![CDATA[" + p.sql + "]]></sql>");
          vApndXML.WriteLine("<charts/>");
          vApndXML.WriteLine("</ds>");
        }
        vApndXML.WriteLine("</dss>");
      }else
        vApndXML.WriteLine("<dss/>");

      vApndXML.WriteLine("<append>");
      //vApndXML.WriteLine("<dbconnection>dummy</dbconnection>");
      vApndXML.WriteLine("<sessionID/>");
      vApndXML.WriteLine("<userName/>");
      vApndXML.WriteLine("<remoteIP/>");

      //String vRptFullPath_ws = Path.GetDirectoryName(template);
      //String vRptDonePath = vRptFullPath_ws + "\\done\\";
      //String vRptTmpPath = vRptFullPath_ws + "\\tmp\\";
      //String vRptLogPath = vRptFullPath_ws + "\\log\\";

      //vApndXML.WriteLine("<rptRootTree>" + Path.GetDirectoryName(template) + "</rptRootTree>");
      //vApndXML.WriteLine("<rptDefPath>" + template + "</rptDefPath>");
      //vApndXML.WriteLine("<donePath>" + vRptDonePath + "</donePath>");
      //vApndXML.WriteLine("<tmpPath>" + vRptTmpPath + "</tmpPath>");
      vApndXML.WriteLine("<rptWorkPath>" + Path.GetDirectoryName(rpt_template) + "</rptWorkPath>");
      if (rpt_params != null) {
        vApndXML.WriteLine("<inParams>");
        foreach (Param vPrm in rpt_params)
          vApndXML.WriteLine("<param name=\"" + vPrm.Name + "\">" + vPrm.Value + "</param>");
        vApndXML.WriteLine("</inParams>");
      } else {
        vApndXML.WriteLine("<inParams/>");
      }
      vApndXML.WriteLine("</append>");
      rptDocs.XmlDoc.DocumentElement.InnerXml = vApndXML.ToString();
      return rptDocs.XmlDoc.DocumentElement.OuterXml;
    }

    /// <summary>
    /// Синхронный вызов построителя отчета
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static String BuildReportSync(CXLReportConfig cfg) {

      String vResult = null;
      //String vRptDefDoc = BuildReportConfigXml(rpt_title, rpt_desc, rpt_template, null, dataFactoryTypeName, reportResultFileName, dss, connStr, debug);
      //CXLReport xlReportDocument = new CXLReport(null, vRptDefDoc, null, null);
      CXLReport xlReportDocument = new CXLReport(cfg);
      try {
        //xlReportDocument.DataSources.Add("cdsRpt", "mRng", null, dataFactoryTypeName);
        //xlReportDocument.RptDefinition.RptParams.AddRange(rpt_params);
        //xlReportDocument.RptDefinition.InParams.AddRange(rpt_params);
        xlReportDocument.BuildReportSync();
        vResult = xlReportDocument.LastResultFile;

      } finally {
        xlReportDocument.Dispose();
      }
      return vResult;
    }

    CXLRDataFactory _dataFactory = null;
    internal CXLRDataFactory DataFactory {
      get {
        if (this._dataFactory == null) {
          if (this.DataSources.Count > 0) {
            this._dataFactory = CXLRDataFactory.createDataFactory(this.RptDefinition.RptCfg);
          }
        }
        return this._dataFactory;
      }
    }


    #region IReportInst Members

    /// <summary>
    /// Уникальный идентификатор отчета. Генерется автоматом при создании экземпляра.
    /// </summary>
    public String UID {
      get {
        return this.RptDefinition.RptUID;
      }
    }

    /// <summary>
    /// Состояние процесса выполнения отчета
    /// </summary>
    public RemoteProcState State {
      get {
        return this.FState;
      }
      set {
        if (this.FState != value) {
          this.FState = value;
          if (this.OnChangeState != null)
            try {
              String vText = null;
              if (this.FState == RemoteProcState.Error) {
                if (File.Exists(this.FLastErrorFile))
                  StrFile.LoadStringFromFile(this.FLastErrorFile, ref vText, null);
              }
              this.OnChangeState(this.FOpener, this, vText);
            } catch (Exception ex) {
              this.FState = RemoteProcState.Error;
              Utl.SaveStringToFile(this.RptDefinition.LogPath + this.RptDefinition.ShortCode + ".err", "Ошибка при изменении состояния отчета. Сообщение: " + ex.ToString(), null);
            }
        }
      }
    }

    public Boolean IsRunning {
      get {
        return rmtUtl.isRunning(this.State);
      }
    }

    public bool IsFinished {
      get {
        return rmtUtl.isFinished(this.State);
      }
    }

    public EBioException LastError {
      get {

        EBioException ebioex = null;
        if (File.Exists(this.FLastErrorFile)) {
          String vErrText = null;
          StrFile.LoadStringFromFile(this.FLastErrorFile, ref vErrText, null);
          ebioex = new EBioException(vErrText);
        }

        return ebioex;
      }
    }

    public DateTime Started { get; private set; }
    public TimeSpan Duration { 
      get {
        return Utl.Duration(this.Started);
      } 
    }

    public String LastResultFile {
      get {
        return this.FLastReportResultFile;
      }
    }

    /// <summary>
    /// Прекратить выполнение
    /// </summary>
    public void Abort(Action callback) {
      this._afterAbortCallback = callback;
      this.State = RemoteProcState.Breaking;
      if (this.FCurrentSQLScript != null) {
        this.writeLogLine("Останов CurrentSQLScript..."); 
        this.FCurrentSQLScript.Cancel();
        this.writeLogLine("Останов CurrentSQLScript - выполнен.");
      }
      if ((this.FReportThread != null) && (this.FReportThread.IsAlive)) {
        this.writeLogLine("Запуск ReportThread.Abort...");
        this.FReportThread.Abort();
        this.writeLogLine("ReportThread.Abort - выполнен.");
      } else {
        this.State = RemoteProcState.Done;
        this._proccessAferAborCallback();
      }
    }

    //public String GetXmlDesc() {
    //  return "<report" +
    //    " guid=\"" + this.RptUID + "\"" +
    //    " remote_ip=\"" + this.RptDefinition.RemoteIP + "\"" +
    //    " user=\"" + this.RptDefinition.UserName + "\"" +
    //    " code=\"" + this.RptDefinition.ShortCode + "\"" +
    //    " state=\"" + this.State + "\"" +
    //    " state_cp=\"" + this.StateDesc + "\">" +
    //    "<title>[" + this.RptDefinition.FullCode + "] " + this.RptDefinition.Title + "</title>" +
    //    "<conn_str><![CDATA[" + this.RptDefinition.DBConnStr + "]]></conn_str>" +
    //     this.RptDefinition.InParams.AsXMLText() +
    //    "</report>";
    //}

    /// <summary>
    /// Строит отчет в новом потоке
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    public void Run(ThreadPriority priority) {
      if (this.State == RemoteProcState.Redy) {
        this.FReportThread = new Thread(new ThreadStart(this.bldReportExec));
        //this.FReportThread.Priority = (ThreadPriority)pPriority;
        this.FReportThread.Start();
      }
    }
    public void Run() {
      this.Run(ThreadPriority.Normal);
    }

    public String Owner {
      get {
        return this.RptDefinition.UserName;
      }
    }

    #endregion

    #region Для долгого процесса с трубой

    public string Pipe {
      get { return null; }
    }

    public void pushPipedLine(string[] pipedLines) {
      
    }

    public String[] popPipedLines() {
      return null; 
    }

    #endregion
  }
}