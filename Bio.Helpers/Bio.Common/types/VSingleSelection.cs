using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Helpers.Common.Types {
  public class VSingleSelection:VSelection {
    public override Object Value { get; set; }
    public override Boolean IsEmpty() {
      return this.Value == null;
    }
    public CRTObject ValueRow { get; set; }
    public override bool Equals(object obj) {
      var v_obj = obj as VSingleSelection;
      var v_rslt = v_obj != null;
      v_rslt = v_rslt && (v_obj.Value == this.Value);
      //for(int i=0; i<this.Values.Length; i++)
      //  v_rslt = v_rslt && (this.Values[i] == v_obj.Values[i]);
      return v_rslt;
    }
    public override int GetHashCode() {
      return base.GetHashCode();
    }
  }
}
