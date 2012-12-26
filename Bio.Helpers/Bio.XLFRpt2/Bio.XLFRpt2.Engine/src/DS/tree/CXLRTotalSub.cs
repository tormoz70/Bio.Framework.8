namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;

	/// <summary>
	/// 
	/// </summary>
	public class CXLRTotalSub:CXLRTotal{
//private

//public
		//constructor
		public CXLRTotalSub(CXLRTotals pOwner, CXLRColDef pCol):base(pOwner, pCol){
		}

		public override String GetRange(){
			return this.Owner.Owner.GetTotalsRangesList();
		}
	}
}
