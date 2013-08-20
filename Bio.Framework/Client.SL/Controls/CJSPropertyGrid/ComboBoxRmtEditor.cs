namespace Bio.Framework.Client.SL.JSPropertyGrid {
  using Helpers.Common;
  using System;
  using SL;
  using Packets;
  using System.Collections.Generic;
  using Helpers.Controls.SL.SLPropertyGrid;

  /// <summary>
  /// An editor for a Boolean Type
  /// </summary>
  public class ComboBoxRmtEditor : ComboBoxEditorBase {
    private readonly ComboBoxRmtEditorAttribute _attrs;
    public ComboBoxRmtEditor(PropertyGridLabel label, PropertyItem property, ComboBoxRmtEditorAttribute attrs)
      : base(label, property) {
      this._attrs = attrs ?? this.Property.GetAttribute<ComboBoxRmtEditorAttribute>();
    }

    public ComboBoxRmtEditor(PropertyGridLabel label, PropertyItem property):this(label, property, null) { }

    protected override Boolean setCboSelectedItem(Object value) {
      var rslt = base.setCboSelectedItem(value);
      if (!rslt) {
        if (this._attrs == null)
          throw new Exception(String.Format("Для {0} необходимо описать {1}.", this.GetType().Name, typeof(ComboBoxRmtEditorAttribute).Name));
        var v_cli = new JsonStoreClient {
          AjaxMng = BioEnvironment.Instance.AjaxMng,
          BioCode = this._attrs.BioCode
        };
        this.cbo.Items.Clear();
        this.cbo.Items.Add(new EnumWrapper {
          Name = "загрузка...",
          Value = 0
        });
        this.cbo.SelectedValue = 0;
        v_cli.Load(null, (s, a) => {
          if (a.Response.Success) {
            var rsp = a.Response as JsonStoreResponse;
            if (rsp != null) {
              var values = new List<Object>();
              foreach (var r in ((JsonStoreClient) s).DS) {
                values.Add(new EnumWrapper {
                  Value = r.GetValue(this._attrs.ValueFieldName),
                  Name = r.GetValue<String>(this._attrs.DisplayFieldName)
                });
              }
              this.LoadItems(values);
              base.setCboSelectedItem(value);
            }
          } else
            this.cbo.Items.Clear();
        });

      }
      return rslt;
    }

  }
}
