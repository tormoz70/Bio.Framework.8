namespace Bio.Framework.Server {
  using System;
  using System.IO;
  using System.Web;
  using System.Xml;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Collections;
  using System.Reflection;

  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  public class CIObject {
    private BioSession _ownerSession;
    //private String FExporter;
    private String _locPath;
    private dom4cs _iniDoc;

    public BioSession OwnerSession { get { return this._ownerSession; } }
    public String bioCode { get; private set; }

    public static CIObject CreateIObject(String bioCode, BioSession ownerSession) {
      return CreateIObject(bioCode, ownerSession, false);
    }

    public static CIObject CreateIObject(String bioCode, BioSession ownerSession, bool suppressErrors) {
      CIObject vResult = null;
      String vIniFileName = SrvUtl.bldiniFileName(ownerSession.Cfg.IniPath, bioCode);
      if(File.Exists(vIniFileName)) {
        vResult = ownerSession.IObj_get(bioCode);
        if(vResult == null) {
          vResult = new CIObject(bioCode, ownerSession);
        }
        //TIObject vResult = new TIObject(pIOCode, pIOUID, pOwnerSession);
        try {
          vResult.initIO(ownerSession.Cfg.DoReloadIOOnInit);
        } catch {
          if(!suppressErrors)
            throw;
          else
            vResult = null;
        }
      }
      return vResult;
    }

    public CIObject(String bioCode, BioSession ownerSession) {
      this._ownerSession = ownerSession;
      this.bioCode = bioCode;
      this._ownerSession.IObj_set(this.bioCode, this);
      this._locPath = Utl.GenBioLocalPath(this.bioCode);
      if(this._locPath.Length > 0)
        this._locPath = this.OwnerSession.Cfg.IniPath + this._locPath;
    }


    private void processStore(XmlElement store) {
      if(store != null) {
        XmlNodeList vSQLs = store.SelectNodes("//SQL");
        foreach(XmlElement vSQL in vSQLs) {
          XmlElement vTextNode = (XmlElement)vSQL.SelectSingleNode("text");
          if(vTextNode != null) {
            String vText = vTextNode.InnerText;
            String vCurrentIOPath = Path.GetDirectoryName(this._iniDoc.FileName);
            Utl.TryLoadMappedFiles(vCurrentIOPath, ref vText);
            vTextNode.RemoveAll();
            vTextNode.AppendChild(vTextNode.OwnerDocument.CreateCDataSection(vText));
          }
        }
      }
    }


    private void initIO(bool reload) {
      /*Загрузка, Наследование, Доступ*/
      if(this._iniDoc == null) {
        this._iniDoc = this.openIni(this.bioCode);
        if((this._iniDoc != null) && (this.OwnerSession.Cfg.EnableDocInhirance))
          this._iniDoc = this.processInhirits(_iniDoc);
      } else if(reload) {
        this._iniDoc.ReopenDocument();
        if(this.OwnerSession.Cfg.EnableDocInhirance)
          this._iniDoc = this.processInhirits(_iniDoc);
      }
      XmlNode vStore = this._iniDoc.XmlDoc.DocumentElement;
      this.processStore((XmlElement)vStore);

      String vLocIOIniFN = this.OwnerSession.Cfg.LocalIOCfgPath +
                           this.OwnerSession.CurSessionRemoteIP + "-" +
                           this.bioCode + "-" +
                           this.OwnerSession.Cfg.CurUser.Login + ".xml";
      if(File.Exists(vLocIOIniFN)) {
        dom4cs vLocIOIniDoc = dom4cs.OpenDocument(vLocIOIniFN);
        XmlElement vLocIniSrc = vLocIOIniDoc.XmlDoc.DocumentElement;
        XmlElement vLocIniDst = this._iniDoc.XmlDoc.CreateElement("local_ini");
        this._iniDoc.XmlDoc.DocumentElement.AppendChild(vLocIniDst);
        dom4cs.CopyAttrs(vLocIniSrc, vLocIniDst);
        vLocIniDst.InnerXml = vLocIniSrc.InnerXml;
        XmlNode vSaveToFNNode = vLocIniDst.SelectSingleNode("saveto");
        if(vSaveToFNNode == null) {
          vSaveToFNNode = this._iniDoc.XmlDoc.CreateElement("saveto");
          vLocIniDst.AppendChild(vSaveToFNNode);
        }
        vSaveToFNNode.InnerText = vLocIOIniFN;
      }
    }

    private dom4cs processInhirits(dom4cs iniDoc) {
      dom4cs parentDoc = null;
      if(iniDoc != null) {
        String parentDocName = ((XmlElement)iniDoc.XmlDoc.DocumentElement).GetAttribute("extends");
        if(!parentDocName.Equals("")) {
          parentDoc = this.openIni(parentDocName);
          if(parentDoc != null) {
            parentDoc.MergeDocument(iniDoc, this.OwnerSession.Cfg.IniPath + SrvConst.csMIFileName);
            parentDoc = processInhirits(parentDoc);
            return parentDoc;
          } else
            return iniDoc;
        } else
          return iniDoc;
      } else
        return iniDoc;
    }

    private dom4cs openIni(String bioCode) {
      return dom4cs.OpenDocument(SrvUtl.bldiniFileName(this.OwnerSession.Cfg.IniPath, bioCode));
    }

    private String buildIOUrlFromIOCode(String bioCode, String ext) {
      return SrvUtl.bldIOFileUrl(this.OwnerSession.Cfg.AppURL, bioCode, ext);
    }

    private String getParentIOUrls(String bioCode, String ext) {
      String fLocIOPath = bioCode.Replace(".", "\\");
      dom4cs vDoc = dom4cs.OpenDocument(this.OwnerSession.Cfg.IniPath + fLocIOPath + ".xml");
      String vParentIOCode = vDoc.XmlDoc.DocumentElement.GetAttribute("extends");
      if(vParentIOCode.Equals(""))
        return buildIOUrlFromIOCode(bioCode, ext);
      else {
        String vCPath = buildIOUrlFromIOCode(bioCode, ext);
        if(vCPath != null)
          return getParentIOUrls(vParentIOCode, ext) + "|" + vCPath;
        else
          return getParentIOUrls(vParentIOCode, ext);
      }
    }

    public String ioUrl {
      get {
        return SrvUtl.bldIOPathUrl(this.OwnerSession.Cfg.AppURL, this.bioCode);
      }
    }

    public String ioTemplate2XL {
      get {
        String fLocIOPath = this.OwnerSession.Cfg.IniPath + this.bioCode.Replace(".", "\\");
        if (File.Exists(fLocIOPath + ".xls"))
          return fLocIOPath + ".xls";
        else if (File.Exists(fLocIOPath + ".xlsx"))
          return fLocIOPath + ".xlsx";
        else if (File.Exists(fLocIOPath + ".xlsm"))
          return fLocIOPath + ".xlsm";
        else
          return null;
      }
    }

    public String[] ioScriptUrls {
      get {
        String vParentIOStriptUrls = getParentIOUrls(this.bioCode, ".js");
        return Utl.SplitString(vParentIOStriptUrls, '|');
      }
    }

    public String[] ioCSSUrls {
      get {
        String vParentIOCSSUrls = getParentIOUrls(this.bioCode, ".css");
        return Utl.SplitString(vParentIOCSSUrls, '|');
      }
    }

    public virtual dom4cs IniDocument {
      get {
        return this._iniDoc;
      }
    }

    public String LocalPath {
      get {
        return this._locPath;
      }
    }


  }
}