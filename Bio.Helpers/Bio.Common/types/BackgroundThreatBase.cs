using System;
using System.Text;
using System.Threading;

namespace Bio.Helpers.Common.Types {
  /// <summary/>
  public delegate void MessageLogWriterDelegate(String msg);
  /// <summary/>
  public delegate void ErrorLogWriterDelegate(Exception ex);
  /// <summary/>
  public abstract class BackgroundThreadBase {
    protected Boolean isJobProcessing = false;
    protected Boolean isRunned = false;
    protected Thread thread = null;
    protected SmtpCfg smtpCfg = null;
    protected Boolean rqAbort = false;
    protected String adminEmail = null;
    protected Int32 sleepMSecsOnEachCicle = 5 * 1000; // 5 secs

    /// <summary/>
    public Boolean IsRunned {
      get {
        return this.isRunned;
      }
    }

    /// <summary/>
    public MessageLogWriterDelegate MsgLogWriter { get; set; }
    /// <summary/>
    public ErrorLogWriterDelegate ErrLogWriter { get; set; }

    protected virtual void log_msg(String msg) {
      lock (this) {
        if (this.MsgLogWriter != null)
          this.MsgLogWriter(msg);
      }
    }
    protected virtual void log_err(Exception ex) {
      lock (this) {
        if (this.ErrLogWriter != null)
          this.ErrLogWriter(ex);
      }
    }

    protected String serviceName = null;
    protected BackgroundThreadBase(String serviceName) {
      this.serviceName = serviceName;
    }

    protected abstract void init();
    protected abstract void logOutInitInfo();
    protected abstract void processJob();
    protected virtual void onStop() {}

    private void _sleepSecs(Int32 secs) {
      var v_waitings = 0;
      while (v_waitings <= secs) {
        Thread.Sleep(1000);
        if (this.rqAbort)
          break;
        v_waitings++;
      }
    }

    private void _trySendMsgToAdmin(String subj, String msg) {
      this.log_msg("Отправка сообщения администратору ...");
      if ((this.smtpCfg != null) && !String.IsNullOrEmpty(this.adminEmail)) {
        try {
          smtpUtl.Send(
            this.smtpCfg.smtpServer,
            this.smtpCfg.port,
            this.smtpCfg.authUser,
            this.smtpCfg.authPwd,
            this.smtpCfg.fromMailAddr,
            this.adminEmail,
            subj,
            msg,
            Encoding.Default, 
            Encoding.UTF8
          );
          this.log_msg(String.Format("Cообщение администратору успешно отправлено на {0}.", this.adminEmail));
        } catch (Exception ex) {
          this.log_msg(String.Format("При отправке сообщения администратору произошла ошибка. Сообщение: {0}\n" +
            "Параметры: smtpServer({1}), port({2}), authUser({3}), authPwd({4}), fromMailAddr({5}), encoding({6}), to({7})",
          ex.Message, this.smtpCfg.smtpServer, this.smtpCfg.port, this.smtpCfg.authUser, this.smtpCfg.authPwd,
            this.smtpCfg.fromMailAddr, this.smtpCfg.encoding, this.adminEmail));
        }
      } else {
        this.log_msg("Отправка сообщения администратору невозможна.");
      }
    }

    private const Int32 CI_ON_ERROR_TIMEOUT = 10; // мин
    private void _execute() {
      this.rqAbort = false;
      lock(this)
        this.isRunned = true;
      this.log_msg("Служба запущена.");
      String v_fatalErr = null;
      try {
        this.log_msg("Инициализация...");
        this.init();
        this.logOutInitInfo();
        this.log_msg("Инициализация выполнена.");
        this._trySendMsgToAdmin(String.Format("От {0}.", this.serviceName), "Служба запущена. Инициализация выполнена.");
        //var v_hour = DateTime.Now.Hour;
        while (true) {
          if (!this.isJobProcessing) {
            this.isJobProcessing = true;
            try {
              this.processJob();
            } catch (EBioDBConnectionError ex) {
              var v_msg = String.Format("Ошибка доступа к БД. Служба остановлена на {0} мин.", CI_ON_ERROR_TIMEOUT);
              this.log_msg(v_msg);
              this.log_msg("Отладочная информация:");
              this.log_err(ex);
              this._trySendMsgToAdmin(String.Format("От {0}.", this.serviceName), v_msg + "\n" + ex.ToString());
              this._sleepSecs(CI_ON_ERROR_TIMEOUT * 60);
              if (!this.rqAbort)
                this.log_msg("Служба продолжила работу...");
            } catch (EBioDBAccessError ex) {
              var v_msg = String.Format("Ошибка выполнения запроса к БД. Служба остановлена на {0} мин.", CI_ON_ERROR_TIMEOUT);
              this.log_msg(v_msg);
              this.log_msg("Отладочная информация:");
              this.log_err(ex);
              this._trySendMsgToAdmin(String.Format("От {0}.", this.serviceName), v_msg + "\n" + ex);
              this._sleepSecs(CI_ON_ERROR_TIMEOUT * 60);
              if (!this.rqAbort) {
                this.log_msg("Служба продолжила работу...");
                this._trySendMsgToAdmin(String.Format("От {0}.", this.serviceName), "Служба продолжила работу.");
              }
            } finally {
              this.isJobProcessing = false;
            }
            Thread.Sleep(100);
            if (this.rqAbort) 
              break;
          }
          //this._sleep(20);
          Thread.Sleep(this.sleepMSecsOnEachCicle);
          //if (DateTime.Now.Hour == vHour) {
          //  this.log_msg("Служба работает нормально...");
          //  vHour = DateTime.Now.AddHours(1).Hour;
          //}
        }
      } catch(ThreadAbortException) {
        Thread.Sleep(100);
      } catch (Exception ex) {
        this.log_err(ex);
        v_fatalErr = ex.ToString();
      } finally {
        lock (this)
          this.isRunned = false;
        this.log_msg("Служба остановлена.");
        this._trySendMsgToAdmin(String.Format("От {0}.", this.serviceName),
          String.IsNullOrEmpty(v_fatalErr) ? "Служба остановлена. Нет ошибок." : 
                                              "Служба остановлена. Неизвестная ошибка: " + v_fatalErr);
      }
    }

    /// <summary/>
    public void Start() {
      if (!this.isRunned) {
        this.log_msg("Запуск службы...");
        this.thread = new Thread(this._execute);
        this.thread.Name = this.serviceName;
        this.thread.Start();
      }
    }
    /// <summary/>
    public void Stop() {
      if (this.isRunned) {
        this.log_msg("Останов службы...");
        this.rqAbort = true;
        this.onStop();
      }
    }
  }
}
