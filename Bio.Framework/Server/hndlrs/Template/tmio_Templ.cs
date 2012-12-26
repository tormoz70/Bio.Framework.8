namespace Bio.Framework.Server {

  using System;
  using System.Data;
  using System.Data.Common;

  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Helpers.Common.Types;

  /// <summary>
  /// Данный обработчик вх. сообщений является шаблоном для создания нового сообщения
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_Templ : ABioHandlerBio {

    public tmio_Templ(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    protected override void doExecute() {
      base.doExecute();

      //XmlElement vDS = this.FBioDesc.DocumentElement;
      //String vConnStr = this.BioSession.detectConnStr(vDS);
      //using(DbConnection vConn = this.BioSession.DBSess.GetConnection(vConnStr)) {
      //  try {
      //    if(vDS != null) {
      //      CSQLCursorBio vCursor = new CSQLCursorBio(vConn, vDS, this.bioCode);
      //      vCursor.DoExecuteSQL(this.bioParams);
      //      this.Context.Response.Write(new CBioResponse() { success = true }.Encode());
      //    }
      //  } finally {
      //    if(vConn != null)
      //      vConn.Close();
      //  }
      //}
    }
  }
}
