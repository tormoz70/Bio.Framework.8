using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bio.Helpers.Common;
using System.Threading;
using System.ComponentModel;

namespace Bio.Helpers.Common.Types {

  /// <summary>
  /// Инрерфейс должен быть реализован объектом, который в отдельном потоке строит файл
  /// </summary>
  public interface IRemoteProcInst {
    /// <summary>
    /// Имя пользователя, который запустил данный процесс
    /// </summary>
    String Owner { get; }
    /// <summary>
    /// UID
    /// </summary>
    String UID { get; }
    /// <summary>
    /// В процессе выполнения
    /// </summary>
    Boolean IsRunning { get; }
    /// <summary>
    /// Последняя ошибка
    /// </summary>
    EBioException LastError { get; }
    /// <summary>
    /// Дата и время запуска
    /// </summary>
    DateTime Started { get; }
    /// <summary>
    /// Прошло с момента запуска
    /// </summary>
    TimeSpan Duration { get; }
    /// <summary>
    /// Состояние
    /// </summary>
    RemoteProcState State { get; }
    /// <summary>
    /// Последний файл результата
    /// </summary>
    String LastResultFile { get; }
    /// <summary>
    /// Остановить 
    /// </summary>
    void Abort(Action callback);
    /// <summary>
    /// Уничтожить объект
    /// </summary>
    void Dispose();

    /// <summary>
    /// Запуск в новом потоке
    /// </summary>
    /// <param name="priority"></param>
    void Run(ThreadPriority priority);

    /// <summary>
    /// Запуск в новом потоке
    /// </summary>
    void Run();

    #region Для долгих процессов
    /// <summary>
    /// Имя трубы для обратной связи с процессом. Если пусто, то нет обратной связи
    /// </summary>
    String Pipe { get; }
    /// <summary>
    /// Процедура для заталкивания данных полученных из трубы монитором в стек процесса
    /// </summary>
    /// <param name="pipedLines"></param>
    void pushPipedLine(String[] pipedLines);
    /// <summary>
    /// Данные считанные из трубы монитором долгого процесса
    /// </summary>
    String[] popPipedLines();
    #endregion
  }

}
