using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using System.Threading;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Browser;

namespace Bio.Framework.Client.SL {

  public class CWebDBClient {

    public IAjaxMng ajaxMng { get; set; }
    public String bioCode { get; set; }
    private Params _bioParams = null;
    public Params bioParams { 
      get {
        if (this._bioParams == null)
          this._bioParams = new Params();
        return this._bioParams;
      }
      set {
        this._bioParams = value;
      }
    }

    public CWebDBClient() { 
    }

    private String _lastRequestedBioCode = null;

    public void Open(Params bioPrms, HtmlPopupWindowOptions opts) {
      //HtmlPage.Window.
      var v_url_body = this._bldBodyUrl();
      var v_url = "sys/HTMLShowPage.htm";

      this.bioParams = Params.PrepareToUse(this.bioParams, bioPrms);
      var v_cli = new CSQLRClient();
      v_cli.ajaxMng = this.ajaxMng;
      v_cli.bioCode = "iod.ping_webdb";

      v_cli.Exec(null, (s, a) => {
        System.Windows.Browser.HtmlPage.Window.Navigate(new Uri(v_url_body, UriKind.Relative), "myTarget");
      });

    }




    public void OpenModal(Params bioPrms, HtmlPopupWindowOptions opts) {
      if (this.ajaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.bioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");

      this.bioParams = Params.PrepareToUse(this.bioParams, bioPrms);
      var v_cli = new CSQLRClient();
      v_cli.ajaxMng = this.ajaxMng;
      v_cli.bioCode = "iod.ping_webdb";

      //http://localhost/ekb8/srv.aspx?rqtp=FileSrv&rqbc=ios.givc.mailer.getAttchmntFile&hf=1216

      var v_url_body = this._bldBodyUrl();
      var v_url = "sys/HTMLShowPage.htm";
      var v_opts = opts ?? new HtmlPopupWindowOptions { Width = 600, Height = 500 };
      v_cli.Exec(null, (s, a) => {
        var v_opts_line = "";
        Utl.AppendStr(ref v_opts_line, "resizable:" + ((v_opts.Resizeable) ? "yes" : "no"), ";");
        Utl.AppendStr(ref v_opts_line, "menubar:no;status:no;center:yes;help:no;minimize:no;maximize:no;border:think;statusbar:no", ";");
        Utl.AppendStr(ref v_opts_line, "dialogWidth:" + v_opts.Width + "px", ";");
        Utl.AppendStr(ref v_opts_line, "dialogHeight:" + v_opts.Height + "px", ";");
        //var v_rsp = a.response as CBioResponse;
        var v_html = "Сообщение" + "||" + v_url_body; 
        var v_js = String.Format("self.showModalDialog('{0}', '{1}', '{2}');", v_url, v_html, v_opts_line);
        HtmlPage.Window.Eval(v_js);
      });

    }

    private String _bldBodyUrl() {
      String prms = ajaxUTL.prepareRequestParams(new CJsonStoreRequestGet {
        requestType = RequestType.WebDB,
        bioCode = this.bioCode,
        bioParams = this.bioParams
      });
      return this.ajaxMng.Env.ServerUrl + "?" + prms;
    }

  }
}
