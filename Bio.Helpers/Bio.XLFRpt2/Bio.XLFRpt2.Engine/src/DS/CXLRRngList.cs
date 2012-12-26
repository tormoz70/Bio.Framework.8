namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Collections.Generic;
#endif
	/// <summary>
	/// 
	/// </summary>
  public class CXLRRngItem {
    public String RangeName = null;
    public Excel.Range Range = null;
  }

  public class CXLRRngList : List<CXLRRngItem> {
//private
		//private ArrayList FList = null;

//public
		//constructor
    public CXLRRngList():base() {
			//this.FList = new ArrayList();
		}

    public void Add(String rngName, Excel.Range rng) {
			CXLRRngItem vItem = new CXLRRngItem();
      vItem.RangeName = rngName;
			vItem.Range = rng;
			this.Add(vItem);
		}

    //public Excel.Range Range(int index) {
    //  Excel.Range vRslt = null;
    //  if ((index >= 0) && (index < FList.Count))
    //    vRslt = ((CXLRRngItem)FList[index]).Data;
    //  return vRslt;
    //}

    public String getRangeName(int index) {
      String vRslt = null;
      if ((index >= 0) && (index < this.Count))
        vRslt = ((CXLRRngItem)this[index]).RangeName;
      return vRslt;
    }

		public bool RangeExists(String name){
			for(int i=0; i<this.Count; i++){
				CXLRRngItem vItem = (CXLRRngItem)this[i];
        if (vItem.RangeName.Equals(name))
					return true;
			}
			return false;
		}

    public Excel.Range this[String name] {
			get{
				for(int i=0; i<this.Count; i++){
					CXLRRngItem vItem = (CXLRRngItem)this[i];
          if (vItem.RangeName.Equals(name))
						return vItem.Range;
				}
				return null;
			}
		}

	}
}
