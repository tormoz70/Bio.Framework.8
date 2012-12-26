using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Collections;

namespace Bio.Helpers.Common.Types {
  public delegate void MessageLogWriterDelegate(String msg);
  public delegate void ErrorLogWriterDelegate(Exception ex);
  public abstract class CBackgroundThreadBase {
    protected Boolean isJobProcessing = false;
    protected Boolean isRunned = false;
    protected Thread _thread = null;
    protected SmtpCfg _smtpCfg = null;
    protected Boolean _requestAbort = false;
    protected String _adminEmail = null;

    public Boolean IsRunned {
      get {
        return this.isRunned;
      }
    }

    public MessageLogWriterDelegate msgLogWriter { get; set; }
    public ErrorLogWriterDelegate errLogWriter { get; set; }

    protected void log_msg(String msg) {
      if (this.msgLogWriter != null)
        this.msgLogWriter(msg);
    }
    protected void log_err(Exception ex) {
      if (this.errLogWriter != null)
        this.errLogWriter(ex);
    }

    protected String _serviceName = null;
    public CBackgroundThreadBase(String serviceName) {
      this._serviceName = serviceName;
    }

    protected abstract void init();
    protected abstract void logOutInitInfo();
    protected abstract void processJob();

    private void _sleep(Int32 secs) {
      Int32 v_waitings = 0;
      while (v_waitings <= secs) {
        Thread.Sleep(1000);
        if (this._requestAbort)
          break;
        v_waitings++;
      }
    }

    private void trySendMsgToAdmin(String subj, String msg) {
      this.log_msg("Отправка сообщения администратору ...");
      if ((this._smtpCfg != null) && !String.IsNullOrEmpty(this._adminEmail)) {
        try {
          smtpUtl.Send(
            this._smtpCfg.smtpServer,
            this._smtpCfg.port,
            this._smtpCfg.authUser,
            this._smtpCfg.authPwd,
            this._smtpCfg.fromMailAddr,
            this._adminEmail,
            subj,
            msg,
            Encoding.Default, //this._cfg.smtp.encoding
            Encoding.UTF8
          );
          this.log_msg(String.Format("Cообщение администратору успешно отправлено на {0}.", this._adminEmail));
        } catch (Exception ex) {
          this.log_msg(String.Format("При отправке сообщения администратору произошла ошибка. Сообщение: {0}\n" +
          "Параметры: smtpServer({1}), port({2}), authUser({3}), authPwd({4}), fromMailAddr({5}), encoding({6}), to({7})",
          ex.Message, this._smtpCfg.smtpServer, this._smtpCfg.port, this._smtpCfg.authUser, this._smtpCfg.authPwd,
            this._smtpCfg.fromMailAddr, this._smtpCfg.encoding, this._adminEmail));
        }
      } else {
        this.log_msg("Отправка сообщения администратору невозможна.");
      }
    }

    private const Int32 ciOnErrorTimeout = 10; // мин
    private void execute() {
      this._requestAbort = false;
      lock(this)
        this.isRunned = true;
      this.log_msg("Служба запущена.");
      String v_fatalErr = null;
      try {
        this.log_msg("Инициализация...");
        this.init();
        this.logOutInitInfo();
        this.log_msg("Инициализация выполнена.");
        this.trySendMsgToAdmin(String.Format("От {0}.", this._serviceName), "Служба запущена. Инициализация выполнена.");
        Int32 vHour = DateTime.Now.Hour;
        while (true) {
          if (!this.isJobProcessing) {
            this.isJobProcessing = true;
            try {
              this.processJob();
            } catch (EBioDBConnectionError ex) {
              String v_msg = String.Format("Ошибка доступа к БД. Служба остановлена на {0} мин.", ciOnErrorTimeout);
              this.log_msg(v_msg);
              this.log_msg("Отладочная информация:");
              this.log_err(ex);
              this.trySendMsgToAdmin(String.Format("От {0}.", this._serviceName), v_msg + "\n" + ex.ToString());
              this._sleep(ciOnErrorTimeout * 60);
              if (!this._requestAbort)
                this.log_msg("Служба продолжила работу...");
            } catch (EBioDBAccessError ex) {
              String v_msg = String.Format("Ошибка выполнения запроса к БД. Служба остановлена на {0} мин.", ciOnErrorTimeout);
              this.log_msg(v_msg);
              this.log_msg("Отладочная информация:");
              this.log_err(ex);
              this.trySendMsgToAdmin(String.Format("От {0}.", this._serviceName), v_msg + "\n" + ex.ToString());
              this._sleep(ciOnErrorTimeout * 60);
              if (!this._requestAbort) {
                this.log_msg("Служба продолжила работу...");
                this.trySendMsgToAdmin(String.Format("От {0}.", this._serviceName), "Служба продолжила работу.");
              }
            } finally {
              this.isJobProcessing = false;
            }
            Thread.Sleep(100);
            if (this._requestAbort) 
              break;
          }
          this._sleep(20);
          //if (DateTime.Now.Hour == vHour) {
          //  this.log_msg("Служба работает нормально...");
          //  vHour = DateTime.Now.AddHours(1).Hour;
          //}
        }
      } catch(ThreadAbortException) {
        Thread.Sleep(100);
      //} catch (EkbUnknownFileLoadException ex) {
      //} catch (EBioUnknownException ex) {
      //  this.log_err(ex);
      //  v_fatalErr = ex.ToString();
      } catch (Exception ex) {
        this.log_err(ex);
        v_fatalErr = ex.ToString();
      } finally {
        lock (this)
          this.isRunned = false;
        this.log_msg("Служба остановлена.");
        this.trySendMsgToAdmin(String.Format("От {0}.", this._serviceName),
          String.IsNullOrEmpty(v_fatalErr) ? "Служба остановлена. Нет ошибок." : 
                                              "Служба остановлена. Неизвестная ошибка: " + v_fatalErr);
      }
    }

    public void start() {
      if (!this.isRunned) {
        this.log_msg("Запуск службы...");
        this._thread = new Thread(new ThreadStart(this.execute));
        this._thread.Start();
      }
    }
    public void stop() {
      if (this.isRunned) {
        this.log_msg("Останов службы...");
        this._requestAbort = true;
      }
    }
  }
}
