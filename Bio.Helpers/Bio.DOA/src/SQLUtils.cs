namespace Bio.Helpers.DOA {
  using System;
  using System.Text.RegularExpressions;
  using System.IO;
  using System.Threading;
  using System.Data;
  using System.Globalization;
  using Oracle.DataAccess.Client;
  using Oracle.DataAccess.Types;
  using System.Linq;
  using System.ComponentModel;
  using Common;
  using Common.Types;

  /// <summary>
  /// 
  /// </summary>
  public enum OracleDataType {
    [Description("VARCHAR2")]
    Varchar2 = OracleDbType.Varchar2,
    [Description("CLOB")]
    Clob = OracleDbType.Clob,
    [Description("CHAR")]
    Char = OracleDbType.Char,
    [Description("NUMBER")]
    Decimal = OracleDbType.Decimal,
    [Description("DATE")]
    Date = OracleDbType.Date,
    [Description("BLOB")]
    Blob = OracleDbType.Blob
  };
  /// <summary>
  /// ������� 
  /// </summary>
  public class SQLUtils {

    /// <summary>
    /// �������������� SQL ����������� � ���������� � Oracle
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static String PrepareSQLForOlacleExecute(String sql) {
      String v_rslt = null;
      if (sql != null) {
        var vr = new Regex("[-]{2}.*[^$]", RegexOptions.IgnoreCase);
        v_rslt = vr.Replace(sql, "");
        v_rslt = v_rslt.Replace("\n", " ");
        v_rslt = v_rslt.Replace("\r", " ");
        v_rslt = v_rslt.Replace("\t", " ");
      }
      return v_rslt;
    }

    /// <summary>
    /// ��������� ���� � BLOB ���� �������
    /// </summary>
    /// <param name="conn">���������� � ��</param>
    /// <param name="tableName">��� �������</param>
    /// <param name="fieldName">��� ����</param>
    /// <param name="keyField">��� ����-���������� �����</param>
    /// <param name="keyValue">�������� ��� ���������� �����</param>
    /// <param name="fileName">��� �����</param>
    /// <param name="deleteFile">������� ���� ����� ��������</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="EBioException"></exception>
    public static void SetFile2BLOBField(IDbConnection conn, String tableName, String fieldName, String keyField, Object keyValue, String fileName, Boolean deleteFile) {
      if(File.Exists(fileName)) {
        if(fileName == null)
          throw new ArgumentNullException("fileName");
        if(tableName == null)
          throw new ArgumentNullException("tableName");
        if(fieldName == null)
          throw new ArgumentNullException("fieldName");
        if(keyField == null)
          throw new ArgumentNullException("keyField");
        if(keyValue == null)
          throw new ArgumentNullException("keyValue");
        var sql = String.Format("update {0} set {1} = :{2} where {3} = :{4}", tableName, fieldName, fieldName, keyField, keyField);
        var oraSess = (OracleConnection)conn;
        var scriptCmd = oraSess.CreateCommand();
        scriptCmd.CommandText = sql;

        var parFile = scriptCmd.Parameters.Add(fieldName, OracleDbType.Blob);
        parFile.Direction = ParameterDirection.Input;
        byte[] v_buffer = null;
        Utl.ReadBinFileInBuffer(fileName, ref v_buffer);
        parFile.Value = v_buffer;

        var parKeyValue = scriptCmd.Parameters.Add(keyField, DetectOraTypeByType(keyValue.GetType()));
        parKeyValue.Value = keyValue;
        parKeyValue.Direction = ParameterDirection.Input;

        try {
          var r = scriptCmd.ExecuteNonQuery();
          if((deleteFile) && (r == 1))
            File.Delete(fileName);
        } catch(ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          var errm = new StringWriter();
          errm.WriteLine("������ ��� ���������� SQL. ���������: " + ex.Message);
          errm.WriteLine("SQL: " + scriptCmd.CommandText);
          throw new EBioException(errm.ToString(), ex);
        }
      }
    }

    /// <summary>
    /// ��������� ���� � BLOB ���� �������
    /// </summary>
    /// <param name="sess">������ ��</param>
    /// <param name="tableName">��� �������</param>
    /// <param name="fieldName">��� ����</param>
    /// <param name="keyField">��� ����-���������� �����</param>
    /// <param name="keyValue">�������� ��� ���������� �����</param>
    /// <param name="fileName">��� �����</param>
    /// <param name="deleteFile">������� ���� ����� ��������</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="EBioException"></exception>
    public static void SetFile2BLOBField(IDBSession sess, String tableName, String fieldName, String keyField, Object keyValue, String fileName, Boolean deleteFile) {
      var conn = sess.GetConnection();
      try {
        SetFile2BLOBField(conn, tableName, fieldName, keyField, keyValue, fileName, deleteFile);
      } finally {
        conn.Close();
      }
    }

    /// <summary>
    /// ��������� SQL ��� ������
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <exception cref="EBioException"></exception>
    public static void ExecSQLCommandNonQuery(IDbConnection conn, ref String sql, Params prms) {
      if(!String.IsNullOrEmpty(sql)) {
        var v_sql = PrepareSQLForOlacleExecute(sql);
        var oraSess = (OracleConnection)conn;
        var scriptCmd = oraSess.CreateCommand();
        scriptCmd.CommandText = v_sql;

        if(prms != null) {
          foreach (var t in prms) {
            var parReslt = scriptCmd.Parameters.Add(t.Name, (t.InnerObject != null) ? DetectOraTypeByType(t.InnerObject.GetType()) : OracleDbType.Varchar2);
            parReslt.Value = t.InnerObject;
            parReslt.Direction = ParameterDirection.Input;
          }
        }

        try {
          scriptCmd.ExecuteNonQuery();
        } catch(ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          var errm = new StringWriter();
          errm.WriteLine("������ ��� ���������� SQL. ���������: " + ex.Message);
          errm.WriteLine("SQL: " + scriptCmd.CommandText);
          throw new EBioException(errm.ToString(), ex);
        }
      }
    }

    /// <summary>
    /// ��������� SQL ��� ������
    /// </summary>
    /// <param name="sess"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    public static void ExecSQLCommandNonQuery(IDBSession sess, ref String sql, Params prms) {
      var conn = sess.GetConnection();
      try {
        ExecSQLCommandNonQuery(conn, ref sql, prms);
      } finally { 
        conn.Close();
      }
    }

    /// <summary>
    /// ���������� ��� ���� �� ��������
    /// </summary>
    /// <param name="fldName"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByFldName(String fldName) {
      if (Utl.RegexMatch(fldName, "^[VFPS]A_", true).Success)
        return OracleDbType.Varchar2;
      if (Utl.RegexMatch(fldName, "^[VFPS]N_", true).Success)
        return OracleDbType.Decimal;
      if (Utl.RegexMatch(fldName, "^[VFPS]D_", true).Success)
        return OracleDbType.Date;
      return OracleDbType.Varchar2;
    }

    /// <summary>
    /// ���������� ��� ���� �� ����� Oracle ����. ��������: VARCHAR2, NUMBER, DATE 
    /// </summary>
    /// <param name="pTypeName"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByOraTypeName(String pTypeName) {
      if(String.Equals(pTypeName, "VARCHAR2", StringComparison.OrdinalIgnoreCase))
        return OracleDbType.Varchar2;
      if(String.Equals(pTypeName, "CLOB", StringComparison.OrdinalIgnoreCase))
        return OracleDbType.Clob;
      if(String.Equals(pTypeName, "NUMBER", StringComparison.OrdinalIgnoreCase))
        return OracleDbType.Decimal;
      if(String.Equals(pTypeName, "DATE", StringComparison.OrdinalIgnoreCase))
        return OracleDbType.Date;
      return OracleDbType.Varchar2;
    }

    /// <summary>
    /// ������ ���� Clob
    /// </summary>
    public const Int32 ClobDBSize = 32 * 1024;

    /// <summary>
    /// ���������� ��� ���� �� ���� ������
    /// </summary>
    /// <param name="type"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByType(Type type, long size) {
      if(type == null)
        throw new ArgumentNullException("type");
      OracleDbType ot;
      if (type == typeof(String))
        ot = size < (ClobDBSize) ? OracleDbType.Varchar2 : OracleDbType.Clob;
      else if (type == typeof(OracleClob))
        ot = OracleDbType.Clob;
      else if ((type == typeof(Decimal)) || (type == typeof(Double)) ||
         (type == typeof(Int16)) || (type == typeof(Int32)) || (type == typeof(Int64)))
        ot = OracleDbType.Decimal;
      else if ((type == typeof(DateTime)) || (type == typeof(DateTime?)) ||
                (type == typeof(DateTimeOffset)) || (type == typeof(DateTimeOffset?)))
        ot = OracleDbType.Date;
      else if (type == typeof(Byte[]))
        ot = OracleDbType.Blob;
      else if (type == typeof(Boolean))
        ot = OracleDbType.Decimal;
      else
        throw new NotSupportedException("���������� ���������� ��� \"" + type.Name + "\".");
      return ot;
    }

    /// <summary>
    /// ���������� ��� ���� �� ���� ������
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByType(Type type) {
      return DetectOraTypeByType(type, 0);
    }

    /// <summary>
    /// ���������� ��� ������ �� ���� ������
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Type DetectTypeByOraType(OracleDbType type) {
      Type v_result;
      switch (type) {
        case OracleDbType.Varchar2:
        case OracleDbType.Char:
        case OracleDbType.NChar:
        case OracleDbType.NVarchar2:
        case OracleDbType.Clob:
          v_result = typeof(String);
          break;
        case OracleDbType.Byte:
        case OracleDbType.Double:
        case OracleDbType.Decimal:
        case OracleDbType.Int16:
        case OracleDbType.Int32:
        case OracleDbType.Int64:
          v_result = typeof(Decimal);
          break;
        case OracleDbType.Date:
        case OracleDbType.TimeStamp:
          v_result = typeof(DateTime);
          break;
        case OracleDbType.Blob:
          v_result = typeof(Byte[]);
          break;
        default:
          throw new NotSupportedException("���������� ������������� ��� \"" + type + "\".");
      }
      return v_result;
    }

    /// <summary>
    /// ������������ �������� ���� OracleDbType � Object
    /// </summary>
    /// <returns></returns>
    public static Object OraDbValueAsObject(Object value) {
      Object valueAsObject = null;
      if (value != null) {
        var type = value.GetType();
        if (type == typeof(OracleString)) {

          if (!((OracleString)value).IsNull) {
            var resultStr = ((OracleString)value).Value;
            valueAsObject = resultStr;
          }
        } else if (type == typeof(OracleDecimal)) {
          if (!((OracleDecimal)value).IsNull) {
            if (((OracleDecimal)value).IsInt) {
              var resultDec = ((OracleDecimal)value).ToInt64();
              valueAsObject = resultDec;
            } else {
              var resultDec = ((OracleDecimal)value).ToDouble();
              valueAsObject = new Decimal(resultDec);
            }
          }
        } else if (type == typeof(OracleDate)) {
          if (!((OracleDate)value).IsNull) {
            var resultDt = ((OracleDate)value).Value;
            valueAsObject = resultDt; //DateTime.SpecifyKind(vResultDt, DateTimeKind.Utc);
          }
        } else if (type == typeof(OracleTimeStamp)) {
          if (!((OracleTimeStamp)value).IsNull) {
            var resultDt = ((OracleTimeStamp)value).Value;
            valueAsObject = resultDt;
          }
        } else if (type == typeof(OracleClob)) {
          if (!((OracleClob)value).IsNull) {
            //var v_resultStr = Utl.EncodeANSI2UTF(((OracleClob)value).Value);
            var resultStr = ((OracleClob)value).Value;
            valueAsObject = resultStr;
          }
        } else if (type == typeof(OracleBlob)) {
          var resultAByte = ((OracleBlob)value).Value;
          valueAsObject = resultAByte;
        } else
          valueAsObject = value;
      }
      return valueAsObject;
    }

    /// <summary>
    /// ���������� ����������� ��������� �� �����. ��������: IN, OUT, IN/OUT
    /// </summary>
    /// <returns></returns>
    public static ParameterDirection DetectParamDirByName(String directionName) {
      var parDir = ParameterDirection.Input;
      if(String.Equals(directionName, "IN", StringComparison.OrdinalIgnoreCase))
        parDir = ParameterDirection.Input;
      else if(String.Equals(directionName, "OUT", StringComparison.OrdinalIgnoreCase))
        parDir = ParameterDirection.Output;
      else if(String.Equals(directionName, "IN/OUT", StringComparison.OrdinalIgnoreCase))
        parDir = ParameterDirection.InputOutput;
      return parDir;
    }

    /// <summary>
    /// ����������� ������ � ��� OracleDbType
    /// </summary>
    /// <param name="value">��������</param>
    /// <param name="oraType">���</param>
    /// <returns></returns>
    internal static Object StrAsOraValue(String value, OracleDbType oraType) {
      Object rslt = null;
      try {
        var culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true) {
          NumberFormat = { NumberDecimalSeparator = "." }
        };
        switch (oraType) {
          case OracleDbType.Varchar2:
          case OracleDbType.Clob:
            rslt = value;
            break;
          case OracleDbType.Decimal:
            rslt = Double.Parse(value.Replace(",", ".").Replace(" ", ""), culture);
            break;
          case OracleDbType.Date:
            rslt = DateTimeParser.Instance.ParsDateTime(value);
            break;
          case OracleDbType.Blob:
            rslt = Convert.FromBase64String(value);
            break;
        }
      } catch (Exception ex) {
        throw new EBioException(
          "��� ���������� �������� \"" + value + "\" � ���� " +
          enumHelper.NameOfValue(oraType) + " ", ex);
      }
      return rslt;
    }

    /// <summary>
    /// ����������� ������ � ��� OracleDbType
    /// </summary>
    /// <param name="value">��������</param>
    /// <param name="type">���</param>
    /// <returns></returns>
    public static Object StrAsOraValue(String value, OracleDataType type) {
      return StrAsOraValue(value, (OracleDbType)type);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pObject"></param>
    /// <returns></returns>
    public static String ObjectAsString(Object pObject) {
      String rslt = null;
      if (pObject != null) {
        var tp = pObject.GetType();
        if (tp == typeof(OracleClob))
          rslt = Utl.EncodeANSI2UTF(((OracleClob)pObject).Value);
        else if (tp == typeof(OracleBlob))
          rslt = Convert.ToBase64String(((OracleBlob)pObject).Value);
        else
          rslt = Utl.ObjectAsString(pObject);
      }
      return rslt;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static Param FindParam(Params prms, String paramName) {
      if ((prms != null) && !String.IsNullOrEmpty(paramName)) {
        return prms.FirstOrDefault(p => {
          var pn1 = p.Name;
          var pn2 = paramName;
          Utl.RegexReplace(ref pn1, @"^\bp_", String.Empty, true);
          Utl.RegexReplace(ref pn2, @"^\bp_", String.Empty, true);
          return String.Equals(pn1, pn2, StringComparison.CurrentCultureIgnoreCase);
        });
      }
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="prm"></param>
    /// <returns></returns>
    public static Object FindParamValue(Params prms, OracleParameter prm) {
      Object rslt = null;
      if ((prms != null) && (prm != null)) {
        var param = FindParam(prms, prm.ParameterName);
        if (param != null) {
          rslt = param.Value;
        }
      }
      return rslt;
    }

    /// <summary>
    /// ���� �������� OracleParameter � ��������� prms �� ����� paramName
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static OracleParameter FindOraParam(OracleParameterCollection prms, String paramName) {
      if ((prms != null) && !String.IsNullOrEmpty(paramName)) {
        return prms.Cast<OracleParameter>().FirstOrDefault(p => {
          var pn1 = p.ParameterName;
          var pn2 = paramName;
          Utl.RegexReplace(ref pn1, @"^\bp_", String.Empty, true);
          Utl.RegexReplace(ref pn2, @"^\bp_", String.Empty, true);
          return String.Equals(pn1, pn2, StringComparison.CurrentCultureIgnoreCase);
        });

      }
      return null;
    }

    /// <summary>
    /// �������� ParamDirection � ParameterDirection
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static ParameterDirection DecodeParamDirection(ParamDirection dir) {
      switch (dir) {
        case ParamDirection.Input: return ParameterDirection.Input;
        case ParamDirection.InputOutput: return ParameterDirection.InputOutput;
        case ParamDirection.Output: return ParameterDirection.Output;
        case ParamDirection.Return: return ParameterDirection.ReturnValue;
        default: return ParameterDirection.Input;
      }
    }

    /// <summary>
    /// �������� ParameterDirection � ParamDirection
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static ParamDirection EncodeParamDirection(ParameterDirection dir) {
      switch (dir) {
        case ParameterDirection.Input: return ParamDirection.Input;
        case ParameterDirection.InputOutput: return ParamDirection.InputOutput;
        case ParameterDirection.Output: return ParamDirection.Output;
        case ParameterDirection.ReturnValue: return ParamDirection.Return;
        default: return ParamDirection.Input;
      }
    }

    /// <summary>
    /// ��������� ��������� ���������� - ������� ��� �������
    /// </summary>
    /// <param name="connState"></param>
    /// <returns></returns>
    public static Boolean DbConnectionStateIsOppened(ConnectionState connState) {
      return ((connState == ConnectionState.Connecting) ||
                (connState == ConnectionState.Executing) ||
                  (connState == ConnectionState.Fetching) ||
                    (connState == ConnectionState.Open));

    }

    /// <summary>
    /// ��������� ���������� - ������� ��� �������
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public static Boolean DbConnectionIsOppened(IDbConnection conn) {
      return (conn != null) && DbConnectionStateIsOppened(conn.State);

    }
  }

}