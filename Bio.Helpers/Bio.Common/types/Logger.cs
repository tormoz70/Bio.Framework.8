using System;
using System.IO;
using System.Text;

namespace Bio.Helpers.Common.Types {
  public class Logger {
    public String LogDirectory { get; private set; }
    public String FileName { get; private set; }
    public Boolean Disabled { get; set; }
    public Logger(String logDirectory, String fileName, Boolean append = false) {
      this.LogDirectory = Utl.NormalizeDir(logDirectory);
      this.FileName = Path.GetFileName(fileName);
      if (!append && File.Exists(this.LogDirectory + this.FileName))
        File.Delete(this.LogDirectory + this.FileName);
    }
    public Logger(String fileName, Boolean append = false) {
      this.LogDirectory = Utl.NormalizeDir(Path.GetDirectoryName(fileName));
      this.FileName = Path.GetFileName(fileName);
      if (!append && File.Exists(this.LogDirectory + this.FileName))
        File.Delete(this.LogDirectory + this.FileName);
    }

    public static void WriteLogLine(String fileName, String msg, DateTime? prevDateTimePoint = null, String ipAddress = null) {
      var curPoint = DateTime.Now;
      var dure = (prevDateTimePoint.HasValue) ? (curPoint - prevDateTimePoint.Value) : new TimeSpan(0);
      var tpoint = curPoint.ToString("yyyy.MM.dd HH:mm:ss");
      var dureStr = String.Format("{0}:{1}:{2}.{3}",
                            dure.Hours.ToString("00"), dure.Minutes.ToString("00"),
                            dure.Seconds.ToString("00"), dure.Milliseconds.ToString("000"));
      var line = String.Format("{0} [{1}]:({2}):{3}", tpoint, dureStr, ipAddress, msg);
      Utl.AppendStringToFile(fileName, line, Encoding.Default);
    }

    private static readonly object _syncObject = new Object();

    private DateTime _lastTimePoint = DateTime.Now;
    public void WriteLn(String line, String ipAddress = null) {
      if (this.Disabled) return;
      lock (_syncObject) {
        WriteLogLine(this.LogDirectory + this.FileName, line, this._lastTimePoint, ipAddress);
        this._lastTimePoint = DateTime.Now;
      }
    }
  }
}
