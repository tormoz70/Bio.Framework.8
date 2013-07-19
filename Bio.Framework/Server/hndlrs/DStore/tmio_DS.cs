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
  public class tmio_DS : ABioHandlerBioTransacted {
    public tmio_DS(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    private void _write_log(String pMsg) {
      //if (this.BioSession.Cfg.Debug)
      //  this.write_log("tmio_DS-doExecute-Get", pMsg);
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
      var v_ds = this.FBioDesc.DocumentElement;
      if (v_ds != null) {
        if (this.BioSession.Cfg.dbSession != null) {
          var v_conn = this.AssignTransaction(v_ds, this.JSReq);
          var transactionFinished = false;
          try {
            if (this.JSReq is JsonStoreRequestGet) {
              var v_jsReqGet = (this.JSReq as JsonStoreRequestGet);
              if (v_jsReqGet.getType == CJSRequestGetType.GetData)
                this._doGet(v_conn, v_ds);
              else if (v_jsReqGet.getType == CJSRequestGetType.GetSelectedPks)
                this._doGetSelectionPks(v_conn, v_ds);
            } else if (this.JSReq is JsonStoreRequestPost)
              this._doPost(v_conn, v_ds);
          } catch(Exception) {
            if (this.JSReq is JsonStoreRequestPost) {
              this.FinishTransaction(v_conn, true, SQLTransactionCmd.Rollback);
              transactionFinished = true;
            }
            throw;
          } finally {
            if(!transactionFinished)
              this.FinishTransaction(v_conn, (this.JSReq is JsonStoreRequestPost), this.JSReq.transactionCmd);
          }

        } else
          throw new EBioException("Нет соединения с БД.");
      } else
        throw new EBioException("В документе инициализации [" + this.bioCode + "] не найден раздел <store>.");
    }

    private void _doGet(IDbConnection conn, XmlElement ds) {
      //throw new Exception("FTW!");
      var v_cursor = new CJSCursor(conn, ds, this.bioCode);

      var rqst = this.BioRequest<JsonStoreRequestGet>();
      v_cursor.Init(rqst);
      v_cursor.Open(rqst.Timeout);
      try {
        var v_sqlToJson = new CSQLtoJSON();
        var packet = v_sqlToJson.Process(v_cursor);
        var rsp = new JsonStoreResponse() {
          BioParams = this.bioParams,
          Ex = null,
          Success = true,
          TransactionID = this.TransactionID,
          packet = packet,
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        v_cursor.Close();
      }
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
