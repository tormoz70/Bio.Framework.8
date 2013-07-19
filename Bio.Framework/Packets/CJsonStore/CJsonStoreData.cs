using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using System.Collections;
using System.Reflection.Emit;
using System.Reflection;
#if SILVERLIGHT
using System.Windows.Controls;
#endif

namespace Bio.Framework.Packets {
  public class CJsonStoreData : ICloneable {
    /// <summary>
    /// Признак первого запроса на сервер данного типа. Используется в CJSGrid
    /// </summary>
    public Boolean isFirstLoad { get; set; }
    /// <summary>
    /// Метаданные устарели. Используется в CJSGrid
    /// </summary>
    public Boolean isMetadataObsolete { get; set; }
    /// <summary>
    /// Номер текущей страницы. Используется в CJSGrid
    /// </summary>
    public Int64 start { get; set; }
    /// <summary>
    /// Кол-во записей на страницу. Используется в CJSGrid
    /// </summary>
    public Int64 limit { get; set; }
    /// <summary>
    /// Конец достигнут. Используется в CJSGrid
    /// </summary>
    public Boolean endReached { get; set; }
    /// <summary>
    /// Записей всего. Используется в CJSGrid
    /// </summary>
    public Int64 totalCount { get; set; }
    /// <summary>
    /// Метаданные пакета
    /// </summary>
    public CJsonStoreMetadata metaData { get; set; }
    /// <summary>
    /// Коллекция строк
    /// </summary>
    public JsonStoreRows rows { get; set; }
    /// <summary>
    /// пока не используется
    /// </summary>
    public CJsonStoreFilter locate { get; set; }

    public CJsonStoreData() {
      this.isFirstLoad = true;
      this.isMetadataObsolete = true;
      this.start = 0;
      this.limit = 0;
      this.endReached = false;
      this.totalCount = 0;
    }

    public JsonStoreRow addRow() {
      if (this.metaData == null)
        throw new EBioException("Для добавления данных в пакет необходимо определить метаданные!!!");
      if (this.rows == null)
        this.rows = new JsonStoreRows();
      var newRow = this.metaData.CreateNewRow();
      this.rows.Add(newRow);
      return newRow;
    }

    public CJsonStoreMetadataFieldDef fieldDefByName(String fieldName) { 
      int indx = this.metaData.indexOf(fieldName);
      if ((indx >= 0) && (indx < this.metaData.fields.Count))
        return this.metaData.fields[indx];
      return null;
    }

    public static Object getValue(CJsonStoreMetadata metaData, JsonStoreRow row, String fieldName, Type asType) {
      int indx = metaData.indexOf(fieldName);
      if ((indx >= 0) && (indx < row.Values.Count))
        return Utl.Convert2Type(row.Values[indx], asType);
      else
        return null;
    }

    public static T getValue<T>(CJsonStoreMetadata metaData, JsonStoreRow row, String fieldName) {
      var rslt = getValue(metaData, row, fieldName, typeof(T));
      if (rslt == null)
        return default(T);
      else
        return (T)rslt;
    }


    public T getValue<T>(JsonStoreRow row, String fieldName) {
      return getValue<T>(this.metaData, row, fieldName);
    }

    public Object getValue(JsonStoreRow row, String fieldName, Type asType) {
      return getValue(this.metaData, row, fieldName, asType);
    }

    public Object getValue(JsonStoreRow row, String fieldName) {
      var fd = this.fieldDefByName(fieldName);
      if (fd != null)
        return getValue(row, fieldName, fd.GetDotNetType());
      else
        return null;
    }

    public static CJsonStoreData decode(String pJsonString) {
      return jsonUtl.decode<CJsonStoreData>(pJsonString, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    public String encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter()/*t12, new CJsonStoreRowConverter() */});
    }

    public object Clone() {
      return new CJsonStoreData() {
        isFirstLoad = this.isFirstLoad,
        isMetadataObsolete = this.isMetadataObsolete,
        start = this.start,
        limit = this.limit,
        endReached = this.endReached,
        totalCount = this.totalCount,
        metaData = (this.metaData != null) ? (CJsonStoreMetadata)this.metaData.Clone() : null,
        rows = (this.rows != null) ? (JsonStoreRows)this.rows.Clone() : null,
        locate = (this.locate != null) ? (CJsonStoreFilter)this.locate.Clone() : null
      };
    }

#if SILVERLIGHT
    /*
    private readonly Dictionary<string, Type> _typeBySigniture = new Dictionary<string, Type>();

    private String _getTypeSigniture() {
      StringBuilder sb = new StringBuilder();
      foreach (CJsonStoreMetadataFieldDef fld in this.metaData.fields) {
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
        foreach (CJsonStoreMetadataFieldDef fld in this.metaData.fields) {
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
        foreach (CJsonStoreMetadataFieldDef fld in this.metaData.fields) {
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
      CJsonStoreMetadataFieldDef fldDef = (fldIndx >= 0) ? this.metaData.fields[fldIndx] : null;
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
