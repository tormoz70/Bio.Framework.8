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

    public tmio_FileSrv(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    private const String csFileNameParam = "v_file_name";
    private const String csFileParam = "v_file";
    private const String csHashCodeWebParam = "hf";
    /// <summary>
    /// Вытаскивает из БД файл и отдает его клиенту
    /// </summary>
    /// <param name="bioParams">Параметры запроса к БД</param>
    private void sendFileToClient(Params bioPrms) {

      this.Context.Response.ClearContent();
      this.Context.Response.ClearHeaders();
      this.Context.Response.ContentType = "application/octet-stream";
      String vFileName = Params.FindParamValue(bioPrms, csFileNameParam) as String;
      Byte[] vFile = Params.FindParamValue(bioPrms, csFileParam) as Byte[];
      if (String.IsNullOrEmpty(vFileName))
        throw new EBioException(String.Format("В исходящих параметрах ИО должен присутствовать параметр \"{0}\"", csFileNameParam));
      if (vFile == null)
        throw new EBioException(String.Format("В исходящих параметрах ИО должен присутствовать параметр \"{0}\"", csFileParam));

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

      var v_hashCodeOfFile = Params.FindParamValue(this.QParams, csHashCodeWebParam) as String;
      if (String.IsNullOrEmpty(v_hashCodeOfFile)) 
        throw new EBioException(String.Format("В параметрах запроса должен присутствовать параметр {0}.", csHashCodeWebParam));

        var rqst = this.bioRequest<CBioRequest>();
        var vConn = this.BioSession.Cfg.dbSession.GetConnection();
        try {
          try {
            CJSCursor vCursor = new CJSCursor(vConn, vDS, this.bioCode);
            int vAjaxRequestTimeOut = Utl.Convert2Type<int>(Params.FindParamValue(this.QParams, "ajaxrqtimeout"));
            SQLGarbageMonitor vMon = SQLGarbageMonitor.GetSQLGarbageMonitor(this.Context);
            vMon.RegisterSQLCmd(vCursor, (SQLCmd vSQLCmd, ref Boolean killQuery, ref Boolean killSession, Boolean vAjaxTimeoutExceeded) => {
              if (Object.Equals(vCursor, vSQLCmd)) {
                killQuery = !this.Context.Response.IsClientConnected || vAjaxTimeoutExceeded;
                killSession = killQuery;
              }
            }, vAjaxRequestTimeOut);
            try {
              var prms = new Params();
              prms.Add("p_hash_code", v_hashCodeOfFile);
              prms.Add(new Param(csFileNameParam, null, typeof(String), ParamDirection.Output));
              prms.Add(new Param(csFileParam, null, typeof(Byte[]), ParamDirection.Output));
              vCursor.DoExecuteSQL(prms, 120);
              this.sendFileToClient(prms);
            } catch (Exception ex) {
              throw EBioException.CreateIfNotEBio(ex);
            } finally {
              vMon.RemoveItem(vCursor);
            }
          } catch (Exception ex) {
            vConn.Close();
            vConn.Dispose();
            throw EBioException.CreateIfNotEBio(ex);
          }
        } catch (Exception ex) {
          ebioex = new EBioException("Ошибка выполнения на сервере. Сообщение: " + ex.Message, ex);
        } finally {
          //this.FinishTransaction(vConn, rqst);
        }
      if (ebioex != null) {
        this.Context.Response.Write(new CBioResponse() { success = false, bioParams = this.bioParams, ex = ebioex }.Encode());
      }
    }
  }
}
