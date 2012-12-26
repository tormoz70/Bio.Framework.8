using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Reflection;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public abstract class PropertyEditor : ContentControl, IPropertyEditor {

    protected PropertyEditor() { }

    private Brush _labelBackground = null;
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="label">The associated label for this Editor control</param>
    /// <param name="property">The associated PropertyItem for this control</param>
    public PropertyEditor(PropertyGridLabel label, PropertyItem property) {
      this.Label = label;
      this.Property = property;
      this.Property.Editor = this;
    }

    public Boolean Initialized { get; private set; }
    protected virtual void initialize() {
      this.Initialized = true;
      if (this.Label != null) {
        this._labelBackground = this.Label.Background;
        this.Label.Name = "lbl" + this.Property.Name;
        this.Label.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Label_MouseLeftButtonDown);
        this.Label.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Label_MouseLeftButtonUp);
        if (!this.Property.CanWrite)
          this.Label.Foreground = new SolidColorBrush(Colors.Gray);
        if (this.Property.Required)
          this.Label.SetRequiredStyle();

      }
      this.Name = "txt" + this.Property.Name;
      this.BorderThickness = new Thickness(0);
      this.Margin = new Thickness(0);
      this.HorizontalAlignment = HorizontalAlignment.Stretch;
      this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
    }

    public void Initialize() {
      this.initialize();
    }

    protected override void OnGotFocus(RoutedEventArgs e) {
      if (this.Label == null)
        return;

      base.OnGotFocus(e);

    }

    protected override void OnLostFocus(RoutedEventArgs e) {
      if (this.Label == null)
        return;

      base.OnLostFocus(e);

      //if (this.IsSelected)
      //  this.Label.Background = new SolidColorBrush(PropertyGrid.backgroundColor);
      //else
      //  this.Label.Background = this._labelBackground;

      //if (this.Property.CanWrite)
      //  this.Label.Foreground = new SolidColorBrush(Colors.Black);
      //else
      //  this.Label.Foreground = new SolidColorBrush(Colors.Gray);
    }

    private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      e.Handled = true;
    }
    private void Label_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      this.Focus();
    }


    /// <summary>
    /// Gets or sets whether this item is selected
    /// </summary>
    public bool IsSelected {
      get {
        return this._isSelected;
      }
      set {
        if (this._isSelected != value) {
          this._isSelected = value;

          if (this.Label != null) {
            if (value) {
              this.Label.Background = new SolidColorBrush(PropertyGrid.backgroundColorFocused);
              this.Label.Foreground = new SolidColorBrush(Colors.White);
            } else {
              this.Label.Background = this._labelBackground;//new SolidColorBrush(Colors.White);
              if (this.Property.CanWrite)
                this.Label.Foreground = new SolidColorBrush(Colors.Black);
              else
                this.Label.Foreground = new SolidColorBrush(Colors.Gray);
            }
          }
        }
      }
    } 
    private Boolean _isSelected = false;

    #region IPropertyEditor Members
    /// <summary>
    /// Gets the associated label for this Editor control
    /// </summary>
    public PropertyGridLabel Label { get; private set; }
    /// <summary>
    /// Gets the associated PropertyItem for this control
    /// </summary>
    public PropertyItem Property { get; private set; }
    #endregion


#region PropertyEditor Static
    public static PropertyEditor GetEditor(PropertyGridLabel label, PropertyItem propertyItem) {
      if (propertyItem == null) throw new ArgumentNullException("propertyItem");

      PropertyEditor v_editor = null;
      EditorAttribute attribute = propertyItem.GetAttribute<EditorAttribute>();
      if (attribute != null) {
        ConstructorInfo ci = attribute.EditorType.GetConstructor(new Type[] { typeof(PropertyGridLabel), typeof(PropertyItem) });
        v_editor = ci.Invoke(new Object[] { label, propertyItem }) as PropertyEditor;
      }

      if (v_editor == null) {
        Type propertyType = propertyItem.PropertyType;
        v_editor = _getEditorByType(propertyType, label, propertyItem);
        while (v_editor == null && propertyType.BaseType != null) {
          propertyType = propertyType.BaseType;
          v_editor = _getEditorByType(propertyType, label, propertyItem);
        }
      }
      return v_editor;
    }
    private static PropertyEditor _getEditorByType(Type propertyType, PropertyGridLabel label, PropertyItem property) {
      if (typeof(Boolean).IsAssignableFrom(propertyType) || typeof(Boolean?).IsAssignableFrom(propertyType))
        return new BooleanValueEditor(label, property);

      if (typeof(Enum).IsAssignableFrom(propertyType))
        return new EnumValueEditor(label, property);

      if (typeof(DateTime).IsAssignableFrom(propertyType) || typeof(DateTime?).IsAssignableFrom(propertyType))
        return new DateValueEditor(label, property);

      if (typeof(String).IsAssignableFrom(propertyType))
        return new StringValueEditor(label, property);

      if (typeof(ValueType).IsAssignableFrom(propertyType))
        return new StringValueEditor(label, property);

      //if (typeof(Object).IsAssignableFrom(propertyType))
      //    return new PropertyGrid(label, property);

      return new StringValueEditor(label, property);
    }
#endregion
  }
}
