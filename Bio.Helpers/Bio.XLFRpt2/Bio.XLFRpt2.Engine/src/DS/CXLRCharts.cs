namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Xml;
	using System.Xml.XPath;
	using System.Collections;
  using Bio.Helpers.Common.Types;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
	/// 
	/// </summary>

  public class CXLRCharts:DisposableObject{

    private CXLRDataSource FOwner = null;
    private List<CXLRChart> FCharts;

		public CXLRCharts(CXLRDataSource pOwner){
			this.FOwner = pOwner;
      this.FCharts = new List<CXLRChart>();
		}

		public CXLRDataSource Owner{
			get{
				return this.FOwner;
			}
		}

		public void Pars(XmlElement pCharts){
			if(pCharts != null){
				XmlNodeList vChrts = pCharts.SelectNodes("chart");
				for(int i=0; i<vChrts.Count; i++)
					if(!((XmlElement)vChrts[i]).GetAttribute("enabled").Equals("false"))
						this.FCharts.Add(new CXLRChart(this, (XmlElement)vChrts[i]));
			}
		}
		
		public CXLRChart this[int index]{
			get{
				if((index >= 0) && (index < this.FCharts.Count))
					return this.FCharts[index];
				else
					return null;
			}
		}

		public CXLRChart ChartByName(String chartName){
      //CXLRChart Result = null;
      //for (int i = 0; i < this.FCharts.Count; i++){
      //  CXLRChart curChart = (CXLRChart)this.FCharts[i];
      //  if (curChart.Name.Equals(vName)){
      //    Result = curChart;
      //    break;
      //  }
      //}
      //return Result;
      var r = this.FCharts.Where((c) => { return String.Equals(c.Name, chartName, StringComparison.CurrentCultureIgnoreCase); });
      return r.FirstOrDefault();
    }
		
		public void  Clear(){
			this.FCharts.Clear();
		}
		
		public int Count{
			get{
				return this.FCharts.Count;
			}
		}

		public void Build(CXLRootGroup pRoot){
			foreach(var c in this.FCharts)
				c.Build(pRoot);
		}
	}

}
