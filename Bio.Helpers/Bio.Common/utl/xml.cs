using System.Xml.XPath;

namespace Bio.Helpers.Common {
	using System;
	using System.Xml;
	using System.IO;
	using System.Xml.Serialization;

  public class Xml {
#if !SILVERLIGHT

    public static XmlDocument NewXmlDocument(String vXML) {
      var doc = new XmlDocument();
      doc.LoadXml(vXML);
      return doc;
    }

    public static void margeAttrs(XmlNode vSrc, XmlNode vDest, bool pReplace) {
      if(((vSrc != null)&&(vSrc.NodeType == XmlNodeType.Element))&&((vDest != null)&&(vDest.NodeType == XmlNodeType.Element))){
        var v_srcElem = (XmlElement)vSrc;
        var v_destElem = (XmlElement)vDest;
        for(var i = 0; i < v_srcElem.Attributes.Count; i++) {
          var v_attr = (XmlAttribute)v_srcElem.Attributes.Item(i);
          if(pReplace || !v_destElem.HasAttribute(v_attr.Name))
            v_destElem.SetAttribute(v_attr.Name, v_attr.Value);
        }
      }
    }

    /// <summary>
    /// Копирует Элемент vSrc в vAddTo, Не зависит от того в каком документе находятся 
    /// элементы vSrc и vAddTo
    /// </summary>
    /// <param name="vSrc"></param>
    /// <param name="vAddTo"></param>
    /// <param name="pDeep"></param>
    /// <returns></returns>
    public static XmlNode copyNode(XmlNode vSrc, XmlNode vAddTo, bool pDeep) {
      if((vAddTo != null) && (vAddTo.NodeType == XmlNodeType.Element) && (vSrc != null)) {
        if(vSrc.OwnerDocument != null && vSrc.OwnerDocument.Equals(vAddTo.OwnerDocument))
          return vAddTo.AppendChild(vSrc.Clone());
        if (vAddTo.OwnerDocument != null) return vAddTo.AppendChild(vAddTo.OwnerDocument.ImportNode(vSrc, pDeep));
      }
      return null;
    }

    public static void margeNodes(XmlNode vSrc, XmlNode vDest, bool pReplaceAttrs) {
      margeAttrs(vSrc, vDest, pReplaceAttrs);
      foreach(XmlNode v_node in vSrc.ChildNodes)
        copyNode(v_node, vDest, true);
    }

    /// <summary>
    ///   Разбирает полное имя элемента на части.
    /// </summary>
    /// <param name="pFullNodeName">Полное имя включая Namespace</param>
    /// <param name="vNameSpace">Возвращает Namespace, если он присутствует в pFullNodeName</param>
    /// <param name="vNodeName">Возвращает имя без Namespace</param>
    public static void parsFullNodeName(String pFullNodeName, ref String vNameSpace, ref String vNodeName) {
      var v_nodeNamePrts = Utl.SplitString(pFullNodeName, ':');
      vNameSpace = (v_nodeNamePrts.Length == 2) ? v_nodeNamePrts[0] : null;
      vNodeName = (v_nodeNamePrts.Length == 2) ? v_nodeNamePrts[1] : ((v_nodeNamePrts.Length == 1) ? v_nodeNamePrts[0] : null);
    }

    public static void appendNode2El(XmlElement vEl, String ppName, String ppValue) {
      if (vEl.OwnerDocument != null) {
        var v_elem = vEl.OwnerDocument.CreateElement(ppName);
        v_elem.InnerText = ppValue;
        vEl.AppendChild(v_elem);
      }
    }

    public static XmlNodeList selectNodesLocal(XmlElement vEl, String pName) {
      return null;
    }


#region Сериализация

    public static void Serialize(Object obj, TextWriter pOutTxtWriter) {
      if (obj != null) {
        var v_serMsg = new XmlSerializer(obj.GetType());
        v_serMsg.Serialize(pOutTxtWriter, obj);
      }
    }

    public static String Encode(Object obj) {
      var v_xml = new StringWriter();
      Serialize(obj, v_xml);
      return v_xml.ToString();
    }

    public static XmlElement Encode(Object obj, XmlElement parentElem) {
      var v_xml = Encode(obj);
      var doc = new XmlDocument();
      doc.LoadXml(v_xml);
      if (parentElem != null) {
        var v_newChld = parentElem.OwnerDocument.ImportNode(doc.DocumentElement, true);
        parentElem.AppendChild(v_newChld);
        return (XmlElement)v_newChld;
      }
      return doc.DocumentElement;
    }

    public static T Decode<T>(Object obj, XmlElement parentElem) where T : new() {
      T rslt = new T();
      return rslt;
    }

#endregion

    public static String getInnerText(XmlNode node, String defaultValue) {
      return (node != null) ? node.InnerText : defaultValue;
    }
    public static String getInnerText(XmlNode node) {
      return getInnerText(node, null);
    }

	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="elem"></param>
	  /// <param name="path"></param>
	  /// <param name="defaultValue"></param>
	  /// <returns></returns>
	  public static String getInnerText(XmlElement elem, String path, String defaultValue) {
      return (elem != null) ? getInnerText(elem.SelectSingleNode(path), defaultValue) : defaultValue;
    }

    public static T getAttribute<T>(XmlElement elem, String attrName, T defaultVal) {
      if (elem.HasAttribute(attrName)) {
        var val = elem.GetAttribute(attrName);
        return Utl.Convert2Type<T>(val);
      } else
        return defaultVal;
    }
#else
    public static String getAttribute(XPathNavigator nav, String attrName) {
      var attr = nav.SelectSingleNode("@"+attrName);
      return (attr != null) ? attr.Value : null;
    }
    public static Boolean hasAttribute(XPathNavigator nav, String attrName) {
      var attr = nav.SelectSingleNode("@" + attrName);
      return (attr != null);
    }
#endif
  }
}