namespace Bio.Helpers.XLFRpt2.Engine.XLRptParams {

	using System;
	using System.Xml;
	using System.Collections;
  using Bio.Helpers.Common.Types;
  using System.Text;
  //using Oracle.DataAccess.Client;
  //using Bio.Helpers.DOA;

	/// <summary>
	/// 
	/// </summary>
	public static class Exts{
    //private CXLReportConfig _owner;
		//public CXLRptParams(CXLReportConfig owner):base(){
		//	this._owner = owner;
		//}

		private static String prepareParamValue(this CParams prms, CParams inParams, String pParamText){
      String vRslt = pParamText;
      for (int i = 0; i < inParams.Count; i++)
        vRslt = vRslt.Replace("#" + inParams[i].Name + "#", inParams[i].ValueAsString());
			return vRslt;
		}

    public static void mergeFromInParams(this CParams prms, CParams inParams) {
      foreach (var prm in inParams) {
        if (prms.IndexOf(prm.Name) == -1)
          prms.Add(prm.Name, prm.Value);
      }
    }

    public static String getReportParameter(this CParams prms, CXLReport report, String pName) {
			String vResult = null;
      CParam vPrm = prms.ParamByName(pName);
      if (vPrm != null) {
        String vSQL = vPrm.ValueAsString();
        String vType = (String)vPrm.InnerObject;
        if (!String.IsNullOrEmpty(vType) && vType.Equals("sql") && report.RptDefinition.DBConnEnabled) {
          try {
            vResult = "" + report.DataFactory.GetScalarValue(report.currentDbConnection, vSQL, report.RptDefinition.InParams, 120);
          } catch (Exception ex) {
            throw new EBioException("Ошибка инициализации параметра отчета :" + pName + ":. Сообщение: " + ex.Message + ". SQL: " + vSQL, ex);
          } finally {
          }
        } else {
          vResult = prms.prepareParamValue(report.RptDefinition.InParams, vSQL);
        }
      }
			return vResult;
		}

    public static String AsXMLText(this CParams prms) {
      StringBuilder rslt = new StringBuilder();
      rslt.AppendLine("<params>");
      foreach (CParam p in prms)
        rslt.AppendLine(String.Format("<param name=\"{0}\"><![CDATA[{1}]]></param>", p.Name, p.ValueAsString()));
      rslt.AppendLine("</params>");
      return rslt.ToString();
    }

	}
}
