namespace Bio.Framework.Server {
  using System;

  using System.Data;

  using System.Xml;
  using Packets;
  using Helpers.Common.Types;

  /// <summary>
  /// Преобразовывает данные из JSON-строки в запрос к БД.
  /// </summary>
  public class CJSONtoSQL {
    public void Process(IDbConnection conn, XmlElement cursorDef, JsonStoreRequest request, Params bioParams, String bioCode) {
      var vCommand = new CJSCursor(conn, cursorDef, bioCode);
      foreach (var row in request.Packet.Rows) {
        vCommand.DoProcessRowPost(request.Packet.MetaData, row, bioParams, request.Timeout);
      }
    }

  }
}
