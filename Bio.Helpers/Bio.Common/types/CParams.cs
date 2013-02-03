namespace Bio.Helpers.Common.Types {
  using System;
#if !SILVERLIGHT
  using System.Web;
  using System.Data;
#else
  using System.Windows.Browser;
#endif
  using System.Collections;
  using System.IO;
  using System.Linq;
  using System.Collections.Generic;
  using Bio.Helpers.Common;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json;
  using System.Collections.Specialized;

  public enum ParamDirection {
    Input,
    Output,
    InputOutput,
    Return
  }
  // *** CParam ***
  #region CParam
  public class CParam: ICloneable {

    public CParam() {
      this.ParamDir = ParamDirection.Input;
    }
    public CParam(String name, Object value) : this() {
      this.Name = name;
      this.Value = value;
    }
    public CParam(String name, Object value, Type paramType, ParamDirection paramDirection)
      : this(name, value) {
      this.ParamType = paramType;
      this.ParamDir = paramDirection;
    }
    public CParam(String name, Object value, Type paramType, int paramSize, ParamDirection paramDirection)
      : this(name, value, paramType, paramDirection) {
        this.ParamSize = paramSize;
    }

    internal CParams owner { get; set; }
    public String Name { get; set; }
    public Object Value { get; set; }

    public String ValueAsString() {
      return Utl.ObjectAsString(this.Value);
    }

    public Object InnerObject { get; set; }
    public Object InnerObjectEx { get; set; }

    public Type ParamType { get; set; }
    public Int32 ParamSize { get; set; }
    public ParamDirection ParamDir { get; set; }
		public Int32 GetParamIndex(){
      return this.owner.IndexOf(this);
		}

    public void Kill() {
      this.owner.Remove(this);
    }

    public override String ToString() {
      String vInnrObjStr = (this.InnerObject == null) ? null : "o:"+this.InnerObject.ToString();
      String vInnrObjExStr = (this.InnerObjectEx == null) ? null : "oex"+this.InnerObjectEx.ToString();
      String vObjsStr = null;
      if(!String.IsNullOrEmpty(vInnrObjStr))
        Utl.AppendStr(ref vObjsStr, vInnrObjStr, ";");
      if(!String.IsNullOrEmpty(vInnrObjExStr))
        Utl.AppendStr(ref vObjsStr, vInnrObjExStr, ";");
      if(!String.IsNullOrEmpty(vObjsStr))
        vObjsStr = "("+vObjsStr+")";
      String valStr = String.Format("{0}{1}", this.Value, vObjsStr);
      return String.Format("({0}=[{1}]; tp:{2}; sz:{3}; dr:{4})", 
        this.Name, valStr, this.ParamType, this.ParamSize, this.ParamDir);
    }

    internal CParam Export(CParams newOwner) {
      CParam vRslt = (CParam)this.Clone();
      vRslt.owner = newOwner;
      return vRslt;
    }

    #region ICloneable Members

    public Object Clone() {
      CParam vRslt = new CParam() {
        owner = this.owner,
        Name = this.Name,
        Value = (this.Value is ICloneable) ? (this.Value as ICloneable).Clone() : this.Value,
        InnerObject = (this.InnerObject is ICloneable) ? (this.InnerObject as ICloneable).Clone() : this.InnerObject,
        InnerObjectEx = (this.InnerObjectEx is ICloneable) ? (this.InnerObjectEx as ICloneable).Clone() : this.InnerObjectEx,
        ParamType = this.ParamType,
        ParamSize = this.ParamSize,
        ParamDir = this.ParamDir
      };
      
      return vRslt;
    }

    #endregion
  }
  #endregion

  // *** CParams ***
  #region CParams
  public class CParams : List<CParam> {

    #region Constructors

    public CParams(){
    }

#if !SILVERLIGHT
    public CParams(NameValueCollection prms)
      : this() {
      foreach (String k in prms.AllKeys)
        this.Add(k, prms[k]);
    }
#endif

    public CParams(params CParam[] paramsArray)
      : this() {
        for (int i = 0; i < paramsArray.Length; i++) {
          base.Add(paramsArray[i]);
      }
    }

    /// <summary>
    /// Подготавливает экземпляр параметров к использованию, если null, то создает новый,
    /// если не null, то перегружает его из overloadPrms с перезаписью
    /// </summary>
    /// <param name="prms"></param>
    /// <param name="overloadPrms"></param>
    public static CParams PrepareToUse(CParams prms, CParams overloadPrms) {
      if (overloadPrms != null) {
        if (prms == null)
          prms = overloadPrms.Clone() as CParams;
        else {
          prms.Clear();
          prms = prms.Merge(overloadPrms, true);
        }
      } else {
        if (prms == null)
          prms = new CParams();
      }
      return prms;
    }

    #endregion

    #region Properties

    public CParam this[String name] {
      get {
        return this.ParamByName(name);
      }
    }

    public virtual String NamesAsDelimitedStrings {
      get {
        String rslt = null;
        for (int i = 0; i < this.Count; i++)
          Utl.AppendStr(ref rslt, "\"" + this[i].Name + "\"", ",");
        return rslt;
      }
    }

    public virtual String ValsAsDelimitedStrings {
      get {
        String rslt = null;
        for (int i = 0; i < this.Count; i++)
          Utl.AppendStr(ref rslt, "\"" + this[i].ValueAsString() + "\"", ",");
        return rslt;
      }
    }

    #endregion

    #region Public methods

    public int IndexOf(String itemName) {
      return base.IndexOf(this.ParamByName(itemName));
    }

    public CParam Remove(CParam item) {
      lock (this) {
        base.Remove(item);
        return item;
      }
    }

    public CParam Remove(String itemName) {
      lock (this) {
        CParam item = this.Remove(this.ParamByName(itemName));
        return item;
      }
    }

    public CParam Add(String name, Object val, Boolean replaceIfExists) {
      lock (this) {
        CParam existsPar = this.ParamByName(name);
        if ((existsPar != null) && replaceIfExists) {
          this.Remove(existsPar);
        }
        if (this.ParamByName(name) == null) {
          CParam rslt = new CParam() {
            owner = this,
            Name = name,
            Value = val
          };
          base.Add(rslt);
          return rslt;
        } else
          return null;
      }
    }

    public CParam Add(String name, Object val) {
      return this.Add(name, val, false);
    }

    public CParam Add(String name, Object val, Object innerObject) {
      CParam vResult = this.Add(name, val, false);
      vResult.InnerObject = innerObject;
      return vResult;
    }

    public CParam Add(CParam item) {
      if (item != null) {
        if (this.ParamByName(item.Name) == null) {
          base.Add(item);
          item.owner = this;
          return item;
        } else
          throw new Exception(String.Format("Параметр с именем [{0}] уже существует. Чтобы добавить параметр с заменой используте метод : Add(item, true).", item.Name));
      } else
        return null;
    }
    public CParam Add(CParam item, Boolean replaceIfExists) {
      if (item != null) {
        if (this.ParamByName(item.Name, true) == null) {
          base.Add(item);
          item.owner = this;
          return item;
        } else {
          if (replaceIfExists) {
            int insIndx = this.IndexOf(this.ParamByName(item.Name, true));
            this.Remove(item.Name);
            base.Insert(insIndx, item);
            return item;
          } else
            return null;
        }
      } else
        return null;
    }

    public CParam AddObject(String name, Object obj) {
      CParam rslt = null;
      if (this.ParamByName(name) == null) {
        rslt = new CParam() { 
          owner = this,
          Name = name,
          InnerObject = obj
        };
        base.Add(rslt);
      }
      return rslt;
    }

    /// <summary>
    /// Добавляет набор параметров к существующему экземпляру.
    /// </summary>
    /// <param name="prms">Набор параметров.</param>
    /// <param name="overwrite">Перезаписывать или нет параметры с совпадающими именами.</param>
    public CParams Merge(CParams prms, bool overwrite) {
      if((prms != null) && (prms != this)) {
        lock(prms) {
          foreach(CParam pp in prms) {
            this.Add(pp.Export(this), overwrite);
          }
        }
      }
      return this;
    }

    public Object ObjByName(String name, bool ignoreCase) {
      CParam vPrm = this.ParamByName(name, ignoreCase);
      return vPrm != null ? vPrm.InnerObject : null;
    }

    public String ValAsStrByName(String name, bool ignoreCase) {
      CParam vPrm = this.ParamByName(name, ignoreCase);
      return vPrm != null ? vPrm.ValueAsString() : null;
    }

    public Object ValByName(String name, bool ignoreCase) {
      CParam vPrm = this.ParamByName(name, ignoreCase);
      return vPrm != null ? vPrm.Value : null;
    }

    /// <summary>
    /// Проверяет параметр на существование
    /// </summary>
    /// <param name="name">Имя параметра (Чувствительно к регистру)</param>
    /// <returns>null, если ненайден</returns>
    public Boolean ParamExists(String name) {
      return this.ParamByName(name, false, false) != null;
    }
    /// <summary>
    /// Проверяет параметр на существование
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <param name="ignoreCase">По умолчанию false</param>
    /// <returns>null, если ненайден</returns>
    public Boolean ParamExists(String name, bool ignoreCase) {
      return this.ParamByName(name, ignoreCase, false) != null;
    }


    /// <summary>
    /// Вытаскивает параметр по имени
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <returns>null, если ненайден</returns>
    public CParam ParamByName(String name) {
      return this.ParamByName(name, false, false);
    }

    /// <summary>
    /// Вытаскивает параметр по имени
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <param name="ignoreCase">По умолчанию false</param>
    /// <returns>null, если ненайден</returns>
    public CParam ParamByName(String name, bool ignoreCase) {
      return this.ParamByName(name, ignoreCase, false);
    }

    /// <summary>
    /// Вытаскивает параметр по имени
    /// </summary>
    /// <param name="pName">Имя параметра</param>
    /// <param name="pIgnoreCase">По умолчанию false</param>
    /// <param name="pCreIfNotfound">По умолчанию false</param>
    /// <returns></returns>
		public CParam ParamByName(String name, bool ignoreCase, bool creIfNotfound){
      CParam curParam = null;
      lock(this) {
        //foreach(CParam curParam in this) {
        //  String vCurPrnName = ignoreCase ? curParam.Name.ToLower() : curParam.Name;
        //  String vSeqName = ignoreCase ? name.ToLower() : name;
        //  if(vCurPrnName.Equals(vSeqName)) {
        //    return curParam;
        //  }
        //}
        curParam =
            (from item in this
            where (ignoreCase ? item.Name.ToLower() : item.Name).Equals(ignoreCase ? name.ToLower() : name)
            select item).FirstOrDefault<CParam>();
      }
      if ((curParam == null) && creIfNotfound) {
        curParam = new CParam() { Name = name };
        this.Add(curParam);
      }
      return curParam;
    }

    /// <summary>
    /// Устанавливает значение существующего параметра, 
    /// если параметр не существует, то добавляет его.
    /// </summary>
    /// <param name="name">Имя параметра. Не чувствительно к регистру.</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public CParam SetValue(String name, Object value) {
      return SetValue(this, name, value);
    }

    /// <summary>
    /// Устанавливает значение существующего параметра, 
    /// если параметр не существует, то добавляет его.
    /// </summary>
    /// <param name="pParams">Коллекция параметров.</param>
    /// <param name="pName">Имя параметра. Не чувствительно к регистру.</param>
    /// <param name="pValue"></param>
    public static CParam SetValue(CParams inParams, String name, Object value) {
      if (inParams != null) {
        CParam vPrm = inParams.ParamByName(name, true, true);
        vPrm.Value = value;
        return vPrm;
      } else
        return null;
    }

    /// <summary>
    /// Объект синхронизации
    /// </summary>
    //public Object SyncRoot {
    //  get { 
    //    return null; 
    //  }
    //}

    /// <summary>
    /// Очищает список
    /// </summary>
		public virtual void Clear(){
      lock (this) {
        base.Clear();
      }
		}

    public Dictionary<String, String> ToDict() {
      Dictionary<String, String> rslt = new Dictionary<String, String>();
      foreach (CParam prm in this) {
        String val = null;
        if ((prm.Value != null) && (prm.Value.GetType() == typeof(String)))
          val = prm.Value as String;
        else
          val = Newtonsoft.Json.JsonConvert.SerializeObject(prm.Value);
        rslt.Add(prm.Name, val);
      }
      return rslt;
    }


    /// <summary>
    /// Создает строку, которую можно подставить в URL
    /// </summary>
    /// <returns></returns>
    public String bldUrlParams() {
      String rslt = null;
      foreach(CParam prm in this) {
        String vParamStr = prm.Name + "=" + HttpUtility.UrlEncode(prm.ValueAsString());
        Utl.AppendStr(ref rslt, vParamStr, "&");
      }
      return rslt;
    }

    /// <summary>
    /// Создает строку, которую можно подставить в URL
    /// </summary>
    /// <param name="pBaseURL"></param>
    /// <returns></returns>
    public String bldUrlParams(String pBaseURL) {
      String rslt = this.bldUrlParams();
      return (pBaseURL.IndexOf('?') >= 0) ? pBaseURL + "&" + rslt : pBaseURL + "?" + rslt;
    }

    /// <summary>
    /// Представляет параметры в виде строки
    /// </summary>
    /// <returns></returns>
    public override String ToString() {
      String rslt = null;
      foreach(CParam prm in this) 
        Utl.AppendStr(ref rslt, prm.ToString(), ";");
      return rslt;
    }

    /// <summary>
    /// Представляет параметры в виде Json-строки
    /// </summary>
    /// <returns></returns>
    public String Encode() {
      return jsonUtl.encode(this, new JsonConverter[] { new EBioExceptionConverter() });
    }
    /// <summary>
    /// Восстанвливает объект из Json-строки
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public static CParams Decode(String jsonString) {
      return jsonUtl.decode<CParams>(jsonString, new JsonConverter[] { new EBioExceptionConverter() });
    }

    /// <summary>
    /// Копирует объект
    /// </summary>
    /// <returns></returns>
    public Object Clone() {
      CParams vResult = new CParams();
      lock(this) {
        foreach (ICloneable prm in this)
          vResult.Add((CParam)prm.Clone());
      }
      return vResult;
    }

    /// <summary>
    /// Ищет значение в указанном объекте 
    /// </summary>
    /// <param name="prms">если null, то в результате будет null</param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public static Object FindParamValue(CParams prms, String paramName){
      return FindParamValue<Object>(prms, paramName);
    }

    public static T FindParamValue<T>(CParams prms, String paramName, T defaultValue) {
      T vResult = defaultValue;
      if (prms != null) {
        Object v_val = prms.ValByName(paramName, true);
        if (v_val != null)
          vResult = Utl.Convert2Type<T>(v_val);
        else
          vResult = defaultValue;
      }
      return vResult;
    }

    public static T FindParamValue<T>(CParams prms, String paramName) {
      return FindParamValue<T>(prms, paramName, default(T));
    }

    /// <summary>
    /// Создает новый экземпляр если prms == null, иначе возвращает prms
    /// </summary>
    /// <param name="pParams"></param>
    /// <returns></returns>
    public static CParams CreNewIfNull(CParams prms) {
      return prms ?? new CParams();
    }

    /// <summary>
    /// Определяет, содержит ли коллекция параметр с указанными именем и значением.
    /// </summary>
    /// <param name="paramName">Имя параметра.</param>
    /// <param name="paramValue">Значение параметра.</param>
    /// <returns>true - если содержит, иначе false.</returns>
    public bool Contains(string paramName, object paramValue) {
      return
        this.Cast<CParam>().Any(
          p =>
          p.Name == paramName &&
          (p.Value == null && paramValue == null || p.Value != null && p.Value.Equals(paramValue)));
    }

    /// <summary>
    /// Определяет, содержит ли коллекция параметр с именем и значением, совпадающими с указанным.
    /// </summary>
    /// <param name="param">Параметр, который будет искаться в коллекции.</param>
    /// <returns>true - если содержит, иначе false.</returns>
    public bool Contains(CParam param) {
      return param != null && this.Contains(param.Name, param.Value);
    }

    /// <summary>
    /// Определяет, содержит ли коллекция все параметры из указанной коллекции.
    /// </summary>
    /// <param name="params">Коллекция, параметры которой будут искаться в исходной коллекции.</param>
    /// <returns>true - если содержит, иначе false.</returns>
    public bool Contains(CParams @params) {
      return @params != null && @params.Cast<CParam>().All(p => this.Contains(p));
    }

    /// <summary>
    /// Добавляет значения в список
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    /// <param name="values">Значения параметров</param>
    public void AddList(String names, Object[] values, Char delimiter) {
      String[] strs = Utl.SplitString(names, delimiter);
      for (int i = 0; i < strs.Length; i++)
        if (i < values.Length) 
          this.Add(strs[i], values[i]);
    }

    /// <summary>
    /// Устанавливает значения в списке
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    /// <param name="values">Значения параметров</param>
    public void SetList(String names, Object[] values, Char delimiter) {
      String[] strs = Utl.SplitString(names, delimiter);
      for (int i = 0; i < strs.Length; i++)
        if (i < values.Length)
          this.SetValue(strs[i], values[i]);
    }

    /// <summary>
    /// Добавляет значения в список
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    /// <param name="values">Значения параметров</param>
    public void AddList(String names, Object[] values) {
      this.AddList(names, values, '/');
    }

    /// <summary>
    /// Добавляет значения в список
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    /// <param name="values">Значения параметров разделенные "/"</param>
    public void AddList(String names, String values, Char delimiter) {
      String[] strsVals = Utl.SplitString(values, delimiter);
      this.AddList(names, strsVals, delimiter);
    }

    /// <summary>
    /// Удаляет значения в списке
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    public void RemoveList(String names, Char delimiter) {
      String[] strs = Utl.SplitString(names, delimiter);
      for (int i = 0; i < strs.Length; i++)
        this.Remove(strs[i]);
    }

    /// <summary>
    /// Удаляет значения в списке
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    public void RemoveList(String names) {
      this.RemoveList(names, '/');
    }

    public void SetList(String names, String values, Char delimiter) {
      String[] strsVals = Utl.SplitString(values, delimiter);
      this.SetList(names, strsVals, delimiter);
    }

    public void SetList(String names, String values) {
      this.SetList(names, values, '/');
    }

    /// <summary>
    /// Добавляет значения в список
    /// </summary>
    /// <param name="names">Имена параметров разделенные "/"</param>
    /// <param name="values">Значения параметров разделенные "/"</param>
    public void AddList(String names, String values) {
      this.AddList(names, values, '/');
    }

    #endregion


  }
  #endregion

}
