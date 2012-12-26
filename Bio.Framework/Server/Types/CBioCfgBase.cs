namespace Bio.Framework.Server {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using Bio.Helpers.Common.Types;
  using System.IO;
  using Bio.Helpers.Common;
  using System.Reflection;

  public abstract class CBioCfgBase {
    public String ConnectionString { get; protected set; }

    public Boolean Debug { get; private set; }
    public Boolean EnableDocInhirance { get; private set; }
    public Boolean DoReloadIOOnInit { get; private set; }

    public String AppURL { get; private set; }
    public String LocalPath { get; private set; }
    public String IniPath { get { return this.LocalPath + "ini\\"; } }
    public String IniURL { get { return this.AppURL + "/ini/"; } }
    public String RptPath { get { return this.IniPath + "reports\\"; } }
    public String BioSysTitle { get; private set; }
    public String WorkSpacePath { get; private set; }
    public String LocalIOCfgPath { get; private set; }
    public String TmpPath { get; private set; }
    public String RptLogsPath { get; private set; }

    public CBioUser CurUser { get; protected set; }

    public abstract void Login(String login);

    public CBioCfgBase(String localPath, String appURL) {
      //Assembly asmb0 = Assembly.GetEntryAssembly();
      //Assembly asmb1 = Assembly.GetCallingAssembly();
      //Assembly asmb2 = Assembly.GetExecutingAssembly();
      //this.LocalPath = Path.GetDirectoryName(asmb1.Location);
      //this.LocalPath = Directory.GetParent(this.LocalPath).FullName;
      this.LocalPath = localPath;
      this.AppURL = appURL;
      dom4cs v_doc = dom4cs.OpenDocument(this.IniPath + SrvConst.csCfgFileName);

      XmlNode vLoadOptions = v_doc.XmlDoc.DocumentElement.SelectSingleNode("iobjects/load_options");
      this.DoReloadIOOnInit = (vLoadOptions != null) ? ((XmlElement)vLoadOptions).GetAttribute("reloadIOOnInit").Equals("true") : true;
      this.EnableDocInhirance = (vLoadOptions != null) ? ((XmlElement)vLoadOptions).GetAttribute("enableDocInhirance").Equals("true") : true;
      XmlNode vDbg = (XmlElement)v_doc.XmlDoc.DocumentElement.SelectSingleNode("debug[@enabled='true']");
      this.Debug = (vDbg != null);

      XmlElement vElmBioSysTitle = v_doc.XmlDoc.DocumentElement.SelectSingleNode("biosys_title") as XmlElement;
      if (vElmBioSysTitle != null)
        this.BioSysTitle = vElmBioSysTitle.InnerText;

      /* Строка соединения с системой учета пользователей */
      this.ConnectionString = null;
      XmlElement vElmBioSysConnectionString = v_doc.XmlDoc.DocumentElement.SelectSingleNode("biosys_connection") as XmlElement;
      if (vElmBioSysConnectionString != null)
        this.ConnectionString = vElmBioSysConnectionString.InnerText;

      /* рабочий каталог - корень */
      this.WorkSpacePath = null;
      if (v_doc.XmlDoc.DocumentElement.SelectSingleNode("work_space_path") != null) {
        this.WorkSpacePath = v_doc.XmlDoc.DocumentElement.SelectSingleNode("work_space_path").InnerText;
        if (Directory.Exists(this.LocalPath + this.WorkSpacePath)) {
          String[] vPath = Utl.SplitString(this.LocalPath, '\\');
          String vAppNamePath = vPath[vPath.Length - 2];
          this.WorkSpacePath = Path.GetFullPath(this.LocalPath + this.WorkSpacePath + vAppNamePath + "_ws") + "\\";
        }
      }
      if (this.WorkSpacePath == null)
        this.WorkSpacePath = this.LocalPath;

      /* рабочие папки */
      this.LocalIOCfgPath = this.WorkSpacePath + "locioini\\";
      this.RptLogsPath = this.WorkSpacePath + "rpt_logs\\";
      this.TmpPath = this.WorkSpacePath + "tmp\\";
    }

  }
}
