using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using Bio.Helpers.Common;
using Bio.Framework.Packets;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {
  public class CPluginViewBase : UserControl {
    public IPlugin ownerPlugin { get; protected set; }
    public CPluginViewBase() {
      this.IsDialog = false;
      if(!Utl.DesignTime){
        //this.LayoutUpdated += new EventHandler(this.CPluginViewBase_LayoutUpdated);
        //this.v
        this.Loaded += new RoutedEventHandler(CPluginViewBase_Loaded);
      }
    }

    public CPluginViewBase(IPlugin owner)
      : this() {
      this.ownerPlugin = owner;
      //this.Loaded += new RoutedEventHandler(CPluginViewBase_Loaded);
    }

    /// <summary>
    /// Происходит при отображении View в контейнере.
    /// </summary>
    public event EventHandler OnShow;
    /// <summary>
    /// Происходит перед закрытием View.
    /// </summary>
    public event EventHandler<CancelEventArgs> OnClosing;
    /// <summary>
    /// Происходит при закрытии View.
    /// </summary>
    public event EventHandler<PluginViewOnCloseEventArgs> OnClosed;

    protected UIElement _container = null;
    public void Show(UIElement container) {
      if (container != null) {
        this._container = container;
        if (this._container is Panel)
          (this._container as Panel).Children.Add(this);
        else if (this._container is ContentControl) {
          (this._container as ContentControl).Content = this;
          //(this._container as ContentControl).con
        }
        this._container.UpdateLayout();
        //this.UpdateLayout();
        //this.ApplyTemplate();
      }
    }

    void ownerWindow_Closing(object sender, CancelEventArgs e) {
      e.Cancel = !this.doOnClosing();
    }

    void ownerWindow_Closed(Object sender, EventArgs e) {
      this._remove();
    }

    public virtual VSelection Selection { get; private set; }
    
    private Action<Boolean?> _callback = null;

    /// <summary>
    /// Открыто в диалоговом окне
    /// </summary>
    public Boolean IsDialog { get; private set; }
    protected CPluginViewDialog ownerWindow = null;

    public void ShowDialog(Action<Boolean?> callback) {
      this._callback = callback;
      this.IsDialog = true;
      if (this.ownerWindow == null) {
        this.ownerWindow = new CPluginViewDialog(this.ownerPlugin);
        this.ownerWindow.Closed += new EventHandler(ownerWindow_Closed);
        this.ownerWindow.Closing += new EventHandler<CancelEventArgs>(ownerWindow_Closing);
        if (this._buttonDefs != null)
          this.ownerWindow.AddButtons(this._buttonDefs);
      }
      if (!this.ownerWindow.IsVisible)
        this.ownerWindow.ShowDialog();
    }

    /// <summary>
    /// Открыть в далоговом окне как Selector
    /// </summary>
    /// <param name="selection">Выбрать запись по значению первичного ключа</param>
    /// <param name="callback">Возвращает значение DialogResult и выбранную запись</param>
    public void ShowSelector(VSelection selection, SelectorCallback callback) {
      this.Selection = selection;
      this.ShowDialog((mr) => {
        if (callback != null) {
          callback(mr, new OnSelectEventArgs { 
            selection = this.Selection 
          });
        }
      });
    }

    public Boolean? DialogResult {
      get {
        return (this.ownerWindow != null) ? this.ownerWindow.DialogResult : null;
      }
    }


    private Boolean _closing = false;
    private Boolean _shown = false;

    void CPluginViewBase_Loaded(object sender, RoutedEventArgs e) {
      if (this._container != null) {
        if (!this._shown && !this._closing) {
          this._shown = true;
          this.doOnShow();
        }
      }
    }

    private void doOnShow() {
      EventHandler handler = this.OnShow;
      if (handler != null) {
        handler(this, new EventArgs());
      }
    }

    private void doOnClose() {
      var handler = this.OnClosed;
      if (handler != null) {
        var a = new PluginViewOnCloseEventArgs(this.DialogResult);
        handler(this, a);
        if (a.dialogResult == true)
          this.Selection = a.selection;
      }

      if (this._callback != null) {
        this._callback(this.DialogResult);
      }
    }

    private Boolean doOnClosing() {
      Boolean v_cancel = false;
      if (this._shown) {
        var handler = this.OnClosing;
        if (handler != null) {
          var args = new CancelEventArgs() { Cancel = v_cancel };
          handler(this, args);
          v_cancel = args.Cancel;
        }
      }
      this._closing = !v_cancel;
      return !v_cancel;
    }

    private void _remove() {
      //try {
      //this._clearData(this.Content);
      if (this._container is Panel)
        (this._container as Panel).Children.Remove(this);
      else if (this._container is ContentControl)
        (this._container as ContentControl).Content = null;
      this._container = null;

      if (this._shown && this._closing) {
        this._shown = false;
        this._closing = false;
        this.doOnClose();
      }

    }

    public void Close() {
      if (this.IsDialog) {
        if (this.ownerWindow != null)
          this.ownerWindow.Close();
      } else {
        if (this.doOnClosing())
          this._remove();
      }
    }

    private PluginViewDialogButton[] _buttonDefs = null;
    /// <summary>
    /// Добавляет кнопки в диалоговое окно
    /// </summary>
    /// <param name="buttons"></param>
    public void AddButtons(params PluginViewDialogButton[] buttons) {
      this._buttonDefs = buttons;
    }
  }

  public class PluginViewOnCloseEventArgs : EventArgs  {
    public Boolean? dialogResult { get; private set; }
    public VSelection selection { get; set; }

    public PluginViewOnCloseEventArgs(Boolean? dialogResult)
      : base() {
      this.dialogResult = dialogResult;
    }
  }
}
