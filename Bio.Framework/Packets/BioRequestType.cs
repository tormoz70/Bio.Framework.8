using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Framework.Packets {
  /// <summary>
  /// Типы запросов.
  /// DS - набор данных;
  /// SQLR - выполняет на сервере PL/SQL, с возможностью вернуть значения;
  /// </summary>
  public enum RequestType {
    [RequestTypeMapping("")]
    Unassigned,
    [RequestTypeMapping("Bio.Framework.Server.tm_ping,Bio.Framework.Server.Types")]
    doPing,
    [RequestTypeMapping("Bio.Framework.Server.tm_login_post,Bio.Framework.Server.Types")]
    doPostLoginForm,
    [RequestTypeMapping("Bio.Framework.Server.tm_logout,Bio.Framework.Server.Types")]
    doLogout,
    [RequestTypeMapping("Bio.Framework.Server.tm_asmb,Bio.Framework.Server.Types")]
    asmbVer,

    [RequestTypeMapping("Bio.Framework.Server.tmio_DS,Bio.Framework.Server.Handler.DStore")]
    DS,
    [RequestTypeMapping("Bio.Framework.Server.tmio_DSFetch,Bio.Framework.Server.Handler.DSFetch")]
    DSFetch,
    [RequestTypeMapping("Bio.Framework.Server.tmio_SQLR,Bio.Framework.Server.Handler.SQLR")]
    SQLR,
    [RequestTypeMapping("Bio.Framework.Server.tmio_WebDB,Bio.Framework.Server.Handler.WebDB")]
    WebDB,
    [RequestTypeMapping("Bio.Framework.Server.tmio_rpt,Bio.Framework.Server.Handler.Rpt")]
    Rpt,
    [RequestTypeMapping("Bio.Framework.Server.tmio_Tree,Bio.Framework.Server.Handler.Tree")]
    Tree,
    [RequestTypeMapping("Bio.Framework.Server.tmio_Pipe,Bio.Framework.Server.Handler.Pipe")]
    srvPipe,
    [RequestTypeMapping("Bio.Framework.Server.tmio_LongOp,Bio.Framework.Server.Handler.LongOp")]
    srvLongOp,
    [RequestTypeMapping("Bio.Framework.Server.tmio_DS2XL,Bio.Framework.Server.Handler.DS2XL")]
    DS2XL,
    [RequestTypeMapping("Bio.Framework.Server.tmio_FileSrv,Bio.Framework.Server.Handler.FileSrv")]
    FileSrv,
  };
}
