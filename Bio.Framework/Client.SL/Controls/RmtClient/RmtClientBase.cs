namespace Bio.Framework.Client.SL {
  using System;
  using Packets;
  using System.ComponentModel;
  using Helpers.Common.Types;
  using Helpers.Common;
  using Helpers.Controls.SL;
  using System.Linq;

  public delegate void CRmtClientReadStateEventHandler(Object sender, BioResponse response);
  public delegate void CRmtClientFinishedEventHandler(Object sender, BioResponse response);

  /// <summary>
  /// Предоставляет возможность запускать на сервере долгоиграющие отчеты.
  /// </summary>
  public class RmtClientBase {

    internal RequestType requestType;
    private readonly RmtStateReader _stateReader;

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
    public RmtClientBase(IAjaxMng ajaxMng, String bioCode, String title) {
      //this.Parent = pParen;
      this.AjaxMng = ajaxMng;
      this.BioCode = bioCode;
      this.Title = title;
      this._stateReader = new RmtStateReader(this);
      this._stateReader.OnRead += this.doOnReadState;
    }

    /// <summary>
    /// Код 
    /// </summary>
    public String BioCode { get; set; }
    /// <summary>
    /// Заголовок 
    /// </summary>
    public String Title { get; set; }
    /// <summary>
    /// Имя и путь к файлу результата
    /// </summary>
    public String ResultFileName { get; set; }

    /// <summary>
    /// Ссылка на менеджер асинхронных запросов.
    /// </summary>
    public IAjaxMng AjaxMng { get; set; }

    private RemoteProcState _lastState = RemoteProcState.Redy;
    private Boolean _isDone;
    /// <summary>
    /// Запуск монитора
    /// </summary>
    private void _startMonitor() {
      if(!this._stateReader.IsRunning && !this._isDone) {
        this._stateReader.Start();
      }
    }

    private RmtClientView _form;
    protected virtual void _doBeforeRun() {
      if (this._form == null) {
        this._form = new RmtClientView(this);
      }
      this._isDone = false;
      this._lastPipedLine = null;
      this._form.Title = this.Title;
      this._form.ClearLog();
      if (!this._form.IsVisible)
        this._form.ShowDialog();
      this._form.DoOnChangeState(new RemoteProcessStatePack { State = RemoteProcState.Starting });
      this._form.AddLineToLog(enumHelper.GetFieldDesc(RemoteProcState.Starting) + "\n");
    }

    public event CRmtClientFinishedEventHandler OnFinished;
    protected virtual void _doOnFinished(BioResponse response) {
      var v_eve = this.OnFinished;
      if (v_eve != null)
        v_eve(this, response);
    }

    private String _lastPipedLine;
    private void _appentLastPipedLine(RemoteProcessStatePack losp, ref String state) {
      if ((losp.lastPipedLines != null) && (losp.lastPipedLines.Length > 0))
        this._lastPipedLine = losp.lastPipedLines.LastOrDefault();
      state = state + (!String.IsNullOrEmpty(this._lastPipedLine) ? "\n" + this._lastPipedLine : null);
    }
    private String _resultFileReferece = null;
    private void _doOnReadState(Object sender, BioResponse response) {
      Boolean v_is_running = false;
      try {
        var losp = response.RmtStatePacket;
        this._lastState = losp.State;
        if (losp != null) {
          v_is_running = losp.IsRunning();
          if (this._form != null) {
            if (!this._isDone) {
              this._isDone = losp.IsFinished();
              var curStateDesc = losp.StateDesc();
              var duration = Utl.FormatDuration(losp.Duration);
              var state = curStateDesc;
              if ((losp.State == RemoteProcState.Running) || this._isDone)
                state = String.Format("{0} - [{1}]", curStateDesc, duration);
              this._appentLastPipedLine(losp, ref state);
              this._form.ChangeLastLogLine(state + "\n");
              this._form.DoOnChangeState(losp);
              if (losp.State == RemoteProcState.Done) {
                this._form.AddLineToLog("Выполнение успешно завершено.\n");
                if (losp.HasResultFile) {
                  this._form.AddLineToLog("Для просмотра результата нажмите кнопку \"Открыть результат\".");

                  String prms = ajaxUTL.PrepareRequestParams(new RmtClientRequest {
                    RequestType = this.requestType,
                    BioCode = this.BioCode,
                    cmd = RmtClientRequestCmd.GetResult
                  });
                  this._resultFileReferece = this.AjaxMng.Env.ServerUrl + "?" + prms;

                }
              } else if (losp.State == RemoteProcState.Breaked) {
                this._form.AddLineToLog("Прервано пользователем.\n");
              } else if (losp.State == RemoteProcState.Error) {
                this._form.AddLineToLog(msgBx.formatError(losp.Ex));
              }
              if (this._isDone)
                this._doOnFinished(response);
            }
          }
          CRmtClientReadStateEventHandler vEveRead = this.OnReadState;
          if (vEveRead != null)
            vEveRead(sender, response);
        } else {
          if (response.Success)
            throw new EBioException("Сервер не вернул ожидаемый ответ!");
          if (response.Ex != null)
            throw response.Ex;
          throw new EBioException("Неизвестная ошибка на сервере!");
        }
      } catch (Exception ex) {
        this._gotoErrorState("Ошибка при обработке ответа на запуск", ex);
      } finally {
        this.IsRunning = v_is_running;
      }
    }

    public event CRmtClientReadStateEventHandler OnReadState;
    protected virtual void doOnReadState(Object sender, RmtStateReaderEventArgs args) {
      this._doOnReadState(sender, args.Response);
      args.Cmd = !this.IsRunning ? RmtMonitorCommand.Break : RmtMonitorCommand.Continue;
    }

    private void _gotoErrorState(String msg, Exception ex) {
      if (this._form != null) {
        this._form.DoOnChangeState(new RemoteProcessStatePack { State = RemoteProcState.Error });
        this._form.ChangeLastLogLine(msg + ":\n");
        this._form.AddLineToLog(msgBx.formatError(EBioException.CreateIfNotEBio(ex)));
      }
    }

    public RmtStateReader StateReader {
      get {
        return this._stateReader;
      }
    }


    protected T creRequestOfClient<T>(RmtClientRequestCmd cmd, Params bioParams, Boolean silent, AjaxRequestDelegate callback) where T : RmtClientRequest, new() {
      var rqst = new T {
        RequestType = this.requestType,
        title = this.Title,
        BioCode = this.BioCode,
        BioParams = bioParams,
        cmd = cmd,
        Silent = silent,
        Callback = callback
      };
      return rqst;
    }

    internal virtual RmtClientRequest createRequest(RmtClientRequestCmd cmd, Params bioParams, Boolean silent, AjaxRequestDelegate callback) {
      return this.creRequestOfClient<RmtClientRequest>(cmd, bioParams, silent, callback);
    }

    protected virtual void doOnRmtProcRunnedSuccess(BioResponse response) { 
    }
    protected virtual void doOnRmtProcBeforeRun(RmtClientRequest request) {
    }

    public Boolean DebugThis { get { return false; } }
    /// <summary>
    /// Посылает команду на сервер
    /// </summary>
    internal void _sendCommand(RmtClientRequestCmd cmd, Params bioParams, AjaxRequestDelegate callback) {
      try {
        if (this.AjaxMng == null)
          throw new ArgumentNullException("AjaxMng", "Свойство должно быть задано.");
        
        if (rmtUtl.IsRunning(this._lastState)) {
          if (cmd != RmtClientRequestCmd.Break) {
            if ((this._form != null) && (!this._form.IsVisible))
              this._form.ShowDialog();
            return;
          }
        }
        var rqst = this.createRequest(cmd, bioParams, true,
          (sndr, args) => {
            var request = args.Request as RmtClientRequest;
            var response = args.Response as BioResponse;

            if (response != null) {
              if (request.cmd == RmtClientRequestCmd.Run) {
                if (response.Success) {
                  if (this._form != null) {
                    this._form.ChangeLastLogLine("Запуск выполнен успешно.\n");
                    this._form.AddLineToLog("Состояние: ");
                    this._form.AddLineToLog("Запрос...");
                    this._doOnReadState(sndr, response);
                  }
                  this.doOnRmtProcRunnedSuccess(response);
                  if (!this.DebugThis)
                    delayedStarter.Do(1000, this._startMonitor);
                } else {
                  this._gotoErrorState("При запуске произошла ошибка", response.Ex);
                }
              }
            } else {
              this._gotoErrorState("На сервере произошла непредвиденная ошибка", args.Response.Ex);
            }


            if (callback != null)
              callback(sndr, args);
          }
      );

        //this._form.changeLastLogLine("Запрос...");
        if (cmd == RmtClientRequestCmd.Run)
          this.doOnRmtProcBeforeRun(rqst);
        this.AjaxMng.Request(rqst);
      } catch (Exception ex) {
        this._gotoErrorState("Неизвестная ошибка при отправке команды", ex);

      }
    }


    private Params _lastBioParams;
    /// <summary>
    /// Запуск
    /// </summary>
    /// <param name="bioParams">Параметры запуска</param>
    /// <param name="callback">Вызывается по выполнении запуска</param>
    public void RunProc(Params bioParams, AjaxRequestDelegate callback) {
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
    public void RunProc(AjaxRequestDelegate callback) {
      this.RunProc(null, callback);
    }

    /// <summary>
    /// Запуск
    /// </summary>
    public void RunProc() {
      this.RunProc(null, null);
    }

    /// <summary>
    /// Перезапуск
    /// </summary>
    /// <param name="callback">Вызывается по выполнении перезапуска</param>
    public void RestartProc(AjaxRequestDelegate callback) {
      this.RunProc(this._lastBioParams, callback);
    }

    /// <summary>
    /// Перезапуск
    /// </summary>
    public void RestartProc() {
      this.RestartProc(null);
    }

    /// <summary>
    /// Останов
    /// </summary>
    /// <param name="callback">Вызывается по выполнении останова</param>
    public void BreakProc(AjaxRequestDelegate callback) {
      this._sendCommand(RmtClientRequestCmd.Break, null, callback);
    }

    /// <summary>
    /// Останов
    /// </summary>
    public void BreakProc() {
      this.BreakProc(null);
    }

    /// <summary>
    /// Загрузить результат выполнения
    /// </summary>
    public void LoadResult() {
      //Params vPrms = new Params();
      //vPrms.Add("cmd", Utl.NameOfEnumValue((Int32)CXLRptRequestCmdType.GetResult, typeof(CXLRptRequestCmdType), false));
      //Params vPrms = new Params(new Param("getResult", "true"));
      //String vUrl = null; // vPrms.bldUrlParams(this._lastRequestedURL);
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

    public void OpenResult(Action callback) {
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
