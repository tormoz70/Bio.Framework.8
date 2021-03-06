﻿namespace Bio.Framework.Server {
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading;
  using System.Collections;
  using System.IO;
  using System.Linq;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;

  public delegate IDbCommand LongOpPrepareCmdDelegate(IDbConnection conn, ref String currentSQL, ref Params currentParams);

  public class CLongOpProc : IRemoteProcInst {
    private RemoteProcState _state = RemoteProcState.Redy;
    private Thread _thread;
    private readonly LongOpPrepareCmdDelegate _prepareCmdProc;
    private EBioException _lastError;
    private IDbCommand _currentCmd;
    private const Int64 CI_PIPED_LINES_MAX_COUNT = 1000;
    private readonly Queue<String> _pipedLines;
    private readonly LongOpClientRequest _request;

    public String Owner { get; private set; }
    public IDBSession DBSess { get; private set; }

    public CLongOpProc(IDBSession dbSess, String owner, LongOpClientRequest request, LongOpPrepareCmdDelegate prepareCmdProc) {
      this.DBSess = dbSess;
      this.Owner = owner;
      this._request = request;
      this._prepareCmdProc = prepareCmdProc;
      this._pipedLines = new Queue<String>();
    }

    public void DoOnStarted() {
    }
    private void doOnFinished() {
    }
    private void doOnBreaked() {
    }
    private void doOnError() {
    }

    private void doProc() {
      if (this._conn != null) {
        try {
          this._state = RemoteProcState.Running;
          this.Started = DateTime.Now;
          this.DoOnStarted();
          if (this._prepareCmdProc != null) {
            String vCurrentSQL = null;
            Params vCurrentParams = null;
            this._currentCmd = this._prepareCmdProc(this._conn, ref vCurrentSQL, ref vCurrentParams);
            SQLCmd.ExecuteScript(this._currentCmd, vCurrentSQL, vCurrentParams);
          }
          Thread.Sleep(100);
        } catch (ThreadAbortException) {
        } catch (EBioSQLBreaked) {
          this._state = RemoteProcState.Breaked;
        } catch (Exception ex) {
          this._state = RemoteProcState.Error;
          this._lastError = EBioException.CreateIfNotEBio(ex);
        } finally {
          if (this._conn != null) {
            this._conn.Close();
            this._conn = null;
          }
          if (this.State == RemoteProcState.Breaking)
            this._state = RemoteProcState.Breaked;
          else if (this.State == RemoteProcState.Running)
            this._state = RemoteProcState.Done;
          
          if (this.State == RemoteProcState.Breaked)
            this.doOnBreaked();
          else if (this.State == RemoteProcState.Error)
            this.doOnError();
          this.doOnFinished();
        }
      }
    }

    public String Pipe { get { return this._request.Pipe; } }

    public bool IsRunning {
      get {
        return rmtUtl.IsRunning(this.State);
      }
    }

    public EBioException LastError {
      get { return this._lastError; }
    }

    public DateTime Started { get; private set; }
    public TimeSpan Duration {
      get {
        return Utl.Duration(this.Started);
      }
    }

    public String LastResultFile { get; private set; }

    public String UID { get; private set; }

    public String bioCode { get; private set; }

    public RemoteProcState State {
      get { return this._state; }
    }

    private String _getSessionUID() {
      String v_rslt = null;
      if (this._conn != null) {
        if (!String.IsNullOrEmpty(this.Pipe))
          v_rslt = (String)SQLCmd.ExecuteScalarSQL(this._conn, "select AI_PIPE.init(:pipeName) as f_result from dual", new Params(new Param("pipeName", this.Pipe)), 120);
        else
          v_rslt = "ORA-SESSION-ID:" + DBSession.GetSessionID(this._conn);
      }
      return v_rslt;
    }

    private IDbConnection _conn = null;
    public void Run(ThreadPriority priority) {
      this._conn = this.DBSess.GetConnection();
      this._state = RemoteProcState.Running;
      this.UID = this._getSessionUID();
      this._thread = new Thread(this.doProc);
      this._thread.Name = "LongOp-{" + this.bioCode + "}-[" + (!String.IsNullOrEmpty(this.Pipe) ? this.Pipe : "NO_PIPE_DEFINED") + "]";
      this._thread.Start();
    }

    public void Run() {
      this.Run(ThreadPriority.Normal);
    }

    public void Abort(Action callback) {
      if (this.IsRunning) {
        this._state = RemoteProcState.Breaking;
        if ((this._currentCmd != null) && (this._conn != null)) {
          this._currentCmd.Cancel();
        }
      }
      //if(new ArrayList {ThreadState.Background, ThreadState.Running, ThreadState.Suspended, ThreadState.SuspendRequested, ThreadState.WaitSleepJoin}.Contains(this.FThread.ThreadState))
      //  this.FThread.Abort();
    }

    //public static void Kill(ref CLongOpProc vProc) {
    //  if (vProc != null) {
    //    vProc.Abort(null);
    //    //vProc.Container.Remove(vProc);
    //    vProc = null;
    //  }
    //}

    public void Dispose() {
      this.Abort(() => {
        this._state = RemoteProcState.Disposed;
      });
    }

    public void pushPipedLine(string[] pipedLines) {
      if (pipedLines != null) {
        lock (this) {
          foreach (String lines in pipedLines) {
            if (!String.IsNullOrEmpty(lines)) {
              this._pipedLines.Enqueue(lines);
              if (this._pipedLines.Count > CI_PIPED_LINES_MAX_COUNT)
                this._pipedLines.Dequeue();
            }
          }
        }
      }
    }

    public String[] popPipedLines() {
      var rslt = this._pipedLines.ToArray();
      this._pipedLines.Clear();
      return rslt; 
    }
  }

}
