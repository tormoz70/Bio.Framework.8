﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !SILVERLIGHT
using Bio.Helpers.Common;
#else
using Bio.Helpers.Common.Types;
#endif

namespace Bio.Framework.Packets {

  public enum JsonStoreSortOrder {
    [Description("NONE")]
    None = 0,
    [Description("ASC")]
    Asc = 1,
    [Description("DESC")]
    Desc = 2
  };

  public class JsonStoreSort : Dictionary<String, JsonStoreSortOrder>, ICloneable {
    public object Clone() {
      var rslt = new JsonStoreSort();
      foreach (var p in this) {
        rslt.Add(p.Key, p.Value);
      }
      return rslt;
    }

#if !SILVERLIGHT
    public String GetSQL() {
      String rslt = null;
      foreach (var s in this) {
        Utl.AppendStr(ref rslt, String.Format("{0} {1}", s.Key, enumHelper.GetFieldDesc(s.Value)), ",");
      }
      return rslt;
    }
#endif
  }

}
