using System;
using System.Collections.Generic;

namespace Bio.Framework.Client.SL.Controls {
  public class CDSCache : Dictionary<String, CachedDS> {
    public void loadData(Action<CachedDS> callback){
      var order = new LinkedList<CachedDS>(this.Values);

      order.First.Value.loadData(callback, order.First.Next);
    }
  }
}
