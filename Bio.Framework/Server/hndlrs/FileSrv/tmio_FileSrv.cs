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
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;
  using Bio.Framework.Packets;

  /// <summary>
  /// Данный обработчик вх. сообщений является шаблоном для создания нового сообщения
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_FileSrv : ABioHandlerBio {

    public tmio_FileSrv(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    private const String CS_FILE_NAME_PARAM = "v_file_name";
    private const String CS_FILE_PARAM = "v_file";
    private const String CS_HASH_CODE_WEB_PARAM = "hf";
    /// <summary>
    /// Вытаскивает из БД файл и отдает его клиенту
    /// </summary>
    /// <param name="bioPrms">Параметры запроса к БД</param>
    private void _sendFileToClient(Params bioPrms) {

      this.Context.Response.ClearContent();
      this.Context.Response.ClearHeaders();
      this.Context.Response.ContentType = "application/octet-stream";
      var vFileName = Params.FindParamValue(bioPrms, CS_FILE_NAME_PARAM) as String;
      var vFile = Params.FindParamValue(bioPrms, CS_FILE_PARAM) as Byte[];
      if (String.IsNullOrEmpty(vFileName))
        throw new EBioException(String.Format("В исходящих параметрах ИО должен присутствовать параметр \"{0}\"", CS_FILE_NAME_PARAM));
      if (vFile == null)
        throw new EBioException(String.Format("В исходящих параметрах ИО должен присутствовать параметр \"{0}\"", CS_FILE_PARAM));

      this.Context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + vFileName + "\"");
      var writer = new MemoryStream(vFile);
      writer.WriteTo(this.Context.Response.OutputStream);
      this.Context.Response.Flush();
      this.Context.Response.Close();

    }

    protected override void doExecute() {
      base.doExecute();
      EBioException ebioex = null;
      if (this.FBioDesc == null)
        throw new EBioException(String.Format("Описание объекта {0} не найдено на сервере.", this.bioCode));

      var vDS = this.FBioDesc.DocumentElement;
      if (vDS == null) 
        throw new EBioException(String.Format("В описании объекта {0} не найден раздел <store>.", this.bioCode));

      var v_hashCodeOfFile = Params.FindParamValue(this.QParams, CS_HASH_CODE_WEB_PARAM) as String;
      if (String.IsNullOrEmpty(v_hashCodeOfFile)) 
        throw new EBioException(String.Format("В параметрах запроса должен присутствовать параметр {0}.", CS_HASH_CODE_WEB_PARAM));

        var rqst = this.BioRequest<BioRequest>();
        var vConn = this.BioSession.Cfg.dbSession.GetConnection();
        try {
          try {
            var cursor = new CJSCursor(vConn, vDS, this.bioCode);
            var ajaxRequestTimeOut = Utl.Convert2Type<int>(Params.FindParamValue(this.QParams, "ajaxrqtimeout"));
            var vMon = SQLGarbageMonitor.GetSQLGarbageMonitor(this.Context);
            vMon.RegisterSQLCmd(cursor, (SQLCmd vSQLCmd, ref Boolean killQuery, ref Boolean killSession, Boolean vAjaxTimeoutExceeded) => {
              if (Equals(cursor, vSQLCmd)) {
                killQuery = !this.Context.Response.IsClientConnected || vAjaxTimeoutExceeded;
                killSession = killQuery;
              }
            }, ajaxRequestTimeOut);
            try {
              var prms = new Params();
              prms.Add("p_hash_code", v_hashCodeOfFile);
              prms.Add(new Param(CS_FILE_NAME_PARAM, null, typeof(String), ParamDirection.Output));
              prms.Add(new Param(CS_FILE_PARAM, null, typeof(Byte[]), ParamDirection.Output));
              cursor.DoExecuteSQL(prms, 120);
              this._sendFileToClient(prms);
            } catch (Exception ex) {
              throw EBioException.CreateIfNotEBio(ex);
            } finally {
              vMon.RemoveItem(cursor);
            }
          } catch (Exception ex) {
            vConn.Close();
            vConn.Dispose();
            throw EBioException.CreateIfNotEBio(ex);
          }
        } catch (Exception ex) {
          ebioex = new EBioException("Ошибка выполнения на сервере. Сообщение: " + ex.Message, ex);
        } 
      if (ebioex != null) {
        this.Context.Response.Write(new BioResponse() { Success = false, BioParams = this.bioParams, Ex = ebioex }.Encode());
      }
    }
  }
}
