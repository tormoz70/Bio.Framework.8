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
  /// Базовый класс для все получаемых системой сообщений
  ///   - взимодействие с системо происходит посредством сообщений,
  ///   чтобы добавить в систему новое сообщение, надо создать клвсс - обработчик сообщения,
  ///   который наследуется от данного и зарегистрировать его в файле /ini/regmsgs.xml
  /// </summary>
  public class CBioHandlerFactory {

    public static ABioHandler CreateBioHandler(HttpContext pContext, CAjaxRequest request) {
      String vHandlerImplTypeName = null;
      //String vHandlerType = null;
      CBioRequestTyped rqt = request as CBioRequestTyped;
      if (rqt != null)
        //vHandlerType = Utl.NameOfEnumValue<RequestType>(rqt.requestType, false); 
        vHandlerImplTypeName = enumHelper.GetAttributeByValue<RequestTypeMappingAttribute>(rqt.requestType).Mapping;
      //if(vHandlerType != null) {
        //dom4cs vHandlerReg = ABioHandler.getHandlersRegistry(pContext);
        //XmlNode vMsgRegNode = vHandlerReg.XmlDoc.DocumentElement.SelectSingleNode("add[@mtp='" + vHandlerType + "']");
        //if(vMsgRegNode == null)
        //  throw new EBioException("Сообщение [" + vHandlerType + "] не зарегистрировано в системе.", null);
        //if(((XmlElement)vMsgRegNode).HasAttribute("goto"))
        //  vHandlerImplTypeName = ((XmlElement)vMsgRegNode).GetAttribute("goto");

      //}
      if (String.IsNullOrEmpty(vHandlerImplTypeName))
        throw new EBioException("Не возможно определить тип сообщения!", null);
      Type t = Type.GetType(vHandlerImplTypeName);
      if(t == null)
        throw new EBioException("Не найден тип сообщения: " + vHandlerImplTypeName, null);
      Type[] parTypes = new Type[2];
      parTypes[0] = typeof(HttpContext);
      parTypes[1] = typeof(CAjaxRequest);
      Object[] parVals = new Object[] { pContext, request };
      ConstructorInfo ci = t.GetConstructor(parTypes);
      ABioHandler obj = (ABioHandler)ci.Invoke(parVals);
      return obj;
    }
  }
}
