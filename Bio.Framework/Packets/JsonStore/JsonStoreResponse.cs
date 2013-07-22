using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {
  public class JsonStoreResponse : BioResponse {
    /// <summary>
    /// Пакет данных который возвращается при запросе к БД типа "select"
    /// </summary>
    public JsonStoreData packet { get; set; }
    /// <summary>
    /// Сортировка, которая использовалясь при запросе к БД
    /// </summary>
    public JsonStoreSort sort { get; set; }
    /// <summary>
    /// Фильтрация, которая использовалясь при запросе к БД
    /// </summary>
    public JsonStoreFilter filter { get; set; }
    //public CJsonStoreNav nav { get; set; }

    public String selectedPkList { get; set; }

    public static JsonStoreResponse Decode(String pJsonString) {
      return jsonUtl.decode<JsonStoreResponse>(pJsonString, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    public override String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    //public override object Clone() {
    //  return AjaxResponse.CopyObj<JsonStoreResponse>(this);
    //}

    protected override void copyThis(ref AjaxResponse destObj) {
      base.copyThis(ref destObj);
      JsonStoreResponse dst = destObj as JsonStoreResponse;
      dst.packet = (this.packet != null) ? (JsonStoreData)this.packet.Clone() : null;
      //dst.cmd = this.cmd;
      dst.sort = (this.sort != null) ? (JsonStoreSort)this.sort.Clone() : null;
      dst.filter = (this.filter != null) ? (JsonStoreFilter)this.filter.Clone() : null;
      dst.TransactionID = this.TransactionID;
      dst.selectedPkList = this.selectedPkList;
    }

  }
}
