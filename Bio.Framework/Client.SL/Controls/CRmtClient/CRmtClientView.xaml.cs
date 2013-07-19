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
using Bio.Helpers.Controls.SL;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;

namespace Bio.Framework.Client.SL {
  public partial class CRmtClientView : FloatableWindow {
    private CRmtClientBase _owner = null;

    public CRmtClientView(CRmtClientBase owner) {
      this._owner = owner;
      InitializeComponent();
      this.Title = this._owner.title;
      if (!this._owner.DebugThis)
        this.btnReadState.Visibility = System.Windows.Visibility.Collapsed;
      this.Closing += new EventHandler<System.ComponentModel.CancelEventArgs>(CRmtClientView_Closing);
    }

    /// Последняя строка добавленная в лог
    /// </summary>
    public String LastAddedLine { get; private set; }
    /// <summary>
    /// Добавляет строку в лог
    /// </summary>
    /// <param name="line"></param>
    public void addLineToLog(String line) {
      if (String.IsNullOrEmpty(this.tbxLog.Text))
        this.tbxLog.Text = line;
      else
        this.tbxLog.Text = this.tbxLog.Text + line;
      this.LastAddedLine = line;
    }

    /// <summary>
    /// Удалает последнюю строку из лога
    /// </summary>
    private void removeLastAddedLineFromLog() {
      Int32 vCutLineLength = String.IsNullOrEmpty(this.LastAddedLine) ? 0 : this.LastAddedLine.Length;
      if (!String.IsNullOrEmpty(this.tbxLog.Text) && (this.tbxLog.Text.Length >= vCutLineLength))
        this.tbxLog.Text = this.tbxLog.Text.Remove(this.tbxLog.Text.Length - vCutLineLength);
    }

    public void clearLog() {
      this.tbxLog.Text = String.Empty;
    }

    /// <summary>
    /// Изменяет последнюю строку в логе
    /// </summary>
    /// <param name="line"></param>
    public void changeLastLogLine(String line) {
      this.removeLastAddedLineFromLog();
      this.addLineToLog(line);
    }

    void CRmtClientView_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      if (this._owner.IsRunning) {
        //this.IsVisible = false;
        e.Cancel = true;
        if (MessageBox.Show("Остановить построение отчета?", "Останов", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
          this._owner.BreakProc();
          MessageBox.Show("Построение отчета остановлено. Дождитесь завершения.", "Останов", MessageBoxButton.OK);
        }
        //this._owner.breakProc((s, a) => {
        //  if (a.response.success) {
        //    //this._owner.stateReader.ForceStop();
        //  } else {
        //    this.addLineToLog(msgBx.formatError(a.response.ex));
        //    e.Cancel = true;
        //  }
        //});
      } else {
        if (this._owner.StateReader != null)
          this._owner.StateReader.ForceStop();
      }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e) {
      //if (this._owner.IsRunning) {
      //  this._owner.breakProc((s, a) => {
      //    if (a.response.success) {
      //      this._owner.stateReader.ForceStop();
      //      this.DialogResult = true;
      //    } else
      //      this.addLineToLog(msgBx.formatError(a.response.ex));
      //  });
      //} else {
      //  if (this._owner.stateReader != null)
      //    this._owner.stateReader.ForceStop();
      //  this.DialogResult = true;
      //}
      this.DialogResult = true;
    }

    private RemoteProcState? _prevState = null;
    private void _doOnChangeState(RemoteProcessStatePack packet) {
      if ((this._prevState == null) || (this._prevState != packet.State)) {
        this._prevState = packet.State;
        if (rmtUtl.IsRunning(packet.State)) {
          this.btnRun.IsEnabled = false;
          this.btnBreak.IsEnabled = true;
          this.btnClose.IsEnabled = false;
          this.btnOpen.IsEnabled = false;
        } else {
          this.btnRun.IsEnabled = true;
          this.btnBreak.IsEnabled = false;
          this.btnClose.IsEnabled = rmtUtl.IsFinished(packet.State);
          this.btnOpen.IsEnabled = (packet.State == RemoteProcState.Done) && packet.HasResultFile;
        }
      }
    }

    public void doOnChangeState(RemoteProcessStatePack packet) {
      this._doOnChangeState(packet);
    }

    //private void _doOnRsltFileLoaded() {
    //  this.btnOpen.IsEnabled = true;
    //}

    //public void doOnRsltFileLoaded() {
    //  this._doOnRsltFileLoaded();
    //}

    private void btnRun_Click(object sender, RoutedEventArgs e) {
      this._owner.RestartProc();
    }

    private void btnBreak_Click(object sender, RoutedEventArgs e) {
      this._owner.BreakProc();
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e) {
      //this.btnOpen.IsEnabled = false;
      this._owner.OpenResult(() => {
        //this.btnOpen.IsEnabled = true;
      });
    }

    private void btnReadState_Click(object sender, RoutedEventArgs e) {
      this._owner.StateReader.readState(null);
    }

  }
}

