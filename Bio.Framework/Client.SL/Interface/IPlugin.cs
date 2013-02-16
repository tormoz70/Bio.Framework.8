namespace Bio.Framework.Client.SL {
  using System;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using System.Windows.Controls;
  using System.Windows;

  /// <summary>
  /// �����, ����������� ��������� ��� ������� DataChanged.
  /// </summary>
  public class DataChangedEventArgs : EventArgs {
    /// <summary>
    /// ���������, ������������ � �������.
    /// </summary>
    public Params Params { get; private set; }
    /// <summary>
    /// ������ ��������� ������, ������������ ���������, ������������ � �������.
    /// </summary>
    /// <param name="pars">���������.</param>
    public DataChangedEventArgs(Params pars) {
      this.Params = pars;
    }

    /// <summary>
    /// ������ ���������.
    /// </summary>
    public new static DataChangedEventArgs Empty = new DataChangedEventArgs(null);
  }

  /// <summary>
  /// �����, ����������� ��������� ��� ������� DataChanging.
  /// </summary>
  public class DataChangingCancelEventArgs : EventArgs {
    private bool _cancel;
    /// <summary>
    /// ������� ������ �������.
    /// </summary>
    public bool Cancel {
      get { return this._cancel; }
      set { this._cancel = value || this._cancel; }
    }
    /// <summary>
    /// ���������, ������������ � �������.
    /// </summary>
    public Params Params { get; private set; }
    /// <summary>
    /// ������ ��������� ������, ������������ ���������, ������������ � �������.
    /// </summary>
    /// <param name="pars">���������.</param>
    /// <param name="cancel">������� ������ �������.</param>
    public DataChangingCancelEventArgs(Params pars, bool cancel) {
      this.Params = pars;
      this._cancel = cancel;
    }
  }

  /// <summary>
  /// ������� ��������� ��������
  /// ��� ������� � ������� ������������ ������ ���������
  /// </summary>
  public interface IPlugin {  
    /// <summary>
    /// ���������
    /// </summary>
    String ViewTitle { get; }
    /// <summary>
    /// �������� ������� �������
    /// </summary>
    IPluginView View { get; set; }
    /// <summary>
    /// �������� �������� ������� ������� � ����������
    /// </summary>
    /// <param name="container">��� ����� ���� Panel ��� ContentControl</param>
    void Show(UIElement container);
    /// <summary>
    /// �������� �������� ������� ������� � ���������� ����
    /// </summary>
    /// <param name="callback"></param>
    void ShowDialog(Action<Boolean?> callback);

    void Close();

    /// <summary>
    /// ��� ������
    /// </summary>
    String ModuleName { get; }

    /// <summary>
    /// ��� ������� ��������� � ������ ������
    /// </summary>
    String PluginName { get; }

    /// <summary>
    /// ID �������. ����� ��������� � ������ ������, � ����� � ����������
    /// </summary>
    String PluginID { get; }


    /// <summary>
    /// ������ �� ���������
    /// </summary>
    IPlugin Owner { get; }

    /// <summary>
    /// ������ �� �����
    /// </summary>
    IEnvironment Env { get; }

    /// <summary>
    /// ��������� �������
    /// </summary>
    Params Params { get; }

    /// <summary>
    /// ���������� ������ �����
    /// </summary>
    /// <param name="prms">����. ���������</param>
    /// <param name="force">�������� ������ �� ������ �� �� ���, �� ��������� false</param>
    void refreshData(Params prms, Boolean force);
    /// <summary>
    /// ���������� ������ �����
    /// </summary>
    /// <param name="prms">����. ���������</param>
    void refreshData(Params prms);

    /// <summary>
    /// ������� ������������, ��� ������������� ���������� ������� ������� �� ��������� ������.
    /// </summary>
    event EventHandler<DataChangedEventArgs> DataChanged;

    /// <summary>
    /// ������� ������������, ��� ������������� ���������� ������� ������� ����� ���������� ������.
    /// </summary>
    event EventHandler<DataChangingCancelEventArgs> DataChanging;

}

  /// <summary>
  /// �������������� � �������� <seealso cref="IPlugin"/> ��������� ��� �������� � �������������� �������.
  /// </summary>
  public interface IPluginEditable {
    /// <summary>
    /// ������� ����, ���� �� � ������� ������������ ������.
    /// </summary>
    Boolean IsDataChanged { get; }

    /// <summary>
    /// ���������� ��������� ������ ����� �������. � ��������.
    /// </summary>
    Boolean SaveChanges(EventHandler<CJsonStoreAfterPostEventArgs> callback);

    /// <summary>
    /// ������ ��������� ������ ����� �������.
    /// </summary>
    void CancelChanges();

  }
}
