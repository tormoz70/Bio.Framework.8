using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using Bio.Helpers.Common.Types;

namespace Bio.Helpers.XLFRpt2.Srvc {

  public class CBackgroundThread : BackgroundThreadBase {
    private readonly ThreadStart _queueProcessor = null;

    public CBackgroundThread(String seviceName, String adminEmail, SmtpCfg smtp, ErrorLogWriterDelegate errLogger, ThreadStart execute)
      : base(seviceName) {
      if (errLogger == null)
        throw new EBioException("Праметер p_errLogger не определен!");
      this.smtpCfg = smtp;
      this.adminEmail = adminEmail;
      this.ErrLogWriter = errLogger;
      this._queueProcessor = execute;
    }

    protected override void init() {
      //base.init();
    }

    protected override void logOutInitInfo() {
      this.log_msg("email администратора: " + this.adminEmail);
    }

    protected override void processJob() {
      if (this._queueProcessor != null)
        this._queueProcessor();
    }
  }

}
