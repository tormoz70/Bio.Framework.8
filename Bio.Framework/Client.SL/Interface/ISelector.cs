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
using Bio.Framework.Packets;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {

  public class OnSelectEventArgs : EventArgs {
    public EBioException ex { get; set; }
    //public String selectedDisplay { get; set; }
    //public Object selectedValue { get; set; }
    //public CRTObject row { get; set; }
    public VSelection selection { get; set; }
  }
  
  /// <summary>
  /// SelectorCallback
  /// </summary>
  /// <param name="modalResult">true-выбор сделан</param>
  /// <param name="args"></param>
  public delegate void SelectorCallback (Boolean? modalResult, OnSelectEventArgs args);

  /// <summary>
  /// Этот итерфейс реализуется модулем, который реализует функцию выбора некоторого значения
  /// </summary>
  public interface ISelector {

    /// <summary>
    /// Запускает процедуру выбора
    /// </summary>
    /// <param name="selection">Значение[я], которое выбрано при запуске</param>
    /// <param name="callback">callback</param>
    void Select(VSelection selection, SelectorCallback callback);

    /// <summary>
    /// Запрос представления значения
    /// </summary>
    /// <param name="selection">Значение выбранное</param>
    /// <param name="callback">callback</param>
    void GetSelection(VSingleSelection selection, SelectorCallback callback);

  }
}
