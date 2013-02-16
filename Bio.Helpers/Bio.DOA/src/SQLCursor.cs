using System.Globalization;

namespace Bio.Helpers.DOA {
  #region

  using System;

  using System.Data;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using Common.Types;
  using Common;
  using System.Threading;

  #endregion

  /// <summary>
  /// Курсор с запоминанием текущего рекорда
  /// </summary>
  public class SQLCursor : SQLCmd {
    private StringCollection _pkFieldsInit;

    /// <summary>
    /// Конструктор
    /// </summary>
    public SQLCursor() {
      this._init();
    }

    /// <summary>
    /// Конструктор
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
    /// Коллекция определений полей в курсоре.
    /// </summary>
    public List<Field> Fields { get; private set; }

    /// <summary>
    /// Коллекция определений полей первичного ключа в курсоре.
    /// </summary>
    public SortedList<string, Field> PKFields { get; private set; }

    /// <summary>
    /// Количество полей
    /// </summary>
    public virtual int FieldsCount {
      get {
        return this.Fields.Count;
      }
    }

    /// <summary>
    /// Значение первичного ключа
    /// </summary>
    public String PKValue {
      get {
        String rslt = null;
        if (this.PKFields.Count > 0) {
          if (this.PKFields.Count == 1) {
            var cfld = this.PKFields["1"];
            rslt = cfld.AsString;
          } else {
            for (var i = 0; i < this.PKFields.Count; i++) {
              var v_key = (i + 1).ToString(CultureInfo.InvariantCulture);
              var cfld = this.PKFields[v_key];
              Utl.AppendStr(ref rslt, "(" + cfld.AsString + ")", "-");
            }
          }
        }
        return rslt;
      }
    }

    private void _init() {
      this.Fields = new List<Field>();
      this.PKFields = new SortedList<String, Field>();
    }

    /// <summary>
    /// Создаёт поля по результату запроса к БД.
    /// </summary>
    protected virtual void InitCursorFields() {
      if (this.DataReader != null) {
        var v_curFldId = 1;
        for (var i = 0; i < this.DataReader.FieldCount; i++) {
          var fname = this.DataReader.GetName(i);
          if (!fname.ToUpper().Equals(Field.FIELD_RNUM)) {
            if (this.FieldByName(fname) == null) {
              var ftype = this.DataReader.GetFieldType(i);
              String v_pkIndex = null;
              int p;
              if ((this._pkFieldsInit != null) && ((p = this._pkFieldsInit.IndexOf(fname.ToUpper())) >= 0))
                v_pkIndex = (p + 1).ToString(CultureInfo.InvariantCulture);
              var v_newFld = new Field(this, v_curFldId, fname, ftype, fname, v_pkIndex);
              this.Fields.Add(v_newFld);
              if (v_pkIndex != null)
                this.PKFields.Add(v_pkIndex, v_newFld);
              v_curFldId++;
            }
          }
        }
      }
    }

    protected override void doOnAfterOpen() {
      this.InitCursorFields();
    }

    /// <summary>
    /// Возвращает поле по имени
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public virtual Field FieldByName(String fieldName) {
      foreach (var v in this.Fields) {
        var fldname = v.FieldName;
        if (fldname.ToUpper().Equals(fieldName.ToUpper())) {
          return v;
        }
      }
      return null;
    }

    /// <summary>
    /// Возвращает поле по индексу
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Field FieldByIndex(Int32 index) {
      if ((index >= 0) && (index < Fields.Count))
        return this.Fields[index];
      return null;
    }

    /// <summary>
    /// Создает коллекцию с именами полей первичного ключа
    /// </summary>
    /// <param name="pkFields"></param>
    public void PKFieldsInit(String pkFields) {
      this._pkFieldsInit = new StringCollection();
      this._pkFieldsInit.AddRange(pkFields.ToUpper().Split(';'));
    }

    /// <summary>
    /// Создает и открывает курсор
    /// </summary>
    /// <param name="sess"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    /// <exception cref="EBioException"></exception>
    public static SQLCursor CreateAndOpenCursor(IDBSession sess, String sql, Params prms, Int32 timeout) {
      SQLCursor v_rslt;
      try {
        v_rslt = new SQLCursor(sess);
        v_rslt.Init(sql, prms);
        v_rslt.Open(timeout);
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw new EBioException(ex.Message, ex);
      }
      return v_rslt;
    }

    /// <summary>
    /// Создает и открывает курсор
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="sql"></param>
    /// <param name="prms"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    /// <exception cref="EBioException"></exception>
    public static SQLCursor CreateAndOpenCursor(IDbConnection conn, String sql, Params prms, Int32 timeout) {
      SQLCursor v_rslt;
      try {
        v_rslt = new SQLCursor(conn);
        v_rslt.Init(sql, prms);
        v_rslt.Open(timeout);
      } catch (ThreadAbortException) {
        throw;
      } catch (Exception ex) {
        throw new EBioException(ex.Message, ex);
      }
      return v_rslt;
    }

    /// <summary>
    /// Возвращает значение поля
    /// </summary>
    /// <param name="fldName"></param>
    /// <returns></returns>
    public Object GetOraValue(String fldName) {
      var v_field = this.FieldByName(fldName.ToUpper());
      return v_field != null ? v_field.AsObject : null;
    }

    /// <summary>
    /// Возвращает значение поля как строку
    /// </summary>
    /// <param name="fldName"></param>
    /// <returns></returns>
    public String GetOraValueAsString(String fldName) {
      var v_field = this.FieldByName(fldName.ToUpper());
      return v_field != null ? v_field.AsString : null;
    }

    /// <summary>
    /// Возвращает значение поля как Int64
    /// </summary>
    /// <param name="fldName"></param>
    /// <returns></returns>
    public Int64 GetOraValueAsInt64(String fldName) {
      var v_field = this.FieldByName(fldName.ToUpper());
      if (v_field != null) {
        var v_val = v_field.AsDecimal;
        return (Int64)v_val;
      }
      return 0;
    }

    /// <summary>
    /// Возвращает значение поля как Double
    /// </summary>
    /// <param name="fldName"></param>
    /// <returns></returns>
    public Double GetOraValueAsDouble(String fldName) {
      var v_field = this.FieldByName(fldName.ToUpper());
      if (v_field != null) {
        var v_val = v_field.AsDecimal;
        return (Double)v_val;
      }
      return 0;
    }

    /// <summary>
    /// Возвращает значение поля как DateTime
    /// </summary>
    /// <param name="fldName"></param>
    /// <returns></returns>
    public DateTime GetOraValueAsDateTime(String fldName) {
      var v_field = this.FieldByName(fldName.ToUpper());
      return v_field != null ? v_field.AsDateTime : DateTime.MinValue;
    }
  }
}