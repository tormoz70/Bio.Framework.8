using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using Bio.Helpers.Common.Types;

namespace Bio.Helpers.XLFRpt2.Srvc {
  /*public class CBackgroundThread {
    private Boolean isRunned = false;
    private Thread _thread = null;
    private ThreadStart queueProcessor = null;
    private ErrorLogWriterDelegate errLogger = null;
    public CBackgroundThread(ErrorLogWriterDelegate p_errLogger, ThreadStart execute) {
      if (p_errLogger == null)
        throw new EBioException("Праметер p_errLogger не определен!");
      this.errLogger = p_errLogger;
      this.queueProcessor = execute;
    }

    private void execute() {
      lock(this)
        this.isRunned = true;
      try {
        while (true) {
          this.queueProcessor();
          Thread.Sleep(10*1000);
        }
      } catch(ThreadAbortException) {
      } catch (Exception ex) {
        this.errLogger(ex);
      } finally {
        lock (this)
          this.isRunned = false;
      }
    }

    public void start() {
      if (!this.isRunned) {
        this._thread = new Thread(new ThreadStart(this.execute));
        this._thread.Start();
      }
    }
    public void stop() {
      if (this.isRunned) {
        this._thread.Abort();
      }
    }
  }*/

  public class CBackgroundThread1 : CBackgroundThreadBase {
    private ThreadStart queueProcessor = null;

    public CBackgroundThread1(String seviceName, String adminEmail, SmtpCfg smtp, ErrorLogWriterDelegate errLogger, ThreadStart execute)
      : base(seviceName) {
      if (errLogger == null)
        throw new EBioException("Праметер p_errLogger не определен!");
      this._smtpCfg = smtp;
      this._adminEmail = adminEmail;
      this.errLogWriter = errLogger;
      this.queueProcessor = execute;
    }

    protected override void init() {
      //base.init();
    }

    protected override void logOutInitInfo() {
      this.log_msg("email администратора: " + this._adminEmail);
    }

    protected override void processJob() {
      if (this.queueProcessor != null)
        this.queueProcessor();
    }
  }

}
