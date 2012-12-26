using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Bio.Helpers.Common.Types {

  public class SortableObservableCollection<T> : ObservableCollection<T> {
    public void Sort() {
      Sort(Comparer<T>.Default);
    }
    public void Sort(IComparer<T> comparer) {
      int i, j;
      T index;
      for (i = 1; i < Count; i++) {
        index = this[i];     //If you can't read it, it should be index = this[x], where x is i :-)
        j = i;
        while ((j > 0) && (comparer.Compare(this[j - 1], index) == 1)) {
          this[j] = this[j - 1];
          j = j - 1;
        }
        this[j] = index;
      }
    }

    public void Sort(String propertyName) {
      var v_cmprr = new CDefaultComparer<T>(propertyName);
      this.Sort(v_cmprr);
    }
  }

  public class CDefaultComparer<T> : IComparer<T> {
    private String _propertyName = null;
    public CDefaultComparer(String propertyName) {
      this._propertyName = propertyName;
    }
    #region IComparer<Contact> Members
    public int Compare(T x, T y) {
      var v_x_val = Utl.GetPropertyValue(x, this._propertyName);
      var v_y_val = Utl.GetPropertyValue(y, this._propertyName);
      if((v_x_val == null) && (v_y_val == null))
        return 0;
      if((v_x_val != null) && (v_y_val == null))
        return 1;
      if((v_x_val == null) && (v_y_val != null))
        return -1;

      if (v_x_val.GetType() == typeof(String))
        return String.Compare((String)v_x_val, (String)v_y_val);
      if (Utl.typeIsNumeric(v_x_val.GetType())) {
        var v_x_num = Utl.Convert2Type<Decimal>(v_x_val);
        var v_y_num = Utl.Convert2Type<Decimal>(v_y_val);
        if (v_x_num == v_y_num)
          return 0;
        else if (v_x_num > v_y_num)
          return 1;
        else if (v_x_num < v_y_num)
          return -1;
      }
      if (v_x_val.GetType() == typeof(DateTime)) {
        var v_x_date = Utl.Convert2Type<DateTime>(v_x_val);
        var v_y_date = Utl.Convert2Type<DateTime>(v_y_val);
        if (v_x_date == v_y_date)
          return 0;
        else if (v_x_date > v_y_date)
          return 1;
        else if (v_x_date < v_y_date)
          return -1;
      }
      throw new Exception(String.Format("Не возможно сравнить значения {0} и {1}!", v_x_val, v_y_val));
    }
    #endregion
  }

}
