using System;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public class LongOpClientRequest : RmtClientRequest {
    public String Pipe { get; set; }
    public String SessionUID { get; set; }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as LongOpClientRequest;
      if (dst != null) {
        dst.Pipe = this.Pipe;
        dst.SessionUID = this.SessionUID;
      }
    }

  }
}
