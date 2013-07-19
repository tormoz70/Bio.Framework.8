using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;

namespace Bio.Helpers.Common.Types {
  public class PropertyMetadata {
    public String Name { get; set; }
    public String DisplayName { get; set; }
    public Type Type { get; set; }
    public Boolean Hidden { get; set; }
    public String Group { get; set; }
    public Boolean? Readonly { get; set; }
    public Boolean? Required { get; set; }
  }

  public class TypeFactory {
    private readonly Dictionary<String, Type> _typeBySigniture = new Dictionary<String, Type>();
    private String _getTypeSigniture(List<PropertyMetadata> propDefs) {
      var sb = new StringBuilder();
      foreach (var propDef in propDefs) {
        sb.AppendFormat("_{0}_{1}", propDef.Name, propDef.Type);
      }
      return sb.ToString().GetHashCode().ToString().Replace("-", "Minus");
    }
    private Type _getTypeByTypeSigniture(String typeSigniture) {
      Type type;
      return this._typeBySigniture.TryGetValue(typeSigniture, out type) ? type : null;
    }

    private TypeBuilder _getTypeBuilder(String typeSigniture, Type baseType) {
      var an = new AssemblyName("TempAssembly" + typeSigniture);
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
      var tb = moduleBuilder.DefineType("TempType" + typeSigniture
                          , TypeAttributes.Public |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout
                          , baseType);
      return tb;
    }

    private static void _createProperty(TypeBuilder tb, PropertyMetadata propDef) {
      var propertyType = propDef.Type;
      if (propertyType.IsValueType && !propertyType.IsGenericType) {
        propertyType = typeof(Nullable<>).MakeGenericType(new[] { propertyType });
      }
      var propertyName = propDef.Name;
      var propertyHidden = propDef.Hidden;
      var v_header = !String.IsNullOrEmpty(propDef.DisplayName) ? propDef.DisplayName : propDef.Name;
      if(String.Equals(v_header, "Empty", StringComparison.CurrentCultureIgnoreCase))
        v_header = String.Empty;
      var propertyHeaderContent = v_header;
      //String groupAggr = !String.IsNullOrEmpty(propDef.Group) ? propDef.Group : "None";

      var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Public);
      var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

      var cabuilder0 = new CustomAttributeBuilder(
        typeof(CategoryAttribute).GetConstructor(new[] { typeof(String) }),
        new Object[] { propDef.Group });
      propertyBuilder.SetCustomAttribute(cabuilder0);

      var cabuilder1 = new CustomAttributeBuilder(
        typeof(HeaderContentAttribute).GetConstructor(new[] { typeof(String) }),
        new Object[] { propertyHeaderContent });
      propertyBuilder.SetCustomAttribute(cabuilder1);

      if (propertyHidden) {
        var cabuilder2 = new CustomAttributeBuilder(
          typeof(BrowsableAttribute).GetConstructor(new[] { typeof(Boolean) }),
          new Object[] { false });
        propertyBuilder.SetCustomAttribute(cabuilder2);
      }

      var displayAttribute = typeof(DisplayAttribute);
      var info1 = displayAttribute.GetProperty("Name");
      var info2 = displayAttribute.GetProperty("AutoGenerateField");
      //PropertyInfo info3 = displayAttribute.GetProperty("Order");
      //PropertyInfo info4 = displayAttribute.GetProperty("GroupName");
      var cabuilder3 = new CustomAttributeBuilder(
          displayAttribute.GetConstructor(new Type[] { }),
          new object[] { },
          new[] { info1, info2/*, info3, info4 */},
          new object[] { propertyHeaderContent, !propertyHidden/*, order, groupAggr */});
      propertyBuilder.SetCustomAttribute(cabuilder3);

      if (propDef.Readonly != null) {
        if (propDef.Readonly == true) {
          var v_cabuilder = new CustomAttributeBuilder(
            typeof(ReadOnlyAttribute).GetConstructor(new[] { typeof(Boolean) }),
            new Object[] { true });
          propertyBuilder.SetCustomAttribute(v_cabuilder);
        }
      }

      if (propDef.Required != null) {
        if (propDef.Required == true) {
          var v_cabuilder = new CustomAttributeBuilder(
            typeof(RequiredAttribute).GetConstructor(new[] { typeof(Boolean) }),
            new Object[] { true });
          propertyBuilder.SetCustomAttribute(v_cabuilder);
        }
      }

      MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName,
              MethodAttributes.Public |
              MethodAttributes.SpecialName |
              MethodAttributes.HideBySig,
              propertyType, Type.EmptyTypes);

      var getIL = getPropMthdBldr.GetILGenerator();

      getIL.Emit(OpCodes.Ldarg_0);
      getIL.Emit(OpCodes.Ldfld, fieldBuilder);
      getIL.Emit(OpCodes.Ret);

      var setPropMthdBldr =
          tb.DefineMethod("set_" + propertyName,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            null, new[] { propertyType });

      //var v_valueChangedMethod = tb.BaseType.GetMethod("valueChanged");
      var setPropertyValueMethod = tb.BaseType.GetMethod("SetPropertyValue");
      //System.Reflection.Emit.Label lbl1 = default(System.Reflection.Emit.Label);
      var setIL = setPropMthdBldr.GetILGenerator();

      /*setIL.DeclareLocal(typeof(bool));
      setIL.Emit(OpCodes.Nop);
      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldfld, fieldBuilder);
      setIL.Emit(OpCodes.Box, propertyType);
      setIL.Emit(OpCodes.Ldarg_1);
      setIL.Emit(OpCodes.Box, propertyType);
      setIL.Emit(OpCodes.Call, v_valueChangedMethod);
      setIL.Emit(OpCodes.Ldc_I4, 0);
      setIL.Emit(OpCodes.Ceq);
      setIL.Emit(OpCodes.Stloc_0);
      setIL.Emit(OpCodes.Ldloc_0);
      lbl1 = setIL.DefineLabel();
      setIL.Emit(OpCodes.Brtrue_S, lbl1);
      
      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldarg_1);
      setIL.Emit(OpCodes.Stfld, fieldBuilder);
      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldstr, propertyName);
      setIL.Emit(OpCodes.Call, v_doOnPropChangedMethod);

      setIL.MarkLabel(lbl1);
      setIL.Emit(OpCodes.Ret);
       */

      setIL.Emit(OpCodes.Nop);
      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldfld, fieldBuilder);
      setIL.Emit(OpCodes.Box, propertyType);
      setIL.Emit(OpCodes.Ldarg_1);
      setIL.Emit(OpCodes.Box, propertyType);
      setIL.Emit(OpCodes.Ldstr, propertyName);
      setIL.Emit(OpCodes.Call, setPropertyValueMethod);
      setIL.Emit(OpCodes.Nop);
      setIL.Emit(OpCodes.Ret);

      propertyBuilder.SetGetMethod(getPropMthdBldr);
      propertyBuilder.SetSetMethod(setPropMthdBldr);
    }


    public Type CreateType(List<PropertyMetadata> propDefs) {
      var baseType = typeof(CRTObject);
      var baseCtor0 = baseType.GetConstructor(new Type[0]);
      var baseCtor1 = baseType.GetConstructor(new[] { typeof(PropertyChangingEventHandler), typeof(PropertyChangedEventHandler) });
      var typeSigniture = this._getTypeSigniture(propDefs);
      var objectType = this._getTypeByTypeSigniture(typeSigniture);
      if (objectType == null) {
        var tb = this._getTypeBuilder(typeSigniture, baseType);

        var constructorBuilder =
                    tb.DefineConstructor(
                                MethodAttributes.Public, CallingConventions.Standard, null);
        var constructorIL = constructorBuilder.GetILGenerator();
        constructorIL.Emit(OpCodes.Ldarg_0);
        constructorIL.Emit(OpCodes.Call, baseCtor0);
        constructorIL.Emit(OpCodes.Ret);

        constructorBuilder = tb.DefineConstructor(
                                MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(PropertyChangingEventHandler), typeof(PropertyChangedEventHandler) });

        constructorIL = constructorBuilder.GetILGenerator();
        constructorIL.Emit(OpCodes.Ldarg_0);
        constructorIL.Emit(OpCodes.Ldarg_1);
        constructorIL.Emit(OpCodes.Ldarg_2);
        constructorIL.Emit(OpCodes.Call, baseCtor1);
        constructorIL.Emit(OpCodes.Ret);


        foreach (var propDef in propDefs) {
          _createProperty(tb, propDef);
        }
        objectType = tb.CreateType();
        this._typeBySigniture.Add(typeSigniture, objectType);
      }
      return objectType;
    }

    public static CRTObject CreateInstance(Type type, PropertyChangingEventHandler onPropertyChanging, PropertyChangedEventHandler onPropertyChanged) {
      var constructor = type.GetConstructor(new[] { typeof(PropertyChangingEventHandler), typeof(PropertyChangedEventHandler) });
      if (constructor != null)
        return constructor.Invoke(new Object[] { onPropertyChanging, onPropertyChanged }) as CRTObject;
      return null;
    }
    public static CRTObject CreateInstance(Type type) {
      var constructor = type.GetConstructor(new Type[0]);
      if (constructor != null)
        return constructor.Invoke(new Object[0]) as CRTObject;
      return null;
    }

    /// <summary>
    /// Возвращает свойство типа по имени
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    public static PropertyInfo FindPropertyOfType(Type type, String propertyName, StringComparison comparison) {
      return type.GetProperties().FirstOrDefault(p => p.Name.Equals(propertyName, comparison));
    }
    /// <summary>
    /// Возвращает свойство типа по имени без учера регистра
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo FindPropertyOfType(Type type, String propertyName) {
      return FindPropertyOfType(type, propertyName, StringComparison.CurrentCultureIgnoreCase);
    }
    /// <summary>
    /// Возвращает свойство объекта по имени
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    public static PropertyInfo FindPropertyOfObject(Object obj, String propertyName, StringComparison comparison) {
      return (obj != null) ? FindPropertyOfType(obj.GetType(), propertyName, comparison) : null;
    }
    /// <summary>
    /// Возвращает свойство объекта по имени без учера регистра
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo FindPropertyOfObject(Object obj, String propertyName) {
      return FindPropertyOfObject(obj, propertyName, StringComparison.CurrentCultureIgnoreCase);
    }

    public static Object GetValueOfPropertyOfObject(Object obj, String propertyName, StringComparison comparison) {
      var v_pi = FindPropertyOfObject(obj, propertyName, comparison);
      return v_pi != null ? v_pi.GetValue(obj, null) : null;
    }
    public static Object GetValueOfPropertyOfObject(Object obj, String propertyName) {
      return GetValueOfPropertyOfObject(obj, propertyName, StringComparison.CurrentCultureIgnoreCase);
    }

    public static void SetValueOfPropertyOfObject(Object obj, String propertyName, Object value) {
      var v_pi = FindPropertyOfObject(obj, propertyName, StringComparison.CurrentCultureIgnoreCase);
      if (v_pi != null) {
        var v_value = Utl.Convert2Type(value, v_pi.PropertyType);
        v_pi.SetValue(obj, v_value, null);
      }
    }

  }


  public class PropertyChangingEventArgsEx : PropertyChangingEventArgs {
    //public String PropertyName { get; private set; }
    public Object OldValue { get; private set; }
    public Object NewValue { get; set; }
    public Boolean Cancel { get; set; }
    public String ReasonForCancel { get; set; }
    public PropertyChangingEventArgsEx(Boolean cancel, String propertyName, Object oldValue, Object newValue)
      : base(propertyName) {
      this.Cancel = cancel;
      this.OldValue = oldValue;
      this.NewValue = newValue;
    }
  }
  //CATypeBase
  //public delegate void PropertyChangingEventHandlerEx(Object sender, PropertyChangingEventArgsEx args);
  public class CRTObject : INotifyPropertyChanged {
    public event PropertyChangingEventHandler OnPropertyChanging;
    public event PropertyChangedEventHandler OnPropertyChanged;
    #region INotifyPropertyChanged Members
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion
    public CRTObject() {
        this.OnPropertyChanging = null;
        this.OnPropertyChanged = null;
    }
    public CRTObject(PropertyChangingEventHandler onChanging, PropertyChangedEventHandler onChanged) {
        this.OnPropertyChanging += onChanging;
        this.OnPropertyChanged += onChanged;
    }
    
    private bool _valueChanged(Object oldValue, Object newValue) {
      return !Equals(oldValue, newValue);
    }

    private void _internalSetProperty(String propertyName, Object value) {
      var fldName = "_" + propertyName;
      var thisType = this.GetType();
      // для private - полей
      //var fldInfos = v_thisType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      //var fldInfo = fldInfos.FirstOrDefault((f) => { 
      //  return f.Name.Equals(fldName); 
      //});
      var fldInfo = thisType.GetField(fldName);
      if (fldInfo != null) {
        fldInfo.SetValue(this, value);
        var eve1 = this.PropertyChanged;
        if (eve1 != null)
          eve1(this, new PropertyChangedEventArgs(propertyName));

      }
    }

    public CRTObject Copy() { 
      var thisType = this.GetType();
      var constr = thisType.GetConstructor(new Type[0]);
      if (constr != null) {
        var rslt = constr.Invoke(new Object[0]) as CRTObject;
        var fldInfos = thisType.GetFields();
        foreach (var fi in fldInfos) {
          fi.SetValue(rslt, fi.GetValue(this));
        }
        return rslt;
      }
      return null;
    }


    private Boolean _eventsDisabled;
    public void DisableEvents() { _eventsDisabled = true; }
    public void EnableEvents() { _eventsDisabled = false; }
    public void SetPropertyValue(Object oldValue, Object newValue, String propertyName) {
      if (this._eventsDisabled)
        this._internalSetProperty(propertyName, newValue);
      else {
        if (this._valueChanged(oldValue, newValue)) {
          var args0 = new PropertyChangingEventArgsEx(false, propertyName, oldValue, newValue);
          var eve0 = this.OnPropertyChanging;
          if (eve0 != null)
            eve0(this, args0);
          if (!args0.Cancel) {
            this._internalSetProperty(propertyName, args0.NewValue);
            var eve1 = this.OnPropertyChanged;
            if (eve1 != null)
              eve1(this, new PropertyChangedEventArgs(propertyName));
          }
        }
      }
    }

    public void SetValue(String propertyName, Object value) {
      var oldVal = this.GetValue<Object>(propertyName);
      this.SetPropertyValue(oldVal, value, propertyName);
    }

    public void SetValues(Object obj) {
      if ((obj != null) && obj.GetType().IsClass) {
        var props = this.GetType().GetProperties();
        foreach (var prop in props) {
          if (prop.CanWrite) {
            try {
              var value = Utl.GetPropertyValue(obj, prop.Name);
              prop.SetValue(this, value, null);
            } catch { }
          }
        }
      }
    }

    public Object this[String propertyName] {
      get {
        return TypeFactory.GetValueOfPropertyOfObject(this, propertyName);
      }
      set {
        TypeFactory.SetValueOfPropertyOfObject(this, propertyName, value);
      }
    }

    public T GetValue<T>(String propertyName) {
      return Utl.Convert2Type<T>(TypeFactory.GetValueOfPropertyOfObject(this, propertyName));
    }
    public Object GetValue(String propertyName, Type type) {
      return Utl.Convert2Type(TypeFactory.GetValueOfPropertyOfObject(this, propertyName), type);
    }
    public Object GetValue(String propertyName) {
      return TypeFactory.GetValueOfPropertyOfObject(this, propertyName);
    }
    [Browsable(false)]
    public Object ExtObject { get; set; }
  }

}
