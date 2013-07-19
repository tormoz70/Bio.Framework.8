namespace Bio.Framework.Server {

  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Xml;
  using System.Web;
  using System.IO;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;

  public class ABioHandlerBioTransacted : ABioHandlerBio {
    public ABioHandlerBioTransacted(HttpContext context, AjaxRequest request) : base(context, request) { }

    protected string TransactionID { get; private set; }
    protected bool AutoCommitTransaction { get; private set; }
    

    protected IDbConnection AssignTransaction(XmlElement ds, BioSQLRequest request) {
      this.TransactionID = null;
      this.AutoCommitTransaction = false;
      if (request is JsonStoreRequestGet) {
        return this.BioSession.Cfg.dbSession.GetConnection();
      }
      this.AutoCommitTransaction = true;
      IDbConnection vConn;
      this.TransactionID = request.transactionID;
      if (String.IsNullOrEmpty(this.TransactionID))
        this.TransactionID = ds.GetAttribute("transactionID");
      this.AutoCommitTransaction = String.IsNullOrEmpty(this.TransactionID);
      if(this.AutoCommitTransaction)
        this.TransactionID = Guid.NewGuid().ToString();
      if (!String.IsNullOrEmpty(this.TransactionID)) {
        var vStoredTrans = this.BioSession.Cfg.dbSession.RestoreTransaction(this.TransactionID);
        if (vStoredTrans == null) {
          vConn = this.BioSession.Cfg.dbSession.GetConnection();
          this.BioSession.Cfg.dbSession.StoreTransaction(this.TransactionID, vConn.BeginTransaction(IsolationLevel.ReadCommitted));
        } else
          vConn = vStoredTrans.Connection;
      } else
        vConn = this.BioSession.Cfg.dbSession.GetConnection();
      return vConn;
    }

    protected void FinishTransaction(IDbConnection vConn, Boolean isPostRequest, SQLTransactionCmd cmd) {
      if (!isPostRequest) {
        if (vConn != null) {
          vConn.Close();
          vConn.Dispose();
        }
        return;
      }

      var vCommited = false;
      if (this.AutoCommitTransaction || (cmd == SQLTransactionCmd.Commit)) {
        vCommited = true;
        if (!String.IsNullOrEmpty(this.TransactionID))
          this.BioSession.Cfg.dbSession.StoreTransaction(this.TransactionID, null);
      } else if (cmd == SQLTransactionCmd.Rollback)
        this.BioSession.Cfg.dbSession.KillTransaction(this.TransactionID);
      if (vCommited || (cmd == SQLTransactionCmd.Rollback)) {
        if (vConn != null) {
          vConn.Close();
          vConn.Dispose();
        }
      }
    }

    protected void RollbackTransaction() {
      if (!String.IsNullOrEmpty(this.TransactionID)) {
        this.BioSession.Cfg.dbSession.KillTransaction(this.TransactionID);
      }
    }

  }
}
