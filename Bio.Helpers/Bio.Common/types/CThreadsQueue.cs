using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.ComponentModel;

namespace Bio.Helpers.Common.Types {

  public class EThreadsQueueMaxSizeExceeded : Exception { 
  }

  public class OnBeforeActionEventArgs : CancelEventArgs {
    //public Object contextObject { get; set; }
    public CThreadsQueueItem item { get; set; }
  }

  public class OnAfterActionEventArgs : EventArgs {
    //public Object contextObject { get; set; }
    public CThreadsQueueItem item { get; set; }
  }

  public class OnErrorActionEventArgs : EventArgs {
    //public Object contextObject { get; set; }
    public CThreadsQueueItem item { get; set; }
    public Exception exception { get; set; }
  }

  public class CThreadsQueueItem {
    public String name { get; set; }
    public Action action { get; set; }
  }

  public class CThreadsQueue {
    private Queue<CThreadsQueueItem> _queue = null;
    private Thread _activeThead = null;

    private Queue<CThreadsQueueItem> queue {
      get {
        if (this._queue == null)
          this._queue = new Queue<CThreadsQueueItem>();
        return this._queue;
      }
    }

    public Int32 Count {
      get {
        Int32 v_queueCount = 0;
        lock (this.queue) {
          v_queueCount = this.queue.Count;
        }
        return v_queueCount;
      }
    }

    public Int32 QueueMaxSize { get; set; }

    public CThreadsQueue() {
      this.QueueMaxSize = 100;
    }

    public event EventHandler<OnBeforeActionEventArgs> OnBeforeAction;
    private void _doOnBeforeAction(OnBeforeActionEventArgs args) {
      var hndlr = this.OnBeforeAction;
      if (hndlr != null)
        hndlr(this, args);
    }

    public event EventHandler<OnAfterActionEventArgs> OnAfterAction;
    private void _doOnAfterAction(OnAfterActionEventArgs args) {
      var hndlr = this.OnAfterAction;
      if (hndlr != null)
        hndlr(this, args);
    }

    public event EventHandler<OnErrorActionEventArgs> OnErrorAction;
    private void _doOnErrorAction(OnErrorActionEventArgs args) {
      var hndlr = this.OnErrorAction;
      if (hndlr != null)
        hndlr(this, args);
    }

    public Boolean _isActive = false;
    public Boolean IsActive {
      get {
        return (this.queue.Count > 0) || this._isActive;
      }
    }
    private void _processQueue() {
      if (this._isActive) return;
      if (this.Count > 0) {
        this._isActive = true;

        CThreadsQueueItem nextItem = null;
        lock (this.queue) {
          nextItem = this.queue.Dequeue();
        }

        this._activeThead = new Thread(new ParameterizedThreadStart((c) => {
          var nItem = c as CThreadsQueueItem;
          try {
            try {
              var args = new OnBeforeActionEventArgs { Cancel = false, item = nItem };
              this._doOnBeforeAction(args);
              if (!args.Cancel) {
                if ((nItem != null) && (nItem.action != null))
                  nItem.action();
              }
            } catch (Exception ex) {
              this._doOnErrorAction(new OnErrorActionEventArgs { item = nItem, exception = ex });
            }
          } finally {
            try {
              this._doOnAfterAction(new OnAfterActionEventArgs { item = nItem });
            } catch (Exception ex) {
              this._doOnErrorAction(new OnErrorActionEventArgs { item = nItem, exception = ex });
            }


            this._isActive = false;
            this._processQueue();
          }
        }));
        this._activeThead.Name = nextItem.name;
        this._activeThead.Start(nextItem);
      }
    }

    public void addAction(String name, Action action) {
      Int32 curCount = this.Count;
      if (curCount >= this.QueueMaxSize)
        throw new EThreadsQueueMaxSizeExceeded();
      lock (this.queue) {
        this.queue.Enqueue(new CThreadsQueueItem { name = name, action = action });
      }
      this._processQueue();
    }
  }

}
