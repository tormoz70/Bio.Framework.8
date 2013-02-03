namespace Bio.Helpers.DOA {
  using System;

  using System.Data;
  using System.Data.Common;
  using Oracle.DataAccess.Client;

  using System.IO;
  using System.Xml;
  using System.Web;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.Specialized;

  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;

  /// <summary>
  /// Сессия БД
  /// </summary>
  public class CDBSessionOldShared : IDBSessionSharedOld {

    
    //public const String csIniFileName = "dbsession.xml";
    private Dictionary<String, IDbTransaction> FStoredTrans = null;
    //private DateTime FCreationDT;
    //private bool FConnected = false;
    //private dom4cs FIniDoc = null;
    private String FConnStr = null;
    private CParams FConnStrItems = null;
    public event EventHandler<DBConnBeforeEventArgs> BeforeDBConnectEvent;
    public event EventHandler<DBConnAfterEventArgs> AfterDBConnectEvent;
    private IDbConnection _sharedConnection = null;

    public CDBSessionOldShared(String connStr) {
      this.FConnStrItems = new CParams();
      this.FStoredTrans = new Dictionary<String, IDbTransaction>();
      //this.FCreationDT = DateTime.Now;
      this.SetConnectionString(connStr);
    }

    public CDBSessionOldShared Clone() {
      CDBSessionOldShared rslt = new CDBSessionOldShared(this.ConnectionString);
      rslt.BeforeDBConnectEvent += this.BeforeDBConnectEvent;
      rslt.AfterDBConnectEvent += this.AfterDBConnectEvent;
      return rslt;
    }

    private void parsConnectionStr(String vConnStr) {
      IDictionary<String, String> vCSItems = Utl.ParsConnectionStr(vConnStr);
      if(vCSItems != null) {
        foreach(KeyValuePair<String, String> vItem in vCSItems)
          this.FConnStrItems.Add(vItem.Key, vItem.Value);
      }
    }

    public void SetConnectionString(String connStr) {
      this.CloseSharedConnection();
      this.FConnStr = connStr;
      this.parsConnectionStr(this.FConnStr);
    }

    public CParams ConnectionStrItems {
      get {
        return this.FConnStrItems;
      }
    }

    public String ConnectionString {
      get {
        return this.FConnStr;
      }
    }

    public static String GetSessionID(IDbConnection conn) {
      return Utl.Convert2Type<String>(SQLCmd.ExecuteScalarSQL(conn, "SELECT SID as f_result FROM V$SESSION WHERE audsid = SYS_CONTEXT('userenv','sessionid')", null, 120));
    }


    private void _beforeDBConnectCallback(DBConnBeforeEventArgs args) {
      EventHandler<DBConnBeforeEventArgs> e = this.BeforeDBConnectEvent;
      if (e != null)
        e(this, args);
    }
    public void _afterDBConnectCallback(DBConnAfterEventArgs args) {
      EventHandler<DBConnAfterEventArgs> e = this.AfterDBConnectEvent;
      if (e != null)
        e(this, args);
    }
    //private Boolean _connectionIsShared = false;
    private IDbConnection _getSharedConn() {
      if (this._sharedConnection == null) {
        this._sharedConnection = dbFactory.CreateConnection(this.FConnStr, this._beforeDBConnectCallback, this._afterDBConnectCallback);
        (this._sharedConnection as OracleConnection).StateChange += new StateChangeEventHandler((o, a) => {
          if (SQLUtils.DbConnectionStateIsOppened(a.CurrentState)) {
            this.CloseSharedConnection();
          }
        });
      }
      return this._sharedConnection;
    }
    public IDbConnection GetConnection(Boolean shareConnection) {
      if (shareConnection) 
        return this._getSharedConn();
      else
        return dbFactory.CreateConnection(this.FConnStr, this._beforeDBConnectCallback, this._afterDBConnectCallback);
    }

    public Boolean SharedConnectionIsOpenned {
      get {
        return SQLUtils.DbConnectionIsOppened(this._sharedConnection);
      }
    }

    public void CloseSharedConnection() {
      if (this._sharedConnection != null) {
        this._sharedConnection.Close();
        this._sharedConnection.Dispose();
        this._sharedConnection = null;
      }
    }

    public Boolean ConnectionIsShared { 
      get{
        return SQLUtils.DbConnectionIsOppened(this._sharedConnection);
      }
    }

    public IDbConnection GetConnection() {
      return this.GetConnection(false);
    }

    public static void commitTransaction(IDbTransaction pTrans) {
      if (pTrans != null) {
        IDbConnection vStoredConn = pTrans.Connection;
        pTrans.Commit();
        pTrans.Dispose();
        if (vStoredConn != null) {
          vStoredConn.Close();
          vStoredConn.Dispose();
        }
      }
    }
    public static void rollbackTransaction(IDbTransaction trans) {
      if (trans != null) {
        IDbConnection vStoredConn = trans.Connection;
        try {
          trans.Rollback();
        } catch (InvalidOperationException) { }
        trans.Dispose();
        if (vStoredConn != null) {
          vStoredConn.Close();
          vStoredConn.Dispose();
        }
      }
    }

    /// <summary>
    /// Сохранает транзакцию БД
    /// </summary>
    /// <param name="pName"></param>
    /// <param name="trans"></param>
    public void StoreTransaction(String pName, IDbTransaction trans) {
      if (!String.IsNullOrEmpty(pName)) {
        IDbTransaction vStoredTrans = (this.FStoredTrans.ContainsKey(pName.ToUpper())) ? this.FStoredTrans[pName.ToUpper()] : null;
        if (vStoredTrans != null) {
          if ((trans == null) || !vStoredTrans.Equals(trans)) {
            commitTransaction(vStoredTrans);
            this.FStoredTrans[pName.ToUpper()] = null;
            this.FStoredTrans.Remove(pName.ToUpper());
          }
        } else {
          if (trans != null) {
            this.FStoredTrans.Add(pName.ToUpper(), trans);
          }
        }
      }
    }

    /// <summary>
    /// Удаляет транзакцию БД
    /// </summary>
    /// <param name="transactionID"></param>
    public void KillTransaction(String transactionID) {
      if (!String.IsNullOrEmpty(transactionID)) {
        IDbTransaction vStoredTrans = (this.FStoredTrans.ContainsKey(transactionID.ToUpper())) ? this.FStoredTrans[transactionID.ToUpper()] : null;
        if (vStoredTrans != null) {
          rollbackTransaction(vStoredTrans);
          this.FStoredTrans[transactionID.ToUpper()] = null;
          this.FStoredTrans.Remove(transactionID.ToUpper());
        }
      }
    }

    /// <summary>
    /// Возвращает сохраненную ранее сессию БД
    /// </summary>
    /// <param name="transactionID"></param>
    /// <returns></returns>
    public IDbTransaction RestoreTransaction(String transactionID) {
      var rslt = (transactionID != null) ? ((this.FStoredTrans.ContainsKey(transactionID.ToUpper())) ? this.FStoredTrans[transactionID.ToUpper()] : null) : null;
      if (rslt != null) {
        if (rslt.Connection == null) {
          this.KillTransaction(transactionID);
          return null;
        } else
          return rslt;
      } else
        return null;
    }

    /// <summary>
    /// Убивает все незавершенные транзакции
    /// </summary>
    public void KillTransactions() {
      foreach (String vKey in this.FStoredTrans.Keys) {
        IDbTransaction vStoredTrans = (this.FStoredTrans.ContainsKey(vKey.ToUpper())) ? this.FStoredTrans[vKey.ToUpper()] : null;
        rollbackTransaction(vStoredTrans);
        this.FStoredTrans[vKey.ToUpper()] = null;
      }
      this.FStoredTrans.Clear();
    }
  }
}
