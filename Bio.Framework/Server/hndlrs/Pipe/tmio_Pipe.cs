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

    public tmio_Pipe(HttpContext context, AjaxRequest request)
      : base(context, request) {
    }

    protected override void doExecute() {
      base.doExecute();

      
      var vMode = Params.FindParamValue(this.QParams, "mod") as String;
      if(String.Equals(vMode, "read", StringComparison.CurrentCultureIgnoreCase)) {
        var vPipeName = Params.FindParamValue(this.QParams, "pipe") as String;
        var vConnStr = this.BioSession.Cfg.ConnectionString;

        BioResponse vRsp = ReadPipe(vPipeName, null);
        this.Context.Response.Write(vRsp.Encode());

      }
    }

    /// <summary>
    /// Данный exctract-ор работает для Сруктур посылаемых из ai_pipe.send
    /// </summary>
    /// <param name="vDataLine"></param>
    /// <returns></returns>
    public static String ExctractSessionID(ref String vDataLine) {
      String vResult = null;
      const string c_delimiter = "||";
      var vLines = Utl.SplitString(vDataLine, c_delimiter);
      if (vLines.Length > 0) {
        vResult = vLines[0];
        var vALines = new ArrayList(vLines);
        vALines.RemoveAt(0);
        vDataLine = Utl.CombineString((String[])vALines.ToArray(typeof(String)), c_delimiter);
      }
      return vResult;
    }

    /// <summary>
    /// Считывает
    /// </summary>
    public static BioResponse ReadPipe(String pipe, IDBSession dbSess) {
      var vResult = new BioResponse {
        Success = true
      };
      var v_sp = new RemoteProcessStatePack();

      if (!String.IsNullOrEmpty(pipe)) {
        var sql = "select ai_pipe.receive(:pipeName) as F_RSLT from dual";
        v_sp.pipe = pipe;
        try {
          var resObj = SQLCmd.ExecuteScalarSQL(dbSess, sql, new Params(new Param("pipeName", v_sp.pipe)), 120);
          var linesData = SQLUtils.ObjectAsString(resObj);
          if (linesData != null) {
            v_sp.sessionUID = ExctractSessionID(ref linesData);
            v_sp.lastPipedLines = new[] { linesData };
          }
        } catch (ThreadAbortException) {
          throw;
        } catch (Exception ex) {
          vResult.Ex = EBioException.CreateIfNotEBio(ex);
        }
      }
      vResult.RmtStatePacket = v_sp;
      return vResult;
    }

  }
}
