namespace Bio.Helpers.Common.Types {
#if SILVERLIGHT
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading;

  public class Mutex {
    private Int32 _counter = 0;
    public Mutex() {
      
    }
    public Int32 Count {
      get { return this.Count; }
    }
    public void WaitOne() {
      while (_counter > 0) {
        Thread.Sleep(100);
      }
      _counter++;
    }
    public void ReleaseMutex() {
      _counter--;
    }
  }
#endif
}
