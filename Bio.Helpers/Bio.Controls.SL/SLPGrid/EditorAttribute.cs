
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System;

  #endregion

  #region EditorAttribute
  /// <summary>
  /// EditorAttribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class EditorAttribute : Attribute {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="type">The AssemblyQualifiedName of the type that must inherit from <see cref="PropertyEditor"/></param>
    public EditorAttribute(Type type) {
      if (type == null) throw new ArgumentNullException("type");
      if (!type.IsSubclassOf(typeof(PropertyEditor)))
        throw new Exception(String.Format("Тип {0} должен быть наследником по отношению к {1}.", type, typeof(PropertyEditor)));
      this.EditorType = type;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the Editors TypeName
    /// </summary>
    public Type EditorType { get; private set; }
    #endregion

    #region Overrides
    /// <summary>
    /// Checks for equality
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) {
      if (obj == this) {
        return true;
      }
      EditorAttribute attribute = obj as EditorAttribute;
      return (((attribute != null) && (attribute.EditorType == this.EditorType)));
    }
    /// <summary>
    /// Gets the hashcode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() {
      return base.GetHashCode();
    }
    #endregion
  }
  #endregion
}
