using System;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Packets.Notifications {
  public abstract class NotificationBase : ICloneable {
    protected abstract void copyThis(ref NotificationBase destObj);

    private static NotificationBase _createInst(Type type) {
      var ci = type.GetConstructor(new Type[0]);
      var rslt = (NotificationBase)ci.Invoke(new Object[0]);
      return rslt;
    }

    public object Clone() {
      var rslt = _createInst(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }

  }
}
