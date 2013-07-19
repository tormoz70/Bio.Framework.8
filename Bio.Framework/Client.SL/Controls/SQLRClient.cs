using System;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;

namespace Bio.Framework.Client.SL {

  public class SQLRClient {

    public IAjaxMng AjaxMng { get; set; }
    public String BioCode { get; set; }
    private Params _bioParams;
    public Params BioParams { 
      get { return this._bioParams ?? (this._bioParams = new Params()); }
      set {
        this._bioParams = value;
      }
    }
    public String TransactionID { get; set; }

    public SQLRClient() { 
    }

    private String _lastRequestedBioCode;
    private Params _lastReturnedParams;

    private void _exec(Params bioPrms, AjaxRequestDelegate callback, SQLTransactionCmd cmd, Boolean silent) {
      if (this.AjaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.BioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");
      this.BioParams = Params.PrepareToUse(this.BioParams, bioPrms);

      this._lastRequestedBioCode = this.BioCode;
      this.AjaxMng.Request(new BioSQLRequest {
        RequestType = RequestType.SQLR,
        BioCode = this.BioCode,
        BioParams = this.BioParams,
        transactionCmd = cmd,
        transactionID = this.TransactionID,
        Prms = null,
        Silent = silent,
        Callback = (sndr, args) => {
          if (args.Response.Success) {
            var rsp = args.Response as BioResponse;
            if (rsp != null){
              this._lastReturnedParams = (rsp.BioParams != null) ? rsp.BioParams.Clone() as Params : null;
              this.TransactionID = rsp.TransactionID;
            }
          }
          if (callback != null) callback(this, args);
        }
      });

    }

    public void Exec(Params bioPrms, AjaxRequestDelegate callback, SQLTransactionCmd cmd, Boolean silent) {
      this._exec(bioPrms, callback, cmd, silent);
    }

    public void Exec(Params bioPrms, AjaxRequestDelegate callback, SQLTransactionCmd cmd) {
      this._exec(bioPrms, callback, cmd, false);
    }

    public void Exec(Params bioPrms, AjaxRequestDelegate callback) {
      this._exec(bioPrms, callback, SQLTransactionCmd.Nop, false);
    }

    public void Exec(Params bioPrms, AjaxRequestDelegate callback, Boolean silent) {
      this._exec(bioPrms, callback, SQLTransactionCmd.Nop, silent);
    }

    public void Commit(AjaxRequestDelegate callback) {
      this._exec(null, callback, SQLTransactionCmd.Commit, false);
    }
    
    public void Rollback(AjaxRequestDelegate callback) {
      this._exec(null, callback, SQLTransactionCmd.Rollback, false);
    }

  }
}
