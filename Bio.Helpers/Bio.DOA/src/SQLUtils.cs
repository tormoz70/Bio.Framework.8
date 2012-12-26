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
  using Bio.Helpers.Common;
  using System.Data.Common;
  using Bio.Helpers.Common.Types;

  public enum CSQLDataType {
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

    public static String PrepareSQLForOlacleExecute(String sql) {
      String vRslt = null;
      if (sql != null) {
        Regex vr = new Regex("[-]{2}.*[^$]", RegexOptions.IgnoreCase);
        vRslt = vr.Replace(sql, "");
        vRslt = vRslt.Replace("\n", " ");
        vRslt = vRslt.Replace("\r", " ");
        vRslt = vRslt.Replace("\t", " ");
      }
      return vRslt;
    }

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
        String vSQL = String.Format("update {0} set {1} = :{2} where {3} = :{4}", tableName, fieldName, fieldName, keyField, keyField);
        var vOraSess = (OracleConnection)conn;
        OracleCommand vScriptCmd = vOraSess.CreateCommand();
        vScriptCmd.CommandText = vSQL;

        OracleParameter vParFile = vScriptCmd.Parameters.Add(fieldName, OracleDbType.Blob);
        vParFile.Direction = ParameterDirection.Input;
        byte[] vBuffer = null;
        //ReadBinFileInBuffer(ref vBuffer, pFileName);
        vParFile.Value = vBuffer;

        OracleParameter vParKeyValue = vScriptCmd.Parameters.Add(keyField, (keyValue != null) ? SQLUtils.DetectOraTypeByType(keyValue.GetType()) : OracleDbType.Varchar2);
        vParKeyValue.Value = keyValue;
        vParKeyValue.Direction = ParameterDirection.Input;

        try {
          int r = vScriptCmd.ExecuteNonQuery();
          if((deleteFile) && (r == 1))
            File.Delete(fileName);
        } catch(ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          StringWriter errm = new StringWriter();
          errm.WriteLine("Ошибка при выполнении SQL. Сообщение: " + ex.Message);
          errm.WriteLine("SQL: " + vScriptCmd.CommandText);
          throw new EBioException(errm.ToString(), ex);
        } finally {
        }
        vScriptCmd = null;
      }
    }

    public static void SetFile2BLOBField(IDBSession sess, String tableName, String fieldName, String keyField, Object keyValue, String fileName, Boolean deleteFile) {
      var conn = sess.GetConnection();
      try {
        SetFile2BLOBField(conn, tableName, fieldName, keyField, keyValue, fileName, deleteFile);
      } finally {
        conn.Close();
      }
    }

    public static void ExecSQLCommandNonQuery(IDbConnection conn, ref String sql, CParams prms) {
      if(!String.IsNullOrEmpty(sql)) {
        var vSQL = PrepareSQLForOlacleExecute(sql);
        var vOraSess = (OracleConnection)conn;
        var vScriptCmd = vOraSess.CreateCommand();
        vScriptCmd.CommandText = vSQL;

        OracleParameter vParReslt = null;
        if(prms != null) {
          for(int i = 0; i < prms.Count; i++) {
            vParReslt = vScriptCmd.Parameters.Add(prms[i].Name, (prms[i].InnerObject != null) ? SQLUtils.DetectOraTypeByType(prms[i].InnerObject.GetType()) : OracleDbType.Varchar2);
            vParReslt.Value = prms[i].InnerObject;
            vParReslt.Direction = ParameterDirection.Input;
          }
        }

        try {
          int r = vScriptCmd.ExecuteNonQuery();
        } catch(ThreadAbortException) {
          throw;
        } catch(Exception ex) {
          var errm = new StringWriter();
          errm.WriteLine("Ошибка при выполнении SQL. Сообщение: " + ex.Message);
          errm.WriteLine("SQL: " + vScriptCmd.CommandText);
          throw new EBioException(errm.ToString(), ex);
        }
        vScriptCmd = null;
      }
    }

    public static void ExecSQLCommandNonQuery(IDBSession sess, ref String sql, CParams prms) {
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
    /// <param name="pFldName"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByFldName(String pFldName) {
      OracleDbType vParType = OracleDbType.Varchar2;
      if(Utl.regexMatch(pFldName, "^[VFPS]A_", true))
        vParType = OracleDbType.Varchar2;
      else if(Utl.regexMatch(pFldName, "^[VFPS]N_", true))
        vParType = OracleDbType.Decimal;
      else if(Utl.regexMatch(pFldName, "^[VFPS]D_", true))
        vParType = OracleDbType.Date;
      return vParType;
    }

    /// <summary>
    /// Определяет тип поля по имени Oracle типа. Например: VARCHAR2, NUMBER, DATE 
    /// </summary>
    /// <param name="pTypeName"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByOraTypeName(String pTypeName) {
      OracleDbType vParType = OracleDbType.Varchar2;
      if(String.Equals(pTypeName, "VARCHAR2", StringComparison.OrdinalIgnoreCase))
        vParType = OracleDbType.Varchar2;
      else if(String.Equals(pTypeName, "CLOB", StringComparison.OrdinalIgnoreCase))
        vParType = OracleDbType.Clob;
      else if(String.Equals(pTypeName, "NUMBER", StringComparison.OrdinalIgnoreCase))
        vParType = OracleDbType.Decimal;
      else if(String.Equals(pTypeName, "DATE", StringComparison.OrdinalIgnoreCase))
        vParType = OracleDbType.Date;
      return vParType;
    }

    public const Int32 ClobDBSize = 32 * 1024;
    /// <summary>
    /// Определяет тип поля по типу данных
    /// </summary>
    /// <param name="type"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByType(Type type, long size) {
      if(type == null)
        throw new ArgumentNullException("pType");
      OracleDbType ot;
      if (type == typeof(System.String))
        ot = size < (ClobDBSize) ? OracleDbType.Varchar2 : OracleDbType.Clob;
      else if (type == typeof(OracleClob))
        ot = OracleDbType.Clob;
      else if ((type == typeof(System.Decimal)) || (type == typeof(System.Double)) ||
         (type == typeof(System.Int16)) || (type == typeof(System.Int32)) || (type == typeof(System.Int64)))
        ot = OracleDbType.Decimal;
      else if ((type == typeof(System.DateTime)) || (type == typeof(System.DateTime?)) ||
                (type == typeof(System.DateTimeOffset)) || (type == typeof(System.DateTimeOffset?)))
        ot = OracleDbType.Date;
      //else if (pType == typeof(System.Char[]))
      //  ot = OracleDbType.Clob;
      else if (type == typeof(System.Byte[]))
        ot = OracleDbType.Blob;
      else if (type == typeof(System.Boolean))
        ot = OracleDbType.Decimal;
      else
        throw new NotSupportedException("Невозможно преобразовать тип \"" + type.Name + "\".");
      return ot;
    }

    /// <summary>
    /// Определяет тип поля по типу данных
    /// </summary>
    /// <param name="pType"></param>
    /// <returns></returns>
    public static OracleDbType DetectOraTypeByType(Type pType) {
      return DetectOraTypeByType(pType, 0);
    }

    /// <summary>
    /// Определяет тип объект по типу данных
    /// </summary>
    /// <param name="pType"></param>
    /// <returns></returns>
    public static Type DetectTypeByOraType(OracleDbType pType) {
      Type vResult;
      switch (pType) {
        case OracleDbType.Varchar2:
        case OracleDbType.Char:
        case OracleDbType.NChar:
        case OracleDbType.NVarchar2:
        case OracleDbType.Clob:
          vResult = typeof(String);
          break;
        case OracleDbType.Byte:
        case OracleDbType.Double:
        case OracleDbType.Decimal:
        case OracleDbType.Int16:
        case OracleDbType.Int32:
        case OracleDbType.Int64:
          vResult = typeof(Decimal);
          break;
        case OracleDbType.Date:
        case OracleDbType.TimeStamp:
          vResult = typeof(DateTime);
          break;
        case OracleDbType.Blob:
          vResult = typeof(Byte[]);
          break;
        default:
          throw new NotSupportedException("Невозможно преобразовать тип \"" + pType + "\".");
      }
      return vResult;
    }

    /// <summary>
    /// Конвертирует значение типа OracleDbType d Object
    /// </summary>
    /// <param name="pType"></param>
    /// <returns></returns>
    public static Object OraDbValueAsObject(Object value) {
      Object v_result = null;
      if (value != null) {
        Type v_type = value.GetType();
        if (v_type == typeof(OracleString)) {
          if (!((OracleString)value).IsNull) {
            String vResultStr = ((OracleString)value).Value;
            v_result = vResultStr;
          }
        } else if (v_type == typeof(OracleDecimal)) {
          if (!((OracleDecimal)value).IsNull) {
            if (((OracleDecimal)value).IsInt) {
              Int64 vResultDec = ((OracleDecimal)value).ToInt64();
              v_result = vResultDec;
            } else {
              Double vResultDec = ((OracleDecimal)value).ToDouble();
              v_result = new Decimal(vResultDec);
            }
          }
        } else if (v_type == typeof(OracleDate)) {
          if (!((OracleDate)value).IsNull) {
            DateTime vResultDt = ((OracleDate)value).Value;
            v_result = vResultDt; //DateTime.SpecifyKind(vResultDt, DateTimeKind.Utc);
          }
        } else if (v_type == typeof(OracleTimeStamp)) {
          if (!((OracleTimeStamp)value).IsNull) {
            DateTime vResultDt = ((OracleTimeStamp)value).Value;
            v_result = vResultDt;
          }
        } else if (v_type == typeof(OracleClob)) {
          if (!((OracleClob)value).IsNull) {
            String vResultStr = Utl.EncodeANSII2UTF(((OracleClob)value).Value);
            v_result = vResultStr;
          }
        } else if (v_type == typeof(OracleBlob)) {
          Byte[] vResultAByte = ((OracleBlob)value).Value;
          v_result = vResultAByte;
        } else
          v_result = value;
      }
      return v_result;
    }

    /// <summary>
    /// Определяет направление параметра по имени. Например: IN, OUT, IN/OUT
    /// </summary>
    /// <param name="pTypeName"></param>
    /// <returns></returns>
    public static ParameterDirection DetectParamDirByName(String pDirName) {
      ParameterDirection vParDir = ParameterDirection.Input;
      if(String.Equals(pDirName, "IN", StringComparison.OrdinalIgnoreCase))
        vParDir = ParameterDirection.Input;
      else if(String.Equals(pDirName, "OUT", StringComparison.OrdinalIgnoreCase))
        vParDir = ParameterDirection.Output;
      else if(String.Equals(pDirName, "IN/OUT", StringComparison.OrdinalIgnoreCase))
        vParDir = ParameterDirection.InputOutput;
      return vParDir;
    }

    /// <summary>
    /// Преобразует строку в тип OracleDbType
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="oraType">Тип</param>
    /// <returns></returns>
    internal static Object StrAsOraValue(String value, OracleDbType oraType) {
      Object vRslt = null;
      try {
        CultureInfo culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true) {
          NumberFormat = { NumberDecimalSeparator = "." }
        };
        switch (oraType) {
          case OracleDbType.Varchar2:
          case OracleDbType.Clob:
            vRslt = value;
            break;
          case OracleDbType.Decimal:
            vRslt = Double.Parse(value.Replace(",", ".").Replace(" ", ""), culture);
            break;
          case OracleDbType.Date:
            vRslt = DateTimeParser.Instance.ParsDateTime(value);
            break;
          case OracleDbType.Blob:
            vRslt = Convert.FromBase64String(value);
            break;
        }
      } catch (Exception ex) {
        throw new EBioException(
          "При приведении значения \"" + value + "\" к типу " +
          enumHelper.NameOfValue(oraType) + " ", ex);
      }
      return vRslt;
    }

    /// <summary>
    /// Преобразует строку в тип OracleDbType
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="type">Тип</param>
    /// <returns></returns>
    public static Object StrAsOraValue(String value, CSQLDataType type) {
      //if (String.IsNullOrEmpty(type))
      //  throw new EBioException("Параметер \"pOraType\" должен быть задан!");
      return StrAsOraValue(value, (OracleDbType)type);
      /*switch (type) {
        case "VARCHAR2": return StrAsOraValue(value, (OracleDbType)type);
        case "CLOB": return StrAsOraValue(value, OracleDbType.Clob);
        case "CHAR": return StrAsOraValue(value, OracleDbType.Char);
        case "NUMBER": return StrAsOraValue(value, OracleDbType.Decimal);
        case "DATE": return StrAsOraValue(value, OracleDbType.Date);
        case "BLOB": return StrAsOraValue(value, OracleDbType.Blob);
        default: throw new EBioException("Неизвестный тип \"" + type + "\"");

      }*/
    }

    public static String ObjectAsString(Object pObject) {
      String rslt = null;
      if (pObject != null) {
        Type tp = pObject.GetType();
        if (tp.Equals(typeof(OracleClob)))
          rslt = Utl.EncodeANSII2UTF(((OracleClob)pObject).Value);
        else if (tp.Equals(typeof(OracleBlob)))
          rslt = Convert.ToBase64String(((OracleBlob)pObject).Value);
        else
          rslt = Utl.ObjectAsString(pObject);
      }
      return rslt;
    }

    public static CParam findParam(CParams prms, String paramName) {
      if ((prms != null) && !String.IsNullOrEmpty(paramName)) {
        return prms.FirstOrDefault((p) => {
          var pn1 = p.Name;
          var pn2 = paramName;
          Utl.regexReplace(ref pn1, @"^\bp_", String.Empty, true);
          Utl.regexReplace(ref pn2, @"^\bp_", String.Empty, true);
          return String.Equals(pn1, pn2, StringComparison.CurrentCultureIgnoreCase);
        });
      }
      return null;
    }

    public static Object findParamValue(CParams prms, OracleParameter prm) {
      Object vRslt = null;
      if ((prms != null) && (prm != null)) {
        CParam vPrm = findParam(prms, prm.ParameterName);
        if (vPrm != null) {
          vRslt = vPrm.Value;
        }
      }
      return vRslt;
    }

    public static OracleParameter findOraParam(OracleParameterCollection prms, String paramName) {
      if ((prms != null) && !String.IsNullOrEmpty(paramName)) {
        //foreach(OracleParameter vPrm in prms)
        //  if(vPrm.ParameterName.ToUpper().Equals(prmName.ToUpper()))
        //    return vPrm;
        return prms.Cast<OracleParameter>().FirstOrDefault((p) => {
          var pn1 = p.ParameterName;
          var pn2 = paramName;
          Utl.regexReplace(ref pn1, @"^\bp_", String.Empty, true);
          Utl.regexReplace(ref pn2, @"^\bp_", String.Empty, true);
          return String.Equals(pn1, pn2, StringComparison.CurrentCultureIgnoreCase);
        });

      }
      return null;
    }

    public static ParameterDirection decodeParamDirection(ParamDirection dir) {
      switch (dir) {
        case ParamDirection.Input: return ParameterDirection.Input;
        case ParamDirection.InputOutput: return ParameterDirection.InputOutput;
        case ParamDirection.Output: return ParameterDirection.Output;
        case ParamDirection.Return: return ParameterDirection.ReturnValue;
        default: return ParameterDirection.Input;
      }
    }

    public static ParamDirection encodeParamDirection(ParameterDirection dir) {
      switch (dir) {
        case ParameterDirection.Input: return ParamDirection.Input;
        case ParameterDirection.InputOutput: return ParamDirection.InputOutput;
        case ParameterDirection.Output: return ParamDirection.Output;
        case ParameterDirection.ReturnValue: return ParamDirection.Return;
        default: return ParamDirection.Input;
      }
    }

    public static Boolean dbConnectionStateIsOppened(ConnectionState connState) {
      return ((connState == ConnectionState.Connecting) ||
                (connState == ConnectionState.Executing) ||
                  (connState == ConnectionState.Fetching) ||
                    (connState == ConnectionState.Open));

    }

    public static Boolean dbConnectionIsOppened(IDbConnection conn) {
      return (conn != null) && dbConnectionStateIsOppened(conn.State);

    }
  }

}