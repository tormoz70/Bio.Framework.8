using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public class CDSFetchClientRequest : RmtClientRequest {
    public String selection { get; set; }
    public CJsonStoreSort sort { get; set; }
    public CJsonStoreFilter filter { get; set; }
    public String execBioCode { get; set; }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as CDSFetchClientRequest;
      dst.selection = this.selection;
      dst.sort = (this.sort != null) ? (CJsonStoreSort)this.sort.Clone() : null;
      dst.filter = (this.filter != null) ? (CJsonStoreFilter)this.filter.Clone() : null;
      dst.execBioCode = this.execBioCode;
    }

  }
}
