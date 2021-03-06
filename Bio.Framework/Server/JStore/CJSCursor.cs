using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml;
using Bio.Framework.Packets;
using Bio.Helpers.Common.Types;
using Bio.Helpers.DOA;
using Bio.Helpers.Common;
using System.Threading;

namespace Bio.Framework.Server {
  /// <summary>
  /// ���������� ������� � ��������� ��.
  /// </summary>
  public class CJSCursor : SQLCursor {
    /// <summary>
    /// ���� ��������, ����������� ���������.
    /// </summary>
    public enum CursorActions {
      /// <summary>
      /// ������� ������.
      /// </summary>
      caSelect,
      /// <summary>
      /// ������� ����� ������ ��� ��������� ������������.
      /// </summary>
      caInsertUpdate,
      /// <summary>
      /// ��������.
      /// </summary>
      caDelete
    };

    private String _bioCode;
    public XmlElement CursorIniDoc { get; private set; }

    public String bioCode {
      get {
        return this._bioCode;
      }
    }

    public CJSCursor(IDbConnection conn, XmlElement cursorIniDoc, String bioCode)
      : base(conn) {
      this._bioCode = bioCode;
      this.CursorIniDoc = cursorIniDoc;
    }

    public static XmlElement detectSQLTextElement(XmlElement pStoreElement, String pIOCode) {
      if ((pStoreElement != null) && pStoreElement.Name.Equals("store")) {
        XmlElement SQLelem = (XmlElement)pStoreElement.SelectSingleNode("SQL[@action='select']");
        if (SQLelem != null) {
          XmlElement SQLtext = (XmlElement)SQLelem.SelectSingleNode("text");
          if (SQLtext != null) {
            SQLtext.InnerText = SQLtext.InnerText.Trim();
            return SQLtext;
          }
        }
        throw new EBioException("� ��������� ������������� [" + pIOCode + "] �� ������� �������� ������� � ��������� action='select'.");

      } else
        throw new EBioException("� ��������� ������������� [" + pIOCode + "] �� ������ ������ <store>.");
    }

    private JsonStoreData _creJSData() {
      return new JsonStoreData {
        EndReached = true,
        IsFirstLoad = true,
        Limit = 0,
        MetaData = JsonStoreMetadata.ConstructMetadata(this.bioCode, this.CursorIniDoc),
        Rows = null,
        Start = 1,
        TotalCount = 0,
        Locate = null
      };
    }

    public virtual void Init(Params pBioParams) {
      this.Init(new JsonStoreRequestGet {
        BioParams = pBioParams,
        Packet = this._creJSData(),
        Sort = null,
        //nav = null
      });
    }

    private const String csLevel0SQLLeftComma = "/*#sql-0#{*/";
    private const String csLevel0SQLRightComma = "/*}#sql-0#*/";
    private void decomposePreparedSQL(ref String vPreparedSQLLevel0, ref String vPreparedSQLLevel1) {
      if (!String.IsNullOrEmpty(vPreparedSQLLevel0)) {
        int vLevel0Bgn = vPreparedSQLLevel0.IndexOf(csLevel0SQLLeftComma, StringComparison.CurrentCultureIgnoreCase);
        int vLevel0End = vPreparedSQLLevel0.IndexOf(csLevel0SQLRightComma, StringComparison.CurrentCultureIgnoreCase);
        if ((vLevel0Bgn >= 0) && (vLevel0End >= 0) && (vLevel0Bgn < vLevel0End)) {
          vPreparedSQLLevel1 = vPreparedSQLLevel0.Substring(0, vLevel0Bgn) + "{0}" +
                               vPreparedSQLLevel0.Substring(vLevel0End + csLevel0SQLRightComma.Length);
          vPreparedSQLLevel0 = vPreparedSQLLevel0.Substring(vLevel0Bgn + csLevel0SQLLeftComma.Length,
                                                            vLevel0End - (vLevel0Bgn + csLevel0SQLLeftComma.Length));
        }
      }

    }

    /// <summary>
    /// ������ ������� ��� ������������� ������ ����������
    /// </summary>
    //private const string csPgnSQLTemplate = "SELECT * FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq WHERE ROWNUM <= :rnumto$) WHERE rnum$ > :rnumfrom$";
    private const string csPgnSQLTemplate = "SELECT * FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) WHERE (rnum$ > :rnumfrom$) and (rnum$ <= :rnumto$)";
    private const string csPgnSQLTemplateGoToLast = "SELECT :rnumto$ as rnumto$, a.* FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) a WHERE a.rnum$ > :rnumfrom$";
    /// <summary>
    /// ������ ������� ��� �������� ������ ����� �������
    /// </summary>
    private const string csTotalCountSQLTemplate = "SELECT COUNT(1) FROM ({0})";
    private void _applyPagging(JsonStoreData pckt, Boolean decompositEnabled, ref Params bioParams, ref String vSQL, Int32 timeout) {
      if (bioParams == null)
        bioParams = new Params();
      // ��������� �� ��������
      Int64 vPgStart = pckt.Start;
      if (pckt.Limit > 0 || vPgStart == Int32.MaxValue && pckt.Limit > 0) {

        String vPreparedSQLLevel0 = vSQL;
        String vPreparedSQLLevel1 = null;
        if (decompositEnabled) {
          /* ���� �� ������� ������� ��� ����������, �������� ������ 0-��� ������.
           * P.S. ������ 0-��� ������ ��� ��������� ������, ������� ���������� ������� csLevel0SQLLeftComma � csLevel0SQLRightComma.
           * ���� �� ���������� ������� ��� ������, �� ������� ���������� �� �������� ����� ����������� ������ � ������� ��������
           * ������. ����� ��� �������� ��� ������...
           */
          this.decomposePreparedSQL(ref vPreparedSQLLevel0, ref vPreparedSQLLevel1);
        }

        String vPgnSQLTemplate = csPgnSQLTemplate;
        if (vPgStart == Int64.MaxValue) {
          vPgnSQLTemplate = csPgnSQLTemplateGoToLast;
          String vSQLStr = String.Format(csTotalCountSQLTemplate, vPreparedSQLLevel0);
          //string vSQLStr = String.Format(C_TotalCountSQLTemplate, this.preparedSQL);
          pckt.TotalCount = Convert.ToInt32(SQLCmd.ExecuteScalarSQL(this.Connection, vSQLStr, bioParams, timeout));
          vPgStart = ((pckt.TotalCount - 1L) / pckt.Limit) * pckt.Limit;
          pckt.Start = vPgStart;
        }
        Int64 vPgEnd = vPgStart + pckt.Limit;


        vPreparedSQLLevel0 = String.Format(vPgnSQLTemplate, vPreparedSQLLevel0);
        if (!String.IsNullOrEmpty(vPreparedSQLLevel1)) {
          vSQL = String.Format(vPreparedSQLLevel1, vPreparedSQLLevel0);
        } else
          vSQL = vPreparedSQLLevel0;

        //this.preparedSQL = String.Format(C_PgnSQLTemplate, vPgEnd - vPgStart + 1, this.preparedSQL);
        //this.preparedSQL = String.Format(C_PgnSQLTemplate, this.preparedSQL);

        bioParams.Add("rnumto$", vPgEnd + 1);
        bioParams.Add("rnumfrom$", vPgStart);
      }
    }

    /// <summary>
    /// ������ ������� ��� ��������� �������
    /// </summary>
    private const string csFltrSQLTemplate = "SELECT * FROM ({0}) WHERE {1}";
    private Boolean _applyFilter(JsonStoreFilter filter, ref Params bioParams, ref String vSQL) {
      // ���������
      String vCondition = null;
      if (filter != null) {
        var v_prms = new Params(); 
        filter.BuildSQLConditions(ref vCondition, v_prms);
        bioParams = v_prms.Merge(bioParams, true);
      }
      Boolean vFilterIsDefined = !String.IsNullOrEmpty(vCondition);
      if (vFilterIsDefined) {
        vSQL = String.Format(csFltrSQLTemplate, vSQL, vCondition);
        return true;
      }
      return false;
    }

    /// <summary>
    /// ������ ������� ��� ����������
    /// </summary>
    private const string csSortSQLTemplate = "SELECT * FROM ({0}) ORDER BY {1}{2}";
    private Boolean _applySorter(JsonStoreSort sort, ref String vSQL) {
      // ���������
      String vSort = (sort != null) ? sort.GetSQL() : null;
      //String vSortDir = (rq.sort != null) ? Utl.NameOfEnumValue<CJsonStoreSortOrder>(rq.sort.direction, false).ToUpper() : null;
      //Boolean vSorterIsDefined = (vSortField != null && vSortField != CJsonStoreMetadata.csPKField && vSortDir != null);
      Boolean vSorterIsDefined = !String.IsNullOrEmpty(vSort);
      if (vSorterIsDefined) {
        String pks = String.Empty;
        foreach (Field fld in this.PKFields.Values)
          pks += ", " + fld.FieldName;
        //vSQL = String.Format(csSortSQLTemplate, vSQL, vSortField + " " + vSortDir, pks);
        vSQL = String.Format(csSortSQLTemplate, vSQL, vSort, pks);
        return true;
      }
      return false;
    }

    /// <summary>
    /// ���������� ������ ��� ������ ������� ��������� � ��������� selection
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="bioParams"></param>
    /// <param name="vSQL"></param>
    private void _buildSelectSelectionSQL(String selection, Params bioParams, ref String vSQL) {
      String v_selection = selection;
      if (!String.IsNullOrEmpty(v_selection) && this.PKFields.Count > 0) {
        Boolean v_inversion = false;
        String v_pks = null;
        String[] v_parts = Utl.SplitString(v_selection, "||");
        if (v_parts.Length == 1)
          v_pks = v_parts[0];
        else if (v_parts.Length == 2) {
          v_inversion = String.Equals(v_parts[0], "1");
          v_pks = v_parts[1];
        }
        if (!String.IsNullOrEmpty(v_pks)) {
          var v_pk_fld = this.PKFields.FirstOrDefault().Value.FieldName;
          String v_not = v_inversion ? "not " : null;
          var v_cond = String.Format("{0} {1}in (select item from table(biosys.ai_utl.trans_list(:pklist4get, ';')))", v_pk_fld, v_not);
          vSQL = String.Format("select * from ({0}) where {1}", vSQL, v_cond);
          bioParams.SetValue("pklist4get", v_pks);
        } else {
          if (!v_inversion)
            vSQL = String.Format("select * from ({0}) where 1=2", vSQL);
        }
      } else
        vSQL = String.Format("select * from ({0}) where 1=2", vSQL);

    }

    /// <summary>
    /// ������ ������� ��� ������ ��������, ���������� ������ ������
    /// </summary>
    //private const string csLocateSQLTemplate = "SELECT rnum$ FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) WHERE {1}";
    private const string csLocateNextSQLTemplate = "SELECT rnum$ FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) WHERE {1} (rnum$ >= :loc_start_from)";
    //public CJsonStoreRequest JSRequest { get; private set; }


    //private CJsonStoreData _rq_packet = null;
    public JsonStoreData rqPacket { get; private set; }
    private Params _rq_bioParams;
    public Params rqBioParams { get { return _rq_bioParams; } }
    private JsonStoreFilter _rq_filter;
    private JsonStoreSort _rq_sorter;
    private String _rq_selection;
    public void Init(
      JsonStoreData packet,
      Params bioParams,
      JsonStoreFilter filter,
      JsonStoreSort sorter,
      String selection,
      Int32 timeout
    ) {

      this.rqPacket = packet;
      this._rq_bioParams = bioParams;
      this._rq_filter = filter;
      this._rq_sorter = sorter;
      this._rq_selection = selection;

      if (this.rqPacket == null)
        this.rqPacket = this._creJSData();
      XmlElement SQLtext = detectSQLTextElement(this.CursorIniDoc, this.bioCode);
      _applyParamsTypes((XmlElement)SQLtext.ParentNode, this.rqBioParams);
      this.InitCursorFields();
      if (this.rqBioParams == null)
        this._rq_bioParams = new Params();
      base.Init(SQLtext.InnerText, this.rqBioParams);
      String vSQL = this.preparedSQL;
      Boolean v_filterIsDefined = this._applyFilter(this._rq_filter, ref this._rq_bioParams, ref vSQL);
      Boolean v_sorterIsDefined = this._applySorter(this._rq_sorter, ref vSQL);

      if (String.IsNullOrEmpty(this._rq_selection)) {
        JsonStoreFilter vLocate = (this.rqPacket != null) ? this.rqPacket.Locate : null;
        if (vLocate != null) {
          // ���� ����������� ������
          var v_min_start = vLocate.FromPosition;
          String vSQLStr = null;
          var v_lprms = new Params();
          vLocate.BuildSQLConditions(ref vSQLStr, v_lprms);
          if (!String.IsNullOrEmpty(vSQLStr))
            vSQLStr = vSQLStr + " AND";
          v_lprms = v_lprms.Merge(this.rqBioParams, true);
          v_lprms.SetValue("loc_start_from", v_min_start);
          vSQLStr = String.Format(csLocateNextSQLTemplate, vSQL, vSQLStr);
          int rnum = Convert.ToInt32(SQLCmd.ExecuteScalarSQL(this.Connection, vSQLStr, v_lprms, timeout));
          if (this.rqPacket.Limit > 0)
            this.rqPacket.Start = Math.Max(((rnum - 1) / this.rqPacket.Limit) * this.rqPacket.Limit, 0);
        }
        this._applyPagging(this.rqPacket, !v_filterIsDefined && !v_sorterIsDefined, ref this._rq_bioParams, ref vSQL, timeout);
      } else {
        this._buildSelectSelectionSQL(this._rq_selection, this.rqBioParams, ref vSQL);
      }
      this.preparedSQL = vSQL;
    }

    /// <summary>
    /// �������������
    /// </summary>
    /// <param name="request">�������� web-�������</param>
    public virtual void Init(JsonStoreRequestGet request) {
      var v_rqget = request; 
      if (v_rqget == null)
        throw new Exception("��� ������� ���� ������� ��������� request ������ ���� ���� CJsonStoreRequestGet, � �� ����� " + request.GetType().Name + ".");
      this.Init(v_rqget.Packet, v_rqget.BioParams, v_rqget.Filter, v_rqget.Sort, v_rqget.selection, request.Timeout);
    }

    protected override void onBeforeOpen() {
      Thread.Sleep(100);
    }

    private static ParameterDirection _detectDir(XmlElement pParamDesc) {
      var result = ParameterDirection.Input;
      var pd = pParamDesc.GetAttribute("direction");
      if (!String.IsNullOrEmpty(pd)) {
        switch (pd) {
          case "Input":
            result = ParameterDirection.Input;
            break;
          case "Output":
            result = ParameterDirection.Output;
            break;
          case "InputOutput":
            result = ParameterDirection.InputOutput;
            break;
        }
      }
      return result;
    }


    /// <summary>
    /// ������������� � ���������� ����, ����������� � �������� ��������.
    /// </summary>
    /// <param name="sqlElement">XML-�������� �������.</param>
    /// <param name="prms">����� ����������, � ������� ���������� ����.</param>
    /// <exception cref="ArgumentNullException"></exception>
    private static void _applyParamsTypes(XmlElement sqlElement, Params prms) {
      if (sqlElement == null)
        throw new ArgumentNullException("sqlElement");
      if (prms != null) {
        foreach (XmlElement SQLParam in sqlElement.SelectNodes("param")) {
          var vParamName = SQLParam.GetAttribute("name");
          var vParamTypeName = SQLParam.GetAttribute("type");
          var vParamType = ftypeHelper.ConvertStrToType(vParamTypeName);

          var vDir = SQLUtils.EncodeParamDirection(_detectDir(SQLParam));
          var param = SQLUtils.FindParam(prms, vParamName);
          if (vDir == ParamDirection.Input) {
            if (param != null) {
              param.ParamType = vParamType;
              param.ParamDir = ParamDirection.Input;
            } else {
              param = new Param(vParamName, null, vParamType, ParamDirection.Input);
              prms.Add(param);
            }
          } else if ((vDir == ParamDirection.Output) || (vDir == ParamDirection.InputOutput)) {
            if (param == null) {
              param = new Param(vParamName, null);
              prms.Add(param);
            }
            param.ParamType = vParamType;
            param.InnerObject = vParamTypeName; // ��������� ��� ���� ���������. ��� �������� ������������ ��� ������������� ��������� ����������

            param.ParamDir = vDir;
            if (param.ParamType == typeof(String))
              param.ParamSize = 32000;
          }
        }
      }
    }

    private void _doExecute(Params prms, String actionName, Int32 timeout) {
      var sqlElement = (XmlElement)this.CursorIniDoc.SelectSingleNode("SQL[@action='" + actionName + "']");
      if (sqlElement != null) {
        var sqLtext = (XmlElement)sqlElement.SelectSingleNode("text");
        if (sqLtext != null) {
          sqLtext.InnerText = sqLtext.InnerText.Trim();
          try {
            _applyParamsTypes(sqlElement, prms);
            var cmd = PrepareCommand(this.Connection, sqLtext.InnerText, prms, timeout);
            ExecuteScript(cmd, sqLtext.InnerText, prms);
          } catch (EBioException be) {
            throw new EBioException("��� ���������� PL/SQL ����� " + this.bioCode + "[" + actionName + "] ��������� ������.\n���������: " + be.Message, be);
          }
        }
      } else {
        throw new EBioException("� ��������� ������������� [" + this.bioCode + "] �� ������� �������� ������� � ��������� action='" + actionName + "'.");
      }
    }

    /// <summary>
    /// �������������� SQL.
    /// </summary>
    /// <exception cref="EBioException">������������, ����� �������� ����������� � �������.</exception>
    public IDbCommand DoPrepareCommand(Params prms, ref String sql, Int32 timeout) {
      String csActionName = "execute";
      IDbCommand stmt = null;
      var SQLelem = (XmlElement)this.CursorIniDoc.SelectSingleNode("SQL[@action='" + csActionName + "']");
      if (SQLelem != null) {
        var SQLtext = (XmlElement)SQLelem.SelectSingleNode("text");
        if (SQLtext != null) {
          sql = SQLtext.InnerText.Trim();
          try {
            _applyParamsTypes(SQLelem, prms);
            stmt = SQLCmd.PrepareCommand(this.Connection, sql, prms, timeout);
          } catch (EBioException be) {
            throw new EBioException("��� ���������� PL/SQL ����� " + this.bioCode + "[" + csActionName + "] ��������� ������.\n���������: " + be.Message, be);
          }
        }
      } else {
        throw new EBioException("� ��������� ������������� [" + this.bioCode + "] �� ������� �������� ������� � ��������� action='" + csActionName + "'.");
      }
      return stmt;
    }

    /*
    /// <summary>
    /// ������� ������ �� �������� ���������� �����.
    /// </summary>
    /// <exception cref="EBioException">������������, ����� �������� ����������� � �������.</exception>
    public void DoDelete(Params @params) {
      this.doExecute(@params, "delete");
    }

    /// <summary>
    /// ���������/�������� ������ �� �������� ���������� �����.
    /// </summary>
    /// <exception cref="EBioException">������������, ����� �������� ����������� � �������.</exception>
    public void DoInsertUpdate(Params @params) {
      this.doExecute(@params, "insertupdate");
    }
    */

    private Params _buildPostParams(JsonStoreMetadata metadata, JsonStoreRow row, Params bioParams) {
      Params v_rslt = new Params();
      for (int i = 0; i < metadata.Fields.Count; i++)
        v_rslt.Add(metadata.Fields[i].Name.ToLower(), row.Values[i]);
      v_rslt = v_rslt.Merge(bioParams, false);
      return v_rslt;

    }

    private void _returnParamsToRow(JsonStoreMetadata metadata, JsonStoreRow row, Params bioParams) {
      var v_out_params = bioParams.Where(p => { return (p.ParamDir != ParamDirection.Input)/* || String.Equals(p.Name, CJsonStoreMetadata.csPKFieldName)*/; });
      foreach (var p in v_out_params)
        row.Values[metadata.IndexOf(p.Name)] = p.Value;
    }

    /// <summary>
    /// ������������ ��������� � ������
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="row"></param>
    /// <param name="bioParams"></param>
    /// <param name="timeout"/>
    public void DoProcessRowPost(JsonStoreMetadata metadata, JsonStoreRow row, Params bioParams, Int32 timeout) {
      var v_params = this._buildPostParams(metadata, row, bioParams);
      if ((row.ChangeType == JsonStoreRowChangeType.Added) ||
          (row.ChangeType == JsonStoreRowChangeType.Modified)) {
            this._doExecute(v_params, "insertupdate", timeout);
        this._returnParamsToRow(metadata, row, v_params);
      } else if (row.ChangeType == JsonStoreRowChangeType.Deleted)
        this._doExecute(v_params, "delete", timeout);
    }

    /// <summary>
    /// ��������� SQL.
    /// </summary>
    /// <exception cref="EBioException">������������, ����� �������� ����������� � �������.</exception>
    public void DoExecuteSQL(Params prms, Int32 timeout) {
      this._doExecute(prms, "execute", timeout);
    }

    /// <summary>
    /// ��������� ������ � ����������� �� ������
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="row"></param>
    /// <param name="bioParams"></param>
    /// <param name="timeout"/>
    public void DoExecuteSQL(JsonStoreMetadata metadata, JsonStoreRow row, Params bioParams, Int32 timeout) {
      var v_params = this._buildPostParams(metadata, row, bioParams);
      this._doExecute(v_params, "execute", timeout);
    }

    /// <summary>
    /// ������ ���� �� �������� ������� �� ��������.
    /// </summary>
    protected override void InitCursorFields() {
      var v_flds = this.CursorIniDoc.SelectNodes("fields/field");
      if (v_flds != null) {
        var v_fieldIndx = 1;
        this.PKFields.Clear();
        foreach (XmlElement celem in v_flds) {
          var v_fieldName = celem.GetAttribute("name");
          if (!v_fieldName.Equals(Field.FIELD_RNUM)) {
            var v_fieldType = ftypeHelper.ConvertStrToFieldType(celem.GetAttribute("type"));
            var v_fieldEncoding = FieldEncoding.UTF8;
            if(celem.HasAttribute("encoding"))
              v_fieldEncoding = ftypeHelper.ConvertStrToFieldEncoding(celem.GetAttribute("encoding"));
            var v_pkIndex = celem.GetAttribute("pk");
            var v_fieldCaption = celem.InnerText;
            var v_newFld = new Field(this, v_fieldIndx, v_fieldName, v_fieldType, v_fieldCaption, v_pkIndex, v_fieldEncoding);
            if (!celem.HasAttribute("generate") || celem.GetAttribute("generate") == "true")
              this.Fields.Add(v_newFld);
            if (!v_pkIndex.Equals(""))
              this.PKFields.Add(v_pkIndex, v_newFld);
            v_fieldIndx++;
          }
        }
      }
    }

    protected override void doOnAfterOpen() { }

    ///// <summary>
    ///// ����������� ������ �� ���������� ���������� ����� � ���������.
    ///// </summary>
    ///// <param name="pkString">������ �� ���������� ���������� �����.</param>
    ///// <returns></returns>
    //private Params PKtoParams(string pkString) {
    //  string[] pkValues = null;
    //  if (pkString != null)
    //    pkValues = Utl.SplitString(pkString.Trim('(', ')'), ")-(");
    //  Params vParams = new Params();
    //  /*if ((pkValues != null) || (this.PKFields.Count == pkValues.Length)) {*/
    //  for (int i = 0; i < this.PKFields.Count; i++) {
    //    String vValue = null;
    //    if ((pkValues != null) && (i < pkValues.Length))
    //      vValue = pkValues[i];
    //    vParams.Add(this.PKFields.Values[i].FieldName, vValue);
    //  }
    //  /*} else
    //    throw new ebio.EBioException("�������� ���������� ����� �� ��������� � ��������� ����� ���������� �����.");*/
    //  /* by AI 20080624. */
    //  return vParams;
    //}

    protected override void prepareSQL() {
      base.prepareSQL();
    }

    protected override void throwOpenError(IDbConnection connection, Exception ex, String sql, String @params) {
      throw new EBioException("[" + detectDBName(connection.ConnectionString) + "] ������ ��� �������� ������� [" + this.bioCode + "].\r\n���������: " + ex.Message + "\r\nSQL: " + this.DbCommand.CommandText + "\r\n" + "��������� �������:{" + @params + "}", ex);
    }

  }
}
