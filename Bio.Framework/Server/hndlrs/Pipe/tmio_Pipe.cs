namespace Bio.Framework.Server {

  using System;
  using System.Collections.Specialized;
  using System.Text;
  using System.Web;
  using System.Xml;
  using System.IO;
  using Bio.Helpers.Common.Types;
  using Bio.Framework.Packets;
  using Bio.Helpers.DOA;
  using Bio.Helpers.Common;
  using System.Collections;
  using System.Threading;

  /// <summary>
  /// Данный обработчик вх. сообщений является шаблоном для создания нового сообщения
  /// Обработчик запросов на отображение таблицы
  /// </summary>
  public class tmio_Pipe:ABioHandlerBio {

    public tmio_Pipe(HttpContext pContext, CAjaxRequest pRequest)
      : base(pContext, pRequest) {
    }

    protected override void doExecute() {
      base.doExecute();

      
      String vMode = CParams.FindParamValue(this.QParams, "mod") as String;
      if(String.Equals(vMode, "read", StringComparison.CurrentCultureIgnoreCase)) {
        String vPipeName = CParams.FindParamValue(this.QParams, "pipe") as String;
        //String vSQL = "select dralpha.ai_pipe.receive(:pPipeName) as F_RSLT from dual";
        String vConnStr = this.BioSession.Cfg.ConnectionString;
        /*using(OracleConnection vConn = this.BioSession.DBSess.GetConnection(vConnStr)) {
          Object vResObj = bioDOA.CSQLCmd.ExecuteScalarSQL(vSQL, vConn, new CParams(new CParam("pPipeName", vPipeName)));
          String vLinesData = bioDOA.utlDOA.ObjectAsString(vResObj);
          CParams vResult = new CParams();
          vResult.Add("lines", vLinesData);
          this.Context.Response.Write(new srvTP.CSimpleResult(true, vResult, null).ToJson());
          vConn.Close();
        }*/

        //String vPipeName = this.FPipeName;
        //String vConnStr = this.FOwner.MonitorSessionConnStr;
        CBioResponse vRsp = ReadPipe(vPipeName, null);
        //CParams vResult = new CParams();
        //vResult.Add("lines", vRsp.lines);
        this.Context.Response.Write(vRsp.Encode());

      }
    }

    /// <summary>
    /// Данный exctract-ор работает для Сруктур посылаемых из ai_pipe.send
    /// </summary>
    /// <param name="vDataLine"></param>
    /// <returns></returns>
    public static String exctractSessionID(ref String vDataLine) {
      String vResult = null;
      String csDelimiter = "||";
      String[] vLines = Utl.SplitString(vDataLine, csDelimiter);
      if (vLines.Length > 0) {
        vResult = vLines[0];
        ArrayList vALines = new ArrayList(vLines);
        vALines.RemoveAt(0);
        vDataLine = Utl.CombineString((String[])vALines.ToArray(typeof(String)), csDelimiter);
      }
      return vResult;
    }

    /// <summary>
    /// Считывает
    /// </summary>
    public static CBioResponse ReadPipe(String pipe, IDBSession dbSess) {
      CBioResponse vResult = new CBioResponse() {
        success = true
      };
      var v_sp = new CRemoteProcessStatePack();

      if (!String.IsNullOrEmpty(pipe)) {
        String vSQL = "select ai_pipe.receive(:pipeName) as F_RSLT from dual";
        v_sp.pipe = pipe;
        try {
          //if(!String.IsNullOrEmpty(connStr)) {
          //using(IDbConnection vConn = CDBFactory.CreateConnection(connStr, )) {
          //vConn.Open();
          Object vResObj = CSQLCmd.ExecuteScalarSQL(dbSess, vSQL, new CParams(new CParam("pipeName", v_sp.pipe)), 120);
          String vLinesData = SQLUtils.ObjectAsString(vResObj);
          if (vLinesData != null) {
            v_sp.sessionUID = exctractSessionID(ref vLinesData);
            v_sp.lastPipedLines = new String[] { vLinesData };
          }
          //  conn.Close();
          //}
          //}
        } catch (ThreadAbortException) {
          throw;
        } catch (Exception ex) {
          vResult.ex = EBioException.CreateIfNotEBio(ex);
        }
      } else {
        //vResult.SessionID = 
        //v_sp.sessionUID =
      }
      vResult.rmtStatePacket = v_sp;
      return vResult;
    }

  }
}
