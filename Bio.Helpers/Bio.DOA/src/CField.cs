namespace Bio.Helpers.DOA {
  using System;
  using System.Globalization;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  //using Bio.Packets.Ex;
  
  /// <summary>
	/// ѕоле курсора - как в TDataSet
	/// </summary>
  public class CField{
    public const String FIELD_RNUM = "RNUM";
		//public const String FIELD_TYPE_VARCHAR = "VARCHAR";
		//public const String FIELD_TYPE_NUMBER = "NUMBER";
		//public const String FIELD_TYPE_DATE = "DATE";
    /// <summary>
    ///  онстанта - формат даты в системе
    /// </summary>
    //public const String SYS_SQL_DATETIME_FORMAT = "YYYY-MM-DD HH24:MI:SS";
		
		private String FName;
    private CFieldType FType;
		private Int32 FID;
		private Int32 FResultSetIndex;
    private String FPkIndex;
    private String FHeader;
		private CSQLCursor FOwner;

    public CField(CSQLCursor owner, Int32 id, String name, CFieldType type, String header, String pkIndex) {
			this.FID = id;
      this.FResultSetIndex = FID + 1;
      this.FOwner = owner;
      this.FName = name.ToUpper();
      this.FType = type;
      this.FHeader = header;
      this.FPkIndex = pkIndex;
		}

    public CField(CSQLCursor owner, Int32 id, String name, Type type, String header, String pkIndex) {
      this.FID = id;
      this.FResultSetIndex = FID + 1;
      this.FOwner = owner;
      this.FName = name.ToUpper();
      this.FType = ftypeHelper.ConvertTypeToFType(type);
      this.FHeader = header;
      this.FPkIndex = pkIndex;
    }
		
		public String FieldName{
			get{
        return this.FName;
			}
		}

    public CFieldType DataType {
			get{
        return this.FType;
			}
		}

    public Type DataTypeNet {
      get {
        return ftypeHelper.ConvertFTypeToType(this.FType);
      }
    }
		
		public Int32 FieldID{
			get{
        return this.FID;
			}
		}
		
		public String FieldCaption{
			get{
        return this.FHeader;
			}
		}

    public String FieldPkIndex {
      get {
        return this.FPkIndex;
      }
    }

    private Object _internalValueConvert(Object value) {
      if (value == null)
        return null;
      Type v_type = this.DataTypeNet;
      var v_value = value;
      Type v_value_type = (v_value != null) ? v_value.GetType() : typeof(Object);
      if (!v_value_type.Equals(v_type)) {
        try {
          v_value = Utl.Convert2Type(v_value, v_type);
        } catch (Exception ex) {
          throw new Exception(String.Format("InternalValueConvert_Exception! {0}: ({1})[{2}] -> ({3}). {4}", this.FieldName, v_value_type.Name, "" + v_value, v_type.Name, ex.Message), ex);
        }
      }
      if ((v_type.Equals(typeof(DateTime)) || v_type.Equals(typeof(DateTime?))) && (this.DataType == CFieldType.DateUTC))
        v_value = DateTime.SpecifyKind((DateTime)v_value, DateTimeKind.Utc);
      return v_value;
    }

    private Object valueFromResultSet{
      get{
        if((this.FOwner != null) && (this.FOwner.DataReader != null)) {
          Object vVal = this.FOwner.RowValues[this.FName.ToUpper()];
          if(this.FName.ToUpper().Equals(FIELD_RNUM)){
            vVal = (vVal == null) ? this.FOwner.CurFetchedRowPos : vVal;
          }
          return this._internalValueConvert(vVal);
        }else
          return null;
      }
    }

		public Boolean IsDBNull{
			get{
        Object vVal = this.valueFromResultSet;
        return (vVal == null);
			}
		}

    public Object AsObject {
      get {
        return this.valueFromResultSet;
      }
    }

		public DateTime AsDateTime{
			get{
        Object v_val = this.valueFromResultSet;
        DateTime rslt = Utl.Convert2Type<DateTime>(v_val);
        /*if (v_val != null) {
          Type tp = v_val.GetType();
          if(tp.Equals(typeof(DateTime))) {
            rslt = (DateTime)v_val;
					}
					else if (tp.Equals(typeof(String))){
						IFormatProvider ifp = new DateTimeFormatInfo();
            rslt = DateTime.Parse((String)v_val, ifp);
					}else{
            throw new EBioException("«начение типа " + tp + " не может быть представлено как DateTime!!! ");
					}
				}*/
				return rslt;
			}
		}
		
		public Boolean AsBoolean{
			get{
        Object v_val = this.valueFromResultSet;
        Boolean rslt = Utl.Convert2Type<Boolean>(v_val);
        /*if (v_val != null) {
          Type tp = v_val.GetType();
          if (tp.Equals(typeof(Decimal))) {
            switch (Decimal.ToInt32((Decimal)v_val)) {
						
							case 0:  rslt = false;
								break;
						
							case 1:  rslt = true;
								break;
						}
          } else if(tp.Equals(typeof(String))) {
						if ((((String) v_val).Equals("0")) || (((String) v_val).Equals("N")) || (((String) v_val).Equals("F")))
							rslt = false;
						else if ((((String) v_val).Equals("1")) || (((String) v_val).Equals("Y")) || (((String) v_val).Equals("T")))
							rslt = true;
						else
							rslt = false;
					}else{
            throw new EBioException("«начение типа " + tp + " не может быть представлено как boolean!!! ");
					}
				}*/
				return rslt;
			}
		}
		
		public Int64 AsInteger{
			get{
        Object v_val = this.valueFromResultSet;
        Int64 rslt = Utl.Convert2Type<Int64>(v_val);
        /*if (v_val != null) {
          Type tp = v_val.GetType();
          if (tp.Equals(typeof(Decimal))) {
            rslt = Decimal.ToInt64((Decimal)v_val);
					}else if (tp.Equals(typeof(String))){
            rslt = Int64.Parse((String)v_val);
					}else{
            throw new EBioException("«начение типа " + tp + " не может быть представлено как Integer!!! ");
					}
				}*/
				return rslt;
			}
		}

    public Decimal AsDecimal {
      get {
        Object v_val = this.valueFromResultSet;
        Decimal rslt = Utl.Convert2Type<Decimal>(v_val);
        /*if (v_val != null) {
          Type tp = v_val.GetType();
          if (tp.Equals(typeof(Decimal))) {
            rslt = (Decimal)v_val;
          } else if (tp.Equals(typeof(String))) {
            rslt = Decimal.Parse((String)v_val);
          } else {
            throw new EBioException("«начение типа " + tp + " не может быть представлено как Decimal!!! ");
          }
        }*/
        return rslt;
      }
    }
		
		public String AsString{
			get{
				String rslt = SQLUtils.ObjectAsString(this.valueFromResultSet);
				return rslt;
			}
		}

	}
}