using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Newtonsoft.Json;

namespace Bio.Framework.Packets {

  
  public class BioLoginRequest : BioRequestTyped {
    /// <summary>
    /// login
    /// </summary>
    public String login { get; set; }

    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter() };
    //}

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      BioLoginRequest dst = destObj as BioLoginRequest;
      dst.login = this.login;
    }

  }


}
 