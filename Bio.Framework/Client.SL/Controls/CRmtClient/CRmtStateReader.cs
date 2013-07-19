namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using System.Threading;
  using Bio.Helpers.Common;

  public class CRmtStateReaderEventArgs : EventArgs {
    public BioResponse response { get; set; }
    public RmtMonitorCommand cmd { get; set; }
  }

  public delegate void CRmtStateReaderReadEventHandler(Object sender, CRmtStateReaderEventArgs args);

  public enum RmtMonitorCommand { Continue = 0, Break = 1, BreakAndKill = 2 };

  /// <summary>
  /// Предоставляет возможность получить с сервера даные о состоянии отчета.
  /// </summary>
  public class CRmtStateReader {

    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="pOwner">Ссылка на родительский контрол</param>
    public CRmtStateReader(CRmtClientBase owner) {
      this.Owner = owner;
    }

    /// <summary>
    /// Выполняется при чтении данных о состоянии с сервера
    /// </summary>
    public event CRmtStateReaderReadEventHandler OnRead;

    /// <summary>
    /// Owner
    /// </summary>
    public CRmtClientBase Owner { get; private set; }

    /// <summary>
    /// Считывает
    /// </summary>
    public void readState(Action<CRmtStateReaderEventArgs> callback) {
      if (this.Owner.ajaxMng == null)
        throw new ArgumentNullException("AjaxMng", "Свойство должно быть задано.");

      RmtClientRequest v_request = this.Owner.createRequest(RmtClientRequestCmd.GetState, null, true,
        new AjaxRequestDelegate((sndr, args) => {
          BioResponse response = args.Response as BioResponse;
          if ((response.RmtStatePacket != null) || !response.Success) {
            CRmtStateReaderReadEventHandler vEve = this.OnRead;
            if (vEve != null) {
              var a = new CRmtStateReaderEventArgs {
                response = response,
                cmd = RmtMonitorCommand.Continue
              };
              vEve(this, a);
              if (callback != null)
                callback(a);
            }
          }
        }));
      this.Owner.ajaxMng.Request(v_request);
    }

    private Boolean _forceStop = false;
    private void doReader() {
      this.readState((a) => {
        if ((!this._forceStop) && (a.cmd == RmtMonitorCommand.Continue)) {
          delayedStarter.Do(2000, this.doReader);
        } else {
          this.IsRunning = false;
          this._readThread = null;
        }
      });
    }

    private Thread _readThread = null;
    /// <summary>
    /// Пуск
    /// </summary>
    public void Start() {
      lock(this) {
        if(!this.IsRunning) {
          this._forceStop = false;
          this.IsRunning = true;
          this._readThread = new Thread(this.doReader);
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

    /// <summary>
    /// Стоп
    /// </summary>
    /*public void Stop() {
      lock(this) {
        if(this.IsRunning) {
          this.IsRunning = false;
          if(!new ArrayList { ThreadState.AbortRequested, ThreadState.Aborted, ThreadState.StopRequested }.Contains(this.FReadThread.ThreadState)) {
            this.FReadThread.Abort();
            this.FReadThread = null;
          }
        }
      }
    }*/
  }
}
