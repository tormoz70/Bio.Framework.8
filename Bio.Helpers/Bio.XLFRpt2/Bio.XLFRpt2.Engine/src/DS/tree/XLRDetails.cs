namespace Bio.Helpers.XLFRpt2.Engine {
  using System;
	using System.Xml;
	using System.Collections;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;

#endif

	/// <summary>
	/// Детали в группе
	/// </summary>
	
	public class XLRDetails:CXLRGroup{
		private readonly ArrayList _detailRows;
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="pOwner"></param>
		/// <param name="pParent"></param>
		/// <param name="pTopRow"></param>
		public XLRDetails(CXLRDataSet pOwner, CXLRGroup pParent, int pTopRow):base(pOwner, pParent, 0){
			this.FTopRow = pTopRow;
			this._detailRows = new ArrayList();
			if(this.RootGroup.FirstDetalsRow == -1){
				this.RootGroup.FirstDetalsRow = this.TopRow;
			}
		}

	  /// <summary>
	  /// Кол-во элементов
	  /// </summary>
	  public int Count{
			get{
				return this._detailRows.Count;
			}
		}

	  protected override void doOnDispose(){
	    this._detailRows.Clear();
	  }

	  public override XmlElement GetXml(XmlDocument pDoc){
			var vCols = this.RootGroup.ColDefs;
			var vRslt = pDoc.CreateElement("details");
			vRslt.SetAttribute("FTopRow", ""+this.TopRow); 
			for(var i=0; i<this._detailRows.Count; i++){
				var vRow = pDoc.CreateElement("row");
				vRow.SetAttribute("row", ""+(i+1));
				vRslt.AppendChild(vRow);
				var vCurVals = (Object[])this._detailRows[i];
				for(var j=0; j<vCols.Count; j++){
					var vVal = "null";
					if(vCurVals[j] != null)
						vVal = vCurVals[j].ToString();
					var vNm = "null";
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
			for(var i=0; i<this._detailRows.Count; i++){
				var vCurVals = (Object[])this._detailRows[i];
				this.RootGroup.AddRowToBuffer(pBuffer, vCurVals);
			}
		}

		public Object[] GetRow(int pIndex){
			if((pIndex >= 0) && (pIndex < this._detailRows.Count))
				return (Object[])this._detailRows[pIndex];
			else
				return null;
		}

    public override void DoOnFetch(long rownum, CXLRDataRow row) {
			var vCols = this.RootGroup.ColDefs;
			var vDetals = new Object[vCols.Count];
			for(var i=0; i<vCols.Count; i++){
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
			this._detailRows.Add(vDetals);
			this.DoOnRowsInsert(1);
		}

	}
}
