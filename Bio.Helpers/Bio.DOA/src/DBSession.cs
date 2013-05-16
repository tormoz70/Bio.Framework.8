namespace Bio.Helpers.DOA {
  using System;

  using System.Data;
  using System.Collections.Generic;
  using Common.Types;
  using Common;

  /// <summary>
  /// Сессия БД
  /// </summary>
  public class DBSession : IDBSession {

    
    private readonly Dictionary<String, IDbTransaction> _storedTrans;
    private readonly Params _connStrItems;
    private readonly String _connStr;
    private readonly String _workSpaceSchema;
    /// <summary>
    /// Событие перед соединением
    /// </summary>
    public event EventHandler<DBConnBeforeEventArgs> BeforeDBConnectEvent;
    /// <summary>
    /// Событие после соединения
    /// </summary>
    public event EventHandler<DBConnAfterEventArgs> AfterDBConnectEvent;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="connStr"></param>
    /// <param name="workSpaceSchema"></param>
    public DBSession(String connStr, String workSpaceSchema) {
      this._connStrItems = new Params();
      this._storedTrans = new Dictionary<String, IDbTransaction>();
      this._connStr = connStr;
      this._parsConnectionStr(this._connStr);
      this._workSpaceSchema = workSpaceSchema;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="connStr"></param>
    public DBSession(String connStr) : this(connStr, null) {}

    /// <summary>
    /// Создать копию
    /// </summary>
    /// <returns></returns>
    public DBSession Clone() {
      var rslt = new DBSession(this.ConnectionString);
      rslt.BeforeDBConnectEvent += this.BeforeDBConnectEvent;
      rslt.AfterDBConnectEvent += this.AfterDBConnectEvent;
      return rslt;
    }

    private void _parsConnectionStr(String vConnStr) {
      var v_csItems = Utl.ParsConnectionStr(vConnStr);
      if(v_csItems != null) {
        foreach(var v_item in v_csItems)
          this._connStrItems.Add(v_item.Key, v_item.Value);
      }
    }

    /// <summary>
    /// Параметры соединения
    /// </summary>
    public Params ConnectionStrItems {
      get {
        return this._connStrItems;
      }
    }

    
    /// <summary>
    /// Строка соединения
    /// </summary>
    public String ConnectionString {
      get {
        return this._connStr;
      }
    }

    /// <summary>
    /// Вытаскивает из БД SESSION_ID
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public static String GetSessionID(IDbConnection conn) {
      return Utl.Convert2Type<String>(SQLCmd.ExecuteScalarSQL(conn, "SELECT SID as f_result FROM V$SESSION WHERE audsid = SYS_CONTEXT('userenv','sessionid')", null, 120));
    }


    private void _beforeDBConnectCallback(DBConnBeforeEventArgs args) {
      var e = this.BeforeDBConnectEvent;
      if (e != null)
        e(this, args);
    }
    private void _afterDBConnectCallback(DBConnAfterEventArgs args) {
      var e = this.AfterDBConnectEvent;
      if (e != null)
        e(this, args);
    }

    /// <summary>
    /// Создает новое соединение
    /// </summary>
    /// <returns></returns>
    public IDbConnection GetConnection() {
      return DBConnectionFactory.Instance.CreateConnection(this._connStr, this._workSpaceSchema, this._beforeDBConnectCallback, this._afterDBConnectCallback);
    }

    /// <summary>
    /// Закрыть транзакцию
    /// </summary>
    /// <param name="transaction"></param>
    public static void СommitTransaction(IDbTransaction transaction) {
      if (transaction != null) {
        var v_storedConn = transaction.Connection;
        transaction.Commit();
        transaction.Dispose();
        if (v_storedConn != null) {
          v_storedConn.Close();
          v_storedConn.Dispose();
        }
      }
    }

    /// <summary>
    /// Откатить транзакцию
    /// </summary>
    /// <param name="trans"></param>
    public static void RollbackTransaction(IDbTransaction trans) {
      if (trans != null) {
        var v_storedConn = trans.Connection;
        try {
          trans.Rollback();
        } catch (InvalidOperationException) { }
        trans.Dispose();
        if (v_storedConn != null) {
          v_storedConn.Close();
          v_storedConn.Dispose();
        }
      }
    }

    /// <summary>
    /// Сохранает транзакцию БД
    /// </summary>
    /// <param name="name"></param>
    /// <param name="transaction"></param>
    public void StoreTransaction(String name, IDbTransaction transaction) {
      if (!String.IsNullOrEmpty(name)) {
        var v_storedTrans = (this._storedTrans.ContainsKey(name.ToUpper())) ? this._storedTrans[name.ToUpper()] : null;
        if (v_storedTrans != null) {
          if ((transaction == null) || !v_storedTrans.Equals(transaction)) {
            СommitTransaction(v_storedTrans);
            this._storedTrans[name.ToUpper()] = null;
            this._storedTrans.Remove(name.ToUpper());
          }
        } else {
          if (transaction != null) {
            this._storedTrans.Add(name.ToUpper(), transaction);
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
        var v_storedTrans = (this._storedTrans.ContainsKey(transactionID.ToUpper())) ? this._storedTrans[transactionID.ToUpper()] : null;
        if (v_storedTrans != null) {
          RollbackTransaction(v_storedTrans);
          this._storedTrans[transactionID.ToUpper()] = null;
          this._storedTrans.Remove(transactionID.ToUpper());
        }
      }
    }

    /// <summary>
    /// Возвращает сохраненную ранее сессию БД
    /// </summary>
    /// <param name="transactionID"></param>
    /// <returns></returns>
    public IDbTransaction RestoreTransaction(String transactionID) {
      var rslt = (transactionID != null) ? ((this._storedTrans.ContainsKey(transactionID.ToUpper())) ? this._storedTrans[transactionID.ToUpper()] : null) : null;
      if (rslt != null) {
        if (rslt.Connection == null) {
          this.KillTransaction(transactionID);
          return null;
        }
        return rslt;
      }
      return null;
    }

    /// <summary>
    /// Убивает все незавершенные транзакции
    /// </summary>
    public void KillTransactions() {
      foreach (var v_key in this._storedTrans.Keys) {
        var v_storedTrans = (this._storedTrans.ContainsKey(v_key.ToUpper())) ? this._storedTrans[v_key.ToUpper()] : null;
        RollbackTransaction(v_storedTrans);
        this._storedTrans[v_key.ToUpper()] = null;
      }
      this._storedTrans.Clear();
    }
  }
}
