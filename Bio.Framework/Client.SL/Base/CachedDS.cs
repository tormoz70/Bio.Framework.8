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
using Bio.Framework.Packets;
using System.Collections.Generic;

namespace Bio.Framework.Client.SL {
  public class CachedDS {
    public IAjaxMng ajaxMng { get; set; }
    public String bioCode { get; set; }
    public Params bioParams { get; set; }
    public Boolean addNullRow { get; set; }
    public DateTime lastLoaded { get; private set; }
    public CJsonStoreMetadata metadata { get; private set; }
    public IEnumerable<CRTObject> data { get; private set; }

    public event EventHandler OnLoaded;

    public void loadData(Action<CachedDS> callback, LinkedListNode<CachedDS> next) {
      var v_cli = new JsonStoreClient {
        AjaxMng = ajaxMng,
        BioCode = bioCode
      };
      v_cli.Load(bioParams, (s, a) => {
        if (a.Response.Success) {
          if (v_cli.jsMetadata.fields.Count > 1) {
            this.metadata = v_cli.jsMetadata;
            this.data = v_cli.DS;
          }
          
          var eve = this.OnLoaded;
          if (eve != null)
            eve(this, new EventArgs());
          
          if (callback != null)
            callback(this);

          if (next != null)
            next.Value.loadData(callback, next.Next);

        }
      });
    }
  }
}
