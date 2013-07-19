using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;
using System.ComponentModel;

namespace Bio.Framework.Packets {

  public enum SQLTransactionCmd { Nop = 0, Commit = 1, Rollback = 2 };
  
  public class BioSQLRequest : BioRequest {
    /// <summary>
    /// Команда управления транзакцией
    /// </summary>
    public SQLTransactionCmd transactionCmd { get; set; }

    /// <summary>
    /// ID транзакции при распределенных операциях с БД
    /// </summary>
    public String transactionID { get; set; }

    //public static JsonConverter[] GetConverters() {
    //  return new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */};
    //}

    protected override void copyThis(ref AjaxRequest destObj) {
      base.copyThis(ref destObj);
      BioSQLRequest dst = destObj as BioSQLRequest;
      dst.transactionCmd = this.transactionCmd;
      dst.transactionID = this.transactionID;
    }

  }


}
 