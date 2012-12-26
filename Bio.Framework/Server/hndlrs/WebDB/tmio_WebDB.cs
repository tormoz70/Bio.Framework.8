namespace Bio.Framework.Server {

  using System;

  using System.Data;
  using System.Data.Common;

  using System.Collections;
  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.Linq;
  using System.IO;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using Bio.Framework.Packets;

  /// <summary>
  /// Обработчик запросов. Возвращает данные  поток.
  /// </summary>
  public class tmio_WebDB:ABioHandlerBio {

    private const int C_MaxRecords = 1000;

    public tmio_WebDB(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private void processData(CJSCursor pCursor, StringBuilder vDoc, ref EBioException vEx) {
      try {
        bool NeedClose = false;
        if(!pCursor.IsActive && (pCursor.Connection != null)) {
          pCursor.Open(120);
          NeedClose = true;
        }
      // перебираем все записи в курсоре
        while(pCursor.Next()) {
          // перебираем все поля одной записи
          StringBuilder vFRow = new StringBuilder();
          foreach(DictionaryEntry vCur in pCursor.RowValues) {
            vFRow.Append(SQLUtils.ObjectAsString(vCur.Value));
          }
          vDoc.Append(vFRow.ToString());
        }
      /*} catch(ebio.doa.EBioDOATooMuchRows ex) {
        vDoc.AppendLine("<!-- " + ex.ToString() + " -->");*/
        if(NeedClose)
          pCursor.Close();
      } catch(Exception ex) {
        vEx = EBioException.CreateIfNotEBio(ex);
      }

    }

    private void _loadDocFromCursor(XmlElement ds, StringBuilder doc, ref EBioException v_ex) {
      using (var vConn = this.BioSession.Cfg.dbSession.GetConnection()) {
        try {
          var vCursor = new CJSCursor(vConn, ds, this.bioCode);
          var v_request = this.bioRequest<CJsonStoreRequestGet>();
          vCursor.Init(v_request);
          try {
            this.processData(vCursor, doc, ref v_ex);
          } finally {
            vCursor.Close();
          }
        } catch (Exception ex) {
          v_ex = EBioException.CreateIfNotEBio(ex);
        } finally {
          if (vConn != null)
            vConn.Close();
        }
      }
    }

    private void _loadDocFromProc(XmlElement ds, StringBuilder doc, ref EBioException v_ex) {
      using (var vConn = this.BioSession.Cfg.dbSession.GetConnection()) {
        try {
          var vCursor = new CJSCursor(vConn, ds, this.bioCode);
          var v_request = this.bioRequest<CJsonStoreRequestGet>();
          vCursor.DoExecuteSQL(v_request.bioParams, 120);
          var v_out_prm = v_request.bioParams.Where((p) => {
            return (p.ParamDir == ParamDirection.InputOutput) ||
                     (p.ParamDir == ParamDirection.Output) ||
                       (p.ParamDir == ParamDirection.Return);
          }).FirstOrDefault();
          if (v_out_prm != null)
            doc.Append(v_out_prm.Value);
        } catch (Exception ex) {
          v_ex = EBioException.CreateIfNotEBio(ex);
        } finally {
          if (vConn != null)
            vConn.Close();
        }
      }
    }

    protected override void doExecute() {
      base.doExecute();
      EBioException vEx = null;
      var vDoc = new StringBuilder();
      try {
        var vDS = this.FBioDesc.DocumentElement;
        if(vDS != null) {
          if (vDS.SelectSingleNode("SQL[@action='select']") != null)
            this._loadDocFromCursor(vDS, vDoc, ref vEx);
          else if (vDS.SelectSingleNode("SQL[@action='execute']") != null)
            this._loadDocFromProc(vDS, vDoc, ref vEx);

        } else
          vEx = new EBioException("В описании объекта [" + this.bioCode + "] не найден раздел <store>.");

      } finally {
        if(vEx == null) {
          String vIOFN = SrvUtl.bldiniFileName(this.BioSession.Cfg.IniPath, this.bioCode);
          String vXSLFN = vIOFN.ToLower().Replace(".xml", ".xsl");
          String vCSSFN = vIOFN.ToLower().Replace(".xml", ".css");
          if(File.Exists(vXSLFN)) {
            dom4cs vRsltDoc = new dom4cs();
            vRsltDoc.XmlDoc = dom4cs.CreXmlDocument(vDoc.ToString());
            XmlElement vElem = vRsltDoc.XmlDoc.CreateElement("appurl");
            vElem.InnerText = this.BioSession.Cfg.AppURL;
            vRsltDoc.XmlDoc.DocumentElement.AppendChild(vElem);
            vElem = vRsltDoc.XmlDoc.CreateElement("biourl");
            vElem.InnerText = SrvUtl.bldIOPathUrl(this.BioSession.Cfg.AppURL, this.bioCode);
            vRsltDoc.XmlDoc.DocumentElement.AppendChild(vElem);
            vRsltDoc.WriteToStream(this.Context.Response.OutputStream, vXSLFN);
          } else {
            this.Context.Response.Write(vDoc.ToString());
          }
        } else {
          String vAgent = this.BioSession.CurSessionRemoteAgent;
          if(vAgent.ToUpper().StartsWith("DALPHA"))
            throw vEx;
          else
            this.Context.Response.Write(vEx.ToString());
        }
        this.Context.Response.Flush();
      }
    }
  }
}
