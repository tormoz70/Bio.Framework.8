using Bio.Helpers.Common.Types;

namespace Bio.Framework.Server {
  using System;
  using System.Data.Common;
  using System.Xml;
  using System.IO;
  using Bio.Framework.Packets;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;

  public delegate bool DlgProcessFieldHandler(ref string pFName, ref Field pFVal);

  /// <summary>
  /// ѕреобразовывает данные, считанные из курсора в JSON-строку.
  /// </summary>
  public class CSQLtoJSON {
    /// <summary>
    /// —обытие дл€ дополнительной обработки значени€ каждого пол€.
    /// </summary>
    public event DlgProcessFieldHandler ProcessField;

    /// <summary>
    /// «апускает процесс обработки данных курсора и возвращает строку в виде JSON-объекта.
    /// </summary>
    /// <param name="cursor">—озданный курсор.</param>
    /// <param name="logFile"></param>
    /// <returns>CJsonStoreData</returns>
    public JsonStoreData Process(CJSCursor cursor, Logger logger = null) {
      if (logger != null) logger.WriteLn("CSQLtoJSON.Process - start");
      var result = new JsonStoreData {
        EndReached = cursor.rqPacket.EndReached,
        Start = 0
      };

      result.MetaData = JsonStoreMetadata.ConstructMetadata(cursor.bioCode, cursor.CursorIniDoc);
      result.Rows = new JsonStoreRows();
      var rowCount = 0;
      result.Start = cursor.rqPacket.Start;
      result.Limit = cursor.rqPacket.Limit;
      result.TotalCount = cursor.rqPacket.TotalCount;
      // перебираем все записи в курсоре
      //throw new Exception("FTW!!!");
      if (logger != null) logger.WriteLn("CSQLtoJSON.Process - cursor - fettching - start");
      while (cursor.Next()) {
        if (result.Limit == 0 || ++rowCount <= result.Limit) {
          var newRow = result.AddRow();
          // перебираем все пол€ одной записи
          if (logger != null) logger.WriteLn("CSQLtoJSON.Process - cursor - fields - fettching - start");
          foreach (var fld in cursor.Fields) {
            var fieldName = fld.FieldName;
            var v_field = fld;
            var process = true;
            if (this.ProcessField != null)
              process = this.ProcessField(ref fieldName, ref v_field);
            if (process) {
              var doEncodeFromAnsi = (v_field.DataType == JSFieldType.Clob) && (v_field.Encoding == FieldEncoding.WINDOWS1251);
              newRow.Values[result.MetaData.IndexOf(fieldName)] = doEncodeFromAnsi ? Utl.EncodeANSI2UTF(v_field.AsObject as String) : v_field.AsObject;
            }
          }
          if (logger != null) logger.WriteLn("CSQLtoJSON.Process - cursor - fields - fettching - end");
        }
      }
      if (logger != null) logger.WriteLn("CSQLtoJSON.Process - cursor - fettching - end");
      result.EndReached = result.Limit == 0 || rowCount <= result.Limit;
      result.TotalCount = result.Start + rowCount;
      if (logger != null) logger.WriteLn("CSQLtoJSON.Process - end");
      return result;
    }

  }
}
