using System;
using Newtonsoft.Json;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
#if SILVERLIGHT

#endif

namespace Bio.Framework.Packets {
  public class JsonStoreData : ICloneable {
    /// <summary>
    /// Признак первого запроса на сервер данного типа. Используется в CJSGrid
    /// </summary>
    public Boolean IsFirstLoad { get; set; }
    /// <summary>
    /// Метаданные устарели. Используется в CJSGrid
    /// </summary>
    public Boolean IsMetadataObsolete { get; set; }
    /// <summary>
    /// Номер текущей страницы. Используется в CJSGrid
    /// </summary>
    public Int64 Start { get; set; }
    /// <summary>
    /// Кол-во записей на страницу. Используется в CJSGrid
    /// </summary>
    public Int64 Limit { get; set; }
    /// <summary>
    /// Конец достигнут. Используется в CJSGrid
    /// </summary>
    public Boolean EndReached { get; set; }
    /// <summary>
    /// Записей всего. Используется в CJSGrid
    /// </summary>
    public Int64 TotalCount { get; set; }
    /// <summary>
    /// Метаданные пакета
    /// </summary>
    public JsonStoreMetadata MetaData { get; set; }
    /// <summary>
    /// Коллекция строк
    /// </summary>
    public JsonStoreRows Rows { get; set; }
    /// <summary>
    /// пока не используется
    /// </summary>
    public JsonStoreFilter Locate { get; set; }

    public JsonStoreData() {
      this.IsFirstLoad = true;
      this.IsMetadataObsolete = true;
      this.Start = 0;
      this.Limit = 0;
      this.EndReached = false;
      this.TotalCount = 0;
    }

    public JsonStoreRow AddRow() {
      if (this.MetaData == null)
        throw new EBioException("Для добавления данных в пакет необходимо определить метаданные!!!");
      if (this.Rows == null)
        this.Rows = new JsonStoreRows();
      var newRow = this.MetaData.CreateNewRow();
      this.Rows.Add(newRow);
      return newRow;
    }

    public JsonStoreMetadataFieldDef FieldDefByName(String fieldName) { 
      int indx = this.MetaData.IndexOf(fieldName);
      if ((indx >= 0) && (indx < this.MetaData.Fields.Count))
        return this.MetaData.Fields[indx];
      return null;
    }

    public static Object GetValue(JsonStoreMetadata metaData, JsonStoreRow row, String fieldName, Type asType) {
      var indx = metaData.IndexOf(fieldName);
      if ((indx >= 0) && (indx < row.Values.Count))
        return Utl.Convert2Type(row.Values[indx], asType);
      return null;
    }

    public static T GetValue<T>(JsonStoreMetadata metaData, JsonStoreRow row, String fieldName) {
      var rslt = GetValue(metaData, row, fieldName, typeof(T));
      if (rslt == null)
        return default(T);
      return (T)rslt;
    }


    public T GetValue<T>(JsonStoreRow row, String fieldName) {
      return GetValue<T>(this.MetaData, row, fieldName);
    }

    public Object GetValue(JsonStoreRow row, String fieldName, Type asType) {
      return GetValue(this.MetaData, row, fieldName, asType);
    }

    public Object GetValue(JsonStoreRow row, String fieldName) {
      var fd = this.FieldDefByName(fieldName);
      if (fd != null)
        return GetValue(row, fieldName, fd.GetDotNetType());
      return null;
    }

    public static JsonStoreData Decode(String pJsonString) {
      return jsonUtl.decode<JsonStoreData>(pJsonString, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    public object Clone() {
      return new JsonStoreData() {
        IsFirstLoad = this.IsFirstLoad,
        IsMetadataObsolete = this.IsMetadataObsolete,
        Start = this.Start,
        Limit = this.Limit,
        EndReached = this.EndReached,
        TotalCount = this.TotalCount,
        MetaData = (this.MetaData != null) ? (JsonStoreMetadata)this.MetaData.Clone() : null,
        Rows = (this.Rows != null) ? (JsonStoreRows)this.Rows.Clone() : null,
        Locate = (this.Locate != null) ? (JsonStoreFilter)this.Locate.Clone() : null
      };
    }

#if SILVERLIGHT
    /*
    private readonly Dictionary<string, Type> _typeBySigniture = new Dictionary<string, Type>();

    private String _getTypeSigniture() {
      StringBuilder sb = new StringBuilder();
      foreach (JsonStoreMetadataFieldDef fld in this.metaData.fields) {
        sb.AppendFormat("_{0}_{1}", fld.name, fld.GetDotNetType());
      }
      return sb.ToString().GetHashCode().ToString().Replace("-", "Minus");
    }

    private void _createProperty(TypeBuilder tb, String propertyName, Type propertyType) {
      if (propertyType.IsValueType && !propertyType.IsGenericType) {
        propertyType = typeof(Nullable<>).MakeGenericType(new[] { propertyType });
      }

      FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
      PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
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

      ILGenerator setIL = setPropMthdBldr.GetILGenerator();

      setIL.Emit(OpCodes.Ldarg_0);
      setIL.Emit(OpCodes.Ldarg_1);
      setIL.Emit(OpCodes.Stfld, fieldBuilder);
      setIL.Emit(OpCodes.Ret);

      propertyBuilder.SetGetMethod(getPropMthdBldr);
      propertyBuilder.SetSetMethod(setPropMthdBldr);
    }

    private IEnumerable _generateEnumerable(Type objectType) {
      var listType = typeof(List<>).MakeGenericType(new[] { objectType });
      var listOfCustom = Activator.CreateInstance(listType);

      foreach (JsonStoreRow r in this.rows) {
        var row = Activator.CreateInstance(objectType);
        foreach (JsonStoreMetadataFieldDef fld in this.metaData.fields) {
            PropertyInfo property = objectType.GetProperty(fld.name);
            var value = r[this.metaData.indexOf(fld.name)];
            value = Utl.Convert2Type(value, property.PropertyType);
            property.SetValue(row, value, null);
        }
        listType.GetMethod("Add").Invoke(listOfCustom, new[] { row });
      }
      return listOfCustom as IEnumerable;
    }

    private TypeBuilder _getTypeBuilder(String typeSigniture) {
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
                          , typeof(Object));
      return tb;
    }

    private Type _getTypeByTypeSigniture(string typeSigniture) {
      Type type;
      return _typeBySigniture.TryGetValue(typeSigniture, out type) ? type : null;
    }

    private IEnumerable _creDataSource() {

      Boolean hasData = (this.rows != null) && (this.rows.Count > 0);
      if (!hasData) {
        return new object[] { };
      }
      String typeSigniture = _getTypeSigniture();
      Type objectType = this._getTypeByTypeSigniture(typeSigniture);
      if (objectType == null) {
        TypeBuilder tb = this._getTypeBuilder(typeSigniture);
        ConstructorBuilder constructor =
                    tb.DefineDefaultConstructor(
                                MethodAttributes.Public |
                                MethodAttributes.SpecialName |
                                MethodAttributes.RTSpecialName);
        foreach (JsonStoreMetadataFieldDef fld in this.metaData.fields) {
          this._createProperty(tb, fld.name, fld.GetDotNetType());
        }
        objectType = tb.CreateType();
        this._typeBySigniture.Add(typeSigniture, objectType);
      }

      return this._generateEnumerable(objectType);
    }

    private DataGrid _dataGrid = null;
    public void assignDataSource(DataGrid grid) {
      this._dataGrid = grid;
      if (this._dataGrid.AutoGenerateColumns) {
        this._dataGrid.AutoGeneratingColumn -= new EventHandler<DataGridAutoGeneratingColumnEventArgs>(this._dataGrid_AutoGeneratingColumn);
        this._dataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(this._dataGrid_AutoGeneratingColumn);
      }
      this._dataGrid.ItemsSource = this._creDataSource();
    }

    void _dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
      Int32 fldIndx = this.metaData.indexOf(e.PropertyName);
      JsonStoreMetadataFieldDef fldDef = (fldIndx >= 0) ? this.metaData.fields[fldIndx] : null;
      e.Cancel = e.PropertyName.Equals("PK_VALUE") || ((fldDef != null) && fldDef.hidden);
      if (!e.Cancel) {
        if (fldDef != null) {
          String headerStr = fldDef.header;
          if (!String.IsNullOrEmpty(headerStr))
            e.Column.Header = headerStr;
        }
      }
    }
    */
#endif

  }
}
