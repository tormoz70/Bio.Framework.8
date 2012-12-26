namespace Bio.Helpers.XLFRpt2.DataFactory.MSSQL {

  using System;
  using System.Data;
  using System.Threading;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using System.IO;
  using System.Collections;
  using System.Collections.Generic;
  using Bio.Helpers.XLFRpt2.Engine;
  using System.Data.Common;
  using System.Data.SqlClient;

  /// <summary>
  /// 
  /// </summary>
  public class CDataFactory:CXLRDataFactory {

    private DbDataReader _dataReader = null;
    private Boolean _connIsOpened = false;
    //private IDbConnection _conn = null;
    private SqlCommand _cmd = null;

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
      SqlDbType ot;
      if (pType == typeof(System.String)){
        ot = pSize < (32 * 1024) ? SqlDbType.NVarChar : SqlDbType.NText;
      } else if ((pType == typeof(System.Decimal)) || (pType == typeof(System.Double)) ||
         (pType == typeof(System.Int16)) || (pType == typeof(System.Int32)) || (pType == typeof(System.Int64)))
        ot = SqlDbType.Decimal;
      else if (pType == typeof(System.DateTime))
        ot = SqlDbType.DateTime;
      //else if (pType == typeof(System.Char[]))
      //  ot = OracleDbType.Clob;
      else if (pType == typeof(System.Byte[]))
        ot = SqlDbType.Binary;
      else
        throw new NotSupportedException("Невозможно преобразовать тип \"" + pType.Name + "\".");
      return ot;
    }

    private void _applayParams(SqlCommand cmd, CParams prms) {
      cmd.Parameters.Clear();
      foreach (CParam p in prms) {
        //cmd.Parameters.Add(new SqlParameter(p.Name, p.Value));
        Type vType = p.Value == null ? typeof(String) : p.Value.GetType();
        if (p.ParamType != null)
          vType = p.ParamType;
        long vSize = 0;
        if(vType == typeof(String))
          vSize = p.ParamSize;
        //if()
        cmd.Parameters.Add(p.Name, detectSqlTypeByType(vType, vSize)).Value = p.Value;
      }
    }

    protected override IDictionary<String, CFieldType> open(IDbConnection conn, CXLReportDSConfig dsCfg, Int32 timeout) {
      IDictionary<String, CFieldType> rslt = null;
      //this._conn = this._getConn(this._cfg.dbSession);
      this._connIsOpened = true;
      this._cmd = new SqlCommand(dsCfg.sql);
      this._cmd.CommandType = dsCfg.commandType;
      this._cmd.Connection = conn as SqlConnection;
      this._cmd.CommandTimeout = timeout;
      //String vSQL = this.FSQL;
      //this.FCmd.Init(vSQL, this.FReportDef.InParams);
      try {
        if (dsCfg.owner.debug)
          Utl.SaveStringToFile(dsCfg.owner.logPath + dsCfg.owner.extAttrs.shortCode + ".prdDS." + dsCfg.rangeName + ".sql", dsCfg.sql, null);
        this._applayParams(this._cmd, dsCfg.owner.inPrms);
        this._dataReader = this._cmd.ExecuteReader();
        rslt = new Dictionary<String, CFieldType>();
        for (int i = 0; i < this._dataReader.FieldCount; i++) {
          rslt.Add(this._dataReader.GetName(i).ToUpper(),
                   ftypeHelper.ConvertTypeToFType(this._dataReader.GetFieldType(i)));
        }
        //}
        return rslt;
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw EBioException.CreateIfNotEBio(ex);
      }
    }

    protected override IList next() {
      IList rslt = new List<Object>();
      try {
        if (this._dataReader.Read()) {
          Object[] vVals = new Object[this._dataReader.FieldCount];
          this._dataReader.GetValues(vVals);
          foreach (Object v in vVals)
            rslt.Add(v);
          return rslt;
        } else {
          //this._cmd.Cancel();
          //this._conn.Close();
          return null;
        }
      } catch (Exception ex) {
        throw new EBioException("Ошибка при считывании данных из БД. Сообщение: " + ex.ToString(), ex);
      }

    }

    public override Object GetScalarValue(IDbConnection conn, String cmd, CParams prms, Int32 timeout) {
      if (!String.IsNullOrEmpty(cmd)) {
        var v_cmd = new SqlCommand(cmd);
        v_cmd.Connection = conn as SqlConnection;
        v_cmd.CommandTimeout = timeout;
        this._applayParams(v_cmd, prms);
        return v_cmd.ExecuteScalar();
      } else
        return null;
    }

    public override IDbCommand PrepareCmd(IDbConnection conn, String cmd, CParams prms, Int32 timeout) {
      //String connStr = dbSession.ConnectionString;
      if (!String.IsNullOrEmpty(cmd)) {
        SqlCommand v_cmd = new SqlCommand(cmd);
        v_cmd.Connection = conn as SqlConnection;
        this._applayParams(v_cmd, prms);
        return v_cmd;
      } else
        return null;
    }

    public override void ExecCmd(IDbCommand cmd) {
      cmd.ExecuteNonQuery();
    }

  }
}
