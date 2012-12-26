using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Bio.Helpers.Controls.SL.SLPropertyGrid;
using System.Threading;
using System.Windows.Input;
using Bio.Helpers.Common.Types;
using System.ComponentModel;
using Bio.Helpers.Common;

namespace Bio.Framework.Client.SL.JSPropertyGrid {

  public abstract class ASelectorValueControl : PropertyEditor {

    public ASelectorValueControl() 
      : base() { 
    }
    
    /// <summary>
    /// Данный конструктор используется в статических PropertyGrid
    /// </summary>
    /// <param name="label"></param>
    /// <param name="property"></param>
    public ASelectorValueControl(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }

    protected VSelection _selection = null;
    protected TextBox _txt = null;
    protected Button _btn = null;

    public virtual Boolean IsMultiselector { get; set; }
    public virtual String SelectorPlugin { get; set; }
    public virtual String DisplayField { get; set; }
    public virtual String ValueField { get; set; }

    protected abstract void _onSelectionChanged();
    protected void _setSelection(Object value) {
      var v_selection = value as VSelection;
      if (v_selection == null){
        if(this._selection is VSingleSelection)
          v_selection = new VSingleSelection { Value = value };
        else
          v_selection = new VMultiSelection { Value = value };
      }
      if (this._selection == v_selection) {
        if (this._selection is VSingleSelection) {
          var v_sel = this._selection as VSingleSelection;
          this._txt.Text = "поиск...";
          this._get_selector((s) => {
            s.GetSelection(v_sel, (is_found, args) => {
              if (is_found == true) {
                v_sel.Display = args.selection.Cast<VSingleSelection>().ValueRow[this.DisplayField] as String;
                this._onSelectionChanged();
              } else {
                v_sel.Value = null;
                v_sel.Display = null;
                this._onSelectionChanged();
              }

            });
          });
        } else
          this._onSelectionChanged();
        return;
      }

      if (!v_selection.GetType().Equals(this._selection.GetType()))
        throw new Exception("Несовместимость типов!");
      if (this._selection is VSingleSelection) {
        var v_sel = this._selection as VSingleSelection;
        var v_in_sel = v_selection as VSingleSelection;
        v_sel.Value = v_in_sel.ValueRow[this.ValueField];
        v_sel.Display = v_in_sel.ValueRow[this.DisplayField] as String;
        v_sel.ValueRow = v_in_sel.ValueRow;
        this._onSelectionChanged();
      } else {
        var v_sel = this._selection as VMultiSelection;
        var v_in_sel = v_selection as VMultiSelection;
        if (v_sel != null) {
          v_sel.Value = v_in_sel.Value;
          v_sel.filterParams = (CParams)v_in_sel.filterParams.Clone();
          this._onSelectionChanged();
        }
      }
    }

    protected IPlugin _ownerPlugin = null;
    protected ISelector _selector = null;
    protected void _get_selector(Action<ISelector> callback) {
      if (this._selector == null) {
        LoadSelector(this._ownerPlugin, this.SelectorPlugin, callback);
      } else {
        if (callback != null)
          callback(this._selector);
      }
    }

    public static void LoadSelector(IPlugin ownerPlg, String selectorName, Action<ISelector> callback) {
      ownerPlg.Env.LoadPlugin(ownerPlg, selectorName, null, (a) => {
        var v_selector = a.Plugin as ISelector;
        if (callback != null)
          callback(v_selector);
      });
    }

    public abstract Object Value { get; set; }


    protected Boolean _selectorOpened = false;
    protected void _showSelector() {
      if (!this._selectorOpened) {
        this._selectorOpened = true;
        this._get_selector((s) => {
          s.Select(this._selection, (mresult, args) => {
            if (mresult == true) {
              this._setSelection(args.selection);
              if (args.selection != null) {
                if(this._selection.IsMultiSelection())
                  this.Value = args.selection;
                else
                  this.Value = ((VSingleSelection)args.selection).ValueRow[this._selection.ValueField];
              } else
                this.Value = null;
            }
            this._selectorOpened = false;
          });
        });
      }
    }

  }
}
