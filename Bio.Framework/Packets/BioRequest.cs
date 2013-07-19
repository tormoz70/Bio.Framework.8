using System;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  
  public class BioRequest : BioRequestTyped {

    /// <summary>
    /// Код запрашиваемого инф. объекта
    /// </summary>
    public String BioCode { get; set; }

    /// <summary>
    /// параметры информационного объекта
    /// </summary>
    public Params BioParams { get; set; }

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as BioRequest;
      if (dst != null) {
        dst.BioCode = this.BioCode;
        dst.BioParams = (this.BioParams != null) ? (Params)this.BioParams.Clone() : null;
      }
    }


  }


}
 