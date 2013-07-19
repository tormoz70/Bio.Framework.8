
namespace Bio.Framework.Client.SL.JSPropertyGrid {
  #region Using Directives
  using System.Windows.Controls;
  using Bio.Helpers.Common;
  using System;
  using Bio.Framework.Client.SL;
  using Bio.Framework.Packets;
  using System.Collections.Generic;
  using Bio.Helpers.Controls.SL.SLPropertyGrid;
  using System.Collections.ObjectModel;

  #endregion

  #region ComboBoxEditor
  /// <summary>
  /// An editor for a Boolean Type
  /// </summary>
  public class ComboBoxRmtEditor : ComboBoxEditorBase {
    private ComboBoxRmtEditorAttribute _attrs = null;
    public ComboBoxRmtEditor(PropertyGridLabel label, PropertyItem property, ComboBoxRmtEditorAttribute attrs)
      : base(label, property) {
      this._attrs = attrs ?? this.Property.GetAttribute<ComboBoxRmtEditorAttribute>();
    }

    protected override void initialize() {
      base.initialize();
    }

    public ComboBoxRmtEditor(PropertyGridLabel label, PropertyItem property):this(label, property, null) { }

    protected override Boolean setCboSelectedItem(Object value) {
      Boolean rslt = base.setCboSelectedItem(value);
      if (!rslt) {
        if (this._attrs == null)
          throw new Exception(String.Format("Для {0} необходимо описать {1}.", this.GetType().Name, typeof(ComboBoxRmtEditorAttribute).Name));
        var v_owner = this.Property.Owner as CJSPropertyGrid;
        if(v_owner == null)
          throw new Exception(String.Format("{0} можно использовать только в {1}.", this.GetType().Name, typeof(CJSPropertyGrid).Name));
        var v_cli = new JsonStoreClient() {
          AjaxMng = v_owner.OwnerPlugin.Env.AjaxMng,
          BioCode = this._attrs.BioCode
        };
        this.cbo.Items.Clear();
        this.cbo.Items.Add(new EnumWrapper {
          Name = "загрузка...",
          Value = 0
        });
        this.cbo.SelectedValue = 0;
        v_cli.Load(null, (s, a) => {
          //this.OwnerTreeView.Dispatcher.BeginInvoke(() => {
          if (a.Response.Success) {
            JsonStoreResponse rsp = a.Response as JsonStoreResponse;
            if (rsp != null) {
              //this.Items.Clear();
              List<Object> values = new List<Object>();
              foreach (var r in rsp.packet.rows) {
                values.Add(new EnumWrapper {
                  Value = rsp.packet.getValue(r, this._attrs.ValueFieldName),
                  Name = rsp.packet.getValue<String>(r, this._attrs.DisplayFieldName)
                });
              }
              //this.Loaded = true;
              this.LoadItems(values);
              base.setCboSelectedItem(value);
            }
          } else
            this.cbo.Items.Clear();
          //if (callback != null) callback(s, a);
          //});
        });

      }
      return rslt;
    }

  }
  #endregion
}
