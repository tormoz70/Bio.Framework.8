using System;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Data;
using System.Xml;
#else
using System.Windows;
#endif
using System.Linq;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets {

  public class JsonStoreMetadataFieldDef {
    public String Name { get; set; }
    public String Format { get; set; }
    public CJSAlignment Align { get; set; }
    public String Header { get; set; }
    public Boolean Hidden { get; set; }
    public Boolean ReadOnly { get; set; }
    public Int32 PK { get; set; }
    public JSFieldType Type { get; set; }
    public Int32 Width { get; set; }
    public String BoolVals { get; set; }
    public Object DefaultVal { get; set; }
    public Int32 Group { get; set; }
    public String GroupAggr { get; set; }
    public Type GetDotNetType() {
      return ftypeHelper.ConvertFTypeToType(this.Type);
    }
#if SILVERLIGHT
    public HorizontalAlignment GetHorAlignment() {
      switch (this.Align) {
        case CJSAlignment.Left: return HorizontalAlignment.Left;
        case CJSAlignment.Center: return HorizontalAlignment.Center;
        case CJSAlignment.Right: return HorizontalAlignment.Right;
        case CJSAlignment.Stretch: return HorizontalAlignment.Stretch;
        default: return HorizontalAlignment.Left;
      }
    }
#endif
  }

  public class JsonStoreMetadata:ICloneable {
    //public const String csPKFieldName = "pk_value";
    //public const String csRowNumberFieldName = "DATAROWNUMBER";
    //public const String csRowNumberHeader = "№ пп";
    /// <summary>
    /// 
    /// </summary>
    public String ID { get; set; }
    //public Boolean RowNumberIsVirtual { get; set; }

    /// <summary>
    /// Набор данных только для чтения.
    /// Все колонки будут readonly, кроме тех у которых явно стоит readOnly = false.
    /// </summary>
    public Boolean ReadOnly { get; set; }

    /// <summary>
    /// Добавляет в начало колонку с чекбоксами и позволяет выбирать несколько строк на разных страницах
    /// </summary>
    public Boolean Multiselection { get; set; }

    /// <summary>
    /// Описание полей
    /// </summary>
    public List<JsonStoreMetadataFieldDef> Fields { get; set; }
    /// <summary>
    /// Возвращает индекс поля по имени
    /// </summary>
    /// <param name="fieldName">Имя поля</param>
    /// <returns></returns>
    public int IndexOf(String fieldName) {
      if (this.Fields != null) {
        for (var i = 0; i < this.Fields.Count; i++) {
          if (String.Equals(this.Fields[i].Name, fieldName, StringComparison.CurrentCultureIgnoreCase))
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
        return this.GetPKFields().Length > 0;
      }
    }

    public JsonStoreRow CreateNewRow() {
      return CreateNewRow(this);
    }

    public static JsonStoreRow CreateNewRow(JsonStoreMetadata metadata) {
      var newRow = new JsonStoreRow { InternalROWUID = Guid.NewGuid().ToString("N"), ChangeType = JsonStoreRowChangeType.Unchanged };
      if (metadata != null) {
        foreach (var fd in metadata.Fields)
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
    public Params GetPK(CRTObject row) {
      var @params = new Params();
      var pkDef = this.GetPKFields();
      foreach (var t in pkDef) {
        var vType = t.GetDotNetType();
        var vValue = row.GetValue(t.Name, vType);
        @params.Add(new Param(t.Name, vValue, vType, ParamDirection.Input));
      }
      return @params;
    }
#else
    /// <summary>
    /// Преобразует строку со значениями первичного ключа в параметры.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public Params getPK(JsonStoreData data, JsonStoreRow row) {
      Params vParams = new Params();
      var v_pkDef = this.GetPKFields();
      for (int i = 0; i < v_pkDef.Length; i++) {
        var vType = v_pkDef[i].GetDotNetType();
        var vValue = data.GetValue(row, v_pkDef[i].Name);
        vParams.Add(new Param(v_pkDef[i].Name, vValue, vType, ParamDirection.Input));
      }
      return vParams;
    }
#endif

    /// <summary>
    /// Описание полей первичного ключа
    /// </summary>
    public JsonStoreMetadataFieldDef[] GetPKFields() {
      if (this.Fields != null) {
        var v_result = this.Fields.Where(f => f.PK > 0).OrderBy(f => f.PK).ToArray();
        return v_result;
      }
      return new JsonStoreMetadataFieldDef[0];
    }

#if !SILVERLIGHT
    /// <summary>
    /// Формирует описание структуры JSON-ответа для клиентского JsonStore из XML-описания.
    /// </summary>
    /// <param name="bioCode"/>
    /// <param name="cursorDef">XML-описание курсора.</param>
    public static JsonStoreMetadata ConstructMetadata(String bioCode, XmlNode cursorDef) {
      JsonStoreMetadata vResult = new JsonStoreMetadata();
      vResult.Fields = new List<JsonStoreMetadataFieldDef>();
      vResult.ReadOnly = Xml.getAttribute<Boolean>(cursorDef as XmlElement, "readOnly", true);
      vResult.Multiselection = Xml.getAttribute<Boolean>(cursorDef as XmlElement, "multiselection", false);
      XmlNodeList xmlFields = cursorDef.SelectNodes("fields/field[not(@generate) or (@generate='true')]");
      if (xmlFields == null || xmlFields.Count <= 0)
        return vResult;

      foreach (XmlElement xmlEl in xmlFields) {
        String v_field_name = xmlEl.GetAttribute("name");
        if (vResult.IndexOf(v_field_name) >= 0)
          throw new EBioException(String.Format("В описании объекта {0} поле {1} определено более 1 раза.", bioCode, v_field_name));
        JsonStoreMetadataFieldDef fldDef = new JsonStoreMetadataFieldDef();
        if (xmlEl.HasAttribute("generate") && xmlEl.GetAttribute("generate") != "true")
          continue;

        fldDef.Name = v_field_name;
        if (String.IsNullOrEmpty(fldDef.Name))
          continue;

        fldDef.Header = xmlEl.GetAttribute("header");
        if (!String.IsNullOrEmpty(fldDef.Header))
          fldDef.Header = fldDef.Header.Replace("\\n", "\n");
        fldDef.Hidden = Xml.getAttribute<Boolean>(xmlEl, "hidden", false);
        fldDef.ReadOnly = Xml.getAttribute<Boolean>(xmlEl, "readOnly", vResult.ReadOnly);
        fldDef.Type = jsonUtl.detectFieldType(xmlEl.GetAttribute("type"));
        fldDef.Width = Xml.getAttribute<Int32>(xmlEl, "width", 0);
        fldDef.Format = Xml.getAttribute<String>(xmlEl, "format", null);
        if (String.IsNullOrEmpty(fldDef.Format)) {
          if ((fldDef.Type == JSFieldType.Date) || (fldDef.Type == JSFieldType.DateUTC))
            fldDef.Format = "dd.MM.yyyy HH:mm:ss";
          if (fldDef.Type == JSFieldType.Int)
            fldDef.Format = "0";
          if (fldDef.Type == JSFieldType.Currency)
            fldDef.Format = "#,##0.00 р";
        }
        fldDef.Align = jsonUtl.detectAlignment(fldDef.Type, xmlEl.GetAttribute("align"));
        fldDef.Group = Xml.getAttribute<Int32>(xmlEl, "group", -1);
        fldDef.GroupAggr = xmlEl.GetAttribute("group_aggr");
        Int32 pkIndx = Xml.getAttribute<Int32>(xmlEl, "pk", 0);
        String boolVals = xmlEl.GetAttribute("boolVals");
        String defaultVal = xmlEl.GetAttribute("defaultVal");
        //if (fldDef.type == "currency") fldDef.Type = "float";
        if (pkIndx > 0)
          fldDef.PK = pkIndx;
        //if (fldDef.type == CJsonStoreMetadataFieldType.ftDate)
        //  fldDef.dateFormat = "Y-m-d\\TH:i:s";
        else if (fldDef.Type == JSFieldType.Boolean && !String.IsNullOrEmpty(boolVals))
          fldDef.BoolVals = boolVals;
        if (!String.IsNullOrEmpty(defaultVal) && fldDef.Type != JSFieldType.Date)
          if (fldDef.Type == JSFieldType.Boolean)
            fldDef.DefaultVal = Utl.Convert2Type<Boolean>(defaultVal);
          else if (fldDef.Type == JSFieldType.String)
            fldDef.DefaultVal = defaultVal;
          else
            fldDef.DefaultVal = Utl.Convert2Type<Double>(defaultVal);
        vResult.Fields.Add(fldDef);
      }
      if (vResult.Multiselection) {
        var v_pks = vResult.GetPKFields();
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
    public static JsonStoreMetadata ConstructMetadata(XPathNavigator cursorDef) {
      JsonStoreMetadata vResult = new JsonStoreMetadata();
      vResult.fields = new List<JsonStoreMetadataFieldDef>();
      XPathNodeIterator xmlFields = cursorDef.Select("fields/field[not(@generate) or (@generate='true')]");
      if (xmlFields == null || xmlFields.Count <= 0)
        return vResult;
       
      if (cursorDef.SelectSingleNode("fields/field/@pk") != null) {
        vResult.fields.Add(new JsonStoreMetadataFieldDef() {
          name = csPKFieldName,
          type = CJsonStoreMetadataFieldType.ftString,
          hidden = true
        });
      }
      while (xmlFields.MoveNext()){
        XPathNavigator curNode = xmlFields.Current;
        JsonStoreMetadataFieldDef fldDef = new JsonStoreMetadataFieldDef();
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
    public static JsonStoreMetadata ConstructMetadata(DataTable table) {
      JsonStoreMetadata vResult = new JsonStoreMetadata();
      vResult.Fields = new List<JsonStoreMetadataFieldDef>();
      if (table == null || table.Columns.Count <= 0)
        return vResult;
      foreach (DataColumn col in table.Columns) {
        //if (col.ColumnName == CExParams.C_RowNumberFieldName)
        //  continue;
        JsonStoreMetadataFieldDef fldDef = new JsonStoreMetadataFieldDef();
        fldDef.Name = col.ColumnName.ToUpper();
        fldDef.Type = jsonUtl.detectFieldType(ftypeHelper.ConvertTypeToStr(col.DataType).ToLower());
        var boolVals = (col.ExtendedProperties.ContainsKey("boolVals")) ? (String)col.ExtendedProperties["boolVals"] : null;
        if (fldDef.Type == JSFieldType.Date)
          fldDef.Format = "Y-m-d\\TH:i:s";
        else if (fldDef.Type == JSFieldType.Boolean && !String.IsNullOrEmpty(fldDef.BoolVals))
          fldDef.BoolVals = boolVals;
        if (col.DefaultValue != DBNull.Value && fldDef.Type != JSFieldType.Date)
          fldDef.DefaultVal = col.DefaultValue;
        vResult.Fields.Add(fldDef);
      }
      return vResult;
    }
#endif

    public void CopyFrom(JsonStoreMetadata mdata) {
      this.ID = mdata.ID;
      if (this.Fields == null)
        this.Fields = new List<JsonStoreMetadataFieldDef>();
      this.Fields.Clear();
      foreach (JsonStoreMetadataFieldDef fd in mdata.Fields) {
        var newFld = new JsonStoreMetadataFieldDef() {
          Name = fd.Name,
          Type = fd.Type,
          PK = fd.PK,
          DefaultVal = fd.DefaultVal,
          Format = fd.Format,
          Align = fd.Align,
          BoolVals = fd.BoolVals,
          Hidden = fd.Hidden,
          Header = fd.Header,
          Group = fd.Group,
          GroupAggr = fd.GroupAggr,
          ReadOnly = fd.ReadOnly,
          Width = fd.Width
        };
        this.Fields.Add(newFld);
      }
    }

    public object Clone() {
      var rslt = new JsonStoreMetadata();
      rslt.CopyFrom(this);
      return rslt;
    }
  }
}
