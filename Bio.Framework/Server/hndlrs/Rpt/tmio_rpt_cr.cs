namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;

  /// <summary>
  /// ���������� ������� ������������� ���������� Rpt
  /// </summary>
  public class tmio_rpt_cr:ABioHandlerBio {

    public tmio_rpt_cr(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private void openReport() {
      this.Context.Response.ClearContent();
      this.Context.Response.ClearHeaders();
      String vFileName = (String)this.Context.Session["last_report_filename"];
      if(String.IsNullOrEmpty(vFileName))
        throw new EBioException("���� ���������� ������������ ����� ������ �� ���������. �������� ����� ��� �������� � ������ ������ ��� �� �������� ������.");
      String vContentType = (String)this.Context.Session["last_report_cnt_type"];
      String vFileExt = (String)this.Context.Session["last_report_fileext"];
      this.Context.Response.ContentType = vContentType;
      String vRemoteFName = Path.GetFileNameWithoutExtension(vFileName).Replace(".", "_") + "." + vFileExt;
      this.Context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + vRemoteFName + "\"");
      this.Context.Response.WriteFile(vFileName);
      this.Context.Response.Flush();
      this.Context.Response.Close();
    }

    protected override void doExecute() {
      base.doExecute();
      String vLoadMode = this.getQParamValue("getRptResult", false);
      if((vLoadMode != null) && (vLoadMode.ToLower().Equals("true"))) {
        // ������ ���������
        this.openReport();
      } else {
        // ������ �����
        try {
          String vRptCode = this.getQParamValue("iocd", true);

          CRptBuilderCR vBldr = new CRptBuilderCR(this.BioSession, vRptCode);
          //String vCS = this.BioSession.DBSess.ConnectionString;
          CParams vPrms = this.bioParams;
          vBldr.doBuild(vPrms);

          //String vFN = xlReportInst.LastReportResultFile;
          this.Context.Session.Add("last_report_filename", vBldr.resultFileName);
          this.Context.Session.Add("last_report_fileext", vBldr.FileExt);
          this.Context.Session.Add("last_report_cnt_type", vBldr.ContentType);
          this.Context.Response.Write(new CBioResponse() { success = true }.Encode());
        } //catch(Exception ex) {
          //  //this.Context.Response.Write("{\"errors\":[], \"errMsg\":\"" + xml2jsn.safeJSON(ex.ToString()) + "\"}");
          //  //this.Context.Response.Write(ex.ToString());
          //}
        finally {
        }
      }
    }
  }
}
