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
  /// ���������� ������� ������������� ���������� Rpt
  /// </summary>
  public class tmio_rpt_xlr: ABioHandlerBio {

    public tmio_rpt_xlr(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private const String csRptQueueIpcClientKey = "RptQueueIpcClientKey";
    protected override void doExecute() {
      base.doExecute();

    }

  }
}
