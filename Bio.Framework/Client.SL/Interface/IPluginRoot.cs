namespace Bio.Framework.Client.SL {
  using System;

  /// <summary>
  /// ��������� root-��������
  /// </summary>
  public interface IPluginRoot: IPlugin {
    IConfigRoot Cfg { get; }
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

  }

}
