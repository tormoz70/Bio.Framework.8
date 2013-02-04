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

  public class CDBSessionFactory : ADBSessionFactory {
    public override IDBSession CreateDBSession(string connStr) {
      return new DBSession(connStr);
    }
  }
}
