using System;

namespace Bio.Framework.Client.SL {
  public interface IConfigRoot: IConfigRec {
    String LastLoggedInUserName { get; set; }
    String LastLoggedInUserPwd { get; set; }

    Int32 RequestTimeout { get; set; }
    Boolean AutoConnect { get; set; }
    Boolean SavePassword { get; set; }

    Boolean SuspendLoadDataInGrids { get; set; }

    Boolean BeOnline { get; set; }
    Boolean CacheCbxItems { get; set; }
  }
}
