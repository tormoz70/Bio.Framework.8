namespace Bio.Helpers.Common.Types {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using System.Runtime.Serialization;


  public class EBioLogin:EBioException {
    public EBioLogin()
      : base() {
    }
    public EBioLogin(String pMsg)
      : base(pMsg) {
    }
    //protected EBioLogin(SerializationInfo info, StreamingContext context) : base (info, context) { }
  }

  public class EBioError:EBioLogin {
    public EBioError()
      : base() {
    }
    public EBioError(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioBadLogin:EBioLogin {
    public EBioBadLogin()
      : base() {
    }
    public EBioBadLogin(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioBadResult:EBioLogin {
    public EBioBadResult()
      : base() {
    }
    public EBioBadResult(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioCancel:EBioLogin {
    public EBioCancel()
      : base() {
    }
    public EBioCancel(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioAutenticationError:EBioLogin {
    public EBioAutenticationError()
      : base() {
    }
    public EBioAutenticationError(String pMsg)
      : base(pMsg) {
    }
    //protected EBioAutenticationError(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
  public class EBioBadPwd : EBioAutenticationError {
    public EBioBadPwd()
      : base() {
    }
    public EBioBadPwd(String pMsg)
      : base(pMsg) {
    }
    //protected EBioBadPwd(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
  public class EBioBadUser:EBioAutenticationError {
    public EBioBadUser()
      : base() {
    }
    public EBioBadUser(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioUncnfrmdUser: EBioAutenticationError {
    public EBioUncnfrmdUser()
      : base() {
    }
    public EBioUncnfrmdUser(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioBlockedUser: EBioAutenticationError {
    public EBioBlockedUser()
      : base() {
    }
    public EBioBlockedUser(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioLoggedOut: EBioLogin {
    public EBioLoggedOut()
      : base() {
    }
    public EBioLoggedOut(String pMsg)
      : base(pMsg) {
    }
  }
  public class EBioOk:EBioLogin {
    public EBioOk()
      : base() {
    }
    public BioUser Usr { get; set; }
    public EBioOk(BioUser pUsr)
      : base() {
      this.Usr = (BioUser)pUsr.Clone();
      //this.Usr.USR_PWD = null;
      //this.Usr.ConnectionString = null;
    }
    public EBioOk(String pMsg)
      : base(pMsg) {
    }

#if !SILVERLIGHT
    protected override XmlDocument encode2xml() {
      XmlDocument vDoc = base.encode2xml();
      XmlElement vRoot = vDoc.DocumentElement;
      String vUsrJSON = ((EBioOk)this).Usr.Encode();
      vRoot.AppendChild(vDoc.CreateElement("cuser")).AppendChild(vDoc.CreateCDataSection(vUsrJSON));
      return vDoc;
    }
#endif

  }
  public class EBioRestartApp:EBioLogin {
    public EBioRestartApp()
      : base() {
    }
    public EBioRestartApp(String pMsg)
      : base(pMsg) {
    }

#if !SILVERLIGHT
    protected override XmlDocument encode2xml(/*String pAppURL*/) {
      XmlDocument vRslt = base.encode2xml(/*pAppURL*/);
      return vRslt;
    }
#endif

  }
  public class EBioStart:EBioLogin {
    public EBioStart()
      : base() {
    }
    public EBioStart(String pMsg)
      : base(pMsg) {
    }
  }
}
