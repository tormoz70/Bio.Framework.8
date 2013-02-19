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
  /// ������� � ����� ����������� ��� ��� �������� �������
  /// ��������� �������� ������ ��� ������ � UniDir ��������
  /// </summary>
	public class SQLCmd:DisposableObject{
    private Params _params;

    protected const Int32 ORAERRCODE_APP_ERR_START = 20000; //������ ��������� ����� ������ ���������� � Oracle
    protected const Int32 ORAERRCODE_USER_BREAKED = 1013; //{"ORA-01013: ������������� ��������� ������ ������� ��������"}
    protected const Int32 FETCHED_ROW_LIMIT = 10000000; // ������������ ���-�� �������, ������� ����� ������� ������ � ��
    private OracleDataReader _dataReader;
		private readonly OracleCommand _statement;
		protected String originSQL = null;
		protected String preparedSQL = null;
    protected Hashtable rowValues = null;
    protected Int64 curFetchedRowPos = 0;
    protected Boolean closeConnOnClose = false;

    /// <summary>
    /// �����������
    /// </summary>
    public SQLCmd() {
      this._statement = new OracleCommand();
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="connection"></param>
    public SQLCmd(IDbConnection connection):this() {
      this._statement.Connection = (OracleConnection)connection;
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="session"></param>
    public SQLCmd(IDBSession session):this() {
      this._statement.Connection = (OracleConnection)session.GetConnection();
      this.closeConnOnClose = true;
    }

    protected virtual void prepareSQL() {
      this.preparedSQL = this.originSQL;
    }

    /// <summary>
    /// �������������� ������ 
    /// </summary>
    /// <param name="pSQL"></param>
    /// <param name="pParams"></param>
    public virtual void Init(String pSQL, Params pParams) {
      this.originSQL = pSQL;
      this._params = pParams;
      this.prepareSQL();
    }

    /// <summary>
    /// ������� ��������� �������
    /// </summary>
    public Params Params {
      get {
        return this._params;
      }
    }

    /// <summary>
    /// ������� ���������� � ��
    /// </summary>
    public IDbConnection Connection {
      get {
        return this._statement.Connection;
      }
    }

    protected override void doOnDispose() {
      try {
        this.Close();
      } catch {}
    }

    protected virtual void onBeforeOpen(){
		}

		protected virtual void doOnAfterOpen(){
		}

    private static void _checkOraConn(IDbConnection pConn) {
      if(pConn == null)
        throw new EBioException("�� ���������� ������!");
      switch(pConn.State) {
        case ConnectionState.Broken: throw new EBioException("���������� � �� ��������!");
        case ConnectionState.Closed: throw new EBioException("���������� � �� �� �����������!");
        case ConnectionState.Connecting: throw new EBioException("������������ ���������� � �� ...");
        case ConnectionState.Executing:
        case ConnectionState.Fetching: throw new EBioException("���������� ������ ������ ���������!");
      }
    }

    /// <summary>
    /// ��������� ������
    /// </summary>
    public void Open(Int32 timeout) {
      this.Open(this._statement.Connection, timeout);
    }


    /// <summary>
    /// ����������� �� SQL ��� ������ �������� ��������
    /// </summary>
    /// <param name="pSQL"></param>
    /// <returns></returns>
    private static IEnumerable<string> _detectExecsOfStoredPrcs(String pSQL) {
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
    /// ����������� �� ������ �������� ��������� ���� pPkgName-{��� ������}, pObjName-{��� ����}
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="pkgName"></param>
    /// <param name="objName"></param>
    /// <returns></returns>
    private static void _detectAutoDBMSParamsParts(String sql, out String pkgName, out String objName) {
      pkgName = Utl.RegexFind(sql, @"\b[\w$]+\b(?=[.])", true);
      objName = Utl.RegexFind(sql, @"(?<=[.])\b[\w$]+\b(?=\s*[(;])", true);
    }

    private static void _detectSQLParams1(OracleCommand oraCommand) {
      var v_sql = oraCommand.CommandText;
      // ������� ��� ��������� ���������
      Utl.RegexReplace(ref v_sql, @"(['])(.*?)\1", "", true);

      // ������� ��� ��������� ���� :qwe_ad
      var m = Utl.RegexMatch(v_sql, @"(?<=:)\b[\w\#\$]+", true);
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
        var v_execs = _detectExecsOfStoredPrcs(v_sql);
        foreach (var t in v_execs) {
          String v_args = null; String v_pkgName; String v_objName;
          _detectAutoDBMSParamsParts(t, out v_pkgName, out v_objName);
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
                    throw new EBioDOA("�� ������ ������ ������������ ���������� �������� ���������.\n" +
                                      "��� ������������� ������������� ���������� � ������� ���������� $PRMLIST\n" +
                                      "����������, ����� ��� ����� ���������� ���������� � �������� " + _DEFAULT_ARGPREFIX + " !");
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
        _detectAutoDBMSParamsParts(vOraCmd.CommandText, out v_pkgName, out v_objName);
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

    private static void _setParamsToStatment(OracleCommand oraCmd, Params prms, Boolean overwrite = true) {
      OracleParameter v_refCursor = null;
      _setParamsToStatment(oraCmd, prms, overwrite, ref v_refCursor);
    }

    private static void _setParamsToStatment(OracleCommand oraCmd, Params prms, ref OracleParameter refCursor) {
      _setParamsToStatment(oraCmd, prms, true, ref refCursor);
    }

    /// <summary>
    /// ������ ��������� ������� ��������� ������� � ����������� �������� ���� ����������.
    /// </summary>
    /// <param name="oraCmd"></param>
    /// <param name="prms"></param>
    /// <param name="overwrite"></param>
    /// <param name="refCursor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private static void _setParamsToStatment(OracleCommand oraCmd, Params prms, Boolean overwrite, ref OracleParameter refCursor) {
      oraCmd.BindByName = true;
      if (oraCmd == null)
        throw new ArgumentNullException("oraCmd");


      _detectSQLParams1(oraCmd);
      Boolean v_isParamListAuto;
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
            throw new EBioException("������ ��� ���������� �������� ��������� [{" + v_prm.ParameterName + "(" + v_targetType + ")" + "}=" + v_paramValue + "], ���������: " + ex.Message, ex);
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

    private static void _setStatmentToParams(OracleCommand oraCmd, Params prms) {
      if(prms != null) {
        foreach(OracleParameter v_prm in oraCmd.Parameters) {
          if((v_prm.Direction == ParameterDirection.Output) ||
              (v_prm.Direction == ParameterDirection.InputOutput) ||
                (v_prm.Direction == ParameterDirection.ReturnValue)) {
            var v_inPrm = SQLUtils.FindParam(prms, v_prm.ParameterName);
            if(v_inPrm != null) {
              v_inPrm.Value = SQLUtils.OraDbValueAsObject(v_prm.Value);
              //vInPrm.Value = vPrm.Value;
            }
          }
        }
      }
    }

    protected static String detectDBName(String connStr) {
      String v_result = null;
      var v_connStrs = Utl.ParsConnectionStr(connStr);
      if (v_connStrs.ContainsKey("Data Source"))
        v_result = v_connStrs["Data Source"];
      if (v_connStrs.ContainsKey("User ID"))
        v_result = (v_result != null) ? v_connStrs["User ID"] + "@" + v_result : v_connStrs["User ID"];
      return v_result;
    }

    private static void processTimeoutError(Exception ex) {
      if ((ex is OracleException) && (((OracleException)ex).Number == 1013))
        throw new EBioSQLTimeout(ex);
    }

    protected virtual void throwOpenError(IDbConnection connection, Exception ex, String sql, String @params) {
      throw new EBioException("[" + detectDBName(connection.ConnectionString) + "] ������ ��� �������� �������.\r\n���������: " + ex.Message + "\r\nSQL: " + sql + "\r\n" + "��������� �������:{" + @params + "}", ex);
    }
    private static void _throwExecError(IDbConnection connection, Exception ex, String sql, String @params) {
      throw new EBioException("[" + detectDBName(connection.ConnectionString) + "] ������ ���������� ������� � ��.\r\n���������: " + ex.Message + "\r\nSQL: " + sql + "\r\n" + "��������� �������:{" + @params + "}", ex);
    }


    private void _processOpenError(IDbConnection connection, Exception ex, String sql, String @params) {
      processTimeoutError(ex);
      this.throwOpenError(connection, ex, sql, @params);
    }

    private static void _processExecError(IDbConnection connection, Exception ex, String sql, String @params) {
      processTimeoutError(ex);
      _throwExecError(connection, ex, sql, @params);
    }

    /// <summary>
    /// ���������� �������� ���������� �������������� ��� ��������� �������� ������� (��� �������).
    /// </summary>
    public String LastOpenParamsDebugText { get; private set; }

    private OracleDataReader _openReaderAsSelect(IDbConnection connection, Int32 timeout) {
      this._statement.Connection = (OracleConnection)connection;
      this._statement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.preparedSQL);
      this._statement.CommandTimeout = timeout;
      try {
        this.onBeforeOpen();
        this._statement.Parameters.Clear();
        _setParamsToStatment(this._statement, this._params);
        this.LastOpenParamsDebugText = BuildDebugParamsStr(this._statement);
        var rslt = this._statement.ExecuteReader();
        return rslt;
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        this._processOpenError(this._statement.Connection, ex, this.preparedSQL, this.LastOpenParamsDebugText);
        return null;
      }
    }

    private OracleDataReader _openReaderAsProcedure(IDbConnection connection, Int32 timeout) {
      this._statement.Connection = (OracleConnection)connection;
      this._statement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.preparedSQL);
      this._statement.CommandType = CommandType.StoredProcedure;
      this._statement.CommandTimeout = timeout;
      try {
        this.onBeforeOpen();
        this._statement.Parameters.Clear();
        OracleParameter v_refCursor = null;
        _setParamsToStatment(this._statement, this._params, ref v_refCursor);
        this.LastOpenParamsDebugText = BuildDebugParamsStr(this._statement);
        this._statement.ExecuteNonQuery();
        return v_refCursor != null ? ((OracleRefCursor)v_refCursor.Value).GetDataReader() : null;
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        this._processOpenError(this._statement.Connection, ex, this.preparedSQL, this.LastOpenParamsDebugText);
        return null;
      }
    }

    /// <summary>
    /// ��������� ������
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="timeout"></param>
    public void Open(IDbConnection connection, Int32 timeout) {
      _checkOraConn(connection);
      var v_cmdType = Utl.DetectCommandType(this.preparedSQL);
      this._dataReader = v_cmdType == CommandType.Text ? this._openReaderAsSelect(connection, timeout) : this._openReaderAsProcedure(connection, timeout);
      this.curFetchedRowPos = 0;
      this.doOnAfterOpen();
    }

    /// <summary>
    /// ������ �������� ���������� ���� ���������
    /// </summary>
    /// <param name="statment"></param>
    /// <returns></returns>
    public static String BuildDebugParamsStr(IDbCommand statment) {
      var v_reslt = new StringBuilder();
      foreach(OracleParameter v_prm in ((OracleCommand)statment).Parameters) {
        var v_dir = "(in)";
        switch (v_prm.Direction) {
          case ParameterDirection.InputOutput:
            v_dir = "(in out)";
            break;
          case ParameterDirection.Output:
            v_dir = "(out)";
            break;
          case ParameterDirection.ReturnValue:
            v_dir = "(ret)";
            break;
        }
        v_reslt.AppendLine(v_prm.ParameterName + v_dir + "[" + v_prm.Value + "]:("+v_prm.DbType+");");
      }
        
      return v_reslt.ToString();
    }

    /// <summary>
    /// ������� � �������������� Command ��� �������
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    /// <exception cref="EBioSQLBreaked"></exception>
    /// <exception cref="EBioException"></exception>
    public static IDbCommand PrepareCommand(IDbConnection connection, String sql, Params prms, Int32 timeout) {
      _checkOraConn(connection);
      var stmt = new OracleCommand();
      stmt.CommandText = SQLUtils.PrepareSQLForOlacleExecute(sql);
      stmt.Connection = (OracleConnection)connection;
      stmt.CommandTimeout = timeout;
      try {
        _setParamsToStatment(stmt, prms);
      } catch (ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch (Exception ex) {
        throw new EBioException("������ ���������� ������� � ��.\r\n���������: " + ex.Message, ex);
      }
      return stmt;
    }

    /// <summary>
    /// ��������� ������
    /// </summary>
    /// <param name="stmt"></param>
    /// <param name="sql"></param>
    /// <param name="params"></param>
    /// <exception cref="EBioSQLBreaked"></exception>
    /// <exception cref="EBioException"></exception>
    public static void ExecuteScript(IDbCommand stmt, String sql, Params @params) {
      String v_dbgPrmsStr = null;
      try {
        v_dbgPrmsStr = BuildDebugParamsStr(stmt);
        stmt.ExecuteNonQuery();
        _setStatmentToParams((OracleCommand)stmt, @params);
      } catch(OracleException oe) {
        String msg = String.Empty;
        if((oe.Errors.Count > 0) && (oe.Errors[0].Number == ORAERRCODE_USER_BREAKED))
          throw new EBioSQLBreaked(oe);
        for(int i = 0; String.IsNullOrEmpty(msg) && i < oe.Errors.Count; i++)
          if(oe.Errors[i].Number >= ORAERRCODE_APP_ERR_START)
            msg = oe.Errors[i].Message;
        throw new EBioException((String.IsNullOrEmpty(msg)) ? "������ ���������� ������� � ��.\r\n���������: " + oe.Message + "\r\n" +
                              "SQL: " + stmt.CommandText + "\r\n" + "��������� �������:{" + (String.IsNullOrEmpty(v_dbgPrmsStr) ? ((@params != null) ? @params.ToString() : null) : v_dbgPrmsStr) + "}" : msg);
      } catch(ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch(Exception ex) {
        _processExecError(stmt.Connection, ex, sql, v_dbgPrmsStr);
      }
    }

    /// <summary>
    /// ��������� ������
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    public static void ExecuteScript(IDbConnection conn, String sql, Params prms, Int32 timeout) {
      var stmt = PrepareCommand(conn, sql, prms, timeout);
      ExecuteScript(stmt, sql, prms);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sess"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    public static void ExecuteScript(IDBSession sess, String sql, Params prms, Int32 timeout) {
      IDbConnection conn = sess.GetConnection();
      try {
        ExecuteScript(conn, sql, prms, timeout);
      } finally {
        conn.Close();
      }
    }

    /// <summary>
    /// ����������� ������ ����������
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public String ExtractOracleApplicationError(String message) {
      if (message != null) {
        var v_errorCode = Utl.RegexFind(message, @"(?<=ORA-)2\d{4}(?=:.*[^$]*ORA-06512)", true);
        var v_message = Utl.RegexFind(message, @"(?<=ORA-2\d{4}:).*[^$]*(?=ORA-06512)", true);
        return String.Format("[{0}] - {1}", v_errorCode, v_message);
      }
      return null;
    }

    /// <summary>
    /// ��������� ������ � ��, ������� ���������� ���� ��������
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public Object ExecuteScalarSQL(IDbConnection connection, Int32 timeout) {
      return ExecuteScalarSQL(connection, this.PreparedSQL, this._params, timeout);
    }

    /// <summary>
    /// ��������� ������ � ��, ������� ���������� ���� ��������
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="connection"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static Object ExecuteScalarSQL(IDbConnection connection, String sql, Params prms, Int32 timeout) {
      _checkOraConn(connection);
      var stmt = new OracleCommand();
      stmt.CommandText = SQLUtils.PrepareSQLForOlacleExecute(sql);
			stmt.Connection = (OracleConnection)connection;
      stmt.CommandTimeout = timeout;
      String v_paramsToDebug = null;
      Object o = null;
			try{
        stmt.Parameters.Clear();
        _setParamsToStatment(stmt, prms);
        v_paramsToDebug = BuildDebugParamsStr(stmt);
				o = stmt.ExecuteScalar();
      } catch(ThreadAbortException ex) {
        throw new EBioSQLBreaked(ex);
      } catch(Exception ex) {
        _processExecError(stmt.Connection, ex, sql, v_paramsToDebug);
			}
			return o;
		}

    /// <summary>
    /// ��������� ������ � ��, ������� ���������� ���� ��������
    /// </summary>
    /// <param name="sess"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static Object ExecuteScalarSQL(IDBSession sess, String sql, Params prms, Int32 timeout) {
      var conn = sess.GetConnection();
      try {
        return ExecuteScalarSQL(conn, sql, prms, timeout);
      } finally {
        conn.Close();
      }
    }

    /// <summary>
    /// SQL - ����������� �������������� � ����������
    /// </summary>
		public String PreparedSQL{
      get{
        return this.preparedSQL;
      }
		}
		
    /// <summary>
    /// SQL - ����������� ��������
    /// </summary>
		public String SQL{
      get{
        return this.originSQL;
      }
		}

    /// <summary>
    /// ������������� ������� ������
    /// </summary>
    public void Refresh(Int32 timeout) {
      this.Refresh(this._statement.Connection, timeout);
    }

    /// <summary>
    /// ������������� ������� ������
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="timeout"></param>
    public void Refresh(IDbConnection conn, Int32 timeout) {
      _checkOraConn(conn);
      this.Close();
      this._statement.Connection = (OracleConnection)conn;
      this._statement.CommandText = SQLUtils.PrepareSQLForOlacleExecute(this.preparedSQL);
      this._statement.CommandTimeout = timeout;
			try{
        this._dataReader = this._statement.ExecuteReader();
        this.curFetchedRowPos = 0;
			}catch (Exception ex){
        throw new EBioException("������ ��� ���������� �������. ���������: " + ex.Message + " SQL: " + this.preparedSQL);
			}
		}

    /// <summary>
    /// �������� �������� �������� �������
    /// </summary>
    public void Cancel() {
      if(this._statement != null) {
        this._statement.Cancel();
        this._dataReader = null;
      }
    }

    /// <summary>
    /// ��������� ������� ������
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
    /// ������� � ��������� ������ � ������� �������
    /// </summary>
    /// <returns></returns>
		public bool Next(){
      try{
        if(this.rowValues == null)
          this.rowValues = new Hashtable();
        this.rowValues.Clear();
        if(this._dataReader.Read()) {
          this.curFetchedRowPos++;
          if((this.curFetchedRowPos > FETCHED_ROW_LIMIT)) {
            String v_ioCode = null;
            var v_msg = "������ " + v_ioCode + "������ ����� " + FETCHED_ROW_LIMIT + " �������. ��������� ��������� �������.";
            throw new EBioDOATooMuchRows(v_msg);
          }
          for (var i = 0; i < this._dataReader.FieldCount; i++) {
            var v_obj = !this._dataReader.IsDBNull(i) ? SQLUtils.OraDbValueAsObject(this._dataReader.GetOracleValue(i)) : null;
            this.rowValues[this._dataReader.GetName(i).ToUpper()] = v_obj;
          }
          return true;
        }
        return false;
      } catch(ThreadAbortException) {
        throw;
      } catch(Exception ex) {
        this.Close();
        throw new EBioException("������ ��� ���������� �������� �������. ���������: " + ex.Message, ex);
			}
		}
		
    /// <summary>
    /// IsActive
    /// </summary>
		public bool IsActive{
      get {
        if(this._dataReader == null)
			    return false;
        return !this._dataReader.IsClosed;
      }
    }
		
    /// <summary>
    /// ������ �� OracleDataReader �������
    /// </summary>
    public IDataReader DataReader{
      get {
        if((this._dataReader != null) && (!this._dataReader.IsClosed))
          return this._dataReader;
        return null;
      }
    }

    /// <summary>
    /// ������ �� IDbCommand
    /// </summary>
    public IDbCommand DbCommand {
      get {
        return this._statement;
      }
    }

    /// <summary>
    /// �������� ������� ������
    /// </summary>
    public Hashtable RowValues {
      get {
        return this.rowValues;
      }
    }

    /// <summary>
    /// ���-�� ������� ��������� �������� Next
    /// </summary>
    public Int64 CurFetchedRowPos {
      get {
        return this.curFetchedRowPos;
      }
    }

    internal SQLGarbageMonitor garbageMonitor = null;
  }
}