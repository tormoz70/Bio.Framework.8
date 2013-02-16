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

namespace Bio.Framework.Client.SL {

  public abstract class CJSTreeItemBase {
    protected CJsonStoreClient _cli = null;
    protected CJSTree _ownerTree { get; set; }
    protected IAjaxMng _ajaxMng { get; set; }
    public CJSTreeItemBase Parent { get; set; }
    public CTreeViewItem Container { get; internal set; }
    public Object ID { get; set; }
    public String _name = null;
    public virtual String Name { 
      get {
        return this._name;
      }
      set {
        this._name = value;
      }
    }
    public Boolean Loaded { get; internal set; }
    private String _bioCode = null;
    public String BioCode { 
      get {
        return this._bioCode ?? this.RootItem.BioCode;
      } 
      set {
        this._bioCode = value;
      } 
    }
    public ObservableCollection<CJSTreeItemBase> Items { get; set; }
    
    public CJSTreeItemBase() {
      this._cli = new CJsonStoreClient();
      this.Items = new ObservableCollection<CJSTreeItemBase>();
    }
    public CJSTreeItemBase(CJSTree ownerTree, IAjaxMng ajaxMng)
      : this() {
        this._ownerTree = ownerTree;
      this._ajaxMng = ajaxMng;
    }

    private CJSTreeItemBase _getRoot(CJSTreeItemBase item) {
      if (item.Parent == null)
        return item;
      else
        return this._getRoot(item.Parent);
    }

    public IAjaxMng AjaxMng {
      get {
        return this.RootItem._ajaxMng;
      }
    }

    public CJSTree OwnerTreeView {
      get {
        return this.RootItem._ownerTree;
      }
    }

    public CJSTreeItemBase RootItem {
      get {
        return this._getRoot(this);
      }
    }
    private Int32 _calcLevel(CJSTreeItemBase parent, Int32 level) {
      if (parent == null)
        return level;
      else
        return this._calcLevel(parent.Parent, level + 1);
    }

    public Int32 Level {
      get {
        return this._calcLevel(this.Parent, 0);
      }
    }

    private String _getPathID(CJSTreeItemBase parent, String path) {
      if (parent == null)
        return path;
      else {
        var v_path = parent.ID + (String.IsNullOrEmpty(path) ? null : "/" + path);
        return this._getPathID(parent.Parent, path);
      }
    }

    public String PathID {
      get {
        return this._getPathID(this, null);
      }
    }

    public T CreateNewChildItem<T>(Object id, String name) where T : CJSTreeItemBase, new() {
      T newItem = new T() {
        Loaded = false,
        Parent = this,
        ID = id,
        Name = name
      };
      this.Items.Add(newItem);
      return newItem;
    }



    protected abstract CJSTreeItemBase doOnCreateNewChildItem(CJsonStoreMetadata metadata, CJsonStoreRow row);

    /// <summary>
    /// Тут можно проинициализировать параметры запроса загрузки дочерних элементов.
    /// При входе в эту процедуру параметры уже содержат "parent_id=this.ID"
    /// </summary>
    /// <param name="prms"></param>
    protected virtual void doOnBeforeLoadItem(ref Params prms) {
    }

    public static String csDefaultParentIDParameterName = "parent_id";
    public void Load(Action<CJSTreeItemBase> actOnItem, AjaxRequestDelegate callback) {
      if (this.OwnerTreeView == null)
        throw new Exception("Не определен атрибут OwnerTreeView в корневом элементе!");
      this._cli.ajaxMng = this.AjaxMng;
      this._cli.bioCode = this.BioCode;
      Params prms = new Params();
      if (!this.Equals(this.RootItem)) {
        prms.Add(csDefaultParentIDParameterName, this.ID);
      }

      this.doOnBeforeLoadItem(ref prms);

      BeforeLoadItemChildrenEventArgs args = new BeforeLoadItemChildrenEventArgs {
        Params = prms
      };
      this.OwnerTreeView.processBeforeLoadItemChildren(this, args);
      if (args.Cancel)
        return;

      this._cli.Load(args.Params, (s, a) => {
        //this.OwnerTreeView.Dispatcher.BeginInvoke(() => {
        if (a.response.success) {
          CJsonStoreResponse rsp = a.response as CJsonStoreResponse;
          if (rsp != null) {
            this.Items.Clear();
            foreach (var r in rsp.packet.rows) {
              CJSTreeItemBase item = this.doOnCreateNewChildItem(rsp.packet.metaData, r);
              if (actOnItem != null) actOnItem(item);
              //this.Items.Add(item);
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
