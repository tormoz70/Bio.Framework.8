using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Bio.Helpers.Common.Types {
  public class SmtpCfg {
    public String smtpServer { get; set; }
    public Int32 port { get; set; }
    public String authUser { get; set; }
    public String authPwd { get; set; }
    public String fromMailAddr { get; set; }
    public String encoding { get; set; }

    public static SmtpCfg LoadFromXml(XmlElement smtpNode) {
      if (smtpNode != null) {
        var rslt = new SmtpCfg {
          smtpServer = Xml.getAttribute<String>(smtpNode, "smtpServer", String.Empty),
          port = Xml.getAttribute<Int32>(smtpNode, "port", 25),
          authUser = Xml.getAttribute<String>(smtpNode, "authUser", String.Empty),
          authPwd = Xml.getAttribute<String>(smtpNode, "authPwd", String.Empty),
          fromMailAddr = Xml.getAttribute<String>(smtpNode, "fromMailAddr", String.Empty),
          encoding = Xml.getAttribute<String>(smtpNode, "encoding", String.Empty)
        };
        return rslt;
      }
      return null;
    }
  }
}
