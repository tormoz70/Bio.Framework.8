using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;
using System.ComponentModel;

namespace Bio.Framework.Packets {

  public enum CSQLTransactionCmd { Nop = 0, Commit = 1, Rollback = 2 };
  
  public class CBioSQLRequest : CBioRequest {
    /// <summary>
    /// Команда управления транзакцией
    /// </summary>
    public CSQLTransactionCmd transactionCmd { get; set; }

    /// <summary>
    /// ID транзакции при распределенных операциях с БД
    /// </summary>
    public String transactionID { get; set; }

    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    //}

    protected override void copyThis(ref CAjaxRequest destObj) {
      base.copyThis(ref destObj);
      CBioSQLRequest dst = destObj as CBioSQLRequest;
      dst.transactionCmd = this.transactionCmd;
      dst.transactionID = this.transactionID;
    }

  }


}
 