
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System;
  using System.ComponentModel;
  using System.Linq;
  using System.Reflection;
  using Converters;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.Common;
  using System.Windows.Media;
  #endregion

  #region PropertyItem
  /// <summary>
  /// PropertyItem hold a reference to an individual property in the propertygrid
  /// </summary>
  public sealed class PropertyItem : INotifyPropertyChanged {
    #region Events
    /// <summary>
    /// Event raised when an error is encountered attempting to set the Value
    /// </summary>
    public event EventHandler<ExceptionEventArgs> ValueError;
    /// <summary>
    /// Raises the ValueError event
    /// </summary>
    /// <param name="ex">The exception</param>
    private void OnValueError(Exception ex) {
      var eve = this.ValueError;
      if (eve != null)
        eve(this, new ExceptionEventArgs(ex));
      else
        throw ex;
    }
    #endregion

    #region Fields
    private PropertyGrid _owner;
    private PropertyInfo _propertyInfo;
    private Object _instance;
    private Boolean _readOnly = false;
    private Boolean _required = false;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="instance"></param>
    /// <param name="value"></param>
    /// <param name="property"></param>
    /// <param name="readOnly"></param>
    /// <param name="required"></param>
    public PropertyItem(PropertyGrid owner, Object instance, Object value, PropertyInfo property, Boolean readOnly, Boolean required) {
      this._owner = owner;
      this._instance = instance;
      this._propertyInfo = property;
      this._value = value;
      this._readOnly = readOnly;
      this._required = required;

      var attr1 = PropertyItem.GetAttribute<HeaderContentAttribute>(this._propertyInfo);
      this.DisplayName = (attr1 != null) ? attr1.Text : Name;

      var attr2 = PropertyItem.GetAttribute<HeaderHideAttribute>(this._propertyInfo);
      this.Hidden = (attr2 != null);

      if (this._instance is INotifyPropertyChanged)
        ((INotifyPropertyChanged)this._instance).PropertyChanged += new PropertyChangedEventHandler(this.PropertyItem_PropertyChanged);
    }


    void PropertyItem_PropertyChanged(Object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == this.Name)
        Value = _propertyInfo.GetValue(this._instance, null);
    }
    #endregion

    #region Properties

    public PropertyGrid Owner { get { return this._owner; } }

    public String Name {
      get { return this._propertyInfo.Name; }
    }

    public Boolean Hidden { get; private set; }
    public String DisplayName { get; private set; }

    private String _category;
    public String Category {
      get {
        if (String.IsNullOrEmpty(this._category)) {
          CategoryAttribute attr = GetAttribute<CategoryAttribute>(this._propertyInfo);
          if (attr != null && !string.IsNullOrEmpty(attr.Category))
            this._category = attr.Category;
          else
            this._category = String.Empty;
        }
        return this._category;
      }
    }

    public event EventHandler<PropertyChangingEventArgsEx> PropertyChanging;
    private void _doOnPropertyChanging(ref Object newValue) {
      var eve = this.PropertyChanging;
      if (eve != null) {
        var v_args = new PropertyChangingEventArgsEx(false, this.Name, this.Value, newValue);
        eve(this, v_args);
        if (v_args.Cancel) {
          throw new MethodAccessException(v_args.ReasonForCancel);
        } else {
          newValue = v_args.NewValue;
        }
      }
    }

    //public CRTObject ValueRow { 
    //  get {
    //    if (this.Value is VSingleSelection)
    //      return (this.Value as VSingleSelection).ValueRow;
    //    else
    //      return null;
    //  }
    //}
    public CRTObject ValueRow { get; private set;}
    public void SetSelection(VSelection selection) {
      if (selection != null) {
        if (selection is VSingleSelection) {
          this.ValueRow = (selection as VSingleSelection).ValueRow;
          this.Value = (selection as VSingleSelection).Value;
        } else {
          this.Value = selection; //(selection as VMultiSelection).ToString();
        }
      }
      //this.Value = selection;
    }
    // TODO Тут надо все переделывать
    private Object _value;
    public Object Value {
      get { return this._value; }
      set {
        //Object v_value = null;
        //var v_selection = value as VSelection;
        //if (v_selection != null) {
        //  if (v_selection is VSingleSelection)
        //    v_value = ((VSingleSelection)v_selection).ValueRow[v_selection.ValueField];
        //  else
        //    v_value = v_selection.Value;
        //} else
        Object v_value = value;

        if (this._value == v_value) 
          return;
        Object v_originalValue = this._value;
        try {
          //Object v_val = v_value;
          this._doOnPropertyChanging(ref v_value);
          this._value = v_value;
          Type v_propertyType = this._propertyInfo.PropertyType;
          if (((v_propertyType == typeof(Object)) || ((this._value == null) && v_propertyType.IsClass)) || ((this._value != null) && v_propertyType.IsAssignableFrom(this._value.GetType()))) {
            this._propertyInfo.SetValue(this._instance, this._value, (BindingFlags.NonPublic | BindingFlags.Public), null, null, null);
            this.OnPropertyChanged("Value");
          } else {
            try {
              if (v_propertyType.IsEnum) {
                Object val = Enum.Parse(this._propertyInfo.PropertyType, v_value.ToString(), false);
                this._propertyInfo.SetValue(this._instance, val, (BindingFlags.NonPublic | BindingFlags.Public), null, null, null);
                this.OnPropertyChanged("Value");
              } else {
                //Object v_val_value = (this._value is VSingleSelection) ? ((VSingleSelection)this._value).ValueRow[((VSingleSelection)this._value).ValueField] : this._value;
                Object v_convertedValue = Utl.Convert2Type(this._value, v_propertyType);
                this._propertyInfo.SetValue(this._instance, v_convertedValue, (BindingFlags.NonPublic | BindingFlags.Public), null, null, null);
              }
            } catch (Exception ex) {
              var v_bad_value = this._value;
              this._value = v_originalValue;
              this.OnPropertyChanged("Value");
              var vex =  new Exception(String.Format("Для свойства [{0}] типа [{1}] не может быть установлено значение ({2}) типа [{3}]",
                this.DisplayName, v_propertyType.ToString(), "" + (v_bad_value ?? "null"), (v_bad_value != null ? v_bad_value.GetType().ToString() : "null")), ex);
              this.OnValueError(vex);
            }
          }
        } catch (MethodAccessException mex) {
          this._value = v_originalValue;
          this._readOnly = true;
          this.OnPropertyChanged("Value");
          this.OnValueError(mex);
        }
      }
    } 

    public Type PropertyType {
      get { return this._propertyInfo.PropertyType; }
    }

    public Boolean CanWrite {
      get { return this._propertyInfo.CanWrite && !this._readOnly; }
    }

    public Boolean Required {
      get { return this._required; }
      set {
        this._required = value;
        if (this._required)
          this.Editor.Label.SetRequiredStyle();
        else
          this.Editor.Label.ResetRequiredStyle();

      }
    }

    public Boolean ReadOnly {
      get { return this._readOnly; }
      set { 
        this._readOnly = value;
        this.OnPropertyChanged("CanWrite");
        if (!this.CanWrite) 
          this.Editor.Label.SetForeground(new SolidColorBrush(Colors.DarkGray));
        else
          this.Editor.Label.ResetForeground();
        
      }
    }

    public PropertyEditor Editor { get; internal set; }

    #endregion

    #region Helpers
    public static T GetAttribute<T>(PropertyInfo propertyInfo) {
      var attributes = propertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
      return attributes.FirstOrDefault();
    }
    public T GetAttribute<T>() {
      return GetAttribute<T>(this._propertyInfo);
    }
    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(String propertyName) {
      if (String.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
      var handler = this.PropertyChanged;
      if (handler != null) 
        handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
  #endregion
}
