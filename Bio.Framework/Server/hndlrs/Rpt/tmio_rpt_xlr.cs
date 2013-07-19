namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using System.Threading;
  //using Excel = Microsoft.Office.Interop.Excel;

  /// <summary>
  /// Обработчик запроса внутренностей компонента Rpt
  /// </summary>
  public class tmio_rpt_xlr: ABioHandlerBio {

    public tmio_rpt_xlr(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    private const String csRptQueueIpcClientKey = "RptQueueIpcClientKey";
    protected override void doExecute() {
      base.doExecute();

    }

  }
}
