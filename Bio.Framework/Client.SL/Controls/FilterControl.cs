using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using Bio.Helpers.Common;
using PropertyMetadata = System.Windows.PropertyMetadata;

namespace Bio.Framework.Client.SL {
  public class FilterControl : Control {

    public FilterControl() {
      this.DefaultStyleKey = typeof(FilterControl);
    }

    public static DependencyProperty FilterFieldsProperty = DependencyProperty.Register("FilterFields", typeof(String), typeof(FilterControl), new PropertyMetadata(String.Empty));
    public String FilterFields {
      get { return (String)this.GetValue(FilterFieldsProperty); }
      set { this.SetValue(FilterFieldsProperty, value); }
    }

    public static DependencyProperty FilterCaptionsProperty = DependencyProperty.Register("FilterCaptions", typeof(String), typeof(FilterControl), new PropertyMetadata(String.Empty));
    public String FilterCaptions {
      get { return (String)this.GetValue(FilterCaptionsProperty); }
      set { this.SetValue(FilterCaptionsProperty, value); }
    }

    public static DependencyProperty FilterValsProperty = DependencyProperty.Register("FilterVals", typeof(String), typeof(FilterControl), new PropertyMetadata(String.Empty));
    public String FilterVals {
      get { 
        return this._bldVals();
      }
      set { 
        var v_vals = Utl.SplitString(value, CC_PATH_DELIMETER);
        if ((this._vals != null) && (v_vals != null)) {
          for (int i = 0; i < this._vals.Length; i++) {
            if (i < v_vals.Length)
              this._vals[i] = v_vals[i];
            else
              this._vals[i] = null;
          }
        }
        if (this._tbx != null) {
          this._tbx.Text = this._bldMask();
          this._prevTxt = this._tbx.Text;
        }
      }
    }

    public Boolean FilterIsDefined {
      get {
        foreach (var v in this._vals)
          if (!String.IsNullOrEmpty(v))
            return true;
        return false;
      }
    }

    private const String CS_HINT_FMT = "В фильтре можно задавать текстовые значения разделенные символом '/'.\n"+
                  "При этом фильтроваться будут соответствующие колонки таблицы в порядке их следования.\n{0}";
    private const String CC_PATH_DELIMETER = "/";

    private static String _fmtMskItm(String itm) {
      return "{" + itm + "}";
    }

    public static void _appendStr(ref String line, String str, String delimiter) {
      if (line == null)
        line = (str ?? String.Empty);
      else
        line += delimiter + (str ?? String.Empty);
    }
    
    private String[] _flds;
    private String[] _capts;
    private String[] _vals;
    private String _bldVals() {
      String lst = null;
      foreach (var t in this._vals)
        _appendStr(ref lst, t, CC_PATH_DELIMETER);
      return lst;
    }
    private String _bldMask() {
      String lst = null;
      for (var i = 0; i < this._capts.Length; i++ ){
        var v_itm = String.IsNullOrEmpty(this._vals[i]) ? _fmtMskItm(this._capts[i]) : this._vals[i];
        _appendStr(ref lst, v_itm, CC_PATH_DELIMETER);
      }
      return lst;
    }
    private String _bldMaskWithVals() {
      String lst = null;
      for (var i = 0; i < this._vals.Length; i++)
        _appendStr(ref lst, String.IsNullOrEmpty(this._vals[i]) ? _fmtMskItm(this._capts[i]) : this._vals[i], CC_PATH_DELIMETER);
      return lst;
    }

    private String _bldHint(String txt, int pos) {
      var v_indx = _detectIndex(txt, pos);
      if(v_indx == -1){
        String v_lst = null;
        foreach (var s in this._capts)
          _appendStr(ref v_lst, _fmtMskItm(s), CC_PATH_DELIMETER);
        return String.Format(CS_HINT_FMT, v_lst);
      }
      return String.Format("{0}", this._capts[v_indx]);
    }
    /// <summary>
    /// Определяет индекс поля по позиции курсора
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private static int _detectIndex(String txt, int pos) {
      if (String.IsNullOrEmpty(txt))
        return -1;
      var v_rslt = -1;
      if ((pos >= 0) && (pos <= txt.Length)) {
        v_rslt = 0;
        for (var i = 0; i < txt.Length; i++) {
          if (i >= pos) break;
          if (txt[i] == CC_PATH_DELIMETER[0])
            v_rslt++;
        }
      }
      return v_rslt;
    }
    private static int _posBack(String txt, Char fnd, int pos) {
      if ((pos >= 0) && (pos <= txt.Length)) {
        for (int i = Math.Min(txt.Length-1, pos); i > 0; i--) 
          if((txt[i] == fnd) && (i < pos))
            return i;
        return 0;
      }
      return -1;
    }

    private static int _posFrwrd(String txt, Char fnd, int pos, int cnt) {
      if ((pos >= 0) && (pos <= txt.Length)) {
        if (pos == txt.Length)
          return txt.Length;
        var v_cnt = 0;
        for (var i = Math.Min(txt.Length - 1, pos); i < txt.Length; i++)
          if (txt[i] == fnd) {
            v_cnt++;
            if (v_cnt >= cnt)
              return i;
          }
        return txt.Length;
      }
      return -1;
    }

    private static int _posFrwrd(String txt, Char fnd, int pos) { 
      return _posFrwrd(txt, fnd, pos, 0);
    }


    private TextBox _tbx;
    private TextBlock _hnt;
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      this._tbx = this.GetTemplateChild("tbxText") as TextBox;
      if (this._tbx != null) {
        this._flds = Utl.SplitString(this.FilterFields, CC_PATH_DELIMETER);
        this._capts = Utl.SplitString(this.FilterCaptions, CC_PATH_DELIMETER);
        this._vals = new String[this._flds.Length];
        this._tbx.Text = this._bldMask();
        this._prevTxt = this._tbx.Text;
        this._tbx.SelectionChanged += tbx_SelectionChanged;
        this._tbx.KeyDown += tbx_KeyDown;
        this._tbx.TextChanged += _tbx_TextChanged;
        this._tbx.GotFocus += _tbx_GotFocus;
        //this._tbx.TextInputStart += new TextCompositionEventHandler(tbx_TextInputStart);
      }
      this._hnt = this.GetTemplateChild("fltrHint") as TextBlock;
      if (this._hnt != null)
        this._hnt.Text = this._bldHint(null, 0);
    }

    void _tbx_GotFocus(object sender, RoutedEventArgs e) {
      this._prevSeldIndx = -1;
      this._prevPos = this._tbx.SelectionStart;
    }

    /// <summary>
    /// Определяет значение фильтра по текущей позиции курсора
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="pos"></param>
    /// <param name="val"></param>
    /// <param name="selStrt"></param>
    /// <param name="selEnd"></param>
    private void _valAtPos(String txt, int pos, out String val, out int selStrt, out int selEnd) {
      val = null; selStrt = -1; selEnd = -1;
      if (!String.IsNullOrEmpty(txt)) {
        _detectSelBounds(txt, pos, ref selStrt, ref selEnd);
        if (selEnd >= selStrt) {
          val = txt.Substring(selStrt, selEnd - selStrt);
        }
      }
    }

    /// <summary>
    /// Устанавливает селекшион в тексте по индексу поля
    /// </summary>
    private void _selByIndx(int indx) {
      var v_txt = this._tbx.Text;
      var v_pos = _posFrwrd(v_txt, CC_PATH_DELIMETER[0], 0, indx + 1);
      if (v_pos >= 0) 
        this._tbx.SelectionStart = v_pos;
      else
        this._tbx.SelectionStart = v_txt.Length;
      this._prevSeldIndx = -1;
      this.tbx_SelectionChanged(null, null);
    }

    private Boolean _checkStructOfTxt(String txt) {
      var actVals = Utl.SplitString(txt, CC_PATH_DELIMETER);
      return actVals.Length == this._vals.Length;
    }

    private Boolean _skipTbxTextChanged;
    private String _prevTxt;
    private int _prevPos;
    void _tbx_TextChanged(object sender, TextChangedEventArgs e) {
      try {
        if (this._skipTbxTextChanged) {
          this._skipTbxTextChanged = false;
          return;
        }
        if (this._tbx != null) {
          var v_txt = this._tbx.Text;
          var v_pos = this._tbx.SelectionStart;
          if (!this._checkStructOfTxt(v_txt)) {
            v_txt = this._bldMaskWithVals();
            this._skipTbxTextChanged = true;
            this._tbx.Text = v_txt;
            var v_prev_indx = _detectIndex(this._prevTxt, this._prevPos);
            this._selByIndx(v_prev_indx);
            return;
          }

          String val; int selStrt; int selEnd;
          this._valAtPos(v_txt, v_pos, out val, out selStrt, out selEnd);
          var v_indx = _detectIndex(this._tbx.Text, v_pos);
          this._vals[v_indx] = String.Equals(val, _fmtMskItm(this._capts[v_indx])) ? null : val;
          if (String.IsNullOrEmpty(val)) {
            this._skipTbxTextChanged = true;
            //if (v_selEnd)
            this._tbx.Text = v_txt.Substring(0, selStrt) + _fmtMskItm(this._capts[v_indx]) + v_txt.Substring(selEnd);
            //this._tbx.SelectionStart = v_selStrt;
            this._selByIndx(v_indx);
          }
        }
      } finally {
        this._prevTxt = this._tbx.Text;
      }
    }

    private static void _detectSelBounds(String txt, int pos, ref int selStrt, ref int selEnd) {
      selStrt = _posBack(txt, CC_PATH_DELIMETER[0], pos);
      if (selStrt > 0) selStrt++;
      selEnd = _posFrwrd(txt, CC_PATH_DELIMETER[0], pos);
    }

    private void _selNextField(int indx) {
      var v_indx = indx; // this._detectIndex(this._tbx.Text, this._tbx.SelectionStart);
      if (v_indx < this._capts.Length - 1)
        this._selByIndx(v_indx + 1);
    }
    private void _selPrevField(int indx) {
      var v_indx = indx; // this._detectIndex(this._tbx.Text, this._tbx.SelectionStart);
      if (v_indx > 0)
        this._selByIndx(v_indx - 1);
    }

    private int _prevSeldIndx = -1;
    void tbx_SelectionChanged(object sender, RoutedEventArgs e) {
      try {
        //throw new NotImplementedException();
        Thread.Sleep(1);
        if (this._tbx != null) {
          var v_txt = this._tbx.Text;
          var v_pos = this._tbx.SelectionStart;
          var v_indx = _detectIndex(this._tbx.Text, v_pos);
          String val; int selStrt; int selEnd;
          this._valAtPos(v_txt, v_pos, out val, out selStrt, out selEnd);
          if (this._prevSeldIndx != v_indx) {
            this._prevSeldIndx = v_indx;
            //var v_selStrt = -1; var v_selEnd = -1;
            //this._detectSelBounds(v_txt, v_pos, ref v_selStrt, ref v_selEnd);
            this._tbx.SelectionStart = selStrt;
            if (selEnd > selStrt)
              this._tbx.SelectionLength = selEnd - selStrt;
          } else {
            if (String.Equals(val, _fmtMskItm(this._capts[v_indx])) && (v_pos == 1) && (this._prevPos == 0)) {
              this._selByIndx(0);
              return;
            }
            if (String.Equals(val, _fmtMskItm(this._capts[v_indx])) && (v_pos == v_txt.Length-1) && (this._prevPos == v_txt.Length)) {
              this._selByIndx(this._vals.Length - 1);
              return;
            }
            if (String.Equals(val, _fmtMskItm(this._capts[v_indx])) && (v_pos > this._prevPos)) {
              this._selNextField(v_indx);
              return;
            }
            if (String.Equals(val, _fmtMskItm(this._capts[v_indx])) && (v_pos < this._prevPos)) {
              this._selPrevField(v_indx);
              return;
            }
          }
        }
      } finally {
        this._prevPos = this._tbx.SelectionStart;
      }
    }

    void tbx_TextInputStart(object sender, TextCompositionEventArgs e) {
      //throw new NotImplementedException();
    }

    void tbx_KeyDown(object sender, KeyEventArgs e) {
      //var v_keys = new Key[] { Key.A, Key.B };
      //if (v_keys.FirstOrDefault(k == e.Key) == null) {
      //e.Handled = true;
      //}
      //if (e.Key == Key.Right) {
      //  e.Handled = true;
      //  var v_indx = this._detectIndex(this._tbx.Text, this._tbx.SelectionStart);
      //  if ((this._tbx.SelectionLength > 0) && (v_indx < this._capts.Length - 1))
      //    this._selByIndx(v_indx + 1);
      //}
      //if (e.Key == Key.Left) {
      //  e.Handled = true;
      //  var v_indx = this._detectIndex(this._tbx.Text, this._tbx.SelectionStart);
      //  if ((this._tbx.SelectionLength > 0) && (v_indx > 0))
      //    this._selByIndx(v_indx - 1);
      //}
    }

   
    //[Bindable(BindableSupport.Yes)]
    //public Object Value {
    //  get {
    //    return this._selection.Value;
    //  }
    //  set {
    //    this._setSelection(value);
    //  }
    //}

    //public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Object), typeof(FilterControl), new PropertyMetadata(default(Object), ValuePropertyChangedCallback));
    //public Object Value {
    //  get {
    //    return this.GetValue(ValueProperty);
    //  }
    //  set {
    //    this.SetValue(ValueProperty, value);
    //  }
    //}

    private Boolean _valuePropertyChangedEnabled = true;
    private void _disablePropertyChangedEvent() { this._valuePropertyChangedEnabled = false; }
    private void _enablePropertyChangedEvent() { this._valuePropertyChangedEnabled = true; }
    internal void _doOnValuePropertyChanged(DependencyPropertyChangedEventArgs e) {
      if (this._valuePropertyChangedEnabled) {
        //this._setSelection(e.NewValue);
      }
    }

    private static void _valuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((FilterControl)d)._doOnValuePropertyChanged(e);
    }


  }

}
