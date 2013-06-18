using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {
  public class CJsonStoreResponse : CBioResponse {
    /// <summary>
    /// Пакет данных который возвращается при запросе к БД типа "select"
    /// </summary>
    public CJsonStoreData packet { get; set; }
    /// <summary>
    /// Сортировка, которая использовалясь при запросе к БД
    /// </summary>
    public CJsonStoreSort sort { get; set; }
    /// <summary>
    /// Фильтрация, которая использовалясь при запросе к БД
    /// </summary>
    public CJsonStoreFilter filter { get; set; }
    //public CJsonStoreNav nav { get; set; }

    public String selectedPkList { get; set; }

    public static CJsonStoreResponse Decode(String pJsonString) {
      return jsonUtl.decode<CJsonStoreResponse>(pJsonString, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    public override String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    //public override object Clone() {
    //  return CAjaxResponse.CopyObj<CJsonStoreResponse>(this);
    //}

    protected override void copyThis(ref CAjaxResponse destObj) {
      base.copyThis(ref destObj);
      CJsonStoreResponse dst = destObj as CJsonStoreResponse;
      dst.packet = (this.packet != null) ? (CJsonStoreData)this.packet.Clone() : null;
      //dst.cmd = this.cmd;
      dst.sort = (this.sort != null) ? (CJsonStoreSort)this.sort.Clone() : null;
      dst.filter = (this.filter != null) ? (CJsonStoreFilter)this.filter.Clone() : null;
      dst.transactionID = this.transactionID;
      dst.selectedPkList = this.selectedPkList;
    }

  }
}
