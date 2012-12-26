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

namespace Bio.Framework.Client.SL {
  public class CDSCache : Dictionary<String, CachedDS> {
    public void loadData(Action<CachedDS> callback){
      var order = new LinkedList<CachedDS>(this.Values);

      order.First.Value.loadData(callback, order.First.Next);
    }
  }
}
