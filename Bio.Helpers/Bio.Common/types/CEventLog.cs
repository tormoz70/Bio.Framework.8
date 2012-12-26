namespace Bio.Helpers.Common.Types {
#if !SILVERLIGHT
	using System;
	using System.Diagnostics;

	public class CEventLog{
		
		private String FMySource = "biosys";
    private String FMyLog = "biosys";

    public CEventLog(String pMySource, String pMyLog) {
      if(pMySource != null)
			  this.FMySource = pMySource;
      if(pMyLog != null)
        this.FMyLog = pMyLog;
      if(!EventLog.SourceExists(this.FMySource))
        EventLog.CreateEventSource(this.FMySource, this.FMyLog);
		}

    public void WriteEvent(String pMsg){
      EventLog vEve = new EventLog();
      vEve.Source=this.FMySource;
      vEve.WriteEntry(pMsg, EventLogEntryType.Error);
    }

	}
#endif
}
