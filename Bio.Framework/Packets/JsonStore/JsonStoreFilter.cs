using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !SILVERLIGHT
using System.Windows.Forms;
#endif
using Bio.Helpers.Common;
using Newtonsoft.Json;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public enum JSFilterItemSplitterType { 
    And,
    Or
  }

  public enum JSFilterComparisionOperatorType {
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

  public abstract class JSFilterItem: ICloneable {
    protected abstract JSFilterItem clone();
#if SILVERLIGHT
    public virtual Boolean check(CRTObject row) { throw new NotImplementedException(); }
#else
    public abstract void BuildSQLCondition(ref String sql, Params prms);
#endif

    #region ICloneable Members

    public object Clone() {
      return this.clone();
    }

    #endregion
  }

  public class JSFilterItemCondition : JSFilterItem {
    /// <summary>
    /// Инвертировать условие
    /// </summary>
    public Boolean Not { get; set; }
    /// <summary>
    /// Имя поля
    /// </summary>
    public String FieldName { get; set; }
    /// <summary>
    /// Тип поля
    /// </summary>
    public JSFieldType? FieldType { get; set; }
    /// <summary>
    /// Сравниваемое значение
    /// </summary>
    public Object FieldValue { get; set; }
    /// <summary>
    /// Оператор сравнения
    /// </summary>
    public JSFilterComparisionOperatorType CmpOperator { get; set; }

    private JSFieldType _detectFTypeGranted() {
      JSFieldType v_ftype = Helpers.Common.JSFieldType.String;
      if (this.FieldType != null)
        v_ftype = (JSFieldType)this.FieldType;
      else {
        if (this.FieldValue != null)
          v_ftype = ftypeHelper.ConvertTypeToFType(this.FieldValue.GetType());
      }
      return v_ftype;
    }

#if SILVERLIGHT

    private Boolean _check(Object value) {
      var rslt = false;
      var v_ftype = this._detectFTypeGranted();
      switch (v_ftype) {
        case JSFieldType.String:
        case JSFieldType.Clob: {
            var val = Utl.Convert2Type<String>(value); if (!String.IsNullOrEmpty(val)) val = val.ToUpper();
            var fval = this.FieldValue as String; if (!String.IsNullOrEmpty(fval)) fval = fval.ToUpper();
            switch (this.CmpOperator) {
              case JSFilterComparisionOperatorType.Eq:
                rslt = (String.IsNullOrEmpty(val) && String.IsNullOrEmpty(fval)) ||
                  (!String.IsNullOrEmpty(val) && val.Equals(fval));
                break;
              case JSFilterComparisionOperatorType.Gt: rslt = String.Compare(val, fval) > 0; break;
              case JSFilterComparisionOperatorType.Ge: rslt = String.Compare(val, fval) >= 0; break;
              case JSFilterComparisionOperatorType.Lt: rslt = String.Compare(val, fval) < 0; break;
              case JSFilterComparisionOperatorType.Le: rslt = String.Compare(val, fval) <= 0; break;
              case JSFilterComparisionOperatorType.Bgn: rslt = val.StartsWith(fval); break;
              case JSFilterComparisionOperatorType.End: rslt = val.EndsWith(fval); break;
              case JSFilterComparisionOperatorType.In: rslt = val.Contains(fval); break;
              case JSFilterComparisionOperatorType.IsNull: rslt = String.IsNullOrEmpty(val); break;
            }
          } break;
        case JSFieldType.Boolean: {
            var v_val = Utl.Convert2Type<Boolean>(value);
            rslt = (Boolean)this.FieldValue == v_val;
          } break;
        case JSFieldType.Float:
        case JSFieldType.Int:
        case JSFieldType.Blob:
        case JSFieldType.Object: {
            var v_val = Utl.Convert2Type<Decimal>(value);
            var v_fval = Utl.Convert2Type<Decimal>(this.FieldValue);
            switch (this.CmpOperator) {
              case JSFilterComparisionOperatorType.Eq: rslt = v_val == v_fval; break;
              case JSFilterComparisionOperatorType.Gt: rslt = v_val > v_fval; break;
              case JSFilterComparisionOperatorType.Ge: rslt = v_val >= v_fval; break;
              case JSFilterComparisionOperatorType.Lt: rslt = v_val < v_fval; break;
              case JSFilterComparisionOperatorType.Le: rslt = v_val <= v_fval; break;
            }
          } break;
        case JSFieldType.Date: {
            var v_val = Utl.Convert2Type<DateTime>(value);
            var v_fval = Utl.Convert2Type<DateTime>(this.FieldValue);
            switch (this.CmpOperator) {
              case JSFilterComparisionOperatorType.Eq: rslt = v_val == v_fval; break;
              case JSFilterComparisionOperatorType.Gt: rslt = v_val > v_fval; break;
              case JSFilterComparisionOperatorType.Ge: rslt = v_val >= v_fval; break;
              case JSFilterComparisionOperatorType.Lt: rslt = v_val < v_fval; break;
              case JSFilterComparisionOperatorType.Le: rslt = v_val <= v_fval; break;
            }
          } break;
      }
      return (this.Not) ? !rslt : rslt;
    }

    public override Boolean check(CRTObject row) {
      var val = row.GetValue<Object>(this.FieldName);
      return this._check(val);
    }
#else
    private String detectSQLFormat(Boolean hasNot, JSFilterComparisionOperatorType operation, JSFieldType jsFieldType) {
      String rslt = null;
      switch (jsFieldType) {
        case JSFieldType.String:
        case JSFieldType.Clob: {
            switch (operation) {
              case JSFilterComparisionOperatorType.Eq: rslt = "UPPER({0}) = UPPER(:{1})"; break;
              case JSFilterComparisionOperatorType.Gt: rslt = "UPPER({0}) > UPPER(:{1})"; break;
              case JSFilterComparisionOperatorType.Ge: rslt = "UPPER({0}) >= UPPER(:{1})"; break;
              case JSFilterComparisionOperatorType.Lt: rslt = "UPPER({0}) < UPPER(:{1})"; break;
              case JSFilterComparisionOperatorType.Le: rslt = "UPPER({0}) <= UPPER(:{1})"; break;
              case JSFilterComparisionOperatorType.Bgn: rslt = "UPPER({0}) LIKE UPPER(:{1}||'%')"; break;
              case JSFilterComparisionOperatorType.End: rslt = "UPPER({0}) LIKE UPPER('%'||:{1})"; break;
              case JSFilterComparisionOperatorType.In: rslt = "UPPER({0}) LIKE UPPER('%'||:{1}||'%')"; break;
              case JSFilterComparisionOperatorType.IsNull: rslt = "{0} IS NULL{1}"; break;
            }
          } break;
        case JSFieldType.Boolean: {
            rslt = "{0} = :{1}";
          } break;
        case JSFieldType.Float:
        case JSFieldType.Int:
        case JSFieldType.Blob:
        case JSFieldType.Object: {
            switch (operation) {
              case JSFilterComparisionOperatorType.Eq: rslt = "{0} = :{1}"; break;
              case JSFilterComparisionOperatorType.Gt: rslt = "{0} > :{1}"; break;
              case JSFilterComparisionOperatorType.Ge: rslt = "{0} >= :{1}"; break;
              case JSFilterComparisionOperatorType.Lt: rslt = "{0} < :{1}"; break;
              case JSFilterComparisionOperatorType.Le: rslt = "{0} <= :{1}"; break;
            }
          } break;
        case JSFieldType.Date: {
            switch (operation) {
              case JSFilterComparisionOperatorType.Eq: rslt = "{0} = :{1}"; break;
              case JSFilterComparisionOperatorType.Gt: rslt = "{0} > :{1}"; break;
              case JSFilterComparisionOperatorType.Ge: rslt = "{0} >= :{1}"; break;
              case JSFilterComparisionOperatorType.Lt: rslt = "{0} < :{1}"; break;
              case JSFilterComparisionOperatorType.Le: rslt = "{0} <= :{1}"; break;
            }
          } break;
      }
      return (hasNot) ? String.Format("NOT({0})", rslt) : rslt;
    }

    public override void BuildSQLCondition(ref String sql, Params prms) {
      if (prms == null)
        throw new ArgumentNullException("prms");
      var v_val_param_name = this.FieldName + "$afilter";
      var v_ftype = this._detectFTypeGranted();
      sql = String.Format(this.detectSQLFormat(this.Not, this.CmpOperator, v_ftype), this.FieldName, v_val_param_name);
      var v_ptype = ftypeHelper.ConvertFTypeToType(v_ftype);
      Object v_pval;
      if (v_ftype == JSFieldType.Boolean) {
        v_ptype = typeof(Int64);
        var v_bool = (Boolean)this.FieldValue;
        v_pval = (v_bool) ? 1 : 0;
      }else
        v_pval = this.FieldValue;
      prms.Add(new Param { 
        Name = v_val_param_name,
        ParamType = v_ptype,
        Value = v_pval
      });
    }
#endif

    protected override JSFilterItem clone() {
      return new JSFilterItemCondition { 
        Not = this.Not,
        FieldName = this.FieldName,
        FieldType = this.FieldType,
        FieldValue = this.FieldValue,
        CmpOperator = this.CmpOperator
      };
    }

  }

  public class JsonStoreFilter : ICloneable {
    public JsonStoreFilter() { this.Items = new List<JSFilterItem>(); }
    public List<JSFilterItem> Items { get; set; }
    public JSFilterItemSplitterType JoinCondition { get; set; }
    public Int64 FromPosition { get; set; }

#if SILVERLIGHT
    public static JsonStoreFilter CreFromSelection(VSingleSelection selection) {
      if (selection != null) {
        var locate = new JsonStoreFilter() { FromPosition = 0 };
        locate.Items.Add(new JSFilterItemCondition {
          FieldName = selection.ValueField,
          CmpOperator = JSFilterComparisionOperatorType.Eq,
          FieldValue = selection.Value
        });
        return locate;
      } else
        return null;
    }

    public Boolean Check(CRTObject row) {
      var rslt = (this.JoinCondition == JSFilterItemSplitterType.And) ? true : false;
      foreach (var c in this.Items) {
        var locRslt = false;
        if (c is JSFilterItemCondition)
          locRslt = c.check(row);
        if (this.JoinCondition == JSFilterItemSplitterType.And)
          rslt = rslt && locRslt;
        else
          rslt = rslt || locRslt;
      }
      return rslt;
    }
#else
    public void BuildSQLConditions(ref String sql, Params prms) {
      sql = null;
      var i = 0;
      while(i < this.Items.Count) {
        String vDelimeter = null;
        if (i > 1) {
          this.Items[i - 1].BuildSQLCondition(ref vDelimeter, prms);
        }
        String v_lsql = null;
        this.Items[i].BuildSQLCondition(ref v_lsql, prms);
        Utl.AppendStr(ref sql, v_lsql, vDelimeter);
        i++; i++;
      }
      if (!String.IsNullOrEmpty(sql))
        sql = String.Format("({0})", sql);
      //return sql;
    }
#endif

    #region ICloneable Members

    public Object Clone() {
      var result = new JsonStoreFilter { FromPosition = this.FromPosition };
      lock (this) {
        foreach (ICloneable prm in this.Items)
          result.Items.Add((JSFilterItem)prm.Clone());
      }
      return result;
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
    public static Params Decode(String pJsonString) {
      return jsonUtl.decode<Params>(pJsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    #endregion
  }
}
