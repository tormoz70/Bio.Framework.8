using System.Collections.Generic;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets.Notifications {
  public class Notifications : List<NotificationBase>, ICloneable {

    public object Clone() {
      var rslt = new Notifications();
      foreach (var item in this)
        rslt.Add((NotificationBase)item.Clone());
      return rslt;
    }
  }
}
