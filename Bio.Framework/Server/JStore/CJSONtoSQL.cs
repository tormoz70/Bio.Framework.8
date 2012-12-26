namespace Bio.Framework.Server {
  using System;

  using System.Data;
  using System.Data.Common;
  //using Oracle.DataAccess.Client;

  using System.Xml;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;

  /// <summary>
  /// Преобразовывает данные из JSON-строки в запрос к БД.
  /// </summary>
  public class CJSONtoSQL {
    //public Json.RowData[] Process(DbConnection pConn, XmlElement pCursorDef, CJsonStoreRequest request, CParams pIOParams, String pIOCode) {
    //  string jsonString = pExParams.Data;
    //  try {
    //    Json.Rows jsonData = LitJson_killd.JsonMapper.ToObject<Json.Rows>(jsonString);
    //    //pConn.BeginTransaction();
    //    CSQLCursorBio vCommand = new CSQLCursorBio(pConn, pCursorDef, pIOCode);
    //    CParams vParams = new CParams();
    //    bool isFirst = true;
    //    foreach (Json.RowData row in jsonData.rows) {
    //      foreach (KeyValuePair<string, LitJson_killd.JsonData> field in row.data) {
    //        object vValue = (field.Value != null) ? field.Value.ToDotNetObject() : null;
    //        if(vValue == null){
    //          CParam vIOPrm = pIOParams.ParamByName(field.Key);
    //          if(vIOPrm != null)
    //            vValue = vIOPrm.Value;
    //        }
    //        if (isFirst)
    //          vParams.Add(field.Key, vValue);
    //        else
    //          vParams[field.Key].Value = vValue;
    //      }
    //      isFirst = false;
    //      vParams.Merge(pIOParams, false);
    //      switch (row.type) {
    //        case "u":
    //        case "c":
    //          vCommand.DoInsertUpdate(vParams);
    //          break;
    //        case "d":
    //          vCommand.DoDelete(vParams);
    //          break;
    //      }
    //      //if (!pExParams.NoReturnValues)
    //        row.data = BuildOutJson(vParams);
    //    }
    //    return jsonData.rows.Where(row => row.type != "d" && row.data != null).ToArray();
    //  } catch (LitJson_killd.JsonException je) {
    //    throw new EBioException("Неверный формат сохраняемых данных.", je);
    //  } catch (Exception ex) {
    //    throw EBioException.CreateIfNotEBio(ex);
    //  }
    //}

    //private static LitJson_killd.JsonData BuildOutJson(CParams pParams) {
    //  string json;
    //  using (StringWriter sw = new StringWriter()) {
    //    LitJson_killd.JsonWriter jw = new LitJson_killd.JsonWriter(sw);
    //    bool f = false;
    //    foreach (CParam param in pParams)
    //      if (param.ParamDirection == ParameterDirection.Output || param.ParamDirection == ParameterDirection.InputOutput) {
    //        if (!f) {
    //          jw.WriteObjectStart();
    //          //jw.WritePropertyName(CExParams.C_RowNumberFieldName).Write(
    //          //  pParams[CExParams.C_RowNumberFieldName].Value);
    //          if (pParams[CJsonStoreMetadata.csPKField] != null && pParams[CJsonStoreMetadata.csPKField].Value != null)
    //            jw.WritePropertyName(CJsonStoreMetadata.csPKField).Write(pParams[CJsonStoreMetadata.csPKField].Value);
    //          f = true;
    //        }
    //        jw.WritePropertyName(param.Name).Write(param.Value);
    //      }
    //    if (f)
    //      jw.WriteObjectEnd();
    //    json = sw.ToString();
    //  }
    //  return String.IsNullOrEmpty(json) ? null : LitJson_killd.JsonMapper.ToObject(json);
    //}

    public void Process(IDbConnection conn, XmlElement cursorDef, CJsonStoreRequest request, CParams bioParams, String bioCode) {
      CJSCursor vCommand = new CJSCursor(conn, cursorDef, bioCode);
      foreach (var row in request.packet.rows) {
        vCommand.DoProcessRowPost(request.packet.metaData, row, bioParams, request.timeout);
      }
    }

  }
}
