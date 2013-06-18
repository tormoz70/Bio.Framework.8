using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
#if !SILVERLIGHT
using System.Data;
#endif
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using System.Xml.XPath;
using System.Windows;

namespace Bio.Framework.Packets {

  public class CJsonStoreMetadataFieldDef {
    public String name { get; set; }
    public String format { get; set; }
    public CJSAlignment align { get; set; }
    public String header { get; set; }
    public Boolean hidden { get; set; }
    public Boolean readOnly { get; set; }
    public Int32 pk { get; set; }
    public FieldType type { get; set; }
    public Int32 width { get; set; }
    //public String dateFormat { get; set; }
    public String boolVals { get; set; }
    public Object defaultVal { get; set; }
    public Int32 group { get; set; }
    public String group_aggr { get; set; }
    public Type GetDotNetType() {
      return ftypeHelper.ConvertFTypeToType(this.type);
    }
#if SILVERLIGHT
    public HorizontalAlignment GetHorAlignment() {
      switch (this.align) {
        case CJSAlignment.Left: return HorizontalAlignment.Left;
        case CJSAlignment.Center: return HorizontalAlignment.Center;
        case CJSAlignment.Right: return HorizontalAlignment.Right;
        case CJSAlignment.Stretch: return HorizontalAlignment.Stretch;
        default: return HorizontalAlignment.Left;
      }
    }
#endif
  }

  public class CJsonStoreMetadata:ICloneable {
    //public const String csPKFieldName = "pk_value";
    //public const String csRowNumberFieldName = "DATAROWNUMBER";
    //public const String csRowNumberHeader = "№ пп";
    /// <summary>
    /// 
    /// </summary>
    public String id { get; set; }
    //public Boolean RowNumberIsVirtual { get; set; }

    /// <summary>
    /// Набор данных только для чтения.
    /// Все колонки будут readonly, кроме тех у которых явно стоит readOnly = false.
    /// </summary>
    public Boolean readOnly { get; set; }

    /// <summary>
    /// Добавляет в начало колонку с чекбоксами и позволяет выбирать несколько строк на разных страницах
    /// </summary>
    public Boolean multiselection { get; set; }

    /// <summary>
    /// Описание полей
    /// </summary>
    public List<CJsonStoreMetadataFieldDef> fields { get; set; }
    /// <summary>
    /// Возвращает индекс поля по имени
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns></returns>
    public int indexOf(String fieldName) {
      if (this.fields != null) {
        for (int i = 0; i < this.fields.Count; i++) {
          if (String.Equals(this.fields[i].name, fieldName, StringComparison.CurrentCultureIgnoreCase))
            return i;
        }
      }
      return -1;
    }
    /// <summary>
    /// Ключь определен
    /// </summary>
    public Boolean PKDefined {
      get {
        return this.getPKFields().Length > 0;
      }
    }

    public CJsonStoreRow CreateNewRow() {
      return CreateNewRow(this);
    }

    public static CJsonStoreRow CreateNewRow(CJsonStoreMetadata metadata) {
      var newRow = new CJsonStoreRow() { internalROWUID = Guid.NewGuid().ToString("N"), changeType = CJsonStoreRowChangeType.Unchanged };
      if (metadata != null) {
        foreach (var fd in metadata.fields)
          newRow.Values.Add(null);
      }
      return newRow;
    }

#if SILVERLIGHT
    /// <summary>
    /// Преобразует строку со значениями первичного ключа в параметры.
    /// </summary>
    /// <param name="row">Строка со значениями первичного ключа.</param>
    /// <returns></returns>
    public Params getPK(CRTObject row) {
      Params vParams = new Params();
      var v_pkDef = this.getPKFields();
      for (int i = 0; i < v_pkDef.Length; i++) {
        var vType = v_pkDef[i].GetDotNetType();
        var vValue = row.GetValue(v_pkDef[i].name, vType);
        vParams.Add(new Param(v_pkDef[i].name, vValue, vType, ParamDirection.Input));
      }
      return vParams;
    }
#else
    /// <summary>
    /// Преобразует строку со значениями первичного ключа в параметры.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public Params getPK(CJsonStoreData data, CJsonStoreRow row) {
      Params vParams = new Params();
      var v_pkDef = this.getPKFields();
      for (int i = 0; i < v_pkDef.Length; i++) {
        var vType = v_pkDef[i].GetDotNetType();
        var vValue = data.getValue(row, v_pkDef[i].name);
        vParams.Add(new Param(v_pkDef[i].name, vValue, vType, ParamDirection.Input));
      }
      return vParams;
    }
#endif

    /// <summary>
    /// Описание полей первичного ключа
    /// </summary>
    public CJsonStoreMetadataFieldDef[] getPKFields() {
      if (this.fields != null) {
        var v_result = this.fields.Where(f => f.pk > 0).OrderBy(f => f.pk).ToArray();
        return v_result;
      } else
        return new CJsonStoreMetadataFieldDef[0];
    }

#if !SILVERLIGHT
    /// <summary>
    /// Формирует описание структуры JSON-ответа для клиентского JsonStore из XML-описания.
    /// </summary>
    /// <param name="bioCode"/>
    /// <param name="cursorDef">XML-описание курсора.</param>
    public static CJsonStoreMetadata ConstructMetadata(String bioCode, XmlNode cursorDef) {
      CJsonStoreMetadata vResult = new CJsonStoreMetadata();
      vResult.fields = new List<CJsonStoreMetadataFieldDef>();
      vResult.readOnly = Xml.getAttribute<Boolean>(cursorDef as XmlElement, "readOnly", true);
      vResult.multiselection = Xml.getAttribute<Boolean>(cursorDef as XmlElement, "multiselection", false);
      XmlNodeList xmlFields = cursorDef.SelectNodes("fields/field[not(@generate) or (@generate='true')]");
      if (xmlFields == null || xmlFields.Count <= 0)
        return vResult;

      foreach (XmlElement xmlEl in xmlFields) {
        String v_field_name = xmlEl.GetAttribute("name");
        if (vResult.indexOf(v_field_name) >= 0)
          throw new EBioException(String.Format("В описании объекта {0} поле {1} определено более 1 раза.", bioCode, v_field_name));
        CJsonStoreMetadataFieldDef fldDef = new CJsonStoreMetadataFieldDef();
        if (xmlEl.HasAttribute("generate") && xmlEl.GetAttribute("generate") != "true")
          continue;

        fldDef.name = v_field_name;
        if (String.IsNullOrEmpty(fldDef.name))
          continue;

        fldDef.header = xmlEl.GetAttribute("header");
        if (!String.IsNullOrEmpty(fldDef.header))
          fldDef.header = fldDef.header.Replace("\\n", "\n");
        fldDef.hidden = Xml.getAttribute<Boolean>(xmlEl, "hidden", false);
        fldDef.readOnly = Xml.getAttribute<Boolean>(xmlEl, "readOnly", vResult.readOnly);
        fldDef.type = jsonUtl.detectFieldType(xmlEl.GetAttribute("type"));
        fldDef.width = Xml.getAttribute<Int32>(xmlEl, "width", 0);
        fldDef.format = Xml.getAttribute<String>(xmlEl, "format", null);
        if (String.IsNullOrEmpty(fldDef.format)) {
          if ((fldDef.type == FieldType.Date) || (fldDef.type == FieldType.DateUTC))
            fldDef.format = "dd.MM.yyyy HH:mm:ss";
          if (fldDef.type == FieldType.Int)
            fldDef.format = "0";
          if (fldDef.type == FieldType.Currency)
            fldDef.format = "#,##0.00 р";
        }
        fldDef.align = jsonUtl.detectAlignment(fldDef.type, xmlEl.GetAttribute("align"));
        fldDef.group = Xml.getAttribute<Int32>(xmlEl, "group", -1);
        fldDef.group_aggr = xmlEl.GetAttribute("group_aggr");
        Int32 pkIndx = Xml.getAttribute<Int32>(xmlEl, "pk", 0);
        String boolVals = xmlEl.GetAttribute("boolVals");
        String defaultVal = xmlEl.GetAttribute("defaultVal");
        //if (fldDef.type == "currency") fldDef.Type = "float";
        if (pkIndx > 0)
          fldDef.pk = pkIndx;
        //if (fldDef.type == CJsonStoreMetadataFieldType.ftDate)
        //  fldDef.dateFormat = "Y-m-d\\TH:i:s";
        else if (fldDef.type == FieldType.Boolean && !String.IsNullOrEmpty(boolVals))
          fldDef.boolVals = boolVals;
        if (!String.IsNullOrEmpty(defaultVal) && fldDef.type != FieldType.Date)
          if (fldDef.type == FieldType.Boolean)
            fldDef.defaultVal = Utl.Convert2Type<Boolean>(defaultVal);
          else if (fldDef.type == FieldType.String)
            fldDef.defaultVal = defaultVal;
          else
            fldDef.defaultVal = Utl.Convert2Type<Double>(defaultVal);
        vResult.fields.Add(fldDef);
      }
      if (vResult.multiselection) {
        var v_pks = vResult.getPKFields();
        if ((v_pks == null) || (v_pks.Length != 1))
          throw new Exception(String.Format("Для режима multiselection, необходимо чтобы для ио [{0}] был определен первичный ключь(не составной).", bioCode));
      }
      return vResult;
    }
#else
    /*
    /// <summary>
    /// Формирует описание структуры JSON-ответа для клиентского JsonStore из XML-описания.
    /// </summary>
    /// <param name="pCursorDef">XML-описание курсора.</param>
    /// <param name="jw">Объект LisJson.JsonWriter, в который запишутся метаданные.</param>
    public static CJsonStoreMetadata ConstructMetadata(XPathNavigator cursorDef) {
      CJsonStoreMetadata vResult = new CJsonStoreMetadata();
      vResult.fields = new List<CJsonStoreMetadataFieldDef>();
      XPathNodeIterator xmlFields = cursorDef.Select("fields/field[not(@generate) or (@generate='true')]");
      if (xmlFields == null || xmlFields.Count <= 0)
        return vResult;
       
      if (cursorDef.SelectSingleNode("fields/field/@pk") != null) {
        vResult.fields.Add(new CJsonStoreMetadataFieldDef() {
          name = csPKFieldName,
          type = CJsonStoreMetadataFieldType.ftString,
          hidden = true
        });
      }
      while (xmlFields.MoveNext()){
        XPathNavigator curNode = xmlFields.Current;
        CJsonStoreMetadataFieldDef fldDef = new CJsonStoreMetadataFieldDef();
        fldDef.name = Xml.getAttribute(curNode, "name");
        if (String.IsNullOrEmpty(fldDef.name))
          continue;
        fldDef.hidden = String.Equals(Xml.getAttribute(curNode, "hidden"), "true");
        fldDef.header = Xml.getAttribute(curNode, "header");
        fldDef.type = detectFieldType(Xml.getAttribute(curNode, "type"));
        String v_group_str = Xml.getAttribute(curNode, "group");
        fldDef.group = Int32.Parse(!String.IsNullOrEmpty(v_group_str) ? v_group_str : "-1");
        fldDef.group_aggr = curNode.GetAttribute("group_aggr", null);
        int pkIndx = Int32.Parse(Xml.hasAttribute(curNode, "pk") ? Xml.getAttribute(curNode, "pk") : "0");
        String boolVals = Xml.getAttribute(curNode, "boolVals");
        String defaultVal = Xml.getAttribute(curNode, "defaultVal");
        //if (fldDef.type == "currency") fldDef.Type = "float";
        if (pkIndx > 0)
          fldDef.pk = pkIndx;
        if (fldDef.type == CJsonStoreMetadataFieldType.ftDate)
          fldDef.dateFormat = "Y-m-d\\TH:i:s";
        else if (fldDef.type == CJsonStoreMetadataFieldType.ftBoolean && !String.IsNullOrEmpty(boolVals))
          fldDef.boolVals = boolVals;
        if (!String.IsNullOrEmpty(defaultVal) && fldDef.type != CJsonStoreMetadataFieldType.ftDate)
          if (fldDef.type == CJsonStoreMetadataFieldType.ftBoolean)
            fldDef.defaultVal = Utl.Convert2Type<Boolean>(defaultVal);
          else if (fldDef.type == CJsonStoreMetadataFieldType.ftString)
            fldDef.defaultVal = defaultVal;
          else
            fldDef.defaultVal = Utl.Convert2Type<Double>(defaultVal);
        vResult.fields.Add(fldDef);
      }
      return vResult;
    }
    */
#endif

#if !SILVERLIGHT
    /// <summary>
    /// Формирует описание структуры JSON-ответа для клиентского JsonStore из объекта System.Data.DataTable.
    /// </summary>
    /// <param name="table">Объект System.Data.DataTable, из которого нужно взять структуру.</param>
    public static CJsonStoreMetadata ConstructMetadata(DataTable table) {
      CJsonStoreMetadata vResult = new CJsonStoreMetadata();
      vResult.fields = new List<CJsonStoreMetadataFieldDef>();
      if (table == null || table.Columns.Count <= 0)
        return vResult;
      foreach (DataColumn col in table.Columns) {
        //if (col.ColumnName == CExParams.C_RowNumberFieldName)
        //  continue;
        CJsonStoreMetadataFieldDef fldDef = new CJsonStoreMetadataFieldDef();
        fldDef.name = col.ColumnName.ToUpper();
        fldDef.type = jsonUtl.detectFieldType(ftypeHelper.ConvertTypeToStr(col.DataType).ToLower());
        String boolVals = (col.ExtendedProperties.ContainsKey("boolVals")) ? (String)col.ExtendedProperties["boolVals"] : null;
        if (fldDef.type == FieldType.Date)
          fldDef.format = "Y-m-d\\TH:i:s";
        else if (fldDef.type == FieldType.Boolean && !String.IsNullOrEmpty(fldDef.boolVals))
          fldDef.boolVals = boolVals;
        if (col.DefaultValue != DBNull.Value && fldDef.type != FieldType.Date)
          fldDef.defaultVal = col.DefaultValue;
        vResult.fields.Add(fldDef);
      }
      return vResult;
    }
#endif

    public void CopyFrom(CJsonStoreMetadata mdata) {
      //this.RowNumberIsVirtual = mdata.RowNumberIsVirtual;
      this.id = mdata.id;
      if (this.fields == null)
        this.fields = new List<CJsonStoreMetadataFieldDef>();
      this.fields.Clear();
      foreach (CJsonStoreMetadataFieldDef fd in mdata.fields) {
        CJsonStoreMetadataFieldDef newFld = new CJsonStoreMetadataFieldDef() {
          name = fd.name,
          type = fd.type,
          pk = fd.pk,
          defaultVal = fd.defaultVal,
          format = fd.format,
          align = fd.align,
          boolVals = fd.boolVals,
          hidden = fd.hidden,
          header = fd.header,
          group = fd.group,
          group_aggr = fd.group_aggr,
          readOnly = fd.readOnly,
          width = fd.width
        };
        this.fields.Add(newFld);
      }
    }

    public object Clone() {
      CJsonStoreMetadata rslt = new CJsonStoreMetadata();
      rslt.CopyFrom(this);
      return rslt;
    }
  }
}
