using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.IO;
using Bio.Helpers.Common;

namespace Bio.Helpers.Common.Types {
  /// <summary>
  /// Конфигурация системы
  /// </summary>
  public class CConfigBase {
    /// <summary>
    /// Отладка разрешена
    /// </summary>
    public Boolean debugEnabled { get; set; }
    /// <summary>
    /// Путь к папке - корень приложения (папка в которой лежит config.xml)
    /// </summary>
    public String physicalApplicationPath { get; set; }
    /// <summary>
    /// Строка соединения с БД
    /// </summary>
    public String connStr { get; private set; }
    /// <summary>
    /// Путь к папке, которая используется как рабочая, для создания влеменных файлов и записи отладочной информации 
    /// </summary>
    public String workPath { get; private set; }
    /// <summary>
    /// Конфигурация почтового клиента
    /// </summary>
    public SmtpCfg smtp { get; set; }
    /// <summary>
    /// Адрес админа, куда отправлять письма при ошибках
    /// </summary>
    public String adminEmail { get; set; }

    /// <summary>
    /// Ссылка на процедуру записи сообщений в лог-файл
    /// </summary>
    public MessageLogWriterDelegate msgLogWriter { get; set; }
    /// <summary>
    /// Ссылка на процедуру записи ошибок в лог-файл
    /// </summary>
    public ErrorLogWriterDelegate errLogWriter { get; set; }
    /// <summary>
    /// Путь + Имя лог-файла
    /// </summary>
    public String logFileName { get; set; }

    protected static String normPath(String root_path, String path) {
      return root_path + (!String.IsNullOrEmpty(path) ? path.TrimStart('\\').TrimEnd('\\') + '\\' : null);
    }
    protected static void initPath(String path) {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
    }
    public XmlDocument CfgDoc { get; private set; }
    protected static T _load<T>(String physicalApplicationPath, String logFileName, ADBSessionFactory dbsFactory) where T : CConfigBase, new() {
      var vphPath = Utl.NormalizeDir(physicalApplicationPath);
      var vcfgDoc = dom4cs.OpenDocument(vphPath + "config.xml").XmlDoc;
      T rslt = new T {
        debugEnabled = false,
        CfgDoc = vcfgDoc,
        physicalApplicationPath = physicalApplicationPath,
        logFileName = logFileName,
        connStr = Xml.getInnerText((XmlElement)vcfgDoc.DocumentElement.SelectSingleNode("connection")),
        workPath = normPath(vphPath, Xml.getInnerText((XmlElement)vcfgDoc.DocumentElement.SelectSingleNode("work_path")))
      };

      var dbg = rslt.CfgDoc.DocumentElement.SelectSingleNode("debug") as XmlElement;
      if (dbg != null)
        rslt.debugEnabled = Xml.getAttribute<Boolean>(dbg, "enabled", rslt.debugEnabled);

      var smtp = rslt.CfgDoc.DocumentElement.SelectSingleNode("smtp") as XmlElement;
      rslt.smtp = SmtpCfg.LoadFromXml(smtp);
      var admin = rslt.CfgDoc.DocumentElement.SelectSingleNode("admin") as XmlElement;
      if (admin != null)
        rslt.adminEmail = Xml.getAttribute<String>(admin, "email", String.Empty);

      initPath(rslt.workPath);

      if (dbsFactory != null)
        rslt.dbSession = dbsFactory.CreateDBSession(rslt.connStr);
      return rslt;
    }
    public static CConfigBase load(String physicalApplicationPath, String logFileName, ADBSessionFactory dbsFactory) {
      return _load<CConfigBase>(physicalApplicationPath, logFileName, dbsFactory);
    }

    public IDBSession dbSession { get; private set; }
  }
}
