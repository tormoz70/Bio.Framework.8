using System;

namespace Bio.Framework.Client.SL {
  public interface IConfigRec {
    void ApplyFrom(Object source);
    Boolean ValidateCfg();
    String GetDisplayName();
    String GetDescription();
    void Store();
  }
}
