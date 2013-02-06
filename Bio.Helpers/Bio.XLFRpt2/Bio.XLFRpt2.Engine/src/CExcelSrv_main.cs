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
  using VBIDE = Microsoft.Vbe.Interop;
  using Bio.Helpers.Common;
#endif
  using Bio.Helpers.XLFRpt2.Engine.XLRptParams;

  public partial class ExcelSrv:DisposableObject {

    //private
		private	Excel.Application FExcel;
    private Excel.Application FOutterExcel;
    private Excel.Workbook FWorkbook;
    private String FFileName = null;

		private void openWB(String pFileName){
      String vFileName = pFileName;/*@"d:\Results(rpt).xls";*/
      CultureInfo vBckp = Thread.CurrentThread.CurrentCulture;
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
      try {
        this.FWorkbook = this.FExcel.Workbooks.Open(vFileName,
          Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
          Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        this.FFileName = vFileName;
      } catch(Exception ex) {
        throw new EBioException("Ошибка при открытии файла XLS " + vFileName + ".\n" +
                            "Сообщение: " + ex.ToString(), ex);
      } finally {
        Thread.CurrentThread.CurrentCulture = vBckp;
      }
		}

    private void protectWS(Excel.Worksheet pWS, String pPwdWrite) {
      if((pWS != null) && (pPwdWrite != null)) {
        Object vWritePwd = Type.Missing;
        if(pPwdWrite != null)
          vWritePwd = pPwdWrite;
        pWS.Protect(
          vWritePwd,
          true,
          true,
          true,
          false,
          true,
          true,
          true,
          false,
          false,
          false,
          false,
          false,
          true,
          true,
          true
        );
      }
    }

    private void protectWB(String pPwdWrite) {
      if(this.FWorkbook != null) {
        for(int i = 1; i <= this.FWorkbook.Worksheets.Count; i++) {
            this.protectWS((Excel.Worksheet)this.FWorkbook.Worksheets[i], pPwdWrite);
        }
      }
    }

    private void saveWB(String fileName, String openPwd, String writePwd, String author) {
      if(this.FWorkbook != null) {
        //if(!String.IsNullOrEmpty(autor))
        this.FWorkbook.Author = author;
        var vFileName = fileName;
        if(vFileName == null)
          vFileName = this.FFileName;
        const int ciTryCnt = 5;
        const int ciMS2Sleep = 3000;
        int vCurTry = 0;
        Object vOpenPwd = Type.Missing;
        if(openPwd != null)
          vOpenPwd = openPwd;
        Object vWritePwd = Type.Missing;
        if(writePwd != null)
          vWritePwd = writePwd;
        if(writePwd != null)
          this.protectWB(writePwd);
        
retry_save:
        try {
          this.FWorkbook.SaveAs(
            vFileName,
            Type.Missing,
            vOpenPwd,
            vWritePwd,
            Type.Missing,
            Type.Missing,
            Excel.XlSaveAsAccessMode.xlExclusive,
            Type.Missing,
            Type.Missing,
            Type.Missing,
            Type.Missing,
            Type.Missing);
          this.FFileName = vFileName;
        } catch(Exception ex) {
          if(vCurTry >= ciTryCnt)
            throw new EBioException("Сохранение в файл \"" + vFileName + "\".", ex);
          else {
            vCurTry++;
            Thread.Sleep(ciMS2Sleep);
            goto retry_save;
          }
        }
      }
    }

//public


		//constructor

    
    public ExcelSrv(Excel.Application pExcelInst) {
      this.FOutterExcel = pExcelInst;
      this.FExcel = null;
      this.FWorkbook = null;
		}
		//destructor
		protected override void doOnDispose(){
      this.FOutterExcel = null;
      this.FExcel = null;
		}

    private void dropModule(String pModuleName, String pLogPath) {
      try {
        int vCount = this.FWorkbook.VBProject.VBComponents.Count;
        VBIDE.VBComponent vModule = null;
        for (int i = 1; i < vCount; i++)
          if (this.FWorkbook.VBProject.VBComponents.Item(i).Name.Equals(xlrModuleName)) {
            vModule = this.FWorkbook.VBProject.VBComponents.Item(i);
            break;
          }
        if (vModule == null)
          this.FWorkbook.VBProject.VBComponents.Remove(vModule);
      } catch (Exception ex) {
        String vWarningFile = pLogPath + "ExcelSrv.dropModule.wrn";
        Utl.SaveStringToFile(vWarningFile, ex.ToString(), null);
      }
    }

    private void dropAllModules(String pLogPath) {
      try {
        VBIDE.VBComponent vModule = null;
        int i = 1;
        while (i <= this.FWorkbook.VBProject.VBComponents.Count) {
          vModule = this.FWorkbook.VBProject.VBComponents.Item(i);
          if (vModule.Type == VBIDE.vbext_ComponentType.vbext_ct_StdModule) {
            String vMName = vModule.Name;
            this.FWorkbook.VBProject.VBComponents.Remove(vModule);
          } else
            i++;
        }
      } catch (Exception ex) {
        String vWarningFile = pLogPath + "ExcelSrv.dropAllModules.wrn";
        Utl.SaveStringToFile(vWarningFile, ex.ToString(), null);
      }
    }

    private void closeWB(Boolean liveScripts, Boolean closeAll, String pwdOpen, String pwdWrite, String logPath, String author) {
      if(this.FWorkbook != null) {
        this.dropModule(xlrModuleName, logPath);
        if(!liveScripts)
          this.dropAllModules(logPath);
        this.saveWB(null, pwdOpen, pwdWrite, author);
        this.FWorkbook.Close(false, Type.Missing, Type.Missing);
        nar(ref this.FWorkbook);
        if (File.Exists(this.FFileName)) {
          //var f = System.Diagnostics.FileVersionInfo.GetVersionInfo(this.FFileName);
          //f.
        }
      }
      if(closeAll) {
        if(this.FExcel.Workbooks.Count > 0) {
          for(int i = 1; i <= this.FExcel.Workbooks.Count; i++) {
            this.FExcel.Workbooks[i].Close(true, Type.Missing, Type.Missing);
          }
          this.FExcel.Workbooks.Close();
        }
      }
    }

    private void runScript(CXLReport rpt, String pName, Params pParams) {
      if(pName != null) {
        Object vArg1 = Type.Missing;
        if(pParams.Count > 0)
          vArg1 = pParams.getReportParameter(rpt, pParams[0].Name);
        Object vArg2 = Type.Missing;
        if(pParams.Count > 1)
          vArg2 = pParams.getReportParameter(rpt, pParams[1].Name);
        Object vArg3 = Type.Missing;
        if(pParams.Count > 2)
          vArg3 = pParams.getReportParameter(rpt, pParams[2].Name);
        Object vArg4 = Type.Missing;
        if(pParams.Count > 3)
          vArg3 = pParams.getReportParameter(rpt, pParams[3].Name);
        Object vArg5 = Type.Missing;
        if(pParams.Count > 4)
          vArg5 = pParams.getReportParameter(rpt, pParams[4].Name);
        Object vArg6 = Type.Missing;
        if(pParams.Count > 5)
          vArg6 = pParams.getReportParameter(rpt, pParams[5].Name);
        Object vArg7 = Type.Missing;
        if(pParams.Count > 6)
          vArg7 = pParams.getReportParameter(rpt, pParams[6].Name);
        Object vArg8 = Type.Missing;
        if(pParams.Count > 7)
          vArg8 = pParams.getReportParameter(rpt, pParams[7].Name);
        Object vArg9 = Type.Missing;
        if(pParams.Count > 8)
          vArg9 = pParams.getReportParameter(rpt, pParams[8].Name);
        Object vArg10 = Type.Missing;
        if(pParams.Count > 9)
          vArg10 = pParams.getReportParameter(rpt, pParams[9].Name);
        Object vArg11 = Type.Missing;
        if(pParams.Count > 10)
          vArg11 = pParams.getReportParameter(rpt, pParams[10].Name);
        Object vArg12 = Type.Missing;
        if(pParams.Count > 11)
          vArg12 = pParams.getReportParameter(rpt, pParams[11].Name);
        Object vArg13 = Type.Missing;
        if(pParams.Count > 12)
          vArg13 = pParams.getReportParameter(rpt, pParams[12].Name);
        Object vArg14 = Type.Missing;
        if(pParams.Count > 13)
          vArg14 = pParams.getReportParameter(rpt, pParams[13].Name);
        Object vArg15 = Type.Missing;
        if(pParams.Count > 14)
          vArg15 = pParams.getReportParameter(rpt, pParams[14].Name);
        Object vArg16 = Type.Missing;
        if(pParams.Count > 15)
          vArg16 = pParams.getReportParameter(rpt, pParams[15].Name);
        Object vArg17 = Type.Missing;
        if(pParams.Count > 16)
          vArg17 = pParams.getReportParameter(rpt, pParams[16].Name);
        Object vArg18 = Type.Missing;
        if(pParams.Count > 17)
          vArg18 = pParams.getReportParameter(rpt, pParams[17].Name);
        Object vArg19 = Type.Missing;
        if(pParams.Count > 18)
          vArg19 = pParams.getReportParameter(rpt, pParams[18].Name);
        Object vArg20 = Type.Missing;
        if(pParams.Count > 19)
          vArg20 = pParams.getReportParameter(rpt, pParams[19].Name);
        Object vArg21 = Type.Missing;
        if(pParams.Count > 20)
          vArg21 = pParams.getReportParameter(rpt, pParams[20].Name);
        Object vArg22 = Type.Missing;
        if(pParams.Count > 21)
          vArg22 = pParams.getReportParameter(rpt, pParams[21].Name);
        Object vArg23 = Type.Missing;
        if(pParams.Count > 22)
          vArg23 = pParams.getReportParameter(rpt, pParams[22].Name);
        Object vArg24 = Type.Missing;
        if(pParams.Count > 23)
          vArg24 = pParams.getReportParameter(rpt, pParams[23].Name);
        Object vArg25 = Type.Missing;
        if(pParams.Count > 24)
          vArg25 = pParams.getReportParameter(rpt, pParams[24].Name);
        Object vArg26 = Type.Missing;
        if(pParams.Count > 25)
          vArg26 = pParams.getReportParameter(rpt, pParams[25].Name);
        Object vArg27 = Type.Missing;
        if(pParams.Count > 26)
          vArg27 = pParams.getReportParameter(rpt, pParams[26].Name);
        Object vArg28 = Type.Missing;
        if(pParams.Count > 27)
          vArg28 = pParams.getReportParameter(rpt, pParams[27].Name);
        Object vArg29 = Type.Missing;
        if(pParams.Count > 28)
          vArg29 = pParams.getReportParameter(rpt, pParams[28].Name);
        Object vArg30 = Type.Missing;
        if(pParams.Count > 29)
          vArg30 = pParams.getReportParameter(rpt, pParams[29].Name);
        if(pParams.Count > 30)
          throw new EBioException("Количество входных параметров макроса не может быть больше 30");
        this.FWorkbook.Application.Run(pName,
          vArg1, vArg2, vArg3, vArg4, vArg5, vArg6, vArg7, vArg8, vArg9, vArg10,
          vArg11, vArg12, vArg13, vArg14, vArg15, vArg16, vArg17, vArg18, vArg19, vArg20,
          vArg21, vArg22, vArg23, vArg24, vArg25, vArg26, vArg27, vArg28, vArg29, vArg30);
      }
    }

    private VBIDE.VBComponent addSubToModule(String pModuleName, String pCode) {
      VBIDE.VBComponent vModule = null;
      int vCount = this.FWorkbook.VBProject.VBComponents.Count;
      for(int i = 1; i < vCount; i++)
        if(this.FWorkbook.VBProject.VBComponents.Item(i).Name.Equals(xlrModuleName)) {
          vModule = this.FWorkbook.VBProject.VBComponents.Item(i);
          break;
        }
      if(vModule == null) {
        vModule = this.FWorkbook.VBProject.VBComponents.Add(VBIDE.vbext_ComponentType.vbext_ct_StdModule);
        vModule.Name = xlrModuleName;
      }
      vModule.CodeModule.AddFromString(pCode);

      return vModule;
    }

    public Excel.Workbook Workbook  {
      get { return this.FWorkbook; }
    }

    public void SetRangeByName(String rangeName, Excel.Range range) {
      //Excel.Range vRslt = null;
      String vName = "";
      for (int i = 1; i <= this.Workbook.Names.Count; i++) {
        Excel.Name vXName = this.Workbook.Names.Item(i, Type.Missing, Type.Missing);
        vName = vXName.Name;
        if (vName.Equals(rangeName)) {
          vXName.RefersTo = range;
          //vRslt = vXName.RefersToRange;
          break;
        }
      }
      //return vRslt;
    }

    private void applyParamsToWS(Excel.Worksheet pWS) {
      /*
      try {
        this.Owner.ExcelSrv.getExcelApp();
        TXLRDefinition vRDef = this.Owner.RptDefinition;
        for(int i = 0; i < vRDef.RptParams.Count; i++) {
          String vPrmName = vRDef.RptParams[i].Name;
          String vPrmValue = vRDef.RptParams.GetReportParameter(vPrmName);
          pWS.Cells.Replace("#" + vPrmName + "#", vPrmValue, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        }
        String vBuffStr = vRDef.ThrowCode;
        pWS.Cells.Replace(TXLRDefinition.csThrowCodeParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.FileCode;
        pWS.Cells.Replace(TXLRDefinition.csRptFullCodeParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.Title;
        pWS.Cells.Replace(TXLRDefinition.csRptTitleParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.Subject;
        pWS.Cells.Replace(TXLRDefinition.csRptSubjectParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.Autor;
        pWS.Cells.Replace(TXLRDefinition.csRptAutorParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.DBConnStr;
        pWS.Cells.Replace(TXLRDefinition.csRptDBConnStrParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.SessionID;
        pWS.Cells.Replace(TXLRDefinition.csRptSessionIDParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.UserName;
        pWS.Cells.Replace(TXLRDefinition.csRptUserNameParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.RemoteIP;
        pWS.Cells.Replace(TXLRDefinition.csRptRemoteIPParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.RptPath;
        pWS.Cells.Replace(TXLRDefinition.csRptLocalPathParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.TmpPath;
        pWS.Cells.Replace(TXLRDefinition.csRptTmpPathParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
        vBuffStr = vRDef.LogPath;
        pWS.Cells.Replace(TXLRDefinition.csRptLogPathParamID, vBuffStr, Excel.XlLookAt.xlPart, Excel.XlSearchOrder.xlByRows, false, false, Type.Missing, Type.Missing);
      } finally {
        this.Owner.ExcelSrv.releaseExcelApp();
      }
       * */
    }

	}
}
