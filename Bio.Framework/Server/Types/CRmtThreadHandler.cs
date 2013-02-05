namespace Bio.Framework.Server {

  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using System.ComponentModel;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common;
  using System.Threading;


  public delegate void CRmtThreadOnRunEvent(CRmtThreadHandler sender, ref IRemoteProcInst inst);
  /// <summary>
  /// Обработчик запросов от CRmtClientBase
  /// </summary>
  public class CRmtThreadHandler {

    public BioSession bioSess { get; private set; }
    public HttpContext context { get; private set; }
    public String contentType { get; private set; }
    public String appURL { get; private set; }
    public String bioCode { get; private set; }
    public XmlDocument bioDoc { get; private set; }
    public CParams bioParams { get; private set; }
    public String instanceUID { get; protected set; }

    public CRmtThreadHandler(BioSession bioSess, String contentType, String instanceUID) {
      this.bioSess = bioSess;
      this.context = this.bioSess.CurBioHandler.Context;
      this.contentType = contentType;
      var iObj = this.bioSess.CurBiObject;
      this.appURL = this.bioSess.Cfg.AppURL;
      this.bioCode = this.bioSess.CurBioCode;
      this.bioDoc = (iObj != null) ? iObj.IniDocument.XmlDoc : null;
      this.instanceUID = instanceUID;
    }

    public virtual IRemoteProcInst Instance {
      get {
        return this.context.Session[this.instanceUID] as IRemoteProcInst;
      }
    }

    protected virtual void SetInstance(IRemoteProcInst instance) {
      this.context.Session.Add(this.instanceUID, instance);
      instance.Run(ThreadPriority.Normal);
    }

    protected virtual void removeInstance() {
      var v_inst = this.Instance;
      if (v_inst != null)
        v_inst.Dispose();
      this.context.Session.Remove(this.instanceUID);
    }

    private void sendFileToClient() {
      var v_inst = this.Instance;
      if (v_inst != null) {
        this.context.Response.ClearContent();
        this.context.Response.ClearHeaders();
        this.context.Response.ContentType = this.contentType;
        String vFileName = v_inst.LastResultFile;
        String v_docExt = Path.GetExtension(vFileName);
        //String v_uid = "_"+DateTime.Now.Ticks;
        String vRemoteFName = Path.GetFileNameWithoutExtension(vFileName).Replace(".", "_") + /*v_uid + */v_docExt;
        this.context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + vRemoteFName + "\"");
        this.context.Response.WriteFile(vFileName);

        //FileStream inStream = File.OpenRead(vFileName); 
        //MemoryStream storeStream = new MemoryStream();

        //storeStream.SetLength(inStream.Length);
        //inStream.Read(storeStream.GetBuffer(), 0, (int)inStream.Length);

        //storeStream.Flush();
        //inStream.Close();

        //storeStream.WriteTo(this.context.Response.OutputStream);
        //storeStream.Close();
        
        this.context.Response.Flush();
        this.context.Response.Close();
      }
    }

    public event CRmtThreadOnRunEvent OnRunEvent;

    protected virtual void doJustAfterRun(IRemoteProcInst inst) { 
    }


    private void doOnRun() {
      var vEve = this.OnRunEvent;
      if (vEve != null) {
        IRemoteProcInst v_inst = null;
        vEve(this, ref v_inst);
        this.SetInstance(v_inst);
        this.doJustAfterRun(v_inst);
      }
    }
    private void run() {
      var v_inst = this.Instance;
      if (v_inst != null) {
        if (v_inst.IsRunning)
          return;
        else {
          v_inst.Dispose();
          //this.context.Session.Remove(this.instanceUID);
          this.removeInstance();
          v_inst = null;
        }
      }
      this.doOnRun();
    }

    protected virtual CRemoteProcessStatePack getCurrentStatePack() {
      var v_inst = this.Instance;
      if (v_inst != null) {
        Boolean v_hasResultFile = (v_inst.State == RemoteProcState.Done) &&
          File.Exists(v_inst.LastResultFile);

        var vStatePack = new CRemoteProcessStatePack() {
          bioCode = this.bioCode,
          owner = v_inst.Owner,
          pipe = v_inst.Pipe,
          sessionUID = v_inst.UID,
          started = v_inst.Started,
          duration = v_inst.Duration,
          state = v_inst.State,
          ex = v_inst.LastError,
          hasResultFile = v_hasResultFile,
          lastPipedLines = v_inst.popPipedLines()
        };

        return vStatePack;
      } else
        throw new EBioException("Выполнение процесса прервано по неизвестной причине.");
    }


    public void doExecute(RmtClientRequestCmd cmd, CParams bioParams) {
      //String vOper = pCmd;
      if (bioParams != null)
        this.bioParams = bioParams;
      if (this.bioParams == null)
        this.bioParams = new CParams();
      try {
        switch (cmd) {
          case RmtClientRequestCmd.Run: {
              // запусить
              this.run();
              this.context.Response.Write(new CBioResponse { 
                success = true, 
                bioParams = this.bioParams,
                rmtStatePacket = this.getCurrentStatePack()
              }.Encode());
            } break;
          case RmtClientRequestCmd.GetState: {
              // проверить состояния
              CBioResponse rspns = new CBioResponse() {
                success = true,
                bioParams = this.bioParams,
                rmtStatePacket = this.getCurrentStatePack()
              };
              this.context.Response.Write(rspns.Encode());
            } break;
          case RmtClientRequestCmd.Break: {
              // остановить
              var vRptInst = this.Instance;
              if (vRptInst != null) {
                vRptInst.Abort(null);
              }
              this.context.Response.Write(new CBioResponse { success = true, bioParams = this.bioParams }.Encode());
            } break;
          case RmtClientRequestCmd.GetResult: {
              // отдать результат
              this.sendFileToClient();
            } break;
          case RmtClientRequestCmd.Kill: {
              var vRptInst = this.Instance;
              if (vRptInst != null) {
                this.removeInstance();
              }
            } break;
        }

      } catch(Exception ex) {
        EBioException ebioex = EBioException.CreateIfNotEBio(ex);
        this.context.Response.Write(new CBioResponse { success = false, bioParams = this.bioParams, ex = ebioex }.Encode());
      }
    }
  }
}
