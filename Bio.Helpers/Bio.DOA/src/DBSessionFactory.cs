using System;

namespace Bio.Helpers.DOA {
  using Common.Types;

  /// <summary>
  /// ������� DBSession
  /// </summary>
  public class DBSessionFactory : IDBSessionFactory {
    /// <summary>
    /// ������� ����� ��������� DBSession
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    public IDBSession CreateDBSession(String connStr) {
      return new DBSession(connStr);
    }
  }
}
