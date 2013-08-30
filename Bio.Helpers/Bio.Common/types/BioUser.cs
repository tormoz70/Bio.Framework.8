using System.ComponentModel;

namespace Bio.Helpers.Common.Types {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using System.Collections;
  using Bio.Helpers.Common;

#pragma warning disable 1591
  public class DbValueAttribute : Attribute {
    public String Value { get; private set; }
    public DbValueAttribute(String value) {
      this.Value = value;
    }
  }
  public class DbFieldAttribute : Attribute {
    public String Name { get; private set; }
    public DbFieldAttribute(String name) {
      this.Name = name;
    }
  }

  public enum BioRoles {
    [DbValue("BIOROOT")]
    BioRoot,
    [DbValue("DEBUGGER")]
    Debugger,
    [DbValue("ADMIN")]
    Admin,
    [DbValue("WSADMIN")]
    WSAdmin,
    [DbValue("USER")]
    User,
    [DbValue("GUEST")]
    Guest
  };

  public enum BioGrants {
    [DbValue("DBG")]
    Debuging
  };

#pragma warning restore 1591

  /// <summary>
  /// Описание пользователя
  /// </summary>
  public class BioUser: ICloneable {
    /// <summary>
    /// UID пользователя
    /// </summary>
    [DbField("usr_uid")]
    public String UID { get; set; }
    /// <summary>
    /// Логин
    /// </summary>
    [DbField("usr_login")]
    public String Login { get; set; }
    /// <summary>
    /// Фамилия
    /// </summary>
    [DbField("fio_fam")]
    public String FioFam { get; set; }
    /// <summary>
    /// Имя
    /// </summary>
    [DbField("fio_name")]
    public String FioName { get; set; }
    /// <summary>
    /// Отчество
    /// </summary>
    [DbField("fio_mname")]
    public String FioMiddleName { get; set; }
    /// <summary>
    /// Дата регистрации
    /// </summary>
    [DbField("reg_date")]
    public DateTime Registred { get; set; }
    /// <summary>
    /// Cписок идентификаторов ролей пользователя разделенные ';'
    /// </summary>
    [DbField("usr_roles")]
    public String Roles { get; set; }
    /// <summary>
    /// Список идентификаторов разрешений пользователя разделенные ';'
    /// </summary>
    [DbField("usr_grants")]
    public String Grants { get; set; }
	  /// <summary>
    /// e-mail
	  /// </summary>
    [DbField("email_addr")]
    public String Email { get; set; }
	  /// <summary>
    /// Телефон
	  /// </summary>
    [DbField("usr_phone")]
    public String Phone { get; set; }	
    /// <summary>
    /// id подразделения пользователя
    /// </summary>
    [DbField("org_id")]
    public String OrgID { get; set; }
    /// <summary>
    /// имя подразделения пользователя
    /// </summary>
    [DbField("org_name")]
    public String OrgName { get; set; }
    /// <summary>
    /// описание подразделения пользователя
    /// </summary>
    [DbField("org_desc")]
    public String OrgDesc { get; set; }
    /// <summary>
    /// путь подразделения пользователя (описание)
    /// </summary>
    [DbField("org_path")]
    public String OrgPath { get; set; }
    /// <summary>
    /// путь подразделения пользователя (ids)
    /// </summary>
    [DbField("org_ids_path")]
    public String OrgIdsPath { get; set; }
    /// <summary>
    /// доп инфо
    /// </summary>
    [DbField("extinfo")]
    public String ExtInfo { get; set; }
    /// <summary>
    /// AddressIP пользователя
    /// </summary>
    public String AddressIP { get; set; }
// --------------------------------------------------------------------------

    /// <summary>
    /// Проверяет наличие у пользователя хотябы одной из перечисленных ролей
    /// </summary>
    /// <param name="roles"></param>
    /// <returns></returns>
    public Boolean CheckRoles(params Object[] roles) {
      String v_roles = null;
      if (roles != null) {
        foreach (var n in roles) {
          var role = enumHelper.GetAttributeByValue<DbValueAttribute>(n).Value;
          Utl.AppendStr(ref v_roles, role, ";");
        }
      }
      return this.CheckRoles(v_roles);
    }

    /// <summary>
    /// Проверяет наличие у пользователя хотябы одной из перечисленных ролей
    /// </summary>
    /// <param name="roles">Список ролей с разделителем [' ' | ';' | ',']</param>
    /// <returns></returns>
    public Boolean CheckRoles(String roles) {
      return this.IsBioRoot() || Utl.DelimitedLineHasCommonTags(this.Roles, roles, new[] { ' ', ';', ',' });
    }

    /// <summary>
    /// Проверяет наличие у пользователя хотябы одного из перечисленных разрешений
    /// </summary>
    /// <param name="grants"></param>
    /// <returns></returns>
    public Boolean CheckGrants(params Object[] grants) {
      String v_grants = null;
      if (grants != null) {
        foreach (var n in grants) {
          var grant = enumHelper.GetAttributeByValue<DbValueAttribute>(n).Value;
          Utl.AppendStr(ref v_grants, grant, ";");
        }
      }
      return this.CheckGrants(v_grants);
    }

    /// <summary>
    /// Проверяет наличие у пользователя хотябы одного из перечисленных разрешений
    /// </summary>
    /// <param name="grants">Список разрешений с разделителем [' ' | ';' | ',']</param>
    /// <returns></returns>
    public Boolean CheckGrants(String grants) {
      return Utl.DelimitedLineHasCommonTags(this.Grants, grants, new[] { ' ', ';', ',' });
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
    public Boolean IsBioRoot() {
      var roleName = enumHelper.GetAttributeByValue<DbValueAttribute>(BioRoles.BioRoot).Value;
      return Utl.DelimitedLineHasCommonTags(this.Roles, roleName, new[] { ' ', ';', ',' });
    }

    /// <summary>
    /// Пользователь имеет разрешение "Отладчика", т.е. данный пользователь будет видеть отладочную информацию в сообщениях об ошибке
    /// </summary>
    public Boolean IsDebugger() {
      var roleName = enumHelper.GetAttributeByValue<DbValueAttribute>(BioRoles.Debugger).Value;
      var grantName = enumHelper.GetAttributeByValue<DbValueAttribute>(BioGrants.Debuging).Value;
      return Utl.DelimitedLineHasCommonTags(this.Roles, roleName, new[] { ' ', ';', ',' })
          || Utl.DelimitedLineHasCommonTags(this.Grants, grantName, new[] { ' ', ';', ',' });
    }

    #region ICloneable Members
    /// <summary>
    /// Клонировать
    /// </summary>
    /// <returns></returns>
    public object Clone() {
      var v_result = new BioUser { 
        UID = this.UID,
        Login = this.Login,
        FioFam = this.FioFam,
        FioName = this.FioName,
        FioMiddleName = this.FioMiddleName,
        Registred = this.Registred,
        Roles = this.Roles,
        Grants = this.Grants,
        Email = this.Email,
        Phone = this.Phone,
        OrgID = this.OrgID,
        OrgName = this.OrgName,
        OrgDesc = this.OrgDesc,
        OrgPath = this.OrgPath,
        OrgIdsPath = this.OrgIdsPath,
        ExtInfo = this.ExtInfo,
        AddressIP = this.AddressIP
      };
      return v_result;
    }

    #endregion

    /// <summary>
    /// Представляет объкт в виде Json-строки
    /// </summary>
    /// <returns></returns>
    public String Encode() {
      return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
    }
    public static BioUser Decode(String pJsonString) {
      return jsonUtl.decode<BioUser>(pJsonString);
    }
  }

}
