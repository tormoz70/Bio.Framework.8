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
using Bio.Framework.Packets;
using Bio.Helpers.Common;
using System.Reflection;
using Bio.Helpers.Common.Types;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using Bio.Helpers.Controls.SL;
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

  public class CEditorBase : INotifyPropertyChanged {
    private Object _getValue(CJsonStoreRow row, CJsonStoreMetadata metaData, String fieldName) {
      int indx = metaData.indexOf(fieldName);
      if ((indx >= 0) && (indx < row.Values.Count))
        return row.Values[indx];
      else
        return null;
    }
    private String _getHeader(CJsonStoreMetadata metaData, String fieldName) {
      int indx = metaData.indexOf(fieldName);
      if (indx >= 0)
        return metaData.fields[indx].header;
      else
        return null;
    }
    private Dictionary<String, String> _headers = null;
    /// <summary>
    /// Присваивает значения из row в свойства данного класса.
    /// Имена свойств (либо атрибут DataFieldMapping) должны соответствовать именам полей в metaData.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="metaData"></param>
    public void assignRow(CJsonStoreRow row, CJsonStoreMetadata metaData) {
      if ((row != null) && (metaData != null)) {
        this._headers = new Dictionary<String, String>();
        PropertyInfo[] props = this.GetType().GetProperties();
        foreach (var prop in props) {
          if (prop.CanWrite) {
            //var attrIgnore = Utl.GetPropertyAttr<JsonIgnoreAttribute>(prop);
            //if (attrIgnore == null) {
            var attr = Utl.GetPropertyAttr<DataFieldMappingAttribute>(prop);
            String fieldName = (attr != null) ? attr.DataField : prop.Name;
            Object value = this._getValue(row, metaData, fieldName);
            prop.SetValue(this, value, null);
            var hcAttr = Utl.GetPropertyAttr<HeaderContentAttribute>(prop);
            String vHeader = (hcAttr != null) ? hcAttr.Text : this._getHeader(metaData, fieldName);
            this._headers.Add(prop.Name, vHeader);
            //}
          }
        }
      }
    }

    public static T CreRec<T>(CJsonStoreRow row, CJsonStoreMetadata metaData) where T : CEditorBase, new() {
      var v_result = new T();
      v_result.assignRow(row, metaData);
      return v_result;
    }
    /// <summary>
    /// Возвращает заголовок поля по имени. Будет работать только после вызова assignRow.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public String getHeader(String fieldName) {
      var hdr = this._headers.Where((a) => { return a.Key.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase); });
      return (hdr.Count() > 0) ? hdr.First().Value : null;
    }
    /// <summary>
    /// Возвращает значения свойств класса в виде Params
    /// </summary>
    /// <returns></returns>
    public Params getAsParams() {
      Params rslt = new Params();
      PropertyInfo[] props = this.GetType().GetProperties();
      foreach (var prop in props) {
        var attrIgnore = Utl.GetPropertyAttr<JsonIgnoreAttribute>(prop);
        if (attrIgnore == null) {
          var attr = Utl.GetPropertyAttr<DataFieldMappingAttribute>(prop);
          String fieldName = (attr != null) ? attr.DataField : prop.Name;
          //Object value = this._getValue(row, metaData, fieldName);
          Object value = prop.GetValue(this, null);
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

    public void assignFrom(Object src) {
      if ((src != null) && src.GetType().IsClass) {
        this._headers = new Dictionary<String, String>();
        PropertyInfo[] props = this.GetType().GetProperties();
        foreach (var prop in props) {
          if (prop.CanWrite) {

            Object value = Utl.GetPropertyValue(src, prop.Name);
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
