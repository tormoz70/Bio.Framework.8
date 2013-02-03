namespace Bio.Helpers.DOA {
  using System;

  using System.Data;
  using System.Data.Common;
  using Oracle.DataAccess.Client;
  using Oracle.DataAccess.Types;
  
  using System.Text.RegularExpressions;
  using System.Collections;
  using System.Globalization;
  using System.Threading;
  using Bio.Helpers.Common.Types;
  using System.Text;
  using Bio.Helpers.Common;
  using System.Collections.Specialized;
  using System.Collections.Generic;
  using System.Linq;
  using System.IO;

  /// <summary>
  /// Базовый и самый примитивный тип для создания курсора
  /// Реализует основные методы для работы с UniDir курсором
  /// </summary>
	public class CSQLCmd:CDisposableObject{
    private CParams FParams = null;

    protected const Int32 ciORAERRCODE_APP_ERR_START = 20000; //начало диапазона кодов ошибок приложения в Oracle
    protected const Int32 ciORAERRCODE_USER_BREAKED = 1013; //{"ORA-01013: пользователем запрошена отмена текущей операции"}
    protected const Int32 ciFetchedRowLimit = 10000000; // Максимальное кол-во записей, которое может вернуть запрос к БД
    private OracleDataReader FResult = null;
		private OracleCommand FStatement = null;
		protected String FSQL = null;
		protected String FPreparedSQL = null;
    protected Hashtable FRowValues = null;
    protected Int64 FCurFetchedRowPos = 0;
    protected Boolean FCloseConnOnClose = false;

    /// <summary>
    /// Конструктор
    /// </summary>
    public CSQLCmd() {
      this.FStatement = new OracleCommand();
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pConn"></param>
    public CSQLCmd(IDbConnection pConn):this() {
      this.FStatement.Connection = (OracleConnection)pConn;
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="sess"></param>
    public CSQLCmd(IDBSession sess):this() {
      this.FStatement.Connection = (OracleConnection)sess.GetConnection();
      this.FCloseConnOnClose = true;
      //if (sess is IDBSession)
      //  this.FCloseConnOnClose = !(sess as IDBSession).ConnectionIsShared;
    }

    protected virtual void prepareSQL() {
      this.FPreparedSQL = this.FSQL;
    }

    /// <summary>
    /// Инициализирует объект 
    /// </summary>
    /// <param name="pSQL"></param>
    /// <param name="pParams"></param>
    public virtual void Init(String pSQL, CParams pParams) {
      this.FSQL = pSQL;
      this.FParams = pParams;
      this.prepareSQL();
    }

    /// <summary>
    /// Текущие параметры запроса
    /// </summary>
    public CParams Params {
      get {
        return this.FParams;
      }
    }

    /// <summary>
    /// Текущее соединение с БД
    /// </summary>
    public IDbConnection Connection {
      get {
        return this.FStatement.Connection;
      }
    }

		protected override void OnDispose(){
			try{
				this.Close();
			}catch{}
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
      this.Open(this.FStatement.Connection, timeout);
    }

    private static void setParamsToStatment(OracleCommand vOraCmd, CParams pParams) {
      setParamsToStatment(vOraCmd, pParams, true);
    }


    /// <summary>
    /// Вытаскивает из SQL все вызовы хранимых процедур
    /// </summary>
    /// <param name="pSQL"></param>
    /// <returns></returns>
    private static String[] detectExecsOfStoredPrcs(String pSQL) {
      const String csDelimiter = "+|+";
      String vResultStr = null;
      Regex vr = new Regex(@"\b[\w$]+\b[.]\b[\w$]+\b\s*[(]\s*[$]PRMLIST\s*[)]", RegexOptions.IgnoreCase);
      Match m = vr.Match(pSQL);
      while(m.Success) {
        //Console.WriteLine("fnd :" + m.Value);
        Utl.AppendStr(ref vResultStr, m.Value, csDelimiter);
        m = m.NextMatch();
      }
      if(!String.IsNullOrEmpty(vResultStr))
        return Utl.SplitString(vResultStr, csDelimiter);
      else
        return new String[0];
    }

    /// <summary>
    /// Вытаскивает из вызова хранимой процедуры пару pPkgName-{имя пакета}, pObjName-{имя проц}
    /// </summary>
    /// <param name="pSQL"></param>
    /// <returns></returns>
    private static void detectAutoDBMSParamsParts(String pSQL, ref String vPkgName, ref String vObjName) {
      vPkgName = null;
      vObjName = null;
      Regex vr = new Regex(@"\b[\w$]+\b[.]", RegexOptions.IgnoreCase);
      Match m = vr.Match(pSQL);
      if(m.Success) 
        vPkgName = m.Value.Substring(0, m.Value.Length-1);
      
      vr = new Regex(@"[.]\b[\w$]+\b", RegexOptions.IgnoreCase);
      m = vr.Match(pSQL);
      if(m.Success) 
        vObjName = m.Value.Substring(1);
    }

    private static void detectSQLParams1(OracleCommand vOraCmd) {
      String vSQL = vOraCmd.CommandText;
      Regex vr = new Regex("(['])(.*?)\\1", RegexOptions.IgnoreCase);
      vSQL = vr.Replace(vSQL, "");
      vr = new Regex(@":[\w#$]+", RegexOptions.IgnoreCase);
      Match m = vr.Match(vSQL);
      while(m.Success) {
        //Console.WriteLine("fnd :" + m.Value);
        String vParName = m.Value.Substring(1);
        if(SQLUtils.findOraParam(vOraCmd.Parameters, vParName) == null) {
          OracleDbType vParType = SQLUtils.DetectOraTypeByFldName(vParName);
          //OracleParameter op = vOraCmd.Parameters.Add(vParName, (vSQLPrm.ParamType == null) ? SQLUtils.DetectOraTypeByFldName(vSQLPrm.Name) : SQLUtils.DetectOraTypeByType(vSQLPrm.ParamType));
          OracleParameter op = vOraCmd.Parameters.Add(vParName, vParType);
        }
        m = m.NextMatch();
      }
    }

    private const String csGetParamsFromDBMS = "select "+
            " argument_name, position, sequence, data_type, in_out, data_length" +
            " from user_arguments" +
            " where package_name = upper(:package_name)" +
            " and object_name = upper(:object_name)" +
            " order by position";
    private const String csArgPrefix = "P_";
    private static void detectSQLParams2(OracleCommand vOraCmd, ref Boolean vIsPRMLIST_AUTO) {

      String vSQL = vOraCmd.CommandText;
      Regex vr = new Regex(@"[(]\s*[$]PRMLIST\s*[)]", RegexOptions.IgnoreCase);
      Match m = vr.Match(vSQL);
      vIsPRMLIST_AUTO = m.Success;

      if(vIsPRMLIST_AUTO) {
        String[] vExecs = detectExecsOfStoredPrcs(vSQL);
        for(int i = 0; i < vExecs.Length; i++) {
          String vArgs = null; String vPkgName = null; String vObjName = null;
          detectAutoDBMSParamsParts(vExecs[i], ref vPkgName, ref vObjName);
          OracleCommand vStatement = new OracleCommand(SQLUtils.PrepareSQLForOlacleExecute(csGetParamsFromDBMS));
          vStatement.Connection = vOraCmd.Connection; 
          try {
            vStatement.Parameters.Add("package_name", OracleDbType.Varchar2).Value = vPkgName;
            vStatement.Parameters.Add("object_name", OracleDbType.Varchar2).Value = vObjName;
            OracleDataReader vReader = vStatement.ExecuteReader();
            try {
              while(vReader.Read()) {
                Int32 vFldIndx = vReader.GetOrdinal("argument_name");
                if(!vReader.IsDBNull(vFldIndx)) {
                  String vParName = vReader.GetString(vFldIndx);
                  if(!vParName.Substring(0, 2).ToUpper().Equals(csArgPrefix))
                    throw new EBioDOA("Не верный формат наименования аргументов хранимой процедуры.\n" +
                                                "Для использования автогенерации аргументов с помощью переменной $PRMLIST\n" +
                                                "необходимо, чтобы все имена аргументов начинались с префикса " + csArgPrefix + " !");
                  vParName = vParName.Substring(2);
                  Utl.AppendStr(ref vArgs, ":" + vParName, ",");
                  String vParTypeName = vReader.GetString(vReader.GetOrdinal("data_type"));
                  String vParDirName = vReader.GetString(vReader.GetOrdinal("in_out"));
                  Object vLenObj = vReader.GetValue(vReader.GetOrdinal("data_length"));
                  Int32 vParLength = ((vLenObj == DBNull.Value) ? 0 : Utl.Convert2Type<Int32>(vLenObj));
                  if (SQLUtils.findOraParam(vOraCmd.Parameters, vParName) == null) {
                    OracleDbType vParType = SQLUtils.DetectOraTypeByOraTypeName(vParTypeName);
                    OracleParameter op = vOraCmd.Parameters.Add(vParName, vParType);
                    op.Direction = SQLUtils.DetectParamDirByName(vParDirName);
                    if(op.Direction == ParameterDirection.Output)
                      op.Size = vParLength;
                  }
                }
              }
            } finally {
              vReader.Close();
              vReader.Dispose();
            }
          } finally {
            //vStatement.Cancel();
            vStatement.Dispose();
          }
          String vNewExec = vExecs[i];
          Utl.regexReplace(ref vNewExec, @"[(]\s*[$]PRMLIST\s*[)]", "(" + vArgs + ")", true);
          vSQL = vSQL.Replace(vExecs[i], vNewExec);
        }
        vOraCmd.CommandText = vSQL;
      }
    }

    private static void detectSQLParamOutCursor(OracleCommand vOraCmd, ref OracleParameter refCursor) {

      if (vOraCmd.CommandType == CommandType.StoredProcedure) {
        String vArgs = null; String vPkgName = null; String vObjName = null;
        detectAutoDBMSParamsParts(vOraCmd.CommandText, ref vPkgName, ref vObjName);
        OracleCommand vStatement = new OracleCommand(SQLUtils.PrepareSQLForOlacleExecute(csGetParamsFromDBMS));
        vStatement.Connection = vOraCmd.Connection;
        try {
          vStatement.Parameters.Add("package_name", OracleDbType.Varchar2).Value = vPkgName;
          vStatement.Parameters.Add("object_name", OracleDbType.Varchar2).Value = vObjName;
          OracleDataReader vReader = vStatement.ExecuteReader();
          try {
            while (vReader.Read()) {
              Int32 vFldIndx = vReader.GetOrdinal("argument_name");
              if (!vReader.IsDBNull(vFldIndx)) {
                String vParName = vReader.GetString(vFldIndx);
                String vParTypeName = vReader.GetString(vReader.GetOrdinal("data_type"));
                String vParDirName = vReader.GetString(vReader.GetOrdinal("in_out"));
                if (vParTypeName.Equals("REF CURSOR") && (vParDirName.Equals("IN/OUT") || vParDirName.Equals("OUT"))) {
                  if (SQLUtils.findOraParam(vOraCmd.Parameters, vParName) == null) {
                    refCursor = vOraCmd.Parameters.Add(vParName, OracleDbType.RefCursor);
                    refCursor.Direction = SQLUtils.DetectParamDirByName(vParDirName);
                  }
                } else {
                  if (SQLUtils.findOraParam(vOraCmd.Parameters, vParName) == null) {
                    OracleDbType vParType = SQLUtils.DetectOraTypeByOraTypeName(vParTypeName);
                    OracleParameter op = vOraCmd.Parameters.Add(vParName, vParType);
                    op.Direction = SQLUtils.DetectParamDirByName(vParDirName);
                    if (op.Direction == ParameterDirection.Output) {
                      Object vLenObj = vReader.GetValue(vReader.GetOrdinal("data_length"));
                      Int32 vParLength = ((vLenObj == DBNull.Value) ? 0 : Utl.Convert2Type<Int32>(vLenObj));
                      op.Size = vParLength;
                    }
                  }
                }
              }
            }
          } finally {
            vReader.Close();
            vReader.Dispose();
          }
        } finally {
          //vStatement.Cancel();
          vStatement.Dispose();
        }
      }
    }

    private static void setParamsToStatment(OracleCommand vOraCmd, CParams pParams, Boolean pOverwrite) {
      OracleParameter refCursor = null;
      setParamsToStatment(vOraCmd, pParams, pOverwrite, ref refCursor);
    }

    private static void setParamsToStatment(OracleCommand vOraCmd, CParams pParams, ref OracleParameter refCursor) {
      setParamsToStatment(vOraCmd, pParams, true, ref refCursor);
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
      //foreach(CParam pram in pParams) {
      //  ExecuteScript("begin utils.setContextParam(:p_name, :p_value); end;", vOraCmd.Connection, new CParams(new CParam("p_name", pram.Name), new CParam("p_value", pram.Value)));
      //}
      if (oraCmd == null)
        throw new ArgumentNullException("oraCmd");


      detectSQLParams1(oraCmd);
      Boolean vIsPRMLIST_AUTO = false;
      detectSQLParams2(oraCmd, ref vIsPRMLIST_AUTO);
      detectSQLParamOutCursor(oraCmd, ref refCursor);
      foreach (OracleParameter vPrm in oraCmd.Parameters) {
        CParam vInPrm = SQLUtils.findParam(prms, vPrm.ParameterName);
        if(vInPrm != null) {

          if(vIsPRMLIST_AUTO) {
            vInPrm.ParamType = SQLUtils.DetectTypeByOraType(vPrm.OracleDbType);
            vInPrm.ParamDir = SQLUtils.encodeParamDirection(vPrm.Direction);
          } else {
            vPrm.Direction = SQLUtils.decodeParamDirection(vInPrm.ParamDir);
            if((vInPrm.ParamType == null) && (vInPrm.Value != null))
              vInPrm.ParamType = vInPrm.Value.GetType();
            if(vInPrm.ParamType != null)
              vPrm.OracleDbType = SQLUtils.DetectOraTypeByType(
                vInPrm.ParamType, (!String.IsNullOrEmpty(vInPrm.ValueAsString()) ? vInPrm.ValueAsString().Length : vInPrm.ParamSize));
          }


          Object vParamValue = SQLUtils.findParamValue(prms, vPrm);
          Object vParamValueDest = null;
          Type vTargetType = SQLUtils.DetectTypeByOraType(vPrm.OracleDbType);
          try {
            if(vParamValue != null) {
              if (vParamValue.GetType().Equals(typeof(String))) {
                if (vTargetType.Equals(typeof(System.Decimal))) {
                  vParamValueDest = SQLUtils.StrAsOraValue((String)vParamValue, OracleDbType.Decimal);
                } else if (vTargetType.Equals(typeof(System.DateTime)))
                  vParamValueDest = SQLUtils.StrAsOraValue((String)vParamValue, OracleDbType.Date);
                else
                  vParamValueDest = vParamValue;
              } else {
                vParamValueDest = Utl.Convert2Type(vParamValue, vTargetType);
              }
            }
          } catch(ThreadAbortException) {
            throw;
          } catch(Exception ex) {
            throw new EBioException("Ошибка при присвоении значения параметру [{" + vPrm.ParameterName + "(" + vTargetType + ")" + "}=" + vParamValue + "], Сообщение: " + ex.Message, ex);
          }
          if(overwrite) {
            vPrm.Value = (vParamValueDest == null) ? DBNull.Value : vParamValueDest;
          } else {
            if(vPrm.Value == null)
              vPrm.Value = (vParamValueDest == null) ? DBNull.Value : vParamValueDest;
          }

          if (vPrm.Direction == ParameterDirection.Output || vPrm.Direction == ParameterDirection.InputOutput) {
            if(vPrm.OracleDbType == OracleDbType.Varchar2) {
              vPrm.Size = (vInPrm.ParamSize > 0) ? vInPrm.ParamSize : 32000;
            }
          }
        } else {
          if(vPrm.Value == null)
            vPrm.Value = DBNull.Value;
        }
      }
    }

    private static void setStatmentToParams(OracleCommand oraCmd, CParams prms) {
      if(prms != null) {
        foreach(OracleParameter vPrm in oraCmd.Parameters) {
          if((vPrm.Direction == ParameterDirection.Output) ||
              (vPrm.Direction == ParameterDirection.InputOutput) ||
                (vPrm.Direction == ParameterDirection.ReturnValue)) {
            CParam vInPrm = SQLUtils.findParam(prms, vPrm.ParameterName);
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
      IDictionary<String, String> vConnStrs = Utl.parsConnectionStr(connStr);
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
      throw new EBioException("[" + detectDBName(conn.ConnectionString) + "] Ошибка при открытии курсора.\r\nСообщение: " + ex.Message + "\r\nSQL: " + this.FStatement.CommandText + "\r\n" + "Параметры запроса:{" + pParams + "}", ex);
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
      this.FStatement.Connection = (OracleConnection)pConn;
      this.FStatement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.FPreparedSQL);
      this.FStatement.CommandTimeout = timeout;
      try {
        this.onBeforeOpen();
        this.FStatement.Parameters.Clear();
        setParamsToStatment(this.FStatement, this.FParams);
        this.LastOpenParamsDebugText = bldDbgParamsStr(this.FStatement);
        rslt = this.FStatement.ExecuteReader();
        return rslt;
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        this._processOpenError(this.FStatement.Connection, ex, this.LastOpenParamsDebugText);
        return null;
      }
    }

    private OracleDataReader _openReaderAsProcedure(IDbConnection pConn, Int32 timeout) {
      OracleDataReader rslt = null;
      this.FStatement.Connection = (OracleConnection)pConn;
      this.FStatement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.FPreparedSQL);
      this.FStatement.CommandType = CommandType.StoredProcedure;
      this.FStatement.CommandTimeout = timeout;
      try {
        this.onBeforeOpen();
        this.FStatement.Parameters.Clear();
        OracleParameter refCursor = null;
        setParamsToStatment(this.FStatement, this.FParams, ref refCursor);
        this.LastOpenParamsDebugText = bldDbgParamsStr(this.FStatement);
        this.FStatement.ExecuteNonQuery();
        if (refCursor != null) {
          rslt = ((OracleRefCursor)refCursor.Value).GetDataReader();
          return rslt;
        } else
          return null;
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        this._processOpenError(this.FStatement.Connection, ex, this.LastOpenParamsDebugText);
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
      var cmdType = Utl.detectCommandType(this.FPreparedSQL);
      if (cmdType == CommandType.Text)
        this.FResult = this._openReaderAsSelect(pConn, timeout);
      else
        this.FResult = this._openReaderAsProcedure(pConn, timeout);
      this.FCurFetchedRowPos = 0;
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
        CSQLCmd.setParamsToStatment(stmt, prms);
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
    //    CSQLCmd.setParamsToStatment(stmt, prms);
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
        if((oe.Errors.Count > 0) && (oe.Errors[0].Number == ciORAERRCODE_USER_BREAKED))
          throw new EBioSQLBreaked(oe);
        for(int i = 0; String.IsNullOrEmpty(msg) && i < oe.Errors.Count; i++)
          if(oe.Errors[i].Number >= ciORAERRCODE_APP_ERR_START)
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
      return ExecuteScalarSQL(conn, this.PreparedSQL, this.FParams, timeout);
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
        return this.FPreparedSQL;
      }
		}
		
    /// <summary>
    /// SQL - предложение исходное
    /// </summary>
		public String SQL{
      get{
        return this.FSQL;
      }
		}

    /// <summary>
    /// Переоткрывает текущий курсор
    /// </summary>
    public void Refresh(Int32 timeout) {
      this.Refresh(this.FStatement.Connection, timeout);
    }

    /// <summary>
    /// Переоткрывает текущий курсор
    /// </summary>
    /// <param name="conn"></param>
    public void Refresh(IDbConnection conn, Int32 timeout) {
      checkOraConn(conn);
      this.Close();
      this.FStatement.Connection = (OracleConnection)conn;
      this.FStatement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.FPreparedSQL);
      this.FStatement.CommandTimeout = timeout;
			try{
        this.FResult = this.FStatement.ExecuteReader();
        this.FCurFetchedRowPos = 0;
			}catch (Exception ex){
        throw new EBioException("Ошибка при обновлении курсора. Сообщение: " + ex.Message + " SQL: " + this.FPreparedSQL);
			}
		}

    /// <summary>
    /// Прервать открытие текущего курсора
    /// </summary>
    public void Cancel() {
      if(this.FStatement != null) {
        this.FStatement.Cancel();
        this.FResult = null;
      }
    }

    /// <summary>
    /// Закрывает текущий курсор
    /// </summary>
		public void  Close(){
      if((this.FStatement != null) && (this.IsActive)) {
        this.FResult.Close();
        this.FStatement.Cancel();
        this.FResult = null;
        if(this.FCloseConnOnClose)
          this.FStatement.Connection.Close();
			}
		}
		
    /// <summary>
    /// Переход к следующей записи в текущем курсоре
    /// </summary>
    /// <returns></returns>
		public bool Next(){
      try{
        if(this.FRowValues == null)
          this.FRowValues = new Hashtable();
        this.FRowValues.Clear();
        if(this.FResult.Read()) {
          this.FCurFetchedRowPos++;
          if((ciFetchedRowLimit > 0) && (this.FCurFetchedRowPos > ciFetchedRowLimit)) {
            String vIOCode = null;// (this is CSQLCursorBio) ? "{" + (this as CSQLCursorBio).IOCode + "} " : null;
            String vMsg = "Запрос " + vIOCode + "вернул более " + ciFetchedRowLimit + " записей. Проверьте параметры запроса.";
            throw new EBioDOATooMuchRows(vMsg);
          }
          for (int i = 0; i < this.FResult.FieldCount; i++) {
            Object vObj;
            if (!this.FResult.IsDBNull(i)) {
              vObj = SQLUtils.OraDbValueAsObject(this.FResult.GetOracleValue(i));
            } else
              vObj = null;
            this.FRowValues[this.FResult.GetName(i).ToUpper()] = vObj;
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
        if(this.FResult == null)
			    return false;
        else
          return !this.FResult.IsClosed;
      }
		}
		
    /// <summary>
    /// Ссылка на OracleDataReader курсора
    /// </summary>
    public IDataReader DataReader{
      get{
        if((this.FResult != null) && (!this.FResult.IsClosed))
          return this.FResult;
        else
          return null;
      }
    }

    public IDbCommand DbCommand {
      get {
        return this.FStatement;
      }
    }

    /// <summary>
    /// Значения текущей записи
    /// </summary>
    public Hashtable RowValues {
      get {
        return this.FRowValues;
      }
    }

    /// <summary>
    /// Кол-во записей пройденых командой Next
    /// </summary>
    public Int64 CurFetchedRowPos {
      get {
        return this.FCurFetchedRowPos;
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