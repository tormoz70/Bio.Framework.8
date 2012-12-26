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
      //this.KeyDown += this._keyDown;
    }

    private void _keyDown(object sender, KeyEventArgs e) {
      //if ((e.Key == Key.Delete) || (e.Key == Key.Back)) {
      //  //if (this.bE != null) {
      //  //  Utl.SetPropertyValue(this.bE.DataItem, this.bE.ParentBinding.Path.Path, null);
      //  //} else
      //  this.SelectedIndex = -1;

      //}
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

    public static void LoadItems(IAjaxMng ajaxMng, ComboBox cbx, String bioCode, CParams bioParams, Action<ComboBox> callback, Boolean addNullItem) {
      var v_cli = new CJsonStoreClient {
        ajaxMng = ajaxMng,
        bioCode = bioCode
      };
      v_cli.Load(bioParams, (s, a) => {
        if (a.response.success) {
          //var v_rsp = a.response as CJsonStoreResponse;
          if (v_cli.jsMetadata.fields.Count > 1) {
            cbx.SelectedValuePath = v_cli.jsMetadata.fields[0].name;
            cbx.DisplayMemberPath = v_cli.jsMetadata.fields[1].name;
            if (addNullItem) {
              var v_NullRow = v_cli.NewRow();
              v_NullRow.SetValue(cbx.SelectedValuePath, null);
              v_NullRow.SetValue(cbx.DisplayMemberPath, "<не выбрано>");
              v_cli.InsertRow(0, v_NullRow);
            }
            cbx.ItemsSource = v_cli.DS;
            if (addNullItem) 
              cbx.SelectedIndex = 0;
          }
          if (callback != null)
            callback(cbx);
        }
      });
    }
    public static void LoadItems(IAjaxMng ajaxMng, ComboBox cbx, String bioCode, CParams bioParams, Action<ComboBox> callback) {
      LoadItems(ajaxMng, cbx, bioCode, bioParams, callback, false);
    }

  }
}
