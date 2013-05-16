namespace Bio.Framework.Server {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using System.ComponentModel;

  public enum TRemoteConnectionStatus { 
    [Description("try")]
    rcsTry, 
    [Description("ok")]
    rcsOk, 
    [Description("off")]
    rcsOff, 
    [Description("bad user")]
    rcsBadUser, 
    [Description("bad pwd")]
    rcsBadPwd 
  };
  public interface IBioCfg {
    Boolean DoReloadIOOnInit { get; }
    Boolean EnableDocInhirance { get; }
    Boolean Debug { get; }
    String AppURL { get; }
    String LocalPath { get; }
    String IniPath { get; }
    String IniURL { get; }
    String RptPath { get; }
    String BioSysTitle { get; }
    String WorkspacePath { get; }
    String LocalIOCfgPath { get; }
    String TmpPath { get; }
    String RptLogsPath { get; }
    CBioUser CurUser { get; }
    IDBSession dbSession { get; }
    void regConnect(String pUser,
                    String pSessionID,
                    String pSessionRemoteIP,
                    String pSessionRemoteHost,
                    String pSessionRemoteClient,
                    TRemoteConnectionStatus pStatus);
    void regUser(CBioUser pUser);
    void Login(String pUser);
    //String getDBConnStr(String pID);
    String ConnectionString { get; }
    //void doOnBeforeDBConnect(Object sender, DBConnBeforeEventArgs args);
    //void doOnAfterDBConnect(Object sender, DBConnAfterEventArgs args);

  }
}
