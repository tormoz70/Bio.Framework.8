using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Threading;

namespace Bio.Helpers.Common {
  public class delayedStarter {
    private class InternalThreadState {
      private Int32 _dueTime = 0;
      private Action _callback = null;
      private Timer _timer = null;
      
      public void start(Int32 dueTime, Action callback) {
        this._dueTime = dueTime;
        this._callback = callback;
        if (this._timer == null) {
          this._timer = new Timer((s) => {
            if (this._callback != null)
              this._callback();
          }, null, this._dueTime, Timeout.Infinite);
        } else
          this._timer.Change(this._dueTime, Timeout.Infinite);
      }
    }
    private static InternalThreadState _state = null;
    static delayedStarter() {
      if (_state == null)
        _state = new InternalThreadState();
    }
    public static void Do(Int32 dueTime, Action callback) { 
      _state.start(dueTime, callback);
    }
  }

}
