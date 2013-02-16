using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Bio.Helpers.XLFRpt2.Ipc;
using Bio.Helpers.Common.Types;
using System.Threading;

namespace Bio.Helpers.XLFRpt2.Srvc {
  class CQueueRemoteControl : MarshalByRefObject, IQueue {
    public CQueue Owner { get; private set; }
    public CQueueRemoteControl() {
      this.Owner = CQueue.instOfQueue;
    }

    public String Add(String rptCode, String sessionID, String userUID, String remoteIP, String prms, int priority, ref String err_json) {
      ThreadPriority v_priority = (ThreadPriority)priority;
      Params v_prms = Params.Decode(prms);
      String rslt = null;
      try {
        rslt = this.Owner.Add(rptCode, sessionID, userUID, remoteIP, v_prms, v_priority);
      } catch(Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
      return rslt;
    }

    public void GetReportResult(String rptUID, String userUID, String remoteIP, ref String fileName, ref byte[] buff, ref String err_json) {
      try {
        this.Owner.GetReportResult(rptUID, userUID, remoteIP, ref fileName, ref buff);
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
    }

    public void Break(String rptUID, String userUID, String remoteIP, ref String err_json) {
      try {
        this.Owner.Break(rptUID, userUID, remoteIP);
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
    }
    public void Restart(String rptUID, String userUID, String remoteIP, ref String err_json) {
      try {
        this.Owner.Restart(rptUID, userUID, remoteIP);
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
    }
    public void Drop(String rptUID, String userUID, String remoteIP, ref String err_json) {
      try {
        this.Owner.Drop(rptUID, userUID, remoteIP);
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
    }

    public String GetQueue(String userUID, String remoteIP, ref String err_json) {
      err_json = null;
      String rslt = null;
      try {
        XmlDocument q = this.Owner.GetQueue(userUID, remoteIP);
        rslt = q.OuterXml;
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
      return rslt;
    }

    public String GetRptTreeNode(String userUID, String remoteIP, String folderCode, ref String err_json) {
      err_json = null;
      String rslt = null;
      try {
        XmlDocument q = this.Owner.GetRptTreeNode(userUID, remoteIP, folderCode);
        if (q != null)
          rslt = q.OuterXml;
        else
          throw new EBioException("Путь \"" + folderCode + "\" не найден!");
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
      return rslt;
    }
    public String CheckUsrLogin(String usr, String pwd, ref String err_json) {
      err_json = null;
      String rslt = null;
      try {
        rslt = this.Owner.CheckUsrLogin(usr, pwd);
      } catch (Exception ex) {
        err_json = EBioException.CreateIfNotEBio(ex).Encode();
      }
      return rslt;
    }

  }
}
