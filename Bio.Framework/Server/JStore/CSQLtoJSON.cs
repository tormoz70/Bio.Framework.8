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
  /// ��������������� ������, ��������� �� ������� � JSON-������.
  /// </summary>
  public class CSQLtoJSON {
    /// <summary>
    /// ������� ��� �������������� ��������� �������� ������� ����.
    /// </summary>
    public event DlgProcessFieldHandler ProcessField;

    /// <summary>
    /// ��������� ������� ��������� ������ ������� � ���������� ������ � ���� JSON-�������.
    /// </summary>
    /// <param name="cursor">��������� ������.</param>
    /// <returns>CJsonStoreData</returns>
    public JsonStoreData Process(CJSCursor cursor) {
      JsonStoreData result = new JsonStoreData {
        EndReached = cursor.rqPacket.EndReached,
        Start = 0
      };

      result.MetaData = JsonStoreMetadata.ConstructMetadata(cursor.bioCode, cursor.CursorIniDoc);
      result.Rows = new JsonStoreRows();
      var v_rowCount = 0;
      result.Start = cursor.rqPacket.Start;
      result.Limit = cursor.rqPacket.Limit;
      result.TotalCount = cursor.rqPacket.TotalCount;
      // ���������� ��� ������ � �������
      //throw new Exception("FTW!!!");
      while (cursor.Next()) {
        if (result.Limit == 0 || ++v_rowCount <= result.Limit) {
          var v_newRow = result.AddRow();
          // ���������� ��� ���� ����� ������
          foreach (var fld in cursor.Fields) {
            var v_fieldName = fld.FieldName;
            var v_field = fld;
            var process = true;
            if (this.ProcessField != null)
              process = this.ProcessField(ref v_fieldName, ref v_field);
            if (process) {
              var v_doEncodeFromAnsi = (v_field.DataType == JSFieldType.Clob) && (v_field.Encoding == FieldEncoding.WINDOWS1251);
              v_newRow.Values[result.MetaData.IndexOf(v_fieldName)] = v_doEncodeFromAnsi ? Utl.EncodeANSI2UTF(v_field.AsObject as String) : v_field.AsObject;
            }
          }
        }
      }
      result.EndReached = result.Limit == 0 || v_rowCount <= result.Limit;
      result.TotalCount = result.Start + v_rowCount;
      return result;
    }

  }
}
