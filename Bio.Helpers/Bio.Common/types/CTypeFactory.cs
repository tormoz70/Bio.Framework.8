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
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Linq;

namespace Bio.Helpers.Common.Types {
  public class CPropertyMetadata {
    public String Name { get; set; }
    public String DisplayName { get; set; }
    public Type Type { get; set; }
    public Boolean Hidden { get; set; }
    public String Group { get; set; }
    public Boolean? Readonly { get; set; }
    public Boolean? Required { get; set; }
  }

  public class CTypeFactory {
    private readonly Dictionary<String, Type> _typeBySigniture = new Dictionary<String, Type>();
    private String _getTypeSigniture(List<CPropertyMetadata> propDefs) {
      StringBuilder sb = new StringBuilder();
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
      AssemblyName an = new AssemblyName("TempAssembly" + typeSigniture);
      AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
      ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
      TypeBuilder tb = moduleBuilder.DefineType("TempType" + typeSigniture
                          , TypeAttributes.Public |
                          TypeAttributes.Class |
                          TypeAttributes.AutoClass |
                          TypeAttributes.AnsiClass |
                          TypeAttributes.BeforeFieldInit |
                          TypeAttributes.AutoLayout
                          , baseType);
      return tb;
    }

    private void _createProperty(TypeBuilder tb, CPropertyMetadata propDef) {
      Type propertyType = propDef.Type;
      if (propertyType.IsValueType && !propertyType.IsGenericType) {
        propertyType = typeof(Nullable<>).MakeGenericType(new[] { propertyType });
      }
      String propertyName = propDef.Name;
      Boolean propertyHidden = propDef.Hidden;
      String v_header = !String.IsNullOrEmpty(propDef.DisplayName) ? propDef.DisplayName : propDef.Name;
      if(String.Equals(v_header, "Empty", StringComparison.CurrentCultureIgnoreCase))
        v_header = String.Empty;
      String propertyHeaderContent = v_header;
      //String groupAggr = !String.IsNullOrEmpty(propDef.Group) ? propDef.Group : "None";

      FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Public);
      PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

      CustomAttributeBuilder cabuilder0 = new CustomAttributeBuilder(
        typeof(CategoryAttribute).GetConstructor(new Type[] { typeof(String) }),
        new Object[] { propDef.Group });
      propertyBuilder.SetCustomAttribute(cabuilder0);

      CustomAttributeBuilder cabuilder1 = new CustomAttributeBuilder(
        typeof(HeaderContentAttribute).GetConstructor(new Type[] { typeof(String) }),
        new Object[] { propertyHeaderContent });
      propertyBuilder.SetCustomAttribute(cabuilder1);

      if (propertyHidden) {
        CustomAttributeBuilder cabuilder2 = new CustomAttributeBuilder(
          typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(Boolean) }),
          new Object[] { false });
        propertyBuilder.SetCustomAttribute(cabuilder2);
      }

      //[Display(Name = "TextThatWillBeDisplayedForGroup")]
      Type displayAttribute = typeof(DisplayAttribute);
      PropertyInfo info1 = displayAttribute.GetProperty("Name");
      PropertyInfo info2 = displayAttribute.GetProperty("AutoGenerateField");
      //PropertyInfo info3 = displayAttribute.GetProperty("Order");
      //PropertyInfo info4 = displayAttribute.GetProperty("GroupName");
      CustomAttributeBuilder cabuilder3 = new CustomAttributeBuilder(
          displayAttribute.GetConstructor(new Type[] { }),
          new object[] { },
          new PropertyInfo[] { info1, info2/*, info3, info4 */},
          new object[] { propertyHeaderContent, !propertyHidden/*, order, groupAggr */});
      propertyBuilder.SetCustomAttribute(cabuilder3);

      if (propDef.Readonly != null) {
        if (propDef.Readonly == true) {
          var v_cabuilder = new CustomAttributeBuilder(
            typeof(ReadOnlyAttribute).GetConstructor(new Type[] { typeof(Boolean) }),
            new Object[] { true });
          propertyBuilder.SetCustomAttribute(v_cabuilder);
        }
      }

      if (propDef.Required != null) {
        if (propDef.Required == true) {
          var v_cabuilder = new CustomAttributeBuilder(
            typeof(RequiredAttribute).GetConstructor(new Type[] { typeof(Boolean) }),
            new Object[] { true });
          propertyBuilder.SetCustomAttribute(v_cabuilder);
        }
      }

      MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName,
              MethodAttributes.Public |
              MethodAttributes.SpecialName |
              MethodAttributes.HideBySig,
              propertyType, Type.EmptyTypes);

      ILGenerator getIL = getPropMthdBldr.GetILGenerator();

      getIL.Emit(OpCodes.Ldarg_0);
      getIL.Emit(OpCodes.Ldfld, fieldBuilder);
      getIL.Emit(OpCodes.Ret);

      MethodBuilder setPropMthdBldr =
          tb.DefineMethod("set_" + propertyName,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            null, new Type[] { propertyType });

      //var v_valueChangedMethod = tb.BaseType.GetMethod("valueChanged");
      var v_setPropertyValueMethod = tb.BaseType.GetMethod("setPropertyValue");
      System.Reflection.Emit.Label lbl1 = default(System.Reflection.Emit.Label);
      ILGenerator setIL = setPropMthdBldr.GetILGenerator();

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
      setIL.Emit(OpCodes.Call, v_setPropertyValueMethod);
      setIL.Emit(OpCodes.Nop);
      setIL.Emit(OpCodes.Ret);

      propertyBuilder.SetGetMethod(getPropMthdBldr);
      propertyBuilder.SetSetMethod(setPropMthdBldr);
    }


    public Type CreateType(List<CPropertyMetadata> propDefs) {
      Type baseType = typeof(CRTObject);
      ConstructorInfo BaseCtor0 = baseType.GetConstructor(new Type[0]);
      ConstructorInfo BaseCtor1 = baseType.GetConstructor(new Type[] { typeof(PropertyChangingEventHandler), typeof(PropertyChangedEventHandler) });
      String typeSigniture = this._getTypeSigniture(propDefs);
      Type objectType = this._getTypeByTypeSigniture(typeSigniture);
      if (objectType == null) {
        TypeBuilder tb = this._getTypeBuilder(typeSigniture, baseType);

        ConstructorBuilder constructorBuilder =
                    tb.DefineConstructor(
                                MethodAttributes.Public, CallingConventions.Standard, null);
        ILGenerator constructorIL = constructorBuilder.GetILGenerator();
        constructorIL.Emit(OpCodes.Ldarg_0);
        constructorIL.Emit(OpCodes.Call, BaseCtor0);
        constructorIL.Emit(OpCodes.Ret);

        constructorBuilder = tb.DefineConstructor(
                                MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(PropertyChangingEventHandler), typeof(PropertyChangedEventHandler) });

        constructorIL = constructorBuilder.GetILGenerator();
        constructorIL.Emit(OpCodes.Ldarg_0);
        constructorIL.Emit(OpCodes.Ldarg_1);
        constructorIL.Emit(OpCodes.Ldarg_2);
        constructorIL.Emit(OpCodes.Call, BaseCtor1);
        constructorIL.Emit(OpCodes.Ret);


        foreach (var propDef in propDefs) {
          this._createProperty(tb, propDef);
        }
        objectType = tb.CreateType();
        this._typeBySigniture.Add(typeSigniture, objectType);
      }
      return objectType;
    }

    public static CRTObject CreateInstance(Type type, PropertyChangingEventHandler onPropertyChanging, PropertyChangedEventHandler onPropertyChanged) {
      var constructor = type.GetConstructor(new Type[] { typeof(PropertyChangingEventHandler), typeof(PropertyChangedEventHandler) });
      if (constructor != null)
        return constructor.Invoke(new Object[] { onPropertyChanging, onPropertyChanged }) as CRTObject;
      else
        return null;
    }
    public static CRTObject CreateInstance(Type type) {
      var constructor = type.GetConstructor(new Type[0]);
      if (constructor != null)
        return constructor.Invoke(new Object[0]) as CRTObject;
      else
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
      return type.GetProperties().FirstOrDefault((p) => { return p.Name.Equals(propertyName, comparison); });
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
    /// <param name="comparison"></param>
    /// <returns></returns>
    public static PropertyInfo FindPropertyOfObject(Object obj, String propertyName) {
      return FindPropertyOfObject(obj, propertyName, StringComparison.CurrentCultureIgnoreCase);
    }

    public static Object GetValueOfPropertyOfObject(Object obj, String propertyName, StringComparison comparison) {
      var v_pi = CTypeFactory.FindPropertyOfObject(obj, propertyName, comparison);
      if (v_pi != null)
        return v_pi.GetValue(obj, null);
      return null;
    }
    public static Object GetValueOfPropertyOfObject(Object obj, String propertyName) {
      return GetValueOfPropertyOfObject(obj, propertyName, StringComparison.CurrentCultureIgnoreCase);
    }

    public static void SetValueOfPropertyOfObject(Object obj, String propertyName, Object value) {
      var v_pi = CTypeFactory.FindPropertyOfObject(obj, propertyName, StringComparison.CurrentCultureIgnoreCase);
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
    public CRTObject()
      : base() {
        this.OnPropertyChanging = null;
        this.OnPropertyChanged = null;
    }
    public CRTObject(PropertyChangingEventHandler onChanging, PropertyChangedEventHandler onChanged)
      : base() {
        this.OnPropertyChanging += onChanging;
        this.OnPropertyChanged += onChanged;
    }
    
    private bool _valueChanged(Object oldValue, Object newValue) {
      return !Object.Equals(oldValue, newValue);
    }

    private void _internalSetProperty(String propertyName, Object value) {
      String fldName = "_" + propertyName;
      Type v_thisType = this.GetType();
      // для private - полей
      //var fldInfos = v_thisType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
      //var fldInfo = fldInfos.FirstOrDefault((f) => { 
      //  return f.Name.Equals(fldName); 
      //});
      var fldInfo = v_thisType.GetField(fldName);
      if (fldInfo != null)
        try {
          fldInfo.SetValue(this, value);
          var eve1 = this.PropertyChanged;
          if (eve1 != null)
            eve1(this, new PropertyChangedEventArgs(propertyName));

        } catch (Exception ex) {
          throw ex;
        }
    }

    public CRTObject Copy() { 
      Type v_thisType = this.GetType();
      var constr = v_thisType.GetConstructor(new Type[0]);
      CRTObject rslt = constr.Invoke(new Object[0]) as CRTObject;
      var fldInfos = v_thisType.GetFields();
      foreach (var fi in fldInfos) {
        fi.SetValue(rslt, fi.GetValue(this));
      }
      return rslt;
    }


    private Boolean _eventsDisabled = false;
    public void DisableEvents() { _eventsDisabled = true; }
    public void EnableEvents() { _eventsDisabled = false; }
    public void setPropertyValue(Object oldValue, Object newValue, String propertyName) {
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
      this.setPropertyValue(oldVal, value, propertyName);
    }

    public void SetValues(Object obj) {
      if ((obj != null) && obj.GetType().IsClass) {
        PropertyInfo[] props = this.GetType().GetProperties();
        foreach (var prop in props) {
          if (prop.CanWrite) {
            try {
              Object value = Utl.GetPropertyValue(obj, prop.Name);
              prop.SetValue(this, value, null);
            } catch { }
          }
        }
      }
    }

    public Object this[String propertyName] {
      get {
        return CTypeFactory.GetValueOfPropertyOfObject(this, propertyName);
      }
      set {
        CTypeFactory.SetValueOfPropertyOfObject(this, propertyName, value);
      }
    }

    public T GetValue<T>(String propertyName) {
      return Utl.Convert2Type<T>(CTypeFactory.GetValueOfPropertyOfObject(this, propertyName));
    }
    public Object GetValue(String propertyName, Type type) {
      return Utl.Convert2Type(CTypeFactory.GetValueOfPropertyOfObject(this, propertyName), type);
    }
    public Object GetValue(String propertyName) {
      return CTypeFactory.GetValueOfPropertyOfObject(this, propertyName);
    }
    [Browsable(false)]
    public Object ExtObject { get; set; }
  }

}
