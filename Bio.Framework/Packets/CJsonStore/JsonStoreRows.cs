using System;
using System.Collections.Generic;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {
  public enum JsonStoreRowChangeType {
    //[Description("n")]
    Unchanged = 0,
    //[Description("a")]
    Added = 1,
    //[Description("u")]
    Modified = 2,
    //[Description("d")]
    Deleted = 3
  };
  public class JsonStoreRow : ICloneable {
    public JsonStoreRow() { this.Values = new List<Object>(); }
    public String InternalROWUID { get; set; }
    public JsonStoreRowChangeType ChangeType { get; set; }
    public List<Object> Values { get; set; }
    public object Clone() {
      var newRow = new JsonStoreRow { InternalROWUID = this.InternalROWUID, ChangeType = this.ChangeType };
      foreach (var val in this.Values) {
        newRow.Values.Add((val is ICloneable) ? (val as ICloneable).Clone() : val);
      }
      return newRow;
    }
  }

  public class JsonStoreRows : List<JsonStoreRow>, ICloneable {
    public void CopyFrom(JsonStoreRows rows) {
      this.Clear();
      foreach (var row in rows) {
        this.Add((JsonStoreRow)row.Clone());
      }
    }
    
    public object Clone() {
      var rslt = new JsonStoreRows();
      rslt.CopyFrom(this);
      return rslt;
    }
  }

}
