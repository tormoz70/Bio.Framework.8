using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace Bio.Helpers.Common {
  public class smtpUtl {
    public static void Send(
        String smtpServer,
        Int32  port,
        String authUser,
        String authPwd,
        String fromMailAddr,
        String toMailAddr,
        String subject,
        String body,
        Encoding encodingSubj,
        Encoding encodingBody) {
      SmtpClient v_cli = new SmtpClient(smtpServer, port);
      if ((authUser != null) && (!authUser.Equals("none"))) {
        v_cli.Credentials = new NetworkCredential(authUser, authPwd);
      }
      String[] v_toAddrs = Utl.SplitString(toMailAddr, new Char[] {' ', ';', ',', '|', '/'});
      if (v_toAddrs.Length > 0) {
        MailMessage message = new MailMessage(
          new MailAddress(fromMailAddr),
          new MailAddress(v_toAddrs[0]));
        if (v_toAddrs.Length > 1){
          for (int i = 1; i < v_toAddrs.Length; i++)
            message.To.Add(new MailAddress(v_toAddrs[i]));
        }
        message.SubjectEncoding = encodingSubj;
        message.Subject = subject;
        message.BodyEncoding = encodingBody;
        message.Body = body;
        //message.
        v_cli.Send(message);
      }
    }

    public static void Send(
        String smtpServer,
        Int32 port,
        String authUser,
        String authPwd,
        String fromMailAddr,
        String toMailAddr,
        String subject,
        String body,
        String encodingSubj,
        String encodingBody) {
      Encoding v_encodingSubj = String.IsNullOrEmpty(encodingSubj) ? Encoding.Default : Encoding.GetEncoding(encodingSubj);
      Encoding v_encodingBody = String.IsNullOrEmpty(encodingBody) ? Encoding.Default : Encoding.GetEncoding(encodingBody);
      Send(
        smtpServer,
        port,
        authUser,
        authPwd,
        fromMailAddr,
        toMailAddr,
        subject,
        body,
        v_encodingSubj,
        v_encodingBody);
    }
  }
}
