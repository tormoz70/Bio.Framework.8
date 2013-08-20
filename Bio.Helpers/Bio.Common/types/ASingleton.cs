using System;

namespace Bio.Helpers.Common {
  /// <summary/>
  public class ASingleton<T> where T : class, new() {

    private static volatile T _instance;
// ReSharper disable StaticFieldInGenericType
    private static readonly object _syncRoot = new Object();
// ReSharper restore StaticFieldInGenericType

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
