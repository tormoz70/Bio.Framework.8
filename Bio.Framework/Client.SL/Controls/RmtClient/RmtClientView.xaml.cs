using System;
using System.Windows;
using Bio.Helpers.Common;
using Bio.Framework.Packets;

namespace Bio.Framework.Client.SL {
  public partial class RmtClientView {
    private readonly RmtClientBase _owner;

    public RmtClientView(RmtClientBase owner) {
      this._owner = owner;
      InitializeComponent();
      this.Title = this._owner.Title;
      if (!this._owner.DebugThis)
        this.btnReadState.Visibility = Visibility.Collapsed;
      this.Closing += this.RmtClientView_Closing;
    }

    /// <summary>
    /// Последняя строка добавленная в лог
    /// </summary>
    public String LastAddedLine { get; private set; }
    /// <summary>
    /// Добавляет строку в лог
    /// </summary>
    /// <param name="line"></param>
    public void AddLineToLog(String line) {
      if (String.IsNullOrEmpty(this.tbxLog.Text))
        this.tbxLog.Text = line;
      else
        this.tbxLog.Text = this.tbxLog.Text + line;
      this.LastAddedLine = line;
    }

    /// <summary>
    /// Удалает последнюю строку из лога
    /// </summary>
    private void _removeLastAddedLineFromLog() {
      var vCutLineLength = String.IsNullOrEmpty(this.LastAddedLine) ? 0 : this.LastAddedLine.Length;
      if (!String.IsNullOrEmpty(this.tbxLog.Text) && (this.tbxLog.Text.Length >= vCutLineLength))
        this.tbxLog.Text = this.tbxLog.Text.Remove(this.tbxLog.Text.Length - vCutLineLength);
    }

    public void ClearLog() {
      this.tbxLog.Text = String.Empty;
    }

    /// <summary>
    /// Изменяет последнюю строку в логе
    /// </summary>
    /// <param name="line"></param>
    public void ChangeLastLogLine(String line) {
      this._removeLastAddedLineFromLog();
      this.AddLineToLog(line);
    }

    void RmtClientView_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      if (this._owner.IsRunning) {
        e.Cancel = true;
        if (MessageBox.Show("Остановить построение отчета?", "Останов", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
          this._owner.BreakProc();
          MessageBox.Show("Построение отчета остановлено. Дождитесь завершения.", "Останов", MessageBoxButton.OK);
        }
      } else {
        if (this._owner.StateReader != null)
          this._owner.StateReader.ForceStop();
      }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

    private RemoteProcState? _prevState;
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

    public void DoOnChangeState(RemoteProcessStatePack packet) {
      this._doOnChangeState(packet);
    }

    private void btnRun_Click(object sender, RoutedEventArgs e) {
      this._owner.RestartProc();
    }

    private void btnBreak_Click(object sender, RoutedEventArgs e) {
      this._owner.BreakProc();
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e) {
      this._owner.OpenResult(() => {
        //this.btnOpen.IsEnabled = true;
      });
    }

    private void btnReadState_Click(object sender, RoutedEventArgs e) {
      this._owner.StateReader.ReadState(null);
    }

  }
}

