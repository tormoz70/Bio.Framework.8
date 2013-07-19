namespace Bio.Framework.Server {

  using System;

  using System.Data;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Helpers.Common.Types;

  internal struct FieldDefs {
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

    public tmio_Tree(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }


    protected override void doExecute() {
      base.doExecute();

      var vTree = this.FBioDesc.DocumentElement;
      var fldDefs = new FieldDefs()
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
          fldDefs.ID_FLD = vTree.GetAttribute("idField");
        if(vTree.HasAttribute("textField"))
          fldDefs.TEXT_FLD = vTree.GetAttribute("textField");
        if(vTree.HasAttribute("leafField"))
          fldDefs.LEAF_FLD = vTree.GetAttribute("leafField");
        if(vTree.HasAttribute("clsField"))
          fldDefs.CLS_FLD = vTree.GetAttribute("clsField");
        if(vTree.HasAttribute("dataField"))
          fldDefs.DATA_FLD = vTree.GetAttribute("dataField");
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
              //      (String)Params.FindParamValue(this.bioParams, "org_uid"), (String)Params.FindParamValue(this.bioParams, "nodeid"),
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
