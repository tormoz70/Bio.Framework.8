namespace Bio.Framework.Server {

  using System;
  using System.IO;
  using System.Web;
  using System.Collections.Generic;
  using System.Xml;
  using System.Text;
  using System.Reflection;

  using Bio.Helpers.Common;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  
  /// <summary>
  /// ������� ����� ��� ��� ���������� �������� ���������
  ///   - ������������� � ������� ���������� ����������� ���������,
  ///   ����� �������� � ������� ����� ���������, ���� ������� ����� - ���������� ���������,
  ///   ������� ����������� �� ������� � ���������������� ��� � ����� /ini/regmsgs.xml
  /// </summary>
  public class CBioHandlerFactory {

    public static ABioHandler CreateBioHandler(HttpContext pContext, AjaxRequest request) {
      String vHandlerImplTypeName = null;
      var rqt = request as BioRequestTyped;
      if (rqt != null)
        vHandlerImplTypeName = enumHelper.GetAttributeByValue<RequestTypeMappingAttribute>(rqt.RequestType).Mapping;
      if (String.IsNullOrEmpty(vHandlerImplTypeName))
        throw new EBioException("�� �������� ���������� ��� ���������!", null);
      var t = Type.GetType(vHandlerImplTypeName);
      if(t == null)
        throw new EBioException("�� ������ ��� ���������: " + vHandlerImplTypeName, null);
      var parTypes = new Type[2];
      parTypes[0] = typeof(HttpContext);
      parTypes[1] = typeof(AjaxRequest);
      var parVals = new Object[] { pContext, request };
      var ci = t.GetConstructor(parTypes);
      if (ci != null) {
        var obj = (ABioHandler)ci.Invoke(parVals);
        return obj;
      }
      return null;
    }
  }
}
