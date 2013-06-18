using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.IO;
using System.Configuration;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Timers;
using Bio.Helpers.Common;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;
using System.Runtime.Remoting.Channels.Ipc;

namespace Bio.Helpers.XLFRpt2.Srvc {
  public partial class Service : ServiceBase {
    private CConfigSys _cfg = null;
    private CQueue _queue = null;
    private String logFileName = null;


    private void log_msg(String msg) {
      Utl.WriteMessageLog(this.logFileName, msg);
    }
    private void log_err(Exception ex) {
      Utl.WriteErrorLog(this.logFileName, ex);
    }

    private void _init() {
      var physicalApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      physicalApplicationPath = Utl.NormalizeDir(Directory.GetParent(physicalApplicationPath).FullName);
      this.logFileName = physicalApplicationPath + "xlfrpt2_srvc.log";
      this.log_msg("*************************************** Инициализация \"Очереди отчетов\"... ***************************************************");
      this.log_msg("\tЗагрузка конфигурации...");
      this._cfg = CConfigSys.load(physicalApplicationPath, this.logFileName);
      this._cfg.msgLogWriter = this.log_msg;
      this._cfg.errLogWriter = this.log_err;
      this.log_msg("\tКонфигурация загружена.");

      this.log_msg("\tИнициализация сервера Ipc...");
      // Create the server channel.

      SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
      NTAccount account = sid.Translate(typeof(NTAccount)) as NTAccount;

      IDictionary channelProperties = new Hashtable();
      channelProperties["portName"] = "Bio.Handlers.XLFRpt2.CQueueRemoteControl.Ipc";
      channelProperties["exclusiveAddressUse"] = false;
      channelProperties["authorizedGroup"] = account.Value;
      channelProperties["typeFilterLevel"] = TypeFilterLevel.Full;
      IpcChannel serverChannel = new IpcChannel(channelProperties, null, null);
      ChannelServices.RegisterChannel(serverChannel, false);

      // Expose an object for remote calls.
      RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(CQueueRemoteControl), "QueueRemoteControl.rem",
              WellKnownObjectMode.Singleton);

      this.log_msg("\tСервер Ipc инициализирован.");
      this.log_msg("*************************************** Инициализация \"Очереди отчетов\" выполнена. ***************************************************");
    }


    public Service() {
      InitializeComponent();
      this._init();
      this._queue = CQueue.creQueue(this, this._cfg);
    }

    static void Main(string[] args) {
      Service vService = new Service();
      if (args.Length == 1 && args[0].Equals("console")) {
        vService.log_msg("Запуск в режиме \"консольное приложение\"...");

        Console.WriteLine("starting...");

        vService.OnStart(null);

        Console.WriteLine("ready (ENTER to exit)");
        Console.ReadLine();

        vService.OnStop();

        Console.WriteLine("stopped");
        vService.log_msg("Выполнение врежиме \"консольное приложение\" завершено.");
      } else {
        //Console.WriteLine("Run as service or use command \"console\".");
        ServiceBase.Run(vService);
      }
    }

    protected override void OnStart(string[] args) {
      this.log_msg("*************************************** Запуск службы ***************************************************");
      this._queue.Start();
      this.log_msg("Служба запущена.");
    }

    protected override void OnStop() {
      this.log_msg("Останов службы...");
      this._queue.Stop();
      this.log_msg("*************************************** Служба остановлена ***************************************************");
    }
  }
}
