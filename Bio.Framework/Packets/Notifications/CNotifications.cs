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
using System.Collections.Generic;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets.Notifications {
  public class CNotifications : List<CNotificationBase>, ICloneable {

    public object Clone() {
      var rslt = new CNotifications();
      foreach (var item in this)
        rslt.Add((CNotificationBase)item.Clone());
      return rslt;
    }
  }
}
