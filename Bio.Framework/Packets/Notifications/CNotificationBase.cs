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
using Bio.Helpers.Common.Types;
using System.Reflection;

namespace Bio.Framework.Packets.Notifications {
  public abstract class CNotificationBase : ICloneable {
    protected abstract void copyThis(ref CNotificationBase destObj);

    private static CNotificationBase CreateInst(Type type) {
      var ci = type.GetConstructor(new Type[0]);
      var rslt = (CNotificationBase)ci.Invoke(new Object[0]);
      return rslt;
    }

    public object Clone() {
      var rslt = CreateInst(this.GetType());
      this.copyThis(ref rslt);
      return rslt;
    }

  }
}
