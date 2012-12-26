namespace Bio.Framework.Server {
  using System;
  using System.Data.Common;
  using System.Xml;
  using System.IO;
  using Bio.Framework.Packets;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;

  public delegate bool DlgProcessFieldHandler(ref string pFName, ref CField pFVal);

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

      CField vFVal;
      result.metaData = CJsonStoreMetadata.ConstructMetadata(cursor.bioCode, cursor.CursorIniDoc);
      result.rows = new CJsonStoreRows();
      int vRowCount = 0;
      result.start = cursor.rqPacket.start;
      result.limit = cursor.rqPacket.limit;
      result.totalCount = cursor.rqPacket.totalCount;
      // перебираем все записи в курсоре
      //throw new Exception("FTW!!!");
      while (cursor.Next()) {
        //vRowCount++;
        if (result.limit == 0 || ++vRowCount <= result.limit) {
          CJsonStoreRow newRow = result.addRow();
          // перебираем все пол€ одной записи
          foreach (CField vCur in cursor.Fields) {
            String vFName = vCur.FieldName;
            vFVal = vCur;
            Boolean process = true;
            if (this.ProcessField != null)
              process = this.ProcessField(ref vFName, ref vFVal);
            if (process) {
              newRow.Values[result.metaData.indexOf(vFName)] = vFVal.AsObject;
            }
          }
        }
      }
      result.endReached = result.limit == 0 ?  true : vRowCount <= result.limit;
      result.totalCount = result.start + vRowCount;
      return result;
    }

  }
}
