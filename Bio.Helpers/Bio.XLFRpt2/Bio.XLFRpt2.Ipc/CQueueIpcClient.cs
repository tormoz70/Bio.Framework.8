using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common.Types;
using System.Threading;

namespace Bio.Helpers.XLFRpt2.Ipc {
  public class CQueueIpcClient {
    private IQueue _remoteObject = null;
    public CQueueIpcClient() {
      this._remoteObject = (IQueue)Activator.GetObject(
        typeof(IQueue), "ipc://Bio.Handlers.XLFRpt2.CQueueRemoteControl.Ipc/QueueRemoteControl.rem");
    }
    /// <summary>
    /// Добавляет в очередь новый процесс создания отчета. Возвращает UID просесса.
    /// </summary>
    /// <param name="rptCode">Код содаваемого отчета</param>
    /// <param name="sessionID">ID сессии в которой создается процесс</param>
    /// <param name="userUID">UID пользователя, который запускает процесс</param>
    /// <param name="remoteIP">IP адрес с котороко запускается процесс</param>
    /// <param name="prms">Параметры отчета</param>
    /// <param name="priority">Уроверь приоритетности</param>
    /// <returns>UID просесса</returns>
    public String Add(String rptCode, String sessionID, String userUID, String remoteIP, Params prms, ThreadPriority priority) {
      String err_json = null;
      int v_priority = (int)priority;
      String v_prms = prms.Encode();
      String rslt = this._remoteObject.Add(rptCode, sessionID, userUID, remoteIP, v_prms, v_priority, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
      return rslt;
    }
    /// <summary>
    /// Запрос файла результата отчета
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="fileName">Имя файла результата отчета</param>
    /// <param name="buff">Сам файл</param>
    public void GetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref byte[] buff) {
      String err_json = null;
      this._remoteObject.GetReportResult(rptUID, userUID, remoteIP, ref fileName, ref buff, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
    }
    /// <summary>
    /// Останав выполнения процесса
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    public void Break(String rptUID, String userUID, String remoteIP) {
      String err_json = null;
      this._remoteObject.Break(rptUID, userUID, remoteIP, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
    }
    /// <summary>
    /// Останав выполнения процесса
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    public void Restart(String rptUID, String userUID, String remoteIP) {
      String err_json = null;
      this._remoteObject.Restart(rptUID, userUID, remoteIP, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
    }
    /// <summary>
    /// Удаление процесса
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    public void Drop(String rptUID, String userUID, String remoteIP) {
      String err_json = null;
      this._remoteObject.Drop(rptUID, userUID, remoteIP, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
    }
    /// <summary>
    /// Запрос списка всех выполняющихся процессов пользователя. Возвращает список просессов в формате XML.
    /// </summary>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <returns>Список просессов в формате XML</returns>
    public String GetQueue(String userUID, String remoteIP) {
      String err_json = null;
      String rslt = this._remoteObject.GetQueue(userUID, remoteIP, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
      return rslt;
    }
    /// <summary>
    /// Запрос списка отчетов в ветке folderCode делева отчетов для пользователя. Взвращает список отчетов в формате XML.
    /// </summary>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="folderCode">Код ветки</param>
    /// <returns>Список отчетов в формате XML</returns>
    public String GetRptTreeNode(String userUID, String remoteIP, String folderCode) {
      String err_json = null;
      String rslt = this._remoteObject.GetRptTreeNode(userUID, remoteIP, folderCode, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
      return rslt;
    }
    /// <summary>
    /// Проверка имени и пароля пользователя. Если ок, то возвращает строку "OK", иначе возвращает сообщение об ошибка.
    /// </summary>
    /// <param name="usr">Имя</param>
    /// <param name="pwd">Пароль</param>
    /// <returns>Если ок, то возвращает строку "OK", иначе возвращает сообщение об ошибка.</returns>
    public String CheckUsrLogin(String usr, String pwd) {
      String err_json = null;
      String rslt = this._remoteObject.CheckUsrLogin(usr, pwd, ref err_json);
      if (!String.IsNullOrEmpty(err_json)) {
        EBioException ex = EBioException.Decode(err_json);
        throw ex;
      }
      return rslt;
    }

  }
}
