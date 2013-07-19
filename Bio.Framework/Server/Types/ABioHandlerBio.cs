namespace Bio.Framework.Server {

  using System;
  using System.Xml;
  using System.Web;
  using System.IO;

  using Bio.Helpers.Common.Types;

  public class ABioHandlerBio:ABioHandlerSys {

    public ABioHandlerBio(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    protected String CurIOLocalPath { get; private set; }
    protected String CurIOLocalUrl { get; private set; }
    protected String CurIOFileName { get; private set; }

    private void removeSC(ref String vStr) {
      if(vStr[0] == '"')
        vStr = vStr.Remove(0, 1);
      if(vStr[vStr.Length - 1] == '"')
        vStr = vStr.Remove(vStr.Length - 1, 1);
    }

    private XmlElement findAppDocOfModule(String pDir, String pIOCode) {
      XmlElement vRslt = null;
      String vAppCfgFN = pDir + "app.config";
      if(File.Exists(vAppCfgFN)) {
        XmlDocument vDoc = dom4cs.OpenDocument(vAppCfgFN).XmlDoc;
        XmlNodeList vMods = vDoc.DocumentElement.SelectNodes("load/module[not(@enabled) or (@enabled='true')]");
        foreach(XmlElement vMod in vMods) {
          //if((!vMod.HasAttribute("enabled") || String.Equals(vMod.GetAttribute("enabled"), "true", StringComparison.CurrentCultureIgnoreCase)) 
          //    && String.Equals(vMod.GetAttribute("iocode"), pIOCode, StringComparison.CurrentCultureIgnoreCase)) {
          if(String.Equals(vMod.GetAttribute("iocode"), pIOCode, StringComparison.CurrentCultureIgnoreCase)) {
            vRslt = vMod;
            break;
          }
        }
      }
      if((vRslt == null) && !String.IsNullOrEmpty(pDir)) {
        DirectoryInfo vCurDir = new DirectoryInfo(pDir);
        DirectoryInfo vPrntDir = Directory.GetParent(pDir.TrimEnd(new Char[]{'\\'}));
        if ((vPrntDir != null) && (vPrntDir.FullName.ToLower().StartsWith(this.BioSession.Cfg.LocalPath.ToLower()))) {
          String vNextIOCode = vCurDir.Name + "." + pIOCode;
          vRslt = this.findAppDocOfModule(vPrntDir.FullName + "\\", vNextIOCode);
        }
      }
      return vRslt;
    }

    private void prepareModuleAsApp(XmlElement pIONode){
      String vIOCode = Path.GetFileNameWithoutExtension(this.CurIOFileName);
      String vIOPath = this.CurIOLocalPath;
      XmlElement vAppDoc = this.findAppDocOfModule(vIOPath, vIOCode);
      if(vAppDoc != null) {
        if(vAppDoc.HasAttribute("addToStartMenu"))
          pIONode.SetAttribute("addToStartUp", vAppDoc.GetAttribute("addToStartMenu"));
      }
    }

    /// <summary>
    /// Удаляет из FXMLResponse лишнюю информацию, которая не нужна на клиенте, например тэг "SQL"
    /// Данная процедура должна вызываться непоследственно перед отправкой данных клиенту
    /// </summary>
    //protected virtual void prepareResponseToSendBack() {
    //  XmlNode vIONode = this.FXMLResponse.DocumentElement.SelectSingleNode("//bio_module");
    //  this.prepareModuleAsApp((XmlElement)vIONode);
    //  if(vIONode != null) {
    //    XmlNodeList vSQLNodes = vIONode.SelectNodes("//SQL");
    //    foreach(XmlNode vSQLNode in vSQLNodes) {
    //      vSQLNode.ParentNode.RemoveChild(vSQLNode);
    //    }
    //  }
    //}

    private void initIO() {
      CIObject vObj = null;
      this.CurIOLocalPath = null;
      if(this.bioCode != null) {
        //Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "self_io_code", this.bioCode);
        //Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "self_io_params", this.FIOParamsStr);
        vObj = CIObject.CreateIObject(this.bioCode, this.BioSession);
        if(vObj != null) {
          this.CurIOLocalPath = vObj.LocalPath;
          this.CurIOLocalUrl = vObj.ioUrl;
          this.CurIOFileName = vObj.IniDocument.FileName;
          //Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "self_io_url", vObj.ioUrl);
          //String vIO_Name = this.bioCode.Substring(this.bioCode.LastIndexOf(".") + 1);
          //if(File.Exists(this.BioSession.LocalPath + vObj.ioUrl.Substring(vObj.ioUrl.IndexOf("/", 1) + 1).Replace("/", "\\") + vIO_Name + ".js"))
          //  Bio.Common.Xml.appendNode2El(this.FXMLResponse.DocumentElement, "load_script", "true");
          //XmlElement vIODoc = vObj.IniDocument.XmlDoc.DocumentElement;
          //this.FXMLResponse.DocumentElement.AppendChild(this.FXMLResponse.ImportElement(vIODoc, true));
          this.FBioDesc = dom4cs.CloneDocument(vObj.IniDocument.XmlDoc);
        }
      }

    }

    /// <summary>
    /// Основная процедура обработки запроса
    /// Здесь ее наследование прерывается
    /// </summary>
    protected override void doExecute() {
      base.doExecute();
      this.initIO();
    }

    /// <summary>
    /// Создает исключение с привязкой к ИО
    /// </summary>
    /// <param name="pMsg"></param>
    /// <param name="pInnerException"></param>
    /// <returns></returns>
    protected EBioException creBioEx(String pMsg, Exception pInnerException) {
      return new EBioException("Ошибка при обработке запроса к ИО {" + this.bioCode + "}. Сообщение: " + pMsg, pInnerException);
    }
  }

}
