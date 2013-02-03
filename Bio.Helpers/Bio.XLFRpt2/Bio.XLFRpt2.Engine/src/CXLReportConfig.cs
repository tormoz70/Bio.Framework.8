namespace Bio.Helpers.XLFRpt2.Engine {

  using System;
  using System.IO;
  using System.Xml;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using System.Data;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;

  public class CXLReportDSConfig {
    public CXLReportConfig owner { get; internal set; }
    public String wsName { get; set; }
    public String sql { get; set; }
    public CommandType commandType { get; set; }
    public DataTable outerDataTable { get; set; }
    public List<CXLReportDSFieldDef> fieldDefs { get; set; }
    public Int64 maxExpRows { get; set; }
    private Int64? _maxRowsLimit = null;
    public Int64 maxRowsLimit {
      get {
        if (this.owner.TemplateFileExt.ToLower().EndsWith("xls"))
          return this._maxRowsLimit ?? 65000L;
        else
          return this._maxRowsLimit ?? 900000L;
      }
    }

    private String _alias = null;
    public String alias {
      get { return this._alias; }
      set {
        this._alias = value;
        if (String.IsNullOrEmpty(this.rangeName))
          this.rangeName = this._alias;
        if (String.IsNullOrEmpty(this.wsName))
          this.wsName = "Набор данных [" + this._rangeName + "]";
      }
    }
    private String _rangeName = null;
    public String rangeName {
      get {
        return this._rangeName;
      }
      set {
        this._rangeName = value;
        if (String.IsNullOrEmpty(this.alias))
          this.alias = this._rangeName;
        if (String.IsNullOrEmpty(this.wsName))
          this.wsName = "Набор данных [" + this._rangeName + "]";
      }
    }
    public Boolean leaveGroupData { get; set; }

    public static CXLReportDSConfig Decode(XmlElement definition, String rptLocalPath) {
      CXLReportDSConfig rslt = new CXLReportDSConfig();
      rslt.leaveGroupData = Xml.getAttribute<Boolean>(definition, "leaveGroupData", false);
      rslt.alias = definition.GetAttribute("alias");
      rslt.rangeName = definition.GetAttribute("range");
      rslt._maxRowsLimit = Xml.getAttribute<Int64?>(definition, "maxRowsLimit", null);
      rslt.wsName = "Набор данных [" + rslt.alias + "]";
      if(definition.HasAttribute("title"))
        rslt.wsName = definition.GetAttribute("title");
      rslt.sql = null;
      XmlElement vSQLElem = (XmlElement)definition.SelectSingleNode("sql");
      if (vSQLElem != null) {
        if (vSQLElem.HasAttribute("commandType"))
          rslt.commandType = enumHelper.GetFieldValueByValueName<CommandType>(vSQLElem.GetAttribute("commandType"), StringComparison.CurrentCulture);
        rslt.sql = vSQLElem.InnerText;
      }
      String vSQL = rslt.sql;
      Utl.TryLoadMappedFiles(rptLocalPath, ref vSQL);
      rslt.sql = vSQL;

      return rslt;
    }
    public static CXLReportDSConfig DecodeFromBio(
      XmlElement definition, 
      String rptLocalPath, 
      String alias, 
      String rangeName,
      String title,
      String factoryTypeName) 
    {
      CXLReportDSConfig rslt = new CXLReportDSConfig();
      rslt.alias = alias;
      rslt.rangeName = rangeName;
      rslt.wsName = (String.IsNullOrEmpty(title)) ? "Набор данных [" + rslt.alias + "]" : title;
      rslt.sql = null;
      XmlElement vSQLElem = (XmlElement)definition.SelectSingleNode("SQL");
      if (vSQLElem != null) {
        rslt.sql = vSQLElem.InnerText;
      }
      String vSQL = rslt.sql;
      Utl.TryLoadMappedFiles(rptLocalPath, ref vSQL);
      rslt.sql = vSQL;
      rslt.commandType = Utl.detectCommandType(rslt.sql);

      rslt.maxExpRows = Xml.getAttribute<Int64>(definition, "maxExpRows", 0);

      XmlNodeList flds = definition.SelectNodes("fields/field");
      if (flds.Count > 0) {
        rslt.fieldDefs = new List<CXLReportDSFieldDef>();
        foreach (XmlElement elem in flds) {
          if (!Xml.getAttribute<Boolean>(elem, "expEnabled", true) || Xml.getAttribute<Boolean>(elem, "hidden", false))
            continue;
          CXLReportDSFieldDef fldDef = new CXLReportDSFieldDef();
          fldDef.name = elem.GetAttribute("name");
          fldDef.type = jsonUtl.detectFieldType(elem.GetAttribute("type"));
          fldDef.align = jsonUtl.detectAlignment(fldDef.type, elem.GetAttribute("align"));
          fldDef.header = elem.GetAttribute("header");
          fldDef.format = elem.GetAttribute("expFormat");
          fldDef.width = Xml.getAttribute(elem, "expWidth", 20);
          rslt.fieldDefs.Add(fldDef);
        }
      }
      return rslt;
    }
  }


  public class CXLReportConfigExtAttributes { 
    public String rootTreePath { get; set; }
    private String _workPath = null;
    public String workPath { 
      get {
        return this._workPath;
      } 
      set {
        this._workPath = Utl.NormalizeDir(value);
      } 
    }
    public String defFileName { get; set; }
    public String localPath { get; set; }
    public String shortCode { get; set; }
    public String throwCode { get; set; }
    public Boolean liveScripts { get; set; }
    public String roles { get; set; }
    public String sessionID { get; set; }
    public String userUID { get; set; }
    public String remoteIP { get; set; }
    //public String logFileName { get; set; }
    public String pwdOpen { get; set; }
    public String pwdWrite { get; set; }
    public CXLRMacro macroBefore { get; set; }
    public CXLRMacro macroAfter { get; set; }
    public String sqlScriptBefore { get; set; }
    public String sqlScriptAfter { get; set; }
  }

  public class CXLReportConfig {
    public const String csDefaultDataFactoryTypeName = "Bio.Helpers.XLFRpt2.DataFactory.CDataFactory, Bio.Helpers.XLFRpt2.DefaultDataFactory";
    public String uid { get; set; }
    private String _fullCode = null;
    public String fullCode { 
      get {
        return this._fullCode;
      }
      set {
        this._fullCode = value;
        String[] prts = Utl.SplitString(this._fullCode, '.');
        this.extAttrs.shortCode = prts.LastOrDefault();
      }
    }
    public String title { get; set; }
    public String subject { get; set; }
    public String autor { get; set; }
    private String _templateAdv = null;
    public String templateAdv { 
      get {
        return this._templateAdv;
      } 
      set {
        this._templateAdv = value;
        if (String.IsNullOrEmpty(this.extAttrs.workPath))
          this.extAttrs.workPath = Path.GetDirectoryName(this._templateAdv);
        if (String.IsNullOrEmpty(this.fullCode))
          this.fullCode = _fileName2ShortCode(this._templateAdv);
        //if (String.IsNullOrEmpty(this.extAttrs.shortCode))
        //  this.extAttrs.shortCode = _fileName2ShortCode(this._templateAdv);
      } 
    }
    private CParams _inPrms = null;
    public CParams inPrms { 
      get {
        if (this._inPrms == null)
          this._inPrms = new CParams();
        return this._inPrms; 
      }
      set {
        this._inPrms = value;
        if (this.rptPrms == null) {
          this.rptPrms = new CParams();
          this.rptPrms.AddRange(this._inPrms);
        }
      }
    }
    private CParams _rptPrms = null;
    public CParams rptPrms { 
      get {
        if (this._rptPrms == null)
          this._rptPrms = new CParams();
        return this._rptPrms;
      } 
      set {
        this._rptPrms = value;
      } 
    }
    private String _dataFactoryTypeName = null;
    public String dataFactoryTypeName {
      get {
        return String.IsNullOrEmpty(this._dataFactoryTypeName) ? csDefaultDataFactoryTypeName : this._dataFactoryTypeName;
      }
      set {
        this._dataFactoryTypeName = value;
      }
    }

    public String filenameFmt { get; set; }
    //public CXLRDataDefs dss { get; set; }

    public String connStr { get; set; }
    public IDBSession dbSession { get; set; }
    public Boolean debug { get; set; }

    public CXLReportConfigExtAttributes extAttrs { get; private set; }
    public CXLReportDSConfigs _dss = null;
    public CXLReportDSConfigs dss { 
      get {
        if (this._dss == null)
          this._dss = new CXLReportDSConfigs(this);
        return this._dss;
      }
      set {
        this._dss = value;
        this._dss.owner = this;
      }
    }

    public CXLReportConfig() {
      this.extAttrs = new CXLReportConfigExtAttributes();
    }

    public Boolean dbConnEnabled { get { return (this.dbSession != null) || (!String.IsNullOrEmpty(this.connStr)); } }


    private String _templateFileName = null;
    private String _templateExt = null;
    internal void detectTemplateFileName() {
      this._templateFileName = null;
      if (this.templateAdv != null) {
        this._templateFileName = this.templateAdv;
        this._templateExt = Path.GetExtension(this.templateAdv);
      } else {
        String vRptFullPath = null;
        String vRptShrtCode = null;
        String vRptThrowCode = null;
        CXLReportConfig.detectRptAttrsByCode(this.extAttrs.rootTreePath, this.fullCode, ref vRptFullPath, ref vRptShrtCode, ref vRptThrowCode);
        if (!String.IsNullOrEmpty(vRptFullPath)) {
          this._templateFileName = vRptFullPath.Replace(".xml", "");
        } else
          this._templateFileName = this.extAttrs.localPath + this.extAttrs.shortCode + "(rpt)";
        if (File.Exists(this._templateFileName + ".xls")) {
          this._templateExt = ".xls";
        } else if (File.Exists(this._templateFileName + ".xlsx")) {
          this._templateExt = ".xlsx";
        } else if (File.Exists(this._templateFileName + ".xlsm")) {
          this._templateExt = ".xlsm";
        }
        this._templateFileName = this._templateFileName + this._templateExt;
      }
      this._templateFileName = Path.GetFullPath(this._templateFileName);
      if (!File.Exists(this._templateFileName)) {
        throw new Exception("Не найден файл шаблона!");
      }
    }

    public String TemplateFileName {
      get {
        return this._templateFileName;
      }
    }

    public String TemplateFileExt {
      get {
        return this._templateExt;
      }
    }

    private static String csRptThrowCodeDetector = "^\\d+[_]";
    private static String extractThrowCodeOfNode(String nodeName) {
      String vCdStr = Utl.regexFind(nodeName, csRptThrowCodeDetector, true);
      if (!String.IsNullOrEmpty(vCdStr)) {
        return vCdStr.Substring(0, vCdStr.IndexOf('_'));
      }
      return null;
    }

    public static String extractFileRptCode(String pCode) {
      String[] vCodes = Utl.SplitString(pCode, '.');
      String vRslt = null;
      for (int i = 0; i < vCodes.Length; i++) {
        String vCd = vCodes[i].Substring(vCodes[i].IndexOf("_") + 1).ToLower();
        if (vRslt == null)
          vRslt = vCd;
        else
          vRslt = vRslt + "." + vCd;
      }
      return vRslt;
    }

    public static String extractShortRptCode(String pCode) {
      String[] vCodes = Utl.SplitString(pCode, '.');
      String vRslt = null;
      if (vCodes.Length > 0)
        vRslt = vCodes[vCodes.Length - 1];
      return vRslt;
    }

    private static String _fileName2ShortCode(String rptFileName) {
      String shrtCode = Path.GetFileNameWithoutExtension(rptFileName);
      Utl.regexReplace(ref shrtCode, csRptThrowCodeDetector, "", true);
      shrtCode = Path.GetFileNameWithoutExtension(shrtCode).Replace("(rpt)", "");
      return shrtCode;
    }

    private static void _findRptPath(String[] nodes, int level, ref String descFilePath, ref String shrtCode, ref String throwCode) {
      DirectoryInfo dirInfo = new DirectoryInfo(descFilePath);
      if (level < nodes.Length - 1) {
        DirectoryInfo[] dirs = dirInfo.GetDirectories();
        foreach (DirectoryInfo dir in dirs) {
          String vStr = csRptThrowCodeDetector + nodes[level];
          if (Utl.regexMatch(dir.Name, vStr, true)) {
            descFilePath = dir.FullName;
            String vCdStr = extractThrowCodeOfNode(dir.Name);
            if (!String.IsNullOrEmpty(vCdStr))
              Utl.AppendStr(ref throwCode, vCdStr, ".");
            _findRptPath(nodes, level + 1, ref descFilePath, ref shrtCode, ref throwCode);
          }
        }
      } else {
        FileInfo[] fls = dirInfo.GetFiles();
        foreach (FileInfo fl in fls) {
          String vStr = csRptThrowCodeDetector + nodes[level] + "[(]rpt[)].xml";
          if (Utl.regexMatch(fl.Name, vStr, true)) {
            descFilePath = fl.FullName;
            shrtCode = fl.Name;
            Utl.regexReplace(ref shrtCode, csRptThrowCodeDetector, "", true);
            String vCdStr = extractThrowCodeOfNode(fl.Name);
            if (!String.IsNullOrEmpty(vCdStr))
              Utl.AppendStr(ref throwCode, vCdStr, ".");
            shrtCode = Path.GetFileNameWithoutExtension(shrtCode).Replace("(rpt)", "");
          }
        }
      }
    }

    public static void detectRptAttrsByCode(String rootRptTreePath, String rptCode, ref String descFilePath, ref String shrtCode, ref String throwCode) {
      descFilePath = null;
      shrtCode = null;
      throwCode = null;
      String[] vNodes = Utl.SplitString(rptCode, '.');
      if (vNodes.Length > 0) {
        descFilePath = rootRptTreePath;
        _findRptPath(vNodes, 0, ref descFilePath, ref shrtCode, ref throwCode);
      }
    }

    public static String detectRptDefFileName(String rootRptTreePath, String rptCode) {
      String vRptFullPath = null;
      String vRptShrtCode = null;
      String vRptThrowCode = null;
      detectRptAttrsByCode(rootRptTreePath, rptCode, ref vRptFullPath, ref vRptShrtCode, ref vRptThrowCode);
      return vRptFullPath;
    }

    /// <summary>
    /// Восстанавливает CXLReportConfig из xml-строки
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static CXLReportConfig Decode(String xml) {
      var rslt = new CXLReportConfig();
      var doc = new XmlDocument();
      doc.InnerXml = xml;
      String vUID = null;
      if (doc.DocumentElement != null && doc.DocumentElement.HasAttribute("uid"))
        vUID = doc.DocumentElement.GetAttribute("uid");
      rslt.uid = (String.IsNullOrEmpty(vUID)) ? Guid.NewGuid().ToString().Replace("-", null).ToUpper() : vUID;

      if (doc.DocumentElement != null && doc.DocumentElement.HasAttribute("defaultTableFactory"))
        rslt.dataFactoryTypeName = doc.DocumentElement.GetAttribute("defaultTableFactory");
      else
        rslt.dataFactoryTypeName = "Bio.Helpers.XLFRpt2.DataFactory.CDataFactory, Bio.Helpers.XLFRpt2.DefaultDataFactory";

      if (doc.DocumentElement != null) {
        var selectSingleNode = doc.DocumentElement.SelectSingleNode("adv_template");
        if (selectSingleNode != null) rslt.templateAdv = selectSingleNode.InnerText;
        rslt.filenameFmt = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("filename_fmt"));

        var singleNode = doc.DocumentElement.SelectSingleNode("passwords/pwd_open");
        if (singleNode != null) rslt.extAttrs.pwdOpen = singleNode.InnerText;
        var xmlNode = doc.DocumentElement.SelectSingleNode("passwords/pwd_write");
        if (xmlNode != null) rslt.extAttrs.pwdWrite = xmlNode.InnerText;
      }

      if (String.IsNullOrEmpty(rslt.extAttrs.pwdOpen))
        rslt.extAttrs.pwdOpen = null;
      if (String.IsNullOrEmpty(rslt.extAttrs.pwdWrite))
        rslt.extAttrs.pwdWrite = null;
      
      if (doc.DocumentElement != null && doc.DocumentElement.SelectSingleNode("append") != null) {
        rslt.extAttrs.rootTreePath = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("append/rptRootTree"));
        rslt.extAttrs.workPath = Utl.ResolvePath(Xml.getInnerText(doc.DocumentElement.SelectSingleNode("append/rptWorkPath")));
        
        if (String.IsNullOrEmpty(rslt.extAttrs.rootTreePath))
          rslt.extAttrs.rootTreePath = rslt.extAttrs.workPath;
      }

      rslt.fullCode = doc.DocumentElement.GetAttribute("full_code");
      String vRptDefFileName = null;
      String vRptShortCode = null;
      String vRptThrowCode = null;
      detectRptAttrsByCode(rslt.extAttrs.rootTreePath, rslt.fullCode, ref vRptDefFileName, ref vRptShortCode, ref vRptThrowCode);
      if (vRptShortCode == null)
        vRptShortCode = rslt.fullCode;
      rslt.extAttrs.defFileName = vRptDefFileName;
      rslt.extAttrs.localPath = Path.GetDirectoryName(rslt.extAttrs.defFileName);
      rslt.extAttrs.shortCode = vRptShortCode;
      rslt.extAttrs.throwCode = vRptThrowCode;
      if (doc.DocumentElement.HasAttribute("liveScripts") && doc.DocumentElement.GetAttribute("liveScripts").Equals("true"))
        rslt.extAttrs.liveScripts = true;
      rslt.extAttrs.roles = doc.DocumentElement.GetAttribute("roles");
      //this.FIcon = this.FXMLDoc.DocumentElement.SelectSingleNode("icon").InnerText;
      //this.FHref = this.FXMLDoc.DocumentElement.SelectSingleNode("href").InnerText;
      rslt.title = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("title"));
      rslt.subject = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("subject"));
      rslt.autor = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("autor"));

      var vDBConn = (XmlElement)doc.DocumentElement.SelectSingleNode("connstr");
      if (vDBConn != null) 
        rslt.connStr = vDBConn.InnerText;

      rslt.extAttrs.sessionID = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("append/sessionID"));
      rslt.extAttrs.userUID = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("append/userName"));
      rslt.extAttrs.remoteIP = Xml.getInnerText(doc.DocumentElement.SelectSingleNode("append/remoteIP"));

      //rslt.extAttrs.logFileName = null;
      if (doc.DocumentElement.HasAttribute("debug") && doc.DocumentElement.GetAttribute("debug").Equals("true")) {
        rslt.debug = true;
      }

      var vDSDefs = doc.DocumentElement.SelectNodes("dss/ds[not(@enabled) or @enabled='true']");
      if (vDSDefs != null) {
        for (var i = 0; i < vDSDefs.Count; i++) {
          CXLReportDSConfig dsCfg = CXLReportDSConfig.Decode((XmlElement) vDSDefs[i], rslt.extAttrs.localPath);
          rslt.dss.Add(dsCfg);
        }
      }


      var inParams = doc.DocumentElement.SelectNodes("append/inParams/param");
      rslt.inPrms = new CParams();
      for (int i = 0; i < inParams.Count; i++)
        rslt.inPrms.Add(((XmlElement)inParams[i]).GetAttribute("name"), ((XmlElement)inParams[i]).InnerText);


      var rptParams = doc.DocumentElement.SelectNodes("params/param");
      rslt.rptPrms = new CParams();
      for (var i = 0; i < rptParams.Count; i++) {
        var vType = "sql";
        if (((XmlElement)rptParams[i]).HasAttribute("type"))
          vType = ((XmlElement)rptParams[i]).GetAttribute("type");
        rslt.rptPrms.Add(((XmlElement)rptParams[i]).GetAttribute("name"), ((XmlElement)rptParams[i]).InnerText, vType);
      }

      rslt.extAttrs.macroBefore = new CXLRMacro((XmlElement)doc.DocumentElement.SelectSingleNode("macroBefore"));
      rslt.extAttrs.macroAfter = new CXLRMacro((XmlElement)doc.DocumentElement.SelectSingleNode("macroAfter"));

      rslt.extAttrs.sqlScriptBefore = null;
      XmlElement vSQLScriptBeforeNode = (XmlElement)doc.DocumentElement.SelectSingleNode("sqlScriptBefore");
      if ((vSQLScriptBeforeNode != null) && Xml.getAttribute<Boolean>(vSQLScriptBeforeNode, "enabled", true)) {
        rslt.extAttrs.sqlScriptBefore = vSQLScriptBeforeNode.InnerText;
        String vSQL = rslt.extAttrs.sqlScriptBefore;
        Utl.TryLoadMappedFiles(rslt.extAttrs.localPath, ref vSQL);
        rslt.extAttrs.sqlScriptBefore = vSQL;
      }
      rslt.extAttrs.sqlScriptAfter = null;
      XmlElement vSQLScriptAfterNode = (XmlElement)doc.DocumentElement.SelectSingleNode("sqlScriptAfter");
      if ((vSQLScriptAfterNode != null) && Xml.getAttribute<Boolean>(vSQLScriptAfterNode, "enabled", true)) {
        rslt.extAttrs.sqlScriptAfter = vSQLScriptAfterNode.InnerText;
        String vSQL = rslt.extAttrs.sqlScriptAfter;
        Utl.TryLoadMappedFiles(rslt.extAttrs.localPath, ref vSQL);
        rslt.extAttrs.sqlScriptAfter = vSQL;
      }

      if (File.Exists(rslt.extAttrs.localPath + rslt.extAttrs.sqlScriptBefore)){
        String vLine = null;
        CStrFile.LoadStringFromFile(rslt.extAttrs.localPath + rslt.extAttrs.sqlScriptBefore, ref vLine, null);
        rslt.extAttrs.sqlScriptBefore = vLine;
      }
      if (File.Exists(rslt.extAttrs.localPath + rslt.extAttrs.sqlScriptAfter)) {
        String vLine = null;
        CStrFile.LoadStringFromFile(rslt.extAttrs.localPath + rslt.extAttrs.sqlScriptAfter, ref vLine, null);
        rslt.extAttrs.sqlScriptAfter = vLine;
      }
      return rslt;
    }

    public String logPath {
      get {
        //return this._rptCfg.logPath;
        String vRslt = this.extAttrs.workPath +
          (!String.IsNullOrEmpty(this.fullCode) ? Utl.GenBioLocalPath(this.fullCode) : null) +
          "log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" +
          (!String.IsNullOrEmpty(this.extAttrs.remoteIP) ? this.extAttrs.remoteIP + "_" : null) +
          (!String.IsNullOrEmpty(this.extAttrs.userUID) ? this.extAttrs.userUID : null);
        vRslt = Utl.NormalizeDir(vRslt);
        if (!Directory.Exists(vRslt))
          Directory.CreateDirectory(vRslt);
        return vRslt;
      }
    }

    public String tmpPath {
      get {
        return this.extAttrs.workPath +
          (!String.IsNullOrEmpty(this.fullCode) ? Utl.GenBioLocalPath(this.fullCode) : null) +
          "tmp\\" +
          (!String.IsNullOrEmpty(this.extAttrs.userUID) ? this.extAttrs.sessionID + "\\" : null);
      }
    }

    public String donePath {
      get {
        return this.extAttrs.workPath +
          (!String.IsNullOrEmpty(this.fullCode) ? Utl.GenBioLocalPath(this.fullCode) : null) +
          "done\\" +
          (!String.IsNullOrEmpty(this.extAttrs.userUID) ? this.extAttrs.userUID + "\\" : null);

      }
    }

    /// <summary>
    /// Загружает описание отчета из файла по коду отчета и подготавливает его к запуску
    /// </summary>
    /// <param name="rptUID">Уникальный идентификатор экземпляра отчета, если null, тогда генерится автоматом</param>
    /// <param name="rptCode">Код отчета</param>
    /// <param name="rootRptTreePath">Корневой каталог дерева отчетов</param>
    /// <param name="rootRptWorkPath">Корневой каталог дерева логов</param>
    /// <param name="connStr">Строка соединения с БД, если указана внутри описания отчета, то можно здесь не указывать</param>
    /// <param name="sessionID">ID сессии, которая запустила отчет. Можно указывать null.</param>
    /// <param name="userName">Имя пользователя, который запустил отчет. Можно указывать null.</param>
    /// <param name="remoteIP">IP адрес скоторого запустили отчет. Можно указывать null.</param>
    /// <param name="inParams">Параметры отчета. Можно указывать null.</param>
    /// <returns></returns>
    public static CXLReportConfig LoadFromFile(
      String rptUID,
      String rptCode,
      String rootRptTreePath,
      String rootRptWorkPath,
      Object conn,
      String sessionID,
      String userName,
      String remoteIP,
      CParams inParams,
      Boolean switchOffDebuging
    ) {
      String v_connStr = null;
      IDBSession v_conn = null;
      if (conn is IDBSession)
        v_conn = conn as IDBSession;
      else if (conn is String)
        v_connStr = conn as String;
      String vFullRptCode = rptCode;
      String vRptFullPath = CXLReportConfig.detectRptDefFileName(rootRptTreePath, vFullRptCode);
      XmlElement rptDocument = dom4cs.OpenDocument(vRptFullPath).XmlDoc.DocumentElement;
      //String vRptCode = Path.GetFileNameWithoutExtension(Path.GetFileName(vRptFullPath)).Replace("(rpt)", "");
      if (!String.IsNullOrEmpty(rptUID))
        rptDocument.SetAttribute("uid", rptUID);
      //rptDocument.SetAttribute("code", vRptShrtCode);
      rptDocument.SetAttribute("full_code", vFullRptCode);
      if (switchOffDebuging)
        rptDocument.SetAttribute("debug", "false");
      //rptDocument.SetAttribute("throw_code", vRptThrowCode);
      //String vRptPath = Utl.NormalizeDir(Path.GetDirectoryName(vRptFullPath));
      //String vRptFullPath_ws = Utl.NormalizeDir(rootRptLogsPath) + Utl.genIOLocalPath(vFullRptCode);
      //String vRptDonePath = Utl.NormalizeDir(Path.GetDirectoryName(vRptFullPath_ws)) + "done\\";
      //String vRptTmpPath = Utl.NormalizeDir(Path.GetDirectoryName(vRptFullPath_ws)) + "tmp\\";
      //String vRptLogPath = Utl.NormalizeDir(Path.GetDirectoryName(vRptFullPath_ws)) + "log\\";

      XmlElement connstrElement = rptDocument.SelectSingleNode("connstr") as XmlElement;
      if ((connstrElement == null) && (!String.IsNullOrEmpty(v_connStr))) {
        connstrElement = rptDocument.OwnerDocument.CreateElement("connstr");
        connstrElement.AppendChild(connstrElement.OwnerDocument.CreateCDataSection(v_connStr));
        rptDocument.AppendChild(connstrElement);
      }

      StringWriter vApndXML = new StringWriter();

      vApndXML.WriteLine("<sessionID>" + sessionID + "</sessionID>");
      vApndXML.WriteLine("<userName>" + userName + "</userName>");
      vApndXML.WriteLine("<remoteIP>" + remoteIP + "</remoteIP>");
      vApndXML.WriteLine("<rptRootTree>" + rootRptTreePath + "</rptRootTree>");
      //vApndXML.WriteLine("<rptDefPath>" + vRptFullPath + "</rptDefPath>");
      //vApndXML.WriteLine("<donePath>" + vRptDonePath + "</donePath>");
      //vApndXML.WriteLine("<tmpPath>" + vRptTmpPath + "</tmpPath>");
      vApndXML.WriteLine("<rptWorkPath>" + rootRptWorkPath + "</rptWorkPath>");
      //if (rptParams != null) {
      //  vApndXML.WriteLine("<inParams>");
      //  foreach (CParam vPrm in rptParams)
      //    vApndXML.WriteLine("<param name=\"" + vPrm.Name + "\">" + vPrm.Value + "</param>");
      //  vApndXML.WriteLine("</inParams>");
      //}

      XmlElement vAppndElem = (XmlElement)rptDocument.SelectSingleNode("append");
      if (vAppndElem == null) {
        vAppndElem = rptDocument.OwnerDocument.CreateElement("append");
        rptDocument.AppendChild(vAppndElem);
      }
      vAppndElem.InnerXml = vApndXML.ToString();
      CXLReportConfig v_rslt = CXLReportConfig.Decode(rptDocument.OuterXml);
      if(v_conn != null)
        v_rslt.dbSession = v_conn;
      v_rslt.inPrms = inParams;
      return v_rslt;
    }

  }

  public class CXLReportDSConfigs : List<CXLReportDSConfig> {
    private CXLReportConfig _owner = null;
    public CXLReportDSConfigs() : base() { }
    public CXLReportDSConfigs(CXLReportConfig owner):base(){
      this._owner = owner;
    }
    public new void Add(CXLReportDSConfig item){
      base.Add(item);
      item.owner = this._owner;
    }
    public CXLReportConfig owner {
      get { return this._owner; }
      set {
        this._owner = value;
        foreach (var item in this)
          item.owner = this._owner;
      }
    }

  }

  public class CXLReportDSFieldDef {
    public String name { get; set; }
    public String header { get; set; }
    public String format { get; set; }
    public CJSAlignment align { get; set; }
    public Int32 width { get; set; }
    public CFieldType type { get; set; }
  }
}
