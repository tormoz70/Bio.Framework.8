using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Linq;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;
using Bio.Helpers.Common;

namespace Bio.Framework.Client.SL {

  public class CDataGrid : DataGrid {
    private JSGrid _owner;
    public CDataGrid() {
      this.LayoutUpdated += this._layoutUpdated;
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      this._owner = this.FindParentOfType<JSGrid>();
      if (this._owner != null) {
        this._owner._doOnDataGridAssigned(this);

        if (this._owner.delaidAssignPopupMenu != null) {
          this._owner.AssignPopupMenu(this._owner.delaidAssignPopupMenu);
          this._owner.delaidAssignPopupMenu = null;
        }

        if (!this._owner.alternatingRowBackgroundIsDefault)
          this.AlternatingRowBackground = this._owner.alternatingRowBackground;
      }
    }

    protected override void OnColumnReordered(DataGridColumnEventArgs e) {
      base.OnColumnReordered(e);
      this._owner._onColumnReordered(e);
    }

    protected Boolean defaultHotKeysEnabled = true;

    protected override void OnKeyDown(KeyEventArgs e) {
      if (this._owner != null) {
        if (this.defaultHotKeysEnabled) {
          if ((e.Key == Key.R) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.Load(null, null);
            e.Handled = true;
          }
          if ((e.Key == Key.A) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.Selection.Values.Clear();
            this._owner.Selection.Inversion = !this._owner.Selection.Inversion;
            this._updateSelection();
            e.Handled = true;
          }
          if ((e.Key == Key.S) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.Selection.Inversion = !this._owner.Selection.Inversion;
            this._updateSelection();
            e.Handled = true;
          }
          if ((e.Key == Key.Space) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            if (this.SelectedItem != null) {
              var v_sld = this.SelectedItem as CRTObject;
              if (v_sld != null) {
                var v_cbx = v_sld.ExtObject as CheckBox;
                if (v_cbx != null)
                  v_cbx.IsChecked = !v_cbx.IsChecked;
              }
            }
            e.Handled = true;
          }
          if ((e.Key == Key.O) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.GoPageFirst(null);
          }
          if ((e.Key == Key.Z) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.GoPagePrev(null);
          }
          if ((e.Key == Key.X) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.GoPageNext(null);
          }
          if ((e.Key == Key.P) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.GoPageLast(null);
          }
        }
        this._owner._onKeyDown(e);
      }
      base.OnKeyDown(e);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
      base.OnSelectionChanged(e);
      if (this._owner != null) {
        this._owner._onSelectionChanged(e);
      }
    }

    private void _buildRowHeader(DataGridRow row) {
      if (this._owner != null) {
        if ((this.HeadersVisibility & DataGridHeadersVisibility.Row) == DataGridHeadersVisibility.Row) {
          var rowIndex = row.GetIndex();
          Int64 startRow = 0;
          if (this._owner.PageSize > 0)
            startRow = (this._owner.PageCurrent - 1) * this._owner.PageSize;
          var maxRownum = "" + (startRow + ((this._owner.PageSize > 0) ? this._owner.PageSize : this._owner.DS.Count()));
          var maxRownumLen = maxRownum.Length;
          var numFmt = new String('0', maxRownumLen);
// ReSharper disable FormatStringProblem
          var rnum = String.Format("{0:" + numFmt + "} ", startRow + rowIndex + 1);
// ReSharper restore FormatStringProblem
          if (!this._owner.SuppressMultiselection && this._owner.Multiselection) {
            var v_hpan = new StackPanel();
            v_hpan.HorizontalAlignment = HorizontalAlignment.Right;
            v_hpan.Orientation = Orientation.Horizontal;
            v_hpan.FlowDirection = FlowDirection.LeftToRight;

            var v_htxt = new TextBlock();
            v_htxt.Text = rnum;
            v_hpan.Children.Add(v_htxt);

            var rowData = (CRTObject)row.DataContext;
            var v_hcbx = new CheckBox();
            var v_is_selected = this._owner.Selection.CheckSelected(rowData);
            if (this._owner.Selection.Inversion)
              v_hcbx.IsChecked = !v_is_selected;
            else
              v_hcbx.IsChecked = v_is_selected;

            v_hpan.Children.Add(v_hcbx);
            v_hcbx.Checked += (s1, e1) => this._doOnRowCheckedChanged(rowData, true);
            v_hcbx.Unchecked += (s1, e1) => this._doOnRowCheckedChanged(rowData, false);
            rowData.ExtObject = v_hcbx;
            row.Header = v_hpan;
          } else
            row.Header = rnum;



          //}
        }
      }
    }

    private void _unbuildRowHeader(DataGridRow row) {
      if (this._owner != null) {
        if ((this.HeadersVisibility & DataGridHeadersVisibility.Row) == DataGridHeadersVisibility.Row) {
          //if (e.Row.Header != null) {
          row.Header = null;
          //}
        }
      }
    }

    /// <summary>
    /// Этот флаг отключает событие OnRowCheckedChanged
    /// Позволяе избежать зацикливания
    /// </summary>
    private Boolean _onRowCheckedChangedEnabled = true;
    private void _doOnRowCheckedChanged(CRTObject row, Boolean chked) {
      if (this._onRowCheckedChangedEnabled) {
        this._onRowCheckedChangedEnabled = false;
        if (chked) {
          if (!this._owner.Selection.Inversion)
            this._owner.Selection.AddSelected(row, false);
          else
            this._owner.Selection.RemoveSelected(row, false);
        } else {
          if (this._owner.Selection.Inversion)
            this._owner.Selection.AddSelected(row, false);
          else
            this._owner.Selection.RemoveSelected(row, false);
        }
        this._onRowCheckedChangedEnabled = true;
      }
    }

    /// <summary>
    /// Оновление состояния выбранных строк при изменении выборки
    /// </summary>
    internal void _updateSelection() {
      foreach (var r in this._owner.DS) {
        var v_cbx = r.ExtObject as CheckBox;
        if (v_cbx != null) {
          this._onRowCheckedChangedEnabled = false; // отключаем вызовы this._miltiselection.AddSelected и this._miltiselection.RemoveSelected
          // при изменении состояния чекбокса
          if (this._owner.Selection.CheckSelected(r))
            v_cbx.IsChecked = !this._owner.Selection.Inversion;
          else
            v_cbx.IsChecked = this._owner.Selection.Inversion;
          this._onRowCheckedChangedEnabled = true;
          v_cbx.UpdateLayout();
        }
      }
    }

    protected override void OnLoadingRow(DataGridRowEventArgs e) {
      if (this._owner != null) {
        this._buildRowHeader(e.Row);
        this._owner._onLoadingRow(e);
      }
      base.OnLoadingRow(e);
    }

    protected override void OnUnloadingRow(DataGridRowEventArgs e) {
      if (this._owner != null) {
        this._unbuildRowHeader(e.Row);
        this._owner._onUnloadingRow(e);
      }
      base.OnUnloadingRow(e);
    }

    private void _layoutUpdated(object sender, EventArgs e) {
      if (this._owner != null) {
        this._owner._refreshCurColumn();
      }
    }

    public event EventHandler OnAnColumnResized;

    private Boolean _colChangedEventsEnabled = true;
    public void DisableColumnsChangedEvents() {
      this._colChangedEventsEnabled = false;
    }
    public void EnableColumnsChangedEvents() {
      this._colChangedEventsEnabled = true;
    }
    protected override Size ArrangeOverride(Size finalSize) {
      delayedStarter.Do(500, () => {
        //Utl.UiThreadInvoke(() => {
        if (this._colChangedEventsEnabled) {
          var eve = this.OnAnColumnResized;
          if (eve != null) {
            eve(this, new EventArgs());
          }
        }
        //});
        //Utl.UiThreadInvoke(() => {
        //  MessageBox.Show("Resized: " + finalSize);
        //});
      });
      return base.ArrangeOverride(finalSize);
    }

    protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e) {
      base.OnAutoGeneratingColumn(e);

      DataGridBoundColumn c = this.Columns.Cast<DataGridBoundColumn>().Where(col => {
        return String.Equals(col.Binding.Path.Path, e.PropertyName, StringComparison.CurrentCultureIgnoreCase);
      }).FirstOrDefault();
      e.Cancel = c != null;

      if (!e.Cancel)
        this._owner._doOnBeforeGenColumn(e);
      if (!e.Cancel) {

        JsonStoreMetadataFieldDef fldDef = this._owner.FieldDefByName(e.PropertyName);
        e.Cancel = ((fldDef == null) || fldDef.Hidden);
        if (!e.Cancel) {
          //e.Column.v
          if (fldDef != null) {
            e.Column.IsReadOnly = fldDef.ReadOnly;
            this.defaultHotKeysEnabled = this.defaultHotKeysEnabled && e.Column.IsReadOnly;
            if (fldDef.Width > 0) {
              e.Column.Width = new DataGridLength(fldDef.Width);
            }
            var headerStr = fldDef.Header;
            if (!String.IsNullOrEmpty(headerStr))
              e.Column.Header = headerStr;
            if (e.Column is DataGridTextColumn) {
              (e.Column as DataGridBoundColumn).Binding.Converter = new CurrFormatter(fldDef, e.Column);
              (e.Column as DataGridBoundColumn).Binding.ValidatesOnDataErrors = true;
              var v_alignment = fldDef.GetHorAlignment();
              var st = new Style(typeof(TextBlock));
              st.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = v_alignment });
              (e.Column as DataGridBoundColumn).ElementStyle = st;
            }

            if (e.Column is DataGridCheckBoxColumn) {
              Thread.Sleep(100);
            }

            this._owner._doOnAfterGenColumn(new JSGridAfterGenColumnEventArgs(e.PropertyName, e.Column, fldDef));
          }
        }
      }

    }

  }
}
