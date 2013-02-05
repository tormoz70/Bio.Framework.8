namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
  using Bio.Helpers.Common.Types;
  using System.Linq;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Collections.Generic;
#endif

	/// <summary>
	/// 
	/// </summary>
	
	public class CXLRGroups:DisposableObject{
//private
		private CXLRDataSet FOwner = null;
		private CXLRGroup FParentGroup = null;
    private List<CXLRGroup> FGroups = null;

//public
		//constructor
		public CXLRGroups(CXLRDataSet pOwner, CXLRGroup pParent){
			this.FParentGroup = pParent;
			this.FOwner = pOwner;
			this.FGroups = new List<CXLRGroup>();
		}

		protected override void doOnDispose(){
			foreach(var vGrp in this.FGroups)
				vGrp.Dispose();
			this.FGroups.Clear();
		}

		public CXLRDataSet Owner{
			get{
				return this.FOwner;
			}
		}

		public CXLRGroup ParentGroup{
			get{
				return this.FParentGroup;
			}
		}

		public CXLRGroup this[int index]{
			get{
				return FGroups[index];
			}
		}

		public int Count{
			get{
				return FGroups.Count;
			}
		}

		public XmlElement GetXml(XmlDocument pDoc){
			XmlElement vRslt = pDoc.CreateElement("childs");
			foreach(var vGrp in this.FGroups)
				vRslt.AppendChild(vGrp.GetXml(pDoc));
			return vRslt;
		}

    public void FillBuffer(Excel.Worksheet pWS, Object[,] pBuffer) {
			foreach(var vGrp in this.FGroups)
				vGrp.FillBuffer(pWS, pBuffer);
		}

    public void AppliayFormat(Excel.Range pDSRange) {
      foreach (var vGrp in this.FGroups) {
        vGrp.AppliayFormat(pDSRange);
      }
		}

    public void RefreshTTLFormula(Excel.Range pDSRange) {
      foreach (var vGrp in this.FGroups) {
        vGrp.RefreshTTLFormula(pDSRange);
      }
    }

    public CXLRGroup AddGroup() {
      CXLRGroup newGroup = new CXLRGroup(this.Owner, this.ParentGroup, this.FGroups.Count);
			this.FGroups.Add(newGroup);
			return newGroup;
		}

		public CXLRGroup LastChildGroup(Boolean force){
			if((this.FGroups.Count == 0) && force)
        return this.AddGroup();
      else
		    return this.FGroups.LastOrDefault();
		}

	}
}
