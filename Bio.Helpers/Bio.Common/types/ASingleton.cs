using System;

namespace Bio.Helpers.Common {
  /// <summary/>
  public class ASingleton<T> where T : class, new() {

    private static volatile T _instance;
    private static object _syncRoot = new Object();

    protected ASingleton() {}

    /// <summary/>
    public static T Instance {
      get {
        if (_instance == null) {
          lock (_syncRoot) {
            if (_instance == null)
              _instance = new T();
          }
        }

        return _instance;
      }
    }
  }
}
