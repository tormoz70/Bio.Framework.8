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
  /// ���������� �������� �� ����������� �������
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
        throw new EBioException("�������� IO:\"" + this.bioCode + "\" �� �������.");
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
                this.doGetSelectionPks(vConn, vDS);
            } else if (this.jsReq is CJsonStoreRequestPost)
              this.doPost(vConn, vDS);
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
          throw new EBioException("��� ���������� � ��.");
      } else
        throw new EBioException("� ��������� ������������� [" + this.bioCode + "] �� ������ ������ <store>.");
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
        var v_SQLtoJSON = new CSQLtoJSON();
        var v_packet = v_SQLtoJSON.Process(vCursor);
        var rsp = new CJsonStoreResponse() {
          bioParams = this.bioParams,
          ex = null,
          success = true,
          transactionID = this.TransactionID,
          packet = v_packet,
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        if (vCursor != null)
          vCursor.Close();
      }
    }

    private void doGetSelectionPks(IDbConnection conn, XmlElement ds) {
      var vCursor = new CJSCursor(conn, ds, this.bioCode);
      var rqst = this.bioRequest<CJsonStoreRequestGet>();
      vCursor.Init(rqst);
      //try {
      vCursor.Open(rqst.timeout);
      //} catch(Exception ex) {
      //  throw ex;
      //}
      try {
        String v_pks = null;
        while (vCursor.Next()) 
          Utl.appendStr(ref v_pks, vCursor.PKValue, ";");
        var rsp = new CJsonStoreResponse() {
          bioParams = this.bioParams,
          ex = null,
          success = true,
          transactionID = this.TransactionID,
          selectedPkList = v_pks
        };

        this.Context.Response.Write(rsp.Encode());
      } finally {
        if (vCursor != null)
          vCursor.Close();
      }
    }

    private void doPost(IDbConnection conn, XmlElement ds) {
      var v_request = this.bioRequest<CJsonStoreRequest>();
      var v_proc = new CJSONtoSQL();
      v_proc.Process(conn, ds, v_request, this.bioParams, this.bioCode);

      var rsp = new CJsonStoreResponse() {
        bioParams = this.bioParams,
        ex = null,
        success = true,
        transactionID = this.TransactionID,
        packet = v_request.packet
      };

      this.Context.Response.Write(rsp.Encode());
    }

  }
}
