namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
  using Bio.Helpers.Common.Types;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Collections.Generic;
  using Bio.Helpers.Common;
#endif

	/// <summary>
	/// 
	/// </summary>

	public delegate void DlgOnParentRowsInsertEvent(int pRowsInserted);
	
	public class CXLRGroup: CDisposableObject{
//private
		protected int FLeftCol = 0;
		protected int FTopRow = 0;
		private int FRowsCount = 0;

		protected CXLRDataSet FOwner = null;
		protected String FGroupKeyField = null;
		protected Object FGroupKeyValue = null;
		protected CXLRGroup FParentGroup = null;
		protected CXLRGroups FChildGroups = null;
		protected CXLRDetails FDetails = null;
		protected CXLRTotals FTotals = null;
		protected int FGrpIndex = -1;
		protected event DlgOnParentRowsInsertEvent FOnParentRowsInsert;

//public
		//constructor
		private CXLRGroup getPrevGroup(){
			if(this.FGrpIndex > 0) 
				return this.ParentGroup.FChildGroups[this.FGrpIndex-1];
			else
				return null;
		}

    public CXLRGroup(CXLRDataSet pOwner, CXLRGroup pParent, int pGrpIndex) {
			this.FGrpIndex = pGrpIndex;
			this.FOwner = pOwner;
			this.FParentGroup = pParent;
			if(this.ParentGroup != null){
				this.FOnParentRowsInsert += new DlgOnParentRowsInsertEvent(this.ParentGroup.DoOnParentRowsInsertEvent);
				if(this.GetType() == typeof(CXLRGroup)){
					this.DoOnRowsInsert(1);
					this.FRowsCount++;
					if(this.ParentGroup.IsRootGroup){
						this.FLeftCol = 2;
					}else{
						this.FLeftCol = this.ParentGroup.LeftCol + 1;
					}
					CXLRGroup vPrevGrp = this.getPrevGroup();
					if(vPrevGrp != null)
						this.FTopRow = vPrevGrp.BottomRow + 1;
					else{
						if(this.ParentGroup.IsRootGroup)
							this.FTopRow = this.ParentGroup.TopRow;
						else
							this.FTopRow = this.ParentGroup.TopRow + 1;
					}
          CXLRColDef vCol = this.RootGroup.ColDefs.GetByColIndex(this.FLeftCol);
          String[] vGrps = vCol.GroupFieldNames;
					this.FGroupKeyField = null;
					if(vGrps.Length > 0)
						this.FGroupKeyField = vGrps[0];
					if(this.RootGroup.ColDefs.HasTotals){
            CXLRColDef vGrpColDef = this.RootGroup.ColDefs[this.FGroupKeyField];
            if((this.FGroupKeyField != null) && (vGrpColDef != null) && (vGrpColDef.GroupFieldHasFooter))
						this.FTotals = new CXLRTotals(this);
					}
				}
			}
		}

		protected override void OnDispose(){
			if(this.ChildGroups(false) != null)
				this.ChildGroups(false).Dispose();
			if(this.GroupDetails(false) != null)
				this.GroupDetails(false).Dispose();
		}

		public String GroupKeyField{
			get{
				return this.FGroupKeyField;
			}
		}

		//  обрабатывает событие при добавлении строк в дочерних группах
		//  и инициирует такое же событие у родителя
		protected virtual void DoOnParentRowsInsertEvent(int pRowsInserted){
			this.FRowsCount += pRowsInserted;
			this.DoOnRowsInsert(pRowsInserted);
		}

		//  инициирует событие OnParentRowsInsertEvent в родительской группе
		//  при добавлении строк в дочерних объектах
		protected void DoOnRowsInsert(int pRowsInserted){
			if(this.FOnParentRowsInsert != null)
				this.FOnParentRowsInsert(pRowsInserted);
		}

		//  инициирует событие OnParentRowsInsertEvent на данном уровне
		//  и в родительской группе
		//  при добавлении строк дочерним объектом, не являющимся потомком TXLRGroup 
		public void DoOnChildRowsInsert(int pRowsInserted){
			this.FRowsCount += pRowsInserted;
			DoOnRowsInsert(pRowsInserted);
		}

		public CXLRootGroup RootGroup{
			get{
				if(this.ParentGroup == null)
					return (CXLRootGroup)this;
				else
					return this.ParentGroup.RootGroup;
			}
		}

		public CXLRGroup ParentGroup{
			get{
				return FParentGroup;
			}
		}

		public CXLRGroups ChildGroups(bool pForce){
			if((this.FChildGroups == null) && (pForce))
				this.FChildGroups = new CXLRGroups(this.Owner, this);
			return this.FChildGroups;
		}

		public int LeftCol{
			get{
				return this.FLeftCol;
			}
		}
		public int TopRow{
			get{
				return this.FTopRow;
			}
		}
		
		public virtual int TopRowOffset{
			get{
        int vOffset = this.RootGroup.TopRowOffset - 1;
				return this.TopRow + vOffset;
			}
		}

		public int RowsCount{
			get{
				return this.FRowsCount;
			}
		}
		public int BottomRow{
			get{
				return this.TopRow + this.RowsCount - 1;
			}
		}

		public int BottomRowOffset{
			get{
        int vOffset = this.RootGroup.TopRowOffset - 1;
				return this.BottomRow + vOffset;
			}
		}

		public CXLRDataSet Owner{
			get{
				return this.FOwner;
			}
		}

    public int GroupLevel {
      get {
        if(this.FParentGroup == null)
          return 0;
        else {
          return this.FParentGroup.GroupLevel + 1;
        }
      }
    }

		public bool IsRootGroup{
			get{
				return this.FParentGroup == null;
			}
		}

		public CXLRGroup LastChildGroup(bool pForce){
			return this.ChildGroups(pForce).LastChildGroup(pForce);
		}

    public bool HasChildGroups {
      get {
        return (this.ChildGroups(false) != null) && (this.ChildGroups(false).Count > 0);
      }
    }

		public CXLRDetails GroupDetails(bool pForce){
			if((this.FDetails == null) && (pForce)){
				int vTopRow = this.TopRow;
				if(!this.IsRootGroup)
					vTopRow++;
				this.FDetails = new CXLRDetails(this.Owner, this, vTopRow);
			}
			return this.FDetails;
		}

		// собирает диапазоны для заголовков данной группы
		public String GetKeysRangesList(String pGroupField){
			String vRslt = "";
			for(int i=0; i<this.ChildGroups(false).Count; i++){
				if(this.ChildGroups(false)[i].GroupKeyField.Equals(pGroupField)){
					int vTopRow = this.ChildGroups(false)[i].TopRowOffset;
          String vCurRng = vTopRow + new String(CExcelSrv.csRowRangetDelimeter, 1) + vTopRow;
					if(vRslt.Equals(""))
						vRslt = vCurRng;
					else
						vRslt += new String(CExcelSrv.csRowRangesListDelimeter, 1) + vCurRng;
				}else
					this.ChildGroups(false)[i].GetKeysRangesList(pGroupField);
			}
			return vRslt;
		}

		public String GetTotalsRange(){
			String vRslt = "";
			if(this.FTotals != null){
        vRslt = this.BottomRowOffset + new String(CExcelSrv.csRowRangetDelimeter, 1) + this.BottomRowOffset;
			}
			return vRslt;
		}

		// собирает диапазоны данных для данной группы
		public String GetDetailsRangesList(){
			String vRslt = "";
			if(this.ChildGroups(false) != null){
				for(int i=0; i<this.ChildGroups(false).Count; i++){
					if(vRslt.Equals(""))
						vRslt += this.ChildGroups(false)[i].GetDetailsRangesList();
					else
						vRslt += new String(CExcelSrv.csRowRangesListDelimeter, 1) + this.ChildGroups(false)[i].GetDetailsRangesList();
				}
			}else{
				if(this.GroupDetails(false) != null){
					int vTopRow = this.GroupDetails(false).TopRowOffset;
					int vRowCount = this.GroupDetails(false).Count;
					int vBottomRow = vTopRow + (vRowCount - 1);
					vRslt = vTopRow + new String(CExcelSrv.csRowRangetDelimeter, 1) + vBottomRow;
				}
			}
			return vRslt;
		}

		// собирает диапазоны totals для данной группы групп
		public String GetTotalsRangesList(){
			if(this.GroupDetails(false) != null)
				return this.GetDetailsRangesList();
			else if(this.ChildGroups(false) != null){
				String vResult = null;
				for(int i=0; i<this.ChildGroups(false).Count; i++)
          Utl.AddObjToLine(ref vResult, new String(CExcelSrv.csRowRangesListDelimeter, 1), this.ChildGroups(false)[i].GetTotalsRange());
        return Utl.NullToBlank(vResult);
			}else
				return "";
		}


		// собирает диапазоны данных для данной группы и ее предшественников на данном уровне
		public String GetAboveDetailsRangesList(){
			String vRslt = "";
			CXLRGroup vPrvGrp = this;
			while((vPrvGrp = vPrvGrp.getPrevGroup()) != null){
				if(vRslt.Equals(""))
					vRslt = vPrvGrp.GetDetailsRangesList();
				else
					vRslt = vPrvGrp.GetDetailsRangesList()+ new String(CExcelSrv.csRowRangesListDelimeter, 1) + vRslt;
			}
			if(vRslt.Equals(""))
				return this.GetDetailsRangesList();
			else
				return vRslt + new String(CExcelSrv.csRowRangesListDelimeter, 1) + this.GetDetailsRangesList();
		}

		// выбирает диапазон totals для предшественника на данном уровне
		public String GetAboveTotalRange(){
			String vRslt = "";
			CXLRGroup vPrvGrp = this.getPrevGroup();
			if(vPrvGrp != null)
				vRslt = vPrvGrp.GetTotalsRange();
			return vRslt;
		}

    public void doFetch(long rownum, CXLRDataRow row) {
			Object vCurGroupKey = row[this.FGroupKeyField] ?? "<пусто>";
      //if (vCurGroupKey != null) {
        if (!vCurGroupKey.Equals(this.FGroupKeyValue)) {
          this.ParentGroup.ChildGroups(true).AddGroup();
          this.ParentGroup.LastChildGroup(true).DoOnFetch(rownum, row);
        } else {
          CXLRColDef vCol = this.RootGroup.ColDefs.GetByColIndex(this.LeftCol + 1);
          String[] vChldGrps = vCol.GroupFieldNames;
          if (vChldGrps.Length > 0)
            this.LastChildGroup(true).DoOnFetch(rownum, row);
          else
            this.GroupDetails(true).DoOnFetch(rownum, row);
        }
      //} else
      //  throw new Exception(String.Format("Поле группировки {0} не может быть пустым!", this.FGroupKeyField));
		}

		public virtual XmlElement GetXml(XmlDocument pDoc){
			XmlElement vRslt = pDoc.CreateElement("group");
			vRslt.SetAttribute("FLeftCol", ""+this.LeftCol); 
			vRslt.SetAttribute("FTopRow", ""+this.TopRow); 
			vRslt.SetAttribute("FBottomRow", ""+this.BottomRow); 
			String vVal = "null";
			if(this.FGroupKeyValue != null)
				vVal = this.FGroupKeyValue.ToString();
			String vNm = "null";
			if(this.FGroupKeyField != null)
				vNm = this.FGroupKeyField;
			vRslt.SetAttribute(vNm, vVal);
			if(this.GroupDetails(false) != null)
				vRslt.AppendChild(this.GroupDetails(false).GetXml(pDoc));
			else{
				if(this.ChildGroups(false) != null)
					vRslt.AppendChild(this.ChildGroups(false).GetXml(pDoc));
			}
			if(this.FTotals != null){
				vRslt.AppendChild(this.FTotals.GetXml(pDoc));
			}
			return vRslt;
		}

    public virtual void GroupChild(Excel.Worksheet ws) {
      CXLRDetails dtls = this.GroupDetails(false);
      if (dtls != null) {
        CExcelSrv.getRange(ws, ws.Rows[dtls.TopRowOffset], ws.Rows[dtls.TopRowOffset + dtls.Count - 1]).Group();
      } else {
        if (!(this is CXLRootGroup)) {
          CExcelSrv.getRange(ws, ws.Rows[this.TopRowOffset + 1], ws.Rows[this.BottomRowOffset - 1]).Group();
        }
      }
      CXLRGroups grps = this.ChildGroups(false);
      if (grps != null) {
        for (int i = 0; i < grps.Count; i++) {
          grps[i].GroupChild(ws);
        }
      }
    }

		public virtual void FillBuffer(Excel.Worksheet ws, Object[,] pBuffer){
			Object[] vGrpHeader = new Object[this.RootGroup.ColDefs.Count];
			vGrpHeader[this.FLeftCol-1] = this.FGroupKeyValue;
			this.RootGroup.AddRowToBuffer(pBuffer, vGrpHeader);
      CXLRDetails dtls = this.GroupDetails(false);
      if (dtls != null) {
        dtls.FillBuffer(ws, pBuffer);
      } else {
        if (this.ChildGroups(false) != null)
          this.ChildGroups(false).FillBuffer(ws, pBuffer);
      }
			if(this.FTotals != null){
        this.FTotals.FillBuffer(ws, pBuffer);
			}
		}

		public virtual void AppliayFormat(Excel.Range dsRange){
      //throw new bioEx.bioSysError("AppliayFormat!!!");
      Excel.Range vRngSrc = this.RootGroup.GRTTmplDef.HeaderFormats[this.FGroupKeyField];
      Excel.Range vDetRng = this.RootGroup.GRTTmplDef.DetailsRng;
      Excel.Range vRngDst = CExcelSrv.getRange(dsRange.Worksheet, vDetRng.Cells[this.TopRow, 1], vDetRng.Cells[this.TopRow, vDetRng.Columns.Count]);
      vRngSrc.Copy(Type.Missing);
			vRngDst.PasteSpecial(Excel.XlPasteType.xlPasteFormats, Excel.XlPasteSpecialOperation.xlPasteSpecialOperationNone, false, false);
			if(this.ChildGroups(false) != null)
        this.ChildGroups(false).AppliayFormat(dsRange);
			if(this.FTotals != null){
        this.FTotals.AppliayFormat(dsRange);
			}
		}

    public virtual void RefreshTTLFormula(Excel.Range dsRange) {
      var v_chlds = this.ChildGroups(false);
      if (v_chlds != null)
        this.ChildGroups(false).RefreshTTLFormula(dsRange);
      if (this.FTotals != null) {
        this.FTotals.RefreshFormula(dsRange);
      }
    }

    public virtual void DoOnFetch(long rownum, CXLRDataRow row) {
			if(this.FGroupKeyField != null){
				if(this.FGroupKeyValue == null){ // первый вход в процедуру объекта
					this.FGroupKeyValue = row[this.FGroupKeyField] ?? "<пусто>";
				}
				this.doFetch(rownum, row);
			}else
        this.GroupDetails(true).DoOnFetch(rownum, row);
		}

		private CXLRGroup findGroupInChilds(String pFldName, String pGrpKey){
			if(this.ChildGroups(false) != null){
				for(int i=0; i<this.ChildGroups(false).Count; i++){
					CXLRGroup vRslt = this.ChildGroups(false)[i].FindGroup(pFldName, pGrpKey);
					if(vRslt != null)
						return vRslt;
				}
				return null;
			}else
				return null;
		}

		public CXLRGroup FindGroup(String pFldName, String pGrpKey){
			if((this.GroupKeyField != null) && (this.FGroupKeyValue != null)){
				if((this.GroupKeyField.Equals(pFldName) && (this.FGroupKeyValue.Equals(pGrpKey))))
					return this;
				else 
					return findGroupInChilds(pFldName, pGrpKey);
			}else 
				return findGroupInChilds(pFldName, pGrpKey);
		}

	}
}
