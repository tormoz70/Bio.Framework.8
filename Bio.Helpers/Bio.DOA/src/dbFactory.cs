using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using System.Threading;
using System.Collections;
using System.ComponentModel;

namespace Bio.Helpers.DOA {
  public class DBConnBeforeEventArgs : CancelEventArgs{
    public String ConnectionString { get; set; }
  }
  public class DBConnAfterEventArgs : EventArgs {
    public IDbConnection Connection { get; set; }
  }

  internal class dbFactory {
    public static DbProviderFactory factory = null;

    static dbFactory() {
    }

    private static void CreateFactory() {
      try {
        factory = DbProviderFactories.GetFactory("Oracle.DataAccess.Client");
      }catch(Exception ex){
        throw new EBioException("Ошибка инициализации Oracle.DataAccess.Client. Сообщение: " + ex.Message, ex);
      }
    }

    public static System.Data.Common.DbCommand CreateCommand() {
      if(factory == null)
        CreateFactory();

      return factory.CreateCommand();
    }

    public static System.Data.Common.DbCommandBuilder CreateCommandBuilder() {
      if(factory == null)
        CreateFactory();

      return factory.CreateCommandBuilder();
    }

    public static IDbConnection CreateConnection(String connStr, Action<DBConnBeforeEventArgs> beforeDBConnectCallback, 
                                                                  Action<DBConnAfterEventArgs> afterDBConnectCallback) {
      String vConnStr = connStr;
      if (beforeDBConnectCallback != null) {
        DBConnBeforeEventArgs args = new DBConnBeforeEventArgs {
          Cancel = false,
          ConnectionString = vConnStr
        };
        beforeDBConnectCallback(args);
        if (args.Cancel) return null;
        vConnStr = args.ConnectionString; 
      }
      if (factory == null)
        CreateFactory();
      IDbConnection vResult = factory.CreateConnection();
      if(!String.IsNullOrEmpty(vConnStr)) {
        vResult.ConnectionString = vConnStr;
        try {
          vResult.Open();
        } catch (ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          throw new EBioDBConnectionError("Ошибка соединения с базой данных. Сообщение сервера: " + ex.Message, ex);
        }
        if (afterDBConnectCallback != null) {
          DBConnAfterEventArgs args = new DBConnAfterEventArgs {
            Connection = vResult
          };
          afterDBConnectCallback(args);
        }

      }
      return vResult;
    }
    public static IDbConnection CreateConnection() {
      return CreateConnection(null, null, null);
    }

    public static System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder() {
      if(factory == null)
        CreateFactory();

      return factory.CreateConnectionStringBuilder();
    }

    public static System.Data.Common.DbDataAdapter CreateDataAdapter() {
      if(factory == null)
        CreateFactory();

      return factory.CreateDataAdapter();
    }

    public static System.Data.Common.DbParameter CreateParameter() {
      if(factory == null)
        CreateFactory();

      return factory.CreateParameter();
    }
  }
}
