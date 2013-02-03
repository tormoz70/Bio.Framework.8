namespace Bio.Helpers.XLFRpt2.Engine {
	using System;
	using System.Xml;
  using System.IO;
	using System.Web;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  class CXLFolderNode {
    public String code { get; set; }
    public String roles { get; set; }
    public String throw_code { get; set; }
    public String title { get; set; }
    public String subject {get; set;}
  }

  class CXLRptNode {
    public String code { get; set; }
    public String roles { get; set; }
    public String throw_code {get; set;}
    public String title {get; set;}
    public String subject {get; set;}
  }

  public class CXLRptTreeNav {

    private static String extractThrowCode(String folderOrRptName){
      String vName = folderOrRptName;
      if (File.Exists(folderOrRptName))
        vName = Path.GetFileNameWithoutExtension(folderOrRptName);
      else if (Directory.Exists(folderOrRptName))
        vName = new DirectoryInfo(folderOrRptName).Name;
      if (!String.IsNullOrEmpty(vName) && (vName.IndexOf("_") >= 0))
        return vName.Substring(0, vName.IndexOf("_"));
      else
        return null;
    }

    private static String extractSCode(String folderOrRptName) {
      String vName = folderOrRptName;
      if (File.Exists(folderOrRptName))
        vName = Path.GetFileNameWithoutExtension(folderOrRptName);
      else if (Directory.Exists(folderOrRptName))
        vName = new DirectoryInfo(folderOrRptName).Name;
      String vNameOK = Utl.RegexFind(vName, "\\d+[_]\\w+", true);
      if (!String.IsNullOrEmpty(vNameOK)) {
        String vSCode = vNameOK.Substring(vNameOK.IndexOf("_") + 1);
        vSCode = vSCode.Replace("(rpt).xml", "");
        return vSCode;
      } else
        return null;
    }

    private static String extractParentFullCode(String pCode) {
      String[] vCodes = Utl.SplitString(pCode, '.');
      String vRslt = null;
      for (int i = 0; i < vCodes.Length - 1; i++) {
        if (vRslt == null)
          vRslt = vCodes[i];
        else
          vRslt = vRslt + "." + vCodes[i];
      }
      return vRslt;
    }


    private static String scanPhisicalPath(String rootPath, String rptFolderCode, Action<DirectoryInfo> act) {
      String[] folders = Utl.SplitString(rptFolderCode, '.');
      String vPath = rootPath;
      foreach (String cFld in folders) {
        DirectoryInfo[] phDi = new DirectoryInfo(vPath).GetDirectories();
        Boolean vFound = false;
        foreach (DirectoryInfo di in phDi) {
          if (di.Name.ToUpper().EndsWith(cFld.ToUpper())) {
            if (act != null)
              act(di);
            vPath = vPath + di.Name + "\\";
            vFound = true;
            break;
          }
        }
        if (!vFound)
          throw new EBioException("Путь \"" + rptFolderCode + "\" не найден в корне \"" + rootPath + "\"");
      }
      return vPath;
    }

    private static String findPhisicalPath(String rootPath, String rptFolderCode) {
      return scanPhisicalPath(rootPath, rptFolderCode, null);
    }


    private static void findTitlePath(String rootPath, String rptFolderCode, String userRoles, ref String fullTitle, ref String fullThrowCode) {
      String vFullTitle = fullTitle;
      String vFullThrowCode = fullThrowCode;
      scanPhisicalPath(rootPath, rptFolderCode, new Action<DirectoryInfo>((di) => {
        CXLFolderNode fldAttr = getFolderAttrs(di.FullName, userRoles);
        if (fldAttr != null) {
          Utl.AppendStr(ref vFullTitle, fldAttr.title, "/");
          String thrCode = extractThrowCode(di.Name);
          Utl.AppendStr(ref vFullThrowCode, thrCode, ".");
        }
      }));
      fullTitle = vFullTitle;
      fullThrowCode = vFullThrowCode;

    }

    private static CXLRptNode getRptAttrs(String rptFileName, String userRoles){
      CXLRptNode rslt = new CXLRptNode();
      if(File.Exists(rptFileName)){
        XmlDocument vRptDoc = new XmlDocument();
        try {
          vRptDoc.Load(rptFileName);
        } catch (Exception ex) {
          throw new Exception("Ошибка при загрузке описания отчета. Путь: " + rptFileName, ex);
        }
        String vRoles = vRptDoc.DocumentElement.GetAttribute("roles");
        if (String.IsNullOrEmpty(vRoles))
          vRoles = userRoles;
        if (checkStrRoles(vRoles, userRoles)) {
          rslt.code = extractSCode(rptFileName);
          rslt.title = vRptDoc.DocumentElement.SelectSingleNode("title").InnerText;
          rslt.subject = vRptDoc.DocumentElement.SelectSingleNode("subject").InnerText;
          rslt.throw_code = null;
          if (vRptDoc.DocumentElement.HasAttribute("throw_code"))
            rslt.throw_code = vRptDoc.DocumentElement.GetAttribute("throw_code");
          else
            rslt.throw_code = extractThrowCode(rptFileName);

          rslt.roles = vRoles;
          //if (vRptDoc.DocumentElement.HasAttribute("roles"))
          //  rslt.roles = vRptDoc.DocumentElement.GetAttribute("roles");
        } else
          rslt = null;
      }else 
        rslt.title = "{Не найден файл описания отчета. "+rptFileName+"}";
      return rslt;
    }

    private static CXLFolderNode getFolderAttrs(String folderPath, String userRoles) {
      String vLoadPath = Utl.NormalizeDir(folderPath) + "_rpt_node.xml";
      if (File.Exists(vLoadPath)) {
        XmlDocument vNodeDoc = new XmlDocument();
        try {
          vNodeDoc.Load(vLoadPath);
        } catch (Exception ex) {
          throw new Exception("Ошибка при загрузке элемента дерева отчетов. Путь: " + vLoadPath, ex);
        }
        String vRoles = vNodeDoc.DocumentElement.GetAttribute("roles");
        if (String.IsNullOrEmpty(vRoles))
          vRoles = userRoles;
        if (checkStrRoles(vRoles, userRoles)) {
          CXLFolderNode rslt = new CXLFolderNode();
          rslt.code = extractSCode(folderPath);
          rslt.throw_code = extractThrowCode(folderPath);
          rslt.title = vNodeDoc.DocumentElement.GetAttribute("title");
          XmlElement vNode = vNodeDoc.DocumentElement.SelectSingleNode("subject") as XmlElement;
          if (vNode != null)
            rslt.subject = vNode.InnerText;
          else
            rslt.subject = "[папка]";
          rslt.roles = vRoles;
          return rslt;
        } else
          return null;

      } else
        return null;
    }

    private static bool checkStrRoles(String nodeRoles, String userRoles) {
      String vNodeRoles = nodeRoles;
      if(String.IsNullOrEmpty(vNodeRoles))
        vNodeRoles = userRoles;
      char[] spr = new char[] { ' ', ';' };
      Boolean usrIsAdmin = Utl.DelimitedLineHasCommonTags("admin", userRoles, spr);
      if (!usrIsAdmin) {
        if (Utl.CheckRoles(nodeRoles, userRoles, spr)) {
          return true;
        } else {
          return false;
        }
      } else
        return true;

    }

    private static void loadRptFolders(String curPath, String fldrCode, XmlDocument doc, String userRoles) {
      DirectoryInfo[] vDirs = new DirectoryInfo(curPath).GetDirectories();
      XmlElement vBackNode = doc.CreateElement("rpt_group");
      if ((curPath != null) && (!curPath.Equals(""))) {

        //String vParentFullCode = extractParentFullCode(fldrCode);
        //String vBackRptCode = "back_door";
        //if (vParentFullCode != null)
        //  vBackRptCode = vParentFullCode + "." + vBackRptCode;
        vBackNode.SetAttribute("code", "back_door");
        vBackNode.SetAttribute("title", "..");
        XmlElement vSubjectElem = (XmlElement)vBackNode.AppendChild(doc.CreateElement("subject"));
        vSubjectElem.AppendChild(doc.CreateCDataSection("[назад]"));
        vBackNode.AppendChild(vSubjectElem);
        doc.DocumentElement.AppendChild(vBackNode);
      }
      foreach (DirectoryInfo di in vDirs) {
        //String vSCode = extractSCode(di.Name);
        //if (!String.IsNullOrEmpty(vSCode)) {

        CXLFolderNode fldAttrs = getFolderAttrs(di.FullName, userRoles);
        if (fldAttrs != null) {

          XmlElement vNode = doc.CreateElement("rpt_group");
          doc.DocumentElement.AppendChild(vNode);

          if (!String.IsNullOrEmpty(fldAttrs.throw_code))
            vNode.SetAttribute("throw_code", fldAttrs.throw_code);
          vNode.SetAttribute("code", fldAttrs.code);
          vNode.SetAttribute("title", fldAttrs.title);
          //vNode.SetAttribute("code", vFullCode);
          XmlElement vSubjectElem = (XmlElement)vNode.AppendChild(doc.CreateElement("subject"));
          vSubjectElem.AppendChild(doc.CreateCDataSection(fldAttrs.subject));
          vNode.AppendChild(vSubjectElem);

          if (!String.IsNullOrEmpty(fldAttrs.roles))
            vNode.SetAttribute("roles", fldAttrs.roles);

        }
        //}

      }
    }

    private static void loadRpts(String curPath, String fldrCode, XmlDocument doc, String userRoles) {
      DirectoryInfo di = new DirectoryInfo(curPath);
      FileInfo[] vRpts = di.GetFiles("*(rpt).xml");
      foreach(FileInfo rpt in vRpts) {

        //String vThrowCode = null;
        //String vTitle = null;
        //String vSubject = null;
        //String vRoles = userRoles;
        CXLRptNode rptAttrs = getRptAttrs(rpt.FullName, userRoles);
        if (rptAttrs != null) {

          XmlElement vNode = doc.DocumentElement;
          XmlElement vRptNode = (XmlElement)vNode.AppendChild(doc.CreateElement("report"));
          //String vSCode = extractSCode(rpt.Name);
          //String vFullCode = curPath.Replace("\\", ".") + "." + vCode;
          if (!String.IsNullOrEmpty(rptAttrs.throw_code))
            vRptNode.SetAttribute("throw_code", rptAttrs.throw_code);
          vRptNode.SetAttribute("code", rptAttrs.code);
          vRptNode.SetAttribute("title", rptAttrs.title);
          //vRptNode.SetAttribute("file_code", genRptFileCodeFromRpt(vFullCode));

          //XmlElement vIconElem = (XmlElement)vRptNode.AppendChild(vDoc.CreateElement("icon"));
          //vIconElem.AppendChild(vIconElem.OwnerDocument.CreateCDataSection("/images/rpts/report.gif"));

          //XmlElement vTitleElem = (XmlElement)vRptNode.AppendChild(vRptNode.OwnerDocument.CreateElement("title"));
          //vTitleElem.AppendChild(vTitleElem.OwnerDocument.CreateCDataSection(rptAttrs.title));
          //vRptNode.AppendChild(vTitleElem);

          XmlElement vSubjectElem = (XmlElement)vRptNode.AppendChild(vRptNode.OwnerDocument.CreateElement("subject"));
          vSubjectElem.AppendChild(vSubjectElem.OwnerDocument.CreateCDataSection(rptAttrs.subject));
          vRptNode.AppendChild(vSubjectElem);

          if (!String.IsNullOrEmpty(rptAttrs.roles))
            vRptNode.SetAttribute("roles", rptAttrs.roles);

          
        }
      }
    }



    public static XmlDocument buildRptTreeNav(String rootPath, String rptFolderCode, String userRoles) {
      String vCurFolderCode = null;
      String vCurPath = null;
      if(rptFolderCode != null) {
        Boolean goBack = rptFolderCode.ToLower().EndsWith("back_door");
        vCurPath = rptFolderCode.Replace(".back_door", "");
        vCurPath = vCurPath.Replace("back_door", "");
        if (goBack)
          vCurPath = extractParentFullCode(vCurPath);
        vCurFolderCode = vCurPath;
        vCurPath = findPhisicalPath(rootPath, vCurPath);
      }
      if(!File.Exists(vCurPath + "\\_rpt_node.xml")) {
        // Запрашиваемый путь не является папкой отчетов
        return null;
      } else {
        // Запрашиваемый путь является папкой отчетов
        XmlDocument vFldDoc = new XmlDocument();
        vFldDoc.Load(vCurPath + "_rpt_node.xml");
        XmlDocument vDoc = dom4cs.CreXmlDocument("cur_group", null, null);
        dom4cs.CopyAttrs(vFldDoc.DocumentElement, vDoc.DocumentElement);
        vDoc.DocumentElement.InnerXml = vFldDoc.DocumentElement.InnerXml;

        if(!String.IsNullOrEmpty(vCurFolderCode))
          vDoc.DocumentElement.SetAttribute("code", vCurFolderCode);
        String vTitle = null;
        String vThrowCode = null;
        if(!String.IsNullOrEmpty(vCurFolderCode))
          findTitlePath(rootPath, vCurFolderCode, userRoles, ref vTitle, ref vThrowCode);
        else
          vTitle = vFldDoc.DocumentElement.GetAttribute("title");
        vDoc.DocumentElement.SetAttribute("title", vTitle);
        if (!String.IsNullOrEmpty(vThrowCode))
          vDoc.DocumentElement.SetAttribute("throw_code", vThrowCode);
        // Считываем все папки в данной папке
        loadRptFolders(vCurPath, vCurFolderCode, vDoc, userRoles);
        // Считываем все отчеты в данной папке
        loadRpts(vCurPath, vCurFolderCode, vDoc, userRoles);
        return vDoc;
      }
    }
    //public static String GenerateRptPath(String pRptRootPath, XmlElement pRptNode){
    //  return pRptRootPath+"root\\"+pRptNode.GetAttribute("code").Replace(".", "\\");
    //}


    //private static void addCurPath(String pPath, ref String vResult) {
    //  if(File.Exists(pPath + "\\_rpt_node.xml")) {
    //    XmlDocument vDoc = new XmlDocument();
    //    vDoc.Load(pPath + "\\_rpt_node.xml");
    //    if(String.IsNullOrEmpty(vResult))
    //      vResult = vDoc.DocumentElement.GetAttribute("title");
    //    else
    //      vResult = vResult + "/" + vDoc.DocumentElement.GetAttribute("title");
    //  }
    //}

    //private static String genRptFileCodeFromRpt(String pCode) {
    //  String[] vCodes = Utl.SplitString(pCode, '.');
    //  String vRslt = null;
    //  for(int i = 0; i < vCodes.Length; i++) {
    //    String vCd = vCodes[i].Substring(vCodes[i].IndexOf("_") + 1).ToLower();
    //    if(vRslt == null)
    //      vRslt = vCd;
    //    else
    //      vRslt = vRslt + "." + vCd;
    //  }
    //  return vRslt;
    //}

	}
}