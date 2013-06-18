using System;
using System.Data;
using System.Threading;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;
using System.Collections;
using System.Collections.Generic;
using Bio.Helpers.XLFRpt2.Engine;
using System.Data.Common;
using System.Data.SqlClient;

namespace Bio.Helpers.XLFRpt2.DataFactory {
  /// <summary>
  /// 
  /// </summary>
  public class CDataFactory:CXLRDataFactory {

    private DbDataReader _dataReader;
    private SqlCommand _cmd;

    public override IDbConnection openDbConnection(CXLReportConfig cfg) {
      IDbConnection v_conn = null;
      if (!String.IsNullOrEmpty(cfg.connStr)) {
        v_conn = new SqlConnection(cfg.connStr);
        v_conn.Open();
      }
      return v_conn;
    }

    private static SqlDbType detectSqlTypeByType(Type pType, long pSize) {
      if (pType == null)
        throw new ArgumentNullException("pType");
      SqlDbType v_ot;
      if (pType == typeof(String)){
        v_ot = pSize < (32 * 1024) ? SqlDbType.NVarChar : SqlDbType.NText;
      } else if ((pType == typeof(Decimal)) || (pType == typeof(Double)) ||
         (pType == typeof(Int16)) || (pType == typeof(Int32)) || (pType == typeof(Int64)))
        v_ot = SqlDbType.Decimal;
      else if (pType == typeof(DateTime))
        v_ot = SqlDbType.DateTime;
      else if (pType == typeof(Byte[]))
        v_ot = SqlDbType.Binary;
      else
        throw new NotSupportedException("Невозможно преобразовать тип \"" + pType.Name + "\".");
      return v_ot;
    }

    private static void _applayParams(SqlCommand cmd, Params prms) {
      cmd.Parameters.Clear();
      foreach (var v_p in prms) {
        var v_type = v_p.Value == null ? typeof(String) : v_p.Value.GetType();
        if (v_p.ParamType != null)
          v_type = v_p.ParamType;
        long v_size = 0;
        if(v_type == typeof(String))
          v_size = v_p.ParamSize;
        cmd.Parameters.Add(v_p.Name, detectSqlTypeByType(v_type, v_size)).Value = v_p.Value;
      }
    }

    protected override IDictionary<String, FieldType> open(IDbConnection conn, CXLReportDSConfig dsCfg, Int32 timeout) {
      this._cmd = new SqlCommand(dsCfg.sql);
      this._cmd.CommandType = dsCfg.commandType;
      this._cmd.Connection = conn as SqlConnection;
      this._cmd.CommandTimeout = timeout;
      try {
        if (dsCfg.owner.debug)
          Utl.SaveStringToFile(dsCfg.owner.logPath + dsCfg.owner.extAttrs.shortCode + ".prdDS." + dsCfg.rangeName + ".sql", dsCfg.sql, null);
        _applayParams(this._cmd, dsCfg.owner.inPrms);
        this._dataReader = this._cmd.ExecuteReader();
        IDictionary<String, FieldType> v_rslt = new Dictionary<String, FieldType>();
        for (var i = 0; i < this._dataReader.FieldCount; i++) {
          v_rslt.Add(this._dataReader.GetName(i).ToUpper(),
                   ftypeHelper.ConvertTypeToFType(this._dataReader.GetFieldType(i)));
        }
        return v_rslt;
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw EBioException.CreateIfNotEBio(ex);
      }
    }

    protected override IList next() {
      IList v_rslt = new List<Object>();
      try {
        if (this._dataReader.Read()) {
          var v_vals = new Object[this._dataReader.FieldCount];
          this._dataReader.GetValues(v_vals);
          foreach (var v in v_vals)
            v_rslt.Add(v);
          return v_rslt;
        }
        return null;
      } catch (Exception ex) {
        throw new EBioException("Ошибка при считывании данных из БД. Сообщение: " + ex, ex);
      }

    }

    public override Object GetScalarValue(IDbConnection conn, String cmd, Params prms, Int32 timeout) {
      if (!String.IsNullOrEmpty(cmd)) {
        var v_cmd = new SqlCommand(cmd);
        v_cmd.Connection = conn as SqlConnection;
        v_cmd.CommandTimeout = timeout;
        _applayParams(v_cmd, prms);
        return v_cmd.ExecuteScalar();
      }
      return null;
    }

    public override IDbCommand PrepareCmd(IDbConnection conn, String cmd, Params prms, Int32 timeout) {
      //String connStr = dbSession.ConnectionString;
      if (!String.IsNullOrEmpty(cmd)) {
        var v_cmd = new SqlCommand(cmd);
        v_cmd.Connection = conn as SqlConnection;
        _applayParams(v_cmd, prms);
        return v_cmd;
      }
      return null;
    }

    public override void ExecCmd(IDbCommand cmd) {
      cmd.ExecuteNonQuery();
    }

  }
}
