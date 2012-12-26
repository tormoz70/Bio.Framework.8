using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
#if !SILVERLIGHT
using System.Windows.Forms;
#endif
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;

namespace Bio.Framework.Packets {

  public enum CJsonStoreSortOrder {
    [Description("NONE")]
    None = 0,
    [Description("ASC")]
    Asc = 1,
    [Description("DESC")]
    Desc = 2
  };

  public class CJsonStoreSort : Dictionary<String, CJsonStoreSortOrder>, ICloneable {
    //[DefaultValue(null)]
    //public String fieldName { get; set; }
    //[DefaultValue(CJsonStoreSortOrder.None)]
    //public CJsonStoreSortOrder direction { get; set; }
    public object Clone() {
      CJsonStoreSort rslt = new CJsonStoreSort();
      foreach (KeyValuePair<String, CJsonStoreSortOrder> p in this) {
        rslt.Add(p.Key, p.Value);
      }
      return rslt;
    }

#if !SILVERLIGHT
    public String GetSQL() {
      String rslt = null;
      foreach (var s in this) {
        Utl.appendStr(ref rslt, String.Format("{0} {1}", s.Key, enumHelper.GetFieldDesc(s.Value)), ",");
      }
      return rslt;
    }
#endif
//#if !SILVERLIGHT
//    public void SetDirection(SortOrder pDir) {
//      //if (typeof(SortOrder) == typeof(T)) {
//      SortOrder dir = pDir;
//      switch (dir) {
//        case SortOrder.None: this.direction = CJsonStoreSortOrder.None; break;
//        case SortOrder.Ascending: this.direction = CJsonStoreSortOrder.Asc; break;
//        case SortOrder.Descending: this.direction = CJsonStoreSortOrder.Desc; break;
//      }
//      //}
//    }
//#endif
  }

  /*public class CJsonStoreNav : ICloneable {
    [DefaultValue(null)]
    public String locate { get; set; }
    public object Clone() {
      return new CJsonStoreNav() {
        locate = this.locate
      };
    }

  }*/
}
