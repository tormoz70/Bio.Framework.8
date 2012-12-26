namespace Bio.Helpers.Common {
	using System;
	using System.Xml;
	using System.Text;
  using System.IO;
  using System.Threading;
  //using System.Data;
  using System.Collections;
  using System.Xml.Serialization;
using System.Xml.XPath;

	public class Xml {
#if !SILVERLIGHT

    public static XmlDocument NewXmlDocument(String vXML) {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(vXML);
      return doc;
    }

    public static void margeAttrs(XmlNode vSrc, XmlNode vDest, bool pReplace) {
      if(((vSrc != null)&&(vSrc.NodeType == XmlNodeType.Element))&&((vDest != null)&&(vDest.NodeType == XmlNodeType.Element))){
        XmlElement vSrcElem = (XmlElement)vSrc;
        XmlElement vDestElem = (XmlElement)vDest;
        for(int i = 0; i < vSrcElem.Attributes.Count; i++) {
          XmlAttribute cAttr = (XmlAttribute)vSrcElem.Attributes.Item(i);
          if(pReplace || !vDestElem.HasAttribute(cAttr.Name))
            vDestElem.SetAttribute(cAttr.Name, cAttr.Value);
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
        if(vSrc.OwnerDocument.Equals(vAddTo.OwnerDocument))
          return vAddTo.AppendChild(vSrc.Clone());
        else
          return vAddTo.AppendChild(vAddTo.OwnerDocument.ImportNode(vSrc, pDeep));
      }
      return null;
    }

    public static void margeNodes(XmlNode vSrc, XmlNode vDest, bool pReplaceAttrs) {
      Xml.margeAttrs(vSrc, vDest, pReplaceAttrs);
      foreach(XmlNode vNode in vSrc.ChildNodes)
        copyNode(vNode, vDest, true);
    }

    /// <summary>
    ///   Разбирает полное имя элемента на части.
    /// </summary>
    /// <param name="pFullNodeName">Полное имя включая Namespace</param>
    /// <param name="vNameSpace">Возвращает Namespace, если он присутствует в pFullNodeName</param>
    /// <param name="vNodeName">Возвращает имя без Namespace</param>
    public static void parsFullNodeName(String pFullNodeName, ref String vNameSpace, ref String vNodeName) {
      String[] nodeNamePrts = Utl.SplitString(pFullNodeName, ':');
      vNameSpace = (nodeNamePrts.Length == 2) ? nodeNamePrts[0] : null;
      vNodeName = (nodeNamePrts.Length == 2) ? nodeNamePrts[1] : ((nodeNamePrts.Length == 1) ? nodeNamePrts[0] : null);
    }

    public static void appendNode2El(XmlElement vEl, String ppName, String ppValue) {
      XmlElement vElem = vEl.OwnerDocument.CreateElement(ppName);
      vElem.InnerText = ppValue;
      vEl.AppendChild(vElem);
    }

    public static XmlNodeList selectNodesLocal(XmlElement vEl, String pName) {
      return null;
    }


#region Сериализация

    public static void Serialize(Object obj, TextWriter pOutTxtWriter) {
      if (obj != null) {
        XmlSerializer vSerMsg = new XmlSerializer(obj.GetType());
        vSerMsg.Serialize(pOutTxtWriter, obj);
      }
    }

    public static String Encode(Object obj) {
      StringWriter vXML = new StringWriter();
      Serialize(obj, vXML);
      return vXML.ToString();
    }

    public static XmlElement Encode(Object obj, XmlElement parentElem) {
      String vXML = Encode(obj);
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(vXML);
      if (parentElem != null) {
        XmlNode newChld = parentElem.OwnerDocument.ImportNode(doc.DocumentElement, true);
        parentElem.AppendChild(newChld);
        return (XmlElement)newChld;
      } else {
        return doc.DocumentElement;
      }
    }

    public static T Decode<T>(Object obj, XmlElement parentElem) where T : new() {
      T rslt = new T();
      return rslt;
    }

#endregion

    public static String getInnerText(XmlNode elem) {
      return (elem != null) ? elem.InnerText : null;
    }

    public static T getAttribute<T>(XmlElement elem, String attrName, T defaultVal) {
      if (elem.HasAttribute(attrName)) {
        String val = elem.GetAttribute(attrName);
        return Utl.Convert2Type<T>(val);
      } else
        return defaultVal;
    }
#else
    public static String getAttribute(XPathNavigator nav, String attrName) {
      XPathNavigator attr = nav.SelectSingleNode("@"+attrName);
      return (attr != null) ? attr.Value : null;
    }
    public static Boolean hasAttribute(XPathNavigator nav, String attrName) {
      XPathNavigator attr = nav.SelectSingleNode("@" + attrName);
      return (attr != null);
    }
#endif
  }
}