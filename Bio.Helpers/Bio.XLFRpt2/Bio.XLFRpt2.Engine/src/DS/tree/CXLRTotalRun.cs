namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;

	/// <summary>
	/// 
	/// </summary>
	public class CXLRTotalRun:CXLRTotal{
//private

//public
		//constructor
		public CXLRTotalRun(CXLRTotals pOwner, CXLRColDef pCol):base(pOwner, pCol){
		}

		public override String GetRange(){
			return this.Owner.Owner.GetAboveTotalRange() + new String(ExcelSrv.csRowRangesListDelimeter, 1) + this.Owner.Owner.GetTotalsRangesList();
		}

	}
}
