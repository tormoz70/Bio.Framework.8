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

  public class RmtClientRequest : BioRequest {
    public String title { get; set; }
    public RmtClientRequestCmd cmd { get; set; }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as RmtClientRequest;
      dst.title = this.title;
      dst.cmd = this.cmd;
    }

  }
}
