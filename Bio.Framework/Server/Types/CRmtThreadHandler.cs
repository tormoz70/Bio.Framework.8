namespace Bio.Framework.Server {

  using System;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Helpers.Common.Types;
  using Packets;
  using Helpers.Common;
  using System.Threading;


  public delegate void CRmtThreadOnRunEvent(CRmtThreadHandler sender, ref IRemoteProcInst inst);
  /// <summary>
  /// Обработчик запросов от CRmtClientBase
  /// </summary>
  public class CRmtThreadHandler {

    public BioSession BioSess { get; private set; }
    public HttpContext Context { get; private set; }
    public String ContentType { get; private set; }
    public String AppURL { get; private set; }
    public String BioCode { get; private set; }
    public XmlDocument BioDoc { get; private set; }
    public Params BioParams { get; private set; }
    public String InstanceUID { get; protected set; }

    public CRmtThreadHandler(BioSession bioSess, String contentType, String instanceUID) {
      this.BioSess = bioSess;
      this.Context = this.BioSess.CurBioHandler.Context;
      this.ContentType = contentType;
      var iObj = this.BioSess.CurBiObject;
      this.AppURL = this.BioSess.Cfg.AppURL;
      this.BioCode = this.BioSess.CurBioCode;
      this.BioDoc = (iObj != null) ? iObj.IniDocument.XmlDoc : null;
      this.InstanceUID = instanceUID;
    }

    public virtual IRemoteProcInst Instance {
      get {
        return this.Context.Session[this.InstanceUID] as IRemoteProcInst;
      }
    }

    protected virtual void SetInstance(IRemoteProcInst instance) {
      this.Context.Session.Add(this.InstanceUID, instance);
      instance.Run(ThreadPriority.Normal);
    }

    protected virtual void removeInstance() {
      var v_inst = this.Instance;
      if (v_inst != null)
        v_inst.Dispose();
      this.Context.Session.Remove(this.InstanceUID);
    }

    private void _sendFileToClient() {
      var v_inst = this.Instance;
      if (v_inst != null) {
        this.Context.Response.ClearContent();
        this.Context.Response.ClearHeaders();
        this.Context.Response.ContentType = this.ContentType;
        var fileName = v_inst.LastResultFile;
        var docExt = Path.GetExtension(fileName);
        var vRemoteFName = Path.GetFileNameWithoutExtension(fileName).Replace(".", "_") + /*v_uid + */docExt;
        this.Context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + vRemoteFName + "\"");
        this.Context.Response.WriteFile(fileName);

        this.Context.Response.Flush();
        this.Context.Response.Close();
      }
    }

    public event CRmtThreadOnRunEvent OnRunEvent;

    protected virtual void doJustAfterRun(IRemoteProcInst inst) { 
    }


    private void _doOnRun() {
      var vEve = this.OnRunEvent;
      if (vEve != null) {
        IRemoteProcInst v_inst = null;
        vEve(this, ref v_inst);
        this.SetInstance(v_inst);
        this.doJustAfterRun(v_inst);
      }
    }
    private void _run() {
      var v_inst = this.Instance;
      if (v_inst != null) {
        if (v_inst.IsRunning)
          return;
        v_inst.Dispose();
        this.removeInstance();
      }
      this._doOnRun();
    }

    protected virtual RemoteProcessStatePack getCurrentStatePack() {
      var v_inst = this.Instance;
      if (v_inst != null) {
        var hasResultFile = (v_inst.State == RemoteProcState.Done) &&
          File.Exists(v_inst.LastResultFile);

        var vStatePack = new RemoteProcessStatePack() {
          BioCode = this.BioCode,
          owner = v_inst.Owner,
          pipe = v_inst.Pipe,
          sessionUID = v_inst.UID,
          Started = v_inst.Started,
          Duration = v_inst.Duration,
          State = v_inst.State,
          Ex = v_inst.LastError,
          HasResultFile = hasResultFile,
          lastPipedLines = v_inst.popPipedLines()
        };

        return vStatePack;
      } else
        throw new EBioException("Выполнение процесса прервано по неизвестной причине.");
    }


    public void DoExecute(RmtClientRequestCmd cmd, Params bioParams) {
      if (bioParams != null)
        this.BioParams = bioParams;
      if (this.BioParams == null)
        this.BioParams = new Params();
      try {
        switch (cmd) {
          case RmtClientRequestCmd.Run: {
              // запусить
              this._run();
              this.Context.Response.Write(new BioResponse { 
                Success = true, 
                BioParams = this.BioParams,
                RmtStatePacket = this.getCurrentStatePack()
              }.Encode());
            } break;
          case RmtClientRequestCmd.GetState: {
              // проверить состояния
              var rspns = new BioResponse {
                Success = true,
                BioParams = this.BioParams,
                RmtStatePacket = this.getCurrentStatePack()
              };
              this.Context.Response.Write(rspns.Encode());
            } break;
          case RmtClientRequestCmd.Break: {
              // остановить
              var vRptInst = this.Instance;
              if (vRptInst != null) {
                vRptInst.Abort(null);
              }
              this.Context.Response.Write(new BioResponse { Success = true, BioParams = this.BioParams }.Encode());
            } break;
          case RmtClientRequestCmd.GetResult: {
              // отдать результат
              this._sendFileToClient();
            } break;
          case RmtClientRequestCmd.Kill: {
              var vRptInst = this.Instance;
              if (vRptInst != null) {
                this.removeInstance();
              }
            } break;
        }

      } catch(Exception ex) {
        var ebioex = EBioException.CreateIfNotEBio(ex);
        this.Context.Response.Write(new BioResponse { Success = false, BioParams = this.BioParams, Ex = ebioex }.Encode());
      }
    }
  }
}
