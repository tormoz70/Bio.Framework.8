
namespace Bio.Framework.Client.SL.JSPropertyGrid {
  #region Using Directives
  using System;
  using Bio.Helpers.Common.Types;

  #endregion

  public delegate void OnValueSelectedCallback(VSelection selection);

  #region SelectValueEditorAttribute
  /// <summary>
  /// EditorAttribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class SelectValueEditorAttribute : Attribute {

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public SelectValueEditorAttribute(String selectorPluginName, String valueFieldName, String displayFieldName, Boolean isMultiselector) {
      if (String.IsNullOrEmpty(selectorPluginName)) throw new ArgumentNullException("selectorPluginName");
      if (String.IsNullOrEmpty(valueFieldName)) throw new ArgumentNullException("valueFieldName");
      if (String.IsNullOrEmpty(displayFieldName)) throw new ArgumentNullException("displayFieldName");
      this.SelectorPluginName = selectorPluginName;
      this.ValueFieldName = valueFieldName;
      this.DisplayFieldName = displayFieldName;
      this.isMultiselector = isMultiselector;
    }

    public SelectValueEditorAttribute(String selectorPluginName, String valueFieldName, String displayFieldName) :
      this(selectorPluginName, valueFieldName, displayFieldName, false) { 
    }

    #endregion

    #region Properties
    /// <summary>
    /// 
    /// </summary>
    public String SelectorPluginName { get; private set; }
    /// <summary>
    /// Имя поля в котором содержатся значения
    /// </summary>
    public String ValueFieldName { get; private set; }
    /// <summary>
    /// Имя поля в котором содержится текст, который будет отображаться в выпадающем списке
    /// </summary>
    public String DisplayFieldName { get; private set; }

    public Boolean isMultiselector { get; private set; }
    #endregion

  }
  #endregion
}
