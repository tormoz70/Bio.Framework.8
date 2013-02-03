namespace Bio.Helpers.XLFRpt2.DataFactory {

  using System;
  using System.Data;
  using System.Threading;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;
  using System.IO;
  using System.Collections;
  using System.Collections.Generic;
  using Bio.Helpers.XLFRpt2.Engine;

  /// <summary>
  /// 
  /// </summary>
  public class CDataFactory:CXLRDataFactory {
    private SQLCursor FCmd = null;
    private CDBSession _dbSess = null;

    public override IDbConnection openDbConnection(CXLReportConfig cfg) {
      if (cfg.dbSession != null)
        return cfg.dbSession.GetConnection();
      else { 
        if((this._dbSess == null) && !String.IsNullOrEmpty(cfg.connStr))
          this._dbSess = new CDBSession(cfg.connStr);
        if (this._dbSess != null)
          return this._dbSess.GetConnection();
        else
          return null;
      }
    }

    protected override IDictionary<String, CFieldType> open(IDbConnection conn, CXLReportDSConfig dsCfg, Int32 timeout) {


      IDictionary<String, CFieldType> rslt = null;

      this.FCmd = new SQLCursor(conn);
      String vSQL = dsCfg.sql;
      this.FCmd.Init(vSQL, dsCfg.owner.inPrms);
      try {
        if (dsCfg.owner.debug)
          Utl.SaveStringToFile(dsCfg.owner.logPath + dsCfg.owner.extAttrs.shortCode + ".prdDS." + dsCfg.rangeName + ".sql", vSQL, null);
        this.FCmd.Open(timeout);
        //if (this.FDataReader.Next()) {
          rslt = new Dictionary<String, CFieldType>();
          for (int i = 0; i < this.FCmd.FieldsCount; i++) {
            rslt.Add(this.FCmd.Fields[i].FieldName,
                     this.FCmd.Fields[i].DataType);
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
        if (this.FCmd.Next()) {
          //Object[] vVals = new Object[this.FCmd.DataReader.FieldCount];
          //this.FCmd.DataReader.GetValues(vVals);
          foreach (var fld in FCmd.Fields)
            rslt.Add(fld.AsObject);
          return rslt;
        } else {
          this.FCmd.Close();
          return null;
        }
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw new EBioException("Ошибка при считывании данных из БД. Сообщение: " + ex.ToString(), ex);
      }

    }

    public override Object GetScalarValue(IDbConnection conn, String cmd, CParams prms, Int32 timeout) {
      return SQLCmd.ExecuteScalarSQL(conn, cmd, prms, timeout);
    }

    private String _preparedSQL = null;
    private CParams _preparedParams = null;
    public override IDbCommand PrepareCmd(IDbConnection conn, String cmd, CParams prms, Int32 timeout) {
      this._preparedSQL = cmd;
      this._preparedParams = prms;
      return SQLCmd.PrepareCommand(conn, cmd, prms, timeout);
    }

    public override void ExecCmd(IDbCommand cmd) {
      var trn = cmd.Connection.BeginTransaction();
      try {
        SQLCmd.ExecuteScript(cmd, this._preparedSQL, this._preparedParams);
        trn.Commit();
      } catch (ThreadAbortException) {
        trn.Rollback();
        throw;
      } catch(EBioSQLBreaked){
        trn.Rollback();
        throw;
      } catch (Exception ex) {
        trn.Rollback();
        throw EBioException.CreateIfNotEBio(ex);
      }
    }


  }
}
