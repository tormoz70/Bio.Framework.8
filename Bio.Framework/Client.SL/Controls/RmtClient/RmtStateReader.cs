namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using System.Threading;
  using Bio.Helpers.Common;

  public class RmtStateReaderEventArgs : EventArgs {
    public BioResponse Response { get; set; }
    public RmtMonitorCommand Cmd { get; set; }
  }

  public delegate void RmtStateReaderReadEventHandler(Object sender, RmtStateReaderEventArgs args);

  public enum RmtMonitorCommand { Continue = 0, Break = 1, BreakAndKill = 2 };

  /// <summary>
  /// Предоставляет возможность получить с сервера даные о состоянии отчета.
  /// </summary>
  public class RmtStateReader {

    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="owner">Ссылка на родительский контрол</param>
    public RmtStateReader(RmtClientBase owner) {
      this.Owner = owner;
    }

    /// <summary>
    /// Выполняется при чтении данных о состоянии с сервера
    /// </summary>
    public event RmtStateReaderReadEventHandler OnRead;

    /// <summary>
    /// Owner
    /// </summary>
    public RmtClientBase Owner { get; private set; }

    /// <summary>
    /// Считывает
    /// </summary>
    public void ReadState(Action<RmtStateReaderEventArgs> callback) {
      if (this.Owner.AjaxMng == null)
        throw new ArgumentNullException("AjaxMng", "Свойство должно быть задано.");

      var v_request = this.Owner.createRequest(RmtClientRequestCmd.GetState, null, true,
        (sndr, args) => {
          var response = args.Response as BioResponse;
          if (response != null && ((response.RmtStatePacket != null) || !response.Success)) {
            var eve = this.OnRead;
            if (eve != null) {
              var a = new RmtStateReaderEventArgs {
                Response = response,
                Cmd = RmtMonitorCommand.Continue
              };
              eve(this, a);
              if (callback != null)
                callback(a);
            }
          }
        });
      this.Owner.AjaxMng.Request(v_request);
    }

    private Boolean _forceStop;
    private void _doReader() {
      this.ReadState(a => {
        if ((!this._forceStop) && (a.Cmd == RmtMonitorCommand.Continue)) {
          delayedStarter.Do(2000, this._doReader);
        } else {
          this.IsRunning = false;
          this._readThread = null;
        }
      });
    }

    private Thread _readThread;
    /// <summary>
    /// Пуск
    /// </summary>
    public void Start() {
      lock(this) {
        if(!this.IsRunning) {
          this._forceStop = false;
          this.IsRunning = true;
          this._readThread = new Thread(this._doReader);
          this._readThread.Start();
        }
      }
    }

    public void ForceStop() {
      this._forceStop = true;
    }

    /// <summary>
    /// Процесс запущен
    /// </summary>
    public Boolean IsRunning { get; private set; }

  }
}
