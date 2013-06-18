namespace Bio.Helpers.Common.Types {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;

  /// <summary>
  /// Базовый класс ошибки при выполнении долгих операций
  /// </summary>
  public class EBioLongOp:EBioException {
    /// <summary>
    /// Конструктор
    /// </summary>
    public EBioLongOp()
      : base() {
    }
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pMsg"></param>
    public EBioLongOp(String pMsg)
      : base(pMsg) {
    }
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pInnerExeption"></param>
    public EBioLongOp(Exception pInnerExeption)
      : base(pInnerExeption.Message, pInnerExeption) {
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pMsg"></param>
    /// <param name="pInnerExeption"></param>
    public EBioLongOp(String pMsg, Exception pInnerExeption)
      : base(pMsg, pInnerExeption) {
    }

  }

  /// <summary>
  /// Ошибка. Процесс уже запущен другим пользователем
  /// </summary>
  public class EBioLongOpSessAlien:EBioLongOp {
    /// <summary>
    /// Имя пользователя, запустившего процесс
    /// </summary>
    public String Owner { get; private set; }

    /// <summary>
    /// Конструктор
    /// </summary>
    public EBioLongOpSessAlien()
      : base() {
    }
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="owner"></param>
    public EBioLongOpSessAlien(String owner)
      : base() {
      this.Owner = owner;
    }
  }

}
