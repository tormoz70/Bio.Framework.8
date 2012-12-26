
namespace Bio.Framework.Client.SL.JSPropertyGrid {
  #region Using Directives
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Linq;
  using System.Reflection;
  using System.Windows;
  using System.Windows.Browser;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using Bio.Framework.Client.SL;
  using Bio.Helpers.Controls.SL.SLPropertyGrid;

  #endregion

  #region PropertyGrid
  /// <summary>
  /// PropertyGrid
  /// </summary>
  public partial class CJSPropertyGrid : PropertyGrid {

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CJSPropertyGrid():base() {
    }
    #endregion

    public IPlugin OwnerPlugin { get; set; }
  }
  #endregion
}
