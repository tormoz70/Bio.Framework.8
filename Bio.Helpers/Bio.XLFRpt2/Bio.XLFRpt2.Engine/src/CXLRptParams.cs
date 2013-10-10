namespace Bio.Helpers.XLFRpt2.Engine.XLRptParams {

	using System;
	using Common.Types;
  using System.Text;

	/// <summary>
	/// 
	/// </summary>
	public static class Exts{

		private static String _prepareParamValue(Params inParams, String paramText) {
		  if (String.IsNullOrEmpty(paramText))
		    return paramText;
      foreach (var t in inParams)
        paramText = paramText.Replace("#" + t.Name + "#", t.ValueAsString());
      return paramText;
		}

    public static void MergeFromInParams(this Params prms, Params inParams) {
      foreach (var prm in inParams) {
        if (prms.IndexOf(prm.Name) == -1)
          prms.Add(prm.Name, prm.Value);
      }
    }

    public static String GetReportParameter(this Params prms, CXLReport report, String name) {
			String result = null;
      var prm = prms.ParamByName(name);
      if (prm != null) {
        var sql = prm.ValueAsString();
        var type = (String)prm.InnerObject;
        if (!String.IsNullOrEmpty(type) && type.Equals("sql") && report.RptDefinition.DBConnEnabled) {
          try {
            result = "" + report.DataFactory.GetScalarValue(report.currentDbConnection, sql, report.RptDefinition.InParams, 120);
          } catch (Exception ex) {
            throw new EBioException("Ошибка инициализации параметра отчета :" + name + ":. Сообщение: " + ex.Message + ". SQL: " + sql, ex);
          }
        } else {
          result = _prepareParamValue(report.RptDefinition.InParams, sql);
        }
      }
			return result;
		}

    public static String AsXMLText(this Params prms) {
      var rslt = new StringBuilder();
      rslt.AppendLine("<params>");
      foreach (var p in prms)
        rslt.AppendLine(String.Format("<param name=\"{0}\"><![CDATA[{1}]]></param>", p.Name, p.ValueAsString()));
      rslt.AppendLine("</params>");
      return rslt.ToString();
    }

	}
}
