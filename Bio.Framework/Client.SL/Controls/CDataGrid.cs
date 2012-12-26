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
using System.Threading;
using System.Linq;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;
using System.Windows.Controls.Primitives;
using Bio.Helpers.Controls.SL;
using Bio.Helpers.Common;

namespace Bio.Framework.Client.SL {

  public class CDataGrid : DataGrid {
    private CJSGrid _owner = null;
    public CDataGrid() : base() {
      this.LayoutUpdated += new EventHandler(this._layoutUpdated);
    }

    private Style _columnHeadrStyle = null;
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      this._owner = this.FindParentOfType<CJSGrid>();
      if (this._owner != null) {
        this._owner._jsClient.AfterLoadData += new JSClientEventHandler<AjaxResponseEventArgs>(_jsClient_AfterLoadData);
        this._owner._doOnDataGridAssigned(this);

        if (this._owner.delaidAssignPopupMenu != null) {
          this._owner.AssignPopupMenu(this._owner.delaidAssignPopupMenu);
          this._owner.delaidAssignPopupMenu = null;
        }

        if (!this._owner._alternatingRowBackgroundIsDefault)
          this.AlternatingRowBackground = this._owner._alternatingRowBackground;
      }
    }

    void CDataGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
      //throw new NotImplementedException();
      Thread.Sleep(1);
    }

    protected override void OnColumnReordered(DataGridColumnEventArgs e) {
      base.OnColumnReordered(e);
      this._owner._onColumnReordered(e);
    }

    protected override void OnDrop(DragEventArgs e) {
      base.OnDrop(e);
    }

    void _jsClient_AfterLoadData(CJsonStoreClient sender, AjaxResponseEventArgs args) {
      if ((this._owner != null) && 
            (this._owner._jsClient != null) && 
              (this._owner._jsClient.jsMetadata != null)) {
        var v_pk_fld = this._owner._jsClient.jsMetadata.getPKFields().FirstOrDefault();
        if (v_pk_fld != null)
          this._owner._multiselection.ValueField = v_pk_fld.name;
      }
    }


    protected Boolean _defaultHotKeysEnabled = true;

    protected override void OnKeyDown(KeyEventArgs e) {
      if (this._owner != null) {
        if (this._defaultHotKeysEnabled) {
          if ((e.Key == Key.R) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.Load(null, null);
            e.Handled = true;
          }
          if ((e.Key == Key.A) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner._multiselection.Values.Clear();
            this._owner._multiselection.Inversion = !this._owner._multiselection.Inversion;
            this._updateSelection();
            e.Handled = true;
          }
          if ((e.Key == Key.S) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner._multiselection.Inversion = !this._owner._multiselection.Inversion;
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
            this._owner.goPageFirst(null);
          }
          if ((e.Key == Key.Z) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.goPagePrev(null);
          }
          if ((e.Key == Key.X) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.goPageNext(null);
          }
          if ((e.Key == Key.P) && (Keyboard.Modifiers == ModifierKeys.Shift)) {
            this._owner.goPageLast(null);
          }
        }
        //MessageBox.Show("" + e.Key);
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
          //if (row.Header == null) {
            Int32 v_row_index = row.GetIndex();
            Int64 v_start_row = 0;
            if (this._owner._jsClient.pageSize > 0) 
              v_start_row = (this._owner._jsClient.pageCurrent - 1) * this._owner._jsClient.pageSize;
            String v_max_rownum = "" + (v_start_row + ((this._owner._jsClient.pageSize > 0) ? this._owner._jsClient.pageSize : this._owner._jsClient.DS.Count()));
            var v_max_rownum_len = v_max_rownum.Length;
            String v_num_fmt = new String('0', v_max_rownum_len);
            String v_rnum = String.Format("{0:" + v_num_fmt + "} ", v_start_row + v_row_index + 1);
            if (!this._owner.SuppressMultiselection && this._owner._jsClient.jsMetadata.multiselection) {
              var v_hpan = new StackPanel();
              v_hpan.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
              v_hpan.Orientation = Orientation.Horizontal;
              v_hpan.FlowDirection = System.Windows.FlowDirection.LeftToRight;

              var v_htxt = new TextBlock();
              v_htxt.Text = v_rnum;
              v_hpan.Children.Add(v_htxt);

              var v_row_data = row.DataContext as CRTObject;
              var v_hcbx = new CheckBox();
              var v_is_selected = this._owner._multiselection.CheckSelected(v_row_data);
              if (this._owner._multiselection.Inversion)
                v_hcbx.IsChecked = !v_is_selected;
              else
                v_hcbx.IsChecked = v_is_selected;

              v_hpan.Children.Add(v_hcbx);
              v_hcbx.Checked += new RoutedEventHandler((s1, e1) => {
                this._doOnRowCheckedChanged(v_row_data, true);
              });
              v_hcbx.Unchecked += new RoutedEventHandler((s1, e1) => {
                this._doOnRowCheckedChanged(v_row_data, false);
              });
              v_row_data.ExtObject = v_hcbx;
              row.Header = v_hpan;
            } else
              row.Header = v_rnum;



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
          if (!this._owner._multiselection.Inversion)
            this._owner._multiselection.AddSelected(row, false);
          else
            this._owner._multiselection.RemoveSelected(row, false);
        } else {
          if (this._owner._multiselection.Inversion)
            this._owner._multiselection.AddSelected(row, false);
          else
            this._owner._multiselection.RemoveSelected(row, false);
        }
        this._onRowCheckedChangedEnabled = true;
      }
    }

    /// <summary>
    /// Оновление состояния выбранных строк при изменении выборки
    /// </summary>
    internal void _updateSelection() {
      foreach (var r in this._owner._jsClient.DS) {
        var v_cbx = r.ExtObject as CheckBox;
        if (v_cbx != null) {
          this._onRowCheckedChangedEnabled = false; // отключаем вызовы this._miltiselection.AddSelected и this._miltiselection.RemoveSelected
                                                    // при изменении состояния чекбокса
          if (this._owner._multiselection.CheckSelected(r))
            v_cbx.IsChecked = !this._owner._multiselection.Inversion;
          else
            v_cbx.IsChecked = this._owner._multiselection.Inversion;
          this._onRowCheckedChangedEnabled = true;
          v_cbx.UpdateLayout();
        }
      }
    }

    protected override void OnLoadingRow(DataGridRowEventArgs e) {
      if (this._owner != null) {
        this._buildRowHeader(e.Row);
        //e.Row.Loaded += this._row_Loaded;
        this._owner._onLoadingRow(e);
      }
      base.OnLoadingRow(e);
    }

    void _row_Loaded(Object sender, RoutedEventArgs e) {
      var row = sender as DataGridRow;
      if (row != null) {
        row.Loaded -= this._row_Loaded;
        this._buildRowHeader(row);
      }
      //Thread.Sleep(100);
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
    public void disableColumnsChangedEvents() {
      this._colChangedEventsEnabled = false;
    }
    public void enableColumnsChangedEvents() {
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

      DataGridBoundColumn c = this.Columns.Cast<DataGridBoundColumn>().Where((col) => {
        return String.Equals(col.Binding.Path.Path, e.PropertyName, StringComparison.CurrentCultureIgnoreCase);
      }).FirstOrDefault();
      e.Cancel = c != null;

      if (!e.Cancel)
        this._owner._doOnBeforeGenColumn(e);
      if (!e.Cancel) {

        CJsonStoreMetadataFieldDef fldDef = this._owner._jsClient.fieldDefByName(e.PropertyName);
        e.Cancel = ((fldDef == null) || fldDef.hidden);
        if (!e.Cancel) {
          //e.Column.v
          if (fldDef != null) {
            e.Column.IsReadOnly = fldDef.readOnly;
            this._defaultHotKeysEnabled = this._defaultHotKeysEnabled && e.Column.IsReadOnly;
            if (fldDef.width > 0) {
              //Double vWidth = new Double(fldDef.width);
              e.Column.Width = new DataGridLength(fldDef.width);
            } else {
              //e.Column.Width = new DataGridLength(e.Column.Width.Value);
            }
            String headerStr = fldDef.header;
            if (!String.IsNullOrEmpty(headerStr))
              e.Column.Header = headerStr;
            if (e.Column is DataGridTextColumn) {
              (e.Column as DataGridBoundColumn).Binding.Converter = new CurrFormatter(fldDef, e.Column);
              HorizontalAlignment v_alignment = fldDef.GetHorAlignment();
              Style st = new Style(typeof(TextBlock));
              st.Setters.Add(new Setter { Property = TextBlock.HorizontalAlignmentProperty, Value = v_alignment });
              (e.Column as DataGridBoundColumn).ElementStyle = st;
            }

            if (e.Column is DataGridCheckBoxColumn) {
              Thread.Sleep(100);
              //(e.Column as DataGridCheckBoxColumn).
            }

            this._owner._doOnAfterGenColumn(new CJSGridAfterGenColumnEventArgs(e.PropertyName, e.Column, fldDef));
          }
        }
      }

    }

  }
}
