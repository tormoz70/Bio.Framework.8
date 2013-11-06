using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Bio.Helpers.Common;
using Bio.Helpers.Common.Types;
using Bio.Framework.Packets;
using System.IO.IsolatedStorage;
using Bio.Helpers.Controls.SL;

namespace Bio.Framework.Client.SL {

  public class BioEnvironment: IEnvironment {

    private readonly Dictionary<String, IPlugin> _plugins;

    private static IEnvironment _instance;

    private BioEnvironment(UserControl startUpControl) {
      if (!Utl.DesignTime) {
        this.StartUpControl = startUpControl;
        this._plugins = new Dictionary<String, IPlugin>();
      }
    }

    public static void Init(UserControl startUpControl) {
      _instance = new BioEnvironment(startUpControl);
    }

    public static IEnvironment Instance {
      get {
        if (_instance == null)
          throw new Exception(String.Format("Instance of {0} not inited!", typeof(BioEnvironment).Name));
        return _instance;
      }
    }

    private void _regPlugin(String pluginID, IPlugin plg, Boolean throwError) {
      if (this._plugins.ContainsKey(pluginID)) {
        if (throwError)
          throw new Exception(String.Format("Модуль с ID:\"{0}\" в системе уже зарегестрирован!", pluginID));
        return;
      }
      this._plugins.Add(pluginID, plg);
    }

    public event EventHandler<AjaxStateChangedEventArgs> OnStateChanged;
    //public event EventHandler OnRootConfigStore;

    public UserControl StartUpControl { get; private set; }

    public String ServerUrl {
      get {
        return "srv.aspx";
      }
    }

    private String _rootPluginName;
    public void LoadRootPlugin(UIElement container, String rootPluginName) {
      this._rootPluginName = rootPluginName;
      this.LoadPlugin(null, this._rootPluginName, a => {
        if (a.Plugin != null) {
          this.PluginRoot = (IPluginRoot)a.Plugin;
          this.PluginRoot.Show(container);
          this.PluginRoot.Cfg.OnStore += this._doOnRootConfigStore;
          if (this.PluginRoot.Cfg.BeOnline)
            this._runBeOnlineTicker();
        }
      });
    }

    private Boolean _breakBeOnlineTicker;
    private void _doBeOnlineTick() {
      if (this._breakBeOnlineTicker)
        return;
      delayedStarter.Do(60 * 1000, () => {
        this.AjaxMng.Request(new BioRequest {
          RequestType = RequestType.doPing,
          Prms = null,
          Callback = (sndr, args) => {
            var rsp = args.Response as BioResponse;
            if ((rsp != null) && rsp.Success && !this._breakBeOnlineTicker)
              this._doBeOnlineTick();
          }
        });
      });
    }

    private void _runBeOnlineTicker() {
      this._breakBeOnlineTicker = false;
      this._doBeOnlineTick();
    }
    private void _stopBeOnlineTicker() {
      this._breakBeOnlineTicker = true;
    }

    private void _doOnRootConfigStore(Object sender, EventArgs e) {
      //var eve = this.OnRootConfigStore;
      //if (eve != null) eve(sender, e);
      if (((IConfigRoot) sender).BeOnline)
        this._runBeOnlineTicker();
      else
        this._stopBeOnlineTicker();
    }

    private const String CS_MODULE_VERSION_KEY_PREFIX = "moduleVersion";
    private String _genModuleVersionKey(String moduleName) {
      return String.Format(CS_MODULE_VERSION_KEY_PREFIX + "[{0}]", moduleName);
    }

    private const Int64 CI_QUOTA = 20 * 1000 * 1024;

    public void IncreaseISQuota(Action<Boolean> callback) {
      var curQuota = Utl.GetISQuota();
      if (curQuota < CI_QUOTA) {
        var rqf = new FrmPromptIncreaseQuotaIS();
        rqf.ShowM(CI_QUOTA, callback);
      } else {
        if (callback != null)
          callback(true);
      }
    }


    private const String CS_IS_PLUGIN_FOLDER_NAME = "plugins";
    private void _saveModuleToLoc(Stream stream, String moduleName, String version) {
      if (stream != null) {
        using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
          if (!store.DirectoryExists(CS_IS_PLUGIN_FOLDER_NAME)) {
            store.CreateDirectory(CS_IS_PLUGIN_FOLDER_NAME);
          }

          var fileName = String.Format(@"{0}\{1}", CS_IS_PLUGIN_FOLDER_NAME, moduleName);
          if(store.FileExists(fileName))
            store.DeleteFile(fileName);
          using (var fs = store.OpenFile(fileName, FileMode.CreateNew)) {
            stream.Position = 0;
            stream.CopyTo(fs);
            fs.Flush();
            fs.Close();
          }
        }
        this.StoreUserObject(this._genModuleVersionKey(moduleName), version);
      }
    }

    private void _clearCachedModules() {
      using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
        if (store.DirectoryExists(CS_IS_PLUGIN_FOLDER_NAME)) {
          var files = store.GetFileNames(CS_IS_PLUGIN_FOLDER_NAME + "\\*.*");
          foreach (var f in files)
            store.DeleteFile(CS_IS_PLUGIN_FOLDER_NAME + "\\" + f);
          store.DeleteDirectory(CS_IS_PLUGIN_FOLDER_NAME);
        }
      }
      var userSettings = IsolatedStorageSettings.ApplicationSettings;
      var moduleVersionKeys = new List<String>();
      foreach(var c in userSettings){
        if (c.Key.StartsWith(CS_MODULE_VERSION_KEY_PREFIX))
          moduleVersionKeys.Add(c.Key);
      }
      foreach (var c in moduleVersionKeys)
        userSettings.Remove(c);
      userSettings.Save();
    }

    private String _getLocAssemblyVersion(String moduleName) {
      return this.RestoreUserObject<String>(this._genModuleVersionKey(moduleName), null);
    }

    private void _loadModuleFromRmt(String moduleName, Action<AjaxResponseEventArgs> callback) {
      var curClientVersion = "cliver="+Utl.GetCurrentClientVersion();
      ajaxUTL.GetFileFromSrv(new BioRequest {
        URL = this.ServerUrl,
        RequestType = RequestType.asmbVer,
        BioParams = new Params(new Param { Name = "moduleName", Value = moduleName },
                                new Param { Name = "getModule", Value = "1" }),
        Callback = (s, a) => {
          callback(a);
        }
      }, curClientVersion);
    }

    private void _loadModuleFromStream(Stream stream, Action<Assembly> callback) {
      Utl.UiThreadInvoke(() => {
        if (stream != null) {
          stream.Position = 0;
          var assemblyPart = new AssemblyPart();
          using (var buf = new MemoryStream()) {
            stream.CopyTo(buf);
            var assembly = assemblyPart.Load(buf);
            if (callback != null)
              callback(assembly);
          }
        }
      });
    }

    private void _loadModuleFromLoc(String moduleName, Action<Assembly> callback) {
      using (var store = IsolatedStorageFile.GetUserStoreForApplication()) {
        var fileName = String.Format(@"{0}\{1}", CS_IS_PLUGIN_FOLDER_NAME, moduleName);
        if (store.FileExists(fileName)) {
          using (var isoStream = store.OpenFile(fileName, FileMode.Open)) {
            this._loadModuleFromStream(isoStream, callback);
          }
        }
      }
    }

    private String _getRmtVer(AjaxResponseEventArgs a) {
      var rsp = a.Response as BioResponse;
      if (rsp != null)
        return Params.FindParamValue(rsp.BioParams, "moduleVersion") as String;
      return null;
    }

    private void _loadRmtAssemblyVer(String moduleName, Action<AjaxResponseEventArgs> callback) {
      ajaxUTL.GetDataFromSrv(new BioRequest {
        URL = this.ServerUrl,
        RequestType = RequestType.asmbVer,
        BioParams = new Params(new Param { Name = "moduleName", Value = moduleName }),
        Callback = (s, a) => {
          callback(a);
        }
      });
    }

    private void _doOnLoadRmtAssembly(String moduleName, String moduleVersion, AjaxResponseEventArgs a, Action<AjaxResponseEventArgs, Assembly> callback) {
      if (a.Response.Success) {
        // Сборка удачно загружена с сервера
        // загружаем ее
        this._loadModuleFromStream(a.Stream, assemly => {
          // Сохраняем ее в локальное хранилище
          var locVer = moduleVersion;
          this._saveModuleToLoc(a.Stream, moduleName, locVer);
          // Возвращаем
          if (callback != null)
            callback(a, assemly);
        });
      } else {
        msgBx.ShowError(EBioException.CreateIfNotEBio(a.Response.Ex), "Ошибка при загрузки модуля с сервера", () => {
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
// ReSharper disable UnusedMember.Local
    private void _loadModuleUsingLocalStore0(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback) {
// ReSharper restore UnusedMember.Local
      // Загружаем версию модуля из локального хранилища
      var locVer = this._getLocAssemblyVersion(moduleName);
      if (!String.IsNullOrEmpty(locVer)) {
        // Загружаем с сервера описание весии этого модуля
        this._loadRmtAssemblyVer(moduleName, a => {
          if (a.Response.Success) {
            var rmtVer = this._getRmtVer(a);
            // Сравниваем версии локального модуля и модуля на сервере
            if (String.Equals(locVer, rmtVer)) {
              // Версии равны возвращаем локальную сборку
                this._loadModuleFromLoc(moduleName, assemly => {
                  if (callback != null) 
                      callback(a, assemly);
                });
            } else {

              // Версии не равны загружаем сборку с сервера
              this._loadModuleFromRmt(moduleName, a1 => {
                this._doOnLoadRmtAssembly(moduleName, null, a1, callback);
              });
            }
          } else {
            msgBx.ShowError(a.Response.Ex, "Ошибка при получении атрибутов модуля с сервера", () => {
              if (callback != null)
                callback(a, null);
            });
          }
        });
      } else {
        // Локальная версия модуля отсутствует
        this._loadModuleFromRmt(moduleName, a1 => {
          this._doOnLoadRmtAssembly(moduleName, null, a1, callback);
        });
      }
    }

    private const String CS_MODULE_EXTENTION = ".dll";

    private Boolean _moduleNameIsRootModuleName(String moduleName) {
      return ((this._rootPluginName + CS_MODULE_EXTENTION).Equals(moduleName, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Загрузка модулей с использованием локального хранилища в качестве буфера
    /// При обновлении буфера исполюзуется версия основного модуля приложения
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="callback"></param>
    private void _loadModuleUsingLocalStore1(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback) {
      // Определяем текущую версию основного модуля
      var curMainVersion = Utl.GetCurrentClientVersion();
      // Загружаем версию модуля из локального хранилища
      var locVer = this._getLocAssemblyVersion(moduleName);
      var doLoadRemote = false;
      if (!String.IsNullOrEmpty(locVer)) {
        // Сравниваем версии локального модуля и модуля на сервере
        if (String.Equals(locVer, curMainVersion)) {
          // Версии равны возвращаем локальную сборку
          this._loadModuleFromLoc(moduleName, assemly => {
            if (assemly != null) {
              if (callback != null)
                callback(new AjaxResponseEventArgs {Response = new AjaxResponse {Success = true}}, assemly);
            } else
              doLoadRemote = true;
          });
        } else 
          doLoadRemote = true;
        
      } else 
          doLoadRemote = true;
      
      if(doLoadRemote){
        // Локальная версия модуля отсутствует
        if (this._moduleNameIsRootModuleName(moduleName))
          this._clearCachedModules(); // - очищаем cache
        // загружаем сборку с сервера
        this._loadModuleFromRmt(moduleName, a1 => {
          this._doOnLoadRmtAssembly(moduleName, curMainVersion, a1, callback);
        });
      }
    }

// ReSharper disable UnusedMember.Local
    private void _loadModuleDirect(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback) {
// ReSharper restore UnusedMember.Local
      this._loadModuleFromRmt(moduleName, a1 => {
        this._loadModuleFromStream(a1.Stream, assemly => {
          if (callback != null)
            callback(a1, assemly);
        });
      });
    }

    private delegate void LoadModuleDelegate(String moduleName, Action<AjaxResponseEventArgs, Assembly> callback);
    public void LoadPlugin(IPlugin ownerPlugin, String pluginName, String pluginID, Action<LoadPluginCompletedEventArgs> act) {
      IPluginRootView rootPlgView = null;
      if (this.PluginRoot != null)
        rootPlgView = this.PluginRoot.View as IPluginRootView;
      if (rootPlgView != null)
        rootPlgView.showBusyIndicator("загрузка модуля...");
      try {
        pluginID = pluginID ?? pluginName;
        IPlugin plgExists;
        // Проверяем а не загружен ли плагин уже...
        if (this._plugins.TryGetValue(pluginID, out plgExists)) {
          // - загружен
          if (act != null)
            act(new LoadPluginCompletedEventArgs { Plugin = plgExists });
          if (rootPlgView != null)
            rootPlgView.hideBusyIndicator();
          return;
        }
        // - не загружен
        var moduleName = pluginName + CS_MODULE_EXTENTION;
        // Запускаем процедуру загрузки плагина
        LoadModuleDelegate loadModuleDelegate = this._loadModuleUsingLocalStore1;
        loadModuleDelegate(moduleName, (e, assembly) => {
          // Сборка загружена. Создаем экземпляр плагина.
          try {
            if (assembly != null) {
              var plgType = _findType(assembly.GetTypes(), typeof(IPlugin));
              var ci = plgType.GetConstructor(new[] { typeof(IPlugin), typeof(String), typeof(String), typeof(String) });
              if (ci != null) {
                var plg = ci.Invoke(new Object[] { ownerPlugin, moduleName, pluginName, pluginID }) as IPlugin ??
                          new PluginNotFoundDummy(ownerPlugin, moduleName, pluginName, pluginID);
                // Регим его в системе
                this._regPlugin(pluginID, plg, false);
                // Возвращаем ссылку через callback
                if (act != null)
                  act(new LoadPluginCompletedEventArgs { Plugin = plg });
              }
            } else {
              var v_msg = String.Format("Ошибка при загрузке модуля {0} с сервера.", moduleName);
              var v_err = new EBioException(v_msg, e.Response.Ex);
              msgBx.ShowError(v_err, "Загрузка модуля", null);
            }
          } finally {
            if (rootPlgView != null)
              rootPlgView.hideBusyIndicator();
          }
        });
      } catch (Exception) {
        if (rootPlgView != null)
          rootPlgView.hideBusyIndicator();
        throw;
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
        var keys = new String[this._plugins.Keys.Count];
        this._plugins.Keys.CopyTo(keys, 0);
        return this._plugins[keys[index]]; 
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
    private void _doOnStateChanged(Object sender, AjaxStateChangedEventArgs args) {
      this._connState = args.ConnectionState;
      this._reqstState = args.RequestState;

      var events = this.OnStateChanged;
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

    private IAjaxMng _ajaxMng;
    public IAjaxMng AjaxMng {
      get {
        if (this._ajaxMng == null) {
          this._ajaxMng = new AjaxMng();
          this._ajaxMng.OnStateChanged += this._doOnStateChanged;
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
      this.AjaxMng.Request(new BioRequest {
        RequestType = RequestType.doPing,
        Prms = null,
        Callback = (sndr, args) => {
          var rsp = args.Response as BioResponse;
          if ((rsp != null) && (rsp.GCfg != null))
            BioGlobal.Debug = rsp.GCfg.Debug;
          if (this.AjaxMng.CurUsr != null) {
            BioGlobal.CurUsrIsDebugger = this.AjaxMng.CurUsr.IsBioRoot() || this.AjaxMng.CurUsr.IsDebugger();
            BioGlobal.CurSessionIsLoggedOn = true;
          }
          if (callback != null) callback(this, args);
        }
      });
    }

    public void Disconnect(AjaxRequestDelegate callback, Boolean silent) {
      this.AjaxMng.Request(new BioRequest {
        RequestType = RequestType.doLogout,
        Silent = silent,
        Prms = null,
        Callback = (sndr, args) => {
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
        return this.PluginRoot.Cfg;
      }
    }

    public void StoreUserObject(String objName, Object obj) {
      Utl.StoreUserObjectStrg(objName, obj);
    }
    public T RestoreUserObject<T>(String objName, T defObj) {
      return Utl.RestoreUserObjectStrg(objName, default(T));
    }

    #endregion

    #region IEnumerable Members

    public System.Collections.IEnumerator GetEnumerator() {
      throw new NotImplementedException();
    }

    #endregion

    private static Type _findType(Type[] types, Type type) {
      if ((types != null) && (type != null)) {
        foreach (var oType in types) {
          if (type.IsClass && (oType == type || oType.IsSubclassOf(type))) {
            return oType;
          }
          if (type.IsInterface) {
            if ((oType == type || oType.IsSubclassOf(type)))
              return oType;
            foreach (var ifc in oType.GetInterfaces())
              if (ifc == type || ifc.IsSubclassOf(type))
                return oType;
          }
        }
        return null;
      }
      return null;
    }

    public String LastSuccessPwd { get; set; }
  }
}
