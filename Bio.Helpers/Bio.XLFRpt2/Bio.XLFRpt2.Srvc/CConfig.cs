using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.IO;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;
using Bio.Helpers.DOA;

namespace Bio.Helpers.XLFRpt2.Srvc {
  //public delegate void MessageLogWriterDelegate(String msg);
  //public delegate void ErrorLogWriterDelegate(Exception ex);
  /// <summary>
  /// Конфигурация системы
  /// </summary>
  public class CConfigSys : CConfigBase {
    /// <summary>
    /// Корневая папка, от которой начинается дерево каталога отчетов.
    /// </summary>
    public String rootRptPath { get; private set; }

    /// <summary>
    /// Полное имя типа реализации CQueue, если не указан, то используется CQueueDefaultImpl
    /// </summary>
    public String queueImplementationType { get; private set; }
    /// <summary>
    /// Сколько отчетов выполняется одновременно в разных потоках, по умолчанию 6
    /// </summary>
    public Int32 poolSize { get; private set; }

    protected static T _load<T>(String physicalApplicationPath, String logFileName) where T : CConfigSys, new() {
      //String vphPath = Utl.NormalizeDir(p_physicalApplicationPath);
      //XmlDocument vDoc = dom4cs.OpenDocument(vphPath + "config.xml").XmlDoc;
      //T rslt = new T {
      //  physicalApplicationPath = p_physicalApplicationPath,
      //  logFileName = p_logFileName,
      //  connStr = Xml.getInnerText((XmlElement)vDoc.DocumentElement.SelectSingleNode("connection")),
      //  rootPath = normPath(vphPath, Xml.getInnerText((XmlElement)vDoc.DocumentElement.SelectSingleNode("root_path"))),
      //  workPath = normPath(vphPath, Xml.getInnerText((XmlElement)vDoc.DocumentElement.SelectSingleNode("work_path"))),
      //  queueImplementationType = vDoc.DocumentElement.GetAttribute("queueImplementationType")
      //};
      //if(String.IsNullOrEmpty(rslt.queueImplementationType))
      //  rslt.queueImplementationType = "Bio.Helpers.XLFRpt2.Srvc.CQueueDefaultImpl";
      //int v_poolSize = 0;
      //if (Int32.TryParse(vDoc.DocumentElement.GetAttribute("poolSize"), out v_poolSize))
      //  rslt.poolSize = v_poolSize;
      //else
      //  rslt.poolSize = 6;

      //XmlElement smtp = vDoc.DocumentElement.SelectSingleNode("smtp") as XmlElement;
      //rslt.smtp = SmtpCfg.LoadFromXml(smtp);
      //XmlElement admin = vDoc.DocumentElement.SelectSingleNode("admin") as XmlElement;
      //if (admin != null)
      //  rslt.adminEmail = Xml.getAttribute<String>(admin, "email", String.Empty);

      //initPath(rslt.rootPath);
      //initPath(rslt.workPath);
      //rslt.dbSession = new DBSession(rslt.connStr);
      T rslt = CConfigBase._load<T>(physicalApplicationPath, logFileName, new CDBSessionFactory());
      rslt.rootRptPath = normPath(rslt.physicalApplicationPath, Xml.getInnerText((XmlElement)rslt.CfgDoc.DocumentElement.SelectSingleNode("root_path")));
      rslt.queueImplementationType = rslt.CfgDoc.DocumentElement.GetAttribute("queueImplementationType");

      if(String.IsNullOrEmpty(rslt.queueImplementationType))
        rslt.queueImplementationType = "Bio.Helpers.XLFRpt2.Srvc.CQueueDefaultImpl";
      int v_poolSize = 0;
      if (Int32.TryParse(rslt.CfgDoc.DocumentElement.GetAttribute("poolSize"), out v_poolSize))
        rslt.poolSize = v_poolSize;
      else
        rslt.poolSize = 6;

      initPath(rslt.rootRptPath);
      return rslt;
    }
    public static CConfigSys load(String physicalApplicationPath, String p_logFileName) {
      return CConfigSys._load<CConfigSys>(physicalApplicationPath, p_logFileName);
    }
  }
  
}
