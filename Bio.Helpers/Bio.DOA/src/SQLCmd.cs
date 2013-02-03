namespace Bio.Helpers.DOA {
  using System;
  using System.Data;
  using System.Text.RegularExpressions;
  using System.Collections;
  using System.Threading;
  using System.Text;
  using System.Collections.Generic;

  using Oracle.DataAccess.Client;
  using Oracle.DataAccess.Types;

  using Common;
  using Common.Types;

  /// <summary>
  /// Базовый и самый примитивный тип для создания курсора
  /// Реализует основные методы для работы с UniDir курсором
  /// </summary>
	public class SQLCmd:CDisposableObject{
    private CParams _params;

    protected const Int32 ORAERRCODE_APP_ERR_START = 20000; //начало диапазона кодов ошибок приложения в Oracle
    protected const Int32 ORAERRCODE_USER_BREAKED = 1013; //{"ORA-01013: пользователем запрошена отмена текущей операции"}
    protected const Int32 FetchedRowLimit = 10000000; // Максимальное кол-во записей, которое может вернуть запрос к БД
    private OracleDataReader _dataReader;
		private readonly OracleCommand _statement;
		protected String originSQL = null;
		protected String preparedSQL = null;
    protected Hashtable rowValues = null;
    protected Int64 curFetchedRowPos = 0;
    protected Boolean closeConnOnClose = false;

    /// <summary>
    /// Конструктор
    /// </summary>
    public SQLCmd() {
      this._statement = new OracleCommand();
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pConn"></param>
    public SQLCmd(IDbConnection pConn):this() {
      this._statement.Connection = (OracleConnection)pConn;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="sess"></param>
    public SQLCmd(IDBSession sess):this() {
      this._statement.Connection = (OracleConnection)sess.GetConnection();
      this.closeConnOnClose = true;
    }

    protected virtual void prepareSQL() {
      this.preparedSQL = this.originSQL;
    }

    /// <summary>
    /// Инициализирует объект 
    /// </summary>
    /// <param name="pSQL"></param>
    /// <param name="pParams"></param>
    public virtual void Init(String pSQL, CParams pParams) {
      this.originSQL = pSQL;
      this._params = pParams;
      this.prepareSQL();
    }

    /// <summary>
    /// Текущие параметры запроса
    /// </summary>
    public CParams Params {
      get {
        return this._params;
      }
    }

    /// <summary>
    /// Текущее соединение с БД
    /// </summary>
    public IDbConnection Connection {
      get {
        return this._statement.Connection;
      }
    }

    protected override void OnDispose() {
      try {
        this.Close();
      } catch {}
    }

    protected virtual void onBeforeOpen(){
		}

		protected virtual void onAfterOpen(){
		}

    private static void checkOraConn(IDbConnection pConn) {
      if(pConn == null)
        throw new EBioException("Не определена сессия!");
      switch(pConn.State) {
        case ConnectionState.Broken: throw new EBioException("Соединение с БД прервано!");
        case ConnectionState.Closed: throw new EBioException("Соединение с БД не установлено!");
        case ConnectionState.Connecting: throw new EBioException("Установление соединения с БД ...");
        case ConnectionState.Executing:
        case ConnectionState.Fetching: throw new EBioException("Соединение занято другим процессом!");
      }
    }

    /// <summary>
    /// Открывает курсор
    /// </summary>
    public void Open(Int32 timeout) {
      this.Open(this._statement.Connection, timeout);
    }

    private static void setParamsToStatment(OracleCommand oraCmd, CParams prms) {
      setParamsToStatment(oraCmd, prms, true);
    }


    /// <summary>
    /// Вытаскивает из SQL все вызовы хранимых процедур
    /// </summary>
    /// <param name="pSQL"></param>
    /// <returns></returns>
    private static String[] detectExecsOfStoredPrcs(String pSQL) {
      const String csDelimiter = "+|+";
      String v_resultStr = null;
      var vr = new Regex(@"\b[\w$]+\b[.]\b[\w$]+\b\s*[(]\s*[$]PRMLIST\s*[)]", RegexOptions.IgnoreCase);
      var m = vr.Match(pSQL);
      while(m.Success) {
        Utl.AppendStr(ref v_resultStr, m.Value, csDelimiter);
        m = m.NextMatch();
      }
      return !String.IsNullOrEmpty(v_resultStr) ? Utl.SplitString(v_resultStr, csDelimiter) : new String[0];
    }

    /// <summary>
    /// Вытаскивает из вызова хранимой процедуры пару pPkgName-{имя пакета}, pObjName-{имя проц}
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="pkgName"></param>
    /// <param name="objName"></param>
    /// <returns></returns>
    private static void DetectAutoDBMSParamsParts(String sql, out String pkgName, out String objName) {
      pkgName = Utl.RegexFind(sql, @"\b[\w$]+\b(?=[.])", true);
      objName = Utl.RegexFind(sql, @"(?<=[.])\b[\w$]+\b(?=\s*[(;])", true);
    }

    private static void detectSQLParams1(OracleCommand oraCommand) {
      var v_sql = oraCommand.CommandText;
      // Удаляем все строковый константы
      Utl.RegexReplace(ref v_sql, @"(['])(.*?)\1", "", true);

      // Находим все параметры вида :qwe_ad
      var m = Utl.RegexMatch(v_sql, @"(?<=:)\b[\w\#\$]+\b", true);
      while(m.Success) {
        var v_parName = m.Value;
        if(SQLUtils.FindOraParam(oraCommand.Parameters, v_parName) == null) {
          var v_parType = SQLUtils.DetectOraTypeByFldName(v_parName);
          oraCommand.Parameters.Add(v_parName, v_parType);
        }
        m = m.NextMatch();
      }
    }

    private const String _SQL_GET_PARAMS_FROM_DBMS = "select "+
            " argument_name, position, sequence, data_type, in_out, data_length" +
            " from user_arguments" +
            " where package_name = upper(:package_name)" +
            " and object_name = upper(:object_name)" +
            " order by position";
    private const String _DEFAULT_ARGPREFIX = "P_";

    private static void _detectSQLParams2(OracleCommand oracleCommand, out Boolean isParamListAuto) {

      var v_sql = oracleCommand.CommandText;

      isParamListAuto = Utl.RegexMatch(v_sql, @"[(]\s*[$]PRMLIST\s*[)]", true).Success;

      if(isParamListAuto) {
        var v_execs = detectExecsOfStoredPrcs(v_sql);
        foreach (var t in v_execs) {
          String v_args = null; String v_pkgName; String v_objName;
          DetectAutoDBMSParamsParts(t, out v_pkgName, out v_objName);
          var v_statement = new OracleCommand(SQLUtils.PrepareSQLForOlacleExecute(_SQL_GET_PARAMS_FROM_DBMS));
          v_statement.Connection = oracleCommand.Connection; 
          try {
            v_statement.Parameters.Add("package_name", OracleDbType.Varchar2).Value = v_pkgName;
            v_statement.Parameters.Add("object_name", OracleDbType.Varchar2).Value = v_objName;
            var v_reader = v_statement.ExecuteReader();
            try {
              while(v_reader.Read()) {
                var v_fldIndx = v_reader.GetOrdinal("argument_name");
                if(!v_reader.IsDBNull(v_fldIndx)) {
                  var v_parName = v_reader.GetString(v_fldIndx);
                  if(!v_parName.Substring(0, 2).ToUpper().Equals(_DEFAULT_ARGPREFIX))
                    throw new EBioDOA("Не верный формат наименования аргументов хранимой процедуры.\n" +
                                      "Для использования автогенерации аргументов с помощью переменной $PRMLIST\n" +
                                      "необходимо, чтобы все имена аргументов начинались с префикса " + _DEFAULT_ARGPREFIX + " !");
                  v_parName = v_parName.Substring(2);
                  Utl.AppendStr(ref v_args, ":" + v_parName, ",");
                  var v_parTypeName = v_reader.GetString(v_reader.GetOrdinal("data_type"));
                  var v_parDirName = v_reader.GetString(v_reader.GetOrdinal("in_out"));
                  var v_lenObj = v_reader.GetValue(v_reader.GetOrdinal("data_length"));
                  var v_parLength = ((v_lenObj == DBNull.Value) ? 0 : Utl.Convert2Type<Int32>(v_lenObj));
                  if (SQLUtils.FindOraParam(oracleCommand.Parameters, v_parName) == null) {
                    var v_parType = SQLUtils.DetectOraTypeByOraTypeName(v_parTypeName);
                    var op = oracleCommand.Parameters.Add(v_parName, v_parType);
                    op.Direction = SQLUtils.DetectParamDirByName(v_parDirName);
                    if(op.Direction == ParameterDirection.Output)
                      op.Size = v_parLength;
                  }
                }
              }
            } finally {
              v_reader.Close();
              v_reader.Dispose();
            }
          } finally {
            v_statement.Dispose();
          }
          var v_newExec = t;
          Utl.RegexReplace(ref v_newExec, @"[(]\s*[$]PRMLIST\s*[)]", "(" + v_args + ")", true);
          v_sql = v_sql.Replace(t, v_newExec);
        }
        oracleCommand.CommandText = v_sql;
      }
    }

    private static void _detectSQLParamOutCursor(OracleCommand vOraCmd, ref OracleParameter refCursor) {

      if (vOraCmd.CommandType == CommandType.StoredProcedure) {
        String v_pkgName; String v_objName;
        DetectAutoDBMSParamsParts(vOraCmd.CommandText, out v_pkgName, out v_objName);
        var v_statement = new OracleCommand(SQLUtils.PrepareSQLForOlacleExecute(_SQL_GET_PARAMS_FROM_DBMS));
        v_statement.Connection = vOraCmd.Connection;
        try {
          v_statement.Parameters.Add("package_name", OracleDbType.Varchar2).Value = v_pkgName;
          v_statement.Parameters.Add("object_name", OracleDbType.Varchar2).Value = v_objName;
          var v_reader = v_statement.ExecuteReader();
          try {
            while (v_reader.Read()) {
              var v_fldIndx = v_reader.GetOrdinal("argument_name");
              if (!v_reader.IsDBNull(v_fldIndx)) {
                var v_parName = v_reader.GetString(v_fldIndx);
                var v_parTypeName = v_reader.GetString(v_reader.GetOrdinal("data_type"));
                var v_parDirName = v_reader.GetString(v_reader.GetOrdinal("in_out"));
                if (v_parTypeName.Equals("REF CURSOR") && (v_parDirName.Equals("IN/OUT") || v_parDirName.Equals("OUT"))) {
                  if (SQLUtils.FindOraParam(vOraCmd.Parameters, v_parName) == null) {
                    refCursor = vOraCmd.Parameters.Add(v_parName, OracleDbType.RefCursor);
                    refCursor.Direction = SQLUtils.DetectParamDirByName(v_parDirName);
                  }
                } else {
                  if (SQLUtils.FindOraParam(vOraCmd.Parameters, v_parName) == null) {
                    var v_parType = SQLUtils.DetectOraTypeByOraTypeName(v_parTypeName);
                    var op = vOraCmd.Parameters.Add(v_parName, v_parType);
                    op.Direction = SQLUtils.DetectParamDirByName(v_parDirName);
                    if (op.Direction == ParameterDirection.Output) {
                      var v_lenObj = v_reader.GetValue(v_reader.GetOrdinal("data_length"));
                      var v_parLength = ((v_lenObj == DBNull.Value) ? 0 : Utl.Convert2Type<Int32>(v_lenObj));
                      op.Size = v_parLength;
                    }
                  }
                }
              }
            }
          } finally {
            v_reader.Close();
            v_reader.Dispose();
          }
        } finally {
          v_statement.Dispose();
        }
      }
    }

    private static void setParamsToStatment(OracleCommand oraCmd, CParams prms, Boolean overwrite) {
      OracleParameter v_refCursor = null;
      setParamsToStatment(oraCmd, prms, overwrite, ref v_refCursor);
    }

    private static void setParamsToStatment(OracleCommand oraCmd, CParams prms, ref OracleParameter refCursor) {
      setParamsToStatment(oraCmd, prms, true, ref refCursor);
    }

    /// <summary>
    /// Данная процедура создает параметры запроса и подставляет значения этих параметров.
    /// </summary>
    /// <param name="oraCmd"></param>
    /// <param name="prms"></param>
    /// <param name="overwrite"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private static void setParamsToStatment(OracleCommand oraCmd, CParams prms, Boolean overwrite, ref OracleParameter refCursor) {
      oraCmd.BindByName = true;
      if (oraCmd == null)
        throw new ArgumentNullException("oraCmd");


      detectSQLParams1(oraCmd);
      var v_isParamListAuto = false;
      _detectSQLParams2(oraCmd, out v_isParamListAuto);
      _detectSQLParamOutCursor(oraCmd, ref refCursor);
      foreach (OracleParameter v_prm in oraCmd.Parameters) {
        var v_inPrm = SQLUtils.FindParam(prms, v_prm.ParameterName);
        if(v_inPrm != null) {

          if(v_isParamListAuto) {
            v_inPrm.ParamType = SQLUtils.DetectTypeByOraType(v_prm.OracleDbType);
            v_inPrm.ParamDir = SQLUtils.EncodeParamDirection(v_prm.Direction);
          } else {
            v_prm.Direction = SQLUtils.DecodeParamDirection(v_inPrm.ParamDir);
            if((v_inPrm.ParamType == null) && (v_inPrm.Value != null))
              v_inPrm.ParamType = v_inPrm.Value.GetType();
            if(v_inPrm.ParamType != null)
              v_prm.OracleDbType = SQLUtils.DetectOraTypeByType(
                v_inPrm.ParamType, (!String.IsNullOrEmpty(v_inPrm.ValueAsString()) ? v_inPrm.ValueAsString().Length : v_inPrm.ParamSize));
          }


          var v_paramValue = SQLUtils.FindParamValue(prms, v_prm);
          Object v_paramValueDest = null;
          var v_targetType = SQLUtils.DetectTypeByOraType(v_prm.OracleDbType);
          try {
            if(v_paramValue != null) {
              var v_s = v_paramValue as string;
              if (v_s != null) {
                if (v_targetType == typeof(Decimal)) {
                  v_paramValueDest = SQLUtils.StrAsOraValue(v_s, OracleDbType.Decimal);
                } else if (v_targetType == typeof(DateTime))
                  v_paramValueDest = SQLUtils.StrAsOraValue(v_s, OracleDbType.Date);
                else
                  v_paramValueDest = v_paramValue;
              } else {
                v_paramValueDest = Utl.Convert2Type(v_paramValue, v_targetType);
              }
            }
          } catch(ThreadAbortException) {
            throw;
          } catch(Exception ex) {
            throw new EBioException("Ошибка при присвоении значения параметру [{" + v_prm.ParameterName + "(" + v_targetType + ")" + "}=" + v_paramValue + "], Сообщение: " + ex.Message, ex);
          }
          if(overwrite) {
            v_prm.Value = v_paramValueDest ?? DBNull.Value;
          } else {
            if(v_prm.Value == null)
              v_prm.Value = v_paramValueDest ?? DBNull.Value;
          }

          if (v_prm.Direction == ParameterDirection.Output || v_prm.Direction == ParameterDirection.InputOutput) {
            if(v_prm.OracleDbType == OracleDbType.Varchar2) {
              v_prm.Size = (v_inPrm.ParamSize > 0) ? v_inPrm.ParamSize : 32000;
            }
          }
        } else {
          if(v_prm.Value == null)
            v_prm.Value = DBNull.Value;
        }
      }
    }

    private static void setStatmentToParams(OracleCommand oraCmd, CParams prms) {
      if(prms != null) {
        foreach(OracleParameter vPrm in oraCmd.Parameters) {
          if((vPrm.Direction == ParameterDirection.Output) ||
              (vPrm.Direction == ParameterDirection.InputOutput) ||
                (vPrm.Direction == ParameterDirection.ReturnValue)) {
            CParam vInPrm = SQLUtils.FindParam(prms, vPrm.ParameterName);
            if(vInPrm != null) {
              vInPrm.Value = SQLUtils.OraDbValueAsObject(vPrm.Value);
              //vInPrm.Value = vPrm.Value;
            }
          }
        }
      }
    }

    protected static String detectDBName(String connStr) {
      String vResult = null;
      IDictionary<String, String> vConnStrs = Utl.ParsConnectionStr(connStr);
      if (vConnStrs.ContainsKey("Data Source"))
        vResult = vConnStrs["Data Source"];
      if (vConnStrs.ContainsKey("User ID"))
        vResult = (vResult != null) ? vConnStrs["User ID"] + "@" + vResult : vConnStrs["User ID"];
      return vResult;
    }

    private static void processTimeoutError(IDbConnection conn, Exception ex, String pParams) {
      if ((ex is OracleException) && (((OracleException)ex).Number == 1013))
        throw new EBioSQLTimeout(ex);
    }

    protected virtual void processOpenError(IDbConnection conn, Exception ex, String pParams) {
      throw new EBioException("[" + detectDBName(conn.ConnectionString) + "] Ошибка при открытии курсора.\r\nСообщение: " + ex.Message + "\r\nSQL: " + this._statement.CommandText + "\r\n" + "Параметры запроса:{" + pParams + "}", ex);
    }
    private static void processExecError(IDbConnection conn, Exception ex, String pSQL, String pParams) {
      throw new EBioException("[" + detectDBName(conn.ConnectionString) + "] Ошибка выполнения запроса к БД.\r\nСообщение: " + ex.Message + "\r\nSQL: " + pSQL + "\r\n" + "Параметры запроса:{" + pParams + "}", ex);
    }


    private void _processOpenError(IDbConnection conn, Exception ex, String pParams) {
      processTimeoutError(conn, ex, pParams);
      this.processOpenError(conn, ex, pParams);
    }

    private static void _processExecError(IDbConnection conn, Exception ex, String pSQL, String pParams) {
      processTimeoutError(conn, ex, pParams);
      processExecError(conn, ex, pSQL, pParams);
    }

    /// <summary>
    /// Возвращает описание параметров использованных при последнем открытии курсора (для отладки).
    /// </summary>
    public String LastOpenParamsDebugText { get; private set; }

    private OracleDataReader _openReaderAsSelect(IDbConnection pConn, Int32 timeout) {
      OracleDataReader rslt = null;
      this._statement.Connection = (OracleConnection)pConn;
      this._statement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.preparedSQL);
      this._statement.CommandTimeout = timeout;
      try {
        this.onBeforeOpen();
        this._statement.Parameters.Clear();
        setParamsToStatment(this._statement, this._params);
        this.LastOpenParamsDebugText = bldDbgParamsStr(this._statement);
        rslt = this._statement.ExecuteReader();
        return rslt;
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        this._processOpenError(this._statement.Connection, ex, this.LastOpenParamsDebugText);
        return null;
      }
    }

    private OracleDataReader _openReaderAsProcedure(IDbConnection pConn, Int32 timeout) {
      OracleDataReader rslt = null;
      this._statement.Connection = (OracleConnection)pConn;
      this._statement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.preparedSQL);
      this._statement.CommandType = CommandType.StoredProcedure;
      this._statement.CommandTimeout = timeout;
      try {
        this.onBeforeOpen();
        this._statement.Parameters.Clear();
        OracleParameter refCursor = null;
        setParamsToStatment(this._statement, this._params, ref refCursor);
        this.LastOpenParamsDebugText = bldDbgParamsStr(this._statement);
        this._statement.ExecuteNonQuery();
        if (refCursor != null) {
          rslt = ((OracleRefCursor)refCursor.Value).GetDataReader();
          return rslt;
        } else
          return null;
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        this._processOpenError(this._statement.Connection, ex, this.LastOpenParamsDebugText);
        return null;
      }
    }

    /// <summary>
    /// Открывает курсор
    /// </summary>
    /// <param name="pConn"></param>
    /// <param name="pTrn"></param>
    public void Open(IDbConnection pConn, Int32 timeout) {
      checkOraConn(pConn);
      var cmdType = Utl.DetectCommandType(this.preparedSQL);
      if (cmdType == CommandType.Text)
        this._dataReader = this._openReaderAsSelect(pConn, timeout);
      else
        this._dataReader = this._openReaderAsProcedure(pConn, timeout);
      this.curFetchedRowPos = 0;
      this.onAfterOpen();
    }

    public static String bldDbgParamsStr(IDbCommand stmt) {
      StringBuilder vReslt = new StringBuilder();
      foreach(OracleParameter vPrm in ((OracleCommand)stmt).Parameters) {
        String vDir = "(in)";
        if(vPrm.Direction == ParameterDirection.InputOutput)
          vDir = "(in out)";
        else if(vPrm.Direction == ParameterDirection.Output)
          vDir = "(out)";
        else if(vPrm.Direction == ParameterDirection.ReturnValue)
          vDir = "(ret)";
        vReslt.AppendLine(vPrm.ParameterName + vDir + "[" + vPrm.Value + "]:("+vPrm.DbType+");");
      }
        
      return vReslt.ToString();
    }

    public static IDbCommand PrepareCommand(IDbConnection conn, String sql, CParams prms, Int32 timeout) {
      checkOraConn(conn);
      var stmt = new OracleCommand();
      stmt.CommandText = SQLUtils.PrepareSQLForOlacleExecute(sql);
      stmt.Connection = (OracleConnection)conn;
      stmt.CommandTimeout = timeout;
      try {
        SQLCmd.setParamsToStatment(stmt, prms);
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        throw new EBioException("Ошибка подготовки запроса к БД.\r\nСообщение: " + ex.Message, ex);
      }
      return stmt;
    }

    //public static IDbCommand PrepareCommand(IDBSession sess, String sql, CParams prms, Int32 timeout) {
    //  var stmt = new OracleCommand();
    //  stmt.CommandText = SQLUtils.PrepareSQLForOlacleExecute(sql);
    //  try {
    //    stmt.Connection = (OracleConnection)sess.GetConnection();
    //    stmt.CommandTimeout = timeout;
    //    //stmt.Connection.Open();
    //    SQLCmd.setParamsToStatment(stmt, prms);
    //  } catch(ThreadAbortException) {
    //    throw;
    //  } catch(Exception ex) {
    //    throw new EBioException("Ошибка подготовки запроса к БД.\r\nСообщение: " + ex.Message, ex);
    //  }
    //  return stmt;
    //}

    public static void ExecuteScript(IDbCommand stmt, String pSQL, CParams pParams) {
      String vDbgPrmsStr = null;
      try {
        vDbgPrmsStr = bldDbgParamsStr(stmt);
        stmt.ExecuteNonQuery();
        setStatmentToParams((OracleCommand)stmt, pParams);
      } catch(OracleException oe) {
        String msg = String.Empty;
        if((oe.Errors.Count > 0) && (oe.Errors[0].Number == ORAERRCODE_USER_BREAKED))
          throw new EBioSQLBreaked(oe);
        for(int i = 0; String.IsNullOrEmpty(msg) && i < oe.Errors.Count; i++)
          if(oe.Errors[i].Number >= ORAERRCODE_APP_ERR_START)
            msg = oe.Errors[i].Message;
        throw new EBioException((String.IsNullOrEmpty(msg)) ? "Ошибка выполнения запроса к БД.\r\nСообщение: " + oe.Message + "\r\n" +
                              "SQL: " + stmt.CommandText + "\r\n" + "Параметры запроса:{" + (String.IsNullOrEmpty(vDbgPrmsStr) ? ((pParams != null) ? pParams.ToString() : null) : vDbgPrmsStr) + "}" : msg);
      } catch(ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch(Exception ex) {
        _processExecError(stmt.Connection, ex, pSQL, vDbgPrmsStr);
      }
    }

    /// <summary>
    /// Выполняет команду к БД
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    public static void ExecuteScript(IDbConnection conn, String sql, CParams prms, Int32 timeout) {
      var stmt = PrepareCommand(conn, sql, prms, timeout);
      ExecuteScript(stmt, sql, prms);
    }

    public static void ExecuteScript(IDBSession sess, String sql, CParams prms, Int32 timeout) {
      IDbConnection conn = sess.GetConnection();
      try {
        ExecuteScript(conn, sql, prms, timeout);
      } finally {
        conn.Close();
      }
    }

    /// <summary>
    /// Вытаскивает ошибку приложения
    /// </summary>
    /// <param name="pMessage"></param>
    /// <returns></returns>
    public String ExtractOracleApplicationError(String pMessage) {
      if (pMessage != null) {
        int vStrtIndx = pMessage.IndexOf("ORA-2");
        if (vStrtIndx >= 0) {
          int vEndIndx = pMessage.IndexOf("ORA-", vStrtIndx + 5);
          String vMsg = pMessage.Substring(vStrtIndx, vEndIndx - vStrtIndx);
          return "[" + vMsg.Substring(4, 5) + "] " + vMsg.Substring(11);
        } else
          return null;
      } else
        return null;
    }

    /// <summary>
    /// Выполняет запрос к БД, который возвращает одно значение
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public Object ExecuteScalarSQL(IDbConnection conn, Int32 timeout) {
      return ExecuteScalarSQL(conn, this.PreparedSQL, this._params, timeout);
    }

    /// <summary>
    /// Выполняет запрос к БД, который возвращает одно значение
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="conn"></param>
    /// <param name="prms"></param>
    /// <returns></returns>
    public static Object ExecuteScalarSQL(IDbConnection conn, String sql, CParams prms, Int32 timeout) {
      checkOraConn(conn);
      var stmt = new OracleCommand();
      stmt.CommandText = SQLUtils.PrepareSQLForOlacleExecute(sql);
			stmt.Connection = (OracleConnection)conn;
      stmt.CommandTimeout = timeout;
      String vParamsToDebug = null;
      Object oR = null;
			try{
        stmt.Parameters.Clear();
        setParamsToStatment(stmt, prms);
        vParamsToDebug = bldDbgParamsStr(stmt);
				oR = stmt.ExecuteScalar();
      } catch(ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch(Exception ex) {
        _processExecError(stmt.Connection, ex, sql, vParamsToDebug);
			}
			return oR;
		}

    /// <summary>
    /// Выполняет запрос к БД, который возвращает одно значение
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="pConnStr"></param>
    /// <param name="pParams"></param>
    /// <returns></returns>
    public static Object ExecuteScalarSQL(IDBSession sess, String sql, CParams prms, Int32 timeout) {
      Object oR = null;
      var conn = sess.GetConnection();
      try {
        oR = ExecuteScalarSQL(conn, sql, prms, timeout);
      } finally {
        conn.Close();
      }
      return oR;
    }

    /// <summary>
    /// SQL - предложение подготовленное к выполнению
    /// </summary>
		public String PreparedSQL{
      get{
        return this.preparedSQL;
      }
		}
		
    /// <summary>
    /// SQL - предложение исходное
    /// </summary>
		public String SQL{
      get{
        return this.originSQL;
      }
		}

    /// <summary>
    /// Переоткрывает текущий курсор
    /// </summary>
    public void Refresh(Int32 timeout) {
      this.Refresh(this._statement.Connection, timeout);
    }

    /// <summary>
    /// Переоткрывает текущий курсор
    /// </summary>
    /// <param name="conn"></param>
    public void Refresh(IDbConnection conn, Int32 timeout) {
      checkOraConn(conn);
      this.Close();
      this._statement.Connection = (OracleConnection)conn;
      this._statement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.preparedSQL);
      this._statement.CommandTimeout = timeout;
			try{
        this._dataReader = this._statement.ExecuteReader();
        this.curFetchedRowPos = 0;
			}catch (Exception ex){
        throw new EBioException("Ошибка при обновлении курсора. Сообщение: " + ex.Message + " SQL: " + this.preparedSQL);
			}
		}

    /// <summary>
    /// Прервать открытие текущего курсора
    /// </summary>
    public void Cancel() {
      if(this._statement != null) {
        this._statement.Cancel();
        this._dataReader = null;
      }
    }

    /// <summary>
    /// Закрывает текущий курсор
    /// </summary>
		public void  Close(){
      if((this._statement != null) && (this.IsActive)) {
        this._dataReader.Close();
        this._statement.Cancel();
        this._dataReader = null;
        if(this.closeConnOnClose)
          this._statement.Connection.Close();
			}
		}
		
    /// <summary>
    /// Переход к следующей записи в текущем курсоре
    /// </summary>
    /// <returns></returns>
		public bool Next(){
      try{
        if(this.rowValues == null)
          this.rowValues = new Hashtable();
        this.rowValues.Clear();
        if(this._dataReader.Read()) {
          this.curFetchedRowPos++;
          if((FetchedRowLimit > 0) && (this.curFetchedRowPos > FetchedRowLimit)) {
            String vIOCode = null;// (this is CSQLCursorBio) ? "{" + (this as CSQLCursorBio).IOCode + "} " : null;
            String vMsg = "Запрос " + vIOCode + "вернул более " + FetchedRowLimit + " записей. Проверьте параметры запроса.";
            throw new EBioDOATooMuchRows(vMsg);
          }
          for (int i = 0; i < this._dataReader.FieldCount; i++) {
            Object vObj;
            if (!this._dataReader.IsDBNull(i)) {
              vObj = SQLUtils.OraDbValueAsObject(this._dataReader.GetOracleValue(i));
            } else
              vObj = null;
            this.rowValues[this._dataReader.GetName(i).ToUpper()] = vObj;
          }
          return true;
        } else
          return false;
      } catch(ThreadAbortException) {
        throw;
      } catch(Exception ex) {
        this.Close();
        throw new EBioException("Ошибка при заполнении значений курсора. Сообщение: " + ex.Message, ex);
			}
		}
		
    /// <summary>
    /// IsActive
    /// </summary>
		public bool IsActive{
      get{
        if(this._dataReader == null)
			    return false;
        else
          return !this._dataReader.IsClosed;
      }
		}
		
    /// <summary>
    /// Ссылка на OracleDataReader курсора
    /// </summary>
    public IDataReader DataReader{
      get{
        if((this._dataReader != null) && (!this._dataReader.IsClosed))
          return this._dataReader;
        else
          return null;
      }
    }

    public IDbCommand DbCommand {
      get {
        return this._statement;
      }
    }

    /// <summary>
    /// Значения текущей записи
    /// </summary>
    public Hashtable RowValues {
      get {
        return this.rowValues;
      }
    }

    /// <summary>
    /// Кол-во записей пройденых командой Next
    /// </summary>
    public Int64 CurFetchedRowPos {
      get {
        return this.curFetchedRowPos;
      }
    }

    internal CSQLGarbageMonitor garbageMonitor = null;

    private void _test() {
      OracleConnection conn = null;
      if (conn.State != System.Data.ConnectionState.Open)
      conn.Open();
      OracleTransaction transaction = conn.BeginTransaction();
      OracleCommand cmd = new OracleCommand();
      //cmd.CommandText = "UDO_P_LOAD_FILES";
      cmd.CommandType = CommandType.StoredProcedure;
      cmd.Parameters.Clear();
      //cmd.Parameters.Add(setParam("dfile_date", file.LastAccessTime));
      //cmd.Parameters.Add(setParam("nfile_size", file.Length));
      //cmd.Parameters.Add(setParam("sfile_name", file.Name));
      //cmd.Parameters.Add(setParam("sfile_path", Path.GetDirectoryName(file.FullName)));
      //cmd.Parameters.Add(setParam("sLoad_comment", "Загрузка данных службой"));
      //var v_paramOut = new OracleParameter("ofile_body", OracleDbType.Clob, ParameterDirection.InputOutput);
      //cmd.Parameters.Add(v_paramOut);
      //cmd.ExecuteNonQuery();
      //var v_result = ((OracleClob)v_paramOut.Value);
      //StreamReader streamreader = new StreamReader(v_result, Encoding.Unicode);
      //char[] cbuffer = new char[100];
      //while((actual = streamreader.Read(cbuffer, 0, cbuffer.Length)) >0){
      //  // тут пишешь cbuffer куда тебе надо : в файл или поток....
      //}
    }
	}
}