
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System.Windows.Controls;
  using Converters;
  using Bio.Helpers.Common;

  #endregion

  #region EnumValueEditor
  /// <summary>
  /// An editor for a Boolean Type
  /// </summary>
  public class EnumValueEditor : ComboBoxEditorBase {
    public EnumValueEditor(PropertyGridLabel label, PropertyItem property)
      : base(label, property) {
    }
    protected override void initialize() {
      this.LoadItems(enumHelper.GetValuesWrapped(Property.PropertyType));
      base.initialize();
    }
  }
  #endregion
}
