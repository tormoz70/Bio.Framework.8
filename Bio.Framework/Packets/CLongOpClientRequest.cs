using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public class CLongOpClientRequest : CRmtClientRequest {
    public String pipe { get; set; }
    public String sessionUID { get; set; }
    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    //}

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as CLongOpClientRequest;
      dst.pipe = this.pipe;
      dst.sessionUID = this.sessionUID;
    }

  }
}
