namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Xml;
	using System.Xml.XPath;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

	/// <summary>
	/// 
	/// </summary>
	public class CXLRMacro:DisposableObject{
//private
		private String FName = null;
    private Params FParams = null;

//public
		//constructor
		public CXLRMacro(XmlElement definition){
      this.FParams = new Params();
      if (definition != null) {
        Boolean v_enabled = Xml.getAttribute<Boolean>(definition, "enabled", true);
        if (v_enabled) {
          this.FName = Xml.getAttribute<String>(definition, "name", null);
          XmlNodeList vParams = definition.SelectNodes("param");
          foreach (XmlElement e in vParams)
            this.FParams.Add(e.GetAttribute("name"), e.InnerText);
        }
      }
		}

    public String Name {
      get {
        return this.FName;
      }
    }

    public Params Params {
      get {
        return this.FParams;
      }
		}
	}
}
