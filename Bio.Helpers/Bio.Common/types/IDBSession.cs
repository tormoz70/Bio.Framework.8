using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Bio.Helpers.Common.Types {
  public interface IDBSession {
    String ConnectionString { get; }
    IDbConnection GetConnection();
    void StoreTransaction(String name, IDbTransaction transaction);
    IDbTransaction RestoreTransaction(String pName);
    void KillTransaction(String pName);
    void KillTransactions();
  }
  public interface IDBSessionSharedOld : IDBSession {
    IDbConnection GetConnection(Boolean shareConnection);
    void CloseSharedConnection();
    Boolean ConnectionIsShared { get; }
    //void StoreTransaction(String name, IDbTransaction pTrans);
    //IDbTransaction RestoreTransaction(String name);
    //void KillTransaction(String name);
    //void KillTransactions();
  }
}
