﻿using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Bio.Helpers.Common.Types;
using System.Threading;
using System.ComponentModel;

namespace Bio.Helpers.DOA {
  /// <summary>
  /// Аргументы события
  /// </summary>
  public class DBConnBeforeEventArgs : CancelEventArgs{

    /// <summary>
    /// Строка соединения
    /// </summary>
    public String ConnectionString { get; set; }
  }

  /// <summary>
  /// Аргументы события
  /// </summary>
  public class DBConnAfterEventArgs : EventArgs {
    /// <summary>
    /// Соединение
    /// </summary>
    public IDbConnection Connection { get; set; }
  }

  internal class DBConnectionFactory {
    private static readonly Object _syncObject = new Object();
    private static DBConnectionFactory _instance;
    private DbProviderFactory _factory;

    private void _createFactory() {
      try {
        _factory = DbProviderFactories.GetFactory("Oracle.DataAccess.Client");
      } catch (Exception ex) {
        throw new EBioException("Ошибка инициализации Oracle.DataAccess.Client. Сообщение: " + ex.Message, ex);
      }
    }

    private DBConnectionFactory() {
      this._createFactory();
    }

    public static DBConnectionFactory Instance {
      get {
        lock (_syncObject) {
          return _instance ?? (_instance = new DBConnectionFactory());
        }
      }

    }

    public DbCommand CreateCommand() {
      return this._factory.CreateCommand();
    }

    public DbCommandBuilder CreateCommandBuilder() {
      return this._factory.CreateCommandBuilder();
    }

    public IDbConnection CreateConnection(String connStr, String workspaceSchema, Action<DBConnBeforeEventArgs> beforeDBConnectCallback, 
                                                                  Action<DBConnAfterEventArgs> afterDBConnectCallback) {
      if (beforeDBConnectCallback != null) {
        var args = new DBConnBeforeEventArgs {
          Cancel = false,
          ConnectionString = connStr
        };
        beforeDBConnectCallback(args);
        if (args.Cancel) return null;
        connStr = args.ConnectionString; 
      }
      IDbConnection result = this._factory.CreateConnection();
      if (!String.IsNullOrEmpty(connStr) && (result != null)) {
        result.ConnectionString = connStr;
        try {
          result.Open();
        } catch (ThreadAbortException) {
          throw;
        } catch (Exception ex) {
          throw new EBioDBConnectionError("Ошибка соединения с базой данных. Сообщение сервера: " + ex.Message, ex);
        }
        if (!String.IsNullOrEmpty(workspaceSchema)) {
          SQLCmd.ExecuteScript(result, "ALTER SESSION SET CURRENT_SCHEMA=" + workspaceSchema.ToUpper(), null, 60);
        }
        if (afterDBConnectCallback != null) {
          var args = new DBConnAfterEventArgs {
            Connection = result
          };
          afterDBConnectCallback(args);
        }

      }
      return result;
    }
    public IDbConnection CreateConnection() {
      return CreateConnection(null, null, null, null);
    }

    public DbConnectionStringBuilder CreateConnectionStringBuilder() {
      return this._factory.CreateConnectionStringBuilder();
    }

    public DbDataAdapter CreateDataAdapter() {
      return this._factory.CreateDataAdapter();
    }

    public DbParameter CreateParameter() {
      return this._factory.CreateParameter();
    }
  }
}
