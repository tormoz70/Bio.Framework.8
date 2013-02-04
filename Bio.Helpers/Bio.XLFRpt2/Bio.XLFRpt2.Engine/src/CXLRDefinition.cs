namespace Bio.Helpers.XLFRpt2.Engine {

  using System;
  using System.IO;
  using System.Xml;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using System.Data;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// Класс атрибута определяющего маппинг на переменную в шаблоне отчета.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class ParamMappingAttribute : Attribute {
    public ParamMappingAttribute(String mappingName) {
      this.ParamMapping = mappingName;
    }
    public String ParamMapping { get; set; }
  }

  /// <summary>
  /// 
  /// </summary>
  public class CXLRDefinition : CDisposableObject {
    //private
    private const string csEcoding = "UTF-8";
    private CXLReport _owner = null;
    public static String getParamMappingOfProp(String propName) {
      return Utl.GetAttributeOfProp<CXLRDefinition, ParamMappingAttribute>(propName).ParamMapping;
    }
    //constructor
    private CXLReportConfig _rptCfg = null;
    public CXLRDefinition(CXLReport owner, CXLReportConfig rptCfg) {
      this._owner = owner;
      this._rptCfg = rptCfg;
      this._rptCfg.detectTemplateFileName();
      if (!this.InParams.ParamExists("SYS_CURUSERUID", true))
        this.InParams.Add("SYS_CURUSERUID", this._rptCfg.extAttrs.userUID);
      if (!this.InParams.ParamExists("SYS_CURUSERIP", true))
        this.InParams.Add("SYS_CURUSERIP", this._rptCfg.extAttrs.remoteIP);
      if (!this.InParams.ParamExists("SYS_CURUSERROLES", true))
        this.InParams.Add("SYS_CURUSERROLES", this._rptCfg.extAttrs.roles);
      if (!this.InParams.ParamExists("SYS_CURODEPUID", true))
        this.InParams.Add("SYS_CURODEPUID", this._rptCfg.extAttrs.userUID);
      if (!this.InParams.ParamExists("SYS_TITLE", true))
        this.InParams.Add("SYS_TITLE", this._rptCfg.title);
    }

    protected override void onDispose() {
      this._owner = null;
      this._rptCfg = null;
    }

    public CXLReportConfig RptCfg {
      get { return this._rptCfg; }
    }

    public String RptUID {
      get {
        return this._rptCfg.uid;
      }
      set {
        if (!String.IsNullOrEmpty(value) && (value.Length != 32))
          throw new EBioException("Значение RptUID может быть только строкой в 32 символа. И не может принимать значение : \""+value+"\"");
        this._rptCfg.uid = value;
      }
    }

    public String DataFactoryType { get { return this._rptCfg.dataFactoryTypeName; } }

    public Boolean DebugIsOn {
      get {
        return this._rptCfg.debug;
      }
    }
    public String LogFileName {
      get {
        return this.LogPath + this.ShortCode + ".log";
      }
    }

    public String PwdOpen {
      get {
        return this._rptCfg.extAttrs.pwdOpen;
      }
    }

    public String PwdWrite {
      get {
        return this._rptCfg.extAttrs.pwdWrite;
      }
    }

    public const String csRptDonePathParamID = "#REPORT_DONEPATH#";
    public const String csRptTmpPathParamID = "#REPORT_TMPPATH#";
    public const String csRptLogPathParamID = "#REPORT_LOGPATH#";
    
    [ParamMapping("#FULL_CODE#")]
    public String FullCode { get { return this._rptCfg.fullCode; } } 
    [ParamMapping("#SHORT_CODE#")]
    public String ShortCode { get { return this._rptCfg.extAttrs.shortCode; } }
    [ParamMapping("#THROW_CODE#")]
    public String ThrowCode { get { return this._rptCfg.extAttrs.throwCode; } } 
    [ParamMapping("#REPORT_ROLES#")]
    public String Roles { get { return this._rptCfg.extAttrs.roles; } }
    //public String Icon{get{return FIcon;}}
    //public String Href{get{return FHref;}}
    [ParamMapping("#REPORT_TITLE#")]
    public String Title { get { return this._rptCfg.title; } } 
    [ParamMapping("#REPORT_SUBJECT#")]
    public String Subject { get { return this._rptCfg.subject; } } 
    [ParamMapping("#REPORT_AUTOR#")]
    public String Autor { get { return this._rptCfg.autor; } } 
    [ParamMapping("#REPORT_DBCONNSTR#")]
    public String ConnStr { get { return this._rptCfg.connStr; } } 
    [ParamMapping("#REPORT_SESSIONID#")]
    public String SessionID { get { return this._rptCfg.extAttrs.sessionID; } } 
    [ParamMapping("#REPORT_USERUID#")]
    public String UserName { get { return this._rptCfg.extAttrs.userUID; } }
    [ParamMapping("#REPORT_REMOTEIP#")]
    public String RemoteIP { get { return this._rptCfg.extAttrs.remoteIP; } } 

    [ParamMapping("#REPORT_ROOTTREEPATH#")]
    public String RptRootTreePath { get { return this._rptCfg.extAttrs.rootTreePath; } }
    [ParamMapping("#REPORT_WORKPATH#")]
    public String RptWorkPath { get { return this._rptCfg.extAttrs.workPath; } } 
    [ParamMapping("#REPORT_DEFFILENAME#")]
    public String RptDefFileName { get { return this._rptCfg.extAttrs.defFileName; } } 
    
    [ParamMapping("#REPORT_LOCALPATH#")]
    public String RptLocalPath { get { return this._rptCfg.extAttrs.localPath; } } 

    public Boolean LiveScripts { get { return this._rptCfg.extAttrs.liveScripts; } }
    public Boolean DBConnEnabled { get { return this._rptCfg.dbConnEnabled; } }
    [ParamMapping("#REPORT_DONEPATH#")]
    public String DonePath { 
      get { 
        return this._rptCfg.donePath;
      } 
    }

    public CXLReportDSConfig dsCfgByRangeName(String rangeName) {
      return this._rptCfg._dss.Where<CXLReportDSConfig>((item) => { 
        return item.rangeName.Equals(rangeName); 
      }).FirstOrDefault();
    }

    //public CXLReportDSConfig dsCfgDefault {
    //  get {
    //    return this._rptCfg._dss.FirstOrDefault();
    //  }
    //}

    //public String tmpPath {
    //  get {
    //    return this.ext_attrs.workPath + Utl.genIOLocalPath(this.full_code) + "tmp\\" + this.ext_attrs.sessionID + "\\";
    //  }
    //}
    //public String logPath {
    //  get {
    //    String vRslt = this.ext_attrs.workPath + Utl.genIOLocalPath(this.full_code) + "log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + this.ext_attrs.remoteIP + "_" + this.ext_attrs.userName + "\\";
    //    if (!Directory.Exists(vRslt))
    //      Directory.CreateDirectory(vRslt);
    //    return vRslt;
    //  }
    //}

    [ParamMapping("#REPORT_TMPPATH#")]
    public String TmpPath { 
      get { 
        return this._rptCfg.tmpPath;
      } 
    }
    [ParamMapping("#REPORT_LOGPATH#")]
    public String LogPath { 
      get { 
        return this._rptCfg.logPath;
      } 
    }



    public CXLRMacro MacroBefore { get { return this._rptCfg.extAttrs.macroBefore; } }
    public CXLRMacro MacroAfter { get { return this._rptCfg.extAttrs.macroAfter; } }
    public String SQLScriptBefore { get { return this._rptCfg.extAttrs.sqlScriptBefore; } }
    public String SQLScriptAfter { get { return this._rptCfg.extAttrs.sqlScriptAfter; } }
    public CParams InParams { get { return this._rptCfg.inPrms; } }
    public CParams RptParams { get { return this._rptCfg.rptPrms; } }

    private String buildFileName(String pTempl) {
      String vRslt = pTempl;
      if (this.InParams != null)
        for (int i = 0; i < this.InParams.Count; i++) {
          String vVal = this.InParams[i].ValueAsString();
          if (!String.IsNullOrEmpty(vVal)) {
            vVal = vVal.Replace("false|", "");
            vVal = vVal.Replace("true|", "{N}");
          }
          if (!String.IsNullOrEmpty(vRslt)) 
            vRslt = vRslt.Replace("{$" + this.InParams[i].Name + "}", vVal);
        }
      if (!String.IsNullOrEmpty(vRslt)) {
        vRslt = vRslt.Replace("{$code}", this.ShortCode);
        vRslt = vRslt.Replace("{$now}", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
      }
      return vRslt;
    }

    //public String TemplateFileName{
    //  get{
    //    if(this.FAdvTemplate != null)
    //      return this.FAdvTemplate;
    //    else
    //      return this.RptPath+this.ShrtCode+"(rpt).xls";
    //  }
    //}
    //public String GetNewTempFileName(){
    //    if(!Directory.Exists(this.DonePath))
    //      Directory.CreateDirectory(this.DonePath);
    //  if(this.FFileNameFmt == null)
    //    return this.DonePath+this.FileCode+"."+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".xls";
    //  else
    //    return this.DonePath+buildFileName(this.FFileNameFmt);
    //}


    public String TemplateFileName {
      get {
        return this._rptCfg.TemplateFileName;
      }
    }

    public String TemplateFileExt {
      get {
        return this._rptCfg.TemplateFileExt;
      }
    }

    //private Int64? _maxRowsLimit = null;
    //public Int64 MaxRowsLimit {
    //  get {
    //    if (this.TemplateFileExt.ToLower().EndsWith("xls"))
    //      return this._maxRowsLimit ?? 65000L;
    //    else
    //      return this._maxRowsLimit ?? 900000L;
    //  }
    //  set {
    //    this._maxRowsLimit = value;
    //  }
    //}

    public String GetNewTempFileName() {
      if (!Directory.Exists(this.DonePath))
        Directory.CreateDirectory(this.DonePath);
      String vResult = null;
      if (this._rptCfg.filenameFmt == null)
        vResult = this.DonePath + this.ShortCode + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + this._rptCfg.TemplateFileExt;
      else
        vResult = this.DonePath + Path.GetFileNameWithoutExtension(buildFileName(this._rptCfg.filenameFmt)) + this._rptCfg.TemplateFileExt;
      vResult = Path.GetFullPath(vResult);
      return vResult;
    }

  }

  internal class EBioTooManyRows : EBioException {
    public EBioTooManyRows(String msg) : base(msg) { }
  }
}
