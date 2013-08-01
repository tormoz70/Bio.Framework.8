namespace Bio.Framework.Server {
  using System;
  using System.Web;
  using Packets;
  using Helpers.Common.Types;
  using Helpers.DOA;
  using Helpers.Common;

  /// <summary>
  /// Обработчик запросов на отображение таблицы
  /// </summary>
// ReSharper disable InconsistentNaming
  public class tmio_SQLR : ABioHandlerBioTransacted {
// ReSharper restore InconsistentNaming

    public tmio_SQLR(HttpContext context, AjaxRequest request)
      : base(context, request) { }

    protected override void doExecute() {
      base.doExecute();
      EBioException ebioex = null;
      if (this.FBioDesc == null)
        throw new EBioException(String.Format("Описание объекта {0} не найдено на сервере.", this.bioCode));
      var vDS = this.FBioDesc.DocumentElement;
      if (vDS != null) {
        var rqst = this.BioRequest<BioSQLRequest>();
        var vConn = this.AssignTransaction(vDS, rqst);
        try {
          var vCursor = new CJSCursor(vConn, vDS, this.bioCode);
          var vAjaxRequestTimeOut = Utl.Convert2Type<int>(Params.FindParamValue(this.QParams, "ajaxrqtimeout"));
          var vMon = SQLGarbageMonitor.GetSQLGarbageMonitor(this.Context);
          vMon.RegisterSQLCmd(vCursor, (SQLCmd vSQLCmd, ref Boolean killQuery, ref Boolean killSession, Boolean vAjaxTimeoutExceeded) => {
            if (Equals(vCursor, vSQLCmd)) {
              killQuery = !this.Context.Response.IsClientConnected || vAjaxTimeoutExceeded;
              killSession = killQuery;
            }
          }, vAjaxRequestTimeOut);
          try {
            vCursor.DoExecuteSQL(this.bioParams, rqst.Timeout);
            this.Context.Response.Write(
              new BioResponse {
                Success = true,
                TransactionID = !this.AutoCommitTransaction ? this.TransactionID : null,
                BioParams = this.bioParams
              }.Encode());
          } finally {
            vMon.RemoveItem(vCursor);
          }
        } catch (Exception ex) {
          this.FinishTransaction(vConn, true, SQLTransactionCmd.Rollback);
          ebioex = new EBioException("Ошибка выполнения на сервере. Сообщение: " + ex.Message, ex);
        } finally {
          this.FinishTransaction(vConn, true, rqst.transactionCmd);
        }
      } else
        ebioex = new EBioException("В описании объекта [" + this.bioCode + "] не найден раздел <store>.");
      if (ebioex != null) {
        this.Context.Response.Write(new BioResponse { Success = false, BioParams = this.bioParams, Ex = ebioex }.Encode());
      }
    }

  }
}
