using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bio.Helpers.Common.Types;
using System.ComponentModel;
using System.Threading;
using Bio.Framework.Client.SL.JSPropertyGrid;
using Bio.Helpers.Common;
using PropertyMetadata = System.Windows.PropertyMetadata;

namespace Bio.Framework.Client.SL {
  public class CVSelectorControl : ASelectorValueControl {


    public CVSelectorControl() {
      this.DefaultStyleKey = typeof(CVSelectorControl);
    }

    protected override void _onSelectionChanged() {
      this._txt.Text = this._selection.Display;
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      if (!Utl.DesignTime) {
        var view = Utl.FindParentElem1<IPluginComponent>(this);
        if (view == null) {
          throw new EBioException("[view] not found. Parent form mast implement IPluginComponent interface!");
        }
        this._ownerPlugin = (view as IPluginComponent).ownerPlugin;
        //this._selection = new VSingleSelection();

        this._txt = this.GetTemplateChild("tbxText") as TextBox;
        if (this._txt != null) {
          this._txt.KeyDown += new KeyEventHandler((s, e) => {
            if ((e.Key == Key.Delete) || (e.Key == Key.Back)) {
              this.Value = null;
            }
          });
        }

        var btn = this.GetTemplateChild("btnSelect") as Button;
        if (btn != null)
          btn.Click += new RoutedEventHandler((s, a) => {
            this._showSelector();
          });

        this._get_selector((selector) => {
          if (this.IsMultiselector) {
            this._selection = new VMultiSelection {
              Value = this.Value
            };
          } else {
            this._selection = new VSingleSelection {
              ValueField = this.ValueField,
              Value = this.Value,
              DisplayField = this.DisplayField,
              Display = null
            };
          }
          this._txt.Text = VSelection.csNotSeldText;
          if (!this._selection.IsEmpty())
            this._setSelection(this._selection);
        });

      }
    }

    public static DependencyProperty SelectorPluginProperty = DependencyProperty.Register("SelectorPlugin", typeof(String), typeof(CVSelectorControl), new PropertyMetadata(String.Empty));
    public override String SelectorPlugin {
      get { return (String)this.GetValue(SelectorPluginProperty); }
      set { this.SetValue(SelectorPluginProperty, value); }
    }

    public static DependencyProperty IsMultiselectorProperty = DependencyProperty.Register("IsMultiselector", typeof(Boolean), typeof(CVSelectorControl), new PropertyMetadata(false));
    public override Boolean IsMultiselector {
      get { return (Boolean)this.GetValue(IsMultiselectorProperty); }
      set { this.SetValue(IsMultiselectorProperty, value); }
    }

    public static DependencyProperty ValueFieldProperty = DependencyProperty.Register("ValueField", typeof(String), typeof(CVSelectorControl), new PropertyMetadata(String.Empty));
    public override String ValueField {
      get { return (String)this.GetValue(ValueFieldProperty); }
      set {
        this.SetValue(ValueFieldProperty, value);
        if (this._selection != null)
          this._selection.ValueField = value;
      }
    }

    public static DependencyProperty DisplayFieldProperty = DependencyProperty.Register("DisplayField", typeof(String), typeof(CVSelectorControl), new PropertyMetadata(String.Empty));
    public override String DisplayField {
      get { return (String)this.GetValue(DisplayFieldProperty); }
      set {
        this.SetValue(DisplayFieldProperty, value);
        if (this._selection != null)
          this._selection.DisplayField = value;
      }
    }

    public static DependencyProperty OwnerPluginProperty = DependencyProperty.Register("OwnerPlugin", typeof(IPlugin), typeof(CVSelectorControl), new PropertyMetadata(default(IPlugin)));
    public IPlugin OwnerPlugin {
      get { return (IPlugin)this.GetValue(OwnerPluginProperty); }
      set { this.SetValue(OwnerPluginProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Object), typeof(CVSelectorControl), new PropertyMetadata(default(Object), ValuePropertyChangedCallback));
    public override Object Value {
      get {
        return this.GetValue(ValueProperty);
      }
      set {
        this.SetValue(ValueProperty, value);
      }
    }

    private Boolean _valuePropertyChangedEnabled = true;
    private void _disablePropertyChangedEvent() { this._valuePropertyChangedEnabled = false; }
    private void _enablePropertyChangedEvent() { this._valuePropertyChangedEnabled = true; }
    internal void _doOnValuePropertyChanged(DependencyPropertyChangedEventArgs e) {
      if (this._valuePropertyChangedEnabled) {
        this._selection.Value = e.NewValue;
        this._setSelection(this._selection);
      }
    }

    private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((CVSelectorControl)d)._doOnValuePropertyChanged(e);
    }

  }

}
