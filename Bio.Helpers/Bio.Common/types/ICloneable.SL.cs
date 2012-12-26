namespace Bio.Helpers.Common.Types {
#if SILVERLIGHT
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  public interface ICloneable {
    Object Clone();
  }
#endif
}
