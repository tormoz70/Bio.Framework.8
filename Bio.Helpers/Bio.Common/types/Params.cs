namespace Bio.Helpers.Common.Types {
  using System;
#if !SILVERLIGHT
  using System.Web;
#else
  using System.Windows.Browser;
#endif
  using System.Linq;
  using System.Collections.Generic;
  using Common;
  using Newtonsoft.Json;
  using System.Collections.Specialized;

  /// <summary>
  /// ����������� ���������
  /// </summary>
  public enum ParamDirection {
    Input,
    Output,
    InputOutput,
    Return
  }

  // *** Param ***

  #region Param

  /// <summary>
  /// ��������
  /// </summary>
  public class Param : ICloneable {

    /// <summary>
    /// �����������
    /// </summary>
    public Param() {
      this.ParamDir = ParamDirection.Input;
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public Param(String name, Object value)
      : this() {
      this.Name = name;
      this.Value = value;
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="paramType"></param>
    /// <param name="paramDirection"></param>
    public Param(String name, Object value, Type paramType, ParamDirection paramDirection)
      : this(name, value) {
      this.ParamType = paramType;
      this.ParamDir = paramDirection;
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="paramType"></param>
    /// <param name="paramSize"></param>
    /// <param name="paramDirection"></param>
    public Param(String name, Object value, Type paramType, int paramSize, ParamDirection paramDirection)
      : this(name, value, paramType, paramDirection) {
      this.ParamSize = paramSize;
    }

    internal Params owner { get; set; }

    /// <summary>
    /// ���
    /// </summary>
    public String Name { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    public Object Value { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    public Object InnerObject { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    public Object InnerObjectEx { get; set; }

    /// <summary>
    /// ���
    /// </summary>
    public Type ParamType { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    public Int32 ParamSize { get; set; }

    /// <summary>
    /// �����������
    /// </summary>
    public ParamDirection ParamDir { get; set; }

    /// <summary>
    /// �������� ��� String
    /// </summary>
    /// <returns></returns>
    public String ValueAsString() {
      return Utl.ObjectAsString(this.Value);
    }

    /// <summary>
    /// ������ ��������� � ���������
    /// </summary>
    /// <returns></returns>
    public Int32 GetParamIndex() {
      return this.owner.IndexOf(this);
    }

    /// <summary>
    /// ������� �������� �� ���������, � ������� �� �����������
    /// </summary>
    public void Kill() {
      var v_owner = this.owner;
      if (v_owner != null) v_owner.Remove(this);
    }

    public override String ToString() {
      var v_innrObjStr = (this.InnerObject == null) ? null : "o:" + this.InnerObject;
      var v_innrObjExStr = (this.InnerObjectEx == null) ? null : "oex" + this.InnerObjectEx;
      String v_objsStr = null;
      if (!String.IsNullOrEmpty(v_innrObjStr))
        Utl.AppendStr(ref v_objsStr, v_innrObjStr, ";");
      if (!String.IsNullOrEmpty(v_innrObjExStr))
        Utl.AppendStr(ref v_objsStr, v_innrObjExStr, ";");
      if (!String.IsNullOrEmpty(v_objsStr))
        v_objsStr = "(" + v_objsStr + ")";
      var v_valStr = String.Format("{0}{1}", this.Value, v_objsStr);
      return String.Format("({0}=[{1}]; tp:{2}; sz:{3}; dr:{4})",
                           this.Name, v_valStr, this.ParamType, this.ParamSize, this.ParamDir);
    }

    internal Param Export(Params newOwner) {
      var v_rslt = (Param) this.Clone();
      v_rslt.owner = newOwner;
      return v_rslt;
    }

    #region ICloneable Members

    public Object Clone() {
      var v_rslt = new Param {
        owner = this.owner,
        Name = this.Name,
        Value = (this.Value is ICloneable) ? (this.Value as ICloneable).Clone() : this.Value,
        InnerObject = (this.InnerObject is ICloneable) ? (this.InnerObject as ICloneable).Clone() : this.InnerObject,
        InnerObjectEx = (this.InnerObjectEx is ICloneable) ? (this.InnerObjectEx as ICloneable).Clone() : this.InnerObjectEx,
        ParamType = this.ParamType,
        ParamSize = this.ParamSize,
        ParamDir = this.ParamDir
      };

      return v_rslt;
    }

    #endregion
  }

  #endregion

  // *** Params ***

  #region Params

  /// <summary>
  /// ��������� ����������
  /// </summary>
  public class Params : List<Param> {

    #region Constructors

    /// <summary>
    /// �����������
    /// </summary>
    public Params() {}

#if !SILVERLIGHT
    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="prms"></param>
    public Params(NameValueCollection prms)
      : this() {
      foreach (var k in prms.AllKeys)
        this.Add(k, prms[k]);
    }
#endif

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="paramsArray"></param>
    public Params(params Param[] paramsArray)
      : this() {
      foreach (var v in paramsArray) {
        base.Add(v);
      }
    }

    /// <summary>
    /// �������������� ��������� ���������� � �������������, ���� null, �� ������� �����,
    /// ���� �� null, �� ����������� ��� �� overloadPrms � �����������
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="overloadPrms"></param>
    public static Params PrepareToUse(Params prms, Params overloadPrms) {
      if (overloadPrms != null) {
        if (prms == null)
          prms = overloadPrms.Clone() as Params;
        else {
          prms.Clear();
          prms = prms.Merge(overloadPrms, true);
        }
      } else {
        if (prms == null)
          prms = new Params();
      }
      return prms;
    }

    #endregion

    #region Properties

    /// <summary>
    /// �������� �� �����
    /// </summary>
    /// <param name="name"></param>
    public Param this[String name] {
      get { return this.ParamByName(name); }
    }

    /// <summary>
    /// ����� ��������� � ������ ����� ����������� ","
    /// </summary>
    public virtual String NamesAsDelimitedStrings {
      get {
        String rslt = null;
        for (var i = 0; i < this.Count; i++)
          Utl.AppendStr(ref rslt, "\"" + this[i].Name + "\"", ",");
        return rslt;
      }
    }

    /// <summary>
    /// �������� ���������� � ������ ����� ����������� ","
    /// </summary>
    public virtual String ValsAsDelimitedStrings {
      get {
        String rslt = null;
        for (var i = 0; i < this.Count; i++)
          Utl.AppendStr(ref rslt, "\"" + this[i].ValueAsString() + "\"", ",");
        return rslt;
      }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// ���������� ������ ���������. -1 - ���� �� ������.
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public int IndexOf(String itemName) {
      return IndexOf(this.ParamByName(itemName));
    }

    /// <summary>
    /// ������� �������� �� ���������
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public new Param Remove(Param item) {
      lock (this) {
        base.Remove(item);
        return item;
      }
    }

    /// <summary>
    /// ������� �������� �� ���������
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public Param Remove(String itemName) {
      lock (this) {
        var item = this.Remove(this.ParamByName(itemName));
        return item;
      }
    }

    /// <summary>
    /// ��������� �������� � ����� ���������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="val"></param>
    /// <param name="replaceIfExists"></param>
    /// <returns></returns>
    public Param Add(String name, Object val, Boolean replaceIfExists) {
      lock (this) {
        var v_existsPar = this.ParamByName(name);
        if ((v_existsPar != null) && replaceIfExists) {
          this.Remove(v_existsPar);
        }
        if (this.ParamByName(name) == null) {
          var rslt = new Param {
            owner = this,
            Name = name,
            Value = val
          };
          base.Add(rslt);
          return rslt;
        }
        return null;
      }
    }

    /// <summary>
    /// ��������� �������� � ����� ���������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public Param Add(String name, Object val) {
      return this.Add(name, val, false);
    }

    /// <summary>
    /// ��������� �������� � ����� ���������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="val"></param>
    /// <param name="innerObject"></param>
    /// <returns></returns>
    public Param Add(String name, Object val, Object innerObject) {
      var v_result = this.Add(name, val, false);
      v_result.InnerObject = innerObject;
      return v_result;
    }

    /// <summary>
    /// ��������� �������� � ����� ���������
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public new Param Add(Param item) {
      if (item != null) {
        if (this.ParamByName(item.Name) == null) {
          base.Add(item);
          item.owner = this;
          return item;
        }
        throw new Exception(String.Format("�������� � ������ [{0}] ��� ����������. ����� �������� �������� � ������� ���������� ����� : Add(item, true).", item.Name));
      }
      return null;
    }

    /// <summary>
    /// ��������� �������� � ����� ���������
    /// </summary>
    /// <param name="item"></param>
    /// <param name="replaceIfExists"></param>
    /// <returns></returns>
    public Param Add(Param item, Boolean replaceIfExists) {
      if (item != null) {
        if (this.ParamByName(item.Name, true) == null) {
          base.Add(item);
          item.owner = this;
          return item;
        }
        if (replaceIfExists) {
          var v_insIndx = this.IndexOf(this.ParamByName(item.Name, true));
          this.Remove(item.Name);
          Insert(v_insIndx, item);
          return item;
        }
        return null;
      }
      return null;
    }

    /// <summary>
    /// ��������� �������� � ����� ���������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public Param AddObject(String name, Object obj) {
      Param rslt = null;
      if (this.ParamByName(name) == null) {
        rslt = new Param {
          owner = this,
          Name = name,
          InnerObject = obj
        };
        base.Add(rslt);
      }
      return rslt;
    }

    /// <summary>
    /// ��������� ����� ���������� � ������������� ����������.
    /// </summary>
    /// <param name="prms">����� ����������.</param>
    /// <param name="overwrite">�������������� ��� ��� ��������� � ������������ �������.</param>
    public Params Merge(Params prms, bool overwrite) {
      if ((prms != null) && (prms != this)) {
        lock (prms) {
          foreach (var pp in prms) {
            this.Add(pp.Export(this), overwrite);
          }
        }
      }
      return this;
    }

    /// <summary>
    /// ���������� ������ ��������� �� �����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public Object ObjectByName(String name, bool ignoreCase) {
      var v_param = this.ParamByName(name, ignoreCase);
      return v_param != null ? v_param.InnerObject : null;
    }

    /// <summary>
    /// ���������� �������� ��������� �� �����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public String ValueAsStringByName(String name, bool ignoreCase) {
      var v_param = this.ParamByName(name, ignoreCase);
      return v_param != null ? v_param.ValueAsString() : null;
    }

    /// <summary>
    /// ���������� �������� ��������� �� �����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public Object ValueByName(String name, bool ignoreCase) {
      var v_param = this.ParamByName(name, ignoreCase);
      return v_param != null ? v_param.Value : null;
    }

    /// <summary>
    /// ��������� �������� �� �������������
    /// </summary>
    /// <param name="name">��� ��������� (������������� � ��������)</param>
    /// <returns>null, ���� ��������</returns>
    public Boolean ParamExists(String name) {
      return this.ParamByName(name, false, false) != null;
    }

    /// <summary>
    /// ��������� �������� �� �������������
    /// </summary>
    /// <param name="name">��� ���������</param>
    /// <param name="ignoreCase">�� ��������� false</param>
    /// <returns>null, ���� ��������</returns>
    public Boolean ParamExists(String name, bool ignoreCase) {
      return this.ParamByName(name, ignoreCase, false) != null;
    }


    /// <summary>
    /// ����������� �������� �� �����
    /// </summary>
    /// <param name="name">��� ���������</param>
    /// <returns>null, ���� ��������</returns>
    public Param ParamByName(String name) {
      return this.ParamByName(name, false, false);
    }

    /// <summary>
    /// ����������� �������� �� �����
    /// </summary>
    /// <param name="name">��� ���������</param>
    /// <param name="ignoreCase">�� ��������� false</param>
    /// <returns>null, ���� ��������</returns>
    public Param ParamByName(String name, bool ignoreCase) {
      return this.ParamByName(name, ignoreCase, false);
    }

    /// <summary>
    /// ����������� �������� �� �����
    /// </summary>
    /// <returns></returns>
    public Param ParamByName(String name, bool ignoreCase, bool createIfNotfound) {
      Param v_curParam;
      lock (this) {
        v_curParam =
          (from item in this
           where (ignoreCase ? item.Name.ToLower() : item.Name).Equals(ignoreCase ? name.ToLower() : name)
           select item).FirstOrDefault<Param>();
      }
      if ((v_curParam == null) && createIfNotfound) {
        v_curParam = new Param {Name = name};
        this.Add(v_curParam);
      }
      return v_curParam;
    }

    /// <summary>
    /// ������������� �������� ������������� ���������, 
    /// ���� �������� �� ����������, �� ��������� ���.
    /// </summary>
    /// <param name="name">��� ���������. �� ������������� � ��������.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Param SetValue(String name, Object value) {
      return SetValue(this, name, value);
    }

    /// <summary>
    /// ������������� �������� ������������� ���������, 
    /// ���� �������� �� ����������, �� ��������� ���.
    /// </summary>
    /// <param name="inParams">��������� ����������.</param>
    /// <param name="name">��� ���������. �� ������������� � ��������.</param>
    /// <param name="value"></param>
    public static Param SetValue(Params inParams, String name, Object value) {
      if (inParams != null) {
        var v_param = inParams.ParamByName(name, true, true);
        v_param.Value = value;
        return v_param;
      }
      return null;
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    public new virtual void Clear() {
      lock (this) {
        base.Clear();
      }
    }

    /// <summary>
    /// ���������� ��������� ��� Dictionary(key, value), ��� value = Serialized Object of prm.Value
    /// </summary>
    /// <returns></returns>
    public Dictionary<String, String> ToDict() {
      var rslt = new Dictionary<String, String>();
      foreach (var prm in this) {
        if (prm != null) {
          String val;
          if (prm.Value is string)
            val = prm.Value as String;
          else
            val = JsonConvert.SerializeObject(prm.Value);
          rslt.Add(prm.Name, val);
        }
      }
      return rslt;
    }


    /// <summary>
    /// ������� ������, ������� ����� ���������� � URL
    /// </summary>
    /// <returns></returns>
    public String bldUrlParams() {
      String rslt = null;
      foreach (var prm in this) {
        var v_paramStr = prm.Name + "=" + HttpUtility.UrlEncode(prm.ValueAsString());
        Utl.AppendStr(ref rslt, v_paramStr, "&");
      }
      return rslt;
    }

    /// <summary>
    /// ������� ������, ������� ����� ���������� � URL
    /// </summary>
    /// <param name="pBaseURL"></param>
    /// <returns></returns>
    public String bldUrlParams(String pBaseURL) {
      var rslt = this.bldUrlParams();
      return (pBaseURL.IndexOf('?') >= 0) ? pBaseURL + "&" + rslt : pBaseURL + "?" + rslt;
    }

    /// <summary>
    /// ������������ ��������� � ���� ������
    /// </summary>
    /// <returns></returns>
    public override String ToString() {
      String rslt = null;
      foreach (var prm in this)
        Utl.AppendStr(ref rslt, prm.ToString(), ";");
      return rslt ?? String.Empty;
    }

    /// <summary>
    /// ������������ ��������� � ���� Json-������
    /// </summary>
    /// <returns></returns>
    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] {new EBioExceptionConverter()});
    }

    /// <summary>
    /// �������������� ������ �� Json-������
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public static Params Decode(String jsonString) {
      return jsonUtl.decode<Params>(jsonString, new JsonConverter[] {new EBioExceptionConverter()});
    }

    /// <summary>
    /// �������� ������
    /// </summary>
    /// <returns></returns>
    public Object Clone() {
      var v_result = new Params();
      lock (this) {
        foreach (ICloneable prm in this)
          v_result.Add((Param) prm.Clone());
      }
      return v_result;
    }

    /// <summary>
    /// ���� �������� � ��������� ������� 
    /// </summary>
    /// <param name="prms">���� null, �� � ���������� ����� null</param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static Object FindParamValue(Params prms, String paramName) {
      return FindParamValue<Object>(prms, paramName);
    }

    /// <summary>
    /// ���� �������� � ���������
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="paramName"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FindParamValue<T>(Params prms, String paramName, T defaultValue) {
      var v_result = defaultValue;
      if (prms != null) {
        var v_val = prms.ValueByName(paramName, true);
        v_result = v_val != null ? Utl.Convert2Type<T>(v_val) : defaultValue;
      }
      return v_result;
    }

    /// <summary>
    /// ���� �������� � ���������
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="paramName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FindParamValue<T>(Params prms, String paramName) {
      return FindParamValue(prms, paramName, default(T));
    }

    /// <summary>
    /// ������� ����� ��������� ���� prms == null, ����� ���������� prms
    /// </summary>
    /// <param name="prms"></param>
    /// <returns></returns>
    public static Params CreNewIfNull(Params prms) {
      return prms ?? new Params();
    }

    /// <summary>
    /// ����������, �������� �� ��������� �������� � ���������� ������ � ���������.
    /// </summary>
    /// <param name="paramName">��� ���������.</param>
    /// <param name="paramValue">�������� ���������.</param>
    /// <returns>true - ���� ��������, ����� false.</returns>
    public bool Contains(string paramName, object paramValue) {
      foreach (var v_param in this) {
        if (v_param.Name == paramName && (v_param.Value == null && paramValue == null || v_param.Value != null && v_param.Value.Equals(paramValue))) return true;
      }
      return false;
    }

    /// <summary>
    /// ����������, �������� �� ��������� �������� � ������ � ���������, ������������ � ���������.
    /// </summary>
    /// <param name="param">��������, ������� ����� �������� � ���������.</param>
    /// <returns>true - ���� ��������, ����� false.</returns>
    public new bool Contains(Param param) {
      return param != null && this.Contains(param.Name, param.Value);
    }

    /// <summary>
    /// ����������, �������� �� ��������� ��� ��������� �� ��������� ���������.
    /// </summary>
    /// <param name="params">���������, ��������� ������� ����� �������� � �������� ���������.</param>
    /// <returns>true - ���� ��������, ����� false.</returns>
    public bool Contains(Params @params) {
      return @params != null && @params.All(p => this.Contains(p));
    }

    /// <summary>
    /// ��������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    /// <param name="values">�������� ����������</param>
    /// <param name="delimiter" />
    public void AddList(String names, Object[] values, Char delimiter) {
      var strs = Utl.SplitString(names, delimiter);
      for (var i = 0; i < strs.Length; i++)
        if (i < values.Length)
          this.Add(strs[i], values[i]);
    }

    /// <summary>
    /// ������������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    /// <param name="values">�������� ����������</param>
    /// <param name="delimiter" />
    public void SetList(String names, Object[] values, Char delimiter) {
      var strs = Utl.SplitString(names, delimiter);
      for (var i = 0; i < strs.Length; i++)
        if (i < values.Length)
          this.SetValue(strs[i], values[i]);
    }

    /// <summary>
    /// ��������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    /// <param name="values">�������� ����������</param>
    public void AddList(String names, Object[] values) {
      this.AddList(names, values, '/');
    }

    /// <summary>
    /// ��������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    /// <param name="values">�������� ���������� ����������� "/"</param>
    /// <param name="delimiter" />
    public void AddList(String names, String values, Char delimiter) {
      var v_strsVals = Utl.SplitString(values, delimiter);
      this.AddList(names, v_strsVals, delimiter);
    }

    /// <summary>
    /// ������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    /// <param name="delimiter"></param>
    public void RemoveList(String names, Char delimiter) {
      var strs = Utl.SplitString(names, delimiter);
      foreach (var v in strs)
        this.Remove(v);
    }

    /// <summary>
    /// ������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    public void RemoveList(String names) {
      this.RemoveList(names, '/');
    }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    /// <param name="names"></param>
    /// <param name="values"></param>
    /// <param name="delimiter"></param>
    public void SetList(String names, String values, Char delimiter) {
      var v_strsVals = Utl.SplitString(values, delimiter);
      this.SetList(names, v_strsVals, delimiter);
    }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    /// <param name="names"></param>
    /// <param name="values"></param>
    public void SetList(String names, String values) {
      this.SetList(names, values, '/');
    }

    /// <summary>
    /// ��������� �������� � ������
    /// </summary>
    /// <param name="names">����� ���������� ����������� "/"</param>
    /// <param name="values">�������� ���������� ����������� "/"</param>
    public void AddList(String names, String values) {
      this.AddList(names, values, '/');
    }

    #endregion


  }

  #endregion

}
