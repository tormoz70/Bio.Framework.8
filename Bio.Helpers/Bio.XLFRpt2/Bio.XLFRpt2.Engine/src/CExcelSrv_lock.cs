namespace Bio.Helpers.XLFRpt2.Engine {
	
	using System;
	using System.Xml;
	using System.Web;
	using System.IO;
	using System.Threading;
  using System.Data;
  using Bio.Helpers.Common.Types;
  using System.Globalization;
#if OFFICE12
  using Excel = Microsoft.Office.Interop.Excel;
  using System.Security.AccessControl;
#endif

  public partial class CExcelSrv:CDisposableObject {

    //public static Mutex MutexExcelApp = new Mutex();

    private const String csCOMExceptionID_1 = "No more results.";
    private const String csCOMExceptionID_2 = "Operation unavailable";
    private const String csCOMExceptionID_3 = "Операция недоступна";

    //private static void nar(Object o) {
    //  try {
    //    System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
    //  } catch { } finally {
    //    o = null;
    //  }
    //}

    private static void nar<T>(ref T o) {
      try {
        System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
      } catch { } finally {
        o = default(T);
      }
    }

    public static void nar(ref Excel.Application o) {
      nar<Excel.Application>(ref o);
    }
    public static void nar(ref Excel.Workbook o) {
      nar<Excel.Workbook>(ref o);
    }
    public static void nar(ref Excel.Worksheet o) {
      nar<Excel.Worksheet>(ref o);
    }
    public static void nar(ref Excel.Range o) {
      nar<Excel.Range>(ref o);
    }
    public static void nar(ref Excel.Name o) {
      nar<Excel.Name>(ref o);
    }

    private Excel.Application runExcel0() {
      //Excel.Application rslt = null;
      //var v_connect_to_existing_inst = false;
      //try {
      //  rslt = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
      //  v_connect_to_existing_inst = true;
      //} catch (System.Runtime.InteropServices.COMException) { }

      //if (!v_connect_to_existing_inst) {
      //  try {
      //    rslt = new Excel.Application();
      //  } catch (Exception ex) {
      //    throw ex;
      //  }
      //}
      //return rslt;
      return new Excel.Application();
    }

    private void stopExcel0(ref Excel.Application excelApp){
      if (excelApp != null) {
        excelApp.Quit();
        nar(ref excelApp);
        GC.Collect();
        GC.WaitForPendingFinalizers();
      }
    }

    private Mutex m = null;
    private const string csMutexName = "Global/XLFRptExcelLockMutex";
    private void creMutex() {
      if (m != null)
        return;
      
      bool doesNotExist = false;
      bool unauthorized = false;

      // The value of this variable is set by the mutex
      // constructor. It is true if the named system mutex was
      // created, and false if the named mutex already existed.
      //
      bool mutexWasCreated = false;

      // Attempt to open the named mutex.
      try {
        // Open the mutex with (MutexRights.Synchronize |
        // MutexRights.Modify), to enter and release the
        // named mutex.
        //
        m = Mutex.OpenExisting(csMutexName);
        return;
      } catch (WaitHandleCannotBeOpenedException) {
        //Console.WriteLine("Mutex does not exist.");
        doesNotExist = true;
      } catch (UnauthorizedAccessException ex) {
        //Console.WriteLine("Unauthorized access: {0}", ex.Message);
        unauthorized = true;
      }

      // There are three cases: (1) The mutex does not exist.
      // (2) The mutex exists, but the current user doesn't 
      // have access. (3) The mutex exists and the user has
      // access.
      //
      if (doesNotExist) {
        // The mutex does not exist, so create it.

        // Create an access control list (ACL) that denies the
        // current user the right to enter or release the 
        // mutex, but allows the right to read and change
        // security information for the mutex.
        //
        String user = Environment.UserDomainName + "\\" + Environment.UserName;
        MutexSecurity mSec = new MutexSecurity();

        MutexAccessRule rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, 
          AccessControlType.Deny);
        mSec.AddAccessRule(rule);

        rule = new MutexAccessRule(user, MutexRights.ReadPermissions | MutexRights.ChangePermissions, 
          AccessControlType.Allow);
        mSec.AddAccessRule(rule);

        // Create a Mutex object that represents the system
        // mutex named by the constant 'mutexName', with
        // initial ownership for this thread, and with the
        // specified security access. The Boolean value that 
        // indicates creation of the underlying system object
        // is placed in mutexWasCreated.
        //
        m = new Mutex(false, csMutexName, out mutexWasCreated, mSec);

        // If the named system mutex was created, it can be
        // used by the current instance of this program, even 
        // though the current user is denied access. The current
        // program owns the mutex. Otherwise, exit the program.
        // 
        if (mutexWasCreated) {
          return;
        } else {
          //Console.WriteLine("Unable to create the mutex.");
          throw new Exception("Unable to create the mutex.");
        }

      } else if (unauthorized) {
        // Open the mutex to read and change the access control
        // security. The access control security defined above
        // allows the current user to do this.
        //
        //try {
          m = Mutex.OpenExisting(csMutexName, MutexRights.ReadPermissions | MutexRights.ChangePermissions);

          // Get the current ACL. This requires 
          // MutexRights.ReadPermissions.
          MutexSecurity mSec = m.GetAccessControl();

          String user = Environment.UserDomainName + "\\" + Environment.UserName;

          // First, the rule that denied the current user 
          // the right to enter and release the mutex must
          // be removed.
          MutexAccessRule rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, 
            AccessControlType.Deny);
          mSec.RemoveAccessRule(rule);

          // Now grant the user the correct rights.
          // 
          rule = new MutexAccessRule(user, MutexRights.Synchronize | MutexRights.Modify, 
            AccessControlType.Allow);
          mSec.AddAccessRule(rule);

          // Update the ACL. This requires
          // MutexRights.ChangePermissions.
          m.SetAccessControl(mSec);

          //Console.WriteLine("Updated mutex security.");

          // Open the mutex with (MutexRights.Synchronize 
          // | MutexRights.Modify), the rights required to
          // enter and release the mutex.
          //
          m = Mutex.OpenExisting(csMutexName);

        //} catch (UnauthorizedAccessException ex) {
        //  Console.WriteLine("Unable to change permissions: {0}",
        //      ex.Message);
        //  return;
        //}

      }

      // If this program created the mutex, it already owns
      // the mutex.
      //
      //if (!mutexWasCreated) {
        // Enter the mutex, and hold it until the program
        // exits.
        //
        //try {
          //Console.WriteLine("Wait for the mutex.");
          //m.WaitOne();
          //Console.WriteLine("Entered the mutex.");
        //} catch (UnauthorizedAccessException ex) {
        //  Console.WriteLine("Unauthorized access: {0}", ex.Message);
        //}
      //}

      //Console.WriteLine("Press the Enter key to exit.");
      //Console.ReadLine();
      //m.ReleaseMutex();
    }

    private void getMutex() {
      this.creMutex();
      if (m != null)
        m.WaitOne();
    }

    private void releaseMutex() {
      if (m != null)
        try {
          m.ReleaseMutex();
        } catch { }
    }

    private Boolean _neadReleaseExcel = false;
    private void getExcelApp(CXLReport pReport) {
      try {
        
        if(this.FExcel == null) {
          if (this.FOutterExcel == null) {
            this.FExcel = this.runExcel0();
            this._neadReleaseExcel = true;
          } else {
            this.getMutex();
            this.FExcel = this.FOutterExcel;
            this._neadReleaseExcel = false;
          }
        }
        if (this.FExcel != null) {
          if (this.FExcel.Visible)
            this.FExcel.Visible = false;
          if (this.FExcel.DisplayAlerts)
            this.FExcel.DisplayAlerts = false;
        }

      } catch(AbandonedMutexException) {
      } finally {
      }
    }

    private void releaseExcelApp() {
      if (this._neadReleaseExcel)
        this.stopExcel0(ref this.FExcel);
      else {
        nar(ref this.FExcel);
        this.releaseMutex();
      }

    }

    public void initReport(CXLReport report) {
      try {
        report.writeLogLine("Запрос Excel...");
        this.getExcelApp(report);
        if (this.FExcel != null)
          report.writeLogLine("Excel получен.");
        else {
          throw new Exception("Доступ к Excel не получен.");
        }
        //Thread.Sleep(1000 * 20);
        this.openWB(report.RptDefinition.TemplateFileName);
        var author = report.RptDefinition.Autor + "[" + report.RptDefinition.UserName + "]";
        this.saveWB(report.LastResultFile, null, null, author);
        if(report.OnPrepareTemplate != null)
          report.OnPrepareTemplate(report.Opener, ref this.FWorkbook, report);
        if (report.RptDefinition.MacroBefore != null)
          this.runScript(report, report.RptDefinition.MacroBefore.Name, report.RptDefinition.MacroBefore.Params);
        report.DataSources.Init(ref this.FWorkbook);
        this.closeWB(true, false, report.RptDefinition.PwdOpen, report.RptDefinition.PwdWrite, report.RptDefinition.LogPath, author);
      } finally {
        report.writeLogLine("Отпускаем Excel...");
        this.releaseExcelApp();
        report.writeLogLine("Excel свободен.");
      }
    }

    public void buildReport(CXLReport report, Int32 timeout) {
      try {
        report.writeLogLine("Запрос Excel...");
        this.getExcelApp(report);
        if (this.FExcel != null)
          report.writeLogLine("Excel получен.");
        else {
          throw new Exception("Доступ к Excel не получен.");
        }
        this.openWB(report.LastResultFile);
        report.DataSources.BuildReport(ref this.FWorkbook, timeout);
        if (report.RptDefinition.MacroAfter != null) {
          report.writeLogLine("Запуск завершающего макроса...");
          try {
            this.runScript(report, report.RptDefinition.MacroAfter.Name, report.RptDefinition.MacroAfter.Params);
            report.writeLogLine("Запуск завершающего макроса - ОК.");
          } catch (Exception ex) {
            report.writeLogLine("Ошибка при запуске завершающего макроса - " + ex.Message);
            throw ex;
          }
          
        }
        var author = (report.RptDefinition.Autor ?? "BioSys") + " - [user:" + report.RptDefinition.UserName + "]";
        this.closeWB(report.RptDefinition.LiveScripts, false, report.RptDefinition.PwdOpen, report.RptDefinition.PwdWrite, report.RptDefinition.LogPath, author);
      } finally {
        report.writeLogLine("Отпускаем Excel...");
        this.releaseExcelApp();
        report.writeLogLine("Excel свободен.");
      }
    }

    public void finalizeReport(CXLReport report) {
      //try {
      //  this.getExcelApp(pReport);
        //this.openWB(pReport.LastReportResultFile);
        //this.runScript(pReport.RptDefinition.MacroAfter.Name, pReport.RptDefinition.MacroAfter.Params);
        //this.closeWB(pReport.RptDefinition.LiveScripts, false, pReport.RptDefinition.PwdOpen, pReport.RptDefinition.PwdWrite);
      //} finally {
      //  this.releaseExcelApp();
      //}
    }

	}
}
