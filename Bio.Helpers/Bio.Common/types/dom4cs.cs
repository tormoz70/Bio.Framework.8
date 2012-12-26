namespace Bio.Helpers.Common.Types {
#if !SILVERLIGHT
  using System;
  using System.IO;
  using System.Text;
  using System.Xml;
  using System.Xml.Xsl;
  using System.Xml.XPath;
  using Bio.Helpers.Common;

  public class dom4cs{
    
    private void InitBlock() {
      this.FEncoding = Utl.SYS_ENCODING;
    }

    public dom4cs() {
      this.InitBlock();
    }


    private XmlDocument FDocument;
    private String FFileName = null;
    private String FLastError = "";
    private String FEncoding;
		
		
		
    public static bool DocumentExists(String pFileName){
      return File.Exists(pFileName);
    }
		
    public XmlDocument XmlDoc {
      get { return this.FDocument; }
      set { this.FDocument = value; }
    }
		
    public String FileName {
      get { return this.FFileName; }
      set { this.FFileName = value; }
    }
		
    public static dom4cs NewDocument(String vRootTag) {
      return NewDocument(vRootTag, null);
    }
		
    public static dom4cs NewDocument(String vRootTag, String vEncoding) {
      return NewDocument(vRootTag, vEncoding, null); ;
    }

    public static XmlDocument NewEmptyDocument(String enc) {
      XmlDocument doc = new XmlDocument();
      String vLocEncoding = Encoding.UTF8.WebName;
      if (!String.IsNullOrEmpty(enc))
        vLocEncoding = enc;
      XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", vLocEncoding, null);
      doc.AppendChild(xd);
      return doc;
    }

    public static dom4cs NewDocument(String vRootTag, String vEncoding, String vDTDFileName) {
      dom4cs newDOM4CS = new dom4cs();
      XmlDocument doc = new XmlDocument();
      if (vEncoding != null)
        newDOM4CS.FEncoding = vEncoding;
      XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", newDOM4CS.FEncoding, null);
      doc.AppendChild(xd);
      if (vDTDFileName != null) {
        XmlDocumentType xdt = doc.CreateDocumentType(vRootTag, null, vDTDFileName, null);
        doc.AppendChild(xdt);
      }
      if (vRootTag != null) {
        XmlElement rn = doc.CreateElement(vRootTag);
        doc.AppendChild(rn);
      }
      newDOM4CS.XmlDoc = doc;
      return newDOM4CS;
    }

    public static XmlDocument CreXmlDocument(String pXML) {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(pXML);
      return doc;
    }

    public static XmlDocument CreXmlDocument(String vRootTag, String vEncoding, String vDTDFileName) {
      XmlDocument doc = new XmlDocument();
      XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", vEncoding, null);
      doc.AppendChild(xd);
      if(vDTDFileName != null) {
        XmlDocumentType xdt = doc.CreateDocumentType(vRootTag, null, vDTDFileName, null);
        doc.AppendChild(xdt);
      }
      if(vRootTag != null) {
        XmlElement rn = doc.CreateElement(vRootTag);
        doc.AppendChild(rn);
      }
      return doc;
    }

    public static dom4cs OpenDocument(String vFileName) {
      dom4cs newTDOM4CS = new dom4cs();
      XmlDocument doc = new XmlDocument();
      doc.Load(vFileName);
      newTDOM4CS.XmlDoc = doc;
      newTDOM4CS.FileName = vFileName;
      return newTDOM4CS;
    }
		
    public static dom4cs AssignDocument(StreamReader vInStream) {
      dom4cs newTDOM4CS = new dom4cs();
      XmlDocument doc = new XmlDocument();
      doc.Load(vInStream);
      newTDOM4CS.XmlDoc = doc;
      return newTDOM4CS;
    }

    public static dom4cs AssignDocument(String vXML) {
      dom4cs newTDOM4CS = new dom4cs();
      XmlDocument doc = new XmlDocument();
      StringReader vInStream = new StringReader(vXML);
      doc.Load(vInStream);
      newTDOM4CS.XmlDoc = doc;
      return newTDOM4CS;
    }

    public virtual void  ReopenDocument(){
      try{
        FDocument.Load(FFileName);
      }catch (Exception ex){
        throw new Exception("Ошибка при открытии файла: '" + FFileName + "'. <br> Сообщение системы:" + ex.Message, ex);
      }
    }
		
		
    public virtual void  SaveDocument(){
      SaveDocumentAs(FFileName);
    }
		
    public virtual void  SaveDocumentAs(String vFileName){
      try{
        
        FileStream fs;
        if (File.Exists(vFileName))
          fs = new FileStream(vFileName, FileMode.Truncate);
        else
          fs = new FileStream(vFileName, FileMode.CreateNew);
        this.WriteToStream(fs);
        fs.Flush();
        fs.Close();
        FFileName = vFileName;
      }catch (Exception ex){
        throw new Exception("Ошибка при сохранении документа '" + FFileName + "'. <br> Сообщение системы:" + ex.Message, ex);
      }
    }

    public static void WriteDocumentToStream(XmlDocument doc, Stream stream, Encoding enc) {
      XmlTextWriter xtw = new XmlTextWriter(stream, enc);
      xtw.Formatting = Formatting.Indented;
      xtw.Indentation = 2;
      xtw.IndentChar = ' ';
      doc.WriteTo(xtw);
      xtw.Flush();
    }

    public virtual void WriteToStream(Stream stream) {
      try {
        Encoding enc = Encoding.GetEncoding(FEncoding);
        WriteDocumentToStream(this.FDocument, stream, enc);
      } catch (Exception ex) {
        throw new Exception("Ошибка при записи документа '" + this.FFileName + "' в поток. <br> Сообщение системы:" + ex.Message, ex);
      }
    }

    public virtual void WriteToStream(Stream pOut, String pXslDoc) {
      IXPathNavigable nav = FDocument.CreateNavigator();
      XslCompiledTransform xslt = new XslCompiledTransform(true);
      XsltSettings settings = new XsltSettings(false, true);
      xslt.Load(pXslDoc, settings, new XmlUrlResolver());
      xslt.Transform(nav, null, pOut);
    }

    public virtual String TransformDocument(String pXslDoc) {
      IXPathNavigable nav = FDocument.CreateNavigator();
      XslCompiledTransform xslt = new XslCompiledTransform();
      XsltSettings settings = new XsltSettings(false, true);
      xslt.Load(pXslDoc, settings, new XmlUrlResolver());
      MemoryStream ms = new MemoryStream();
      xslt.Transform(nav, null, ms);

      StringWriter vRslt = new StringWriter();
      ms.Seek(0, SeekOrigin.Begin);
      StreamReader sr = new StreamReader(ms);
      String vLine;
      while((vLine = sr.ReadLine()) != null)
        vRslt.WriteLine(vLine);
      return vRslt.ToString();
    }

    public virtual String GetLastError(){
      String fLstErr = FLastError;
      FLastError = "";
      return fLstErr;
    }


    private void ExchengeNodes(XmlNode vNode_1, XmlNode vNode_2){
      XmlNode vParentNode = vNode_2.ParentNode;
      XmlNode vNode_1_clone = vNode_1.Clone();
      vParentNode.InsertBefore(vNode_1_clone, vNode_2);
      vParentNode.ReplaceChild(vNode_2, vNode_1);
    }


    private void _quickSort(XmlNode vNode, String pPath, int iLo, int iHi, String sortAttr){
      XmlNodeList fList = null;
      if(pPath == null)
        fList = vNode.ChildNodes;
      else
        fList = vNode.SelectNodes(pPath);
      int Lo = iLo;
      int Hi = iHi;
      Int64 Mid = this.getIntByPath(fList.Item((Lo + Hi) / 2), sortAttr);
      do{
        Int64 aLo = this.getIntByPath(fList.Item(Lo), sortAttr);
        Int64 aHi = this.getIntByPath(fList.Item(Hi), sortAttr);
      while(aLo < Mid){
        Lo++;
        aLo = this.getIntByPath(fList.Item(Lo), sortAttr);
      }
      while(aHi > Mid){
        Hi--;
        aHi = this.getIntByPath(fList.Item(Hi), sortAttr);
      }
        if(Lo <= Hi){
          if(Lo != Hi)
            ExchengeNodes(fList.Item(Lo), fList.Item(Hi));
          fList = vNode.ChildNodes;
          Lo++;
          Hi--;
        }
      }while(Lo < Hi);

      if(Hi > iLo) _quickSort(vNode, pPath, iLo, Hi, sortAttr);
      if(Lo < iHi) _quickSort(vNode, pPath, Lo, iHi, sortAttr);
    }

    public void QuickSort(XmlNode vNode, String pPath, String sortAttr){
      if(vNode != null){
        XmlNodeList fList = null;
        if(pPath == null)
          fList = vNode.ChildNodes;
        else
          fList = vNode.SelectNodes(pPath);
        this._quickSort(vNode, pPath, 0, fList.Count-1, sortAttr);
      }
    }

    private XmlNodeList _getNodeListForBubble(XmlNode vNode, String pPath){
      if(pPath == null)
        return vNode.ChildNodes;
      else
        return vNode.SelectNodes(pPath);
    }
    public void BubbleSort(XmlNode vNode, String pPath, String sortAttr){
      if(vNode != null){
        XmlNodeList fList = this._getNodeListForBubble(vNode, pPath);
        int vListCnt = fList.Count;
        for(int i=vListCnt-1; i >= 0; i--){
          for(int j=0; j<vListCnt-1; j++){
            if(this.getIntByPath(fList.Item(j), sortAttr) > this.getIntByPath(fList.Item(j+1), sortAttr)){
              ExchengeNodes(fList.Item(j), fList.Item(j+1));
            }
            fList = this._getNodeListForBubble(vNode, pPath);
          }
        }
      }
    }

    private String getStrByPath(XmlNode vNode, String vPaths){
      String[] paths = Utl.SplitString(vPaths, '|');
      String cValue = null;
      for(int i=0; i<paths.Length; i++){
        String cPath = paths[i];
        XmlNode cNode = vNode.SelectSingleNode(cPath);
        if(cNode != null){
          if(cNode.NodeType == XmlNodeType.Attribute)
            cValue = ((XmlAttribute)cNode).Value;
          else if(cNode.NodeType == XmlNodeType.Element)
            cValue = cNode.InnerText;
          break;
        }
      }
      return cValue;
    }

    private Int64 getIntByPath(XmlNode vNode, String vPaths){
      String[] paths = Utl.SplitString(vPaths, '|');
      String cValue = null;
      for(int i=0; i<paths.Length; i++){
        String cPath = paths[i];
        XmlNode cNode = vNode.SelectSingleNode(cPath);
        if(cNode != null){
          if(cNode.NodeType == XmlNodeType.Attribute)
            cValue = ((XmlAttribute)cNode).Value;
          else if(cNode.NodeType == XmlNodeType.Element)
            cValue = cNode.InnerText;
          break;
        }
      }
      return Int64.Parse(cValue);
    }

    private XmlNode _cloneNode(XmlDocument pDoc, XmlNode pNode){
      XmlNode newNode = pDoc.CreateElement(pNode.Name);
      newNode.InnerXml = pNode.InnerXml;
      _syncAttrs(pNode, newNode);
      return newNode;
    }

    private void _addNode(XmlNode vPrent, XmlNode vNode){
      if(vPrent != null){ 
        XmlNode newNode = this._cloneNode(vPrent.OwnerDocument, vNode);
        vPrent.AppendChild(newNode);
      }
    }

    private void _insNode(XmlNode vPrent, XmlNode vNode, String vSortPath){
      if(vPrent != null){
        this._addNode(vPrent, vNode);
        //this.QuickSort(vPrent, vSortPath);
      }
    }

    private void _insNode(XmlNode vPrent, XmlNode vNode, int pIndex){
      if(vPrent != null){
        XmlNode newNode = this._cloneNode(vPrent.OwnerDocument, vNode);
        if((pIndex < vPrent.ChildNodes.Count) && (pIndex >= 0))
          vPrent.InsertBefore(newNode, vPrent.ChildNodes[pIndex]);
        else
          vPrent.AppendChild(newNode);
      }
    }

    private void _replaceNode(XmlNode vPrent, XmlNode vDest, XmlNode vSrc){
      if(vPrent != null){
        XmlNode newNode = this._cloneNode(vPrent.OwnerDocument, vSrc);
        vPrent.ReplaceChild(newNode, vDest);
      }
    }

    private void _syncAttrs(XmlNode vSrc, XmlNode vDest){
      if(vSrc.Name.Equals("iobject") && vDest.Name.Equals("iobject")){
        if(((XmlElement)vSrc).HasAttribute("exporter")){
          XmlAttribute cAttr = (XmlAttribute)((XmlElement)vSrc).Attributes.GetNamedItem("exporter");
          ((XmlElement)vDest).SetAttribute(cAttr.Name, cAttr.Value);
        }
      }else{
        for(int i=0; i<vSrc.Attributes.Count; i++){
          XmlAttribute cAttr = (XmlAttribute)vSrc.Attributes.Item(i);
          ((XmlElement)vDest).SetAttribute(cAttr.Name, cAttr.Value);
        }
      }
    }

    private void _killCData(XmlNode pNode){
      for(int i=0; i<pNode.ChildNodes.Count; i++){
        XmlNode vNode = pNode.ChildNodes.Item(i);
        if(vNode.NodeType == XmlNodeType.CDATA){
          vNode.RemoveChild(vNode);
        }else
          i++;
      }
    }

    private void _recurseNode(XmlNode pSrc, XmlDocument pInsructions){
      for(int i=0; i<pSrc.ChildNodes.Count; i++){
        XmlNode vSrcNode = pSrc.ChildNodes.Item(i);
        if(vSrcNode.NodeType == XmlNodeType.Text){
          String fPath = this._getNodePath(pSrc, pInsructions); 
          XmlNode vDest = this.XmlDoc.SelectSingleNode(fPath);
          vDest.InnerText = vSrcNode.InnerText;
        }else if(vSrcNode.NodeType == XmlNodeType.CDATA){
          String fPath = this._getNodePath(pSrc, pInsructions); 
          XmlNode vDest = this.XmlDoc.SelectSingleNode(fPath);
          //this._killCData(vDest);
          vDest.InnerXml = "";
          vDest.AppendChild(vDest.OwnerDocument.CreateCDataSection(vSrcNode.InnerText));
        }else if(vSrcNode.NodeType == XmlNodeType.Comment){

        }else if(vSrcNode.NodeType == XmlNodeType.Element)
          this.syncNode(vSrcNode, pInsructions);
      }
    }

    private String _getNodePath(XmlNode pSrcNode, XmlDocument pInsructions){
      String vAttrCond = "";
      XmlAttribute vComp = (XmlAttribute)pInsructions.DocumentElement.SelectSingleNode("insruction[@tagName='"+pSrcNode.Name+"']/compare/@path");
      if(vComp != null){
        String[] vConds = Utl.SplitString(vComp.Value, '|');
        String vCondPath = "";
        String vCondVal = "";
        for(int i=0; i<vConds.Length; i++){
          XmlNode vCndValNode = pSrcNode.SelectSingleNode(vConds[i]);
          if(vCndValNode != null){
            if(vCndValNode.NodeType == XmlNodeType.Element){
              vCondVal = ((XmlElement)vCndValNode).InnerText;
              vCondPath = vConds[i];
            }else if(vCndValNode.NodeType == XmlNodeType.Attribute){
              vCondVal = ((XmlAttribute)vCndValNode).Value;
              vCondPath = vConds[i];
            }
            break;
          }
        }
        if(!vCondVal.Equals(""))
          vAttrCond = "["+vCondPath+"='"+vCondVal+"']";
      }

      if(pSrcNode.ParentNode.NodeType != XmlNodeType.Document){
        return _getNodePath(pSrcNode.ParentNode, pInsructions)+"/"+pSrcNode.Name+vAttrCond;
      }else
        return pSrcNode.Name+vAttrCond;
    }
		
    private int _getNodeIndex(XmlNode pNode){
      int vRslt = 0;
      XmlNode vCurNode = pNode;
      while(vCurNode.PreviousSibling != null){
        vCurNode = vCurNode.PreviousSibling;
        vRslt ++;
      }
      return vRslt;
    }

    private bool syncNode(XmlNode vSrcNode, XmlDocument pInsructions){
      if(vSrcNode.NodeType == XmlNodeType.Comment) return true;
      String fPath = this._getNodePath(vSrcNode, pInsructions); 
      XmlNode fInsruction = pInsructions.DocumentElement.SelectSingleNode("insruction[@tagName='"+vSrcNode.Name+"']");
      if(fInsruction == null)
        throw new Exception("Для элемента " + vSrcNode.Name + " не найдена инструкция наследования.");
      String onNotExists = ((XmlElement)fInsruction).GetAttribute("onNotExists");
      String onExists = ((XmlElement)fInsruction).GetAttribute("onExists");
      String onEquals = null;
      String onElse = null;
      String compPath = null; 
      XmlNode fDestNode = this.XmlDoc.SelectSingleNode(fPath);
      bool fExists = fDestNode != null;
      if((fDestNode != null) && (fDestNode.ParentNode != null) && (onExists.Equals("compare"))){
        XmlNode compare = fInsruction.SelectSingleNode("compare");
        onEquals = ((XmlElement)compare).GetAttribute("onEquals");
        onElse = ((XmlElement)compare).GetAttribute("onElse");
        compPath = ((XmlElement)compare).GetAttribute("path");
        XmlNode fndNode = null;
        if(compare != null){
          String fSrcVal = this.getStrByPath(vSrcNode, compPath);
          for(int i=0; i<fDestNode.ParentNode.ChildNodes.Count; i++){
            XmlNode cNode = fDestNode.ParentNode.ChildNodes.Item(i);
            String fDstVal = this.getStrByPath(cNode, compPath);
            if(((fSrcVal != null) && (fDstVal != null)) && (fSrcVal.Equals(fDstVal))){
              fndNode = fDestNode.ParentNode.ChildNodes.Item(i);
              break;
            }
          }
        }
        fDestNode = fndNode;
      }
      XmlNode myParent;
      if(vSrcNode.ParentNode.NodeType != XmlNodeType.Document)
        myParent = this.XmlDoc.SelectSingleNode(this._getNodePath(vSrcNode.ParentNode, pInsructions));
      else
        myParent = null;
      if(!fExists){
        if(onNotExists.Equals("append")){
          this._addNode(myParent, vSrcNode);
        }else if(onNotExists.Equals("insert")){
          int vInsIndex = this._getNodeIndex(vSrcNode);
          this._insNode(myParent, vSrcNode, vInsIndex);
        }else if (onNotExists.Equals("raiseError")){
          throw new Exception("Ошибка при объединении документа, не существует обязательный элемент '" + fPath + "'");
        }
      }else{
        if(onExists.Equals("append")){
          this._addNode(myParent, vSrcNode);
        }else if (onExists.Equals("replace")){
          this._replaceNode(myParent, fDestNode, vSrcNode);
        }else if (onExists.Equals("compare")){
          if(fDestNode != null){
            if(onEquals.Equals("replace")){
              this._replaceNode(myParent, fDestNode, vSrcNode);
            }else if(onEquals.Equals("recurseAndReplaceAttrs")){
              this._syncAttrs(vSrcNode, fDestNode);
              this._recurseNode(vSrcNode, pInsructions);
            }else if(onEquals.Equals("recurseAndMergeAttrs")){
              this._syncAttrs(vSrcNode, fDestNode);
              this._recurseNode(vSrcNode, pInsructions);
            }else if (onExists.Equals("recurseAndDontChangeAttrs")){
              this._recurseNode(vSrcNode, pInsructions);
            }
          }else{
            if(onElse.Equals("append")){
              this._addNode(myParent, vSrcNode);
            }else if(onElse.Equals("insert")){
              this._insNode(myParent, vSrcNode, compPath);
            }else if(onElse.Equals("raiseError")){
              throw new Exception("Ошибка при объединении документа, не существует обязательный элемент '" + fPath + "'");
            }
          }
        }else if (onExists.Equals("recurseAndReplaceAttrs")){
          fDestNode.Attributes.RemoveAll();
          _syncAttrs(vSrcNode, fDestNode);
          this._recurseNode(vSrcNode, pInsructions);
        }else if (onExists.Equals("recurseAndMergeAttrs")){
          this._syncAttrs(vSrcNode, fDestNode);
          this._recurseNode(vSrcNode, pInsructions);
        }else if (onExists.Equals("recurseAndDontChangeAttrs")){
          this._recurseNode(vSrcNode, pInsructions);
        }
      }
      return true;
    }


    public bool MergeDocument(dom4cs vSrcDoc, String vMergeInsructionFileName){
      dom4cs fMI = dom4cs.OpenDocument(vMergeInsructionFileName);
      this.syncNode(vSrcDoc.XmlDoc.DocumentElement, fMI.XmlDoc);
      this.FFileName = vSrcDoc.FFileName;
      this.FEncoding = vSrcDoc.FEncoding;
      XmlNodeList vForms = this.XmlDoc.DocumentElement.SelectNodes("//form");
      for(int i=0; i<vForms.Count; i++){
        XmlNodeList vFormTabs = ((XmlElement)vForms[i]).SelectNodes("tabs/tab");
        for(int j=0; j<vFormTabs.Count; j++){
          BubbleSort(vFormTabs[i], "rows/row", "@id");
        }
      }
      XmlElement vTools = (XmlElement)this.XmlDoc.DocumentElement.SelectSingleNode("//tools");
      //Utils.AppendStringToFile("d:\\mrg_sort_bfr.xml", this.XmlDoc.OuterXml, Encoding.UTF8);
      BubbleSort(vTools, "tool", "@id");
      //Utils.AppendStringToFile("d:\\mrg_sort_aftr.xml", this.XmlDoc.OuterXml, Encoding.UTF8);
      return true;
    }

    public static void CopyAttrs(XmlElement vNodeSrc, XmlElement vNodeDst){
      for(int i=0; i<vNodeSrc.Attributes.Count; i++)
        vNodeDst.SetAttribute(vNodeSrc.Attributes[i].Name, vNodeSrc.Attributes[i].Value);
    }

    
    public static XmlDocument CloneDocument(XmlDocument pXmlDoc, String pEncoding) {
      if (pXmlDoc != null) {
        XmlDocument doc = new XmlDocument();
        XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", pEncoding, null);
        doc.AppendChild(xd);
        XmlNode rn = doc.ImportNode(pXmlDoc.DocumentElement, true);
        doc.AppendChild(rn);
        return doc;
      } else
        return null;
    }
    public static XmlDocument CloneDocument(XmlDocument pXmlDoc) {
      return CloneDocument(pXmlDoc, Utl.SYS_ENCODING);
    }
  }
#endif
}