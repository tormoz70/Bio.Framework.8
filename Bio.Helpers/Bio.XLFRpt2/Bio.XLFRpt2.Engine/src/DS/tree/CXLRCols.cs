namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
  using System.Collections.Generic;
  using System.Linq;

	/// <summary>
	/// 
	/// </summary>
	
	public enum XLRTTLType {xttNototal, xttRuntotal, xttSubtotal, xttFormula};
  public enum XLRTTLCalcType { xctSum, xctCnt, xctAvg, xctMax, xctMin, xctFormula };

	public class CXLRColDef{
//private
		private XLRTTLType FTTLType = XLRTTLType.xttNototal;
    private XLRTTLCalcType FTTLCalcType = XLRTTLCalcType.xctSum;
		private String FFieldName = null;
		private String FFormula = null;
    private String[] FGroupFieldNames = null;
    private int FColIndx = 0;
		private bool FIsGroupField = false;
    private bool FGroupFieldHasFooter = false;
    private bool FIsSysField = false;
//public
		//constructor
    public CXLRColDef(XLRTTLType pTTLType, XLRTTLCalcType pTTLCalcType, String pFieldName, int pColIndx, bool pIsGroupField, bool pGroupFieldHasFooter, bool pIsSysField, String pFormula) {
			this.FTTLType = pTTLType;
      this.FTTLCalcType = pTTLCalcType;
			this.FFieldName = pFieldName;
			this.FColIndx = pColIndx;
			this.FIsGroupField = pIsGroupField;
      this.FGroupFieldHasFooter = pGroupFieldHasFooter;
      this.FIsSysField = pIsSysField;
      this.FFormula = pFormula;
		}

		public XLRTTLType TTLType{
			get{
				return this.FTTLType;
			}
		}

    public XLRTTLCalcType TTLCalcType{
      get{
        return this.FTTLCalcType;
      }
    }

		public String FieldName{
			get{
				return this.FFieldName;
			}
		}

		public String Formula{
			get{
				return this.FFormula;
			}
		}
		
		public int ColIndex{
			get{
				return this.FColIndx;
			}
		}

		public bool IsGroupField{
			get{
				return this.FIsGroupField;
			}
		}

    public bool GroupFieldHasFooter {
      get {
        return this.FGroupFieldHasFooter;
      }
    }
    
    public bool IsSysField{
      get{
        return this.FIsSysField;
      }
    }
    public String[] GroupFieldNames {
      get {
        return this.FGroupFieldNames;
      }
      set {
        this.FGroupFieldNames = value;
      }
    }
  }


	public class CXLRColDefs{
//private
    private List<CXLRColDef> FCols = null;
//public
//constructor
		public CXLRColDefs(){
      this.FCols = new List<CXLRColDef>();
		}

		public void Clear(){
			this.FCols.Clear();
		}

		public CXLRColDef this[int index]{
			get{
				if((index >= 0) && (index < this.FCols.Count))
					return this.FCols[index];
				else
					return null;
			}
		}

    public CXLRColDef this[String fieldName] {
      get {
        //String vFieldName = null;
        //if(pFieldName != null)
        //  vFieldName = pFieldName.ToUpper();
        //if(vFieldName != null)
        //  for(int i = 0; i < this.Count; i++) {
        //    if(this[i].FieldName.ToUpper().Equals(vFieldName))
        //      return this[i];
        //  }
        //return null;
        var r = this.FCols.Where((c) => { return String.Equals(c.FieldName, fieldName, StringComparison.CurrentCultureIgnoreCase); });
        return r.FirstOrDefault();
      }
    }
		
    public CXLRColDef GetByColIndex(int colIndex){
      //foreach(var col in this.FCols) {
      //  if(col.ColIndex == pColIndex)
      //    return col;
      //}
      //return null;
      var r = this.FCols.Where((c) => { return c.ColIndex == colIndex; });
      return r.FirstOrDefault();
    }

		public int Count{
			get{
				return this.FCols.Count;
			}
		}

    public CXLRColDef Add(XLRTTLType pTTLType, XLRTTLCalcType pTTLCalcType, String pFieldName, bool pIsGroupField, bool pGroupFieldHasFooter, bool pIsSysField, String pFormula) {
      CXLRColDef vReslt = new CXLRColDef(pTTLType, pTTLCalcType, pFieldName, this.FCols.Count + 1, pIsGroupField, pGroupFieldHasFooter, pIsSysField, pFormula);
			this.FCols.Add(vReslt);
      return vReslt;
		}

    public CXLRColDef Add(XLRTTLType pTTLType, XLRTTLCalcType pTTLCalcType, String pFieldName, bool pIsGroupField, bool pGroupFieldHasFooter, bool pIsSysField) {
			return this.Add(pTTLType, pTTLCalcType, pFieldName, pIsGroupField, pIsSysField, pGroupFieldHasFooter, null);
		}
		
		public bool HasTotals{
			get{
				foreach(var col in this.FCols)
					if(col.TTLType != XLRTTLType.xttNototal)
						return true;
				return false;
			}
		}
		
		public CXLRColDef[] GetDetailsColDefs(){
			ArrayList vRslt = new ArrayList();
			foreach(var col in this.FCols){
				if(col.TTLType == XLRTTLType.xttNototal)
					vRslt.Add(col);
			}
			if(vRslt.Count > 0){
				CXLRColDef[] vArr = new CXLRColDef[vRslt.Count];
				vRslt.CopyTo(vArr);
				return vArr;
			}else
				return new CXLRColDef[0];
		}
		
	}
}
