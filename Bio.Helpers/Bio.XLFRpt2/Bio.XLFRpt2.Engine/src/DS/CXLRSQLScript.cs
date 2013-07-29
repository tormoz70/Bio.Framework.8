namespace Bio.Helpers.XLFRpt2.Engine {

	using System;
	using System.Data;
	using System.Xml;
	using System.Collections;
	using System.IO;
  using System.Threading;
  using Bio.Helpers.Common.Types;
  using Bio;
  //using Bio.Helpers.DOA;
  using System.Data.Common;

	/// <summary>
	/// 
	/// </summary>
	public class CXLRSQLScript:DisposableObject{
//private
		private CXLReport FOwner = null;
    private IDbCommand FCurrentCmd = null;

//public
		//constructor
		public CXLRSQLScript(CXLReport pOwner){
			this.FOwner = pOwner;
		}

		public CXLReport Owner{
			get{
				return FOwner;
			}
		}

    //private String prepareSQL(String pSQL){
    //  String vRslt = pSQL;
    //  /*for(int i=0; i<this.Owner.RptDefinition.InParams.Count; i++)
    //    vRslt = vRslt.Replace("#"+this.Owner.RptDefinition.InParams[i].Name+"#", this.Owner.RptDefinition.InParams[i].Value);
    //  //SQLCSProcessor.ProcessSQLCSFunction(ref vRslt);*/
    //  return vRslt;
    //}

    public void Run(String pSQL){
      this.Run(pSQL, null);
    }

    public void Cancel() {
      if (this.FCurrentCmd != null) {
        this.FOwner.writeLogLine("CXLRSQLScript.Cancel - Start Canceling of CurrentCmd...");
        this.FCurrentCmd.Cancel();
        this.FOwner.writeLogLine("CXLRSQLScript.Cancel - CurrentCmd - Canceled.");
      } else {
        this.FOwner.writeLogLine("CXLRSQLScript.Cancel - Nothing to cancel.");
      }
    }

		public void Run(String sql, String prdFileNameDebug){
      try {
        if(this.FOwner.RptDefinition.DebugIsOn){
          if(prdFileNameDebug != null){
            Directory.CreateDirectory(Path.GetDirectoryName(prdFileNameDebug));
            StrFile.SaveStringToFile(prdFileNameDebug, sql, null); 
          }
        }
        this.FCurrentCmd = this.FOwner.DataFactory.PrepareCmd(this.FOwner.currentDbConnection, sql, this.Owner.RptDefinition.InParams, 60 * 30);
        this.FOwner.DataFactory.ExecCmd(this.FCurrentCmd);
      } finally {
        this.Cancel();
        this.FCurrentCmd = null;
      }
		}

	}
}
