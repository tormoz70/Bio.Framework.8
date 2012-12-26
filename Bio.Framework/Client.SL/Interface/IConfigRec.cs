using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Framework.Client.SL {
  public interface IConfigRec {
    void ApplyFrom(Object pSource);
    Boolean ValidateCfg();
    String GetDisplayName();
    String GetDescription();

  }
}
