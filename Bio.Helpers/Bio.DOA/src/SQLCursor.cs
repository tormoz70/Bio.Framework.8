namespace Bio.Helpers.DOA {
  using System;

  using System.Data;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using Common.Types;
  using Common;
  using System.Threading;

  /// <summary>
  /// Курсор с запоминанием текущего рекорда
  /// </summary>
  public class SQLCursor : SQLCmd {
    private StringCollection _pkFieldsInit;

    /// <summary>
    /// Коллекция определений полей в курсоре.
    /// </summary>
    public List<Field> Fields { get; private set; }

    /// <summary>
    /// Коллекция определений полей первичного ключа в курсоре.
    /// </summary>
    public SortedList<string, Field> PKFields { get; private set; }

    private void _init() {
      this.Fields = new List<Field>();
      this.PKFields = new SortedList<String, Field>();
    }

    /// <summary>
    /// 
    /// </summary>
    public SQLCursor()
      : base() {
      this._init();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    public SQLCursor(IDbConnection connection)
      : base(connection) {
      this._init();
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    public SQLCursor(IDBSession session)
      : base(session) {
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
          if (!fname.ToUpper().Equals(Field.FIELD_RNUM)) {
            if (this.FieldByName(fname) == null) {
              Type ftype = this.DataReader.GetFieldType(i);
              string pkIndex = null;
              int p;
              if ((this._pkFieldsInit != null) && ((p = this._pkFieldsInit.IndexOf(fname.ToUpper())) >= 0))
                pkIndex = (p + 1).ToString();
              Field newFld = new Field(this, vCurFldId, fname, ftype, fname, pkIndex);
              this.Fields.Add(newFld);
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
        return this.Fields.Count;
      }
    }

    public virtual Field FieldByName(String FieldName) {
      for (int i = 0; i < this.Fields.Count; i++) {
        String fldname = ((Field)this.Fields[i]).FieldName;
        if (fldname.ToUpper().Equals(FieldName.ToUpper())) {
          return (Field)this.Fields[i];
        }
      }
      return null;
    }

    public Field FieldByNum(int Index) {
      if ((Index >= 0) && (Index < Fields.Count))
        return (Field)this.Fields[Index];
      return null;
    }

    public String PKValue {
      get {
        String rslt = null;
        if (this.PKFields.Count > 0) {
          if (this.PKFields.Count == 1) {
            Field cfld = this.PKFields["1"];
            rslt = cfld.AsString;
          } else {
            for (int i = 0; i < this.PKFields.Count; i++) {
              String vKey = (i + 1).ToString();
              Field cfld = this.PKFields[vKey];
              Utl.AppendStr(ref rslt, "(" + cfld.AsString + ")", "-");
            }
          }
        }
        return rslt;
      }
    }

    public void PKFieldsInit(String pkFields) {
      this._pkFieldsInit = new StringCollection();
      this._pkFieldsInit.AddRange(pkFields.ToUpper().Split(';'));
    }

    public static SQLCursor creAndOpenCursor(IDBSession sess, String sql, CParams prms, Int32 timeout) {
      SQLCursor vRslt = null;
      try {
        vRslt = new SQLCursor(sess);
        vRslt.Init(sql, prms);
        vRslt.Open(timeout);
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw new EBioException(ex.Message, ex);
      }
      return vRslt;
    }

    public static SQLCursor creAndOpenCursor(IDbConnection conn, String sql, CParams prms, Int32 timeout) {
      SQLCursor vRslt = null;
      try {
        vRslt = new SQLCursor(conn);
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
      Field vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null)
        return vFld.AsObject;
      else
        return null;
    }

    public String getOraValueAsString(String pFldName) {
      Field vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null)
        return vFld.AsString;
      else
        return null;
    }

    public Int64 getOraValueAsInt64(String pFldName) {
      Field vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null) {
        Decimal vVal = vFld.AsDecimal;
        return (Int64)vVal;
      } else
        return 0;
    }

    public Double getOraValueAsDouble(String pFldName) {
      Field vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null) {
        Decimal vVal = vFld.AsDecimal;
        return (Double)vVal;
      } else
        return 0;
    }

    public DateTime getOraValueAsDateTime(String pFldName) {
      Field vFld = this.FieldByName(pFldName.ToUpper());
      if (vFld != null)
        return vFld.AsDateTime;
      else
        return DateTime.MinValue;
    }

  }
}