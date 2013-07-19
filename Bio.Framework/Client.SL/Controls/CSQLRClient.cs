using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using System.Threading;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace Bio.Framework.Client.SL {

  public class CSQLRClient {

    public IAjaxMng ajaxMng { get; set; }
    public String bioCode { get; set; }
    private Params _bioParams = null;
    public Params bioParams { 
      get {
        if (this._bioParams == null)
          this._bioParams = new Params();
        return this._bioParams;
      }
      set {
        this._bioParams = value;
      }
    }
    public String transactionID { get; set; }

    public CSQLRClient() { 
    }

    private String _lastRequestedBioCode = null;
    private Params _lastReturnedParams = null;

    private void _exec(Params bioPrms, AjaxRequestDelegate callback, SQLTransactionCmd cmd, Boolean silent) {
      if (this.ajaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.bioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");
      this.bioParams = Params.PrepareToUse(this.bioParams, bioPrms);
      //if (bioPrms != null) {
      //  if (this.bioParams == null)
      //    this.bioParams = bioPrms.Clone() as Params;
      //  else {
      //    this.bioParams.Clear();
      //    this.bioParams = this.bioParams.Merge(bioPrms, true);
      //  }
      //} else {
      //  if (this.bioParams == null)
      //    this.bioParams = new Params();
      //}

      this._lastRequestedBioCode = this.bioCode;
      this.ajaxMng.Request(new BioSQLRequest {
        RequestType = RequestType.SQLR,
        BioCode = this.bioCode,
        BioParams = this.bioParams,
        transactionCmd = cmd,
        transactionID = this.transactionID,
        Prms = null,
        Silent = silent,
        Callback = (sndr, args) => {
          if (args.Response.Success) {
            BioResponse rsp = args.Response as BioResponse;
            if (rsp != null){
              this._lastReturnedParams = (rsp.BioParams != null) ? rsp.BioParams.Clone() as Params : null;
              this.transactionID = rsp.TransactionID;
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
