namespace Bio.Framework.Server {
  using System;
  using System.Data.Common;
  using System.IO;
  using System.Web;
  using System.Xml;

  using System.Diagnostics;
  using Bio.Helpers.DOA;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
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
      //this.Context.Trace.Write(this.bioCode, "doExecute-begin");
      //this._write_log("begin0");
      //this._write_log("qparams:" + this.QParams.Encode());
      base.doExecute();
      if (this.FBioDesc == null)
        throw new EBioException("Описание IO:\"" + this.bioCode + "\" не найдено.");
      XmlElement vDS = this.FBioDesc.DocumentElement;
      if (vDS != null) {
        if (this.BioSession.Cfg.dbSession != null) {
          IDbConnection vConn = this.AssignTransaction(vDS, this.jsReq);
          var transactionFinished = false;
          try {
            if (this.jsReq is CJsonStoreRequestGet) {
              var v_jsReqGet = (this.jsReq as CJsonStoreRequestGet);
              if (v_jsReqGet.getType == CJSRequestGetType.GetData)
                this.doGet(vConn, vDS);
              else if (v_jsReqGet.getType == CJSRequestGetType.GetSelectedPks)
                this._doGetSelectionPks(vConn, vDS);
            } else if (this.jsReq is CJsonStoreRequestPost)
              this._doPost(vConn, vDS);
          } catch(Exception ex) {
            if (this.jsReq is CJsonStoreRequestPost) {
              this.FinishTransaction(vConn, true, CSQLTransactionCmd.Rollback);
              transactionFinished = true;
            }
            throw ex;
          } finally {
            if(!transactionFinished)
              this.FinishTransaction(vConn, (this.jsReq is CJsonStoreRequestPost), this.jsReq.transactionCmd);
          }

        } else
          throw new EBioException("Нет соединения с БД.");
      } else
        throw new EBioException("В документе инициализации [" + this.bioCode + "] не найден раздел <store>.");
      //this.Context.Trace.Write(this.bioCode, "doExecute-end");
      //this._write_log("end0");
    }

    private void doGet(IDbConnection conn, XmlElement ds) {
      
      var vCursor = new CJSCursor(conn, ds, this.bioCode);

      var rqst = this.bioRequest<CJsonStoreRequestGet>();
      vCursor.Init(rqst);
      //try {
      vCursor.Open(rqst.timeout);
      //}catch(Exception ex){
      //  throw ex;
      //}
      try {
        var vSqLtoJson = new CSQLtoJSON();
        var packet = vSqLtoJson.Process(vCursor);
        var rsp = new CJsonStoreResponse() {
          bioParams = this.bioParams,
          ex = null,
          success = true,
          transactionID = this.TransactionID,
          packet = packet,
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        vCursor.Close();
      }
    }

    private void _doGetSelectionPks(IDbConnection conn, XmlElement ds) {
      var vCursor = new CJSCursor(conn, ds, this.bioCode);
      var rqst = this.bioRequest<CJsonStoreRequestGet>();
      vCursor.Init(rqst);
      vCursor.Open(rqst.timeout);
      try {
        String pks = null;
        while (vCursor.Next()) 
          Utl.AppendStr(ref pks, vCursor.PKValue, ";");
        var rsp = new CJsonStoreResponse() {
          bioParams = this.bioParams,
          ex = null,
          success = true,
          transactionID = this.TransactionID,
          selectedPkList = pks
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        vCursor.Close();
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
