using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Bio.Helpers.Common.Types;
using System.Windows.Data;
using System.Globalization;
using Bio.Helpers.Common;
using System.ComponentModel;
using Bio.Framework.Packets;
using System.Collections;
using Bio.Helpers.Controls.SL;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using PropertyMetadata = System.Windows.PropertyMetadata;

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
    internal JsonStoreClient _jsClient = null;

    public CJSGrid() {
      this.DefaultStyleKey = typeof(CJSGrid);
      this._multiselection = new VMultiSelection();
      this._jsClient = new JsonStoreClient();
      //this._jsClient.grid = this;
      this._jsClient.OnBeforeLoadData += this._doOnBeforeLoadData;
      this._jsClient.OnAfterLoadData += this._doOnAfterLoadData;
      this._jsClient.OnJsonStoreResponseSuccess += this._doOnJsonStoreResponseSuccess;
      this._jsClient.OnJsonStoreDSLoaded += this._doOnJsonStoreDSLoaded;
      this._jsClient.OnRowPropertyChanging += this._doOnRowPropertyChanging;
      this._jsClient.OnRowPropertyChanged += this._doOnRowPropertyChanged;
    }

    private void _doOnJsonStoreResponseSuccess(JsonStoreClient sender, AjaxResponseEventArgs args) {
      var v_rsp = (JsonStoreResponse) args.Response;
      if (v_rsp.packet.limit > 0) {
        var curPageI = ((v_rsp.packet.start + v_rsp.packet.rows.Count) / v_rsp.packet.limit);
        var curPageD = ((Double)(v_rsp.packet.start + v_rsp.packet.rows.Count) / (Double)v_rsp.packet.limit);
        if (curPageI < curPageD)
          this._curPage = (int)(curPageD + 1.0);
        else
          this._curPage = curPageI;
        if (this._curPage < 1) this._curPage = 1;
        if (v_rsp.packet.endReached && (v_rsp.packet.rows.Count > 0)) {
          this._lastPage = this._curPage;
          this._lastPageDetected = this._lastPage;
        } else {
          this._lastPage = this._curPage + 1;
        }
      }
      if (this._dataGrid != null)
        this._dataGrid.ItemsSource = null;
    }

    public String GroupDefinition { get; set; }

    private PagedCollectionView _pcv;
    private IEnumerable _creDSGrp(IEnumerable ds, CJsonStoreMetadata metadata) {
      this._pcv = new PagedCollectionView(ds);
      var flds = Utl.SplitString(this.GroupDefinition, new Char[] { ',', ';', ' ' });
      foreach (var fldName in flds) {
        var v_indx = metadata.indexOf(fldName);
        if (v_indx == -1)
          throw new EBioException("Группировка по полю {0} не возможна, т.к. поле не найдено в метаданных.");
        var gd = new PropertyGroupDescription();
        gd.PropertyName = metadata.fields[v_indx].name;
        gd.StringComparison = StringComparison.CurrentCultureIgnoreCase;
        gd.Converter = new GroupHeaderFormatter(gd);
        this._pcv.GroupDescriptions.Add(gd);
      }
      return this._pcv;
    }

    private void _initGroupDef(CJsonStoreMetadata md) {
      if (!String.IsNullOrEmpty(this.GroupDefinition))
        return;
      var groups = (new[] { new { indx = 0, field = "" } }).ToList();
      groups.Clear();
      foreach (var fld in md.fields) {
        if (fld.group > 0) {
          groups.Add(new { indx = fld.group, field = fld.name });
        }
      }
      if (groups.Count > 0) {
        groups.Sort((a, b) => {
          if (a.indx == b.indx)
            return 0;
          if (a.indx > b.indx)
            return 1;
          return -1;
        });
        String grpDef = null;
        foreach (var f in groups)
          Utl.AppendStr(ref grpDef, f.field, ";");
        this.GroupDefinition = grpDef;
      }
    }

    private void _doOnJsonStoreDSLoaded(JsonStoreClient sender, JsonStoreDSLoadedEventArgs args) {
      var rsp = (JsonStoreResponse)args.Response;
      var rq = (JsonStoreRequest)args.Request;
      if (this._dataGrid != null) {
        this._initGroupDef(this._jsClient.jsMetadata);
        this._dataGrid.disableColumnsChangedEvents();
        if ((rsp.packet.limit == 0) && (!String.IsNullOrEmpty(this.GroupDefinition)))
          this._dataGrid.ItemsSource = this._creDSGrp(args.DS, this.jsMetadata);
        else
          this._dataGrid.ItemsSource = args.DS;

        this._dataGrid.enableColumnsChangedEvents();
      }

      if (rq.Packet.locate != null) {
        this._lastLocatedRow = this._locateInternal(rq.Packet.locate);
        this.SelectedItem = this._lastLocatedRow;
      } else
        this.SelectedIndex = this._curRowIndex;
      this.CurrentColumnIndex = this._curColumnIndex;
    }

    public event JSClientEventHandler<JSClientRowPropertyChangingEventArgs> OnRowPropertyChanging;
    public event JSClientEventHandler<JSClientRowPropertyChangedEventArgs> OnRowPropertyChanged;

    private void _doOnRowPropertyChanging(JsonStoreClient sender, JSClientRowPropertyChangingEventArgs args) {
      var eve = this.OnRowPropertyChanging;
      if (eve != null)
        eve(sender, args);
    }
    private void _doOnRowPropertyChanged(JsonStoreClient sender, JSClientRowPropertyChangedEventArgs args) {
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

    private CDataGrid _dataGrid;

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

    internal void updPosState() {
      var tbx = this.GetTemplateChild("tbxPagePos") as TextBox;
      if (tbx != null)
        tbx.Text = "" + this.PageCurrent;
      var tblck = this.GetTemplateChild("tbxLastPage") as TextBlock;
      if (tblck != null)
        tblck.Text = ((this.PageLast == -1) ? "?" : "" + this.PageLast);
    }

    private GridLength fPanelNavRowHeight = new GridLength(0);

    private void _updNavPanelVisibility() {
      var pnl = this.GetTemplateChild("navPanel") as StackPanel;
      if (pnl != null)
        pnl.Visibility = this.PageSize == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void _updAutoRefreshPanelVisibility() {
      var pnl = this.GetTemplateChild("grdAutoRefresh") as Grid;
      if (pnl != null) 
        pnl.Visibility = this.AutoRefreshEnabled ? Visibility.Visible : Visibility.Collapsed;
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
      if (((CheckBox)sender).IsChecked == true)
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


    private const Int32 ciLastPageUnassigned = -1;
    private Int64 _curPage = 1;
    public Int64 PageCurrent {
      get {
        return this._curPage;
      }
    }

    /// <summary>
    /// Это последняя страница, когда достигнута последняя страница
    /// </summary>
    private Int64 _lastPageDetected = ciLastPageUnassigned;
    /// <summary>
    /// Последняя страница пока не достигнута последняя страница
    /// </summary>
    private Int64 _lastPage = ciLastPageUnassigned;
    public Int64 PageLast {
      get {
        return this._lastPageDetected;
      }
    }

    public void goPageFirst(AjaxRequestDelegate callback) {
      if (this._jsClient.PageSize == this.PageSize) {
        if (this._curPage > 1) 
          this._goto(0, "к первой странице...", callback, null);
        else {
          if (callback != null)
            callback(null, null);
        }
      } else
        this.Refresh(callback);
    }
    public void goPagePrev(AjaxRequestDelegate callback) {
      if (this._jsClient.PageSize == this.PageSize) {
        if (this._curPage > 1) {
          this._curPage--;
          var startFrom = (this._curPage - 1) * this.PageSize;
          this._goto(startFrom, "к пред. странице...", callback, null);
        } else {
          if (callback != null)
            callback(null, null);
        }
      } else
        this.Refresh(callback);
    }
    public void goPageNext(AjaxRequestDelegate callback) {
      if (this._jsClient.PageSize == this.PageSize) {
        if (this._curPage < this._lastPage) {
          this._curPage++;
          var startFrom = (this._curPage - 1) * this.PageSize;
          this._goto(startFrom, "к след. странице...", callback, null);
        } else {
          if (callback != null)
            callback(null, null);
        }
      } else
        this.Refresh(callback);
    }
    public void goPageLast(AjaxRequestDelegate callback) {
      if (this._jsClient.PageSize == this.PageSize) {
        if (this._curPage < this._lastPage) {
          this._goto(Int64.MaxValue, "к последней странице...", callback, null);
        } else {
          if (callback != null)
            callback(null, null);
        }
      } else
        this.Refresh(callback);
    }
    public void Refresh(AjaxRequestDelegate callback) {
      this._setBtnVisibility("btnRefreshFirst", Visibility.Collapsed);
      this._goto(null, "Обновление...", callback, null);
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
          this._exporter = new CExpClient(this.ajaxMng, this.BioCode, this.Title ?? "Экспорт");
        } else
          this._exporter.bioCode = this.BioCode;
        this._exporter.title = this.Title;
        this.BioParams.SetValue("dataGridOnClientHeaders", this._getColumnHeaderList());
        this._exporter.RunProc(this.BioParams, null);

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
      var v_rslt = v_prnt_typeName + "-[" + this.BioCode + "]";
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

    private void _invertMultySelection() { 
      foreach(CRTObject r in this.SelectedItems){
        if (!this.Selection.CheckSelected(r))
          this.Selection.AddSelected(r);
        else
          this.Selection.RemoveSelected(r);
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
      if (!e.Handled) {
          if (e.Key == Key.Space) {
            this._invertMultySelection();
          }
      }
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

    public Int64 PageSize {
      get {
        return (this.SuppressPaging) ? 0 : (((this._cfg != null) && (this._cfg.pageSize != null)) ? (Int64)this._cfg.pageSize : this.DefaultPageSize);
      }
    }

    public String BioCode {
      get {
        return this._jsClient.BioCode;
      }
      set {
        var oldBioCode = this._jsClient.BioCode;
        this._jsClient.BioCode = value;
        if (!String.Equals(oldBioCode, this._jsClient.BioCode))
          this._restoreCfg();
      }
    }
    public IAjaxMng ajaxMng {
      get {
        return this._jsClient.AjaxMng;
      }
      set {
        this._jsClient.AjaxMng = value;
      }
    }
    public Params BioParams {
      get {
        return this._jsClient.BioParams;
      }
      set {
        this._jsClient.BioParams = value;
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

    /// <summary>
    /// Переводит курсор на первую запись в текущем DS на клиенте
    /// </summary>
    public void SelectFirstLocal() {
      this._callOnShowDelaied(() => {
        this.SelectedItem = this._jsClient.DS.FirstOrDefault();
      });
    }

    /// <summary>
    /// Переводит запись на следующую строку в текущем DS на клиенте
    /// </summary>
    /// <returns>true если изменилась</returns>
    public Boolean SelectNextLocal() {
      if (this.SelectedItem == null) {
        this.SelectFirstLocal();
        return true;
      }

      var v_storeSelectedIndex = this.SelectedIndex;
      this.SelectedIndex++;
      return v_storeSelectedIndex < this.SelectedIndex;
    }

    /// <summary>
    /// Переводит запись на предыдущую строку в текущем DS на клиенте
    /// </summary>
    /// <returns>true если изменилась</returns>
    public Boolean SelectPrivLocal() {
      if (this.SelectedItem == null) {
        this.SelectFirstLocal();
        return true;
      }

      var v_storeSelectedIndex = this.SelectedIndex;
      this.SelectedIndex--;
      return v_storeSelectedIndex > this.SelectedIndex;
    }

    /// <summary>
    /// Возвращает следующую строку по отношению к выбранной в текущем DS на клиенте
    /// </summary>
    /// <returns>null, если курсор стоит на последней или невыбрана ни одна строка</returns>
    public CRTObject GetNextLocal() {
      if (this.SelectedItem == null)
        return null;
      return this._jsClient.DS.ElementAtOrDefault(this.SelectedIndex + 1);
    }

    /// <summary>
    /// Возвращает предыдущую строку по отношению к выбранной в текущем DS на клиенте
    /// </summary>
    /// <returns>null, если курсор стоит на первой или невыбрана ни одна строка</returns>
    public CRTObject GetPrivLocal() {
      if (this.SelectedItem == null)
        return null;
      return this._jsClient.DS.ElementAtOrDefault(this.SelectedIndex - 1);
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
        if ((this._dataGrid != null) && (this._dataGrid.SelectedItem != null) && (this._jsClient.DS.Count() > 0)) {
          Utl.UiThreadInvoke(() => {
            var v_col = this._dataGrid.Columns.Where((c) => { return c.DisplayIndex == this._currentColumnIndex; }).FirstOrDefault();
            if (v_col != null) {
              this._dataGrid.CurrentColumn = v_col;
              this._dataGrid.ScrollIntoView(this._dataGrid.SelectedItem, this._dataGrid.CurrentColumn);
            }
          });
        }
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
        return null;
      }
    }

    public static String GetDataGridColumnName(DataGridColumn column) {
      DataGridBoundColumn col = column as DataGridBoundColumn;
      if ((col.Binding != null) && (col.Binding.Path != null))
        return col.Binding.Path.Path;
      return null;
    }

    public DataGridColumn ColumnByBindingPath(String bindingPath) {
      if (this.Columns != null)
        return this.Columns.Where((c) => { return String.Equals(GetDataGridColumnName(c), bindingPath, StringComparison.CurrentCultureIgnoreCase); }).FirstOrDefault();
      return null;
    }

    public String CurrentColumnName {
      get {
        if (this._dataGrid != null) 
          return GetDataGridColumnName(this._dataGrid.CurrentColumn);
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
      public Params BioParams { get; set; }
      public T Callback { get; set; }
      public CJsonStoreFilter Locate { get; set; }
    };
    private LoadParams<AjaxRequestDelegate> _suspendLoadParams;
    private Boolean _isFirstLoading = true;
    public void Load(Params bioParams, AjaxRequestDelegate callback, CJsonStoreFilter locate) {
      if (this.SuspendFirstLoad && this._isFirstLoading) {
        this._suspendLoadParams = new LoadParams<AjaxRequestDelegate> {
          BioParams = (bioParams != null) ? (Params)bioParams.Clone() : null,
          Callback = callback,
          Locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
        };
        this._setBtnVisibility("btnRefreshFirst", Visibility.Visible);
        this._isFirstLoading = false;
      } else {
        this._setBtnVisibility("btnRefreshFirst", Visibility.Collapsed);
        var loadParams = this._suspendLoadParams ?? new LoadParams<AjaxRequestDelegate> {
          BioParams = (bioParams != null) ? (Params)bioParams.Clone() : null,
          Callback = callback,
          Locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
        };
        this._suspendLoadParams = null;
        this._callOnShowDelaied(() => this._goto((this.PageCurrent - 1)*this.PageSize, "Загрузка...",
                                                 (sndr, a) => {
                                                   this.hideBusyIndicator();

                                                   if (a.Response.Success) {
                                                     var rsp = (a.Response as JsonStoreResponse);
                                                     if (rsp != null) {
                                                       if (this._dataGrid != null)
                                                         this._dataGrid.UpdateLayout();
                                                       this.updPosState();
                                                       if (loadParams.Callback != null)
                                                         loadParams.Callback(sndr, a);
                                                     }
                                                   }
                                                 }, loadParams.Locate));
      }
    }

    private void _doOnLocation(CRTObject row, EBioException exception, EventHandler<OnSelectEventArgs> callback) {
      if (row != null) {
          this.SelectedItem = row;
      } else {
        if (this._dataGrid.ItemsSource != null) {
          var venumr = this._dataGrid.ItemsSource.GetEnumerator();
          if (venumr != null) {
            venumr.Reset();
            if (venumr.MoveNext()) {
              this.SelectedItem = venumr.Current as CRTObject;
              row = this.SelectedItem;
            }
          }
        }
      }
      if (callback != null) {
        callback(this, new OnSelectEventArgs {
          ex = exception,
          selection = new VSingleSelection { ValueRow = row }
        });
      }
    }

    public event EventHandler<CancelEventArgs> OnBeforeLoadData;
    private void _doOnBeforeLoadData(JsonStoreClient sender, CancelEventArgs args) {
      this.DataIsLoaded = false;
      if (this.OnBeforeLoadData != null) {
        //Utl.UiThreadInvoke(() => {
          this.OnBeforeLoadData(this, args);
        //});
      }
    }

    public bool DataIsLoaded { get; private set; }

    private JsonStoreClient _forwardSender;
    private AjaxResponseEventArgs _forwardArgs;
    void _dataGrid_LayoutUpdated(object sender, EventArgs e) {
      this._dataGrid.LayoutUpdated -= this._dataGrid_LayoutUpdated;
      this._prepareGridAfterLoadData();
    }

    private void _prepareGridAfterLoadData() {
      this._applyCfgToGrid();
      var handler = this.OnAfterLoadData;
      if (handler != null) {
        handler(this._forwardSender, this._forwardArgs);
      }
    }


    public event JSClientEventHandler<AjaxResponseEventArgs> OnAfterLoadData;
    void _doOnAfterLoadData(JsonStoreClient sender, AjaxResponseEventArgs e) {
      // Все эти "танцы с бубном", чтобы на событии "OnAfterLoadData" грид уже содержал колонки загруженных данных
      this._forwardSender = sender;
      this._forwardArgs = e;
      if (this._dataGrid != null) {
        this._dataGrid.LayoutUpdated += this._dataGrid_LayoutUpdated;
      } else {
        this._prepareGridAfterLoadData();
      }
      this.DataIsLoaded = true;
    }

    private Int32 _curRowIndex = -1;
    private Int32 _curColumnIndex = -1;
    private void _goto(Int64? startFrom, String waitMsg, AjaxRequestDelegate callback, CJsonStoreFilter locate) {
      this.showBusyIndicator(waitMsg);
      try {
        this._curColumnIndex = this.CurrentColumnIndex;
        this._curRowIndex = this.SelectedIndex;
        this._jsClient.Load(null, (s, a) => {
          this.hideBusyIndicator();
          this.updPosState();
          if (callback != null)
            callback(s, a);
        }, locate, this.PageSize, startFrom);
      } catch {
        this.hideBusyIndicator();
        throw;
      }
    }

    private static Boolean _checkFilter(CRTObject row, Int64 rowPos, CJsonStoreFilter filter) {
      if ((row != null) && (filter != null)) {
        return rowPos >= filter.fromPosition && filter.check(row);
      }
      return false;
    }

    private CRTObject _locateInternal(CJsonStoreFilter locate) {
      if (this.ItemsSource != null) {
        var rows = this.ItemsSource.Cast<CRTObject>().ToArray();
        for (var i = 0; i < rows.Length; i++) {
          var rowPos = i + (this.PageCurrent * this.PageSize);
          if (_checkFilter(rows[i], rowPos, locate)) {
            locate.fromPosition = rowPos + 1;
            return rows[i];
          }
        }
      }
      return null;
    }

    private CRTObject _lastLocatedRow = null;
    private void _locate(CJsonStoreFilter locate, EventHandler<OnSelectEventArgs> callback, Boolean forceRemote = false) {
      CRTObject v_row = null;
      if(!forceRemote)
        v_row = this._locateInternal(locate);
      if (v_row != null)
        this._doOnLocation(v_row, null, callback);
      else {
        this._goto(0, "Поиск...", (s, a) => {
          if (a.Response.Success)
            this._doOnLocation(this._lastLocatedRow, null, callback);
          else
            this._doOnLocation(null, a.Response.Ex, callback);

        }, locate);
      }
    }

    private LoadParams<EventHandler<OnSelectEventArgs>> _suspendLocateParams = null;
    public void Locate(CJsonStoreFilter locate, EventHandler<OnSelectEventArgs> callback, Boolean forceRemote) {
      this._callOnShowDelaied(() => {
        if (this.SuspendFirstLoad && this._isFirstLoading) {
          this._suspendLocateParams = new LoadParams<EventHandler<OnSelectEventArgs>> {
            BioParams = null,
            Callback = callback,
            Locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
          };
          this._setBtnVisibility("btnRefreshFirst", Visibility.Visible);
          this._isFirstLoading = false;
        } else {
          this._setBtnVisibility("btnRefreshFirst", Visibility.Collapsed);
          var locateParams = this._suspendLocateParams ?? new LoadParams<EventHandler<OnSelectEventArgs>> {
            BioParams = null,
            Callback = callback,
            Locate = (locate != null) ? (CJsonStoreFilter)locate.Clone() : null
          };
          this._suspendLocateParams = null;
          this._locate(locateParams.Locate, locateParams.Callback, forceRemote);
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
    public void SaveData(AjaxRequestDelegate callback, String trunsactionID, SQLTransactionCmd cmd) {
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
      this.SaveData(callback, null, SQLTransactionCmd.Nop);
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
        this._jsClient.Load((s, a) => {
          this._autorefreshing = false;
        });
      }
    }
    public void AutoRefreshStop() {
      if (this._autoRefreshTimer != null)
        this._autoRefreshTimer.Stop();
    }
    public void LoadSelectedPks(AjaxRequestDelegate callback) {
      var v_selection = this.Selection.ToString();
      if (String.IsNullOrEmpty(v_selection)) {
        if (callback != null) {
          callback(this, new AjaxResponseEventArgs());
        } else
          return;
      }
      this._jsClient.LoadSelectedPks(callback, v_selection);
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
      if (this.DataIsLoaded)
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
      var results = new List<T>();
      _getChildrenByType<T>(element, condition, results);
      return results;
    }

    private static void _getChildrenByType<T>(UIElement element, Func<T, bool> condition, List<T> results) where T : UIElement {
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++) {
        var child = VisualTreeHelper.GetChild(element, i) as UIElement;
        if (child != null) {
          T t = child as T;
          if (t != null) {
            if (condition == null) {
              results.Add(t);
            } else if (condition(t)) {
              results.Add(t);
            }
          }
          _getChildrenByType<T>(child, condition, results);
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
    private DataGrid _grid;

    private IEnumerator _enumerator;

    public GridRowEnumerator(DataGrid grid) {
      _grid = grid;

      _enumerator = _grid.ItemsSource.GetEnumerator();
    }

    #region IEnumerator<DataGridRow> Members

    public DataGridRow Current {
      get {
        var rowItem = _enumerator.Current;

        // Ensures that all rows are loaded. 
        _grid.ScrollIntoView(rowItem, _grid.Columns.Last());

        // Get the content of the cell. 
        FrameworkElement el = _grid.Columns.Last().GetCellContent(rowItem);

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
      return _enumerator.MoveNext();
    }

    public void Reset() {
      _enumerator.Reset();
    }

    #endregion
  }

  public class MyConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return "[" + ((CollectionViewGroup) value).Items.Count + "]";
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

  public class CurrFormatter : IValueConverter {
    private CJsonStoreMetadataFieldDef _fldDef;
    private DataGridColumn _column;
    public CurrFormatter(CJsonStoreMetadataFieldDef fldDef, DataGridColumn col) {
      this._fldDef = fldDef;
      this._column = col;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (!String.IsNullOrEmpty(this._fldDef.format)) {
        if (value is Decimal) {
          return ((Decimal)value).ToString(this._fldDef.format);
        }
        if (value is Double) {
          return ((Double)value).ToString(this._fldDef.format);
        }
        if (value is Int64) {
          return ((Int64)value).ToString(this._fldDef.format);
        }
        if ((value is DateTime) || (value is DateTime?) || (value is DateTimeOffset) || (value is DateTimeOffset?)) {
          return ((DateTime)value).ToString(this._fldDef.format);
        }
        return value;
      }
      return value;
    }

    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
      try {
        return Utl.Convert2Type(value, targetType);
      } catch (Exception) {
        if (Utl.TypeIsNumeric(targetType))
          return 0;
        if (targetType == typeof(DateTime))
          return DateTime.MinValue;
        return null;
      }
    }
  }

  public class GroupHeaderFormatter : IValueConverter {
    private PropertyGroupDescription _grpDescr;
    public GroupHeaderFormatter(PropertyGroupDescription grpDescr) {
      this._grpDescr = grpDescr;
    }
    public object Convert(Object value, Type targetType, Object parameter, CultureInfo culture) {
      return value;
    }

    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }

  #endregion

}
