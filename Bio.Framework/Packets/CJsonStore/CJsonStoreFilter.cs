using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
#if !SILVERLIGHT
using System.Windows.Forms;
#endif
using Bio.Helpers.Common;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public enum CJSFilterItemSplitterType { 
    And,
    Or
  }

  public enum CJSFilterComparisionOperatorType {
    [Description("Равно")]
    Eq = 0,
    [Description("Меньше")]
    Lt = 1,
    [Description("Меньше или равно")]
    Le = 2,
    [Description("Больше")]
    Gt = 3,
    [Description("Больше или равно")]
    Ge = 4,
    [Description("Начинается с")]
    Bgn = 5,
    [Description("Оканчивается на")]
    End = 6,
    [Description("Содержит")]
    In = 7,
    [Description("Пусто")]
    IsNull = 8
  }

  public abstract class CJSFilterItem: ICloneable {
    public abstract void buildSQLCondition(ref String sql, CParams prms);
    protected abstract CJSFilterItem clone();
#if SILVERLIGHT
    public virtual Boolean check(CRTObject row) { throw new NotImplementedException(); }
#endif
    #region ICloneable Members

    public object Clone() {
      return this.clone();
    }

    #endregion
  }

  /*public class CJSFilterItemSplitter : CJSFilterItem {
    public CJSFilterItemSplitterType joinCondition { get; set; }
    public override void buildSQLCondition(ref String sql, CParams prms) {
      sql = String.Format(" {0} ", enumHelper.NameOfValue(this.joinCondition, false).ToUpper());
    }
    protected override CJSFilterItem clone() {
      return new CJSFilterItemSplitter { 
        joinCondition = this.joinCondition
      };
    }
  }*/

  public class CJSFilterItemCondition : CJSFilterItem {
    /// <summary>
    /// Инвертировать условие
    /// </summary>
    public Boolean not { get; set; }
    /// <summary>
    /// Имя поля
    /// </summary>
    public String fieldName { get; set; }
    /// <summary>
    /// Тип поля
    /// </summary>
    public FieldType? fieldType { get; set; }
    /// <summary>
    /// Сравниваемое значение
    /// </summary>
    public Object fieldValue { get; set; }
    /// <summary>
    /// Оператор сравнения
    /// </summary>
    public CJSFilterComparisionOperatorType cmpOperator { get; set; }

    private String detectSQLFormat(Boolean hasNot, CJSFilterComparisionOperatorType operation, FieldType fieldType) {
      //String valStr = (val != null) ? val.ToString() : null;
      String rslt = null;
      switch (fieldType) {
        case FieldType.String:
        case FieldType.Clob: {
            switch (operation) {
              case CJSFilterComparisionOperatorType.Eq: rslt = "UPPER({0}) = UPPER(:{1})"; break;
              case CJSFilterComparisionOperatorType.Gt: rslt = "UPPER({0}) > UPPER(:{1})"; break;
              case CJSFilterComparisionOperatorType.Ge: rslt = "UPPER({0}) >= UPPER(:{1})"; break;
              case CJSFilterComparisionOperatorType.Lt: rslt = "UPPER({0}) < UPPER(:{1})"; break;
              case CJSFilterComparisionOperatorType.Le: rslt = "UPPER({0}) <= UPPER(:{1})"; break;
              case CJSFilterComparisionOperatorType.Bgn: rslt = "UPPER({0}) LIKE UPPER(:{1}||'%')"; break;
              case CJSFilterComparisionOperatorType.End: rslt = "UPPER({0}) LIKE UPPER('%'||:{1})"; break;
              case CJSFilterComparisionOperatorType.In: rslt = "UPPER({0}) LIKE UPPER('%'||:{1}||'%')"; break;
              case CJSFilterComparisionOperatorType.IsNull: rslt = "{0} IS NULL{1}"; break;
            }
          } break;
        case FieldType.Boolean: {
            //Boolean valBool = Utl.parsBoolean(valStr);
            //if (valBool)
            //  rslt = "(to_char({0}) = '1' or upper(to_char({0})) = 'TRUE' or upper(to_char({0})) = 'Y' or upper(to_char({0})) = 'T'){1}";
            //else
            //  rslt = "(to_char({0}) = '0' or upper(to_char({0})) = 'FALSE' or upper(to_char({0})) = 'N' or upper(to_char({0})) = 'F'){1}";
            rslt = "{0} = :{1}";
          } break;
        case FieldType.Float:
        case FieldType.Int:
        case FieldType.Blob:
        case FieldType.Object: {
            switch (operation) {
              case CJSFilterComparisionOperatorType.Eq: rslt = "{0} = :{1}"; break;
              case CJSFilterComparisionOperatorType.Gt: rslt = "{0} > :{1}"; break;
              case CJSFilterComparisionOperatorType.Ge: rslt = "{0} >= :{1}"; break;
              case CJSFilterComparisionOperatorType.Lt: rslt = "{0} < :{1}"; break;
              case CJSFilterComparisionOperatorType.Le: rslt = "{0} <= :{1}"; break;
            }
          } break;
        case FieldType.Date: {
            //String vOperStr = null;
            switch (operation) {
              case CJSFilterComparisionOperatorType.Eq: rslt = "{0} = :{1}"; break;
              case CJSFilterComparisionOperatorType.Gt: rslt = "{0} > :{1}"; break;
              case CJSFilterComparisionOperatorType.Ge: rslt = "{0} >= :{1}"; break;
              case CJSFilterComparisionOperatorType.Lt: rslt = "{0} < :{1}"; break;
              case CJSFilterComparisionOperatorType.Le: rslt = "{0} <= :{1}"; break;
            }
            //rslt = "{0} " + vOperStr + " TO_DATE('{1}', '" + OraDTP.Instance.DetectDateTimeFmt(valStr) + "')";
          } break;
      }
      return (hasNot) ? String.Format("NOT({0})", rslt) : rslt;
    }

    private FieldType _detectFTypeGranted() {
      FieldType v_ftype = FieldType.String;
      if (this.fieldType != null)
        v_ftype = (FieldType)this.fieldType;
      else {
        if (this.fieldValue != null)
          v_ftype = ftypeHelper.ConvertTypeToFType(this.fieldValue.GetType());
      }
      return v_ftype;
    }

    public override void buildSQLCondition(ref String sql, CParams prms) {
      if (prms == null)
        throw new ArgumentNullException("prms");
      String v_val_param_name = this.fieldName + "$afilter";
      FieldType v_ftype = this._detectFTypeGranted();
      sql = String.Format(this.detectSQLFormat(this.not, this.cmpOperator, v_ftype), this.fieldName, v_val_param_name);
      var v_ptype = ftypeHelper.ConvertFTypeToType(v_ftype);
      Object v_pval = null;
      if (v_ftype == FieldType.Boolean) {
        v_ptype = typeof(Int64);
        var v_bool = (Boolean)this.fieldValue;
        v_pval = (v_bool) ? 1 : 0;
      }else
        v_pval = this.fieldValue;
      prms.Add(new CParam { 
        Name = v_val_param_name,
        ParamType = v_ptype,
        Value = v_pval
      });
    }


    private Boolean _check(Object value) {
      //String valStr = (val != null) ? val.ToString() : null;
      Boolean rslt = false;
      FieldType v_ftype = this._detectFTypeGranted();
      switch (v_ftype) {
        case FieldType.String:
        case FieldType.Clob: {
            String v_val = Utl.Convert2Type<String>(value); if(!String.IsNullOrEmpty(v_val)) v_val = v_val.ToUpper();
            String v_fval = this.fieldValue as String; if (!String.IsNullOrEmpty(v_fval)) v_fval = v_fval.ToUpper();
            switch (this.cmpOperator) {
              case CJSFilterComparisionOperatorType.Eq:
                rslt = (String.IsNullOrEmpty(v_val) && String.IsNullOrEmpty(v_fval)) ||
                  (!String.IsNullOrEmpty(v_val) && v_val.Equals(v_fval)); 
                break;
              case CJSFilterComparisionOperatorType.Gt: rslt = String.Compare(v_val, v_fval) > 0; break;
              case CJSFilterComparisionOperatorType.Ge: rslt = String.Compare(v_val, v_fval) >= 0; break;
              case CJSFilterComparisionOperatorType.Lt: rslt = String.Compare(v_val, v_fval) < 0; break;
              case CJSFilterComparisionOperatorType.Le: rslt = String.Compare(v_val, v_fval) <= 0; break;
              case CJSFilterComparisionOperatorType.Bgn: rslt = v_val.StartsWith(v_fval); break;
              case CJSFilterComparisionOperatorType.End: rslt = v_val.EndsWith(v_fval); break;
              case CJSFilterComparisionOperatorType.In: rslt = v_val.Contains(v_fval); break;
              case CJSFilterComparisionOperatorType.IsNull: rslt = String.IsNullOrEmpty(v_val); break;
            }
          } break;
        case FieldType.Boolean: {
            //Boolean valBool = Utl.parsBoolean(valStr);
            //if (valBool)
            //  rslt = "(to_char({0}) = '1' or upper(to_char({0})) = 'TRUE' or upper(to_char({0})) = 'Y' or upper(to_char({0})) = 'T'){1}";
            //else
            //  rslt = "(to_char({0}) = '0' or upper(to_char({0})) = 'FALSE' or upper(to_char({0})) = 'N' or upper(to_char({0})) = 'F'){1}";
            Boolean v_val = Utl.Convert2Type<Boolean>(value);
            rslt = (Boolean)this.fieldValue == v_val;
          } break;
        case FieldType.Float:
        case FieldType.Int:
        case FieldType.Blob:
        case FieldType.Object: {
          Decimal v_val = Utl.Convert2Type<Decimal>(value);
          Decimal v_fval = Utl.Convert2Type<Decimal>(this.fieldValue);
            switch (this.cmpOperator) {
              case CJSFilterComparisionOperatorType.Eq: rslt = v_val == v_fval; break;
              case CJSFilterComparisionOperatorType.Gt: rslt = v_val > v_fval; break;
              case CJSFilterComparisionOperatorType.Ge: rslt = v_val >= v_fval; break;
              case CJSFilterComparisionOperatorType.Lt: rslt = v_val < v_fval; break;
              case CJSFilterComparisionOperatorType.Le: rslt = v_val <= v_fval; break;
            }
          } break;
        case FieldType.Date: {
          DateTime v_val = Utl.Convert2Type<DateTime>(value);
          DateTime v_fval = Utl.Convert2Type<DateTime>(this.fieldValue);
            switch (this.cmpOperator) {
              case CJSFilterComparisionOperatorType.Eq: rslt = v_val == v_fval; break;
              case CJSFilterComparisionOperatorType.Gt: rslt = v_val > v_fval; break;
              case CJSFilterComparisionOperatorType.Ge: rslt = v_val >= v_fval; break;
              case CJSFilterComparisionOperatorType.Lt: rslt = v_val < v_fval; break;
              case CJSFilterComparisionOperatorType.Le: rslt = v_val <= v_fval; break;
            }
            //rslt = "{0} " + vOperStr + " TO_DATE('{1}', '" + OraDTP.Instance.DetectDateTimeFmt(valStr) + "')";
          } break;
      }
      return (this.not) ? !rslt : rslt;
    }

#if SILVERLIGHT
    public override Boolean check(CRTObject row) {
      var val = row.GetValue<Object>(this.fieldName);
      return this._check(val);
    }
#endif

    protected override CJSFilterItem clone() {
      return new CJSFilterItemCondition { 
        not = this.not,
        fieldName = this.fieldName,
        fieldType = this.fieldType,
        fieldValue = this.fieldValue,
        cmpOperator = this.cmpOperator
      };
    }

  }

  public class CJsonStoreFilter : ICloneable {
    public CJsonStoreFilter() { this.Items = new List<CJSFilterItem>(); }
    public List<CJSFilterItem> Items { get; set; }
    public CJSFilterItemSplitterType joinCondition { get; set; }
    public Int64 fromPosition { get; set; }

#if SILVERLIGHT
    public static CJsonStoreFilter CreFromSelection(VSingleSelection selection) {
      if (selection != null) {
        var locate = new CJsonStoreFilter() { fromPosition = 0 };
        locate.Items.Add(new CJSFilterItemCondition {
          fieldName = selection.ValueField,
          cmpOperator = CJSFilterComparisionOperatorType.Eq,
          fieldValue = selection.Value
        });
        return locate;
      } else
        return null;
    }

    public Boolean check(CRTObject row) {
      Boolean rslt = (this.joinCondition == CJSFilterItemSplitterType.And) ? true : false;
      foreach (var c in this.Items) {
        Boolean v_locRslt = false;
        if (c is CJSFilterItemCondition)
          v_locRslt = c.check(row);
        if (this.joinCondition == CJSFilterItemSplitterType.And)
          rslt = rslt && v_locRslt;
        else
          rslt = rslt || v_locRslt;
      }
      return rslt;
    }
#endif

    public void buildSQLConditions(ref String sql, CParams prms) {
      sql = null;
      int i = 0;
      while(i < this.Items.Count) {
        String vDelimeter = null;
        if (i > 1) {
          this.Items[i - 1].buildSQLCondition(ref vDelimeter, prms);
        }
        String v_lsql = null;
        this.Items[i].buildSQLCondition(ref v_lsql, prms);
        Utl.AppendStr(ref sql, v_lsql, vDelimeter);
        i++; i++;
      }
      if (!String.IsNullOrEmpty(sql))
        sql = String.Format("({0})", sql);
      //return sql;
    }

    #region ICloneable Members

    public Object Clone() {
      CJsonStoreFilter vResult = new CJsonStoreFilter() { fromPosition = this.fromPosition };
      lock (this) {
        foreach (ICloneable prm in this.Items)
          vResult.Items.Add((CJSFilterItem)prm.Clone());
      }
      return vResult;
    }

    #endregion

    #region Serializing

    /// <summary>
    /// Представляет параметры в виде Json-строки
    /// </summary>
    /// <returns></returns>
    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }
    /// <summary>
    /// Восстанвливает объект из Json-строки
    /// </summary>
    /// <param name="pJsonString"></param>
    /// <returns></returns>
    public static CParams Decode(String pJsonString) {
      return jsonUtl.decode<CParams>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    #endregion
  }
}
