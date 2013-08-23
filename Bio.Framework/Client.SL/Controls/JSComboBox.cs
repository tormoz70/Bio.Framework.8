using System;
using System.Windows.Controls;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Collections.Generic;
using Bio.Framework.Packets;
using System.Linq;
using System.Windows.Data;
using Newtonsoft.Json;

namespace Bio.Framework.Client.SL {

  public class JSComboBox : ComboBox {

    private BindingExpression _bE;
    public JSComboBox() {
      this.SelectionChanged += this._selectionChanged;
    }

    private void _selectionChanged(Object sender, SelectionChangedEventArgs e) {
      if (this._bE == null) {
        this._bE = this.GetBindingExpression(SelectedValueProperty);
      } else {
        if (this.GetBindingExpression(SelectedValueProperty) == null) {
          this.SetBinding(SelectedValueProperty, this._bE.ParentBinding);
        }
      }
    }

    private static void _storeItems(String bioCode, CbxItems items) {
      Utl.StoreUserObjectStrg("cbxItems-" + bioCode, items, new JsonConverter[] { new CbxItemsConverter() });
    }

    private static CbxItems _restoreItems(String bioCode) {
      try {
        return Utl.RestoreUserObjectStrg<CbxItems>("cbxItems-" + bioCode, null,
                                                   new JsonConverter[] {new CbxItemsConverter()});
      } catch (Exception) {
        return null;
      }
    }

    private static void _loadItems(ComboBox cbx, CbxItems items, Boolean addNullItem) {
      if ((items != null) && (items.metadata.Fields.Count > 1)) {
        cbx.SelectedValuePath = items.metadata.Fields[0].Name;
        cbx.DisplayMemberPath = items.metadata.Fields[1].Name;
        if (addNullItem) {
          var nullRow = items.NewRow();
          nullRow.SetValue(cbx.SelectedValuePath, null);
          nullRow.SetValue(cbx.DisplayMemberPath, "<не выбрано>");
          ((IList)items.ds).Insert(0, nullRow);
        }
        cbx.ItemsSource = items.ds;
        if (addNullItem)
          cbx.SelectedIndex = 0;
      }
    }

    public static void LoadItems(AjaxResponse prevResponse, IAjaxMng ajaxMng, ComboBox cbx, String bioCode, Params bioParams, Action<ComboBox, AjaxResponse> callback, Boolean addNullItem, Boolean useCache) {
      if ((prevResponse != null) && (!prevResponse.Success)) {
        if (callback != null)
          callback(cbx, prevResponse);
        return;
      }

      var v_cli = new JsonStoreClient {
        AjaxMng = ajaxMng,
        BioCode = bioCode
      };
      CbxItems storedItems = null;
      if (useCache)
        storedItems = _restoreItems(bioCode);
      if (storedItems != null) {
        _loadItems(cbx, storedItems, addNullItem);
        if (callback != null)
          callback(cbx, new AjaxResponse { Success = true });
      } else {
        v_cli.Load(bioParams, (s, a) => {
          if (a.Response.Success) {
            var cbxitems = new CbxItems {metadata = v_cli.JSMetadata, ds = v_cli.DS};
            if (useCache)
              _storeItems(bioCode, cbxitems);
            _loadItems(cbx, cbxitems, addNullItem);
          }
          if (callback != null)
            callback(cbx, a.Response);
        });
      }
    }
    public static void LoadItems(AjaxResponse prevResponse, IAjaxMng ajaxMng, ComboBox cbx, String bioCode, Params bioParams, Action<ComboBox, AjaxResponse> callback, Boolean addNullItem) {
      LoadItems(prevResponse, ajaxMng, cbx, bioCode, bioParams, callback, addNullItem, false);
    }

    public static void LoadItems(AjaxResponse prevResponse, IAjaxMng ajaxMng, ComboBox cbx, String bioCode, Params bioParams, Action<ComboBox, AjaxResponse> callback) {
      LoadItems(prevResponse, ajaxMng, cbx, bioCode, bioParams, callback, false, false);
    }

  }

  public class CbxItems {
    public JsonStoreMetadata metadata = null;
    public IEnumerable<CRTObject> ds = null;

    public CRTObject NewRow() {
      if ((this.ds != null) && (this.ds.First() != null)) {
        var row = TypeFactory.CreateInstance(this.ds.First().GetType(), null, null);
        row.DisableEvents();
        try {
          var intRowUID = Guid.NewGuid().ToString("N");
          row[JsonStoreClient.CS_INTERNAL_ROWUID_FIELD_NAME] = intRowUID;

        } finally {
          row.EnableEvents();
        }
        return row;
      }
      return null;
    }
  }
}
