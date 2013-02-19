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
    public tmio_DS(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private void _write_log(String pMsg) {
      //if (this.BioSession.Cfg.Debug)
      //  this.write_log("tmio_DS-doExecute-Get", pMsg);
    }

    private CJsonStoreRequest jsReq {
      get {
        return this.FBioRequest as CJsonStoreRequest;
      }
    }

    protected override void doExecute() {
      base.doExecute();
      if (this.FBioDesc == null)
        throw new EBioException("Описание IO:\"" + this.bioCode + "\" не найдено.");
      var v_ds = this.FBioDesc.DocumentElement;
      if (v_ds != null) {
        if (this.BioSession.Cfg.dbSession != null) {
          var v_conn = this.AssignTransaction(v_ds, this.jsReq);
          var v_transactionFinished = false;
          try {
            if (this.jsReq is CJsonStoreRequestGet) {
              var v_jsReqGet = (this.jsReq as CJsonStoreRequestGet);
              if (v_jsReqGet.getType == CJSRequestGetType.GetData)
                this._doGet(v_conn, v_ds);
              else if (v_jsReqGet.getType == CJSRequestGetType.GetSelectedPks)
                this._doGetSelectionPks(v_conn, v_ds);
            } else if (this.jsReq is CJsonStoreRequestPost)
              this._doPost(v_conn, v_ds);
          } catch(Exception ex) {
            if (this.jsReq is CJsonStoreRequestPost) {
              this.FinishTransaction(v_conn, true, CSQLTransactionCmd.Rollback);
              v_transactionFinished = true;
            }
            throw;
          } finally {
            if(!v_transactionFinished)
              this.FinishTransaction(v_conn, (this.jsReq is CJsonStoreRequestPost), this.jsReq.transactionCmd);
          }

        } else
          throw new EBioException("Нет соединения с БД.");
      } else
        throw new EBioException("В документе инициализации [" + this.bioCode + "] не найден раздел <store>.");
    }

    private void _doGet(IDbConnection conn, XmlElement ds) {
      
      var v_cursor = new CJSCursor(conn, ds, this.bioCode);

      var rqst = this.bioRequest<CJsonStoreRequestGet>();
      v_cursor.Init(rqst);
      v_cursor.Open(rqst.timeout);
      try {
        var v_sqlToJson = new CSQLtoJSON();
        var packet = v_sqlToJson.Process(v_cursor);
        var rsp = new CJsonStoreResponse() {
          bioParams = this.bioParams,
          ex = null,
          success = true,
          transactionID = this.TransactionID,
          packet = packet,
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        v_cursor.Close();
      }
    }

    private void _doGetSelectionPks(IDbConnection conn, XmlElement ds) {
      var v_cursor = new CJSCursor(conn, ds, this.bioCode);
      var rqst = this.bioRequest<CJsonStoreRequestGet>();
      v_cursor.Init(rqst);
      v_cursor.Open(rqst.timeout);
      try {
        String pks = null;
        while (v_cursor.Next()) 
          Utl.AppendStr(ref pks, v_cursor.PKValue, ";");
        var rsp = new CJsonStoreResponse() {
          bioParams = this.bioParams,
          ex = null,
          success = true,
          transactionID = this.TransactionID,
          selectedPkList = pks
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        v_cursor.Close();
      }
    }

    private void _doPost(IDbConnection conn, XmlElement ds) {
      var request = this.bioRequest<CJsonStoreRequest>();
      var proc = new CJSONtoSQL();
      proc.Process(conn, ds, request, this.bioParams, this.bioCode);

      var rsp = new CJsonStoreResponse() {
        bioParams = this.bioParams,
        ex = null,
        success = true,
        transactionID = this.TransactionID,
        packet = request.packet
      };

      this.Context.Response.Write(rsp.Encode());
    }

  }
}
