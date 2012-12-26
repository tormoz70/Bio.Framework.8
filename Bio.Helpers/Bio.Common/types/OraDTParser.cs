using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Helpers.Common.Types {
  public class OraDTP : DateTimeParser {
    /// <summary>
    /// Единственный экземпляр класса.
    /// </summary>
    public new static OraDTP Instance;

    static OraDTP() {
      Instance = new OraDTP();
      Instance.Fmts = new CDTFmt[] {
        new CDTFmt {
          fmt = "YYYYMMDD",
          regex  = "^[012]\\d{3}[01]\\d{1}[0123]\\d{1}$"
        },
        new CDTFmt {
          fmt = "YYYY-MM-DD HH24:MI:SS",
          regex  = "^[012]\\d{3}[-][01]\\d{1}[-][0123]\\d{1}\\s[012]\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        }
      };
    }

    //public override string SYS_CS_DATETIME_FORMATS(int pFmtIndex) {
    //  switch (pFmtIndex) {
    //    case 0: return "YYYYMMDD";
    //    case 1: return "YYYY-MM-DD HH24:MI:SS";
    //    default: return null;
    //  }
    //}
    //protected override int SYS_CI_DATETIME_FORMATS_COUNT {
    //  get { return 2; }
    //}
    //protected override String SYS_CS_DATETIME_REGEXS(int pFmtIndex) {
    //  switch (pFmtIndex) {
    //    case 0: return "^" + csYear + csMonth + csDay + "$";
    //    case 1: return "^" + csYear + cdD + csMonth + cdD + csDay + "T" + csHour + cdT + csMinute + cdT + csSecs + "$";
    //    default: return null;
    //  }
    //}
  }
}
