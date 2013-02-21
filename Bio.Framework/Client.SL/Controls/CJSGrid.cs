using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Helpers.Common.Types;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Globalization;
using Bio.Helpers.Common;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Bio.Framework.Packets;
using System.Collections;
using Bio.Helpers.Controls.SL;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;

namespace Bio.Framework.Client.SL {

  public class CJSGridAfterGenColumnEventArgs : EventArgs {
    public String PropertyName { get; private set; }
    public DataGridColumn Column { get; private set; }
    public CJsonStoreMetadataFieldDef Field { get; private set; }
    public CJSGridAfterGenColumnEventArgs(String propertyName, DataGridColumn column, CJsonStoreMetadataFieldDef field)
      : base() {
      this.PropertyName = propertyName;
      this.Column = column;
      this.Field = field;
    }
  }

  public class CJSGrid : ContentControl, IDataControl {
    internal CJsonStoreClient _jsClient = null;

    public CJSGrid() {
      this.DefaultStyleKey = typeof(CJSGrid);
      this._multiselection = new VMultiSelection();
      this._jsClient = new CJsonStoreClient();
      this._jsClient.grid = this;
      this._jsClient.OnBeforeLoadData += this._onBeforeLoadData;
      this._jsClient.AfterLoadData += this._onAfterLoadData;
      this._jsClient.OnRowPropertyChanging += new JSClientEventHandler<JSClientRowPropertyChangingEventArgs>(_jsClient_OnRowPropertyChanging);
      this._jsClient.OnRowPropertyChanged += new JSClientEventHandler<JSClientRowPropertyChangedEventArgs>(_jsClient_OnRowPropertyChanged);
    }

    public event JSClientEventHandler<JSClientRowPropertyChangingEventArgs> OnRowPropertyChanging;
    public event JSClientEventHandler<JSClientRowPropertyChangedEventArgs> OnRowPropertyChanged;

    private void _jsClient_OnRowPropertyChanging(CJsonStoreClient sender, JSClientRowPropertyChangingEventArgs args) {
      var eve = this.OnRowPropertyChanging;
      if (eve != null)
        eve(sender, args);
    }
    private void _jsClient_OnRowPropertyChanged(CJsonStoreClient sender, JSClientRowPropertyChangedEventArgs args) {
      var eve = this.OnRowPropertyChanged;
      if (eve != null)
        eve(sender, args);
    }

    public Boolean SuppressMultiselection { get; set; }

    /// <summary>
    /// Происходит при отображении View в контейнере.
    /// </summary>
    public event EventHandler OnShow;
    public event EventHandler<DataGridAutoGeneratingColumnEventArgs> OnBeforeGenColumn;
    public event EventHandler<CJSGridAfterGenColumnEventArgs> OnAfterGenColumn;

    internal void _doOnBeforeGenColumn(DataGridAutoGeneratingColumnEventArgs args) {
      var eve = this.OnBeforeGenColumn;
      if (eve != null)
        eve(this, args);
    }
    internal void _doOnAfterGenColumn(CJSGridAfterGenColumnEventArgs args) {
      var eve = this.OnAfterGenColumn;
      if (eve != null)
        eve(this, args);
    }

    internal CDataGrid _dataGrid = null;

    private BusyIndicator _busyIndicator;

    private BusyIndicator busyIndicator {
      get {
        if (this._busyIndicator == null) {
          this._busyIndicator = this.GetTemplateChild("busyIndicator") as BusyIndicator;
        }
        return this._busyIndicator;
      }
    }

    internal VMultiSelection _multiselection = null;
    /// <summary>
    /// Возвращает текущую выборку при включенном атрибуте "miltiselection"
    /// </summary>
    public VMultiSelection Selection {
      get {
        return this._multiselection;
      }
    }

    public void BeginEdit() {
      this._dataGrid.Focus();
      this._dataGrid.BeginEdit();
    }

    public void SetSelection(VMultiSelection selection) {
      if (selection != null) {
        selection.ApplyTo(this._multiselection);
        if (this._dataGrid != null)
          this._dataGrid._updateSelection();
      }
    }

    public void SetSelection(String selection) {
      if (selection != null) {
        var v_selection = VMultiSelection.Create(selection);
        this.SetSelection(v_selection);
      }
    }

    private void _addBtnEvent(String btnName, RoutedEventHandler onClick) {
      if (onClick != null) {
        Button btn = this.GetTemplateChild(btnName) as Button;
        if (btn != null)
          btn.Click += onClick;
      }
    }

    private void _setBtnVisibility(String btnName, Visibility visibility) {
      Utl.UiThreadInvoke(() => {
        var btn = this.GetTemplateChild(btnName) as Button;
        if (btn != null) {
          btn.Visibility = visibility;
          btn.Focus();
        }
      });
    }

    internal void updPosState(Int64 curPage, Int64 lastPage) {
      TextBox tbx = this.GetTemplateChild("tbxPagePos") as TextBox;
      if (tbx != null)
        tbx.Text = "" + curPage;
      TextBlock tblck = this.GetTemplateChild("tbxLastPage") as TextBlock;
      if (tblck != null)
        tblck.Text = ((lastPage == -1) ? "?" : "" + lastPage);
    }

    private GridLength fPanelNavRowHeight = new GridLength(0);
    private void _updNavPanelVisibility() {
      StackPanel pnl = this.GetTemplateChild("navPanel") as StackPanel;
      if (pnl != null) {
        if (this.pageSize == 0)
          pnl.Visibility = System.Windows.Visibility.Collapsed;
        else
          pnl.Visibility = System.Windows.Visibility.Visible;
      }
    }

    private void _updAutoRefreshPanelVisibility() {
      var pnl = this.GetTemplateChild("grdAutoRefresh") as Grid;
      if (this.AutoRefreshEnabled)
        pnl.Visibility = System.Windows.Visibility.Visible;
      else
        pnl.Visibility = System.Windows.Visibility.Collapsed;
      var cbx = this.GetTemplateChild("cbxAutoRefresh") as CheckBox;
      if (cbx != null) {
        cbx.Checked += this.cbxAutoRefresh_Checked;
        cbx.Unchecked += this.cbxAutoRefresh_Checked;
      }
      var nud = this.GetTemplateChild("numudAutoRefreshPeriod") as NumericUpDown;
      if (nud != null) {
        nud.Minimum = 5;
        nud.Maximum = 60;
        nud.Increment = 5;
        nud.Value = this._autoRefreshPeriodSecs;
        nud.ValueChanged += new RoutedPropertyChangedEventHandler<Double>(nud_ValueChanged);
      }
    }

    void nud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      this.AutoRefreshPeriod = (Int32)e.NewValue;
    }

    private void cbxAutoRefresh_Checked(Object sender, RoutedEventArgs e) {
      if ((sender as CheckBox).IsChecked == true)
        this.AutoRefreshStart();
      else
        this.AutoRefreshStop();
    }

    public void showBusyIndicator(String text) {
      Utl.UiThreadInvoke(() => {
        if (this.busyIndicator != null) {
          this.busyIndicator.BusyContent = text;
          this.busyIndicator.IsBusy = true;
        }
      });
    }
    public void hideBusyIndicator() {
      Utl.UiThreadInvoke(() => {
        if (this.busyIndicator != null)
          this.busyIndicator.IsBusy = false;
      });
    }

    private CExpClient _exporter = null;
    //private CJSGridCols _cfgEditor = null;

    internal Boolean _alternatingRowBackgroundIsDefault = true;
    internal Brush _alternatingRowBackground = null;
    public Brush AlternatingRowBackground {
      get {
        if (this._dataGrid != null)
          return this._dataGrid.AlternatingRowBackground;
        else
          return this._alternatingRowBackground;
      }
      set {
        this._alternatingRowBackground = value;
        this._alternatingRowBackgroundIsDefault = false;
        if (this._dataGrid != null)
          this._dataGrid.AlternatingRowBackground = this._alternatingRowBackground;
      }
    }

    private Dictionary<String, String> _getColumnHeaderList() {
      var rslt = new Dictionary<String, String>();
      foreach (var c in this._dataGrid.Columns)
        rslt.Add(GetDataGridColumnName(c).ToUpper(), ((String)c.Header).Replace("\n", " "));
      return rslt;
    }

    public void goPageFirst(AjaxRequestDelegate callback) {
      if (this._jsClient.pageSize == this.pageSize)
        this._jsClient.goPageFirst(callback);
      else
        this.Refresh(callback);
    }
    public void goPagePrev(AjaxRequestDelegate callback) {
      if (this._jsClient.pageSize == this.pageSize)
        this._jsClient.goPagePrev(callback);
      else
        this.Refresh(callback);
    }
    public void goPageNext(AjaxRequestDelegate callback) {
      if (this._jsClient.pageSize == this.pageSize)
        this._jsClient.goPageNext(callback);
      else
        this.Refresh(callback);
    }
    public void goPageLast(AjaxRequestDelegate callback) {
      if (this._jsClient.pageSize == this.pageSize)
        this._jsClient.goPageLast(callback);
      else
        this.Refresh(callback);
    }
    public void Refresh(AjaxRequestDelegate callback) {
      this._setBtnVisibility("btnRefreshFirst", Visibility.Collapsed);
      this._jsClient.Refresh(callback);
    }

    public Boolean AutoRefreshEnabled { get; set; }
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      this._addBtnEvent("btnFirst", new RoutedEventHandler((s, a) => {
        this.goPageFirst(null);
      }));
      this._addBtnEvent("btnPrev", new RoutedEventHandler((s, a) => {
        this.goPagePrev(null);
      }));
      this._addBtnEvent("btnNext", new RoutedEventHandler((s, a) => {
        this.goPageNext(null);
      }));
      this._addBtnEvent("btnLast", new RoutedEventHandler((s, a) => {
        this.goPageLast(null);
      }));
      this._addBtnEvent("btnRefresh", new RoutedEventHandler((s, a) => {
        this.Refresh(null);
      }));
      this._addBtnEvent("btnRefreshFirst", new RoutedEventHandler((s, a) => {
        this.Load();
      }));
      this._addBtnEvent("btnExp", new RoutedEventHandler((s, a) => {
        if (this._exporter == null) {
          this._exporter = new CExpClient(this.ajaxMng, this.bioCode, this.Title ?? "Экспорт");
        } else
          this._exporter.bioCode = this.bioCode;
        this._exporter.title = this.Title;
        this.bioParams.SetValue("dataGridOnClientHeaders", this._getColumnHeaderList());
        this._exporter.runProc(this.bioParams, null);

      }));
      this._addBtnEvent("btnCfg", new RoutedEventHandler((s, a) => {
        this._editCfg();
      }));

      this._updNavPanelVisibility();
      this._updAutoRefreshPanelVisibility();
      this._doOnShow();
    }

    internal void _doOnDataGridAssigned(CDataGrid dataGrid) {
      this._dataGrid = dataGrid;
      this._dataGrid.OnAnColumnResized += this._doOnAnColumnResized;
    }

    private Boolean _isShown = false;
    private void _doOnShow() {
      if (!this._isShown) {
        this._isShown = true;
        this._callOnShowDelaiedHandler(this, new EventArgs());
        var eve = this.OnShow;
        if (eve != null)
          eve(this, new EventArgs());
      }
    }
    //public void 

    private String _generateGridUID() {
      String v_prnt_typeName = null;
      Utl.UiThreadInvoke(() => {
        var v_prnt = Utl.FindParentElem1<UserControl>(this);
        if (v_prnt == null)
          v_prnt = Utl.FindParentElem1<FloatableWindow>(this);
        v_prnt_typeName = (v_prnt != null) ? v_prnt.GetType().FullName : this.GetType().Name;
      });
      var v_rslt = v_prnt_typeName + "-[" + this.bioCode + "]";
      return v_rslt;
    }

    /// <summary>
    /// Используется при экспорте в MS Excel
    /// </summary>
    public String Title { get; set; }

    private Int32 _selectedIndex = 0;
    private CRTObject _selectedItem = null;
    private IList _selectedItems = null;
    public event SelectionChangedEventHandler SelectedChanged;
    internal void _onSelectionChanged(SelectionChangedEventArgs e) {
      if (this._dataGrid != null) {
        this._selectedIndex = this._dataGrid.SelectedIndex;
        this._selectedItem = this._dataGrid.SelectedItem as CRTObject;
        this._selectedItems = this._dataGrid.SelectedItems;
      }
      var handler = this.SelectedChanged;
      if (handler != null) {
        try {
          handler(this, e);
        } catch (Exception ex) {
          throw new EBioException("Непредвиденная ошибка при вызове обработчика собития \"SelectedChanged\"! Сообщение: " + ex.Message, ex); 
        }
      }
    }

    public new event KeyEventHandler KeyDown;
    internal void _onKeyDown(KeyEventArgs e) {
      var handler = this.KeyDown;
      if (handler != null) {
        handler(this, e);
      }
      //if (!e.Handled) {
      //  if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
      //    if (e.Key == Key.F5) {
      //      this._jsClient.Refresh(null);
      //    }
      //  }
      //}
    }

    public event EventHandler<DataGridRowEventArgs> OnLoadingRow;
    internal void _onLoadingRow(DataGridRowEventArgs e) {
      var handler = this.OnLoadingRow;
      if (handler != null) {
        handler(this, e);
      }
    }

    public event EventHandler<DataGridRowEventArgs> OnUnloadingRow;
    internal void _onUnloadingRow(DataGridRowEventArgs e) {
      var handler = this.OnUnloadingRow;
      if (handler != null) {
        handler(this, e);
      }
    }

    public event JSClientEventHandler<CancelEventArgs> OnBeforeLoadData;
    private void _onBeforeLoadData(CJsonStoreClient sender, CancelEventArgs args) {
      this._dataIsLoaded = false;
      if (this.OnBeforeLoadData != null) {
        this.OnBeforeLoadData(sender, args);
      }
    }

    private Boolean _dataIsLoaded = false;
    public event JSClientEventHandler<AjaxResponseEventArgs> OnAfterLoadData;
    void _onAfterLoadData(CJsonStoreClient sender, AjaxResponseEventArgs e) {
      // Все эти "танцы с бубном", чтобы на событии "AfterLoadData" грид уже содержал колонки загруженных данных
      this._forward_sender = sender;
      this._forward_args = e;
      if (this._dataGrid != null) {
        this._dataGrid.LayoutUpdated += this._dataGrid_LayoutUpdated;
      } else {
        this._prepareGridAfterLoadData();
      }
      this._dataIsLoaded = true;
    }

    public Boolean DataIsLoaded {
      get {
        return this._dataIsLoaded;
      }
    }

    private CJsonStoreClient _forward_sender = null;
    private AjaxResponseEventArgs _forward_args = null;
    void _dataGrid_LayoutUpdated(object sender, EventArgs e) {
      this._dataGrid.LayoutUpdated -= this._dataGrid_LayoutUpdated;
      this._prepareGridAfterLoadData();
    }

    private void _prepareGridAfterLoadData() {
      //if(this.Columns.Count != this._cfg.columnDefs.Count)
      //  this._cfg.refresh(this);
      //else
      this._applyCfgToGrid();
      var handler = this.OnAfterLoadData;
      if (handler != null) {
        handler(this._forward_sender, this._forward_args);
      }
    }

    private CJSGridConfig _cfg = null;
    private void _restoreCfg() {
      var uid = this._generateGridUID();
      this._cfg = CJSGridConfig.restore(uid, null);
      if (this._cfg == null)
        this._cfg = this._creCfg();
    }

    private void _storeCfg(Boolean skipRefresh) {
      if (this._cfg != null) {
        Utl.UiThreadInvoke(new Action<CJSGrid>((grd) => {
          if (!skipRefresh)
            grd._cfg.refresh(grd);
          grd._cfg.store(grd._generateGridUID());
        }), this);
      }
    }
    private void _storeCfg() {
      this._storeCfg(false);
    }

    private Int64 _defaultPageSize = 30;
    public Int64 DefaultPageSize {
      get {
        return this._defaultPageSize;
      }
      set {
        this._defaultPageSize = value;
        this._updNavPanelVisibility();
      }
    }

    private Boolean _suppressPaging = false;
    public Boolean SuppressPaging {
      get {
        return this._suppressPaging;
      }
      set {
        this._suppressPaging = value;
        this._updNavPanelVisibility();
      }
    }

    public Int64 pageSize {
      get {
        return (this.SuppressPaging) ? 0 : (((this._cfg != null) && (this._cfg.pageSize != null)) ? (Int64)this._cfg.pageSize : this.DefaultPageSize);
      }
    }

    public String bioCode {
      get {
        return this._jsClient.bioCode;
      }
      set {
        var oldBioCode = this._jsClient.bioCode;
        this._jsClient.bioCode = value;
        if (!String.Equals(oldBioCode, this._jsClient.bioCode))
          this._restoreCfg();
      }
    }
    public IAjaxMng ajaxMng {
      get {
        return this._jsClient.ajaxMng;
      }
      set {
        this._jsClient.ajaxMng = value;
      }
    }
    public Params bioParams {
      get {
        return this._jsClient.bioParams;
      }
      set {
        this._jsClient.bioParams = value;
      }
    }

    //public DataGridHeadersVisibility RowHea {
    //  get {
    //    DataGrid grd = this._getGrid();
    //    return grd.HeadersVisibility;
    //  }
    //  set { 
    //  }
    //}

    //public static DependencyProperty HeadersVisibilityProperty = DependencyProperty.Register("HeadersVisibility", typeof(DataGridHeadersVisibility), typeof(CJSGrid), new PropertyMetadata(DataGridHeadersVisibility.All));
    //public DataGridHeadersVisibility HeadersVisibility {
    //  get { return (DataGridHeadersVisibility)this.GetValue(HeadersVisibilityProperty); }
    //  set { this.SetValue(HeadersVisibilityProperty, value); }
    //}

    public static DependencyProperty ColumnResizedProperty = DependencyProperty.Register("ColumnResized", typeof(SizeChangedEventHandler), typeof(CJSGrid), new PropertyMetadata(null));
    public SizeChangedEventHandler ColumnResized {
      get { return (SizeChangedEventHandler)this.GetValue(ColumnResizedProperty); }
      set { this.SetValue(ColumnResizedProperty, value); }
    }

    public static DependencyProperty TestTxtProperty = DependencyProperty.Register("TestTxt", typeof(String), typeof(CJSGrid), new PropertyMetadata(String.Empty));
    public String TestTxt {
      get { return (String)this.GetValue(TestTxtProperty); }
      set { this.SetValue(TestTxtProperty, value); }
    }

    public static DependencyProperty HeadersVisibilityProperty = DependencyProperty.Register("HeadersVisibility", typeof(DataGridHeadersVisibility), typeof(CJSGrid), new PropertyMetadata(DataGridHeadersVisibility.All));
    public DataGridHeadersVisibility HeadersVisibility {
      get { return (DataGridHeadersVisibility)this.GetValue(HeadersVisibilityProperty); }
      set { this.SetValue(HeadersVisibilityProperty, value); }
    }

    public static DependencyProperty RowHeaderWidthProperty = DependencyProperty.Register("RowHeaderWidth", typeof(Int32), typeof(CJSGrid), new PropertyMetadata(40));
    public Int32 RowHeaderWidth {
      get { return (Int32)this.GetValue(RowHeaderWidthProperty); }
      set { this.SetValue(RowHeaderWidthProperty, value); }
    }

    //public static DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(Int32), typeof(CJSGrid), new PropertyMetadata(-1));
    public Int32 SelectedIndex {
      get { return this._selectedIndex; }
      set {
        CRTObject[] v_rows = this._dataGrid.ItemsSource.Cast<CRTObject>().ToArray();
        if (v_rows.Length > 0) {
          int v_seld_index = (value >= v_rows.Length) ? v_rows.Length - 1 : (value < 0) ? 0 : value;
          this.SelectedItem = v_rows[v_seld_index];
        }
      }
    }

    public void SelectFirst() {
      this._callOnShowDelaied(() => {
        this.SelectedItem = this._jsClient.DS.FirstOrDefault();
      });
    }

    private Queue<Action> _callOnShowDelaiedCallback = new Queue<Action>();
    private void _callOnShowDelaiedHandler(Object sender, EventArgs e) {
      while (this._callOnShowDelaiedCallback.Count > 0) {
        var v_act = this._callOnShowDelaiedCallback.Dequeue();
        if (v_act != null)
          v_act();
      }
    }

    private void _callOnShowDelaied(Action callback) {
      if (this.IsShown) {
        if (callback != null)
          callback();
      } else {
        this._callOnShowDelaiedCallback.Enqueue(callback);
      }
    }

    public Boolean IsShown {
      get {
        return this._isShown;
      }
    }

    //public static DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(Object), typeof(CJSGrid), new PropertyMetadata(null));
    public CRTObject SelectedItem {
      get { return this._selectedItem; }
      set {
        if (this._dataGrid != null) {
          this._dataGrid.SelectedItem = value;
          if (this._dataGrid.SelectedItem != null) {
            Utl.UiThreadInvoke(() => {
              try {
                this._dataGrid.ScrollIntoView(this._dataGrid.SelectedItem, this._dataGrid.Columns.FirstOrDefault());
              } catch { }
            });
          }
        }
      }
    }

    public DataGridColumn CurrentColumn {
      get {
        return (this._dataGrid != null) ? this._dataGrid.CurrentColumn : null;
      }
    }

    private Boolean _refreshCurColumnEnabled = false;
    private void _enableRefreshCurColumn() {
      this._refreshCurColumnEnabled = true;
    }
    private void _disableRefreshCurColumn() {
      this._refreshCurColumnEnabled = false;
    }
    internal void _refreshCurColumn() {
      if (this._refreshCurColumnEnabled) {
        this._disableRefreshCurColumn();
        if ((this._dataGrid != null) && (this._dataGrid.SelectedItem != null)) {
          foreach (var c in this._dataGrid.Columns)
            if (c.DisplayIndex == this._currentColumnIndex) {
              Utl.UiThreadInvoke(() => {
                try {
                  this._dataGrid.CurrentColumn = c;
                  this._dataGrid.ScrollIntoView(this._dataGrid.SelectedItem, this._dataGrid.CurrentColumn);
                } catch { }
              });
              break;
            }
        }
        //this._enableRefreshCurColumn();
      }
    }

    private Int32 _currentColumnIndex = -1;
    public Int32 CurrentColumnIndex {
      get {
        return ((this._dataGrid != null) && (this._dataGrid.CurrentColumn != null)) ? this._dataGrid.CurrentColumn.DisplayIndex : -1;
      }
      set {
        this._currentColumnIndex = value;
        this._enableRefreshCurColumn();
      }
    }

    //public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CJSGrid), new PropertyMetadata(null));
    public IEnumerable ItemsSource {
      get {
        return (this._dataGrid != null) ? this._dataGrid.ItemsSource : null;
      }
      set {
        if (this._dataGrid != null)
          this._dataGrid.ItemsSource = value;
      }
    }

    public ObservableCollection<DataGridColumn> Columns {
      get {
        if (this._dataGrid != null)
          return this._dataGrid.Columns;
        else
          return null;
      }
    }

    public static String GetDataGridColumnName(DataGridColumn column) {
      DataGridBoundColumn col = column as DataGridBoundColumn;
      if ((col.Binding != null) && (col.Binding.Path != null))
        return col.Binding.Path.Path;
      else
        return null;
    }

    public DataGridColumn ColumnByBindingPath(String bindingPath) {
      if (this.Columns != null)
        return this.Columns.Where((c) => { return String.Equals(GetDataGridColumnName(c), bindingPath, StringComparison.CurrentCultureIgnoreCase); }).FirstOrDefault();
      else
        return null;
    }

    public String CurrentColumnName {
      get {
        if (this._dataGrid != null) {
          return GetDataGridColumnName(this._dataGrid.CurrentColumn);
        } else
          return null;
      }
      set {
        var col = ColumnByBindingPath(value);
        if (col != null) {
          this.CurrentColumnIndex = col.DisplayIndex;
        }
      }
    }

    public Object SelectedCell {
      get {
        if ((this._dataGrid != null) && (this._dataGrid.SelectedItem != null))
          return Utl.GetPropertyValue<String>(this._dataGrid.SelectedItem, this.CurrentColumnName);
        else
          return null;
      }
    }

    public IList SelectedItems {
      get { return this._selectedItems; }
    }

    public static DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(DataGridSelectionMode), typeof(CJSGrid), new PropertyMetadata(DataGridSelectionMode.Single));
    public DataGridSelectionMode SelectionMode {
      get { return (DataGridSelectionMode)this.GetValue(SelectionModeProperty); }
      set { this.SetValue(SelectionModeProperty, value); }
    }

    private class LoadParams<T> {
      public Params bioParams { get; set; }
      public T callback { get; set; }
      public CJsonStoreFilter locate { get; set; }
    };
    private LoadParams<AjaxRequestDelegate> _suspendLoadParams = null;
    private Boolean _isFirstLoading = true;
    public void Load(Params bioParams, AjaxRequestDelegate callback, CJsonStoreFilter locate) {
      if (this.SuspendFirstLoad && this._isFirstLoading) {
        this._suspendLoadParams = new LoadParams<AjaxRequestDelegate> {
          bioParams = (bioParams != null) ? (Params)bioParams.Clone() : null,
          callback = callback,
          locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
        };
        this._setBtnVisibility("btnRefreshFirst", Visibility.Visible);
        this._isFirstLoading = false;
      } else {
        this._setBtnVisibility("btnRefreshFirst", Visibility.Collapsed);
        var loadParams = this._suspendLoadParams ?? new LoadParams<AjaxRequestDelegate> {
          bioParams = (bioParams != null) ? (Params)bioParams.Clone() : null,
          callback = callback,
          locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
        };
        this._suspendLoadParams = null;
        this._callOnShowDelaied(() => {
          
          try {
            this._jsClient.Load(loadParams.bioParams, (sndr, a) => {
              this.hideBusyIndicator();

              if (a.response.success) {
                CJsonStoreResponse rsp = (a.response as CJsonStoreResponse);
                if (rsp != null) {

                  if (this._dataGrid != null)
                    this._dataGrid.UpdateLayout();

                  this.updPosState((sndr as CJsonStoreClient).pageCurrent, (sndr as CJsonStoreClient).pageLast);
                  if (loadParams.callback != null)
                    loadParams.callback(sndr, a);
                }
              }
            }, loadParams.locate);
          } catch {
            this.hideBusyIndicator();
            throw;
          }
        });
      }
    }

    private LoadParams<EventHandler<OnSelectEventArgs>> _suspendLocateParams = null;
    public void Locate(CJsonStoreFilter locate, EventHandler<OnSelectEventArgs> callback, Boolean forceRemote) {
      this._callOnShowDelaied(() => {
        if (this.SuspendFirstLoad && this._isFirstLoading) {
          this._suspendLocateParams = new LoadParams<EventHandler<OnSelectEventArgs>> {
            bioParams = null,
            callback = callback,
            locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
          };
          this._setBtnVisibility("btnRefreshFirst", Visibility.Visible);
          this._isFirstLoading = false;
        } else {
          this._setBtnVisibility("btnRefreshFirst", Visibility.Collapsed);
          var locateParams = this._suspendLocateParams ?? new LoadParams<EventHandler<OnSelectEventArgs>> {
            bioParams = null,
            callback = callback,
            locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
          };
          this._suspendLocateParams = null;
          this._jsClient.Locate(locateParams.locate, locateParams.callback, forceRemote);
        }
      });
    }

    public void Locate(CJsonStoreFilter locate, EventHandler<OnSelectEventArgs> callback) {
      this.Locate(locate, callback, false);
    }

    public void Locate(CJsonStoreFilter locate, Boolean forceRemote) {
      this.Locate(locate, null, forceRemote);
    }

    public void Locate(CJsonStoreFilter locate) {
      this.Locate(locate, null, false);
    }

    public void Load(Params bioParams, AjaxRequestDelegate callback) {
      this.Load(bioParams, callback, null);
    }

    public void Load(Params bioParams) {
      this.Load(bioParams, null, null);
    }

    public void Load() {
      this.Load(null, null, null);
    }


    internal PopupMenu delaidAssignPopupMenu = null;
    public void AssignPopupMenu(PopupMenu menu) {
      if (this._dataGrid == null)
        this.delaidAssignPopupMenu = menu;
      else
        menu.AddTrigger(TriggerTypes.RightClick, this._dataGrid);
    }

    public void ClearData() {
      this._jsClient.ClearData();
      this.AutoRefreshStop();
    }

    public void SelectItems(Func<Object, Boolean> onScan) {
      var itms = this._dataGrid.ItemsSource.Cast<Object>();
      var found_itms = itms.Where(onScan);
      if (this._dataGrid.SelectionMode == DataGridSelectionMode.Extended) {
        foreach (var s in found_itms)
          this._dataGrid.SelectedItems.Add(s);
      } else {
        this._dataGrid.SelectedItem = found_itms.FirstOrDefault();
        this._dataGrid.Focus();
      }
    }

    public IEnumerable<Object> FindRows(Func<Object, Boolean> onScan) {
      var itms = this._dataGrid.ItemsSource.Cast<Object>();
      return itms.Where(onScan);
    }
    /// <summary>
    /// Тип Строки
    /// </summary>
    public Type RowType {
      get {
        return this._jsClient.RowType;
      }
    }
    /// <summary>
    /// Создает объект тип RowType
    /// </summary>
    /// <returns></returns>
    public CRTObject NewRow() {
      var row = this._jsClient.NewRow();
      return row;
    }
    /// <summary>
    /// Вставляет строку в позицию
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public void InsertRow(Int32 index, CRTObject row) {
      this._jsClient.InsertRow(index, row);
      this._dataGrid.ScrollIntoView(row, this._dataGrid.Columns.FirstOrDefault());
    }
    /// <summary>
    /// Добавляет строку
    /// </summary>
    /// <returns></returns>
    public void AddRow(CRTObject row) {
      this._jsClient.AddRow(row);
      this._dataGrid.ScrollIntoView(row, this._dataGrid.Columns.FirstOrDefault());
      this._dataGrid.SelectedItem = row;
      DataGridColumn curCol = null;
      foreach (var col in this._dataGrid.Columns)
        if (!col.IsReadOnly) {
          curCol = col;
          break;
        }
      this._dataGrid.CurrentColumn = curCol ?? this._dataGrid.Columns[0];
      this.BeginEdit();
    }
    /// <summary>
    /// Удаляет строку по индексу
    /// </summary>
    /// <param name="index"></param>
    public void RemoveRowAt(Int32 index) {
      int v_seld_indx = this.SelectedIndex;
      this._jsClient.RemoveRowAt(index);
      this.SelectedIndex = v_seld_indx;
    }
    /// <summary>
    /// Удаляет строку по значению
    /// </summary>
    /// <param name="row"></param>
    public void RemoveRow(CRTObject row) {
      int v_seld_indx = this.SelectedIndex;
      this._jsClient.RemoveRow(row);
      this.SelectedIndex = v_seld_indx;
    }

    /// <summary>
    /// Удаляет выбранные строки
    /// </summary>
    public void RemoveSelectedRows() {
      int v_seld_indx = this.SelectedIndex;
      var v_seld = new List<CRTObject>();
      foreach (var r in this._dataGrid.SelectedItems)
        v_seld.Add(r as CRTObject);
      foreach (var r in v_seld)
        this._jsClient.RemoveRow(r as CRTObject);
      this.SelectedIndex = v_seld_indx;
    }

    /// <summary>
    /// Сохраняет все сделанные изменения
    /// </summary>
    /// <param name="callback">Выполняется по завершении запроса</param>
    /// <param name="trunsactionID">ID транзакции</param>
    /// <param name="cmd">Команда для транзакции</param>
    public void SaveData(AjaxRequestDelegate callback, String trunsactionID, CSQLTransactionCmd cmd) {
      this.showBusyIndicator("сохранение изменений...");
      try {
        this._jsClient.Post((s, a) => {
          this.hideBusyIndicator();
          if (callback != null)
            callback(s, a);
        }, trunsactionID, cmd);
      } catch {
        this.hideBusyIndicator();
        throw;
      }
    }

    /// <summary>
    /// Сохраняет все сделанные изменения
    /// </summary>
    /// <param name="callback">Выполняется по завершении запроса</param>
    public void SaveData(AjaxRequestDelegate callback) {
      this.SaveData(callback, null, CSQLTransactionCmd.Nop);
    }

    /// <summary>
    /// Сохраняет все сделанные изменения
    /// </summary>
    public void SaveData() {
      this.SaveData(null);
    }

    /// <summary>
    /// Метаданные набора данных
    /// </summary>
    public CJsonStoreMetadata jsMetadata {
      get {
        return this._jsClient.jsMetadata;
      }
    }

    public IEnumerator<DataGridRow> GetEnumerator() {
      if (this._dataGrid != null)
        return this._dataGrid.GetRowsEnumerator();
      else
        return null;
    }

    private Int32 _autoRefreshPeriodSecs = 20;
    public Int32 AutoRefreshPeriod {
      get { return this._autoRefreshPeriodSecs; }
      set {
        this._autoRefreshPeriodSecs = value;
        if (this._autoRefreshTimer != null)
          this._autoRefreshTimer.Interval = new TimeSpan(0, 0, this._autoRefreshPeriodSecs);

      }
    }

    private DispatcherTimer _autoRefreshTimer = null;
    public void AutoRefreshStart(Int32 periodSecs) {
      if (periodSecs > 0)
        this._autoRefreshPeriodSecs = periodSecs;
      if (this._autoRefreshTimer == null) {
        this._autoRefreshTimer = new DispatcherTimer();
        this._autoRefreshTimer.Interval = new TimeSpan(0, 0, this._autoRefreshPeriodSecs);
        this._autoRefreshTimer.Tick += this._autoRefreshTimer_Tick;
      }
      this._autoRefreshTimer.Start();
    }
    public void AutoRefreshStart() {
      this.AutoRefreshStart(0);
    }

    private Boolean _autorefreshing = false;
    private void _autoRefreshTimer_Tick(Object sender, EventArgs e) {
      if (!this._autorefreshing) {
        this._autorefreshing = true;
        this._jsClient.Refresh((s, a) => {
          this._autorefreshing = false;
        });
      }
    }
    public void AutoRefreshStop() {
      if (this._autoRefreshTimer != null)
        this._autoRefreshTimer.Stop();
    }
    public void LoadSelectedPks(AjaxRequestDelegate callback) {
      this._jsClient.LoadSelectedPks(callback);
    }

    public Boolean IsChanged {
      get {
        return this._jsClient.Changes.Count > 0;
      }
    }

    private CJSGridConfig _creCfg() {
      var v_rslt = new CJSGridConfig();
      if (this._jsClient != null)
        v_rslt.pageSize = this.DefaultPageSize;
      v_rslt.refresh(this);
      return v_rslt;
    }

    private void _editCfg() {
      var v_cfgEditor = new CJSGridProps();
      v_cfgEditor.Title = "Выбор колонок таблицы";
      v_cfgEditor.Closed += new EventHandler((Object sender, EventArgs e) => {
        if (((FloatableWindow)sender).DialogResult == true) {
          this._applyCfgToGrid();
          this._storeCfg(true);
          this._updNavPanelVisibility();
          this.Refresh(null);
        }
      });
      if (this._cfg == null)
        this._restoreCfg();
      v_cfgEditor.ShowDialog(this._cfg);
    }

    private void _applyCfgToGrid() {
      this._dataGrid.disableColumnsChangedEvents();
      try {
        if ((this._cfg == null) || !String.Equals(this._generateGridUID(), this._cfg.uid))
          this._restoreCfg();
        if (this._cfg != null) {
          foreach (var c in this._cfg.columnDefs) {
            var v_col = this.Columns.Where((cc) => {
              return String.Equals(((DataGridBoundColumn)cc).Binding.Path.Path, c.FieldName, StringComparison.CurrentCultureIgnoreCase);
            }).FirstOrDefault();
            if (v_col != null) {
              if ((c.Index >= 0) && (c.Index < this.Columns.Count))
                v_col.DisplayIndex = c.Index;
              if (BioGlobal.Debug) {
                if (c.Width > 5.0D)
                  v_col.Width = new DataGridLength(c.Width);
              }
              v_col.Visibility = (c.IsChecked ? Visibility.Visible : Visibility.Collapsed);
            }
          }
        }
      } finally {
        this._dataGrid.enableColumnsChangedEvents();
      }
    }

    internal void _onColumnReordered(DataGridColumnEventArgs e) {
      //if (this._dataIsLoaded && this._colChangedEventsEnabled)
      //  this._storeCfg();
    }

    private void _doOnAnColumnResized(Object sender, EventArgs args) {
      if (this._dataIsLoaded)
        this._storeCfg();
    }

    public void CancelChanges(CRTObject row) {
      if (this._jsClient != null)
        this._jsClient.CancelChanges(row);
    }
    public void CancelChanges() {
      if (this._jsClient != null)
        this._jsClient.CancelChanges();
    }

    //internal Object getTemplOgHeader() {
    //  return this.Resources["MyCustomColHeaderCtrl"] as Style;
    //}
    public Boolean SuspendFirstLoad { get; set; }
  }

  #region CoClasses

  public static class Extensions {
    //public static DataGridColumn getCol(this DataGridColumnHeader element) {
    //  //DataGridColumn rslt = element.FindParentOfType<DataGridColumn>();
    //  DataGridColumn rslt = element.;
    //  return rslt;
    //}
    public static T FindParentOfType<T>(this FrameworkElement element) {
      var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;

      while (parent != null) {
        if (parent is T)
          return (T)(object)parent;

        parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
      }
      return default(T);
    }

    // Methods
    public static List<T> GetChildrenByType<T>(this UIElement element) where T : UIElement {
      return element.GetChildrenByType<T>(null);
    }

    public static List<T> GetChildrenByType<T>(this UIElement element, Func<T, bool> condition) where T : UIElement {
      List<T> results = new List<T>();
      GetChildrenByType<T>(element, condition, results);
      return results;
    }

    private static void GetChildrenByType<T>(UIElement element, Func<T, bool> condition, List<T> results) where T : UIElement {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
        UIElement child = VisualTreeHelper.GetChild(element, i) as UIElement;
        if (child != null) {
          T t = child as T;
          if (t != null) {
            if (condition == null) {
              results.Add(t);
            } else if (condition(t)) {
              results.Add(t);
            }
          }
          GetChildrenByType<T>(child, condition, results);
        }
      }
    }

    public static bool HasChildrenByType<T>(this UIElement element, Func<T, bool> condition) where T : UIElement {
      return (element.GetChildrenByType<T>(condition).Count != 0);
    }

    public static IEnumerator<DataGridRow> GetRowsEnumerator(this DataGrid grid) {
      return new GridRowEnumerator(grid);
    }


  }

  public class GridRowEnumerator : IEnumerator<DataGridRow> {
    private DataGrid m_Grid;

    private IEnumerator m_Enumerator;

    public GridRowEnumerator(DataGrid grid) {
      m_Grid = grid;

      m_Enumerator = m_Grid.ItemsSource.GetEnumerator();
    }

    #region IEnumerator<DataGridRow> Members

    public DataGridRow Current {
      get {
        var rowItem = m_Enumerator.Current;

        // Ensures that all rows are loaded. 
        m_Grid.ScrollIntoView(rowItem, m_Grid.Columns.Last());

        // Get the content of the cell. 
        FrameworkElement el = m_Grid.Columns.Last().GetCellContent(rowItem);

        // Retrieve the row which is parent of given element. 
        //DataGridRow row = DataGridRow.GetRowContainingElement(el); 
        DataGridRow row = DataGridRow.GetRowContainingElement(el.Parent as FrameworkElement);

        return row;
      }
    }

    #endregion

    #region IDisposable Members

    public void Dispose() {

    }

    #endregion

    #region IEnumerator Members

    object IEnumerator.Current {
      get {
        return this.Current;
      }
    }

    public bool MoveNext() {
      return m_Enumerator.MoveNext();
    }

    public void Reset() {
      m_Enumerator.Reset();
    }

    #endregion
  }

  public class MyConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return "[" + (value as CollectionViewGroup).Items.Count + "]";
    }

    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  public class RHConverter : IValueConverter {
    private CJSGrid _owner = null;
    public RHConverter(CJSGrid owner) {
      this._owner = owner;
    }
    public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
      return "[" + value + "]";
    }

    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  #endregion

}
