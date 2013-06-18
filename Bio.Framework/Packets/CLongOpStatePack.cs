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

    protected override void copyThis(ref CRemoteProcessStatePack destObj) {
      base.copyThis(ref destObj);
      CLongOpStatePack1 dst = destObj as CLongOpStatePack1;
      dst.pipe = this.pipe;
      dst.sessionUID = this.sessionUID;
      dst.owner = this.owner;
      dst.lastPipedLines = (String[])this.lastPipedLines.Clone();
    }

  }
  
}
