using System.Windows.Controls;
using System;
using System.Windows;
using System.Windows.Media;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class PropertyGridLabel : ContentControl {
    // Methods
    public PropertyGridLabel() {
      base.DefaultStyleKey = typeof(PropertyGridLabel);
      //this.IsTabStop = false;
    }

    private Brush _brush = null;
    public void SetForeground(Brush brush) {
      var cnt = this.Content as TextBlock;
      if (cnt != null) {
        if (this._brush == null)
          this._brush = cnt.Foreground;
        cnt.Foreground = brush;
      }
    }

    public void ResetForeground() {
      var cnt = this.Content as TextBlock;
      if (cnt != null) {
        if (this._brush != null)
          cnt.Foreground = this._brush;
      }
    }

    public void SetRequiredStyle() {
      var cnt = this.Content as TextBlock;
      if (cnt != null) 
        cnt.FontWeight = FontWeights.Bold;
    }
    public void ResetRequiredStyle() {
      var cnt = this.Content as TextBlock;
      if (cnt != null) 
        cnt.FontWeight = FontWeights.Normal;
    }
  }
}
