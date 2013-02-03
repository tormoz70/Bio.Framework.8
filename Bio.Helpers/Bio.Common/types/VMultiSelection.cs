using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if SILVERLIGHT
using System.Windows.Controls;
#endif

namespace Bio.Helpers.Common.Types {
  public class VMultiSelection: VSelection {
    public VMultiSelection() {
      this.Values = new List<String>();
    }
    public Boolean Inversion { get; set; }
    public List<String> Values { get; set; }
    
#if SILVERLIGHT
    public Boolean CheckSelected(CRTObject row) {
      return this.Values.Any((v) => {
        return String.Equals(v, row.GetValue<String>(this.ValueField));
      });
    }

    public override Boolean IsEmpty() {
      return (this.Values.Count == 0) && !this.Inversion;
    }
    
    public void AddSelected(CRTObject row, Boolean applyToCheckBox) {
      if (!this.CheckSelected(row)) {
        if (applyToCheckBox) {
          var v_cbx = row.ExtObject as CheckBox;
          if (v_cbx != null)
            v_cbx.IsChecked = !this.Inversion;
        }
        this.Values.Add(row.GetValue<String>(this.ValueField));
      }
    }
    public void AddSelected(CRTObject row) {
      this.AddSelected(row, true);
    }

    public void RemoveSelected(CRTObject row, Boolean applyToCheckBox) {
      if (this.CheckSelected(row)) {
        if (applyToCheckBox) {
          var v_cbx = row.ExtObject as CheckBox;
          if (v_cbx != null)
            v_cbx.IsChecked = this.Inversion;
        }
        this.Values.Remove(row.GetValue<String>(this.ValueField));
      }
    }

    public void RemoveSelected(CRTObject row) {
      this.RemoveSelected(row, true);
    }

    public void Clear() {
      this.Inversion = false;
      this.Values.Clear();
    }
#endif

    public override string ToString() {
      var v_rslt = String.Empty;
      foreach (var c in this.Values)
        Utl.AppendStr(ref v_rslt, c, ";");
      v_rslt = (this.Inversion ? "1" : "0") + "||" + v_rslt;
      return v_rslt;
    }

    /// <summary>
    /// Копирует значения всех атрибутов в destination
    /// </summary>
    /// <param name="destination"></param>
    public void ApplyTo(VMultiSelection destination) {
      if (destination != null) {
        destination.Inversion = this.Inversion;
        if(!String.IsNullOrEmpty(this.ValueField))
          destination.ValueField = this.ValueField;
        destination.Values.Clear();
        destination.Values.AddRange(this.Values);
      }
    }

    private void _pars(String selection) {
      this.Inversion = false;
      this.Values.Clear();
      if (!String.IsNullOrEmpty(selection)) {
        String[] v_parts = Utl.SplitString(selection, "||");
        if ((v_parts.Length == 1) || (v_parts.Length == 2)) {
          if ((v_parts.Length == 2) && (v_parts[0] == "1"))
            this.Inversion = true;
          String v_items_str = (v_parts.Length == 2) ? v_parts[1] : v_parts[0];
          String[] v_items = String.IsNullOrEmpty(v_items_str) ? new String[0] : Utl.SplitString(v_items_str, ';');
          foreach (var v_itm in v_items)
            this.Values.Add(v_itm);
        } else
          throw new Exception(String.Format("Параметер selection [{0}] имеет не верную структуру.", selection));
      }
    }

    public override Object Value { 
      get {
        return this.ToString();
      }
      set {
        if ((value != null) && (value.GetType() == typeof(String)))
          this._pars(value as String);
      }
    }
    public override String Display {
      get {
        return this.IsEmpty() ? csNotSeldText : csMultiSeldText;
      }
      set {
        base.Display = value;
      }
    }

    /// <summary>
    /// Создает объект типа VMultiSelection из строки
    /// </summary>
    /// <typeparam name="T">Тип значения первичного ключа</typeparam>
    /// <param name="selection">Строка в формате "id1;id2;..." или "флагИнверсии||id1;id2;...",
    ///                         где флагИнверсии = 0(значение по умолчанию) или 1, если флагИнверсии = 1, то указанные идентификаторы строк яляются исключениями из выборки</param>
    /// <returns></returns>
    public static VMultiSelection Create(String selection) {
      var v_rslt = new VMultiSelection();
      v_rslt._pars(selection);
      return v_rslt;
    }
  }
}
