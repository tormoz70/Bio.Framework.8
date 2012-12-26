namespace Bio.Framework.Packets {
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Text;
  using System.ComponentModel;
  using System.Collections;
  
  /// <summary>
  /// Информация о состояние выполняемой операции
  /// </summary>
  public class CLongOpStatePack1 : CRemoteProcessStatePack {
    /// <summary>
    /// Имя трубы
    /// </summary>
    public String pipeName { get; set; }
    /// <summary>
    /// UID сессии
    /// </summary>
    public String sessionUID { get; set; }
    /// <summary>
    /// Владелец-пользователь, запустивший процесс
    /// </summary>
    public String owner { get; set; }
    /// <summary>
    /// Последнее сообщение считанное PipeReader'ом
    /// </summary>
    public String[] lastPipedLines { get; set; }

    protected override void copyThis(ref CRemoteProcessStatePack destObj) {
      base.copyThis(ref destObj);
      CLongOpStatePack1 dst = destObj as CLongOpStatePack1;
      dst.pipeName = this.pipeName;
      dst.sessionUID = this.sessionUID;
      dst.owner = this.owner;
      dst.lastPipedLines = (String[])this.lastPipedLines.Clone();
    }

  }
  
}
