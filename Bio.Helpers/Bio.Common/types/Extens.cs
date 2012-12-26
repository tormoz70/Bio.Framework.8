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
using System.Linq;

namespace Bio.Helpers.Common.Extentions {
  //public static class WebClientExtens {
  //  public static string DownloadString(this WebClient webClient, Uri address) {
  //    // Превратить событие в блокируемый IEnumerable
  //    var blocker = Observable.FromEvent<DownloadStringCompletedEventArgs>
  //        (webClient, "DownloadStringCompleted")
  //        .SubscribeOnDispatcher()
  //        .ToEnumerable();
  //    // Вызываем асинхронный метод
  //    webClient.DownloadStringAsync(address);
  //    // Получаем первый элемент блокируемого Enumerable
  //    var eventArgs = blocker.First().EventArgs;
  //    // Выкидываем исключение
  //    if (eventArgs.Error != null) {
  //      throw eventArgs.Error;
  //    }
  //    return eventArgs.Result;
  //  }
  //}
  //public static class ChildWindowExtens {
  //  public static void ShowMod(this ChildWindow win) {
  //    // Превратить событие в блокируемый IEnumerable
  //    var blocker = Observable.FromEvent<EventArgs>(win, "Closed").SubscribeOnDispatcher().ToEnumerable();
  //    // Вызываем асинхронный метод
  //    win.Show();
  //    // Получаем первый элемент блокируемого Enumerable
  //    var eventArgs = blocker.First();
  //    //// Выкидываем исключение
  //    //if (eventArgs.Error != null) {
  //    //  throw eventArgs.Error;
  //    //}
  //    //return eventArgs.Result;
  //  }
  //}

  public static class FrameworkElementExtensions {
    public static T GetRoot<T>(this FrameworkElement child, Func<FrameworkElement, Boolean> onScan) where T : FrameworkElement {
      var parent = child.Parent as FrameworkElement;
      if ((onScan != null) && ((onScan(child)) || (parent == null)))
          return child as T;
      else if ((onScan == null) && (parent == null))
        return child as T;
      return parent.GetRoot<T>(onScan);
    }
  }

}
