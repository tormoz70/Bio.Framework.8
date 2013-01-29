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
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Xml.Linq;
using System.IO;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;
using System.IO.IsolatedStorage;
using Bio.Helpers.Controls.SL;

namespace Bio.Framework.Client.SL {

  public class CEnvironment: IEnvironment {
    private const String csRootContentPropName = "RootContent";

    private Dictionary<String, AppDomain> _domains = null;
    private Dictionary<String, IPlugin> _plugins = null;
    
    private void _regPlugin(String pluginID, IPlugin plg, Boolean throwError) {
      if (this._plugins.ContainsKey(pluginID)) {
        if (throwError)
          throw new Exception(String.Format("Модуль с ID:\"{0}\" в системе уже зарегестрирован!", pluginID));
        else
          return;
      }
      this._plugins.Add(pluginID, plg);
    }

    public event EventHandler<AjaxStateChangedEventArgs> OnStateChanged;

    public UserControl StartUpControl { get; private set; }

    public String ServerUrl {
      get {
        return "srv.aspx";
      }
    }

    private String _rootPluginName = null;
    public void LoadRootPlugin(UIElement container, String rootPluginName) {
      this._rootPluginName = rootPluginName;
      this.LoadPlugin(null, this._rootPluginName, (a) => {
        if (a.Plugin != null) {
          this.PluginRoot = a.Plugin as IPluginRoot;
          a.Plugin.Show(container);
        }
      });
    }

    public CEnvironment(UserControl startUpControl) {
      if (!Utl.DesignTime) {
        this.StartUpControl = startUpControl;
        this._domains = new Dictionary<String, AppDomain>();
        this._plugins = new Dictionary<String, IPlugin>();
      }
    }

    private const String csModuleVersionKeyPrefix = "moduleVersion";
    private String _genModuleVersionKey(String moduleName) {
      return String.Format(csModuleVersionKeyPrefix + "[{0}]", moduleName);
    }

    private const Int64 ciQuota = 10000 * 1024;
    public void IncreaseQuota() {
      //using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
      //  if (store.Quota < ciQuota)
      //    store.IncreaseQuotaTo(ciQuota);
      //}
    }
    private const String csISPluginFolderName = "plugins";
    private void _saveModuleToLoc(Stream stream, String moduleName, String version) {
      if (stream != null) {
        try {
          using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
            if (!store.DirectoryExists(csISPluginFolderName)) {
              store.CreateDirectory(csISPluginFolderName);
            }

            String v_fileName = String.Format(@"{0}\{1}", csISPluginFolderName, moduleName);
            if(store.FileExists(v_fileName))
              store.DeleteFile(v_fileName);
            using (var fs = store.OpenFile(v_fileName, FileMode.CreateNew)) {
              stream.Position = 0;
              stream.CopyTo(fs);
              fs.Flush();
              fs.Close();
            }
          }
          this.StoreUserObject(this._genModuleVersionKey(moduleName), version);
        } finally {
        }
      }
    }

    private void _clearCachedModules() {
      using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
        if (store.DirectoryExists(csISPluginFolderName)) {
          String[] v_files = store.GetFileNames(csISPluginFolderName + "\\*.*");
          foreach (var f in v_files)
            store.DeleteFile(csISPluginFolderName + "\\" + f);
          store.DeleteDirectory(csISPluginFolderName);
        }
      }
      var userSettings = IsolatedStorageSettings.ApplicationSettings;
      List<String> v_moduleVersionKeys = new List<String>();
      foreach(var c in userSettings){
        if (c.Key.StartsWith(csModuleVersionKeyPrefix))
          v_moduleVersionKeys.Add(c.Key);
      }
      foreach (var c in v_moduleVersionKeys)
        userSettings.Remove(c);
      userSettings.Save();
    }

    private String _getLocAssemblyVersion(String moduleName) {
      return this.RestoreUserObject<String>(this._genModuleVersionKey(moduleName), null);
    }

    private void _loadModuleFromRmt(String moduleName, Action<AjaxResponseEventArgs> callback) {
      var v_curClientVersion = "cliver="+Utl.GetCurrentClientVersion();
      ajaxUTL.getFileFromSrv(new CBioRequest {
        url = this.ServerUrl,
        requestType = RequestType.asmbVer,
        bioParams = new CParams(new CParam { Name = "moduleName", Value = moduleName },
                                new CParam { Name = "getModule", Value = "1" }),
        callback = (s, a) => {
          callback(a);
        }
      }, v_curClientVersion);
    }

    private Assembly _loadModuleFromStream(Stream stream) {
      Assembly assembly = null;
      if (stream != null) {
        stream.Position = 0;
        AssemblyPart assemblyPart = new AssemblyPart();
        using (var buf = new MemoryStream()) {
          stream.CopyTo(buf);
          assembly = assemblyPart.Load(buf);
        }
      }
      return assembly;
    }

    private Assembly _loadModuleFromLoc(String moduleName) {
      Assembly assembly = null;
      using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
        String v_fileName = String.Format(@"{0}\{1}", csISPluginFolderName, moduleName);
        if (store.FileExists(v_fileName)) {
          using (var isoStream = store.OpenFile(v_fileName, FileMode.Open)) {
            assembly = this._loadModuleFromStream(isoStream);
          }
        }
      }
      return assembly;
    }

    private String _getRmtVer(AjaxResponseEventArgs a) {
      CBioResponse rsp = a.response as CBioResponse;
      if (rsp != null)
        return CParams.FindParamValue(rsp.bioParams, "moduleVersion") as String;
      else
        return null;
    }

    private void _loadRmtAssemblyVer(String moduleName, Action<AjaxResponseEventArgs> callback) {
      ajaxUTL.getDataFromSrv(new CBioRequest {
        url = this.ServerUrl,
        requestType = RequestType.asmbVer,
        bioParams = new CParams(new CParam { Name = "moduleName", Value = moduleName }),
        callback = (s, a) => {
          callback(a);
        }
      });
    }

    private void _doOnLoadRmtAssembly(String moduleName, String moduleVersion, AjaxResponseEventArgs a, Action<AjaxResponseEventArgs, Assembly> callback) {
      if (a.response.success) {
        // Сборка удачно загружена с сервера
        // загружаем ее
        Assembly assemly = this._loadModuleFromStream(a.stream);
        // Сохраняем ее в локальное хранилище
        String v_loc_ver = moduleVersion;
        this._saveModuleToLoc(a.stream, moduleName, v_loc_ver);
        // Возвращаем
        if (callback != null)
          callback(a, assemly);
      } else {
        msgBx.showError(EBioException.CreateIfNotEBio(a.response.ex), "Ошибка при загрузки модуля с сервера", () => {
          if (callback != null)
            callback(a, null);
        });
      }
    }

    /// <summary>
    /// Загрузка модулей с использованием локального хранилища в качестве буфера
    /// При обновлении буфера исполюзуется сравнение версий серверной и локальной сборки
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="callback"></param>
    private void _loadModuleUsingLocalStore0(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback) {
      // Загружаем версию модуля из локального хранилища
      String v_loc_ver = this._getLocAssemblyVersion(moduleName);
      if (!String.IsNullOrEmpty(v_loc_ver)) {
        // Загружаем с сервера описание весии этого модуля
        this._loadRmtAssemblyVer(moduleName, (a) => {
          if (a.response.success) {
            String v_rmt_ver = this._getRmtVer(a);
            // Сравниваем версии локального модуля и модуля на сервере
            if (String.Equals(v_loc_ver, v_rmt_ver)) {
              // Версии равны возвращаем локальную сборку
              if (callback != null) {
                Assembly assemly = this._loadModuleFromLoc(moduleName);
                callback(a, assemly);
              }
            } else {

              // Версии не равны загружаем сборку с сервера
              this._loadModuleFromRmt(moduleName, (a1) => {
                this._doOnLoadRmtAssembly(moduleName, null, a1, callback);
              });
            }
          } else {
            msgBx.showError(a.response.ex, "Ошибка при получении атрибутов модуля с сервера", () => {
              if (callback != null)
                callback(a, null);
            });
          }
        });
      } else {
        // Локальная версия модуля отсутствует
        this._loadModuleFromRmt(moduleName, (a1) => {
          this._doOnLoadRmtAssembly(moduleName, null, a1, callback);
        });
      }
    }

    private const String csModuleExtention = ".dll";

    private Boolean _moduleNameIsRootModuleName(String moduleName) {
      return ((this._rootPluginName + csModuleExtention).Equals(moduleName, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Загрузка модулей с использованием локального хранилища в качестве буфера
    /// При обновлении буфера исполюзуется версия основного модуля приложения
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="callback"></param>
    private void _loadModuleUsingLocalStore1(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback) {
      // Определяем текущую версию основного модуля
      var v_cur_mainVersion = Utl.GetCurrentClientVersion();
      // Загружаем версию модуля из локального хранилища
      var v_loc_ver = this._getLocAssemblyVersion(moduleName);
      var v_doLoadRemote = false;
      if (!String.IsNullOrEmpty(v_loc_ver)) {
        // Сравниваем версии локального модуля и модуля на сервере
        if (String.Equals(v_loc_ver, v_cur_mainVersion)) {
          // Версии равны возвращаем локальную сборку
          if (callback != null) {
            Assembly assemly = this._loadModuleFromLoc(moduleName);
            if (assemly != null)
              callback(new AjaxResponseEventArgs { response = new CAjaxResponse { success = true } }, assemly);
            else
              v_doLoadRemote = true;
          }
        } else 
          v_doLoadRemote = true;
        
      } else 
          v_doLoadRemote = true;
      
      if(v_doLoadRemote){
        // Локальная версия модуля отсутствует
        if (this._moduleNameIsRootModuleName(moduleName))
          this._clearCachedModules(); // - очищаем cache
        // загружаем сборку с сервера
        this._loadModuleFromRmt(moduleName, (a1) => {
          this._doOnLoadRmtAssembly(moduleName, v_cur_mainVersion, a1, callback);
        });
      }
    }

    private void _loadModuleDirect(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback) {
      this._loadModuleFromRmt(moduleName, (a1) => {
        Assembly assemly = this._loadModuleFromStream(a1.stream);
        if (callback != null)
          callback(a1, assemly);
      });
    }

    private delegate void LoadModuleDelegate(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback);
    public void LoadPlugin(IPlugin ownerPlugin, String pluginName, String pluginID, Action<LoadPluginCompletedEventArgs> act) {
      IPluginRootView v_rootPlgView = null;
      if (this.PluginRoot != null)
        v_rootPlgView = this.PluginRoot.View as IPluginRootView;
      if (v_rootPlgView != null)
        v_rootPlgView.showBusyIndicator("загрузка модуля...");
      try {
        pluginID = pluginID ?? pluginName;
        IPlugin vPlgExists = null;
        // Проверяем а не загружен ли плагин уже...
        if (this._plugins.TryGetValue(pluginID, out vPlgExists)) {
          // - загружен
          if (act != null)
            act(new LoadPluginCompletedEventArgs { Plugin = vPlgExists });
          if (v_rootPlgView != null)
            v_rootPlgView.hideBusyIndicator();
          return;
        }
        // - не загружен
        String moduleName = pluginName + csModuleExtention;
        // Запускаем процедуру загрузки плагина
        LoadModuleDelegate v_loadModuleDelegate = null;
        v_loadModuleDelegate = this._loadModuleUsingLocalStore1;
        v_loadModuleDelegate(moduleName, (e, assembly) => {
          // Сборка загружена. Создаем экземпляр плагина.
          try {
            if (assembly != null) {
              Type plgType = findType(assembly.GetTypes(), typeof(IPlugin));
              ConstructorInfo ci = plgType.GetConstructor(new Type[] { typeof(IPlugin), typeof(IEnvironment), typeof(String), typeof(String), typeof(String) });
              IPlugin plg = ci.Invoke(new Object[] { ownerPlugin, this, moduleName, pluginName, pluginID }) as IPlugin;
              if (plg == null)
                plg = new PluginNotFoundDummy(ownerPlugin, this, moduleName, pluginName, pluginID);
              // Регим его в системе
              this._regPlugin(pluginID, plg, false);
              // Возвращаем ссылку через callback
              if (act != null)
                act(new LoadPluginCompletedEventArgs { Plugin = plg });
            } else {
              var v_msg = String.Format("Ошибка при загрузке модуля {0} с сервера.", moduleName);
              var v_err = new EBioException(v_msg, e.response.ex);
              msgBx.showError(v_err, "Загрузка модуля", null);
            }
          } finally {
            if (v_rootPlgView != null)
              v_rootPlgView.hideBusyIndicator();
          }
        });
      } catch (Exception ex) {
        if (v_rootPlgView != null)
          v_rootPlgView.hideBusyIndicator();
        throw ex;
      }
    }
    public void LoadPlugin(IPlugin ownerPlugin, String pluginName, Action<LoadPluginCompletedEventArgs> act) {
      this.LoadPlugin(ownerPlugin, pluginName, null, act);
    }
    
    #region IEnvironment Members

    public void setAppAttrs(string pProducerCompany, string pAppName, string pAppTitle, string pAppVersion) {
      throw new NotImplementedException();
    }

    public string ProducerCompany {
      get { return this.PluginRoot.ProducerCompany; }
    }

    public string AppName {
      get { return this.PluginRoot.AppName; }
    }

    public string AppTitle {
      get { return this.PluginRoot.AppTitle; }
    }

    public String AppVersion {
      get { return this.PluginRoot.AppVersion; }
    }

    public IPlugin this[int index] {
      get {
        String[] v_keys = new String[this._plugins.Keys.Count];
        this._plugins.Keys.CopyTo(v_keys, 0);
        return this._plugins[v_keys[index]]; 
      }
    }

    public int PlgCount {
      get { return this._plugins.Count; }
    }

    public String UserAgentName {
      get { return this.PluginRoot.AppName; }
    }

    public String UserAgentTitleAndVer {
      get { return this.PluginRoot.AppTitle + " " + this.PluginRoot.AppVersion; }
    }

    public IPluginRoot PluginRoot { get; private set; }

    /// <summary>
    /// Вызывается каждый раз при изменении состояния соединения
    /// </summary>
    /// <param name="pConnState"></param>
    private void doOnStateChanged(Object sender, AjaxStateChangedEventArgs args) {
      this._connState = args.ConnectionState;
      this._reqstState = args.RequestState;

      EventHandler<AjaxStateChangedEventArgs> events = this.OnStateChanged;
      if (events != null) {
        if (!Utl.IsUiThread) {
          Utl.UiThreadInvoke(new Action<IEnvironment, AjaxStateChangedEventArgs>((s, a) => {
            events(s, a);
          }), this, args);
        } else
          events(this, args);
      }
    }

    private ConnectionState _connState = ConnectionState.Unconnected;
    private RequestState _reqstState = RequestState.Idle;
    /// <summary>
    /// Состояние текущего соединения
    /// </summary>
    public ConnectionState ConnectionState {
      get {
        return this._connState;
      }
    }
    /// <summary>
    /// Состояние текущего запроса
    /// </summary>
    public RequestState RequestState {
      get {
        return this._reqstState;
      }
    }

    private IAjaxMng _ajaxMng = null;
    public IAjaxMng AjaxMng {
      get {
        if (this._ajaxMng == null) {
          this._ajaxMng = new CAjaxMng(this);
          this._ajaxMng.OnStateChanged += this.doOnStateChanged;
        }
        return this._ajaxMng;
      }
    }

    public Boolean IsConnected { 
      get {
        return (this.ConnectionState == ConnectionState.Connected);
      } 
    }
    public void Connect(AjaxRequestDelegate callback) {
      this.AjaxMng.Request(new CBioRequest {
        requestType = RequestType.doPing,
        prms = null,
        callback = (sndr, args) => {
          var rsp = args.response as CBioResponse;
          if ((rsp != null) && (rsp.gCfg != null))
            BioGlobal.Debug = rsp.gCfg.Debug;
          if (this.AjaxMng.CurUsr != null) {
            BioGlobal.CurUsrIsDebugger = this.AjaxMng.CurUsr.isBioAdmin() || this.AjaxMng.CurUsr.isDebugger();
            BioGlobal.CurSessionIsLoggedOn = true;
          }
          if (callback != null) callback(this, args);
        }
      });
    }

    public void Disconnect(AjaxRequestDelegate callback, Boolean silent) {
      this.AjaxMng.Request(new CBioRequest {
        requestType = RequestType.doLogout,
        silent = silent,
        prms = null,
        callback = (sndr, args) => {
          if (callback != null) callback(this, args);
        }
      });
    }

    public void Disconnect() {
      this.Disconnect(null, true);
    }

    public void Reconnect() {
      throw new NotImplementedException();
    }


    public IConfigRoot ConfigRoot {
      get {
        return this.PluginRoot.CfgRoot;
      }
    }

    public void StoreUserObject(String objName, Object obj) {
      Utl.StoreUserObjectStrg(objName, obj);
    }
    public T RestoreUserObject<T>(String objName, T defObj) {
      return Utl.RestoreUserObjectStrg<T>(objName, defObj);
    }

    #endregion

    #region IEnumerable Members

    public System.Collections.IEnumerator GetEnumerator() {
      throw new NotImplementedException();
    }

    #endregion

    private static Type findType(Type[] types, Type type) {
      if ((types != null) && (type != null)) {
        foreach (Type oType in types) {
          if (type.IsClass && (oType.Equals(type) || oType.IsSubclassOf(type))) {
            return oType;
          } else if (type.IsInterface) {
            if ((oType.Equals(type) || oType.IsSubclassOf(type)))
              return oType;
            else
              foreach (Type ifc in oType.GetInterfaces())
                if (ifc.Equals(type) || ifc.IsSubclassOf(type))
                  return oType;
          }
        }
        return null;
      } else
        return null;
    }

    public String LastSuccessPwd { get; set; }
  }
}
