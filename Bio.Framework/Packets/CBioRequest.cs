using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  
  public class CBioRequest : CBioRequestTyped {

    /// <summary>
    /// Код запрашиваемого инф. объекта
    /// </summary>
    public String bioCode { get; set; }

    /// <summary>
    /// параметры информационного объекта
    /// </summary>
    public Params bioParams { get; set; }

    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    //}

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      CBioRequest dst = destObj as CBioRequest;
      dst.bioCode = this.bioCode;
      dst.bioParams = (this.bioParams != null) ? (Params)this.bioParams.Clone() : null;
    }


  }


}
 