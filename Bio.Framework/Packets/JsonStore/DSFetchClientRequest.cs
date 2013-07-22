using System;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public class DSFetchClientRequest : RmtClientRequest {
    public String Selection { get; set; }
    public JsonStoreSort Sort { get; set; }
    public JsonStoreFilter Filter { get; set; }
    public String ExecBioCode { get; set; }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as DSFetchClientRequest;
      if (dst != null) {
        dst.Selection = this.Selection;
        dst.Sort = (this.Sort != null) ? (JsonStoreSort)this.Sort.Clone() : null;
        dst.Filter = (this.Filter != null) ? (JsonStoreFilter)this.Filter.Clone() : null;
        dst.ExecBioCode = this.ExecBioCode;
      }
    }

  }
}
