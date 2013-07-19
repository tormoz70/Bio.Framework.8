using System;
using Bio.Helpers.Common.Types;
using System.Collections.ObjectModel;
using Bio.Framework.Packets;

namespace Bio.Framework.Client.SL.Old {

  public class CTreeSourceItemBase {
    internal JSTree ownerTree { get; set; }
    internal IAjaxMng ajaxMng { get; set; }
    public CTreeSourceItemBase Parent { get; set; }
    public Object ID { get; set; }
    public String Name { get; set; }
    public Boolean Loaded { get; internal set; }
    public String BioCode { get; set; }
    public ObservableCollection<CTreeSourceItemBase> Items { get; set; }
    
    public CTreeSourceItemBase() {
      this.Items = new ObservableCollection<CTreeSourceItemBase>();
    }
    public CTreeSourceItemBase(IAjaxMng ajaxMng)
      : this() {
      this.ajaxMng = ajaxMng;
    }
  }

  public class CTreeSourceItem<T> : CTreeSourceItemBase where T : CTreeSourceItemBase, new() {
    private readonly JsonStoreClient _cli;

    public CTreeSourceItem() {
      this._cli = new JsonStoreClient();
    }

    public CTreeSourceItem(JSTree ownerTree)
      : this() {
      this.OwnerTreeView = ownerTree;
    }

    public IAjaxMng AjaxMng { 
      get {
        var v_root = this._getRoot(this as T);
        return v_root.ajaxMng;
      }
      set {
        var v_root = this._getRoot(this as T);
        v_root.ajaxMng = value;
      }
    }

    public JSTree OwnerTreeView {
      get {
        T v_root = this._getRoot(this as T);
        return v_root.ownerTree;
      }
      set {
        T v_root = this._getRoot(this as T);
        v_root.ownerTree = value;
      }
    }

    private T _getRoot(T item) {
      return item.Parent == null ? item : this._getRoot(item.Parent as T);
    }

    public T RootItem {
      get {
        return this._getRoot(this as T);
      }
    }

    private Int32 _calcLevel(T parent, Int32 level) {
      return parent == null ? level : this._calcLevel(parent.Parent as T, level + 1);
    }

    public Int32 Level {
      get {
        return this._calcLevel(this.Parent as T, 0);
      }
    }

    public T Add(Object id, String name, params T[] items) {
      var newItem = new T {
        Loaded = false,
        Parent = this,
        ID = id,
        Name = name
      };
      foreach (var item in items) {
        item.Parent = this;
        newItem.Items.Add(item);

      }
      this.Items.Add(newItem);
      return newItem;
    }

    protected virtual T doOnLoadItem(CJsonStoreMetadata metadata, JsonStoreRow row) {
      var bgnIndx = 0;
      if(metadata.PKDefined)
        bgnIndx = 1;
      var vID = row.Values[bgnIndx];
      var item = this.Add(vID, row.Values[bgnIndx + 1] as String);
      item.BioCode = this.BioCode;
      return item;
    }

    protected virtual void doOnBeforeLoadItem(ref Params prms) { 
    }

    public void Load(Action<T> actOnItem, AjaxRequestDelegate callback) {
      if (this.OwnerTreeView == null)
        throw new Exception("Не определен атрибут OwnerTreeView в корневом элементе!");
      this._cli.AjaxMng = this.AjaxMng;
      this._cli.BioCode = this.BioCode;
      Params prms = null;
      if (!this.Equals(this.RootItem)) {
        prms = new Params();
        prms.Add("parent_id", this.ID);
      }

      this.doOnBeforeLoadItem(ref prms);

      var args = new BeforeLoadItemChildrenEventArgs {
        Params = prms
      };
      if (args.Cancel)
        return;

      this._cli.Load(args.Params, (s, a) => {
        //this.OwnerTreeView.Dispatcher.BeginInvoke(() => {
        if (a.Response.Success) {
          var rsp = a.Response as JsonStoreResponse;
          if (rsp != null) {
            this.Items.Clear();
            foreach (var r in rsp.packet.rows) {
              T item = this.doOnLoadItem(rsp.packet.metaData, r);
              if (actOnItem != null) actOnItem(item);
            }
            this.Loaded = true;
          }
        }
        if (callback != null) callback(s, a);
        //});
      });
    }
  }

}
