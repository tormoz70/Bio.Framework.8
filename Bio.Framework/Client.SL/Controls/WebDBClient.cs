using System;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using System.Windows.Browser;

namespace Bio.Framework.Client.SL {

  public class WebDBClient {

    public IAjaxMng AjaxMng { get; set; }
    public String BioCode { get; set; }
    private Params _bioParams;
    public Params BioParams { 
      get { return this._bioParams ?? (this._bioParams = new Params()); }
      set {
        this._bioParams = value;
      }
    }

    public WebDBClient() { 
    }

    public void Open(Params bioPrms, HtmlPopupWindowOptions opts) {
      var v_url_body = this._bldBodyUrl();

      this.BioParams = Params.PrepareToUse(this.BioParams, bioPrms);
      var v_cli = new SQLRClient();
      v_cli.AjaxMng = this.AjaxMng;
      v_cli.BioCode = "iod.ping_webdb";

      v_cli.Exec(null, (s, a) => HtmlPage.Window.Navigate(new Uri(v_url_body, UriKind.Relative), "myTarget"));

    }




    public void OpenModal(Params bioPrms, HtmlPopupWindowOptions opts) {
      if (this.AjaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.BioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");

      this.BioParams = Params.PrepareToUse(this.BioParams, bioPrms);
      var v_cli = new SQLRClient();
      v_cli.AjaxMng = this.AjaxMng;
      v_cli.BioCode = "iod.ping_webdb";

      //http://localhost/ekb8/srv.aspx?rqtp=FileSrv&rqbc=ios.givc.mailer.getAttchmntFile&hf=1216

      var urlBody = this._bldBodyUrl();
      const string c_url = "sys/HTMLShowPage.htm";
      var v_opts = opts ?? new HtmlPopupWindowOptions { Width = 600, Height = 500 };
      v_cli.Exec(null, (s, a) => {
        var v_opts_line = "";
        Utl.AppendStr(ref v_opts_line, "resizable:" + ((v_opts.Resizeable) ? "yes" : "no"), ";");
        Utl.AppendStr(ref v_opts_line, "menubar:no;status:no;center:yes;help:no;minimize:no;maximize:no;border:think;statusbar:no", ";");
        Utl.AppendStr(ref v_opts_line, "dialogWidth:" + v_opts.Width + "px", ";");
        Utl.AppendStr(ref v_opts_line, "dialogHeight:" + v_opts.Height + "px", ";");
        //var v_rsp = a.response as BioResponse;
        var v_html = "Сообщение" + "||" + urlBody; 
        var v_js = String.Format("self.showModalDialog('{0}', '{1}', '{2}');", c_url, v_html, v_opts_line);
        HtmlPage.Window.Eval(v_js);
      });

    }

    private String _bldBodyUrl() {
      var prms = ajaxUTL.PrepareRequestParams(new JsonStoreRequestGet {
        RequestType = RequestType.WebDB,
        BioCode = this.BioCode,
        BioParams = this.BioParams
      });
      return this.AjaxMng.Env.ServerUrl + "?" + prms;
    }

  }
}
