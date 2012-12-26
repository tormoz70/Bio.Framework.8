using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {
  public enum RmtClientRequestCmd {
    Run = 0, Break = 1, Kill = 2, GetState = 3, GetResult = 4
  };

  public class CRmtClientRequest : CBioRequest {
    public String title { get; set; }
    public RmtClientRequestCmd cmd { get; set; }

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as CRmtClientRequest;
      dst.title = this.title;
      dst.cmd = this.cmd;
    }

  }
}
