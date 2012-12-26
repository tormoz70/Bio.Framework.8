
namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  #region Using Directives
  using System;

  #endregion

  #region ExceptionEventArgs
  /// <summary>
  /// ExceptionEventArgs
  /// </summary>
  public class ExceptionEventArgs : EventArgs {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventException">The Exception</param>
    public ExceptionEventArgs(Exception eventException) {
      this.EventException = eventException;
    }

    /// <summary>
    /// Gets or sets the Exception
    /// </summary>
    public Exception EventException {
      get;
      set;
    }
  }
  #endregion
}
