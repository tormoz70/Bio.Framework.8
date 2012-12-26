namespace Bio.Framework.Packets {

  using System;
  using Bio.Helpers.Common.Types;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json;
  

  public class CBioResponse : CAjaxResponse {
    public String transactionID { get; set; }
    public CParams bioParams { get; set; }

    public CGlobalCfgPack gCfg { get; set; }

    public CRemoteProcessStatePack rmtStatePacket { get; set; }
    //public CLongOpStatePack loStatePacket { get; set; }
    public String txtContent { get; set; }
    //public Byte[] binaryData { get; set; }

    protected override void copyThis(ref CAjaxResponse destObj) {
      base.copyThis(ref destObj);
      CBioResponse dst = destObj as CBioResponse;
      dst.transactionID = this.transactionID;
      dst.bioParams = (this.bioParams != null) ? (CParams)this.bioParams.Clone() : null;
      dst.gCfg = (this.gCfg != null) ? (CGlobalCfgPack)this.gCfg.Clone() : null;
      dst.rmtStatePacket = (this.rmtStatePacket != null) ? (CRemoteProcessStatePack)this.rmtStatePacket.Clone() : null;
      //dst.loStatePacket = (this.loStatePacket != null) ? (CLongOpStatePack)this.loStatePacket.Clone() : null;
      dst.txtContent = this.txtContent;
    }

  }

}
