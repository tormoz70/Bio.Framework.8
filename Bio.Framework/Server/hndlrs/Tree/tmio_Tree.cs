namespace Bio.Framework.Server {

  using System;

  using System.Data;
  using System.Data.Common;

  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Helpers.Common.Types;

  internal struct sFieldDefs {
    public String ID_FLD;
    public String TEXT_FLD;
    public String LEAF_FLD;
    public String CLS_FLD;
    public String DATA_FLD;
    public String ICON_FLD;
  }
  /// <summary>
  /// Данный обработчик вх. сообщений является шаблоном для создания нового сообщения
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_Tree:ABioHandlerBio {

    public tmio_Tree(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    //private void loadChildren(
    //                DbConnection pConn,
    //                XmlElement pDS,
    //                sFieldDefs vFldDefs,
    //                Boolean pFullLoad,
    //                String pOrgUid, String pNodeUid,
    //                LitJson_killd.JsonWriter jw) {
    //  CSQLCursorBio vCursor = new CSQLCursorBio(pConn, pDS, this.bioCode);
    //  CParams vPrms = new CParams(new CParam("org_uid", pOrgUid), new CParam("nodeid", pNodeUid));
    //  vCursor.Init(vPrms);
    //  vCursor.Open();
    //  jw.WriteArrayStart();
    //  try {
    //    while(vCursor.Next()) {
    //      jw.WriteObjectStart();
    //      try {
    //        Field vIdFld = vCursor.FieldByName(vFldDefs.ID_FLD);
    //        Field vTxtFld = vCursor.FieldByName(vFldDefs.TEXT_FLD);
    //        Field vLeafFld = vCursor.FieldByName(vFldDefs.LEAF_FLD);
    //        Field vClsFld = vCursor.FieldByName(vFldDefs.CLS_FLD);
    //        if(vIdFld == null)
    //          throw new EBioException("В описании <store> объекта [" + this.bioCode + "] должно быть объявлено поле \"" + vFldDefs.ID_FLD + "\".");
    //        if(vTxtFld == null)
    //          throw new EBioException("В описании <store> объекта [" + this.bioCode + "] должно быть объявлено поле \"" + vFldDefs.TEXT_FLD + "\".");
    //        if(vLeafFld == null)
    //          throw new EBioException("В описании <store> объекта [" + this.bioCode + "] должно быть объявлено поле \"" + vFldDefs.LEAF_FLD + "\".");
    //        if(vClsFld == null)
    //          throw new EBioException("В описании <store> объекта [" + this.bioCode + "] должно быть объявлено поле \"" + vFldDefs.CLS_FLD + "\".");

    //        jw.WritePropertyName("id").Write(vIdFld.AsString);
    //        jw.WritePropertyName("text").Write(vTxtFld.AsString);
    //        Boolean vLeaf = vLeafFld.AsInteger > 0;
    //        jw.WritePropertyName("leaf").Write(vLeaf);
    //        jw.WritePropertyName("cls").Write(vClsFld.AsString);
    //        Field vDataFld = vCursor.FieldByName(vFldDefs.DATA_FLD);
    //        if(vDataFld != null) {
    //          String vJSO = vDataFld.AsString;
    //          jw.WritePropertyName("data").Write(vJSO);
    //        }
    //        Field vIconFld = vCursor.FieldByName(vFldDefs.ICON_FLD);
    //        if(vIconFld != null) {
    //          String vIconData = vIconFld.AsString;
    //          jw.WritePropertyName("icon").Write(this.CurIOLocalUrl + vIconData);
    //        }
    //        if(pFullLoad) {
    //          //jw.WritePropertyName("attributes");
    //          //jw.WriteObjectStart();
    //          jw.WritePropertyName("children");
    //          this.loadChildren(
    //                pConn,
    //                pDS,
    //                vFldDefs,
    //                pFullLoad,
    //                pOrgUid, vIdFld.AsString,
    //                jw);
    //          //jw.WriteObjectEnd();
    //        }
    //      } finally {
    //        jw.WriteObjectEnd();
    //      }
    //    }
    //  } finally {
    //    jw.WriteArrayEnd();
    //  }
    //}

    protected override void doExecute() {
      base.doExecute();

      XmlElement vTree = (XmlElement)this.FBioDesc.DocumentElement;
      sFieldDefs vFldDefs = new sFieldDefs()
      {
        ID_FLD = "f_uid",
        TEXT_FLD = "f_text",
        LEAF_FLD = "f_leaf",
        CLS_FLD = "f_cls",
        DATA_FLD = "f_data",
        ICON_FLD = "f_icon"
      };
      if(vTree != null) {
        if(vTree.HasAttribute("idField"))
          vFldDefs.ID_FLD = vTree.GetAttribute("idField");
        if(vTree.HasAttribute("textField"))
          vFldDefs.TEXT_FLD = vTree.GetAttribute("textField");
        if(vTree.HasAttribute("leafField"))
          vFldDefs.LEAF_FLD = vTree.GetAttribute("leafField");
        if(vTree.HasAttribute("clsField"))
          vFldDefs.CLS_FLD = vTree.GetAttribute("clsField");
        if(vTree.HasAttribute("dataField"))
          vFldDefs.DATA_FLD = vTree.GetAttribute("dataField");
      }
      XmlElement vDS = this.FBioDesc.DocumentElement;
      String vResult = null;
      using(IDbConnection vConn = this.BioSession.Cfg.dbSession.GetConnection()) {
        try {
          if(vDS != null) {

            using(StringWriter sw = new StringWriter()) {
              //LitJson_killd.JsonWriter jw = new LitJson_killd.JsonWriter(sw);
              Boolean vFullLoad = false;
              if(this.bioParams.ParamByName("fullLoad") != null)
                vFullLoad = this.bioParams.ParamByName("fullLoad").ValueAsString().ToLower() == "true";
              //this.loadChildren(
              //      vConn, vDS, vFldDefs, vFullLoad,
              //      (String)CParams.FindParamValue(this.bioParams, "org_uid"), (String)CParams.FindParamValue(this.bioParams, "nodeid"),
              //      jw);
              vResult = sw.ToString();
            }
            this.Context.Response.Write(vResult);
          }
        } finally {
          if(vConn != null)
            vConn.Close();
        }
      }
    }
  }
}
