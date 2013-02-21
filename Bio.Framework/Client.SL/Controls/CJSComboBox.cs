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

namespace Bio.Framework.Client.SL {

  public class CJSComboBox : ComboBox {

    private BindingExpression bE;
    public CJSComboBox() {
      this.SelectionChanged += this._selectionChanged;
    }

    private void _selectionChanged(Object sender, SelectionChangedEventArgs e) {
      if (this.bE == null) {
        this.bE = this.GetBindingExpression(ComboBox.SelectedValueProperty);
      } else {
        if (this.GetBindingExpression(ComboBox.SelectedValueProperty) == null) {
          this.SetBinding(ComboBox.SelectedValueProperty, this.bE.ParentBinding);
        }
      }
    }

    private static void _storeItems(String bioCode, CbxItems items) {
      //Utl.StoreUserObjectStrg("cbxItems-" + bioCode, items);
    }

    private static CbxItems _restoreItems(String bioCode) {
      //return Utl.RestoreUserObjectStrg<CbxItems>("cbxItems-" + bioCode, null);
      return null;
    }

    private static void _loadItems(ComboBox cbx, CbxItems items, Boolean addNullItem) {
      if ((items != null) && (items.metadata.fields.Count > 1)) {
        cbx.SelectedValuePath = items.metadata.fields[0].name;
        cbx.DisplayMemberPath = items.metadata.fields[1].name;
        if (addNullItem) {
          var v_NullRow = items.NewRow();
          v_NullRow.SetValue(cbx.SelectedValuePath, null);
          v_NullRow.SetValue(cbx.DisplayMemberPath, "<не выбрано>");
          (items.ds as IList).Insert(0, v_NullRow);
        }
        cbx.ItemsSource = items.ds;
        if (addNullItem)
          cbx.SelectedIndex = 0;
      }
    }

    public static void LoadItems(CAjaxResponse prevResponse, IAjaxMng ajaxMng, ComboBox cbx, String bioCode, Params bioParams, Action<ComboBox, CAjaxResponse> callback, Boolean addNullItem, Boolean useCache) {
      if ((prevResponse != null) && (prevResponse.success == false)) {
        if (callback != null)
          callback(cbx, prevResponse);
        return;
      }

      var v_cli = new CJsonStoreClient {
        ajaxMng = ajaxMng,
        bioCode = bioCode
      };
      CbxItems storedItems = null;
      if (useCache)
        storedItems = _restoreItems(bioCode);
      if (storedItems != null) {
        _loadItems(cbx, storedItems, addNullItem);
        if (callback != null)
          callback(cbx, new CAjaxResponse { success = true });
      } else {
        v_cli.Load(bioParams, (s, a) => {
          if (a.response.success) {
            var cbxitems = new CbxItems {metadata = v_cli.jsMetadata, ds = v_cli.DS};
            if (useCache)
              _storeItems(bioCode, cbxitems);
            _loadItems(cbx, cbxitems, addNullItem);
          }
          if (callback != null)
            callback(cbx, a.response);
        });
      }
    }
    public static void LoadItems(CAjaxResponse prevResponse, IAjaxMng ajaxMng, ComboBox cbx, String bioCode, Params bioParams, Action<ComboBox, CAjaxResponse> callback, Boolean addNullItem) {
      LoadItems(prevResponse, ajaxMng, cbx, bioCode, bioParams, callback, addNullItem, false);
    }

    public static void LoadItems(CAjaxResponse prevResponse, IAjaxMng ajaxMng, ComboBox cbx, String bioCode, Params bioParams, Action<ComboBox, CAjaxResponse> callback) {
      LoadItems(prevResponse, ajaxMng, cbx, bioCode, bioParams, callback, false, false);
    }

  }

  public class CbxItems {
    public CJsonStoreMetadata metadata = null;
    public IEnumerable<CRTObject> ds = null;

    public CRTObject NewRow() {
      if ((this.ds != null) && (this.ds.First() != null)) {
        var row = CTypeFactory.CreateInstance(this.ds.First().GetType(), null, null);
        row.DisableEvents();
        try {
          String v_intRowUID = Guid.NewGuid().ToString("N");
          if (row != null) {
            row[CJsonStoreClient.csInternalROWUID_FieldName] = v_intRowUID;
          }

        } finally {
          row.EnableEvents();
        }
        return row;
      } else
        return null;
    }

  }
}
