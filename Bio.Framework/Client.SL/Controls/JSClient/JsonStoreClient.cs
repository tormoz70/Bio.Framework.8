using System;
using System.Windows.Controls;
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Collections.Generic;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;
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
    public JSClientAfterMonRowEventArgs(CRTObject row) {
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
    public JSClientRowPropertyChangedEventArgs(CRTObject row, String propertyName) {
      this.Row = row;
      this.PropertyName = propertyName;
    }
  }

  public delegate void JSClientEventHandler<in T>(JsonStoreClient sender, T args) where T : EventArgs;

  public class JSChangedRow {
    public JSChangedRow(Int32 index, JsonStoreRowChangeType state, CRTObject newRow, CRTObject origRow) {
      this.Index = index;
      this.State = state;
      this.OrigRow = origRow;
      this.CurRow = newRow;
    }
    public Int32 Index { get; private set; }
    public JsonStoreRowChangeType State { get; private set; }
    public CRTObject OrigRow { get; private set; }
    public CRTObject CurRow { get; private set; }
    public void MarkUnchanged() {
      this.State = JsonStoreRowChangeType.Unchanged;
    }
  }

  public enum RestorePosModeType {
    ByPosition,
    ByPrimaryKey
  }

  public class JsonStoreClient {

    //private JSGrid _grid { get; set; }

    public event JSClientEventHandler<JSClientBeforeMonRowEventArgs> OnBeforeInsertRow;
    public event JSClientEventHandler<JSClientAfterMonRowEventArgs> OnAfterInsertRow;
    public event JSClientEventHandler<JSClientBeforeMonRowEventArgs> OnBeforeDeleteRow;
    public event JSClientEventHandler<JSClientAfterMonRowEventArgs> OnAfterDeleteRow;

    public event JSClientEventHandler<JSClientRowPropertyChangingEventArgs> OnRowPropertyChanging;
    public event JSClientEventHandler<JSClientRowPropertyChangedEventArgs> OnRowPropertyChanged;

    public event EventHandler<RefreshEventArgs> OnDSRequestRefresh;

    public IAjaxMng AjaxMng { get; set; }
    public String BioCode { get; set; }
    private Params _bioParams;
    public Params BioParams {
      get {
        if (this._bioParams == null)
          this._bioParams = new Params();
        return this._bioParams;
      }
      set {
        this._bioParams = value;
      }
    }

    public long PageSize { get; private set; }

    public long StartFrom { get; private set; }

    public const String CS_INTERNAL_ROWUID_FIELD_NAME = "InternalROWUID";
    private List<PropertyMetadata> _genPropertyDefs(List<CJsonStoreMetadataFieldDef> fieldDefs) {
      var rslt = new List<PropertyMetadata>();
      rslt.Add(new PropertyMetadata {
        Name = CS_INTERNAL_ROWUID_FIELD_NAME,
        DisplayName = String.Empty,
        Type = typeof(String),
        Group = String.Empty,
        Hidden = true
      });
      foreach (var fieldDef in fieldDefs)
        rslt.Add(new PropertyMetadata {
          Name = fieldDef.name,
          DisplayName = fieldDef.header,
          Type = fieldDef.GetDotNetType(),
          Group = (fieldDef.group > 0) ? "Group-" + fieldDef.group : String.Empty,
          Hidden = fieldDef.hidden
        });
      return rslt;
    }

    private readonly TypeFactory _typeFactory = new TypeFactory();
    private Type _creRowType(List<CJsonStoreMetadataFieldDef> fieldDefs) {
      var propDefs = this._genPropertyDefs(fieldDefs);
      return this._typeFactory.CreateType(propDefs);
    }

    private IEnumerable _creDSInst(Type rowType) {
      IEnumerable rslt;
      if (this.PageSize > 0) {
        var listType = typeof(SortableCollectionView<>).MakeGenericType(new[] { rowType });
        rslt = Activator.CreateInstance(listType, new EventHandler<RefreshEventArgs>(this._doOnDSRequestRefresh)) as IEnumerable;
      } else {
        var listType = typeof(ObservableCollection<>).MakeGenericType(new[] { rowType });
        rslt = Activator.CreateInstance(listType) as IEnumerable;
      }
      return rslt;
    }

    //private void _doOnDSRequestRefresh(object sender, RefreshEventArgs e) {
    //  this._goto(null, "Сортировка...", null, null);
    //}

    private void _doOnDSRequestRefresh(Object sender, RefreshEventArgs e) {
      var eve = this.OnDSRequestRefresh;
      if (eve != null)
        eve(sender, e);
    }

    private CJsonStoreMetadata _metadata;
    public Boolean ReadOnly { get { return (this._metadata == null) || this._metadata.readOnly; } }
    private readonly List<JSChangedRow> _dsChanges;

    public IEnumerable DS0 { get; private set; }

    public IEnumerable<CRTObject> DS {
      get {
        return (this.DS0 != null) ? this.DS0.Cast<CRTObject>() : null;
      }
    }

    public List<JSChangedRow> Changes { get { return this._dsChanges; } }
    //public String GroupDefinition { get; set; }

    //public JSGrid grid {
    //  get {
    //    return this._grid;
    //  }
    //  set {
    //    if (this._grid != value) {
    //      this._grid = value;
    //      if ((this._grid != null) && (this._grid._dataGrid != null)) {
    //        this._grid._dataGrid.AutoGenerateColumns = true;
    //      }
    //    }
    //  }
    //}

    public RestorePosModeType RestorePosMode { get; set; }

    public JsonStoreClient() {
      DS0 = null;
      this._dsChanges = new List<JSChangedRow>();
    }

    private static List<CJsonStoreMetadataFieldDef> _creFldDefsRmt(CJsonStoreMetadata metaData) {
      var flds = new List<CJsonStoreMetadataFieldDef>();
      foreach (var fld in metaData.fields)
        flds.Add(fld);
      return flds;
    }

    private void _regChanges(JSChangedRow chng) {
      var v_exists = this._dsChanges.FirstOrDefault(c => c.CurRow.Equals(chng.CurRow));
      switch (chng.State) {
        case JsonStoreRowChangeType.Added:
          if (v_exists == null)
            this._dsChanges.Add(chng);
          else {
            if ((v_exists.State == JsonStoreRowChangeType.Deleted) ||
                (v_exists.State == JsonStoreRowChangeType.Modified)) {
              this._dsChanges.Remove(v_exists);
              this._dsChanges.Add(chng);
            }
          }
          break;
        case JsonStoreRowChangeType.Deleted:
          if (v_exists == null)
            this._dsChanges.Add(chng);
          else {
            if (v_exists.State == JsonStoreRowChangeType.Added) {
              this._dsChanges.Remove(v_exists);
            }
            if (v_exists.State == JsonStoreRowChangeType.Modified) {
              this._dsChanges.Remove(v_exists);
              this._dsChanges.Add(chng);
            }
          }
          break;
        case JsonStoreRowChangeType.Modified:
          if (v_exists == null)
            this._dsChanges.Add(chng);
          break;
      }
    }

    private Boolean _rowMonEventsDisabled;
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

    public Int32 IndexOf(CRTObject row) {
      var foundItem = this.DS.Select((item, index) => new {
        Item = item,
        Position = index
      }).FirstOrDefault(i => i.Item == row);
      return (foundItem != null) ? foundItem.Position : -1;
    }

    private void _doOnAfterInsertRow(CRTObject newRow) {
      if (!this._rowMonEventsDisabled) {
        var v_indx = this.IndexOf(newRow);
        this._regChanges(new JSChangedRow(v_indx, JsonStoreRowChangeType.Added, newRow, null));
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
        var v_index = this.IndexOf(r);
        this._regChanges(new JSChangedRow(v_index, JsonStoreRowChangeType.Added, r, null));
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
        this._regChanges(new JSChangedRow(index, JsonStoreRowChangeType.Deleted, row, row));
        var eve = this.OnAfterDeleteRow;
        if (eve != null) {
          var args = new JSClientAfterMonRowEventArgs(row);
          eve(this, args);
        }
      }
    }

    public void InsertRow(Int32 index, CRTObject row) {
      var v_ds = this.DS0 as IList;
      if (v_ds != null) {
        var v_cancel = false;
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

    private CRTObject _lastPreChangedRow;
    private void _doOnRowPropertyChanging(Object sender, PropertyChangingEventArgs e) {
      this._lastPreChangedRow = ((CRTObject)sender).Copy();
      var eve = this.OnRowPropertyChanging;
      if (eve != null) {
        var ee = new JSClientRowPropertyChangingEventArgs(false, this._lastPreChangedRow, e.PropertyName);
        eve(this, ee);
      }
    }
    private void _doOnRowPropertyChanged(Object sender, PropertyChangedEventArgs e) {
      var v_indx = this.IndexOf(this._lastPreChangedRow);
      this._regChanges(new JSChangedRow(v_indx, JsonStoreRowChangeType.Modified, (CRTObject)sender, this._lastPreChangedRow));
      var eve = this.OnRowPropertyChanged;
      if (eve != null) {
        var ee = new JSClientRowPropertyChangedEventArgs(this._lastPreChangedRow, e.PropertyName);
        eve(this, ee);
      }
    }


    public CRTObject NewRow() {
      if (this._lastRowType != null) {
        var row = TypeFactory.CreateInstance(this._lastRowType, this._doOnRowPropertyChanging, this._doOnRowPropertyChanged);
        row.DisableEvents();
        try {
          var intRowUID = Guid.NewGuid().ToString("N");
          _setFieldValue(row, CS_INTERNAL_ROWUID_FIELD_NAME, intRowUID);
        } finally {
          row.EnableEvents();
        }
        return row;
      }
      return null;
    }

    public void RemoveRowAt(Int32 index) {
      var v_ds = this.DS0 as IList;
      if (v_ds != null) {
        if ((index >= 0) && (index < v_ds.Count)) {
          var row = v_ds[index] as CRTObject;
          this.RemoveRow(row);
        }
      }
    }
    public void RemoveRow(CRTObject row) {
      if (row != null) {
        var v_ds = this.DS0 as IList;
        if (v_ds != null) {
          var v_cancel = false;
          this._doOnBeforeDeleteRow(ref v_cancel, row);
          if (!v_cancel) {
            var v_indx = this.IndexOf(row);
            v_ds.Remove(row);
            this._doOnAfterDeleteRow(v_indx, row);
          }
        }
      }
    }

    private static void _setFieldValue(CRTObject row, String fldName, Object value) {
      if (row != null) {
        row[fldName] = value;
      }
    }

    public Type RowType { get { return this._lastRowType; } }
    private Type _lastRowType;
    [MethodImpl(MethodImplOptions.Synchronized)]
    private void _loadDS(Type rowType, JsonStoreResponse jsRsp) {
      this._rowMonEventsDisabled = true;
      try {
        var rsp = jsRsp;
        if ((rsp != null) && (rsp.packet != null) &&
                (rsp.packet.metaData != null) && (rsp.packet.rows != null)) {
          if (rowType != null)
            this._lastRowType = rowType;
          var lastRowType = this._lastRowType;
          if (lastRowType != null) {
            this.ClearData();
            foreach (var r in rsp.packet.rows) {
              var row = this.AddRow();
              row.DisableEvents();
              try {
                foreach (var fld in rsp.packet.metaData.fields) {
                  var value = r.Values[rsp.packet.metaData.indexOf(fld.name)];
                  _setFieldValue(row, fld.name, value);
                }
              } finally {
                row.EnableEvents();
              }
            }
            rsp.packet.rows.Clear();
          }
        }
      } finally {
        this._rowMonEventsDisabled = false;
      }
    }

    public void ClearData() {
      var v_ds = this.DS0 as IList;
      if (v_ds != null) {
        v_ds.Clear();
      }
      this._clearChanges();
    }

    //private const Int32 ciLastPageUnassigned = -1;
    ///// <summary>
    ///// Это последняя страница, когда достигнута последняя страница
    ///// </summary>
    //private Int64 _lastPageDetected = ciLastPageUnassigned;
    ///// <summary>
    ///// Последняя страница пока не достигнута последняя страница
    ///// </summary>
    //private Int64 _lastPage = ciLastPageUnassigned;
    ///// <summary>
    ///// Текущая страница
    ///// </summary>
    //private Int64 _curPage = 1;
    //public String PagePositionDesc {
    //  get {
    //    if (this.PageSize > 0) {
    //      var v_lstPageNum = (this._lastPageDetected == ciLastPageUnassigned) ? "?" : "" + this._lastPageDetected;
    //      return String.Format("Страница {0} из {1} (по {2} записей)", this._curPage, v_lstPageNum, this.PageSize);
    //    }
    //    return null;
    //  }
    //}
    //public Int64 PageCurrent {
    //  get {
    //    return this._curPage;
    //  }
    //}
    //public Int64 PageLast {
    //  get {
    //    return this._lastPageDetected;
    //  }
    //}

    //private PagedCollectionView _pcv;
    //private IEnumerable _creDSGrp(IEnumerable ds, CJsonStoreMetadata metadata) {
    //  this._pcv = new PagedCollectionView(ds);
    //  var flds = Utl.SplitString(this.GroupDefinition, new Char[] { ',', ';', ' ' });
    //  foreach (var fldName in flds) {
    //    var v_indx = metadata.indexOf(fldName);
    //    if (v_indx == -1)
    //      throw new EBioException("Группировка по полю {0} не возможна, т.к. поле не найдено в метаданных.");
    //    var gd = new PropertyGroupDescription();
    //    gd.PropertyName = metadata.fields[v_indx].name;
    //    gd.StringComparison = StringComparison.CurrentCultureIgnoreCase;
    //    gd.Converter = new GroupHeaderFormatter(gd);
    //    this._pcv.GroupDescriptions.Add(gd);
    //  }
    //  return this._pcv;
    //}

    //private void _initGroupDef(CJsonStoreMetadata md) {
    //  if (!String.IsNullOrEmpty(this.GroupDefinition))
    //    return;
    //  var groups = (new[] { new { indx = 0, field = "" } }).ToList();
    //  groups.Clear();
    //  foreach (var fld in md.fields) {
    //    if (fld.group > 0) {
    //      groups.Add(new { indx = fld.group, field = fld.name });
    //    }
    //  }
    //  if (groups.Count > 0) {
    //    groups.Sort((a, b) => {
    //      if (a.indx == b.indx)
    //        return 0;
    //      if (a.indx > b.indx)
    //        return 1;
    //      return -1;
    //    });
    //    String grpDef = null;
    //    foreach (var f in groups)
    //      Utl.AppendStr(ref grpDef, f.field, ";");
    //    this.GroupDefinition = grpDef;
    //  }
    //}

    private String _lastRequestedBioCode;
    private Params _lastRequestedParams;
    private Params _lastReturnedParams;
    //private CRTObject _lastLocatedRow = null;

    private Boolean _paramsChanged(Params bioPrms) {

      var inParamsChanged = false;
      if (bioPrms != null) {
        var inInPrms = bioPrms.Where(p => {
          return (p.ParamDir == ParamDirection.Input) || (p.ParamDir == ParamDirection.InputOutput);
        });
        if (this._lastRequestedParams != null) {
          var inLastInPrms = this._lastRequestedParams.Where(p => {
            return (p.ParamDir == ParamDirection.Input) || (p.ParamDir == ParamDirection.InputOutput);
          });
          if (inInPrms.Count() != inLastInPrms.Count())
            inParamsChanged = true;
          else {
            foreach (var param in inInPrms) {
              var v_cur = inLastInPrms.Where(p => {
                return String.Equals(p.Name, param.Name, StringComparison.CurrentCultureIgnoreCase);
              }).FirstOrDefault();
              if (v_cur == null) {
                inParamsChanged = true;
                break;
              }
              if (!Equals(v_cur.Value, param.Value)) {
                inParamsChanged = true;
                break;
              }
            }
          }
        } else
          inParamsChanged = true;
      }
      return inParamsChanged;
    }

    private void _detectIsFirstLoad(String curBioCode, Params bioPrms, ref Boolean isMetadataObsolete, ref Boolean isFirstLoad) {
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
      var rslt = new CJsonStoreSort();
      foreach (var s in sort) {
        rslt.Add(s.PropertyName, (s.Direction == ListSortDirection.Ascending) ? CJsonStoreSortOrder.Asc : CJsonStoreSortOrder.Desc);
      }
      return rslt;
    }

    public event JSClientEventHandler<AjaxResponseEventArgs> OnAfterLoadData;
    private void _doOnAfterLoadData(AjaxResponseEventArgs args) {
      var handler = this.OnAfterLoadData;
      if (handler != null)
        handler(this, args);
    }

    public event JSClientEventHandler<CancelEventArgs> OnBeforeLoadData;
    private void _doBeforLoadData(Action<CancelEventArgs> callback) {
      //Utl.UiThreadInvoke(() => {
        var v_args = new CancelEventArgs {
          Cancel = false
        };
        if (this.OnBeforeLoadData != null) 
          this.OnBeforeLoadData(this, v_args);
         
        if (callback != null)
          callback(v_args);
      //});
    }

    public CJsonStoreMetadataFieldDef FieldDefByName(String fieldName) {
      var fldIndx = this._metadata.indexOf(fieldName);
      var fldDef = (fldIndx >= 0) ? this._metadata.fields[fldIndx] : null;
      return fldDef;
    }

    //private void _refreshGreadReadOnly() {
    //  if (this.grid._dataGrid != null) {
    //  }
    //}

    //private void _addItemToSelection(CJsonStoreFilter selection, CRTObject item) {
    //  var v_pkVals = this._metadata.getPK(item);
    //  foreach (var p in v_pkVals) {
    //    var v_item = new CJSFilterItemCondition {
    //      fieldName = p.Name,
    //      fieldType = ftypeHelper.ConvertTypeToFType(p.ParamType),
    //      fieldValue = p.Value,
    //      cmpOperator = CJSFilterComparisionOperatorType.Eq
    //    };
    //    selection.Items.Add(v_item);
    //  }
    //}

    //private CJsonStoreFilter _getCurrentSelection() {
    //  CJsonStoreFilter v_pkSelection = null;
    //  if ((this.grid != null) && (this.grid.SelectedItem != null) && (this._metadata != null)) {
    //    v_pkSelection = new CJsonStoreFilter {
    //      fromPosition = 0,
    //      joinCondition = CJSFilterItemSplitterType.And
    //    };
    //    this._addItemToSelection(v_pkSelection, this.grid.SelectedItem);
    //  }
    //  return v_pkSelection;
    //}

    //private Int32 _curRowIndex = -1;
    //private Int32 _curColumnIndex = -1;

    public event JSClientEventHandler<AjaxResponseEventArgs> OnJsonStoreResponseSuccess;
    private void _doOnJsonStoreResponseSuccess(JsonStoreRequest request, JsonStoreResponse response) {
      var eve = this.OnJsonStoreResponseSuccess;
      if (eve != null)
        eve(this, new AjaxResponseEventArgs { Request = request, Response = response });
    }

    public event JSClientEventHandler<JsonStoreDSLoadedEventArgs> OnJsonStoreDSLoaded;
    private void _doOnJsonStoreDSLoaded(IEnumerable ds, JsonStoreRequest request, JsonStoreResponse response) {
      var eve = this.OnJsonStoreDSLoaded;
      if (eve != null)
        eve(this, new JsonStoreDSLoadedEventArgs { DS = ds, Request = request, Response = response });
    }

    private void _load (
      CJSRequestGetType getType,
      Params bioPrms, 
      AjaxRequestDelegate callback, 
      CJsonStoreFilter locate, 
      Int64? pageSize,
      Int64? startFrom,
      String selection
    ) {
      var v_selection = selection;
      //if (getType == CJSRequestGetType.GetSelectedPks) {
      //  v_selection = (this._grid != null) ? this._grid.Selection.ToString() : null;
      //  if (String.IsNullOrEmpty(v_selection)) {
      //    if (callback != null) {
      //      callback(this, new AjaxResponseEventArgs());
      //    } else
      //      return;
      //  }
      //}
      var v_locate = locate;
      if (pageSize.HasValue)
        this.PageSize = pageSize.Value;
      //var v_storeSelection = (startFrom == null);
      //if (startFrom.HasValue)
      //  startFrom = (this._curPage - 1) * this.PageSize;
      if (startFrom.HasValue)
        this.StartFrom = startFrom.Value;

      if (this.AjaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.BioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");

      this._doBeforLoadData(bla => {
        if (bla.Cancel) {
          return;
        }
        //if (!(String.IsNullOrEmpty(waitMsg)) && (this.grid != null))
        //  this.grid.showBusyIndicator(waitMsg);

        //if (this.grid != null)
        //  this.SetPageSize(this.grid.pageSize);
        if (bioPrms != null) {
          if (this.BioParams == null)
            this.BioParams = bioPrms.Clone() as Params;
          else {
            this.BioParams.Clear();
            this.BioParams = this.BioParams.Merge(bioPrms, true);
          }
        } else {
          if (this.BioParams == null)
            this.BioParams = new Params();
        }

        var isMetadataObsolete = false;
        var isFirstLoad = false;
        this._detectIsFirstLoad(this.BioCode, this.BioParams, ref isMetadataObsolete, ref isFirstLoad);
        if (isFirstLoad) {
          this.StartFrom = 0;
          //this._curPage = 1;
          //this._lastPageDetected = ciLastPageUnassigned;
        }

        this._lastRequestedBioCode = this.BioCode;
        CJsonStoreSort sortDefinition = null;
        if (!isMetadataObsolete && (this.DS0 != null)) {
          var cv = (this.DS0 as ICollectionView);
          if (cv != null)
            sortDefinition = this._genJSSortDefinition(cv.SortDescriptions);
          //if (this.RestorePosMode == RestorePosModeType.ByPrimaryKey) {
          //  var v_paramsChanged = this._paramsChanged(this.BioParams);
          //  if ((!v_paramsChanged) && (v_locate == null)) {
          //    if (v_storeSelection || (this._curPage == startFrom))
          //      v_locate = this._getCurrentSelection();
          //  }
          //}
        }

        this._lastRequestedParams = (Params)this.BioParams.Clone();
        var reqst = new JsonStoreRequestGet {
          BioCode = this.BioCode,
          getType = getType,
          BioParams = this.BioParams,
          Prms = null,
          Packet = new CJsonStoreData {
            limit = this.PageSize,
            start = this.StartFrom,
            isFirstLoad = isFirstLoad,
            isMetadataObsolete = isMetadataObsolete,
            locate = v_locate
          },
          Sort = sortDefinition,
          selection = v_selection,
          Callback = (sndr, args) => {
            try {
              Type rowType = null;
              var rq = (JsonStoreRequest)args.Request;
              var rsp = args.Response as JsonStoreResponse;
              if (rsp != null) {
                this._lastReturnedParams = (rsp.BioParams != null) ? rsp.BioParams.Clone() as Params : null;
                if ((rsp.packet != null) && (rsp.packet.rows != null)) {
                  if (BioGlobal.Debug) {
                    if (rsp.Ex != null)
                      throw new EBioException("Unhandled exception!!!! silent:[" + rq.Silent + "]", rsp.Ex);
                  }
                  if (rq.Packet.isMetadataObsolete) {
                    this._metadata = rsp.packet.metaData;
                    var fldDefs = _creFldDefsRmt(this._metadata);
                    rowType = this._creRowType(fldDefs);
                    this.DS0 = this._creDSInst(rowType);
                  }

                  this._doOnJsonStoreResponseSuccess(rq, rsp);

                  //if (v_rsp.packet.limit > 0) {
                  //  var v_curPageI = ((v_rsp.packet.start + v_rsp.packet.rows.Count) / v_rsp.packet.limit);
                  //  var v_curPageD = ((Double)(v_rsp.packet.start + v_rsp.packet.rows.Count) / (Double)v_rsp.packet.limit);
                  //  if (v_curPageI < v_curPageD)
                  //    this._curPage = (int)(v_curPageD + 1.0);
                  //  else
                  //    this._curPage = v_curPageI;
                  //  if (this._curPage < 1) this._curPage = 1;
                  //  if (v_rsp.packet.endReached && (v_rsp.packet.rows.Count > 0)) {
                  //    this._lastPage = this._curPage;
                  //    this._lastPageDetected = this._lastPage;
                  //  } else {
                  //    this._lastPage = this._curPage + 1;
                  //  }
                  //}
                  //if ((this.grid != null) && (this.grid._dataGrid != null))
                  //  this.grid._dataGrid.ItemsSource = null;

                  this._loadDS(rowType, rsp);

                  this._doOnJsonStoreDSLoaded(this.DS0, rq, rsp);

                  //if ((this.grid != null) && (this.grid._dataGrid != null)) {
                  //  this._initGroupDef(this._metadata);
                  //  this.grid._dataGrid.disableColumnsChangedEvents();
                  //  if ((v_rsp.packet.limit == 0) && (!String.IsNullOrEmpty(this.groupDefinition)))
                  //    this.grid._dataGrid.ItemsSource = this._creDSGrp(this.DS0, this._metadata);
                  //  else
                  //    this.grid._dataGrid.ItemsSource = this.DS0;

                  //  this.grid._dataGrid.enableColumnsChangedEvents();
                  //}

                  //if (this.grid != null) {
                  //  if (v_rq.packet.locate != null) {
                  //    this._lastLocatedRow = this._locateInternal(v_rq.packet.locate);
                  //    this.grid.SelectedItem = this._lastLocatedRow;
                  //  } else
                  //    this.grid.SelectedIndex = (this._curRowIndex == -1 ? 0 : this._curRowIndex);
                  //  this.grid.CurrentColumnIndex = (this._curColumnIndex == -1 ? 0 : this._curColumnIndex);
                  //} else { 
                  if (rq.Packet.locate != null) 
                    this._lastLocatedRow = this._locateInternal(rq.Packet.locate);
                  //}

                } else {
                  if (BioGlobal.Debug) {
                    var m = "Bad response: ";
                    if (rsp.packet == null)
                      m = m + "rsp.packet=null;";
                    else if (rsp.packet.rows == null)
                      m = m + "rsp.packet.rows=null;";
                    throw new EBioException(m);
                  }
                }
              } else {
                if (BioGlobal.Debug) {
                  var biorsp = args.Response as BioResponse;
                  if (biorsp == null)
                    throw new EBioException("Bad response: biorsp=null");
                }
              }
            } finally {
              this._doOnAfterLoadData(args);
              if (callback != null) callback(this, args);
            }
          }
        };
        //this._curColumnIndex = (this.grid != null) ? this.grid.CurrentColumnIndex : -1;
        //this._curRowIndex = (this.grid != null) ? this.grid.SelectedIndex : -1;
        this.AjaxMng.Request(reqst);


      });
    }

    public void Load(Params bioPrms, AjaxRequestDelegate callback, CJsonStoreFilter locate, Int64? pageSize, Int64? startFrom) {
      this._load(CJSRequestGetType.GetData, bioPrms, callback, locate, pageSize, startFrom, null);
    }

    public void Load(Params bioPrms, AjaxRequestDelegate callback, CJsonStoreFilter locate) {
      this.Load(bioPrms, callback, locate, null, null);
    }

    public void Load(Params bioPrms, AjaxRequestDelegate callback) {
      this.Load(bioPrms, callback, null);
    }

    public void Load(AjaxRequestDelegate callback) {
      this.Load(null, callback);
    }

    public void LoadSelectedPks(AjaxRequestDelegate callback, String selection) {
      this._load(CJSRequestGetType.GetSelectedPks, null, callback, null, 0, null, selection);
    }

    private static Boolean _checkFilter(CRTObject row, Int64 rowPos, CJsonStoreFilter filter) {
      if ((row != null) && (filter != null)) {
        if (rowPos >= filter.fromPosition) {
          return filter.check(row);
        }
        return false;
      }
      return false;
    }

    private CRTObject _locateInternal(CJsonStoreFilter locate) {
      if (this.DS0 != null) {
        var rows = this.DS0.Cast<CRTObject>().ToArray();
        for (var i = 0; i < rows.Length; i++) {
          var rowPos = i + this.StartFrom;
          if (_checkFilter(rows[i], rowPos, locate)) {
            locate.fromPosition = rowPos + 1;
            return rows[i];
          }
        }
      }
      return null;
    }

    private void _doOnLocation(CRTObject row, EBioException exception, EventHandler<OnSelectEventArgs> callback) {
      if (callback != null) {
        callback(this, new OnSelectEventArgs {
          ex = exception,
          selection = new VSingleSelection { ValueRow = row }
        });
      }
    }

    //public void Locate(CJsonStoreFilter locate, EventHandler<OnSelectEventArgs> callback, Boolean forceRemote = false) {
    //  CRTObject v_row = null;
    //  if(!forceRemote)
    //    v_row = this._locateInternal(locate);
    //  if (v_row != null)
    //    this._doOnLocation(v_row, null, callback);
    //  else {
    //    this._goto(0, "Поиск...", (s, a) => {
    //      if (a.response.success)
    //        this._doOnLocation(this._lastLocatedRow, null, callback);
    //      else
    //        this._doOnLocation(null, a.response.ex, callback);

    //    }, locate);
    //  }
    //}

    private CRTObject _lastLocatedRow;
    public void Locate(Params bioPrms, EventHandler<OnSelectEventArgs> callback, CJsonStoreFilter locate, Boolean forceRemote = false) {
      CRTObject v_row = null;
      if (!forceRemote)
        v_row = this._locateInternal(locate);
      if (v_row != null)
        this._doOnLocation(v_row, null, callback);
      else {
        this.Load(bioPrms, (s, a) => {
          if (a.Response.Success)
            this._doOnLocation(this._lastLocatedRow, null, callback);
          else
            this._doOnLocation(null, a.Response.Ex, callback);

        }, locate, null, 0);
      }
    }

    public void Locate(EventHandler<OnSelectEventArgs> callback, CJsonStoreFilter locate, Boolean forceRemote = false) {
      this.Locate(null, callback, locate, forceRemote);
    }

    //public void goPageNext(AjaxRequestDelegate callback) {
    //  if (this._curPage < this._lastPage) {
    //    this._curPage++;
    //    Int64 startFrom = (this._curPage - 1) * this.PageSize;
    //    this._goto(startFrom, "к след. странице...", callback, null);
    //  } else {
    //    if (callback != null)
    //      callback(null, null);
    //  }
    //}
    //public void goPagePrev(AjaxRequestDelegate callback) {
    //  if (this._curPage > 1) {
    //    this._curPage--;
    //    Int64 startFrom = (this._curPage - 1) * this.PageSize;
    //    this._goto(startFrom, "к пред. странице...", callback, null);
    //  } else {
    //    if (callback != null)
    //      callback(null, null);
    //  }
    //}
    //public void goPageFirst(AjaxRequestDelegate callback) {
    //  if (this._curPage > 1) {
    //    Int64 startFrom = 0;
    //    this._goto(startFrom, "к первой странице...", callback, null);
    //  } else {
    //    if (callback != null)
    //      callback(null, null);
    //  }
    //}
    //public void goPageLast(AjaxRequestDelegate callback) {
    //  if (this._curPage < this._lastPage) {
    //    Int64 startFrom = Int64.MaxValue;
    //    this._goto(startFrom, "к последней странице...", callback, null);
    //  } else {
    //    if (callback != null)
    //      callback(null, null);
    //  }
    //}

    public event JSClientEventHandler<CancelEventArgs> BeforePostData;
    private void _doBeforPostData(ref Boolean cancel) {
      var handler = this.BeforePostData;
      if (handler != null) {
        var args = new CancelEventArgs {
          Cancel = cancel
        };
        handler(this, args);
        cancel = args.Cancel;
      }
    }

    public event JSClientEventHandler<AjaxResponseEventArgs> AfterPostData;
    private void _doAfterPostData(AjaxResponseEventArgs args) {
      var handler = this.AfterPostData;
      if (handler != null)
        handler(this, args);
    }

    private void _applyPostingResults(CJsonStoreData packet) {
      foreach (var row in packet.rows.Where(r => (r.ChangeType == JsonStoreRowChangeType.Added) || (r.ChangeType == JsonStoreRowChangeType.Modified))) {
        var v_ds_row = this.DS0.Cast<Object>().FirstOrDefault(itm => {
          return String.Equals((String)TypeFactory.GetValueOfPropertyOfObject(itm, CS_INTERNAL_ROWUID_FIELD_NAME), row.InternalROWUID);
        });
        if (v_ds_row != null) {
          for (var i = 0; i < packet.metaData.fields.Count; i++) {
            var fd = packet.metaData.fields[i];
            var v_value = row.Values[i];
            var v_prop = TypeFactory.FindPropertyOfObject(v_ds_row, fd.name);
            v_prop.SetValue(v_ds_row, v_value, null);
          }
        }
      }
    }

    public void CancelChanges(CRTObject row) {
      if ((this._dsChanges != null) && (row != null)) {
        var c = this._dsChanges.Where((itm) => { return itm.CurRow == row; }).FirstOrDefault();
        if (c != null) {
          c.CurRow.DisableEvents();
          c.OrigRow.DisableEvents();
          try {
            if (c.State == JsonStoreRowChangeType.Modified) {
              c.CurRow.SetValues(c.OrigRow);
            }
            c.MarkUnchanged();
          } finally {
            c.CurRow.EnableEvents();
            c.OrigRow.EnableEvents();
          }
          this._dsChanges.Remove(c);
        }
      }
    }

    public void CancelChanges() {
      if (this._dsChanges != null) {
        foreach (var c in this._dsChanges) {
          c.CurRow.DisableEvents();
          c.OrigRow.DisableEvents();
          try {
            if (c.State == JsonStoreRowChangeType.Added) {
              this.RemoveRow(c.CurRow);
            } else if (c.State == JsonStoreRowChangeType.Deleted) {
              this.InsertRow(c.Index, c.OrigRow);
            } else if (c.State == JsonStoreRowChangeType.Modified) {
              c.CurRow.SetValues(c.OrigRow);
            }
            c.MarkUnchanged();
          } finally {
            c.CurRow.EnableEvents();
            c.OrigRow.EnableEvents();
          }
        }
        this._clearChanges();
      }
    }

    private void _clearChanges() {
      this._dsChanges.Clear();
    }

    private JsonStoreRow _rowAsJSRow(JSChangedRow row) {
      if (row != null) {
        var intRowUID = TypeFactory.GetValueOfPropertyOfObject(row.CurRow, CS_INTERNAL_ROWUID_FIELD_NAME) as String;
        var v_rslt = new JsonStoreRow { InternalROWUID = intRowUID, ChangeType = row.State };
        foreach (var fd in this._metadata.fields) {
          var v_value = TypeFactory.GetValueOfPropertyOfObject(row.CurRow, fd.name);
          v_rslt.Values.Add(v_value);
        }
        return v_rslt;
      }
      return null;
    }

    private JsonStoreRows _getChangesAsJSRows() {
      var rows = new JsonStoreRows();
      foreach (var c in this._dsChanges) {
        var v_row = this._rowAsJSRow(c);
        if (v_row != null)
          rows.Add(v_row);
      }
      return rows;
    }

    private void _post(AjaxRequestDelegate callback, String trunsactionID, SQLTransactionCmd cmd) {
      if (this.AjaxMng == null)
        throw new EBioException("Свойство \"ajaxMng\" должно быть определено!");
      if (String.IsNullOrEmpty(this.BioCode))
        throw new EBioException("Свойство \"bioCode\" должно быть определено!");

      var cancel = false;
      this._doBeforPostData(ref cancel);
      if (cancel) {
        return;
      }

      if (this.BioParams == null)
        this.BioParams = new Params();

      var v_rows = this._getChangesAsJSRows();
      if (v_rows.Count > 0) {
        JsonStoreRequest reqst = new JsonStoreRequestPost {
          BioCode = this.BioCode,
          BioParams = this.BioParams,
          Prms = null,
          transactionID = trunsactionID,
          transactionCmd = cmd,
          Packet = new CJsonStoreData {
            metaData = this._metadata,
            rows = v_rows
          },
          Callback = (sndr, args) => {
            if (args.Response.Success) {
              var rqst = args.Request as JsonStoreRequest;
              var rsp = args.Response as JsonStoreResponse;
              if (rsp != null)
                this._lastReturnedParams = (rsp.BioParams != null) ? rsp.BioParams.Clone() as Params : null;
              if ((rsp != null) && (rsp.packet != null)) {
                this._applyPostingResults(rsp.packet);
              }
              this._clearChanges();
            }
            if (callback != null) callback(this, args);
            this._doAfterPostData(args);
          }
        };
        this.AjaxMng.Request(reqst);
      } else {
        var v_args = new AjaxResponseEventArgs {
          Request = null,
          Response = new AjaxResponse {
            Ex = null,
            Success = true,
            ResponseText = String.Empty
          },
          Stream = null
        };
        if (callback != null) callback(this, v_args);
        this._doAfterPostData(v_args);
      }
    }

    public void Post(AjaxRequestDelegate callback, String trunsactionID, SQLTransactionCmd cmd) {
      this._post(callback, trunsactionID, cmd);
    }
    public void Post(AjaxRequestDelegate callback, String trunsactionID) {
      this._post(callback, trunsactionID, SQLTransactionCmd.Nop);
    }
    public void Post(AjaxRequestDelegate callback) {
      this._post(callback, null, SQLTransactionCmd.Nop);
    }

    public CJsonStoreMetadata JSMetadata {
      get {
        return this._metadata;
      }
    }
  }


}
