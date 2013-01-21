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
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using System.Threading;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Threading;
using System.Runtime.CompilerServices;

namespace Bio.Framework.Client.SL {
  public class JSClientBeforeMonRowEventArgs : CancelEventArgs {
    public CRTObject Row { get; private set; }
    public JSClientBeforeMonRowEventArgs(Boolean cancel, CRTObject row)
      : base(cancel) {
      this.Row = row;
    }
  }
  public class JSClientAfterMonRowEventArgs : EventArgs {
    public CRTObject Row { get; private set; }
    public JSClientAfterMonRowEventArgs(CRTObject row)
      : base() {
      this.Row = row;
    }
  }
  public class JSClientRowPropertyChangingEventArgs : CancelEventArgs {
    public CRTObject Row { get; private set; }
    public String PropertyName { get; private set; }
    public JSClientRowPropertyChangingEventArgs(Boolean cancel, CRTObject row, String propertyName)
      : base(cancel) {
      this.Row = row;
      this.PropertyName = propertyName;
    }
  }
  public class JSClientRowPropertyChangedEventArgs : EventArgs {
    public CRTObject Row { get; private set; }
    public String PropertyName { get; private set; }
    public JSClientRowPropertyChangedEventArgs(CRTObject row, String propertyName)
      : base() {
      this.Row = row;
      this.PropertyName = propertyName;
    }
  }

  public delegate void JSClientEventHandler<T>(CJsonStoreClient sender, T args) where T : EventArgs;

  public class CJSChangedRow {
    public CJSChangedRow(Int32 index, CJsonStoreRowChangeType state, CRTObject newRow, CRTObject origRow)
      : base() {
      this.Index = index;
      this.State = state;
      this.OrigRow = origRow;
      this.CurRow = newRow;
    }
    public Int32 Index { get; private set; }
    public CJsonStoreRowChangeType State { get; private set; }
    public CRTObject OrigRow { get; private set; }
    public CRTObject CurRow { get; private set; }
    public void markUnchanged() {
      this.State = CJsonStoreRowChangeType.Unchanged;
    }
  }

  public class CJsonStoreClient {

    private CJSGrid _grid { get; set; }

    public event JSClientEventHandler<JSClientBeforeMonRowEventArgs> OnBeforeInsertRow;
    public event JSClientEventHandler<JSClientAfterMonRowEventArgs> OnAfterInsertRow;
    public event JSClientEventHandler<JSClientBeforeMonRowEventArgs> OnBeforeDeleteRow;
    public event JSClientEventHandler<JSClientAfterMonRowEventArgs> OnAfterDeleteRow;

    public event JSClientEventHandler<JSClientRowPropertyChangingEventArgs> OnRowPropertyChanging;
    public event JSClientEventHandler<JSClientRowPropertyChangedEventArgs> OnRowPropertyChanged;

    public IAjaxMng ajaxMng { get; set; }
    public String bioCode { get; set; }
    private CParams _bioParams = null;
    public CParams bioParams {
      get {
        if (this._bioParams == null)
          this._bioParams = new CParams();
        return this._bioParams;
      }
      set {
        this._bioParams = value;
      }
    }
    private Int64 _pageSize = 0L;
    public Int64 pageSize {
      get {
        return this._pageSize;
      }
    }

    public void setPageSize(Int64 pageSize) {
      this._pageSize = pageSize;
    }

    private const String csInternalROWUID_FieldName = "InternalROWUID";
    private List<CPropertyMetadata> _genPropertyDefs(List<CJsonStoreMetadataFieldDef> fieldDefs) {
      List<CPropertyMetadata> rslt = new List<CPropertyMetadata>();
      rslt.Add(new CPropertyMetadata {
        Name = csInternalROWUID_FieldName,
        DisplayName = String.Empty,
        Type = typeof(String),
        Group = String.Empty,
        Hidden = true
      });
      foreach (var fieldDef in fieldDefs)
        rslt.Add(new CPropertyMetadata {
          Name = fieldDef.name,
          DisplayName = fieldDef.header,
          Type = fieldDef.GetDotNetType(),
          Group = (fieldDef.group > 0) ? "Group-" + fieldDef.group : String.Empty,
          Hidden = fieldDef.hidden
        });
      return rslt;
    }

    CTypeFactory _typeFactory = new CTypeFactory();
    private Type _creRowType(List<CJsonStoreMetadataFieldDef> fieldDefs) {
      List<CPropertyMetadata> v_propDefs = this._genPropertyDefs(fieldDefs);
      return this._typeFactory.CreateType(v_propDefs);
    }

    private IEnumerable _creDSInst(Type rowType) {
      IEnumerable rslt = null;
      if (this.pageSize > 0) {
        var listType = typeof(SortableCollectionView<>).MakeGenericType(new[] { rowType });
        rslt = Activator.CreateInstance(listType, new EventHandler<RefreshEventArgs>(this._doOnDSRequestRefresh)) as IEnumerable;
      } else {
        var listType = typeof(ObservableCollection<>).MakeGenericType(new[] { rowType });
        rslt = Activator.CreateInstance(listType) as IEnumerable;
      }
      return rslt;
    }

    private void _doOnDSRequestRefresh(object sender, RefreshEventArgs e) {
      this._goto(null, "Сортировка...", null, null);
    }

    private CJsonStoreMetadata _metadata = null;
    public Boolean readOnly { get { return (this._metadata != null) ? this._metadata.readOnly : true; } }
    private List<CJSChangedRow> _ds_changes = null;

    private IEnumerable _ds = null;
    public IEnumerable ds0 { get { return this._ds; } }
    public IEnumerable<CRTObject> DS {
      get {
        return (this._ds != null) ? this._ds.Cast<CRTObject>() : null;
      }
    }

    public List<CJSChangedRow> Changes { get { return this._ds_changes; } }
    public String groupDefinition { get; set; }

    public CJSGrid grid {
      get {
        return this._grid;
      }
      set {
        if (this._grid != value) {
          this._grid = value;
          if ((this._grid != null) && (this._grid._dataGrid != null)) {
            this._grid._dataGrid.AutoGenerateColumns = true;
          }
        }
      }
    }

    public CJsonStoreClient() {
      this._ds_changes = new List<CJSChangedRow>();
    }

    private List<CJsonStoreMetadataFieldDef> _creFldDefsRmt(CJsonStoreMetadata metaData) {
      List<CJsonStoreMetadataFieldDef> flds = new List<CJsonStoreMetadataFieldDef>();
      foreach (CJsonStoreMetadataFieldDef fld in metaData.fields)
        flds.Add(fld);
      return flds;
    }

    private void _regChanges(CJSChangedRow chng) {
      var v_exists = this._ds_changes.FirstOrDefault((c) => {
        return c.CurRow.Equals(chng.CurRow);
      });
      if (chng.State == CJsonStoreRowChangeType.Added) {
        if (v_exists == null)
          this._ds_changes.Add(chng);
        else {
          if ((v_exists.State == CJsonStoreRowChangeType.Deleted) ||
                (v_exists.State == CJsonStoreRowChangeType.Modified)) {
            this._ds_changes.Remove(v_exists);
            this._ds_changes.Add(chng);
          }
        }
      } else if (chng.State == CJsonStoreRowChangeType.Deleted) {
        if (v_exists == null)
          this._ds_changes.Add(chng);
        else {
          if (v_exists.State == CJsonStoreRowChangeType.Added) {
            this._ds_changes.Remove(v_exists);
          }
          if (v_exists.State == CJsonStoreRowChangeType.Modified) {
            this._ds_changes.Remove(v_exists);
            this._ds_changes.Add(chng);
          }
        }
      } else if (chng.State == CJsonStoreRowChangeType.Modified) {
        if (v_exists == null)
          this._ds_changes.Add(chng);
      }
    }

    private Boolean _rowMonEventsDisabled = false;
    private void _doOnBeforeInsertRow(ref Boolean cancel, CRTObject row) {
      if (!this._rowMonEventsDisabled) {
        var eve = this.OnBeforeInsertRow;
        if (eve != null) {
          var args = new JSClientBeforeMonRowEventArgs(cancel, row);
          eve(this, args);
          cancel = args.Cancel;
        }
      }
    }

    public Int32 indexOf(CRTObject row) {
      var foundItem = this.DS.Select((item, index) => new {
        Item = item,
        Position = index
      }).Where(i => i.Item == row).FirstOrDefault();
      return (foundItem != null) ? foundItem.Position : -1;
    }

    private void _doOnAfterInsertRow(CRTObject newRow) {
      if (!this._rowMonEventsDisabled) {
        var v_indx = this.indexOf(newRow);
        this._regChanges(new CJSChangedRow(v_indx, CJsonStoreRowChangeType.Added, newRow, null));
        var eve = this.OnAfterInsertRow;
        if (eve != null) {
          var args = new JSClientAfterMonRowEventArgs(newRow);
          eve(this, args);
        }
      }
    }

    public void ForceAllRowsChanged() {
      this._clearChanges();
      foreach (var r in this.DS) {
        var v_index = this.indexOf(r);
        this._regChanges(new CJSChangedRow(v_index, CJsonStoreRowChangeType.Added, r, null));
      }
    }

    private void _doOnBeforeDeleteRow(ref Boolean cancel, CRTObject row) {
      if (!this._rowMonEventsDisabled) {
        var eve = this.OnBeforeDeleteRow;
        if (eve != null) {
          var args = new JSClientBeforeMonRowEventArgs(cancel, row);
          eve(this, args);
          cancel = args.Cancel;
        }
      }
    }
    private void _doOnAfterDeleteRow(Int32 index, CRTObject row) {
      if (!this._rowMonEventsDisabled) {
        this._regChanges(new CJSChangedRow(index, CJsonStoreRowChangeType.Deleted, row, row));
        var eve = this.OnAfterDeleteRow;
        if (eve != null) {
          var args = new JSClientAfterMonRowEventArgs(row);
          eve(this, args);
        }
      }
    }

    public void InsertRow(Int32 index, CRTObject row) {
      var v_ds = this._ds as IList;
      if (v_ds != null) {
        Boolean v_cancel = false;
        this._doOnBeforeInsertRow(ref v_cancel, row);
        if (!v_cancel) {
          if (index >= 0)
            v_ds.Insert(index, row);
          else
            v_ds.Add(row);
          this._doOnAfterInsertRow(row);
        }

      }
    }
    public CRTObject InsertRow(Int32 index) {
      var row = this.NewRow();
      this.InsertRow(index, row);
      return row;
    }
    public void AddRow(CRTObject row) {
      this.InsertRow(-1, row);
    }
    public CRTObject AddRow() {
      return this.InsertRow(-1);
    }

    private CRTObject _lastPreChangedRow = null;
    private void _doOnRowPropertyChanging(Object sender, PropertyChangingEventArgs e) {
      this._lastPreChangedRow = (sender as CRTObject).Copy();
      var eve = this.OnRowPropertyChanging;
      if (eve != null) {
        var ee = new JSClientRowPropertyChangingEventArgs(false, this._lastPreChangedRow, e.PropertyName);
        eve(this, ee);
      }
    }
    private void _doOnRowPropertyChanged(Object sender, PropertyChangedEventArgs e) {
      var v_indx = this.indexOf(this._lastPreChangedRow);
      this._regChanges(new CJSChangedRow(v_indx, CJsonStoreRowChangeType.Modified, (CRTObject)sender, this._lastPreChangedRow));
      var eve = this.OnRowPropertyChanged;
      if (eve != null) {
        var ee = new JSClientRowPropertyChangedEventArgs(this._lastPreChangedRow, e.PropertyName);
        eve(this, ee);
      }
    }


    public CRTObject NewRow() {
      if (this._lastRowType != null) {
        var row = CTypeFactory.CreateInstance(this._lastRowType, this._doOnRowPropertyChanging, this._doOnRowPropertyChanged);
        row.DisableEvents();
        try {
          String v_intRowUID = Guid.NewGuid().ToString("N");
          this._setFieldValue(row, csInternalROWUID_FieldName, v_intRowUID);
        } finally {
          row.EnableEvents();
        }
        return row;
      } else
        return null;
    }
    public void RemoveRowAt(Int32 index) {
      var v_ds = this._ds as IList;
      if (v_ds != null) {
        if ((index >= 0) && (index < v_ds.Count)) {
          var row = v_ds[index] as CRTObject;
          this.RemoveRow(row);
        }
      }
    }
    public void RemoveRow(CRTObject row) {
      if (row != null) {
        var v_ds = this._ds as IList;
        if (v_ds != null) {
          Boolean v_cancel = false;
          this._doOnBeforeDeleteRow(ref v_cancel, row);
          if (!v_cancel) {
            var v_indx = this.indexOf(row);
            v_ds.Remove(row);
            this._doOnAfterDeleteRow(v_indx, row);
          }
        }
      }
    }

    private void _setFieldValue(CRTObject row, String fldName, Object value) {
      if (row != null) {
        row[fldName] = value;
      }
    }

    public Type RowType { get { return this._lastRowType; } }
    private Type _lastRowType = null;
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void _loadDS(Type rowType, CJsonStoreResponse jsRsp) {
      this._rowMonEventsDisabled = true;
      try {
        CJsonStoreResponse v_jsRsp = jsRsp;
        if ((v_jsRsp != null) && (v_jsRsp.packet != null) &&
                (v_jsRsp.packet.metaData != null) && (v_jsRsp.packet.rows != null)) {
          if (rowType != null)
            this._lastRowType = rowType;
          Type v_lastRowType = this._lastRowType;
          if (v_lastRowType != null) {
            this.ClearData();
            Int64 vRowNum = v_jsRsp.packet.start;
            if (v_lastRowType != null) {
              foreach (var r in v_jsRsp.packet.rows) {
                var row = this.AddRow();
                row.DisableEvents();
                try {
                  foreach (CJsonStoreMetadataFieldDef fld in v_jsRsp.packet.metaData.fields) {
                    var value = r.Values[v_jsRsp.packet.metaData.indexOf(fld.name)];
                    this._setFieldValue(row, fld.name, value);
                  }
                } finally {
                  row.EnableEvents();
                }
              }
            }
          }
        }
      } finally {
        this._rowMonEventsDisabled = false;
      }
    }

    public void ClearData() {
      IList v_ds = this._ds as IList;
      if (v_ds != null) {
        v_ds.Clear();
      }
      this._clearChanges();
    }

    private const Int32 ciLastPageUnassigned = -1;
    /// <summary>
    /// Это последняя страница, когда достигнута последняя страница
    /// </summary>
    private Int64 _lastPageDetected = ciLastPageUnassigned;
    /// <summary>
    /// Последняя страница пока не достигнута последняя страница
    /// </summary>
    private Int64 _lastPage = ciLastPageUnassigned;
    /// <summary>
    /// Текущая страница
    /// </summary>
    private Int64 _curPage = 1;
    public String pagePositionDesc {
      get {
        if (this.pageSize > 0) {
          String v_lstPageNum = (this._lastPageDetected == ciLastPageUnassigned) ? "?" : "" + this._lastPageDetected;
          return String.Format("Страница {0} из {1} (по {2} записей)", this._curPage, v_lstPageNum, this.pageSize);
        } else
          return null;
      }
    }
    public Int64 pageCurrent {
      get {
        return this._curPage;
      }
    }
    public Int64 pageLast {
      get {
        return this._lastPageDetected;
      }
    }

    private PagedCollectionView _pcv = null;
    private IEnumerable _creDSGrp(IEnumerable ds, CJsonStoreMetadata metadata) {
      this._pcv = new PagedCollectionView(ds);
      String[] flds = Utl.SplitString(this.groupDefinition, new Char[] { ',', ';', ' ' });
      foreach (String fldName in flds) {
        Int32 v_indx = metadata.indexOf(fldName);
        if (v_indx == -1)
          throw new EBioException("Группировка по полю {0} не возможна, т.к. поле не найдено в метаданных.");
        PropertyGroupDescription gd = new PropertyGroupDescription();
        gd.PropertyName = metadata.fields[v_indx].name;
        gd.StringComparison = StringComparison.CurrentCultureIgnoreCase;
        gd.Converter = new GroupHeaderFormatter(gd);
        this._pcv.GroupDescriptions.Add(gd);
      }
      return this._pcv;
    }

    private void _initGroupDef(CJsonStoreMetadata md) {
      if (!String.IsNullOrEmpty(this.groupDefinition))
        return;
      var v_groups = (new[] { new { indx = 0, field = "" } }).ToList();
      v_groups.Clear();
      foreach (var fld in md.fields) {
        if (fld.group > 0) {
          v_groups.Add(new { indx = fld.group, field = fld.name });
        }
      }
      if (v_groups.Count > 0) {
        v_groups.Sort((a, b) => {
          if (a.indx == b.indx)
            return 0;
          else if (a.indx > b.indx)
            return 1;
          else
            return -1;
        });
        String v_grpDef = null;
        foreach (var f in v_groups)
          Utl.appendStr(ref v_grpDef, f.field, ";");
        this.groupDefinition = v_grpDef;
      }
    }

    private String _lastRequestedBioCode = null;
    private CParams _lastRequestedParams = null;
    private CParams _lastReturnedParams = null;
    private CRTObject _lastLocatedRow = null;

    private Boolean _paramsChanged(CParams bioPrms) {

      Boolean vInParamsChanged = false;
      if (bioPrms != null) {
        var v_inInPrms = bioPrms.Where(p => {
          return (p.ParamDir == ParamDirection.Input) || (p.ParamDir == ParamDirection.InputOutput);
        });
        if (this._lastRequestedParams != null) {
          var v_inLastInPrms = this._lastRequestedParams.Where(p => {
            return (p.ParamDir == ParamDirection.Input) || (p.ParamDir == ParamDirection.InputOutput);
          });
          if (v_inInPrms.Count() != v_inLastInPrms.Count())
            vInParamsChanged = true;
          else {
            foreach (var p_last in v_inInPrms) {
              var v_cur = v_inLastInPrms.Where(p => {
                return String.Equals(p.Name, p_last.Name, StringComparison.CurrentCultureIgnoreCase);
              }).FirstOrDefault();
              if (v_cur == null) {
                vInParamsChanged = true;
                break;
              } else {
                if (!Object.Equals(v_cur.Value, p_last.Value)) {
                  vInParamsChanged = true;
                  break;
                }
              }
            }
          }
        } else
          vInParamsChanged = true;
      }
      return vInParamsChanged;
    }

    private void _detectIsFirstLoad(String curBioCode, CParams bioPrms, ref Boolean isMetadataObsolete, ref Boolean isFirstLoad) {
      if (!isFirstLoad) {
        if ((this._metadata == null) || !String.Equals(curBioCode, this._lastRequestedBioCode)) {
          isMetadataObsolete = true;
          isFirstLoad = true;
          return;
        }

        isFirstLoad = this._paramsChanged(bioPrms);
      }
    }

    private CJsonStoreSort _genJSSortDefinition(SortDescriptionCollection sort) {
      CJsonStoreSort rslt = new CJsonStoreSort();
      foreach (var s in sort) {
        rslt.Add(s.PropertyName, (s.Direction == ListSortDirection.Ascending) ? CJsonStoreSortOrder.Asc : CJsonStoreSortOrder.Desc);
      }
      return rslt;
    }

    public event JSClientEventHandler<AjaxResponseEventArgs> AfterLoadData;
    private void doAfterLoadData(AjaxResponseEventArgs args) {
      var handler = this.AfterLoadData;
      if (handler != null)
        handler(this, args);
    }

    public event JSClientEventHandler<CancelEventArgs> OnBeforeLoadData;
    private void doBeforLoadData(Action<CancelEventArgs> callback) {
      Utl.UiThreadInvoke(() => {
        var v_args = new CancelEventArgs {
          Cancel = false
        };
        if (this.OnBeforeLoadData != null) 
          this.OnBeforeLoadData(this, v_args);
         
        if (callback != null)
          callback(v_args);
      });
    }

    public CJsonStoreMetadataFieldDef fieldDefByName(String fieldName) {
      Int32 fldIndx = this._metadata.indexOf(fieldName);
      CJsonStoreMetadataFieldDef fldDef = (fldIndx >= 0) ? this._metadata.fields[fldIndx] : null;
      return fldDef;
    }

    private void _refreshGreadReadOnly() {
      if (this.grid._dataGrid != null) {
      }
    }

    private CJsonStoreFilter _getCurrentSelection() {
      CJsonStoreFilter v_pkSelection = null;
      if ((this.grid != null) && (this.grid.SelectedItem != null) && (this._metadata != null)) {
        var v_pk_vals = this._metadata.getPK(this.grid.SelectedItem);
        v_pkSelection = new CJsonStoreFilter {
          fromPosition = 0,
          joinCondition = CJSFilterItemSplitterType.And
        };
        foreach (var p in v_pk_vals) {
          var v_item = new CJSFilterItemCondition {
            fieldName = p.Name,
            fieldType = ftypeHelper.ConvertTypeToFType(p.ParamType),
            fieldValue = p.Value,
            cmpOperator = CJSFilterComparisionOperatorType.Eq
          };
          v_pkSelection.Items.Add(v_item);
        }
      }
      return v_pkSelection;
    }

    private Int32 _curColumnIndex = -1;
    private void _load(CParams bioPrms, AjaxRequestDelegate callback, Int64? startFrom, CJsonStoreFilter locate, CJSRequestGetType getType, String waitMsg) {
      String v_selection = null;
      if (getType == CJSRequestGetType.GetSelectedPks) {
        v_selection = (this._grid != null) ? this._grid.Selection.ToString() : null;
        if (String.IsNullOrEmpty(v_selection)) {
          if (callback != null) {
            callback(this, new AjaxResponseEventArgs());
          } else
            return;
        }
      }
      var v_locate = locate;

      var v_storeSelection = (startFrom == null);
      if (startFrom == null)
        startFrom = (this._curPage - 1) * this.pageSize;

      if (this.ajaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.bioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");

      this.doBeforLoadData((bla) => {
        if (bla.Cancel) {
          return;
        }
        if (!(String.IsNullOrEmpty(waitMsg)) && (this.grid != null))
          this.grid.showBusyIndicator(waitMsg);

        if (this.grid != null)
          this.setPageSize(this.grid.pageSize);
        if (bioPrms != null) {
          if (this.bioParams == null)
            this.bioParams = bioPrms.Clone() as CParams;
          else {
            this.bioParams.Clear();
            this.bioParams = this.bioParams.Merge(bioPrms, true);
          }
        } else {
          if (this.bioParams == null)
            this.bioParams = new CParams();
        }

        Boolean v_isMetadataObsolete = false;
        Boolean v_isFirstLoad = false;
        this._detectIsFirstLoad(this.bioCode, this.bioParams, ref v_isMetadataObsolete, ref v_isFirstLoad);
        if (v_isFirstLoad) {
          startFrom = 0;
          this._curPage = 1;
          this._lastPageDetected = ciLastPageUnassigned;
        }

        this._lastRequestedBioCode = this.bioCode;
        CJsonStoreSort v_sort = null;
        if (!v_isMetadataObsolete && (this._ds != null)) {
          var v_cv = (this._ds as ICollectionView);
          if (v_cv != null)
            v_sort = this._genJSSortDefinition(v_cv.SortDescriptions);
          var v_paramsChanged = this._paramsChanged(this.bioParams);
          if ((!v_paramsChanged) && (v_locate == null)) {
            if (v_storeSelection || (this._curPage == startFrom))
              v_locate = this._getCurrentSelection();
          }
        }

        this._lastRequestedParams = (CParams)this.bioParams.Clone();
        var v_reqst = new CJsonStoreRequestGet {
          bioCode = this.bioCode,
          getType = getType,
          bioParams = this.bioParams,
          prms = null,
          packet = new CJsonStoreData {
            limit = this.pageSize,
            start = (Int64)startFrom,
            isFirstLoad = v_isFirstLoad,
            isMetadataObsolete = v_isMetadataObsolete,
            locate = v_locate
          },
          sort = v_sort,
          selection = v_selection,
          callback = (sndr, args) => {
            try {
              Type rowType = null;
              var v_rq = args.request as CJsonStoreRequest;
              var v_rsp = args.response as CJsonStoreResponse;
              if (v_rsp != null) {
                this._lastReturnedParams = (v_rsp.bioParams != null) ? v_rsp.bioParams.Clone() as CParams : null;
                if ((v_rsp.packet != null) && (v_rsp.packet.rows != null)) {
                  if (BioGlobal.Debug) {
                    if (v_rsp.ex != null)
                      throw new EBioException("Unhandled exception!!!! silent:[" + v_rq.silent + "]", v_rsp.ex);
                  }
                  if (v_rq.packet.isMetadataObsolete) {
                    this._metadata = v_rsp.packet.metaData;
                    List<CJsonStoreMetadataFieldDef> fldDefs = this._creFldDefsRmt(this._metadata);
                    rowType = this._creRowType(fldDefs);
                    this._ds = this._creDSInst(rowType);
                  }
                  if (v_rsp.packet.limit > 0) {
                    Int64 v_curPageI = ((v_rsp.packet.start + v_rsp.packet.rows.Count) / v_rsp.packet.limit);
                    Double v_curPageD = ((Double)(v_rsp.packet.start + v_rsp.packet.rows.Count) / (Double)v_rsp.packet.limit);
                    if (v_curPageI < v_curPageD)
                      this._curPage = (int)(v_curPageD + 1.0);
                    else
                      this._curPage = v_curPageI;
                    if (this._curPage < 1) this._curPage = 1;
                    if (v_rsp.packet.endReached && (v_rsp.packet.rows.Count > 0)) {
                      this._lastPage = this._curPage;
                      this._lastPageDetected = this._lastPage;
                    } else {
                      this._lastPage = this._curPage + 1;
                    }
                  }
                  if ((this.grid != null) && (this.grid._dataGrid != null))
                    this.grid._dataGrid.ItemsSource = null;

                  this._loadDS(rowType, v_rsp);

                  if ((this.grid != null) && (this.grid._dataGrid != null)) {
                    this._initGroupDef(this._metadata);
                    this.grid._dataGrid.disableColumnsChangedEvents();
                    if ((v_rsp.packet.limit == 0) && (!String.IsNullOrEmpty(this.groupDefinition)))
                      this.grid._dataGrid.ItemsSource = this._creDSGrp(this._ds, this._metadata);
                    else
                      this.grid._dataGrid.ItemsSource = this._ds;

                    this.grid._dataGrid.enableColumnsChangedEvents();
                  }

                  if (v_rq.packet.locate != null) {
                    this._lastLocatedRow = this._locateInternal(v_rq.packet.locate);
                    if (this.grid != null) {
                      this.grid.SelectedItem = this._lastLocatedRow;
                      this.grid.CurrentColumnIndex = this._curColumnIndex;
                    }
                  }
                } else {
                  if (BioGlobal.Debug) {
                    var m = "Bad response: ";
                    if (v_rsp.packet == null)
                      m = m + "v_rsp.packet=null;";
                    else if (v_rsp.packet.rows == null)
                      m = m + "v_rsp.packet.rows=null;";
                    throw new EBioException(m);
                  }
                }
              } else {
                if (BioGlobal.Debug) {
                  var v_biorsp = args.response as CBioResponse;
                  if (v_biorsp == null)
                    throw new EBioException("Bad response: v_biorsp=null");
                }
              }
            } finally {
              this.doAfterLoadData(args);
              if (callback != null) callback(this, args);
            }
          }
        };
        this._curColumnIndex = (this.grid != null) ? this.grid.CurrentColumnIndex : -1;
        this.ajaxMng.Request(v_reqst);


      });
    }

    public void Load(CParams bioPrms, AjaxRequestDelegate callback, CJsonStoreFilter locate) {
      this._load(bioPrms, callback, null, locate, CJSRequestGetType.GetData, "Загрузка данных...");
    }

    public void Load(CParams bioPrms, AjaxRequestDelegate callback) {
      this.Load(bioPrms, callback, null);
    }

    public void LoadSelectedPks(AjaxRequestDelegate callback) {
      this._load(null, callback, null, null, CJSRequestGetType.GetSelectedPks, null);
    }
    private Boolean _checkFilter(CRTObject row, Int64 rowPos, CJsonStoreFilter filter) {
      if ((row != null) && (filter != null)) {
        if (rowPos >= filter.fromPosition) {
          return filter.check(row);
        } else
          return false;
      } else
        return false;
    }
    private CRTObject _locateInternal(CJsonStoreFilter locate) {
      if (this._ds != null) {
        var rows = this._ds.Cast<CRTObject>().ToArray();
        for (int i = 0; i < rows.Length; i++) {
          var rowPos = i + (this.pageCurrent * this.pageSize);
          if (this._checkFilter(rows[i], rowPos, locate)) {
            locate.fromPosition = rowPos + 1;
            return rows[i];
          }
        }
      }
      return null;
    }

    private void _doOnLocation(CRTObject row, EBioException exception, EventHandler<OnSelectEventArgs> callback) {
      if (row != null) {
        if (this.grid != null)
          this.grid.SelectedItem = row;
      } else {
        if ((this.grid != null) && (this.grid._dataGrid != null) && (this.grid._dataGrid.ItemsSource != null)) {
          var venumr = this.grid._dataGrid.ItemsSource.GetEnumerator();
          if (venumr != null) {
            venumr.Reset();
            if (venumr.MoveNext()) {
              this.grid.SelectedItem = venumr.Current as CRTObject;
              row = this.grid.SelectedItem;
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

    private void _goto(Int64? startFrom, String waitMsg, AjaxRequestDelegate callback, CJsonStoreFilter locate) {
      try {
        this._load(null, (s, a) => {
          if (this.grid != null) {
            this.grid.hideBusyIndicator();
            if (s != null)
              this.grid.updPosState((s as CJsonStoreClient).pageCurrent, (s as CJsonStoreClient).pageLast);
          }
          if (callback != null)
            callback(s, a);
        }, startFrom, locate, CJSRequestGetType.GetData, waitMsg);
      } catch {
        if (this.grid != null)
          this.grid.hideBusyIndicator();
        throw;
      }
    }

    public void Refresh(AjaxRequestDelegate callback) {
      this._goto(null, "Обновление...", callback, null);
    }

    public void Locate(CJsonStoreFilter locate, EventHandler<OnSelectEventArgs> callback) {
      var v_row = this._locateInternal(locate);
      if (v_row != null)
        this._doOnLocation(v_row, null, callback);
      else {
        this._goto(0, "Поиск...", (s, a) => {
          if (a.response.success)
            this._doOnLocation(this._lastLocatedRow, null, callback);
          else
            this._doOnLocation(null, a.response.ex, callback);

        }, locate);
      }
    }

    public void goPageNext(AjaxRequestDelegate callback) {
      if (this._curPage < this._lastPage) {
        this._curPage++;
        Int64 startFrom = (this._curPage - 1) * this.pageSize;
        this._goto(startFrom, "к след. странице...", callback, null);
      } else {
        if (callback != null)
          callback(null, null);
      }
    }
    public void goPagePrev(AjaxRequestDelegate callback) {
      if (this._curPage > 1) {
        this._curPage--;
        Int64 startFrom = (this._curPage - 1) * this.pageSize;
        this._goto(startFrom, "к пред. странице...", callback, null);
      } else {
        if (callback != null)
          callback(null, null);
      }
    }
    public void goPageFirst(AjaxRequestDelegate callback) {
      if (this._curPage > 1) {
        Int64 startFrom = 0;
        this._goto(startFrom, "к первой странице...", callback, null);
      } else {
        if (callback != null)
          callback(null, null);
      }
    }
    public void goPageLast(AjaxRequestDelegate callback) {
      if (this._curPage < this._lastPage) {
        Int64 startFrom = Int64.MaxValue;
        this._goto(startFrom, "к последней странице...", callback, null);
      } else {
        if (callback != null)
          callback(null, null);
      }
    }

    public event JSClientEventHandler<CancelEventArgs> BeforePostData;
    private void doBeforPostData(ref Boolean cancel) {
      var handler = this.BeforePostData;
      if (handler != null) {
        CancelEventArgs args = new CancelEventArgs {
          Cancel = cancel
        };
        handler(this, args);
        cancel = args.Cancel;
      }
    }

    public event JSClientEventHandler<AjaxResponseEventArgs> AfterPostData;
    private void doAfterPostData(AjaxResponseEventArgs args) {
      var handler = this.AfterPostData;
      if (handler != null)
        handler(this, args);
    }

    private void _applyPostingResults(CJsonStoreData packet) {
      foreach (var row in packet.rows.Where(r => (r.changeType == CJsonStoreRowChangeType.Added) || (r.changeType == CJsonStoreRowChangeType.Modified))) {
        var v_ds_row = this._ds.Cast<Object>().FirstOrDefault(itm => {
          return String.Equals((String)CTypeFactory.GetValueOfPropertyOfObject(itm, csInternalROWUID_FieldName), row.internalROWUID);
        });
        if (v_ds_row != null) {
          for (int i = 0; i < packet.metaData.fields.Count; i++) {
            var fd = packet.metaData.fields[i];
            var v_value = row.Values[i];
            var v_prop = CTypeFactory.FindPropertyOfObject(v_ds_row, fd.name);
            v_prop.SetValue(v_ds_row, v_value, null);
          }
        }
      }
    }

    public void CancelChanges(CRTObject row) {
      if ((this._ds_changes != null) && (row != null)) {
        var c = this._ds_changes.Where((itm) => { return itm.CurRow == row; }).FirstOrDefault();
        if (c != null) {
          c.CurRow.DisableEvents();
          c.OrigRow.DisableEvents();
          try {
            if (c.State == CJsonStoreRowChangeType.Modified) {
              c.CurRow.SetValues(c.OrigRow);
            }
            c.markUnchanged();
          } finally {
            c.CurRow.EnableEvents();
            c.OrigRow.EnableEvents();
          }
          this._ds_changes.Remove(c);
        }
      }
    }

    public void CancelChanges() {
      if (this._ds_changes != null) {
        foreach (var c in this._ds_changes) {
          c.CurRow.DisableEvents();
          c.OrigRow.DisableEvents();
          try {
            if (c.State == CJsonStoreRowChangeType.Added) {
              this.RemoveRow(c.CurRow);
            } else if (c.State == CJsonStoreRowChangeType.Deleted) {
              this.InsertRow(c.Index, c.OrigRow);
            } else if (c.State == CJsonStoreRowChangeType.Modified) {
              c.CurRow.SetValues(c.OrigRow);
            }
            c.markUnchanged();
          } finally {
            c.CurRow.EnableEvents();
            c.OrigRow.EnableEvents();
          }
        }
        this._clearChanges();
      }
    }

    private void _clearChanges() {
      this._ds_changes.Clear();
    }

    private CJsonStoreRow _rowAsJSRow(CJSChangedRow row) {
      if (row != null) {
        String v_intRowUID = CTypeFactory.GetValueOfPropertyOfObject(row.CurRow, csInternalROWUID_FieldName) as String;
        var v_rslt = new CJsonStoreRow() { internalROWUID = v_intRowUID, changeType = row.State };
        foreach (var fd in this._metadata.fields) {
          Object v_value = CTypeFactory.GetValueOfPropertyOfObject(row.CurRow, fd.name);
          v_rslt.Values.Add(v_value);
        }
        return v_rslt;
      } else
        return null;
    }
    private CJsonStoreRows _getChangesAsJSRows() {
      CJsonStoreRows v_rows = new CJsonStoreRows();
      foreach (var c in this._ds_changes) {
        var v_row = this._rowAsJSRow(c);
        if (v_row != null)
          v_rows.Add(v_row);
      }
      return v_rows;
    }

    private void _post(AjaxRequestDelegate callback, String trunsactionID, CSQLTransactionCmd cmd) {
      if (this.ajaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.bioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");

      Boolean cancel = false;
      this.doBeforPostData(ref cancel);
      if (cancel) {
        return;
      }

      if (this.bioParams == null)
        this.bioParams = new CParams();

      CJsonStoreRows v_rows = this._getChangesAsJSRows();
      if (v_rows.Count > 0) {
        CJsonStoreRequest reqst = new CJsonStoreRequestPost {
          bioCode = this.bioCode,
          bioParams = this.bioParams,
          prms = null,
          transactionID = trunsactionID,
          transactionCmd = cmd,
          packet = new CJsonStoreData {
            metaData = this._metadata,
            rows = v_rows
          },
          callback = (sndr, args) => {
            if (args.response.success) {
              Type rowType = null;
              CJsonStoreRequest rqst = args.request as CJsonStoreRequest;
              CJsonStoreResponse rsp = args.response as CJsonStoreResponse;
              if (rsp != null)
                this._lastReturnedParams = (rsp.bioParams != null) ? rsp.bioParams.Clone() as CParams : null;
              if ((rsp != null) && (rsp.packet != null)) {
                this._applyPostingResults(rsp.packet);
              }
              this._clearChanges();
            }
            if (callback != null) callback(this, args);
            this.doAfterPostData(args);
          }
        };
        this.ajaxMng.Request(reqst);
      } else {
        var v_args = new AjaxResponseEventArgs() {
          request = null,
          response = new CAjaxResponse() {
            ex = null,
            success = true,
            responseText = String.Empty
          },
          stream = null
        };
        if (callback != null) callback(this, v_args);
        this.doAfterPostData(v_args);
      }
    }

    public void Post(AjaxRequestDelegate callback, String trunsactionID, CSQLTransactionCmd cmd) {
      this._post(callback, trunsactionID, cmd);
    }
    public void Post(AjaxRequestDelegate callback, String trunsactionID) {
      this._post(callback, trunsactionID, CSQLTransactionCmd.Nop);
    }
    public void Post(AjaxRequestDelegate callback) {
      this._post(callback, null, CSQLTransactionCmd.Nop);
    }

    public CJsonStoreMetadata jsMetadata {
      get {
        return this._metadata;
      }
    }
  }

  public class CurrFormatter : IValueConverter {
    private CJsonStoreMetadataFieldDef _fldDef = null;
    private DataGridColumn _column = null;
    public CurrFormatter(CJsonStoreMetadataFieldDef fldDef, DataGridColumn col) {
      this._fldDef = fldDef;
      this._column = col;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (!String.IsNullOrEmpty(this._fldDef.format)) {

        if (value is Decimal) {
          return ((Decimal)value).ToString(this._fldDef.format);
        } else if (value is Double) {
          return ((Double)value).ToString(this._fldDef.format);
        } else if (value is Int64) {
          return ((Int64)value).ToString(this._fldDef.format);
        } else if ((value is DateTime) || (value is DateTime?) || (value is DateTimeOffset) || (value is DateTimeOffset?)) {
          if (value != null)
            return ((DateTime)value).ToString(this._fldDef.format);
          else
            return null;
        } else
          return value;

      } else
        return value;
    }

    public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture) {
      try {
        return Utl.Convert2Type(value, targetType);
      } catch (Exception) {
        if (Utl.typeIsNumeric(targetType))
          return 0;
        else if (targetType == typeof(DateTime))
          return DateTime.MinValue;
        else
          return null;
      }
    }
  }

  public class GroupHeaderFormatter : IValueConverter {
    private PropertyGroupDescription _grpDescr = null;
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

}
