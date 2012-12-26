namespace Bio.Helpers.Common.Types {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using System.Collections;
  using Bio.Helpers.Common;

  /// <summary>
  /// Уровень доступа
  /// </summary>
  /*public enum TBioAccess { 
    /// <summary>
    /// нет доступа
    /// </summary>
    bioacNone=0, 
    /// <summary>
    /// только чтение
    /// </summary>
    bioacRead=1, 
    /// <summary>
    /// полный доступ
    /// </summary>
    bioacReadWrite=2 
  };*/

  /// <summary>
  /// Глобальные константы для системы доступа
  /// </summary>
  public class CBioAcc {
    /// <summary>
    /// Данная роль прошита в логику. Изменение невозможно
    /// </summary>
    public static String BIOADMIN_ROLE_NAME = "BIO-ADMIN";
    public static String BIODEBUGGER_GRANT_NAME = "DBG";
    ///// <summary>
    ///// Данная роль прошита в логику. Изменение невозможно
    ///// </summary>
    //public static String BIOWSADMIN_ROLE_NAME = "BIO-WSADMIN";

  }

  /// <summary>
  /// Описание пользователя
  /// </summary>
  public class CBioUser: ICloneable {
    /// <summary>
    /// UID пользователя
    /// </summary>
    public String USR_UID { get; set; }
    /// <summary>
    /// IP пользователя
    /// </summary>
    public String USR_IP { get; set; }
    /// <summary>
    /// UID города	
    /// </summary>
    //public String CITY_UID { get; set; }
    /// <summary>
    /// Название города
    /// </summary>
    //public String CITY_NAME { get; set; }
    /// <summary>
    /// UID округа	
    /// </summary>
    //public String CDEP_UID { get; set; }
    /// <summary>
    /// Название округа	
    /// </summary>
    //public String CDEP_NAME { get; set; }
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public String Role { get; set; }
    /// <summary>
    /// Список разрешений пользователя
    /// </summary>
    public String[] Grants { get; set; }
    /// <summary>
    /// Логин
    /// </summary>
    public String USR_NAME { get; set; }
    /// <summary>
    /// Пароль
    /// </summary>
    //public String USR_PWD { get; set; }
    /// <summary>
    /// Фамилия
	  /// </summary>
    public String USR_FAM { get; set; }	
    /// <summary>
    /// Имя
    /// </summary>
    public String USR_FNAME { get; set; }
	  /// <summary>
    /// Отчество
	  /// </summary>
    public String USR_SNAME { get; set; }	
    /// <summary>
    /// Дата регистрации
    /// </summary>
    public DateTime REG { get; set; }
	  /// <summary>
    /// Заблокирован
	  /// </summary>
    //public bool BLOCKED { get; set; }
	  /// <summary>
    /// e-mail
	  /// </summary>
    public String EMAIL { get; set; }
	  /// <summary>
    /// Телефон
	  /// </summary>
    public String PHONE { get; set; }	
    /// <summary>
    /// UID организации
    /// </summary>
    //public String ORG_UID { get; set; }	
    /// <summary>
    /// Название организации
    /// </summary>
    //public String ORG_NAME { get; set; }	
    /// <summary>
    /// UID отдела
    /// </summary>
    public String ODEP_UID { get; set; }
    /// <summary>
    /// Название отдела
    /// </summary>
    public String ODEP_NAME { get; set; }
    /// <summary>
    /// Описание отдела
    /// </summary>
    public String ODEP_DESC { get; set; }
    /// <summary>
    /// Активирован
    /// </summary>
    //public bool CONFIRMED { get; set; }
    /// <summary>
    /// Удален в карзину
    /// </summary>
    //public bool GARBAGED { get; set; }

    /// <summary>
    /// Полный путь подразделения
    /// </summary>
    public String ODepPath { get; set; }

    /// <summary>
    /// Полный путь идентификаторов подразделения
    /// </summary>
    public String[] ODepUidPath { get; set; }

    /// <summary>
    /// Полный путь идентификаторов подразделения
    /// Перечисление UIDов подразделений разделенных разделителем ['/']
    /// </summary>
    public String ODepUidPathStr { get; set; }

    /// <summary>
    /// Заполняет список идентификаторов подразделений в пути
    /// </summary>
    /// <param name="pODepPath">Перечисление UIDов подразделений разделенных разделителем ['/']</param>
    public void parsODepUidPath(String pODepPath) {
      this.ODepUidPath = Utl.SplitString(pODepPath, new Char[] { '/' });
    }

    /// <summary>
    /// Проверяет : принадлежит ли пользователь данному подразделению
    /// </summary>
    /// <param name="pODEP_UID"></param>
    /// <returns></returns>
    public Boolean memberOfDep(String pODEP_UID) {
      foreach(String vItem in this.ODepUidPath)
        if(String.Equals(pODEP_UID, vItem, StringComparison.OrdinalIgnoreCase)) {
          return true;
        }
      return false;
    }

    /// <summary>
    /// Заполняет список разрешений пользователя
    /// </summary>
    /// <param name="rolesList">Перечисление разрешений разделенных разделителем [' ' | ';' | ',']</param>
    public void parsGrants(String grantsList) {
      this.Grants = Utl.SplitString(grantsList, new Char[] { ' ', ';', ',' });
    }

    /// <summary>
    /// Проверяет : имеет-ли пользователь данную роль
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public Boolean HasGrant(Object grant) {
      String v_grant = "" + grant;
      return this.checkAnGrant(v_grant);
    }

    ///// <summary>
    ///// Проверяет : имеет-ли пользователь хотябы одну из перечисленных ролей
    ///// </summary>
    ///// <param name="roles">Список ролей с разделителем [' ' | ';' | ',']</param>
    ///// <returns></returns>
    //public Boolean HasOneOfRoles(String roles) {
    //  String[] vRoles = Utl.SplitString(roles, new Char[] { ' ', ';', ',' });
    //  for(int i = 0; i < vRoles.Length; i++)
    //    if(this.HasRole(vRoles[i]))
    //      return true;
    //  return false;
    //}

    public Boolean CheckRoles<T>(params T[] roles) {
      String v_roles = null;
      if (roles != null) {
        foreach (var n in roles) {
          var fi = enumHelper.GetFieldInfo(n);
          String v_role = n.GetType().IsEnum ? "" + fi.GetRawConstantValue() : "" + n;
          Utl.appendStr(ref v_roles, v_role, ";");
        }
      }
      return this.CheckRoles(v_roles);
    }

    /// <summary>
    /// Проверяет данное разрешение
    /// </summary>
    /// <param name="grant"></param>
    /// <returns></returns>
    private Boolean checkAnGrant(String grant) {
      if (!String.IsNullOrEmpty(grant)) {
        foreach (var gr in this.Grants)
          if (String.Equals(gr, grant, StringComparison.CurrentCultureIgnoreCase) || grant.Equals("*")) {
            return true;
          }
      }
      return false;
    }
    /// <summary>
    /// Проверяет сразу несколько ролей
    /// </summary>
    /// <param name="roles">Список ролей с разделителем [' ' | ';' | ',']</param>
    /// <returns></returns>
    public Boolean CheckRoles(String roles) {
      if(String.IsNullOrEmpty(roles))
        return false;
      String[] vRoles = Utl.SplitString(roles, new Char[] { ' ', ';', ',' });
      foreach(var r in vRoles) {
        if (String.Equals(r, this.Role, StringComparison.CurrentCultureIgnoreCase)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Проверяет сразу несколько разрешений
    /// </summary>
    /// <param name="grants">Список разрешений с разделителем [' ' | ';' | ',']</param>
    /// <returns></returns>
    public Boolean CheckGrants(String grants) {
      if (String.IsNullOrEmpty(grants))
        return true;
      String[] v_grants = Utl.SplitString(grants, new Char[] { ' ', ';', ',' });
      foreach (var gr in v_grants) {
        if (this.checkAnGrant(gr)) {
          return true;
        }
      }
      return false;
    }

#if !SILVERLIGHT
    /// <summary>
    /// Проверяет роли XML-элемента
    /// </summary>
    /// <param name="pControlDefinition"></param>
    /// <returns></returns>
    public Boolean CheckRoules(XmlElement pControlDefinition) {
      String vRoulesOfControl = pControlDefinition.GetAttribute("roles");
      return this.CheckRoles(vRoulesOfControl);
    }
#endif

    /// <summary>
    /// Пользователь имеет наивысший уровень доступа
    /// </summary>
    public Boolean isBioAdmin() {
      return String.Equals(this.Role, CBioAcc.BIOADMIN_ROLE_NAME, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Пользователь имеет разрешение "Отладчика", т.е. данный пользователь будет видеть отладочную информацию в сообщениях об ошибке
    /// </summary>
    public Boolean isDebugger() {
      return this.HasGrant(CBioAcc.BIODEBUGGER_GRANT_NAME);
    }

    #region ICloneable Members
    /// <summary>
    /// Клонировать
    /// </summary>
    /// <returns></returns>
    public object Clone() {
      CBioUser vResult = new CBioUser() { 
        //BLOCKED = this.BLOCKED,
        //CDEP_NAME = this.CDEP_NAME,
        //CDEP_UID = this.CDEP_UID,
        //CITY_NAME = this.CITY_NAME,
        //CITY_UID = this.CITY_UID,
        //CONFIRMED = this.CONFIRMED,
        //ConnectionString = this.ConnectionString,
        EMAIL = this.EMAIL,
        USR_FAM = this.USR_FAM,
        USR_FNAME = this.USR_FNAME,
        USR_SNAME = this.USR_SNAME,
        ODEP_NAME = this.ODEP_NAME,
        ODEP_UID = this.ODEP_UID,
        ODepPath = this.ODepPath,
        ODepUidPathStr = this.ODepUidPathStr,
        ODepUidPath = (this.ODepUidPath != null) ? (String[])this.ODepUidPath.Clone() :  null,
        //ORG_NAME = this.ORG_NAME,
        //ORG_UID = this.ORG_UID,
        PHONE = this.PHONE,
        REG = this.REG,
        Role = this.Role,
        Grants = (this.Grants != null) ? (String[])this.Grants.Clone() : null,
        USR_NAME = this.USR_NAME,
        //USR_PWD = this.USR_PWD,
        USR_UID = this.USR_UID
      };
      return vResult;
    }

    #endregion

    /// <summary>
    /// Представляет объкт в виде Json-строки
    /// </summary>
    /// <returns></returns>
    public String Encode() {
      return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
    }
    public static CBioUser Decode(String pJsonString) {
      return jsonUtl.decode<CBioUser>(pJsonString);
    }
  }

}
