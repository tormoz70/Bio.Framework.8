namespace Bio.Framework.Client.SL {
  using System;
  using System.ComponentModel;
  using System.Globalization;
  using System.Reflection;
  using Bio.Helpers.Common.Types;

  /// <summary>
  /// Базовый класс для описания настроек приложения или его модулей.
  /// <para>Наследуемый класс может содержать публичные свойства и поля. Свойства будут видны в редакторе настроек, поля - нет.</para>
  /// <para>Так же наследуемый класс может иметь атрибуты:</para>
  /// <para>- DisplayName - имя, которое будет отображаться слева в дереве.</para>
  /// <para>- Description - описание, которое будет отображаться внизу дерева.</para>
  /// <para>Свойства могут иметь атрибуты:</para>
  /// <para>- Category - имя подгруппы параметров, отображаемое в редакторе.</para>
  /// <para>- DisplayName - имя параметра, отображаемое в редакторе.</para>
  /// <para>- Description - описание параметра, отображаемое в редакторе.</para>
  /// <para>- DefaultValue - значение параметра по-умолчанию.</para>
  /// <para>- Browsable - false - не показывать свойство в редакторе. По-умолчанию true.</para>
  /// <para>- ReadOnly - true - не позволять редактировать свойство в редакторе. По-умолчанию false.</para>
  /// <para>- Invisible - true - не обрабатывать свойство в конфигураторе. По-умолчанию false.</para>
  /// <para>Поля могут иметь атрибуты:</para>
  /// <para>- DefaultValue - значение параметра по-умолчанию.</para>
  /// <para>- Invisible - true - не обрабатывать поле в конфигураторе. По-умолчанию false.</para>
  /// </summary>
  public abstract class ConfigRec : Bio.Helpers.Common.Types.ICloneable, IConfigRec {
    private Attribute[] attrCollection;

    protected ConfigRec() {
      PropertyInfo[] pdc = this.GetType().GetProperties();
      foreach(PropertyInfo pd in pdc) {
        //pd.GetCustomAttributes();
        Object[] attrs = pd.GetCustomAttributes(typeof(DefaultValueAttribute), false);
        DefaultValueAttribute attr = (attrs.Length == 0) ? null : attrs[0] as DefaultValueAttribute;
        if (attr != null)
          pd.SetValue(this, attr.Value, null);
      }
      FieldInfo[] fia = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
      foreach(FieldInfo fi in fia) {
        object[] attrs = fi.GetCustomAttributes(typeof(DefaultValueAttribute), true);
        if (attrs != null && attrs.Length > 0)
          fi.SetValue(this, ((DefaultValueAttribute)attrs[0]).Value);
      }
    }

    /// <summary>
    /// Копирует значения свойств переданного объекта в экземпляр.
    /// </summary>
    /// <param name="newVals">Объект, значения свойств которого будут скопированы.</param>
    public void ApplyFrom(Object pSource) {
      ApplyFromTo(pSource, this);
    }

    /// <summary>
    /// Копирует значения свойства одного объекта в другой.
    /// </summary>
    /// <param name="pSource">Объект, из которого будут взяты значения свойств.</param>
    /// <param name="pDest">Объект, в который будут скопированы значения свойств.</param>
    public static void ApplyFromTo(Object pSource, Object pDest) {
      if (pSource == null)
        throw new ArgumentNullException("pSource");
      if (pDest == null)
        throw new ArgumentNullException("pDest");
      PropertyInfo[] pdc = pSource.GetType().GetProperties();
      PropertyInfo[] newpdc = pDest.GetType().GetProperties();
      foreach (PropertyInfo pd in newpdc) {
        foreach (PropertyInfo npd in pdc)
          if (npd.Name.Equals(pd.Name))
            npd.SetValue(pDest, pd.GetValue(pSource, null), null);
        //newpdc[pd.Name].SetValue(pDest, pd.GetValue(pSource));
      }
    }

    /// <summary>
    /// Метод вызывается перед сохранением параметров в конфигураторе для проверки правильности значений параметров, если необходимо.
    /// </summary>
    /// <returns>true - проверка прошла, выполнить сохранение, иначе false</returns>
    public virtual Boolean ValidateCfg() {
      return true;
    }

    /// <summary>
    /// Возвращает имя класса, заданное атрибутом DisplayNameAttribute.
    /// </summary>
    public String GetDisplayName() {
      return this.GetDescription();
    }

    /// <summary>
    /// Возвращает описание класса, заданное атрибутом DescriptionAttribute.
    /// </summary>
    public String GetDescription() {
      if (this.attrCollection == null)
        this.attrCollection = (Attribute[])this.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false);
      return this.attrCollection.Length == 0 ? String.Empty : (this.attrCollection[0] as DescriptionAttribute).Description;
    }

    #region ICloneable Members

    /// <summary>
    /// Создаёт копию экземпляра.
    /// </summary>
    /// <returns>Копия экземпляра.</returns>
    public object Clone() {
      ConstructorInfo ci = this.GetType().GetConstructor(Type.EmptyTypes);
      if (ci != null) {
        ConfigRec newRec = (ConfigRec)ci.Invoke(null);
        ApplyFromTo(this, newRec);
        FieldInfo[] fia = this.GetType().GetFields();
        FieldInfo[] newfia = newRec.GetType().GetFields();
        foreach(FieldInfo fi in fia) {
          int idx = Array.IndexOf(newfia, fi);
          if (idx >= 0) {
            FieldInfo newfi = (FieldInfo)newfia.GetValue(idx);
            newfi.SetValue(newRec, fi.GetValue(this));
          }
        }
        return newRec;
      }
      throw new InvalidOperationException("Класс '" + this.GetType().FullName + "' не содержит конструктора без параметров.");
    }

    #endregion
  }

  /// <summary>
  /// Класс атрибута определяющего, будет ли поле или свойство обрабатываться процедурами сохранения/загрузки модуля конфигурации.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class InvisibleAttribute : Attribute {
    /// <summary>
    /// Атрибут определяет, будет ли поле или свойство обрабатываться процедурами сохранения/загрузки модуля конфигурации.
    /// </summary>
    /// <param name="pInvisible">true - невидим для конфигуратора.</param>
    public InvisibleAttribute(bool pInvisible) {
      this.Invisible = pInvisible;
    }

    /// <summary>
    /// true - невидим для конфигуратора.
    /// </summary>
    public bool Invisible { get; set; }
  }


}
