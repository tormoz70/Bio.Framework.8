namespace Bio.Framework.Packets {
  using System;

  /// <summary>
  /// Информация о состояние выполняемой операции
  /// </summary>
  public class LongOpStatePack1 : RemoteProcessStatePack {

    protected override void copyThis(ref RemoteProcessStatePack destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as LongOpStatePack1;
      if (dst != null) {
        dst.pipe = this.pipe;
        dst.sessionUID = this.sessionUID;
        dst.owner = this.owner;
        dst.lastPipedLines = (String[])this.lastPipedLines.Clone();
      }
    }

  }
  
}
