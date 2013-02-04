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
  /// Реализация Курсора в контексте ИО.
  /// </summary>
  public class CJSCursor : SQLCursor {
    /// <summary>
    /// Типы действий, совершаемых запросами.
    /// </summary>
    public enum CursorActions {
      /// <summary>
      /// Выборка данных.
      /// </summary>
      caSelect,
      /// <summary>
      /// Вставка новой записи или изменение существующей.
      /// </summary>
      caInsertUpdate,
      /// <summary>
      /// Удаление.
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
        throw new EBioException("В документе инициализации [" + pIOCode + "] не найдено описание запроса с атрибутом action='select'.");

      } else
        throw new EBioException("В документе инициализации [" + pIOCode + "] не найден раздел <store>.");
    }

    private CJsonStoreData _creJSData() {
      return new CJsonStoreData {
        endReached = true,
        isFirstLoad = true,
        limit = 0,
        metaData = CJsonStoreMetadata.ConstructMetadata(this.bioCode, this.CursorIniDoc),
        rows = null,
        start = 1,
        totalCount = 0,
        locate = null
      };
    }

    public virtual void Init(CParams pBioParams) {
      this.Init(new CJsonStoreRequestGet {
        bioParams = pBioParams,
        packet = this._creJSData(),
        sort = null,
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
    /// Шаблон запроса для постраничного вывода результата
    /// </summary>
    //private const string csPgnSQLTemplate = "SELECT * FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq WHERE ROWNUM <= :rnumto$) WHERE rnum$ > :rnumfrom$";
    private const string csPgnSQLTemplate = "SELECT * FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) WHERE (rnum$ > :rnumfrom$) and (rnum$ <= :rnumto$)";
    private const string csPgnSQLTemplateGoToLast = "SELECT :rnumto$ as rnumto$, a.* FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) a WHERE a.rnum$ > :rnumfrom$";
    /// <summary>
    /// Шаблон запроса для подсчёта общего числа записей
    /// </summary>
    private const string csTotalCountSQLTemplate = "SELECT COUNT(1) FROM ({0})";
    private void _applyPagging(CJsonStoreData pckt, Boolean decompositEnabled, ref CParams bioParams, ref String vSQL, Int32 timeout) {
      if (bioParams == null)
        bioParams = new CParams();
      // разбиваем на страницы
      Int64 vPgStart = pckt.start;
      if (pckt.limit > 0 || vPgStart == Int32.MaxValue && pckt.limit > 0) {

        String vPreparedSQLLevel0 = vSQL;
        String vPreparedSQLLevel1 = null;
        if (decompositEnabled) {
          /* Если не включен Фильтер или сортировка, выделяем запрос 0-ого уровня.
           * P.S. запрос 0-ого уровня это вложенный запрос, который помечается метками csLevel0SQLLeftComma и csLevel0SQLRightComma.
           * Если не установлен фильтер или сортер, то условие разделения на страницы будет приметяться именно к запросу нулевого
           * уровня. Иначе все работает как прежде...
           */
          this.decomposePreparedSQL(ref vPreparedSQLLevel0, ref vPreparedSQLLevel1);
        }

        String vPgnSQLTemplate = csPgnSQLTemplate;
        if (vPgStart == Int64.MaxValue) {
          vPgnSQLTemplate = csPgnSQLTemplateGoToLast;
          String vSQLStr = String.Format(csTotalCountSQLTemplate, vPreparedSQLLevel0);
          //string vSQLStr = String.Format(C_TotalCountSQLTemplate, this.preparedSQL);
          pckt.totalCount = Convert.ToInt32(SQLCmd.ExecuteScalarSQL(this.Connection, vSQLStr, bioParams, timeout));
          vPgStart = ((pckt.totalCount - 1L) / pckt.limit) * pckt.limit;
          pckt.start = vPgStart;
        }
        Int64 vPgEnd = vPgStart + pckt.limit;


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
    /// Шаблон запроса для наложения фильтра
    /// </summary>
    private const string csFltrSQLTemplate = "SELECT * FROM ({0}) WHERE {1}";
    private Boolean _applyFilter(CJsonStoreFilter filter, ref CParams bioParams, ref String vSQL) {
      // фильтруем
      String vCondition = null;
      if (filter != null) {
        var v_prms = new CParams(); 
        filter.buildSQLConditions(ref vCondition, v_prms);
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
    /// Шаблон запроса для сортировки
    /// </summary>
    private const string csSortSQLTemplate = "SELECT * FROM ({0}) ORDER BY {1}{2}";
    private Boolean _applySorter(CJsonStoreSort sort, ref String vSQL) {
      // сортируем
      String vSort = (sort != null) ? sort.GetSQL() : null;
      //String vSortDir = (rq.sort != null) ? Utl.NameOfEnumValue<CJsonStoreSortOrder>(rq.sort.direction, false).ToUpper() : null;
      //Boolean vSorterIsDefined = (vSortField != null && vSortField != CJsonStoreMetadata.csPKField && vSortDir != null);
      Boolean vSorterIsDefined = !String.IsNullOrEmpty(vSort);
      if (vSorterIsDefined) {
        String pks = String.Empty;
        foreach (CField fld in this.PKFields.Values)
          pks += ", " + fld.FieldName;
        //vSQL = String.Format(csSortSQLTemplate, vSQL, vSortField + " " + vSortDir, pks);
        vSQL = String.Format(csSortSQLTemplate, vSQL, vSort, pks);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает запрос для выбора записей указанных в параметре selection
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="bioParams"></param>
    /// <param name="vSQL"></param>
    private void _buildSelectSelectionSQL(String selection, CParams bioParams, ref String vSQL) {
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
    /// Шаблон запроса для поиска страницы, содержащей нужную запись
    /// </summary>
    //private const string csLocateSQLTemplate = "SELECT rnum$ FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) WHERE {1}";
    private const string csLocateNextSQLTemplate = "SELECT rnum$ FROM (SELECT innerq.*, ROWNUM rnum$ FROM ({0}) innerq) WHERE {1} (rnum$ >= :loc_start_from)";
    //public CJsonStoreRequest JSRequest { get; private set; }


    //private CJsonStoreData _rq_packet = null;
    public CJsonStoreData rqPacket { get; private set; }
    private CParams _rq_bioParams = null;
    public CParams rqBioParams { get { return _rq_bioParams; } }
    private CJsonStoreFilter _rq_filter = null;
    private CJsonStoreSort _rq_sorter = null;
    private String _rq_selection = null;
    public void Init(
      CJsonStoreData packet,
      CParams bioParams,
      CJsonStoreFilter filter,
      CJsonStoreSort sorter,
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
      this.ApplyParamsTypes((XmlElement)SQLtext.ParentNode, this.rqBioParams);
      this.InitCursorFields();
      if (this.rqBioParams == null)
        this._rq_bioParams = new CParams();
      base.Init(SQLtext.InnerText, this.rqBioParams);
      String vSQL = this.preparedSQL;
      Boolean v_filterIsDefined = this._applyFilter(this._rq_filter, ref this._rq_bioParams, ref vSQL);
      Boolean v_sorterIsDefined = this._applySorter(this._rq_sorter, ref vSQL);

      if (String.IsNullOrEmpty(this._rq_selection)) {
        CJsonStoreFilter vLocate = (this.rqPacket != null) ? this.rqPacket.locate : null;
        if (vLocate != null) {
          // ищем запрошенную запись
          var v_min_start = vLocate.fromPosition;
          String vSQLStr = null;
          var v_lprms = new CParams();
          vLocate.buildSQLConditions(ref vSQLStr, v_lprms);
          if (!String.IsNullOrEmpty(vSQLStr))
            vSQLStr = vSQLStr + " AND";
          v_lprms = v_lprms.Merge(this.rqBioParams, true);
          v_lprms.SetValue("loc_start_from", v_min_start);
          vSQLStr = String.Format(csLocateNextSQLTemplate, vSQL, vSQLStr);
          int rnum = Convert.ToInt32(SQLCmd.ExecuteScalarSQL(this.Connection, vSQLStr, v_lprms, timeout));
          if (this.rqPacket.limit > 0)
            this.rqPacket.start = Math.Max(((rnum - 1) / this.rqPacket.limit) * this.rqPacket.limit, 0);
        }
        this._applyPagging(this.rqPacket, !v_filterIsDefined && !v_sorterIsDefined, ref this._rq_bioParams, ref vSQL, timeout);
      } else {
        this._buildSelectSelectionSQL(this._rq_selection, this.rqBioParams, ref vSQL);
      }
      this.preparedSQL = vSQL;
    }

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="request">Описание web-запроса</param>
    /// <param name="selection">Фильтр по первичному ключу</param>
    public virtual void Init(CJsonStoreRequestGet request) {
      var v_rqget = request; 
      if (v_rqget == null)
        throw new Exception("Для данного вида запроса параметер request должен быть типа CJsonStoreRequestGet, а на входе " + request.GetType().Name + ".");
      this.Init(v_rqget.packet, v_rqget.bioParams, v_rqget.filter, v_rqget.sort, v_rqget.selection, request.timeout);
    }

    protected override void onBeforeOpen() {
      Thread.Sleep(100);
    }

    private System.Data.ParameterDirection detectDir(XmlElement pParamDesc) {
      System.Data.ParameterDirection vResult = System.Data.ParameterDirection.Input;
      String pd = pParamDesc.GetAttribute("direction");
      if (!String.IsNullOrEmpty(pd)) {
        switch (pd) {
          case "Input":
            vResult = System.Data.ParameterDirection.Input;
            break;
          case "Output":
            vResult = System.Data.ParameterDirection.Output;
            break;
          case "InputOutput":
            vResult = System.Data.ParameterDirection.InputOutput;
            break;
          /*case "ReturnValue":
          vResult = System.Data.ParameterDirection.ReturnValue;
          break;*/
        }
      }
      return vResult;
    }


    /// <summary>
    /// Устанавливает у параметров типы, прописанные в описании ИОбъекта.
    /// </summary>
    /// <param name="sqlElement">XML-описание запроса.</param>
    /// <param name="prms">Набор параметров, в которые пропишутся типы.</param>
    /// <exception cref="ArgumentNullException"></exception>
    private void ApplyParamsTypes(XmlElement sqlElement, CParams prms) {
      if (sqlElement == null)
        throw new ArgumentNullException("sqlElement");
      if (prms != null) {
        foreach (XmlElement SQLParam in sqlElement.SelectNodes("param")) {
          String vParamName = SQLParam.GetAttribute("name");
          String vParamTypeName = SQLParam.GetAttribute("type");
          //Type vParamType;
          //if(String.Equals(vParamTypeName, "clob", StringComparison.CurrentCultureIgnoreCase))
          //  vParamType = typeof(OracleClob);
          ////else if (String.Equals(vParamTypeName, "blob", StringComparison.CurrentCultureIgnoreCase))
          ////  vParamType = typeof(OracleBlob);
          //else
          Type vParamType = ftypeHelper.ConvertStrToType(vParamTypeName);
          ParamDirection vDir = SQLUtils.EncodeParamDirection(this.detectDir(SQLParam));
          CParam param = SQLUtils.FindParam(prms, vParamName);
          if (vDir == ParamDirection.Input) {
            if (param != null) {
              param.ParamType = vParamType;
              param.ParamDir = ParamDirection.Input;
            } else {
              param = new CParam(vParamName, null, vParamType, ParamDirection.Input);
              prms.Add(param);
            }
          } else if ((vDir == ParamDirection.Output) || (vDir == ParamDirection.InputOutput)) {
            if (param == null) {
              param = new CParam(vParamName, null);
              prms.Add(param);
            }
            param.ParamType = vParamType;
            param.ParamDir = vDir;
            if (param.ParamType.Equals(typeof(System.String)))
              param.ParamSize = 32000;
          }
          /*if((param != null) && ((vDir == ParamDirection.Input) || (vDir == ParamDirection.InputOutput))) {
            if (!String.IsNullOrEmpty(param.ValueAsString())) {
              if(param.ParamType.Equals(typeof(System.Decimal)))
                param.Value = SQLUtils.StrAsOraValue(param.ValueAsString(), OracleDataType.Varchar2);
              else if(param.ParamType.Equals(typeof(System.DateTime)))
                param.Value = SQLUtils.StrAsOraValue(param.ValueAsString(), OracleDataType.Date);
              //else if (param.ParamType.Equals(typeof(System.Char[])))
              //  param.Value = SQLUtils.StrAsOraValue(param.Value, OracleDbType.Clob);
              else if (param.ParamType.Equals(typeof(System.Byte[])))
                param.Value = SQLUtils.StrAsOraValue(param.ValueAsString(), OracleDataType.Blob);
            }
          }*/

        }
      }
    }

    private void _doExecute(CParams prms, String actionName, Int32 timeout) {
      var SQLelem = (XmlElement)this.CursorIniDoc.SelectSingleNode("SQL[@action='" + actionName + "']");
      if (SQLelem != null) {
        var SQLtext = (XmlElement)SQLelem.SelectSingleNode("text");
        if (SQLtext != null) {
          SQLtext.InnerText = SQLtext.InnerText.Trim();
          try {
            this.ApplyParamsTypes(SQLelem, prms);
            var cmd = SQLCmd.PrepareCommand(this.Connection, SQLtext.InnerText, prms, timeout);
            SQLCmd.ExecuteScript(cmd, SQLtext.InnerText, prms);
          } catch (EBioException be) {
            throw new EBioException("При выполнении PL/SQL блока " + this.bioCode + "[" + actionName + "] произошла ошибка.\nСообщение: " + be.Message, be);
          }
        }
      } else {
        throw new EBioException("В документе инициализации [" + this.bioCode + "] не найдено описание запроса с атрибутом action='" + actionName + "'.");
      }
    }

    /// <summary>
    /// Подготавливает SQL.
    /// </summary>
    /// <exception cref="EBioException">Возбуждается, когда операция завершилась с ошибкой.</exception>
    public IDbCommand DoPrepareCommand(CParams prms, ref String sql, Int32 timeout) {
      String csActionName = "execute";
      IDbCommand stmt = null;
      var SQLelem = (XmlElement)this.CursorIniDoc.SelectSingleNode("SQL[@action='" + csActionName + "']");
      if (SQLelem != null) {
        var SQLtext = (XmlElement)SQLelem.SelectSingleNode("text");
        if (SQLtext != null) {
          sql = SQLtext.InnerText.Trim();
          try {
            this.ApplyParamsTypes(SQLelem, prms);
            stmt = SQLCmd.PrepareCommand(this.Connection, sql, prms, timeout);
          } catch (EBioException be) {
            throw new EBioException("При подготовке PL/SQL блока " + this.bioCode + "[" + csActionName + "] произошла ошибка.\nСообщение: " + be.Message, be);
          }
        }
      } else {
        throw new EBioException("В документе инициализации [" + this.bioCode + "] не найдено описание запроса с атрибутом action='" + csActionName + "'.");
      }
      return stmt;
    }

    /*
    /// <summary>
    /// Удаляет запись по признаку первичного ключа.
    /// </summary>
    /// <exception cref="EBioException">Возбуждается, когда операция завершилась с ошибкой.</exception>
    public void DoDelete(CParams @params) {
      this.doExecute(@params, "delete");
    }

    /// <summary>
    /// Добавляет/Изменяет запись по признаку первичного ключа.
    /// </summary>
    /// <exception cref="EBioException">Возбуждается, когда операция завершилась с ошибкой.</exception>
    public void DoInsertUpdate(CParams @params) {
      this.doExecute(@params, "insertupdate");
    }
    */

    private CParams _buildPostParams(CJsonStoreMetadata metadata, CJsonStoreRow row, CParams bioParams) {
      CParams v_rslt = new CParams();
      for (int i = 0; i < metadata.fields.Count; i++)
        v_rslt.Add(metadata.fields[i].name.ToLower(), row.Values[i]);
      v_rslt = v_rslt.Merge(bioParams, false);
      return v_rslt;

    }

    private void _returnParamsToRow(CJsonStoreMetadata metadata, CJsonStoreRow row, CParams bioParams) {
      var v_out_params = bioParams.Where(p => { return (p.ParamDir != ParamDirection.Input)/* || String.Equals(p.Name, CJsonStoreMetadata.csPKFieldName)*/; });
      foreach (var p in v_out_params)
        row.Values[metadata.indexOf(p.Name)] = p.Value;
    }

    /// <summary>
    /// Обрабатывает изменения в данных
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="row"></param>
    /// <param name="bioParams"></param>
    public void DoProcessRowPost(CJsonStoreMetadata metadata, CJsonStoreRow row, CParams bioParams, Int32 timeout) {
      var v_params = this._buildPostParams(metadata, row, bioParams);
      if ((row.changeType == CJsonStoreRowChangeType.Added) ||
          (row.changeType == CJsonStoreRowChangeType.Modified)) {
            this._doExecute(v_params, "insertupdate", timeout);
        this._returnParamsToRow(metadata, row, v_params);
      } else if (row.changeType == CJsonStoreRowChangeType.Deleted)
        this._doExecute(v_params, "delete", timeout);
    }

    /// <summary>
    /// Выполняет SQL.
    /// </summary>
    /// <exception cref="EBioException">Возбуждается, когда операция завершилась с ошибкой.</exception>
    public void DoExecuteSQL(CParams prms, Int32 timeout) {
      this._doExecute(prms, "execute", timeout);
    }

    /// <summary>
    /// Выполняет скрипт с параметрами из строки
    /// </summary>
    /// <param name="metadata"></param>
    /// <param name="row"></param>
    /// <param name="bioParams"></param>
    public void DoExecuteSQL(CJsonStoreMetadata metadata, CJsonStoreRow row, CParams bioParams, Int32 timeout) {
      var v_params = this._buildPostParams(metadata, row, bioParams);
      this._doExecute(v_params, "execute", timeout);
    }

    /// <summary>
    /// Создаёт поля по описанию курсора из ИОбъекта.
    /// </summary>
    protected override void InitCursorFields() {
      XmlNodeList flds = this.CursorIniDoc.SelectNodes("fields/field");
      if (flds != null) {
        //this.creRNUMFld();
        int fIndx = 1;
        this.PKFields.Clear();
        foreach (XmlElement celem in flds) {
          String fname = celem.GetAttribute("name");
          if (!fname.Equals(CField.FIELD_RNUM)) {
            //String ftype = TField.ConvertToCompatible(celem.GetAttribute("type"));
            CFieldType ftype = ftypeHelper.ConvertStrToFType(celem.GetAttribute("type"));
            String fpkindx = celem.GetAttribute("pk");
            String fcaption = celem.InnerText;
            CField newFld = new CField(this, fIndx, fname, ftype, fcaption, fpkindx);
            if (!celem.HasAttribute("generate") || celem.GetAttribute("generate") == "true")
              this.Fields.Add(newFld);
            if (!fpkindx.Equals(""))
              this.PKFields.Add(fpkindx, newFld);
            fIndx++;
          }
        }
      }
    }

    protected override void onAfterOpen() { }

    ///// <summary>
    ///// Преобразует строку со значениями первичного ключа в параметры.
    ///// </summary>
    ///// <param name="pkString">Строка со значениями первичного ключа.</param>
    ///// <returns></returns>
    //private CParams PKtoParams(string pkString) {
    //  string[] pkValues = null;
    //  if (pkString != null)
    //    pkValues = Utl.SplitString(pkString.Trim('(', ')'), ")-(");
    //  CParams vParams = new CParams();
    //  /*if ((pkValues != null) || (this.PKFields.Count == pkValues.Length)) {*/
    //  for (int i = 0; i < this.PKFields.Count; i++) {
    //    String vValue = null;
    //    if ((pkValues != null) && (i < pkValues.Length))
    //      vValue = pkValues[i];
    //    vParams.Add(this.PKFields.Values[i].FieldName, vValue);
    //  }
    //  /*} else
    //    throw new ebio.EBioException("Значения первичного ключа не совпадают с описанием полей первичного ключа.");*/
    //  /* by AI 20080624. */
    //  return vParams;
    //}

    protected override void prepareSQL() {
      base.prepareSQL();
    }

    protected override void throwOpenError(IDbConnection connection, Exception ex, String sql, String @params) {
      throw new EBioException("[" + detectDBName(connection.ConnectionString) + "] Ошибка при открытии курсора [" + this.bioCode + "].\r\nСообщение: " + ex.Message + "\r\nSQL: " + this.DbCommand.CommandText + "\r\n" + "Параметры запроса:{" + @params + "}", ex);
    }

  }
}
