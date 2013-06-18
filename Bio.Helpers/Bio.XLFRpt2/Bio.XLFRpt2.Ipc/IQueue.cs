using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Helpers.XLFRpt2.Ipc {
  public interface IQueue {
    /// <summary>
    /// Добавляет в очередь новый процесс создания отчета.
    /// </summary>
    /// <param name="rptCode">Код содаваемого отчета</param>
    /// <param name="sessionID">ID сессии в которой создается процесс</param>
    /// <param name="userUID">UID пользователя, который запускает процесс</param>
    /// <param name="remoteIP">IP адрес с котороко запускается процесс</param>
    /// <param name="prms_json">Параметры отчета</param>
    /// <param name="priority">Уроверь приоритетности</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    /// <returns>UID просесса</returns>
    String Add(String rptCode, String sessionID, String userUID, String remoteIP, String prms_json, int priority, ref String err_json);
    /// <summary>
    /// Запрос файла результата отчета
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="fileName">Имя файла результата отчета</param>
    /// <param name="buff">Сам файл</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    void GetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref byte[] buff, ref String err_json);
    /// <summary>
    /// Останов выполнения процесса
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    void Break(String rptUID, String userUID, String remoteIP, ref String err_json);
    /// <summary>
    /// Перезапуск процесса
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    void Restart(String rptUID, String userUID, String remoteIP, ref String err_json);
    /// <summary>
    /// Удаление процесса
    /// </summary>
    /// <param name="rptUID">UID просесса</param>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    void Drop(String rptUID, String userUID, String remoteIP, ref String err_json);
    /// <summary>
    /// Запрос списка всех выполняющихся процессов пользователя
    /// </summary>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    /// <returns>Список просессов в формате XML</returns>
    String GetQueue(String userUID, String remoteIP, ref String err_json);
    /// <summary>
    /// Запрос списка отчетов в ветке folderCode делева отчетов для пользователя
    /// </summary>
    /// <param name="userUID">UID пользователя</param>
    /// <param name="remoteIP">IP адрес</param>
    /// <param name="folderCode">Код ветки</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    /// <returns>Список отчетов в формате XML</returns>
    String GetRptTreeNode(String userUID, String remoteIP, String folderCode, ref String err_json);
    /// <summary>
    /// Проверка имени и пароля пользователя
    /// </summary>
    /// <param name="usr">Имя</param>
    /// <param name="pwd">Пароль</param>
    /// <param name="err_json">Ошибка, если она произошла</param>
    /// <returns>Если ок, то возвращает строку "OK", иначе возвращает сообщение об ошибка.</returns>
    String CheckUsrLogin(String usr, String pwd, ref String err_json);
  }
}
