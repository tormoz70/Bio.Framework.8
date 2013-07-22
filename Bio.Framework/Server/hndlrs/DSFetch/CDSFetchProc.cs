namespace Bio.Framework.Server {
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
using System.Xml;

  public class CDSFetchProc : IRemoteProcInst {
    private RemoteProcState _state = RemoteProcState.Redy;
    private Thread _thread = null;
    private EBioException _lastError = null;
    private DSFetchClientRequest _request = null;
    private XmlElement _cursor_ds = null;
    private XmlElement _exec_ds = null;

    public String Owner { get; private set; }
    public IDBSession dbSess { get; private set; }

    public CDSFetchProc(IDBSession dbSess, String owner, DSFetchClientRequest request, XmlElement cursor_ds, XmlElement exec_ds) {
      this.dbSess = dbSess;
      this.Owner = owner;
      this._request = request;
      this._cursor_ds = cursor_ds;
      this._exec_ds = exec_ds;
    }

    public void doOnStarted() {
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
          this.doOnStarted();
          this._doProcessCursor();
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

    public String Pipe { get { return null; } }

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

    private IDbConnection _conn = null;
    public void Run(ThreadPriority priority) {
      this._conn = this.dbSess.GetConnection();
      this._state = RemoteProcState.Running;
      this.UID = null;
      this._thread = new Thread(this.doProc);
      this._thread.Name = "DSFetch-{" + this.bioCode + "}-[" + (!String.IsNullOrEmpty(this.Pipe) ? this.Pipe : "NO_PIPE_DEFINED") + "]";
      this._thread.Start();
    }

    public void Run() {
      this.Run(ThreadPriority.Normal);
    }

    public void Abort(Action callback) {
      if (this.IsRunning) {
        this._state = RemoteProcState.Breaking;
      }
    }

    public void Dispose() {
      this.Abort(() => {
        this._state = RemoteProcState.Disposed;
      });
    }

    public void pushPipedLine(string[] pipedLines) {
    }

    public String[] popPipedLines() {
      return null;
    }

    private void _doProcessRecord(JsonStoreMetadata metadata, JsonStoreRow row) {
      var conn = this.dbSess.GetConnection();
      try {
        var vCmd = new CJSCursor(conn, this._exec_ds, this._request.ExecBioCode);
        vCmd.DoExecuteSQL(metadata, row, this._request.BioParams, 120);
      } finally {
        if (conn != null)
          conn.Close();
      }
    }

    private void _doProcessCursor() {
      var conn = this.dbSess.GetConnection();
      var vCursor = new CJSCursor(conn, this._cursor_ds, this.bioCode);
      vCursor.Init(null, this._request.BioParams, this._request.Filter, this._request.Sort, this._request.Selection, 120);
      vCursor.Open(120);
      try {
        while (vCursor.Next()) {
          if (this._state == RemoteProcState.Breaking)
            break;
          var newRow = vCursor.rqPacket.MetaData.CreateNewRow();
          // перебираем все поля одной записи
          foreach (Field vCur in vCursor.Fields) {
            var vFName = vCur.FieldName;
            var vFVal = vCur;
            newRow.Values[vCursor.rqPacket.MetaData.IndexOf(vFName)] = vCur.AsObject;
          }
          this._doProcessRecord(vCursor.rqPacket.MetaData, newRow);
        }
      } finally {
        vCursor.Close();
        if (conn != null)
          conn.Close();
      }
    }
  }

}
