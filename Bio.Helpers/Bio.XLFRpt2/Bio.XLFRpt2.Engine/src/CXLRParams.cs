namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Xml;
	using System.Collections;
  using Bio.Helpers.Common.Types;
  using System.Text;

	/// <summary>
	/// 
	/// </summary>
	/*public class TXLRParam{
		private String FName;
		private String FValue;
		private Object FInnerObject;
		private TXLRParams FOwner;

		public TXLRParam(TXLRParams pOwner, String pName, String pValue){
			FOwner = pOwner;
			FName = pName;
			FValue = pValue;
		}

		public TXLRParam(TXLRParams pOwner, String pName, String pValue, Object pObject){
			FOwner = pOwner;
			FName = pName;
			FValue = pValue;
			FInnerObject = pObject;
		}

		public String Name{
			get{
				return FName;
			}
		}

		public String Value{
			get{
				return FValue;
			}
			set{
				FValue = value;
			}
		}

		public Object InnerObject{
			get{
				return FInnerObject;
			}
			set{
				FInnerObject = value;
			}
		}

		public int ParamIndex{
			get{
				int Result = -1;
				for (int i = 0; i < FOwner.Count; i++){
					if (FOwner[i].Name.Equals(FName)){
						Result = i;
						break;
					}
				}
				return Result;
			}
		}

	}*/
	
	//public class CXLRParams: Params{
		/*private void  InitBlock(){
			FParams = new ArrayList();
		}
		
		private ArrayList FParams;
		
		public TXLRParams(){
			InitBlock();
		}

		public static void ParsInParams(String vPars, TXLRParams vParams){
			if((vPars != null)&&(vParams != null)){
				vParams.Clear();
				if(!vPars.Equals("reset")){
					String[] lst = utlSTD.SplitString(vPars, ';');
					for (int i = 0; i < lst.Length; i++){
						String fpar = lst[i];
            String[] ppp = utlSTD.SplitString(fpar, '=');
						String pName = null;
						String pValue = null;
						if (ppp.Length > 0)
							pName = ppp[0];
						if (ppp.Length > 1)
							pValue = ppp[1];
						vParams.Add(pName, pValue);
					}
				}
			}
		}
		
		public TXLRParam this [int index]{
			get{
				if((index >= 0) && (index < FParams.Count))
					return (TXLRParam)FParams[index];
				else
					return null;
			}
		}

		public void  Add(String vName, String vValue, Object vObject){
			TXLRParam exPar = this.ParamByName(vName);
			if(exPar != null){
				FParams.RemoveAt(exPar.ParamIndex);
			}
			TXLRParam newParam = new TXLRParam(this, vName, vValue, vObject);
			FParams.Add(newParam);
		}

		public void  Add(String vName, String vValue){
			this.Add(vName, vValue, null);
		}

		public void  AddObject(String vName, Object vObject){
			this.Add(vName, null, vObject);
		}

		public TXLRParam ParamByName(String vName){
			TXLRParam Result = null;
			for (int i = 0; i < FParams.Count; i++){
				TXLRParam curParam = (TXLRParam) FParams[i];
				if (curParam.Name.Equals(vName)){
					Result = curParam;
					break;
				}
			}
			return Result;
		}
		
		public void  Clear(){
			FParams.Clear();
		}
		
		public int Count{
			get{
				return FParams.Count;
			}
		}*/
  //  public String AsXMLText() { 
  //    StringBuilder rslt = new StringBuilder();
  //    rslt.AppendLine("<params>");
  //    foreach (Param p in this) 
  //      rslt.AppendLine(String.Format("<param name=\"{0}\"><![CDATA[{1}]]></param>", p.Name, p.ValueAsString()));
  //    rslt.AppendLine("</params>");
  //    return rslt.ToString();
  //  }
  //}
}
