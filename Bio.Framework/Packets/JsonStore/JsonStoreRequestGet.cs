using System;
namespace Bio.Framework.Packets {

  using System;
  using Bio.Helpers.Common.Types;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json;
  using System.Collections.Generic;
  using System.Xml;
  using System.ComponentModel;
  using System.Reflection;
  using Bio.Helpers.Common;
#if !SILVERLIGHT
  using System.Data;
  using System.Windows.Forms;
#endif

  public enum CJSRequestGetType { GetData = 0, GetSelectedPks = 1 };

  public class JsonStoreRequestGet : JsonStoreRequest {
    public CJSRequestGetType getType { get; set; }
    public String selection { get; set; }

    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    //}

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as JsonStoreRequestGet;
      dst.getType = this.getType;
      dst.selection = this.selection;
    }

  }


}
