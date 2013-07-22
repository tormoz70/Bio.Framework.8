namespace Bio.Helpers.DOA {
  using System;
  using Common;

  /// <summary>
	/// Поле курсора - как в TDataSet
	/// </summary>
  public class Field{
    /// <summary>
    /// 
    /// </summary>
    public const String FIELD_RNUM = "RNUM";

    private readonly SQLCursor _owner;

    /// <summary>
    /// Коструктор
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="header"></param>
    /// <param name="pkIndex"></param>
    /// <param name="fieldEncoding">Кодировка для CLOB полей</param>
    public Field(SQLCursor owner, Int32 id, String name, JSFieldType type, String header, String pkIndex, FieldEncoding fieldEncoding = FieldEncoding.UTF8) {
			this.FieldID = id;
      this._owner = owner;
      this.FieldName = name.ToUpper();
      this.DataType = type;
      this.Encoding = fieldEncoding;
      this.FieldCaption = header;
      this.FieldPkIndex = pkIndex;
		}

    /// <summary>
    /// Коструктор
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="header"></param>
    /// <param name="pkIndex"></param>
    /// <param name="fieldEncoding">Кодировка для CLOB полей</param>
    public Field(SQLCursor owner, Int32 id, String name, Type type, String header, String pkIndex, FieldEncoding fieldEncoding = FieldEncoding.UTF8) {
      this.FieldID = id;
      this._owner = owner;
      this.FieldName = name.ToUpper();
      this.DataType = ftypeHelper.ConvertTypeToFType(type);
      this.Encoding = fieldEncoding;
      this.FieldCaption = header;
      this.FieldPkIndex = pkIndex;
    }

    /// <summary>
    /// Имя поля
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Тип поля по БД
    /// </summary>
    public JSFieldType DataType { get; private set; }

    /// <summary>
    /// Кодировка строки в CLOB
    /// </summary>
    public FieldEncoding Encoding { get; private set; }

    /// <summary>
    /// Тип поля в .Net
    /// </summary>
    public Type DataTypeNet {
      get {
        return ftypeHelper.ConvertFTypeToType(this.DataType);
      }
    }

    /// <summary>
    /// ID поля
    /// </summary>
    public int FieldID { get; private set; }

    /// <summary>
    /// Заголовок для UI
    /// </summary>
    public string FieldCaption { get; private set; }

    /// <summary>
    /// Индекс в перивичном ключе
    /// </summary>
    public string FieldPkIndex { get; private set; }

    private Object valueFromResultSet{
      get {
        if((this._owner != null) && (this._owner.DataReader != null)) {
          var v_val = this._owner.RowValues[this.FieldName.ToUpper()];
          if(this.FieldName.ToUpper().Equals(FIELD_RNUM)){
            v_val = v_val ?? this._owner.CurFetchedRowPos;
          }
          return this._internalValueConvert(v_val);
        }
        return null;
      }
    }

		/// <summary>
		/// Значение соответствует NULL в БД
		/// </summary>
		public Boolean IsDBNull{
			get{
        var v_val = this.valueFromResultSet;
        return (v_val == null);
			}
		}

    /// <summary>
    /// Возвращает как Object
    /// </summary>
    public Object AsObject {
      get {
        return this.valueFromResultSet;
      }
    }

		/// <summary>
    /// Возвращает как DateTime использует <see cref="Utl.Convert2Type">Utl.Convert2Type</see>
		/// </summary>
		public DateTime AsDateTime{
			get{
				return Utl.Convert2Type<DateTime>(this.valueFromResultSet);
			}
		}

		/// <summary>
    /// Возвращает как Boolean использует <see cref="Utl.Convert2Type">Utl.Convert2Type</see>
    /// </summary>
		public Boolean AsBoolean{
			get{
				return Utl.Convert2Type<Boolean>(this.valueFromResultSet);
			}
		}

    /// <summary>
    /// Возвращает как Int64 использует <see cref="Utl.Convert2Type">Utl.Convert2Type</see>
    /// </summary>
    public Int64 AsInteger {
			get{
        var v_val = this.valueFromResultSet;
        var rslt = Utl.Convert2Type<Int64>(v_val);
				return rslt;
			}
		}

    /// <summary>
    /// Возвращает как Decimal использует <see cref="Utl.Convert2Type">Utl.Convert2Type</see>
    /// </summary>
    public Decimal AsDecimal {
      get {
        var v_val = this.valueFromResultSet;
        var rslt = Utl.Convert2Type<Decimal>(v_val);
        return rslt;
      }
    }

    /// <summary>
    /// Возвращает как String использует <see cref="SQLUtils.ObjectAsString">SQLUtils.ObjectAsString</see>
    /// </summary>
    public String AsString {
			get{
				var rslt = SQLUtils.ObjectAsString(this.valueFromResultSet);
				return rslt;
			}
		}

    private Object _internalValueConvert(Object value) {
      if (value == null)
        return null;
      var v_type = this.DataTypeNet;
      var v_value = value;
      var v_valueType = v_value.GetType();
      if (v_valueType != v_type) {
        try {
          v_value = Utl.Convert2Type(v_value, v_type);
        } catch (Exception ex) {
          throw new Exception(String.Format("InternalValueConvert_Exception! {0}: ({1})[{2}] -> ({3}). {4}", this.FieldName, v_valueType.Name, "" + v_value, v_type.Name, ex.Message), ex);
        }
      }
      if ((v_type == typeof(DateTime) || v_type == typeof(DateTime?)) && (this.DataType == JSFieldType.DateUTC))
        v_value = DateTime.SpecifyKind((DateTime)v_value, DateTimeKind.Utc);
      return v_value;
    }
  }
}