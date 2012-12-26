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
    private CParams _bioParams = null;
    public CParams bioParams { 
      get {
        if (this._bioParams == null)
          this._bioParams = new CParams();
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
    private CParams _lastReturnedParams = null;

    private void _exec(CParams bioPrms, AjaxRequestDelegate callback, CSQLTransactionCmd cmd, Boolean silent) {
      if (this.ajaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.bioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");
      this.bioParams = CParams.PrepareToUse(this.bioParams, bioPrms);
      //if (bioPrms != null) {
      //  if (this.bioParams == null)
      //    this.bioParams = bioPrms.Clone() as CParams;
      //  else {
      //    this.bioParams.Clear();
      //    this.bioParams = this.bioParams.Merge(bioPrms, true);
      //  }
      //} else {
      //  if (this.bioParams == null)
      //    this.bioParams = new CParams();
      //}

      this._lastRequestedBioCode = this.bioCode;
      this.ajaxMng.Request(new CBioSQLRequest {
        requestType = RequestType.SQLR,
        bioCode = this.bioCode,
        bioParams = this.bioParams,
        transactionCmd = cmd,
        transactionID = this.transactionID,
        prms = null,
        silent = silent,
        callback = (sndr, args) => {
          if (args.response.success) {
            CBioResponse rsp = args.response as CBioResponse;
            if (rsp != null){
              this._lastReturnedParams = (rsp.bioParams != null) ? rsp.bioParams.Clone() as CParams : null;
              this.transactionID = rsp.transactionID;
            }
          }
          if (callback != null) callback(this, args);
        }
      });

    }

    public void Exec(CParams bioPrms, AjaxRequestDelegate callback, CSQLTransactionCmd cmd, Boolean silent) {
      this._exec(bioPrms, callback, cmd, silent);
    }

    public void Exec(CParams bioPrms, AjaxRequestDelegate callback, CSQLTransactionCmd cmd) {
      this._exec(bioPrms, callback, cmd, false);
    }

    public void Exec(CParams bioPrms, AjaxRequestDelegate callback) {
      this._exec(bioPrms, callback, CSQLTransactionCmd.Nop, false);
    }

    public void Exec(CParams bioPrms, AjaxRequestDelegate callback, Boolean silent) {
      this._exec(bioPrms, callback, CSQLTransactionCmd.Nop, silent);
    }

    public void Commit(AjaxRequestDelegate callback) {
      this._exec(null, callback, CSQLTransactionCmd.Commit, false);
    }
    
    public void Rollback(AjaxRequestDelegate callback) {
      this._exec(null, callback, CSQLTransactionCmd.Rollback, false);
    }

  }
}
