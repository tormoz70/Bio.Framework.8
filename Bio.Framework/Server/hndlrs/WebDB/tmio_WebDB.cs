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

    public tmio_WebDB(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    private void processData(CJSCursor pCursor, StringBuilder vDoc, ref EBioException vEx) {
      try {
        var needClose = false;
        if(!pCursor.IsActive && (pCursor.Connection != null)) {
          pCursor.Open(120);
          needClose = true;
        }
      // перебираем все записи в курсоре
        while(pCursor.Next()) {
          // перебираем все поля одной записи
          var fRow = new StringBuilder();
          foreach(DictionaryEntry vCur in pCursor.RowValues) {
            fRow.Append(SQLUtils.ObjectAsString(vCur.Value));
          }
          vDoc.Append(fRow);
        }
        if(needClose)
          pCursor.Close();
      } catch(Exception ex) {
        vEx = EBioException.CreateIfNotEBio(ex);
      }

    }

    private void _loadDocFromCursor(XmlElement ds, StringBuilder doc, ref EBioException v_ex) {
      using (var vConn = this.BioSession.Cfg.dbSession.GetConnection()) {
        try {
          var vCursor = new CJSCursor(vConn, ds, this.bioCode);
          var v_request = this.BioRequest<JsonStoreRequestGet>();
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
          var v_request = this.BioRequest<JsonStoreRequestGet>();
          vCursor.DoExecuteSQL(v_request.BioParams, 120);
          var v_out_prm = v_request.BioParams.Where((p) => {
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
          var iofn = SrvUtl.bldiniFileName(this.BioSession.Cfg.IniPath, this.bioCode);
          var xslfn = iofn.ToLower().Replace(".xml", ".xsl");
          var cssfn = iofn.ToLower().Replace(".xml", ".css");
          if(File.Exists(xslfn)) {
            var rsltDoc = new dom4cs();
            rsltDoc.XmlDoc = dom4cs.CreXmlDocument(vDoc.ToString());
            var vElem = rsltDoc.XmlDoc.CreateElement("appurl");
            vElem.InnerText = this.BioSession.Cfg.AppURL;
            rsltDoc.XmlDoc.DocumentElement.AppendChild(vElem);
            vElem = rsltDoc.XmlDoc.CreateElement("biourl");
            vElem.InnerText = SrvUtl.bldIOPathUrl(this.BioSession.Cfg.AppURL, this.bioCode);
            rsltDoc.XmlDoc.DocumentElement.AppendChild(vElem);
            rsltDoc.WriteToStream(this.Context.Response.OutputStream, xslfn);
          } else {
            this.Context.Response.Write(vDoc.ToString());
          }
        } else {
          var vAgent = this.BioSession.CurSessionRemoteAgent;
          if(vAgent.ToUpper().StartsWith("DALPHA"))
            throw vEx;
          this.Context.Response.Write(vEx.ToString());
        }
        this.Context.Response.Flush();
      }
    }
  }
}
