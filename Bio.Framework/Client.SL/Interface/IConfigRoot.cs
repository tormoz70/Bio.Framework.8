using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Framework.Client.SL {
  public interface IConfigRoot: IConfigRec {
    String LastLoggedInUserName { get; set; }
    String LastLoggedInUserPwd { get; set; }

    Int32 RequestTimeout { get; set; }
    Boolean AutoConnect { get; set; }
    Boolean SavePassword { get; set; }

    Boolean SuspendLoadDataInGrids { get; set; }

    Boolean ValidateCfg();
  }
}
