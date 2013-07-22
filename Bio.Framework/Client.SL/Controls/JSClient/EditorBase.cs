using System;
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Bio.Framework.Client.SL {
  [AttributeUsage(AttributeTargets.Property)]
  public class DataFieldMappingAttribute : Attribute {
    private readonly string _dataFieldMapping;

    public string DataField {
      get { return this._dataFieldMapping; }
    }

    public DataFieldMappingAttribute(string dataFieldMapping) {
      this._dataFieldMapping = dataFieldMapping;
    }
  }

  public class EditorBase : INotifyPropertyChanged {
    private String _getHeader(JsonStoreMetadata metaData, String fieldName) {
      var indx = metaData.IndexOf(fieldName);
      if (indx >= 0)
        return metaData.Fields[indx].Header;
      return null;
    }
    private Dictionary<String, String> _headers;
    /// <summary>
    /// Присваивает значения из row в свойства данного класса.
    /// Имена свойств (либо атрибут DataFieldMapping) должны соответствовать именам полей в metaData.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="metaData"></param>
    public void AssignRow(CRTObject row, JsonStoreMetadata metaData) {
      if ((row != null) && (metaData != null)) {
        this._headers = new Dictionary<String, String>();
        var props = this.GetType().GetProperties();
        foreach (var prop in props) {
          if (prop.CanWrite) {
            var attr = Utl.GetPropertyAttr<DataFieldMappingAttribute>(prop);
            var fieldName = (attr != null) ? attr.DataField : prop.Name;
            var value = row.GetValue(fieldName);
            prop.SetValue(this, value, null);
            var hcAttr = Utl.GetPropertyAttr<HeaderContentAttribute>(prop);
            var vHeader = (hcAttr != null) ? hcAttr.Text : this._getHeader(metaData, fieldName);
            this._headers.Add(prop.Name, vHeader);
          }
        }
      }
    }

    public static T CreRec<T>(CRTObject row, JsonStoreMetadata metaData) where T : EditorBase, new() {
      var v_result = new T();
      v_result.AssignRow(row, metaData);
      return v_result;
    }
    /// <summary>
    /// Возвращает заголовок поля по имени. Будет работать только после вызова assignRow.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public String GetHeader(String fieldName) {
      var hdr = this._headers.Where(a => a.Key.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase));
      return (hdr.Any()) ? hdr.First().Value : null;
    }
    /// <summary>
    /// Возвращает значения свойств класса в виде Params
    /// </summary>
    /// <returns></returns>
    public Params GetAsParams() {
      var rslt = new Params();
      var props = this.GetType().GetProperties();
      foreach (var prop in props) {
        var attrIgnore = Utl.GetPropertyAttr<JsonIgnoreAttribute>(prop);
        if (attrIgnore == null) {
          var attr = Utl.GetPropertyAttr<DataFieldMappingAttribute>(prop);
          var fieldName = (attr != null) ? attr.DataField : prop.Name;
          //Object value = this._getValue(row, metaData, fieldName);
          var value = prop.GetValue(this, null);
          Params valueAdditionalParams = null;
          if (value is VSelection) {
            valueAdditionalParams = ((VSelection)value).filterParams;
            value = ((VSelection)value).Value;
          }
          rslt.Add(fieldName, value);
          if (valueAdditionalParams != null)
            rslt.AddRange(valueAdditionalParams);
        }
      }
      return rslt;
    }

    public void AssignFrom(Object src) {
      if ((src != null) && src.GetType().IsClass) {
        this._headers = new Dictionary<String, String>();
        var props = this.GetType().GetProperties();
        foreach (var prop in props) {
          if (prop.CanWrite) {

            var value = Utl.GetPropertyValue(src, prop.Name);
            prop.SetValue(this, value, null);
            this.doPropertyChanged(prop.Name);
            var hcAttr = Utl.GetPropertyAttr<HeaderContentAttribute>(prop);
            if(hcAttr != null)
              this._headers.Add(prop.Name, hcAttr.Text);
          }
        }
      }
    }


    #region INotifyPropertyChanged Members

    private Boolean _propertyChangedEventEnabled = true;
    protected void enablePropertyChangedEvent() {
      this._propertyChangedEventEnabled = true;
    }
    protected void disablePropertyChangedEvent() {
      this._propertyChangedEventEnabled = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void doPropertyChanged(String propertyName) {
      if (this._propertyChangedEventEnabled) {
        var eve = this.PropertyChanged;
        if (eve != null)
          eve(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #endregion
  }
}
