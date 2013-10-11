using System.IO;

namespace Bio.Framework.Server {
  using System;
  using System.Web;
  using System.Xml;
  using Packets;
  using Helpers.Common.Types;
  using Helpers.Common;
  using System.Data;

  /// <summary>
  /// Обработчик запросов на отображение таблицы
  /// </summary>
// ReSharper disable InconsistentNaming
  public class tmio_DS : ABioHandlerBioTransacted {
// ReSharper restore InconsistentNaming
    public tmio_DS(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    private JsonStoreRequest JSReq {
      get {
        return this.FBioRequest as JsonStoreRequest;
      }
    }

    protected override void doExecute() {
      base.doExecute();
      if (this.FBioDesc == null)
        throw new EBioException("Описание IO:\"" + this.bioCode + "\" не найдено.");
      var ds = this.FBioDesc.DocumentElement;
      if (ds != null) {
        if (this.BioSession.Cfg.dbSession != null) {
          var conn = this.AssignTransaction(ds, this.JSReq);
          var transactionFinished = false;
          try {
            if (this.JSReq is JsonStoreRequestGet) {
              var jsReqGet = (this.JSReq as JsonStoreRequestGet);
              switch (jsReqGet.getType) {
                case CJSRequestGetType.GetData:
                  this._doGet(conn, ds);
                  break;
                case CJSRequestGetType.GetSelectedPks:
                  this._doGetSelectionPks(conn, ds);
                  break;
              }
            } else if (this.JSReq is JsonStoreRequestPost)
              this._doPost(conn, ds);
          } catch(Exception) {
            if (this.JSReq is JsonStoreRequestPost) {
              this.FinishTransaction(conn, true, SQLTransactionCmd.Rollback);
              transactionFinished = true;
            }
            throw;
          } finally {
            if(!transactionFinished)
              this.FinishTransaction(conn, (this.JSReq is JsonStoreRequestPost), this.JSReq.transactionCmd);
          }

        } else
          throw new EBioException("Нет соединения с БД.");
      } else
        throw new EBioException("В документе инициализации [" + this.bioCode + "] не найден раздел <store>.");
    }

    private void _doGet(IDbConnection conn, XmlElement ds) {
      var logger = new Logger(this.BioSession.Cfg.WorkspacePath, "debug.log") { Disabled = true };
      var cursor = new CJSCursor(conn, ds, this.bioCode);
      var rqst = this.BioRequest<JsonStoreRequestGet>();
      logger.WriteLn("_doGet - start");
      cursor.Init(rqst);
      logger.WriteLn("_doGet - cursor.Init - done");
      cursor.Open(rqst.Timeout);
      logger.WriteLn("_doGet - cursor.Open - done");
      try {
        var sqlToJson = new CSQLtoJSON();
        var packet = sqlToJson.Process(cursor, logger);
        var rsp = new JsonStoreResponse {
          BioParams = this.bioParams,
          Ex = null,
          Success = true,
          TransactionID = this.TransactionID,
          packet = packet,
        };
        logger.WriteLn("_doGet - sqlToJson.Process - done");

        this.Context.Response.Write(rsp.Encode());
        logger.WriteLn("_doGet - Response.Write - done");
      } finally {
        cursor.Close();
      }
      logger.WriteLn("_doGet - end");
    }

    private void _doGetSelectionPks(IDbConnection conn, XmlElement ds) {
      var v_cursor = new CJSCursor(conn, ds, this.bioCode);
      var rqst = this.BioRequest<JsonStoreRequestGet>();
      v_cursor.Init(rqst);
      v_cursor.Open(rqst.Timeout);
      try {
        String pks = null;
        while (v_cursor.Next()) 
          Utl.AppendStr(ref pks, v_cursor.PKValue, ";");
        var rsp = new JsonStoreResponse {
          BioParams = this.bioParams,
          Ex = null,
          Success = true,
          TransactionID = this.TransactionID,
          selectedPkList = pks
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        v_cursor.Close();
      }
    }

    private void _doPost(IDbConnection conn, XmlElement ds) {
      var request = this.BioRequest<JsonStoreRequest>();
      var proc = new CJSONtoSQL();
      proc.Process(conn, ds, request, this.bioParams, this.bioCode);

      var rsp = new JsonStoreResponse {
        BioParams = this.bioParams,
        Ex = null,
        Success = true,
        TransactionID = this.TransactionID,
        packet = request.Packet
      };

      this.Context.Response.Write(rsp.Encode());
    }

  }
}
