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
  /// Утилиты 
  /// </summary>
  public class SQLUtils {

    /// <summary>
    /// Подготавливает SQL предложение к выполнению в Oracle
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
    /// Загружает файл в BLOB поле таблицы
    /// </summary>
    /// <param name="conn">Соединения с БД</param>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="keyField">Имя поля-первичного ключа</param>
    /// <param name="keyValue">Значение для первичного ключа</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="deleteFile">Удалить файл после загрузки</param>
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
        var v_sql = String.Format("update {0} set {1} = :{2} where {3} = :{4}", tableName, fieldName, fieldName, keyField, keyField);
        var v_oraSess = (OracleConnection)conn;
        var v_scriptCmd = v_oraSess.CreateCommand();
        v_scriptCmd.CommandText = v_sql;

        var v_parFile = v_scriptCmd.Parameters.Add(fieldName, OracleDbType.Blob);
        v_parFile.Direction = ParameterDirection.Input;
        byte[] v_buffer = null;
        Utl.ReadBinFileInBuffer(fileName, ref v_buffer);
        v_parFile.Value = v_buffer;

        var v_parKeyValue = v_scriptCmd.Parameters.Add(keyField, (keyValue != null) ? DetectOraTypeByType(keyValue.GetType()) : OracleDbType.Varchar2);
        v_parKeyValue.Value = keyValue;
        v_parKeyValue.Direction = ParameterDirection.Input;

        try {
          var r = v_scriptCmd.ExecuteNonQuery();
          if((deleteFile) && (r == 1))
            File.Delete(fileName);
        } catch(ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          var errm = new StringWriter();
          errm.WriteLine("Ошибка при выполнении SQL. Сообщение: " + ex.Message);
          errm.WriteLine("SQL: " + v_scriptCmd.CommandText);
          throw new EBioException(errm.ToString(), ex);
        }
      }
    }

    /// <summary>
    /// Загружает файл в BLOB поле таблицы
    /// </summary>
    /// <param name="sess">Сессия БД</param>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="fieldName">Имя поля</param>
    /// <param name="keyField">Имя поля-первичного ключа</param>
    /// <param name="keyValue">Значение для первичного ключа</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="deleteFile">Удалить файл после загрузки</param>
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
    /// Выполняет SQL как скрипт
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <exception cref="EBioException"></exception>
    public static void ExecSQLCommandNonQuery(IDbConnection conn, ref String sql, Params prms) {
      if(!String.IsNullOrEmpty(sql)) {
        var v_sql = PrepareSQLForOlacleExecute(sql);
        var v_oraSess = (OracleConnection)conn;
        var v_scriptCmd = v_oraSess.CreateCommand();
        v_scriptCmd.CommandText = v_sql;

        if(prms != null) {
          foreach (var t in prms) {
            var v_parReslt = v_scriptCmd.Parameters.Add(t.Name, (t.InnerObject != null) ? DetectOraTypeByType(t.InnerObject.GetType()) : OracleDbType.Varchar2);
            v_parReslt.Value = t.InnerObject;
            v_parReslt.Direction = ParameterDirection.Input;
          }
        }

        try {
          v_scriptCmd.ExecuteNonQuery();
        } catch(ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          var errm = new StringWriter();
          errm.WriteLine("Ошибка при выполнении SQL. Сообщение: " + ex.Message);
          errm.WriteLine("SQL: " + v_scriptCmd.CommandText);
          throw new EBioException(errm.ToString(), ex);
        }
      }
    }

    /// <summary>
    /// Выполняет SQL как скрипт
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
    /// Определяет тип поля по префиксу
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
    /// Определяет тип поля по имени Oracle типа. Например: VARCHAR2, NUMBER, DATE 
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
    /// Размер поля Clob
    /// </summary>
    public const Int32 ClobDBSize = 32 * 1024;

    /// <summary>
    /// Определяет тип поля по типу данных
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
        throw new NotSupportedException("Невозможно определить тип \"" + type.Name + "\".");
      return ot;
    }

    /// <summary>
    /// Определяет тип поля по типу данных
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByType(Type type) {
      return DetectOraTypeByType(type, 0);
    }

    /// <summary>
    /// Определяет тип объект по типу данных
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
          throw new NotSupportedException("Невозможно преобразовать тип \"" + type + "\".");
      }
      return v_result;
    }

    /// <summary>
    /// Конвертирует значение типа OracleDbType в Object
    /// </summary>
    /// <returns></returns>
    public static Object OraDbValueAsObject(Object value) {
      Object v_result = null;
      if (value != null) {
        var v_type = value.GetType();
        if (v_type == typeof(OracleString)) {
          if (!((OracleString)value).IsNull) {
            var v_resultStr = ((OracleString)value).Value;
            v_result = v_resultStr;
          }
        } else if (v_type == typeof(OracleDecimal)) {
          if (!((OracleDecimal)value).IsNull) {
            if (((OracleDecimal)value).IsInt) {
              var v_resultDec = ((OracleDecimal)value).ToInt64();
              v_result = v_resultDec;
            } else {
              var v_vResultDec = ((OracleDecimal)value).ToDouble();
              v_result = new Decimal(v_vResultDec);
            }
          }
        } else if (v_type == typeof(OracleDate)) {
          if (!((OracleDate)value).IsNull) {
            var v_resultDt = ((OracleDate)value).Value;
            v_result = v_resultDt; //DateTime.SpecifyKind(vResultDt, DateTimeKind.Utc);
          }
        } else if (v_type == typeof(OracleTimeStamp)) {
          if (!((OracleTimeStamp)value).IsNull) {
            var v_resultDt = ((OracleTimeStamp)value).Value;
            v_result = v_resultDt;
          }
        } else if (v_type == typeof(OracleClob)) {
          if (!((OracleClob)value).IsNull) {
            var v_resultStr = Utl.EncodeANSII2UTF(((OracleClob)value).Value);
            v_result = v_resultStr;
          }
        } else if (v_type == typeof(OracleBlob)) {
          var v_resultAByte = ((OracleBlob)value).Value;
          v_result = v_resultAByte;
        } else
          v_result = value;
      }
      return v_result;
    }

    /// <summary>
    /// Определяет направление параметра по имени. Например: IN, OUT, IN/OUT
    /// </summary>
    /// <returns></returns>
    public static ParameterDirection DetectParamDirByName(String directionName) {
      var v_parDir = ParameterDirection.Input;
      if(String.Equals(directionName, "IN", StringComparison.OrdinalIgnoreCase))
        v_parDir = ParameterDirection.Input;
      else if(String.Equals(directionName, "OUT", StringComparison.OrdinalIgnoreCase))
        v_parDir = ParameterDirection.Output;
      else if(String.Equals(directionName, "IN/OUT", StringComparison.OrdinalIgnoreCase))
        v_parDir = ParameterDirection.InputOutput;
      return v_parDir;
    }

    /// <summary>
    /// Преобразует строку в тип OracleDbType
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="oraType">Тип</param>
    /// <returns></returns>
    internal static Object StrAsOraValue(String value, OracleDbType oraType) {
      Object v_rslt = null;
      try {
        var culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true) {
          NumberFormat = { NumberDecimalSeparator = "." }
        };
        switch (oraType) {
          case OracleDbType.Varchar2:
          case OracleDbType.Clob:
            v_rslt = value;
            break;
          case OracleDbType.Decimal:
            v_rslt = Double.Parse(value.Replace(",", ".").Replace(" ", ""), culture);
            break;
          case OracleDbType.Date:
            v_rslt = DateTimeParser.Instance.ParsDateTime(value);
            break;
          case OracleDbType.Blob:
            v_rslt = Convert.FromBase64String(value);
            break;
        }
      } catch (Exception ex) {
        throw new EBioException(
          "При приведении значения \"" + value + "\" к типу " +
          enumHelper.NameOfValue(oraType) + " ", ex);
      }
      return v_rslt;
    }

    /// <summary>
    /// Преобразует строку в тип OracleDbType
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="type">Тип</param>
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
          rslt = Utl.EncodeANSII2UTF(((OracleClob)pObject).Value);
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
      Object v_rslt = null;
      if ((prms != null) && (prm != null)) {
        var v_prm = FindParam(prms, prm.ParameterName);
        if (v_prm != null) {
          v_rslt = v_prm.Value;
        }
      }
      return v_rslt;
    }

    /// <summary>
    /// Ищет параметр OracleParameter в коллекции prms по имени paramName
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
    /// Приводит ParamDirection к ParameterDirection
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
    /// Приводит ParameterDirection к ParamDirection
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
    /// Проверяет состояние соединения - Открыто или Закрыто
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
    /// Проверяет соединение - Открыто или Закрыто
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public static Boolean DbConnectionIsOppened(IDbConnection conn) {
      return (conn != null) && DbConnectionStateIsOppened(conn.State);

    }
  }

}