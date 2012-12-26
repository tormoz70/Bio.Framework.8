namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Framework.Packets;
  using System.ComponentModel;
  using Bio.Helpers.Common.Types;
  using System.IO;
  using Bio.Helpers.Common;
  using System.Windows;
  using Bio.Helpers.Controls.SL;
  using Bio.Helpers.Ajax;
  using System.Linq;

  public delegate void CRmtClientReadStateEventHandler(Object sender, CBioResponse response);
  public delegate void CRmtClientFinishedEventHandler(Object sender, CBioResponse response);

  /// <summary>
  /// Предоставляет возможность запускать на сервере долгоиграющие отчеты.
  /// </summary>
  public class CRmtClientBase {

    internal RequestType _requestType;
    private CRmtStateReader _stateReader = null;

    /// <summary>
    /// Если True, то отчет еще выполняется
    /// </summary>
    public Boolean IsRunning { get; private set; }

    /// <summary>
    /// Конструктор класса.
    /// </summary>
    /// <param name="ajaxMng">ссылка на AjaxMng</param>
    /// <param name="bioCode">Код</param>
    /// <param name="title">Заголовок</param>
    public CRmtClientBase(IAjaxMng ajaxMng, String bioCode, String title) {
      //this.Parent = pParen;
      this.ajaxMng = ajaxMng;
      this.bioCode = bioCode;
      this.title = title;
      this._stateReader = new CRmtStateReader(this);
      this._stateReader.OnRead += this.doOnReadState;
    }

    /// <summary>
    /// Код 
    /// </summary>
    public String bioCode { get; set; }
    /// <summary>
    /// Заголовок 
    /// </summary>
    public String title { get; set; }
    /// <summary>
    /// Имя и путь к файлу результата
    /// </summary>
    public String resultFileName { get; set; }

    /// <summary>
    /// Ссылка на менеджер асинхронных запросов.
    /// </summary>
    public IAjaxMng ajaxMng { get; set; }

    private RemoteProcState _lastState = RemoteProcState.Redy;
    private Boolean _isDone = false;
    /// <summary>
    /// Запуск монитора
    /// </summary>
    private void _startMonitor() {
      if(!this._stateReader.IsRunning && !this._isDone) {
        this._stateReader.Start();
      }
    }

    private CRmtClientView _form = null;
    protected virtual void _doBeforeRun() {
      if (this._form == null) {
        this._form = new CRmtClientView(this);
      }
      this._isDone = false;
      this._lastPipedLine = null;
      this._form.Title = this.title;
      this._form.clearLog();
      if (!this._form.IsVisible)
        this._form.ShowDialog();
      //this._form.BringToFront();
      //this._form.doOnChangeState(RemoteProcState.Starting);
      this._form.doOnChangeState(new CRemoteProcessStatePack { state = RemoteProcState.Starting });
      this._form.addLineToLog(enumHelper.GetFieldDesc(RemoteProcState.Starting) + "\n");
    }

    public event CRmtClientFinishedEventHandler OnFinished;
    protected virtual void _doOnFinished(CBioResponse response) {
      var v_eve = this.OnFinished;
      if (v_eve != null)
        v_eve(this, response);
    }

    private String _lastPipedLine = null;
    private void _appentLastPipedLine(CRemoteProcessStatePack losp, ref String v_state) {
      if ((losp.lastPipedLines != null) && (losp.lastPipedLines.Length > 0))
        this._lastPipedLine = losp.lastPipedLines.LastOrDefault();
      v_state = v_state + (!String.IsNullOrEmpty(this._lastPipedLine) ? "\n" + this._lastPipedLine : null);
    }
    private String _resultFileReferece = null;
    private void _doOnReadState(Object sender, CBioResponse response) {
      Boolean v_is_running = false;
      try {
        var v_losp = response.rmtStatePacket;
        this._lastState = v_losp.state;
        if (v_losp != null) {
          v_is_running = v_losp.isRunning();
          if (this._form != null) {
            if (!this._isDone) {
              this._isDone = v_losp.isFinished();
              String v_curStateDesc = v_losp.stateDesc();
              String v_duration = Utl.FormatDuration(v_losp.duration);
              String v_state = v_curStateDesc;
              if ((v_losp.state == RemoteProcState.Running) || this._isDone)
                v_state = String.Format("{0} - [{1}]", v_curStateDesc, v_duration);
              this._appentLastPipedLine(v_losp, ref v_state);
              this._form.changeLastLogLine(v_state + "\n");
              this._form.doOnChangeState(v_losp);
              if (v_losp.state == RemoteProcState.Done) {
                this._form.addLineToLog("Выполнение успешно завершено.\n");
                if (v_losp.hasResultFile) {
                  this._form.addLineToLog("Для просмотра результата нажмите кнопку \"Открыть результат\".");

                  String prms = ajaxUTL.prepareRequestParams(new CRmtClientRequest {
                    requestType = this._requestType,
                    bioCode = this.bioCode,
                    cmd = RmtClientRequestCmd.GetResult
                  });
                  this._resultFileReferece = this.ajaxMng.Env.ServerUrl + "?" + prms;

                }
              } else if (v_losp.state == RemoteProcState.Breaked) {
                this._form.addLineToLog("Прервано пользователем.\n");
              } else if (v_losp.state == RemoteProcState.Error) {
                this._form.addLineToLog(msgBx.formatError(v_losp.ex));
              }
              if (this._isDone)
                this._doOnFinished(response);
            }
          }
          CRmtClientReadStateEventHandler vEveRead = this.OnReadState;
          if (vEveRead != null)
            vEveRead(sender, response);
        } else {

          if (response.success)
            throw new EBioException("Сервер не вернул ожидаемый ответ!");
          else {
            if (response.ex != null)
              throw response.ex;
            else
              throw new EBioException("Неизвестная ошибка на сервере!");
          }
        }
      } catch (Exception ex) {
        this._gotoErrorState("Ошибка при обработке ответа на запуск", ex);
      } finally {
        this.IsRunning = v_is_running;
      }
    }

    public event CRmtClientReadStateEventHandler OnReadState;
    protected virtual void doOnReadState(Object sender, CRmtStateReaderEventArgs args) {
      this._doOnReadState(sender, args.response);
      if (!this.IsRunning) {
        args.cmd = RmtMonitorCommand.Break;
      } else
        args.cmd = RmtMonitorCommand.Continue;
    }

    private void _gotoErrorState(String msg, Exception ex) {
      if (this._form != null) {
        //this._form.doOnChangeState(RemoteProcState.Error);
        this._form.doOnChangeState(new CRemoteProcessStatePack { state = RemoteProcState.Error });
        this._form.changeLastLogLine(msg + ":\n");
        this._form.addLineToLog(msgBx.formatError(EBioException.CreateIfNotEBio(ex)));
      }
    }

    public CRmtStateReader stateReader {
      get {
        return this._stateReader;
      }
    }


    protected T creRequestOfClient<T>(RmtClientRequestCmd cmd, CParams bioParams, Boolean silent, AjaxRequestDelegate callback) where T : CRmtClientRequest, new() {
      var rqst = new T {
        requestType = this._requestType,
        title = this.title,
        bioCode = this.bioCode,
        bioParams = bioParams,
        cmd = cmd,
        silent = silent,
        callback = callback
      };
      return rqst;
    }

    internal virtual CRmtClientRequest createRequest(RmtClientRequestCmd cmd, CParams bioParams, Boolean silent, AjaxRequestDelegate callback) {
      return this.creRequestOfClient<CRmtClientRequest>(cmd, bioParams, silent, callback);
    }

    protected virtual void doOnRmtProcRunnedSuccess(CBioResponse response) { 
    }
    protected virtual void doOnRmtProcBeforeRun(CRmtClientRequest request) {
    }

    public Boolean DebugThis { get { return false; } }
    /// <summary>
    /// Посылает команду на сервер
    /// </summary>
    internal void _sendCommand(RmtClientRequestCmd cmd, CParams bioParams, AjaxRequestDelegate callback) {
      try {
        if (this.ajaxMng == null)
          throw new ArgumentNullException("AjaxMng", "Свойство должно быть задано.");
        
        if (rmtUtl.isRunning(this._lastState)) {
          if (cmd != RmtClientRequestCmd.Break) {
            if ((this._form != null) && (!this._form.IsVisible))
              this._form.ShowDialog();
            return;
          }
        }
        //this._form.addLineToLog(Utl.GetEnumFieldDesc<RemoteProcState>(cmd) + "\n");
        var rqst = this.createRequest(cmd, bioParams, true,
          new AjaxRequestDelegate((sndr, args) => {
            var request = args.request as CRmtClientRequest;
            var response = args.response as CBioResponse;

            if (response != null) {
              if (request.cmd == RmtClientRequestCmd.Run) {
                if (response.success) {
                  if (this._form != null) {
                    this._form.changeLastLogLine("Запуск выполнен успешно.\n");
                    this._form.addLineToLog("Состояние: ");
                    this._form.addLineToLog("Запрос...");
                    this._doOnReadState(sndr, response);
                  }
                  this.doOnRmtProcRunnedSuccess(response);
                  if (!this.DebugThis)
                    delayedStarter.Do(1000, this._startMonitor);
                } else {
                  this._gotoErrorState("При запуске произошла ошибка", response.ex);
                }
              }
            } else {
              this._gotoErrorState("На сервере произошла непредвиденная ошибка", args.response.ex);
            }


            if (callback != null)
              callback(sndr, args);
          })
      );

        //this._form.changeLastLogLine("Запрос...");
        if (cmd == RmtClientRequestCmd.Run)
          this.doOnRmtProcBeforeRun(rqst);
        this.ajaxMng.Request(rqst);
      } catch (Exception ex) {
        this._gotoErrorState("Неизвестная ошибка при отправке команды", ex);

      }
    }


    private CParams _lastBioParams = null;
    /// <summary>
    /// Запуск
    /// </summary>
    /// <param name="bioParams">Параметры запуска</param>
    /// <param name="callback">Вызывается по выполнении запуска</param>
    public void runProc(CParams bioParams, AjaxRequestDelegate callback) {
      if (!this.IsRunning) {
        this._lastBioParams = bioParams;
        this._doBeforeRun();
        this._sendCommand(RmtClientRequestCmd.Run, bioParams, callback);
      }
    }

    /// <summary>
    /// Запуск
    /// </summary>
    /// <param name="callback">Вызывается по выполнении запуска</param>
    public void runProc(AjaxRequestDelegate callback) {
      this.runProc(null, callback);
    }

    /// <summary>
    /// Запуск
    /// </summary>
    public void runProc() {
      this.runProc(null, null);
    }

    /// <summary>
    /// Перезапуск
    /// </summary>
    /// <param name="callback">Вызывается по выполнении перезапуска</param>
    public void restartProc(AjaxRequestDelegate callback) {
      this.runProc(this._lastBioParams, callback);
    }

    /// <summary>
    /// Перезапуск
    /// </summary>
    public void restartProc() {
      this.restartProc(null);
    }

    /// <summary>
    /// Останов
    /// </summary>
    /// <param name="callback">Вызывается по выполнении останова</param>
    public void breakProc(AjaxRequestDelegate callback) {
      this._sendCommand(RmtClientRequestCmd.Break, null, callback);
    }

    /// <summary>
    /// Останов
    /// </summary>
    public void breakProc() {
      this.breakProc(null);
    }

    /// <summary>
    /// Загрузить результат выполнения
    /// </summary>
    public void loadResult() {
      CParams vPrms = new CParams();
      //vPrms.Add("cmd", Utl.NameOfEnumValue((Int32)CXLRptRequestCmdType.GetResult, typeof(CXLRptRequestCmdType), false));
      //CParams vPrms = new CParams(new CParam("getResult", "true"));
      String vUrl = null; // vPrms.bldUrlParams(this._lastRequestedURL);
      //CFileDownloader.loadFileAsync(this.RptFileName, vUrl, this.loadFileAsyncCallback, null);
      //try {
      //  CFileDownloader.loadFile(this.RptFileName, vUrl);
      //  this.doOnReportLoadSuccess(this);
      //} catch(Exception ex) {
      //  this.doOnReportLoadError(this, EBioException.CreateIfNotEBio(ex));
      //} finally {
      //  this.doOnReportLoad(this);
      //}
    }

    private void _loadFileAsyncCallback(Object sndr, AsyncCompletedEventArgs args) {
      //if(File.Exists(this.ResultFileName))
      //  this.doOnResultLoadSuccess(this);
      //else
      //  this.doOnResultLoadError(this, new EBioException("Неизвестная ошибка загрузки файла."));
      //this.doOnResultLoad(this);
    }

    public void openResult(Action callback) {
      browserUtl.loadFile(this._resultFileReferece, callback);
      //if(File.Exists(this.resultFileName)) {
        //Process proc = new Process();
        //proc.StartInfo.FileName = this.RptFileName;
        //proc.StartInfo.UseShellExecute = true;
        //proc.Start();
      //}
    }

  }
}
