using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Bio.Helpers.Common.Types {
  public abstract class ADBSessionFactory {
    public abstract IDBSession CreateDBSession(String connStr);
  }
}
