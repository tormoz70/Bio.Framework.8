using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using Bio.Helpers.Common.Types;
using System.IO;
using System.Reflection;

namespace Bio.Helpers.XLFRpt2.Srvc {
  [RunInstaller(true)]
  public class ProjectInstaller : ASrvcInstaller {
    protected override String defaultSrvcName() {
      return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
    }
    public ProjectInstaller()
      : base() {
    }

  }
}
