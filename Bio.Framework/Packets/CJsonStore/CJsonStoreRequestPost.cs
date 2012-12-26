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

  public class CJsonStoreRequestPost : CJsonStoreRequest {
    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    //}
  }


}
