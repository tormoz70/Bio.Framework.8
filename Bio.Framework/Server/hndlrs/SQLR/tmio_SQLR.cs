namespace Bio.Framework.Server {
  using System;
  using System.Data.Common;
  using System.Web;
  using System.Xml;
  using Bio.Framework.Packets;
  using Bio.Helpers.Common.Types;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;
  using System.Data;

  /// <summary>
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_SQLR : ABioHandlerBioTransacted {

    public tmio_SQLR(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) { }

    protected override void doExecute() {
      base.doExecute();
      EBioException ebioex = null;
      if (this.FBioDesc == null)
        throw new EBioException(String.Format("Описание объекта {0} не найдено на сервере.", this.bioCode));
      var vDS = this.FBioDesc.DocumentElement;
      if (vDS != null) {
        var rqst = this.bioRequest<CBioSQLRequest>();
        var vConn = this.AssignTransaction(vDS, rqst);
        try {
          //try {
          var vCursor = new CJSCursor(vConn, vDS, this.bioCode);
          int vAjaxRequestTimeOut = Utl.Convert2Type<int>(CParams.FindParamValue(this.QParams, "ajaxrqtimeout"));
          var vMon = SQLGarbageMonitor.GetSQLGarbageMonitor(this.Context);
          vMon.RegisterSQLCmd(vCursor, (SQLCmd vSQLCmd, ref Boolean killQuery, ref Boolean killSession, Boolean vAjaxTimeoutExceeded) => {
            if (Object.Equals(vCursor, vSQLCmd)) {
              killQuery = !this.Context.Response.IsClientConnected || vAjaxTimeoutExceeded;
              killSession = killQuery;
            }
          }, vAjaxRequestTimeOut);
          try {
            vCursor.DoExecuteSQL(this.bioParams, rqst.timeout);
            this.Context.Response.Write(
              new CBioResponse() {
                success = true,
                transactionID = !this.AutoCommitTransaction ? this.TransactionID : null,
                bioParams = this.bioParams
              }.Encode());
          //} catch (Exception ex) {
          //  throw EBioException.CreateIfNotEBio(ex);
          } finally {
            vMon.RemoveItem(vCursor);
          }
          //} catch (Exception ex) {
          //  this.RollbackTransaction();
          //  throw EBioException.CreateIfNotEBio(ex);
          //}
        } catch (Exception ex) {
          this.FinishTransaction(vConn, true, CSQLTransactionCmd.Rollback);
          ebioex = new EBioException("Ошибка выполнения на сервере. Сообщение: " + ex.Message, ex);
        } finally {
          this.FinishTransaction(vConn, true, rqst.transactionCmd);
        }
      } else
        ebioex = new EBioException("В описании объекта [" + this.bioCode + "] не найден раздел <store>.");
      if (ebioex != null) {
        this.Context.Response.Write(new CBioResponse() { success = false, bioParams = this.bioParams, ex = ebioex }.Encode());
      }
    }

  }
}
