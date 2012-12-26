using System.Windows;
using System.Windows.Media;

namespace Bio.Helpers.Controls.SL {
  public class PopupMenuSeparator : PopupMenuItem {
    public PopupMenuSeparator()
      : base(null, null, true) {
      HorizontalSeparatorVisibility = Visibility.Visible;
      IsEnabled = false;
      CloseOnClick = false;

      Color endColor = SeparatorEndColor;
      if (endColor.A + 10 <= 255)
        endColor.A += 10;

      HorizontalSeparatorBrush = PopupMenuUtils.MakeColorGradient(SeparatorStartColor, endColor, 90);
    }
  }
}