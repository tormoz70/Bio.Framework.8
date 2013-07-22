using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Bio.Framework.Packets.Notifications {
  public class NotificationCachedDSChanged : NotificationBase {

    public String DSName { get; set; }

    protected override void copyThis(ref NotificationBase destObj) {
      //base.copyThis(ref destObj);
      var dst = destObj as NotificationCachedDSChanged;
      if (dst != null) {
        dst.DSName = this.DSName;
      }
    }
  }
}
