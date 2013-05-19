namespace Bio.Framework.Server {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using Bio.Helpers.Common.Types;
  using System.IO;
  using Bio.Helpers.Common;
  using System.Reflection;

  public abstract class BioCfgBase {
    public String ConnectionString { get; protected set; }
    public String WorkspaceSchema { get; protected set; }

    public Boolean Debug { get; private set; }
    public Boolean EnableDocInhirance { get; private set; }
    public Boolean DoReloadIOOnInit { get; private set; }

    public String AppURL { get; private set; }
    public String LocalPath { get; private set; }
    public String IniPath { get { return this.LocalPath + "ini\\"; } }
    public String IniURL { get { return this.AppURL + "/ini/"; } }
    public String RptPath { get { return this.IniPath + "reports\\"; } }
    public String BioSysTitle { get; private set; }
    public String WorkspacePath { get; private set; }
    public String LocalIOCfgPath { get; private set; }
    public String TmpPath { get; private set; }
    public String RptLogsPath { get; private set; }

    public BioUser CurUser { get; protected set; }

    public abstract void Login(String login);

    public BioCfgBase(String localPath, String appURL) {
      //Assembly asmb0 = Assembly.GetEntryAssembly();
      //Assembly asmb1 = Assembly.GetCallingAssembly();
      //Assembly asmb2 = Assembly.GetExecutingAssembly();
      //this.LocalPath = Path.GetDirectoryName(asmb1.Location);
      //this.LocalPath = Directory.GetParent(this.LocalPath).FullName;
      this.LocalPath = localPath;
      this.AppURL = appURL;
      var v_doc = dom4cs.OpenDocument(this.IniPath + SrvConst.csCfgFileName);

      var v_loadOptions = v_doc.XmlDoc.DocumentElement.SelectSingleNode("iobjects/load_options");
      this.DoReloadIOOnInit = (v_loadOptions != null) ? ((XmlElement)v_loadOptions).GetAttribute("reloadIOOnInit").Equals("true") : true;
      this.EnableDocInhirance = (v_loadOptions != null) ? ((XmlElement)v_loadOptions).GetAttribute("enableDocInhirance").Equals("true") : true;
      var v_dbg = (XmlElement)v_doc.XmlDoc.DocumentElement.SelectSingleNode("debug[@enabled='true']");
      this.Debug = (v_dbg != null);

      this.BioSysTitle = Xml.getInnerText(v_doc.XmlDoc.DocumentElement, "biosys_title", null);

      /* Строка соединения с системой учета пользователей */
      this.ConnectionString = Xml.getInnerText(v_doc.XmlDoc.DocumentElement, "biosys_connection", null);
      /* Рабочая схема */
      this.WorkspaceSchema = Xml.getInnerText(v_doc.XmlDoc.DocumentElement, "work_space_schema", null);

      /* рабочий каталог - корень */
      this.WorkspacePath = null;
      if (v_doc.XmlDoc.DocumentElement.SelectSingleNode("work_space_path") != null) {
        this.WorkspacePath = v_doc.XmlDoc.DocumentElement.SelectSingleNode("work_space_path").InnerText;
        if (Directory.Exists(this.LocalPath + this.WorkspacePath)) {
          var v_path = Utl.SplitString(this.LocalPath, '\\');
          var v_appNamePath = v_path[v_path.Length - 2];
          this.WorkspacePath = Path.GetFullPath(this.LocalPath + this.WorkspacePath + v_appNamePath + "_ws") + "\\";
        }
      }
      if (this.WorkspacePath == null)
        this.WorkspacePath = this.LocalPath;

      /* рабочие папки */
      this.LocalIOCfgPath = this.WorkspacePath + "locioini\\";
      this.RptLogsPath = this.WorkspacePath + "rpt_logs\\";
      this.TmpPath = this.WorkspacePath + "tmp\\";
    }

  }
}
