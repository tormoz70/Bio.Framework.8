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
    /// <returns>CJsonStoreData</returns>
    public CJsonStoreData Process(CJSCursor cursor) {
      CJsonStoreData result = new CJsonStoreData() {
        endReached = cursor.rqPacket.endReached,
        start = 0
      };

      result.metaData = CJsonStoreMetadata.ConstructMetadata(cursor.bioCode, cursor.CursorIniDoc);
      result.rows = new CJsonStoreRows();
      var v_rowCount = 0;
      result.start = cursor.rqPacket.start;
      result.limit = cursor.rqPacket.limit;
      result.totalCount = cursor.rqPacket.totalCount;
      // перебираем все записи в курсоре
      //throw new Exception("FTW!!!");
      while (cursor.Next()) {
        if (result.limit == 0 || ++v_rowCount <= result.limit) {
          var v_newRow = result.addRow();
          // перебираем все пол€ одной записи
          foreach (var fld in cursor.Fields) {
            var v_fieldName = fld.FieldName;
            var v_field = fld;
            var process = true;
            if (this.ProcessField != null)
              process = this.ProcessField(ref v_fieldName, ref v_field);
            if (process) {
              var v_doEncodeFromAnsi = (v_field.DataType == FieldType.Clob) && (v_field.Encoding == FieldEncoding.WINDOWS1251);
              v_newRow.Values[result.metaData.indexOf(v_fieldName)] = v_doEncodeFromAnsi ? Utl.EncodeANSI2UTF(v_field.AsObject as String) : v_field.AsObject;
            }
          }
        }
      }
      result.endReached = result.limit == 0 || v_rowCount <= result.limit;
      result.totalCount = result.start + v_rowCount;
      return result;
    }

  }
}
