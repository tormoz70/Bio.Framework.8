namespace Bio.Helpers.DOA {
  using System;

  using System.Data;
  using System.Data.Common;
  using Oracle.DataAccess.Client;

  using System.Collections.Generic;
  using System.Collections.Specialized;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  //using Bio.Packets.Ex;
  using System.Threading;
  //using Bio.Packets;

  /// <summary>
  /// Курсор с запоминанием текущего рекорда
  /// </summary>
  public class CSQLCursor : CSQLCmd {

    private List<CField> FFields;
    private SortedList<string, CField> FPKFields;
    private StringCollection FPKFieldsInit;

    /// <summary>
    /// Коллекция определений полей в курсоре.
    /// </summary>
    public List<CField> Fields {
      get {
        return this.FFields;
      }
    }

    /// <summary>
    /// Коллекция определений полей первичного ключа в курсоре.
    /// </summary>
    public SortedList<String, CField> PKFields {
      get {
        return this.FPKFields;
      }
    }

    private void _init() {
      this.FFields = new List<CField>();
      this.FPKFields = new SortedList<string, CField>();
    }

    public CSQLCursor()
      : base() {
      this._init();
    }

    public CSQLCursor(IDbConnection pConn)
      : base(pConn) {
      this._init();
    }
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pConnStr"></param>
    public CSQLCursor(IDBSession sess)
      : base(sess) {
      this._init();
    }

    /// <summary>
    /// Создаёт поля по результату запроса к БД.
    /// </summary>
    protected virtual void InitCursorFields() {
      if (this.DataReader != null) {
        //this.creRNUMFld();
        int vCurFldId = 1;
        for (int i = 0; i < this.DataReader.FieldCount; i++) {
          String fname = this.DataReader.GetName(i);
          if (!fname.ToUpper().Equals(CField.FIELD_RNUM)) {
            if (this.FieldByName(fname) == null) {
              Type ftype = this.DataReader.GetFieldType(i);
              string pkIndex = null;
              int p;
              if ((this.FPKFieldsInit != null) && ((p = this.FPKFieldsInit.IndexOf(fname.ToUpper())) >= 0))
                pkIndex = (p + 1).ToString();
              CField newFld = new CField(this, vCurFldId, fname, ftype, fname, pkIndex);
              this.FFields.Add(newFld);
              if (pkIndex != null)
                this.PKFields.Add(pkIndex, newFld);
              vCurFldId++;
            }
          }
        }
      }
    }

    protected override void onAfterOpen() {
      this.InitCursorFields();
    }

    public virtual int FieldsCount {
      get {
        return this.FFields.Count;
      }
    }

    public virtual CField FieldByName(String FieldName) {
      for (int i = 0; i < this.FFields.Count; i++) {
        String fldname = ((CField)this.FFields[i]).FieldName;
        if (fldname.ToUpper().Equals(FieldName.ToUpper())) {
          return (CField)this.FFields[i];
        }
      }
      return null;
    }

    public CField FieldByNum(int Index) {
      if ((Index >= 0) && (Index < FFields.Count))
        return (CField)this.FFields[Index];
      return null;
    }

    public String PKValue {
      get {
        String rslt = null;
        if (this.FPKFields.Count > 0) {
          if (this.FPKFields.Count == 1) {
            CField cfld = this.FPKFields["1"];
            rslt = cfld.AsString;
          } else {
            for (int i = 0; i < this.FPKFields.Count; i++) {
              String vKey = (i + 1).ToString();
              CField cfld = this.FPKFields[vKey];
              Utl.appendStr(ref rslt, "(" + cfld.AsString + ")", "-");
            }
          }
        }
        return rslt;
      }
    }

    public void PKFieldsInit(String pkFields) {
      this.FPKFieldsInit = new StringCollection();
      this.FPKFieldsInit.AddRange(pkFields.ToUpper().Split(';'));
    }

    public static CSQLCursor creAndOpenCursor(IDBSession sess, String sql, CParams prms, Int32 timeout) {
      CSQLCursor vRslt = null;
      try {
        vRslt = new CSQLCursor(sess);
        vRslt.Init(sql, prms);
        vRslt.Open(timeout);
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw new EBioException(ex.Message, ex);
      }
      return vRslt;
    }

    public static CSQLCursor creAndOpenCursor(IDbConnection conn, String sql, CParams prms, Int32 timeout) {
      CSQLCursor vRslt = null;
      try {
        vRslt = new CSQLCursor(conn);
        vRslt.Init(sql, prms);
        vRslt.Open(timeout);
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw new EBioException(ex.Message, ex);
      }
      return vRslt;
    }

    public Object getOraValue(String pFldName) {
      CField vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null)
        return vFld.AsObject;
      else
        return null;
    }

    public String getOraValueAsString(String pFldName) {
      CField vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null)
        return vFld.AsString;
      else
        return null;
    }

    public Int64 getOraValueAsInt64(String pFldName) {
      CField vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null) {
        Decimal vVal = vFld.AsDecimal;
        return (Int64)vVal;
      } else
        return 0;
    }

    public Double getOraValueAsDouble(String pFldName) {
      CField vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null) {
        Decimal vVal = vFld.AsDecimal;
        return (Double)vVal;
      } else
        return 0;
    }

    public DateTime getOraValueAsDateTime(String pFldName) {
      CField vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null)
        return vFld.AsDateTime;
      else
        return DateTime.MinValue;
    }

  }
}