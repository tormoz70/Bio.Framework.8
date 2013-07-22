namespace Bio.Framework.Packets {

  using System;
  using Helpers.Common.Types;


  public class BioResponse : AjaxResponse {
    public String TransactionID { get; set; }
    public Params BioParams { get; set; }

    public GlobalCfgPack GCfg { get; set; }

    public RemoteProcessStatePack RmtStatePacket { get; set; }
    public String TxtContent { get; set; }

    protected override void copyThis(ref AjaxResponse destObj) {
      base.copyThis(ref destObj);
      var dst = destObj as BioResponse;
      if (dst != null) {
        dst.TransactionID = this.TransactionID;
        dst.BioParams = (this.BioParams != null) ? (Params)this.BioParams.Clone() : null;
        dst.GCfg = (this.GCfg != null) ? (GlobalCfgPack)this.GCfg.Clone() : null;
        dst.RmtStatePacket = (this.RmtStatePacket != null) ? (RemoteProcessStatePack)this.RmtStatePacket.Clone() : null;
        dst.TxtContent = this.TxtContent;
      }
    }

  }

}
