namespace Bio.Framework.Client.SL {
  using System;
  using System.Collections;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.ComponentModel;
  using System.Windows;
  using Bio.Helpers.Common.Types;

  public class LoadPluginCompletedEventArgs : AjaxResponseEventArgs {
    public IPlugin Plugin { get; set; }
  }

  public class AjaxStateChangedEventArgs : EventArgs {
    public ConnectionState ConnectionState { get; set; }
    public RequestState RequestState { get; set; }
  }

  /// <summary>
  /// ��������� ����� �������� ��������
  /// 
  /// </summary>
  public interface IEnvironment: IEnumerable {

    /// <summary>
    /// ������������� ��������� �������
    /// </summary>
    /// <param name="pProducerCompany"></param>
    /// <param name="pAppName"></param>
    /// <param name="pAppTitle"></param>
    /// <param name="pAppVersion"></param>
    void setAppAttrs(String pProducerCompany, String pAppName, String pAppTitle, String pAppVersion);

    /// <summary>
    /// �������� �������� - �������������
    /// </summary>
    String ProducerCompany { get; }
    /// <summary>
    /// �������� ����������
    /// </summary>
    String AppName { get; }
    /// <summary>
    /// ��������� ����������
    /// </summary>
    String AppTitle { get; }
    /// <summary>
    /// ������ ����������
    /// </summary>
    String AppVersion { get; }
    /// <summary>
    /// URL ��� ������������� �������� � �������
    /// </summary>
    String ServerUrl { get; }

    event EventHandler<AjaxStateChangedEventArgs> OnStateChanged;

    /*
    /// <summary>
    /// ������������� Ajax
    /// </summary>
    /// <param name="pServerUrl"></param>
    /// <param name="pRequestTimeout"></param>
    void InitAjax(String pServerUrl, int pRequestTimeout);
    */

    void LoadRootPlugin(UIElement container, String rootPluginName);
    void LoadPlugin(IPlugin ownerPlugin, String pluginName, String pluginID, Action<LoadPluginCompletedEventArgs> act);    

    /// <summary>
    /// ������ �� ������� � �������, ������������������� � ����� ��������
    /// ������ ������� ������������ �������� ����������� � ����� ��������.
    /// </summary>
    /// <param name="pIndex"></param>
    /// <returns></returns>
    IPlugin this[int pIndex] { get; }
    /// <summary>
    /// ���������� ������������������ ��������
    /// </summary>
    Int32 PlgCount { get; }

    /// <summary>
    /// ���������� ��� ������� ��� ��������� User-Agent � ������� � �������
    /// </summary>
    String UserAgentName { get; }
    /// <summary>
    /// ��������� ������� + ������
    /// </summary>
    String UserAgentTitleAndVer { get; }

    /// <summary>
    /// AjaxMng
    /// </summary>
    IAjaxMng AjaxMng { get; }

    /// <summary>
    /// ������ �� ������� ����
    /// </summary>
    UserControl StartUpControl { get; }
    IPluginRoot PluginRoot { get; }

    ///// <summary>
    ///// ����� �����������, ��������� ��������� �������� CGrid
    ///// </summary>
    ///// <param name="pOwnerPlg"></param>
    ///// <param name="pGrid"></param>
    ///// <param name="pExportTitle"></param>
    //void ExportGrid(IPlugin pOwnerPlg, CGrid pGrid, String pExportTitle);

    ConnectionState ConnectionState { get; }
    RequestState RequestState { get; }

    /// <summary>
    /// �������� Ping-������ �� ������. ���� ������ �� ������� �� ����������,
    /// ����� ����� ����������� ���� � �������.
    /// </summary>
    void Connect(AjaxRequestDelegate callback);

    /// <summary>
    /// �������� �� ������ ������ � ���������� ������
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="silent"></param>
    void Disconnect(AjaxRequestDelegate callback, Boolean silent);

    /// <summary>
    /// �������� �� ������ ������ � ���������� ������ : Disconnect(null, true, true);
    /// </summary>
    void Disconnect();

    /// <summary>
    /// IsConnected
    /// </summary>
    Boolean IsConnected { get; }

    /// <summary>
    /// ������������, ����������
    /// </summary>
    void Reconnect();

    IConfigRoot ConfigRoot { get; }

    String LastSuccessPwd { get; set; }

    void IncreaseISQuota(Action<Boolean> callback);

    void StoreUserObject(String objName, Object obj);
    T RestoreUserObject<T>(String objName, T defObj);

  }
}
