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
	
	public class CXLRDetails:CXLRGroup{
//private
		private ArrayList FDetailRows = null;
//public
		//constructor
		public CXLRDetails(CXLRDataSet pOwner, CXLRGroup pParent, int pTopRow):base(pOwner, pParent, 0){
			this.FTopRow = pTopRow;
			this.FDetailRows = new ArrayList();
			if(this.RootGroup.FirstDetalsRow == -1){
				this.RootGroup.FirstDetalsRow = this.TopRow;
			}
		}

		protected override void OnDispose(){
			this.FDetailRows.Clear();
		}

		public int Count{
			get{
				return this.FDetailRows.Count;
			}
		}

		public override XmlElement GetXml(XmlDocument pDoc){
			CXLRColDefs vCols = this.RootGroup.ColDefs;
			XmlElement vRslt = pDoc.CreateElement("details");
			vRslt.SetAttribute("FTopRow", ""+this.TopRow); 
			for(int i=0; i<this.FDetailRows.Count; i++){
				XmlElement vRow = pDoc.CreateElement("row");
				vRow.SetAttribute("row", ""+(i+1));
				vRslt.AppendChild(vRow);
				Object[] vCurVals = (Object[])this.FDetailRows[i];
				for(int j=0; j<vCols.Count; j++){
					String vVal = "null";
					if(vCurVals[j] != null)
						vVal = vCurVals[j].ToString();
					String vNm = "null";
					if(vCols[j].FieldName != null)
						vNm = vCols[j].FieldName;
					vRow.SetAttribute(vNm, vVal);
				}
			}
			return vRslt;
		}

    public void Colaps() { 
    }

    public override void FillBuffer(Excel.Worksheet pWS, Object[,] pBuffer) {
			for(int i=0; i<this.FDetailRows.Count; i++){
				Object[] vCurVals = (Object[])this.FDetailRows[i];
				this.RootGroup.AddRowToBuffer(pBuffer, vCurVals);
			}
		}

		public Object[] GetRow(int pIndex){
			if((pIndex >= 0) && (pIndex < this.FDetailRows.Count))
				return (Object[])this.FDetailRows[pIndex];
			else
				return null;
		}

    public override void DoOnFetch(long rownum, CXLRDataRow row) {
			CXLRColDefs vCols = this.RootGroup.ColDefs;
			Object[] vDetals = new Object[vCols.Count];
			for(int i=0; i<vCols.Count; i++){
				Object vCurDT = null;
        var v_leaveGroupData = this.Owner.Owner.Cfg.leaveGroupData;
        if (!v_leaveGroupData && vCols[i].IsGroupField)
          vCurDT = null;
        else{
          if(vCols[i].IsSysField){
            if(vCols[i].FieldName.Equals("RNUM"))
            vCurDT = new Decimal(rownum);
          }else
            vCurDT = this.RootGroup.GetData(i, row);
        }
				vDetals[i] = vCurDT;
			}
			this.FDetailRows.Add(vDetals);
			this.DoOnRowsInsert(1);
		}

	}
}
