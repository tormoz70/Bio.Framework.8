using System;

namespace Bio.Helpers.Common.Types {
  /// <summary>
  /// 
  /// </summary>
  public interface IDBSessionFactory {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    IDBSession CreateDBSession(String connStr);
  }
}
