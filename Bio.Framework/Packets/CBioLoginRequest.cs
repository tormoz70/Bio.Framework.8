using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Newtonsoft.Json;

namespace Bio.Framework.Packets {

  
  public class CBioLoginRequest : CBioRequestTyped {
    /// <summary>
    /// login
    /// </summary>
    public String login { get; set; }

    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter() };
    //}

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      CBioLoginRequest dst = destObj as CBioLoginRequest;
      dst.login = this.login;
    }

  }


}
 