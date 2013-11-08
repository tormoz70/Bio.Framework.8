using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;

namespace Bio.Framework.Client.SL {
  public class PluginViewBase : UserControl, IPluginView {
    public IPlugin ownerPlugin { get; protected set; }
    public PluginViewBase() {
      this.IsDialog = false;
      if(!Utl.DesignTime){
        this.Loaded += CPluginViewBase_Loaded;
      }
    }

    public PluginViewBase(IPlugin owner)
      : this() {
      this.ownerPlugin = owner;
    }

    /// <summary>
    /// Происходит при отображении View в контейнере, где sender - ссылка на View.
    /// </summary>
    public event EventHandler OnShow;
    /// <summary>
    /// Происходит при отображении диалогового окна, в котором собирается отобразится данный View. Соответственно sender - указатель на диалоговое окно. Можно использовать для установки размеров окна.
    /// </summary>
    public event EventHandler OnShowDialog;
    /// <summary>
    /// Происходит перед закрытием View.
    /// </summary>
    public event EventHandler<CancelEventArgs> OnClosing;
    /// <summary>
    /// Происходит при закрытии View.
    /// </summary>
    public event EventHandler<PluginViewOnCloseEventArgs> OnClosed;

    private UIElement _container;
    public void Show(UIElement container) {
      this._shown = (this.Visibility == Visibility.Visible) && (this._container != null) && (this._container == container);
      if (container != null) {
        this._container = container;
        if (this._container is Panel)
          (this._container as Panel).Children.Add(this);
        else if ((this._container is ContentControl) && ((this._container as ContentControl).Content != this)) {
          (this._container as ContentControl).Content = this;
        }
        if (this.Visibility == Visibility.Collapsed)
          this.Visibility = Visibility.Visible;
        this._container.UpdateLayout();
        this.UpdateLayout();
      }
    }

    void ownerWindow_Closing(object sender, CancelEventArgs e) {
      e.Cancel = !this._doOnClosing();
    }

    void ownerWindow_Closed(Object sender, EventArgs e) {
      this._remove();
    }

    public virtual VSelection Selection { get; private set; }
    
    private Action<Boolean?> _callback;

    /// <summary>
    /// Открыто в диалоговом окне
    /// </summary>
    public Boolean IsDialog { get; private set; }
    protected PluginViewDialog ownerWindow = null;

    public void ShowDialog(Action<Boolean?> callback) {
      this._callback = callback;
      this.IsDialog = true;
      if (this.ownerWindow == null) {
        this.ownerWindow = new PluginViewDialog(this);
        this.ownerWindow.Closed += ownerWindow_Closed;
        this.ownerWindow.Closing += ownerWindow_Closing;
        this.ownerWindow.OnShow += ownerWindow_OnShow;
        if (this._buttonDefs != null)
          this.ownerWindow.AddButtons(this._buttonDefs);
      }
      if (!this.ownerWindow.IsVisible)
        this.ownerWindow.ShowDialog();
    }

    void ownerWindow_OnShow(object sender, EventArgs e) {
      var onShowDialog = this.OnShowDialog;
      if (onShowDialog != null)
        onShowDialog(sender, e);
    }

    /// <summary>
    /// Открыть в далоговом окне как Selector
    /// </summary>
    /// <param name="selection">Выбрать запись по значению первичного ключа</param>
    /// <param name="callback">Возвращает значение DialogResult и выбранную запись</param>
    public void ShowSelector(VSelection selection, SelectorCallback callback) {
      this.Selection = selection;
      this.ShowDialog(mr => {
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
      set {
        if (this.ownerWindow != null)
          this.ownerWindow.DialogResult = value;
      }
    }


    private Boolean _closing;
    private Boolean _shown;

    void CPluginViewBase_Loaded(object sender, RoutedEventArgs e) {
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
    }

    protected override Size ArrangeOverride(Size finalSize) {
      if (this._container != null) {
        if (!this._shown && !this._closing) {
          this._shown = true;
          this._doOnShow();
        }
      }
      return base.ArrangeOverride(finalSize);
    }

    private void _doOnShow() {
      var handler = this.OnShow;
      if (handler != null) {
        handler(this, new EventArgs());
      }
    }

    private void _doOnClose() {
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

    private Boolean _doOnClosing() {
      var cancel = false;
      if (this._shown) {
        var handler = this.OnClosing;
        if (handler != null) {
          var args = new CancelEventArgs { Cancel = cancel };
          handler(this, args);
          cancel = args.Cancel;
        }
      }
      this._closing = !cancel;
      return !cancel;
    }

    private void _remove() {
      if (this._container is Panel)
        (this._container as Panel).Children.Remove(this);
      else if (this._container is ContentControl)
        (this._container as ContentControl).Content = null;
      this._container = null;

      if (this._shown && this._closing) {
        this._doOnClose();
      }
      this._shown = false;
      this._closing = false;
    }

    public void Close() {
      if (this.IsDialog) {
        if (this.ownerWindow != null)
          this.ownerWindow.Close();
      } else {
        if (this._doOnClosing())
          this._remove();
      }
    }

    private PluginViewDialogButton[] _buttonDefs;
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

    public PluginViewOnCloseEventArgs(Boolean? dialogResult) {
      this.dialogResult = dialogResult;
    }
  }
}
