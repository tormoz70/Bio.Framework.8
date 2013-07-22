using System;
using Bio.Helpers.Common.Types;
using System.Collections.ObjectModel;
using Bio.Framework.Packets;

namespace Bio.Framework.Client.SL {

  public abstract class JSTreeItemBase {
    protected JsonStoreClient _cli = null;
    protected JSTree _ownerTree { get; set; }
    protected IAjaxMng _ajaxMng { get; set; }
    public JSTreeItemBase Parent { get; set; }
    public CTreeViewItem Container { get; internal set; }
    public Object ID { get; set; }
    public virtual string Name { get; set; }

    public Boolean Loaded { get; internal set; }
    private String _bioCode;
    public String BioCode { 
      get {
        return this._bioCode ?? this.RootItem.BioCode;
      } 
      set {
        this._bioCode = value;
      } 
    }
    public ObservableCollection<JSTreeItemBase> Items { get; set; }

    protected JSTreeItemBase() {
      this._cli = new JsonStoreClient();
      this.Items = new ObservableCollection<JSTreeItemBase>();
    }

    protected JSTreeItemBase(JSTree ownerTree, IAjaxMng ajaxMng)
      : this() {
        this._ownerTree = ownerTree;
      this._ajaxMng = ajaxMng;
    }

    private JSTreeItemBase _getRoot(JSTreeItemBase item) {
      if (item.Parent == null)
        return item;
      return this._getRoot(item.Parent);
    }

    public IAjaxMng AjaxMng {
      get {
        return this.RootItem._ajaxMng;
      }
    }

    public JSTree OwnerTreeView {
      get {
        return this.RootItem._ownerTree;
      }
    }

    public JSTreeItemBase RootItem {
      get {
        return this._getRoot(this);
      }
    }
    private Int32 _calcLevel(JSTreeItemBase parent, Int32 level) {
      if (parent == null)
        return level;
      return this._calcLevel(parent.Parent, level + 1);
    }

    public Int32 Level {
      get {
        return this._calcLevel(this.Parent, 0);
      }
    }

    private String _getPathID(JSTreeItemBase parent, String path) {
      if (parent == null)
        return path;
      var v_path = parent.ID + (String.IsNullOrEmpty(path) ? null : "/" + path);
      return this._getPathID(parent.Parent, v_path);
    }

    public String PathID {
      get {
        return this._getPathID(this, null);
      }
    }

    public T CreateNewChildItem<T>(Object id, String name) where T : JSTreeItemBase, new() {
      var newItem = new T {
        Loaded = false,
        Parent = this,
        ID = id,
        Name = name
      };
      this.Items.Add(newItem);
      return newItem;
    }



    protected abstract JSTreeItemBase doOnCreateNewChildItem(JsonStoreMetadata metadata, CRTObject row);

    /// <summary>
    /// Тут можно проинициализировать параметры запроса загрузки дочерних элементов.
    /// При входе в эту процедуру параметры уже содержат "parent_id=this.ID"
    /// </summary>
    /// <param name="prms"></param>
    protected virtual void doOnBeforeLoadItem(ref Params prms) {
    }

    public static String csDefaultParentIDParameterName = "parent_id";
    public void Load(Action<JSTreeItemBase> actOnItem, AjaxRequestDelegate callback) {
      if (this.OwnerTreeView == null)
        throw new Exception("Не определен атрибут OwnerTreeView в корневом элементе!");
      this._cli.AjaxMng = this.AjaxMng;
      this._cli.BioCode = this.BioCode;
      var prms = new Params();
      if (!this.Equals(this.RootItem)) {
        prms.Add(csDefaultParentIDParameterName, this.ID);
      }

      this.doOnBeforeLoadItem(ref prms);

      var args = new BeforeLoadItemChildrenEventArgs {
        Params = prms
      };
      this.OwnerTreeView.processBeforeLoadItemChildren(this, args);
      if (args.Cancel)
        return;

      this._cli.Load(args.Params, (s, a) => {
        if (a.Response.Success) {
          var rsp = a.Response as JsonStoreResponse;
          if (rsp != null) {
            this.Items.Clear();
            foreach (var r in ((JsonStoreClient) s).DS) {
              var item = this.doOnCreateNewChildItem(rsp.packet.MetaData, r);
              if (actOnItem != null) actOnItem(item);
            }
            this.Loaded = true;
          }
        }
        if (callback != null) callback(s, a);
      });
    }

  }

}
