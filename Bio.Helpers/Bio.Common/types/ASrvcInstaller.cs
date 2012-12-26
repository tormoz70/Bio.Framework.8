using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Install;
using System.ServiceProcess;
using System.IO;
using System.Reflection;

namespace Bio.Helpers.Common.Types {
  public abstract class ASrvcInstaller : Installer {
    private ServiceProcessInstaller _serviceProcessInstaller;
    private ServiceInstaller _serviceInstaller;

    protected abstract String defaultSrvcName();

    public ASrvcInstaller() {
      this._serviceProcessInstaller = new ServiceProcessInstaller();
      this._serviceInstaller = new ServiceInstaller();

      this._serviceProcessInstaller.Account = ServiceAccount.LocalService;
      this._serviceProcessInstaller.Password = null;
      this._serviceProcessInstaller.Username = null;

      this._serviceInstaller.ServiceName = this.defaultSrvcName();

      this._serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
      // 
      // ProjectInstaller
      // 
      this.Installers.AddRange(new Installer[] {
            this._serviceProcessInstaller,
            this._serviceInstaller});
      this.BeforeInstall += new InstallEventHandler(this._beforeInstall);
      this.BeforeUninstall += new InstallEventHandler(this._beforeUninstall);
    }
    void _beforeInstall(object sender, InstallEventArgs e) {
      if (!String.IsNullOrEmpty(this.Context.Parameters["srvcName"])) {
        this._serviceInstaller.ServiceName = this.Context.Parameters["srvcName"];
      }
      this._serviceInstaller.Description = "Instance of \"" + this.defaultSrvcName() + "\"";
      
    }
    void _beforeUninstall(object sender, InstallEventArgs e) {
      if (!String.IsNullOrEmpty(this.Context.Parameters["srvcName"])) {
        this._serviceInstaller.ServiceName = this.Context.Parameters["srvcName"];
      }
    }
  }
}
