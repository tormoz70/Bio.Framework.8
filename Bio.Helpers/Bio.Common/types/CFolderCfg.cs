namespace Bio.Helpers.Common.Types {
#if !SILVERLIGHT
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Xml;
  using System.Reflection;
  
  public class CFolderCfg {
    private String FRootFolderPath = null;
    private XmlDocument FDom = null;

    public CFolderCfg(String pFolderPath) {
      this.FRootFolderPath = (pFolderPath[pFolderPath.Length - 1] == '\\') ? pFolderPath : pFolderPath + '\\';
      this.FDom = new XmlDocument();
      XmlDeclaration xd = this.FDom.CreateXmlDeclaration("1.0", "UTF-8", null);
      this.FDom.AppendChild(xd);
      XmlElement vRoot = this.FDom.CreateElement("root");
      this.FDom.AppendChild(vRoot);
    }

    private void processAssembly(XmlElement vFolderNode, String pAssemblyFileName) {
      if(File.Exists(pAssemblyFileName) && !pAssemblyFileName.ToLower().Contains(".vshost.exe")) {
        bool vIsAssembly = true;
        Assembly vAsmblyInst = null;
        AssemblyName vAsmbly = null;
        try {
          vAsmblyInst = Assembly.LoadFile(pAssemblyFileName);
          vAsmbly = vAsmblyInst.GetName();
        } catch(BadImageFormatException) {
          vIsAssembly = false;
        }
        XmlElement vNode = this.FDom.CreateElement("assembly");
        FileInfo vFL = new FileInfo(pAssemblyFileName);
        if(vIsAssembly) {
          vNode.SetAttribute("name", vAsmblyInst.ManifestModule.Name);
          vNode.SetAttribute("version", vAsmbly.Version.ToString());
        } else {
          vNode.SetAttribute("name", Path.GetFileName(pAssemblyFileName));
          String vVerS = vFL.Length + "";/*DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")*/
          vVerS = "1.0." + vVerS.Trim();
          vNode.SetAttribute("version", vVerS);
        }
        vNode.SetAttribute("size", "" + vFL.Length);
        vFolderNode.AppendChild(vNode);
      }
    }

    private void prcMask(DirectoryInfo di, XmlElement vFolderNode, String pMask) {
      FileInfo[] vAsmbls = di.GetFiles(pMask);
      for(int i = 0; i < vAsmbls.Length; i++) {
        this.processAssembly(vFolderNode, vAsmbls[i].FullName);
      }
    }

    private void scanFolder(XmlElement vFolderNode, String pPath) {
      XmlElement vCurFolderNode = (vFolderNode == null) ? this.FDom.DocumentElement : vFolderNode;
      DirectoryInfo di = new DirectoryInfo(pPath);
      this.prcMask(di, vCurFolderNode, "*.exe");
      this.prcMask(di, vCurFolderNode, "*.dll");
      this.prcMask(di, vCurFolderNode, "*.chm");
      
      DirectoryInfo[] vDirs = di.GetDirectories();
      for(int i = 0; i < vDirs.Length; i++) {
        XmlElement vNewFldrNode = this.FDom.CreateElement("folder");
        vNewFldrNode.SetAttribute("name", vDirs[i].Name);
        vCurFolderNode.AppendChild(vNewFldrNode);
        this.scanFolder(vNewFldrNode, pPath + vDirs[i].Name + "\\");
      }
    }

    public void Build() {
      this.scanFolder(null, this.FRootFolderPath);
    }

    public XmlDocument Document {
      get {
        return this.FDom;
      }
    }

    /// <summary>
    /// Сравнивает два документа. Результат в pFldLocal
    /// </summary>
    /// <param name="pFldLocal"></param>
    /// <param name="pFldRemote"></param>
    public static void compareFldrs(XmlElement pFldLocal, XmlElement pFldRemote) {
      /*Обновление модулей и удаление старых*/
      XmlNodeList vMdls = pFldLocal.SelectNodes("assembly");
      foreach(XmlElement vNode in vMdls) {
        XmlElement vRNode = (XmlElement)pFldRemote.SelectSingleNode("assembly[@name='" + vNode.GetAttribute("name") + "']");
        if((vRNode != null) && (Utl.CompareVer(vRNode.GetAttribute("version"), vNode.GetAttribute("version")) > 0)) {
          XmlNode vNewEl = pFldLocal.OwnerDocument.ImportNode(vRNode, true);
          pFldLocal.ReplaceChild(vNewEl, vNode);
        } else {
          if(vRNode == null)
            vNode.SetAttribute("action", "kill");
          else
            vNode.SetAttribute("action", "skip");
        }
      }
      /*Добавляем новые модули*/
      vMdls = pFldRemote.SelectNodes("assembly");
      foreach(XmlElement vNode in vMdls) {
        XmlElement vLNode = (XmlElement)pFldLocal.SelectSingleNode("assembly[@name='" + vNode.GetAttribute("name") + "']");
        if(vLNode == null) {
          XmlNode vNewEl = pFldLocal.OwnerDocument.ImportNode(vNode, true);
          pFldLocal.AppendChild(vNewEl);
        }
      }

      /*Обновляем существующие и удаляем старые папки*/
      vMdls = pFldLocal.SelectNodes("folder");
      foreach(XmlElement vNode in vMdls) {
        XmlElement vRNode = (XmlElement)pFldRemote.SelectSingleNode("folder[@name='" + vNode.GetAttribute("name") + "']");
        if(vRNode != null) {
          compareFldrs(vNode, vRNode);
        } else {
          if(!vNode.GetAttribute("name").Equals("trash"))
            vNode.SetAttribute("action", "kill");
        }
      }
      /*Добавляем новые папки*/
      vMdls = pFldRemote.SelectNodes("folder");
      foreach(XmlElement vNode in vMdls) {
        XmlNode vLNode = pFldLocal.SelectSingleNode("folder[@name='" + vNode.GetAttribute("name") + "']");
        if(vLNode == null) {
          XmlNode vNewEl = pFldLocal.OwnerDocument.ImportNode(vNode, true);
          pFldLocal.AppendChild(vNewEl);
        }
      }

    }

  }
#endif
}
