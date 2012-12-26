
namespace Bio.Framework.Client.SL.JSPropertyGrid {
  #region Using Directives
  using System;

  #endregion

  #region ComboBoxRmtEditorAttribute
  /// <summary>
  /// EditorAttribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class ComboBoxRmtEditorAttribute : Attribute {
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ComboBoxRmtEditorAttribute(String bioCode, String valueFieldName, String displayFieldName) {
      if (String.IsNullOrEmpty(bioCode)) throw new ArgumentNullException("bioCode");
      if (String.IsNullOrEmpty(valueFieldName)) throw new ArgumentNullException("valueFieldName");
      if (String.IsNullOrEmpty(displayFieldName)) throw new ArgumentNullException("displayFieldName");
      this.BioCode = bioCode;
      this.ValueFieldName = valueFieldName;
      this.DisplayFieldName = displayFieldName;
    }
    #endregion

    #region Properties
    /// <summary>
    /// BioCode для запроса на сервер значений
    /// </summary>
    public String BioCode { get; private set; }
    /// <summary>
    /// Имя поля в котором содержатся значения
    /// </summary>
    public String ValueFieldName { get; private set; }
    /// <summary>
    /// Имя поля в котором содержится текст, который будет отображаться в выпадающем списке
    /// </summary>
    public String DisplayFieldName { get; private set; }
    #endregion

  }
  #endregion
}
