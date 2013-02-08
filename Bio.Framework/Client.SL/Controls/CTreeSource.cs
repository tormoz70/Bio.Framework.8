using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Helpers.Common.Types;
using System.Collections.ObjectModel;
using Bio.Framework.Packets;
using System.Windows.Threading;
using Bio.Helpers.Common;
using System.ComponentModel;

namespace Bio.Framework.Client.SL.Old {

  public class CTreeSourceItemBase {
    internal CJSTree _ownerTree { get; set; }
    internal IAjaxMng _ajaxMng { get; set; }
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
      this._ajaxMng = ajaxMng;
    }
  }

  public class CTreeSourceItem<T> : CTreeSourceItemBase where T : CTreeSourceItemBase, new() {
    private CJsonStoreClient _cli = null;

    public CTreeSourceItem() : base() {
      this._cli = new CJsonStoreClient();
    }

    public CTreeSourceItem(CJSTree ownerTree)
      : this() {
      this.OwnerTreeView = ownerTree;
    }

    public IAjaxMng AjaxMng { 
      get {
        T v_root = this._getRoot(this as T);
        return v_root._ajaxMng;
      }
      set {
        T v_root = this._getRoot(this as T);
        v_root._ajaxMng = value;
      }
    }

    public CJSTree OwnerTreeView {
      get {
        T v_root = this._getRoot(this as T);
        return v_root._ownerTree;
      }
      set {
        T v_root = this._getRoot(this as T);
        v_root._ownerTree = value;
      }
    }

    private T _getRoot(T item) {
      if (item.Parent == null)
        return item as T;
      else
        return this._getRoot(item.Parent as T);
    }

    public T RootItem {
      get {
        return this._getRoot(this as T);
      }
    }

    private Int32 _calcLevel(T parent, Int32 level) {
      if (parent == null)
        return level;
      else
        return this._calcLevel(parent.Parent as T, level + 1);
    }

    public Int32 Level {
      get {
        return this._calcLevel(this.Parent as T, 0);
      }
    }

    public T Add(Object id, String name, params T[] items) {
      T newItem = new T() {
        Loaded = false,
        Parent = this,
        ID = id,
        Name = name
      };
      foreach (var item in items) {
        item.Parent = this;
        newItem.Items.Add(item);

      };
      this.Items.Add(newItem);
      return newItem;
    }

    protected virtual T doOnLoadItem(CJsonStoreMetadata metadata, CJsonStoreRow row) {
      int bgnIndx = 0;
      if(metadata.PKDefined)
        bgnIndx = 1;
      Object vID = row.Values[bgnIndx];
      T item = this.Add(vID, row.Values[bgnIndx + 1] as String);
      item.BioCode = this.BioCode;
      return item;
    }

    protected virtual void doOnBeforeLoadItem(ref Params prms) { 
    }

    public void Load(Action<T> actOnItem, AjaxRequestDelegate callback) {
      if (this.OwnerTreeView == null)
        throw new Exception("Не определен атрибут OwnerTreeView в корневом элементе!");
      this._cli.ajaxMng = this.AjaxMng;
      this._cli.bioCode = this.BioCode;
      Params prms = null;
      if (!this.Equals(this.RootItem)) {
        prms = new Params();
        prms.Add("parent_id", this.ID);
      }

      this.doOnBeforeLoadItem(ref prms);

      BeforeLoadItemChildrenEventArgs args = new BeforeLoadItemChildrenEventArgs {
        Params = prms
      };
      //this.OwnerTreeView.processBeforeLoadItemChildren(this, args);
      if (args.Cancel)
        return;

      this._cli.Load(args.Params, (s, a) => {
        //this.OwnerTreeView.Dispatcher.BeginInvoke(() => {
        if (a.response.success) {
          CJsonStoreResponse rsp = a.response as CJsonStoreResponse;
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
