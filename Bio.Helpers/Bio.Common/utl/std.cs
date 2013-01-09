namespace Bio.Helpers.Common {
  using System;
  //using System.Xml;
  //using System.Text;
  using System.Text.RegularExpressions;
  using System.IO;
#if !SILVERLIGHT
  using System.Web;
  using System.Windows.Forms;
  using System.Drawing;
  using System.Data;
  using Microsoft.Win32;
#else
  using System.Windows.Browser;
  using System.Windows.Controls;
  using System.Xml.Linq;
  using System.Windows.Resources;
#endif
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Globalization;
  using System.ComponentModel;
  using System.Threading;
  using System.Reflection;
  using System.Linq;

  using Bio.Helpers.Common.Types;
  using System.Net;
  using System.Windows;
  using System.Text;
  using System.Xml;
  using System.IO.IsolatedStorage;

  /// <summary>
  /// Утилиты общего назначения
  /// </summary>
  public class Utl {

    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String Enc_ISO_8859_1 = "ISO-8859-1";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String Enc_UTF_8 = "UTF-8";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String Enc_Cp866 = "Cp866";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String Enc_ISO_8859_5 = "ISO-8859-5";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String Enc_Windows_1251 = "WINDOWS-1251";
    /// <summary>
    /// 
    /// </summary>
    public const String SYS_ENCODING = Enc_UTF_8;

#if SILVERLIGHT
    public static System.Text.Encoding DefaultEncoding = System.Text.Encoding.UTF8;
#else
    public static System.Text.Encoding DefaultEncoding = System.Text.Encoding.Default;
#endif

    /// <summary>
    /// Константа - имя параметра запроса для быстого входа в систему
    /// </summary>
    public const String QLOGIN_PARNAME = "QLOGIN";
    /// <summary>
    /// Константа - имя параметра запроса для входа в систему по хэш-коду
    /// </summary>
    public const String HASHLOGIN_PARNAME = "HLGN";
    /// <summary>
    /// Константа - имя параметра запроса для быстого входа в систему
    /// </summary>
    public const String FLOGIN_PARNAME = "FLOGIN";
    /// <summary>
    /// Константа - имя параметра запроса cliname
    /// </summary>
    public const String CLINAME_PARNAME = "cliname";
    /// <summary>
    /// Константа - значение параметра запроса cliname
    /// </summary>
    public const String CLINAME_PARVALUE = "dalpha";
    /// <summary>
    /// Константа - шаблон для построения URL
    /// </summary>
    public const String SrvURLTemplate = "{0}/srv.aspx";

#if !SILVERLIGHT
    /// <summary>
    /// Вытаскивает из URL строки начение параметра по имени
    /// </summary>
    /// <param name="queryURI"></param>
    /// <param name="prm"></param>
    /// <returns></returns>
    public static String GetQueryParam(String queryURI, String prm) {
      if (queryURI != null) {
        String pQString = HttpUtility.UrlDecode(queryURI.Substring(queryURI.IndexOf("?") + 1));
        Char[] spr = new Char[] { '&' };
        String[] pars = pQString.Split(spr);
        for (int i = 0; i < pars.Length; i++) {
          String pName = prm + "=";
          if (pars[i].StartsWith(pName)) {
            String rslt = pars[i].Substring(pName.Length);
            if (rslt.Trim().Equals(""))
              return null;
            else
              return rslt;
          }
        }
      }
      return null;
    }
#endif

    /// <summary>
    /// кодирует ANSII -> UTF
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static String EncodeANSII2UTF(String msg) {
#if !SILVERLIGHT
      if (msg != null) {
        String Result = "";
        UTF8Encoding enc = new UTF8Encoding();
        byte[] bfr = enc.GetBytes(msg);
        Encoding.Convert(Encoding.GetEncoding(Enc_Windows_1251), Encoding.GetEncoding(SYS_ENCODING), bfr);
        String tmps = enc.GetString(bfr);
        Result = tmps;
        return Result;
      } else
        return null;
#else
      return msg;
#endif
    }

    /// <summary>
    /// Проверяет условие "(value == null) || (value == DBNull.Value)"
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Boolean IsNull(Object value) {
      return (value == null) || (value == DBNull.Value);
    }

    /// <summary>
    /// Если Null, то возвращает пустую строку
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static String NullToBlank(String value) {
      if (value == null)
        return "";
      else
        return value;
    }

    public static String NullToNULL(String value) {
      if (value == null)
        return "NULL";
      else if (value.Equals(""))
        return "NULL";
      else
        return value;
    }

    /// <summary>
    /// если на входе пустая строка или null, то возвращает ноль (нуна для перевода в double)
    /// </summary>
    /// <param name="str"></param>
    /// <returns>0 или входная строка</returns>
    public static String BlankTo0(String str) {
      String vStr = "";
      if ((str == "") || (str == null))
        vStr = "0";
      else
        vStr = str;
      return vStr;
    }

    /// <summary>
    /// строку в double
    /// </summary>
    /// <param name="prm"></param>
    /// <returns></returns>
    public static Double ToDbl(String prm) {
      NumberFormatInfo provider = new NumberFormatInfo();
      provider.NumberDecimalSeparator = ".";
      return Double.Parse(BlankTo0(prm.Replace(",", ".")), provider);
    }

    public static String BoolToStr(Boolean value) {
      return (value) ? "Y" : "N";
    }

#if !SILVERLIGHT
    public static String GetCookieByName(HttpCookieCollection vcook, String vName) {
      String Result = "null";
      for (int i = 0; i < vcook.Count; i++) {
        if (vcook[i].Name.Equals(vName))
          Result = vcook[i].Value;
      }
      return Result;
    }
#endif

    static public String encode(String newStr) {
      Char[] ar = new Char[newStr.Length];
      for (int i = 0; i < ar.Length; i++) {
        if (ar[i] == '"')
          ar[i] = '$';
        if (ar[i] == '\'')
          ar[i] = '*';
      }
      return new String(ar);
    }

    static public String decode(String newStr) {
      Char[] ar = new Char[newStr.Length];
      for (int i = 0; i < ar.Length; i++) {
        if (ar[i] == '$')
          ar[i] = '"';
        if (ar[i] == '*')
          ar[i] = '\'';
      }
      return new String(ar);
    }

    static public String nvl(String source, String target) {
      if (source != null) {
        return source;
      } else {
        return target;
      }
    }

    static public Int32 nvl(Int32 source, Int32 target) {
      if ((Object)source != null) {
        return source;
      } else {
        return target;
      }
    }

#if !SILVERLIGHT
    static public String GetMsg_ToAdminPlease(String vAdmName, XmlNodeList vAdmCntcts) {
      String rslt = "Обратитесь к администратору системы.<br>";
      rslt += "Контактная информация:<br>";
      for (int i = 0; i < vAdmCntcts.Count; i++) {
        XmlElement cCntct = (XmlElement)vAdmCntcts[i];
        String cc = "";
        if (cCntct.GetAttribute("type").Equals("office"))
          cc = "ком. №";
        else if (cCntct.GetAttribute("type").Equals("phone"))
          cc = "телефон";
        else if (cCntct.GetAttribute("type").Equals("mail"))
          cc = "эл. почта";
        rslt += cc + " : " + ((XmlElement)cCntct).InnerText + "<br>";
      }
      rslt += "имя : " + vAdmName + "<br>";
      return rslt;
    }
#endif

    public static String NormalizeURL(String appURL, String url) {
      if (url != null)
        return url.Replace("SYS_APP_URL", appURL);
      else
        return null;
    }

    public static String[] SplitString(String str, Char[] delimeter) {
      if (str != null) {
        String[] aRows = str.Split(delimeter);
        return aRows;
      } else
        return new String[0];
    }
    public static String[] SplitString(String str, Char delimiter) {
      return SplitString(str, new Char[] { delimiter });
    }


    public static String[] SplitString(String str, String[] delimeter) {
      if (!String.IsNullOrEmpty(str)) {
        String vLine = str;
        String v_dlmtr = null;
        if (delimeter.Length > 1) {
          const String csDlmtrPG = "#inner_pg_delimeter_str#";
          for (int i = 0; i < delimeter.Length; i++)
            vLine = vLine.Replace(delimeter[i], csDlmtrPG);
        } else
          v_dlmtr = delimeter.FirstOrDefault();
        IList<String> vList = new List<String>();
        int v_item_bgn = 0;
        //int v_item_end = 0;
        while (v_item_bgn <= vLine.Length) {
          String v_line2Add = String.Empty;
          int v_dlmtrPos = vLine.IndexOf(v_dlmtr, v_item_bgn);
          if (v_dlmtrPos == -1)
            v_dlmtrPos = vLine.Length;
          v_line2Add = vLine.Substring(v_item_bgn, v_dlmtrPos - v_item_bgn);
          vList.Add(v_line2Add);
          v_item_bgn += v_line2Add.Length + v_dlmtr.Length;
        }
        return (String[])vList.ToArray();
      } else
        return new String[] { };
    }

    public static String[] SplitString(String str, String delimiter) {
      return SplitString(str, new String[] { delimiter });
    }

    public static String CombineString(String[] lines, String delimiter) {
      String vResult = null;
      foreach (String vLine in lines)
        appendStr(ref vResult, vLine, delimiter);
      return vResult;
    }


    public static String UrlEncode(String line) {
      return line.Replace("&", "&amp;");
    }

    public static String UrlDecode(String line) {
      return line.Replace("&amp;", "&");
    }

    public static int Mod(int x, int y) {
      return x - (x / y) * y;
    }

    private static Mutex FMutexOfLogFile = new Mutex();
    public static void AppendStringToFile(String fileName, String line, Encoding encoding, Boolean createPath) {
      FMutexOfLogFile.WaitOne();
      try {
        if (!Directory.Exists(Path.GetDirectoryName(fileName)) && createPath)
          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        Encoding vEncode = encoding;
        if (vEncode == null)
          vEncode = Utl.DefaultEncoding;
        using (StreamWriter sw = new StreamWriter(fs, vEncode)) {
          sw.WriteLine(line);
          sw.Flush();
          sw.Close();
        }
      } finally {
        FMutexOfLogFile.ReleaseMutex();
      }
    }

    public static void AppendStringToFile(String fileName, String line, Encoding encoding) {
      AppendStringToFile(fileName, line, encoding, false);
    }

    public static void SaveStringToFile(String fileName, String line, Encoding encoding) {
      if (File.Exists(fileName))
        File.Delete(fileName);
      AppendStringToFile(fileName, line, encoding);
    }

    public static void AddObjToLine(ref String line, String delimiter, Object obj) {
      if (obj != null) {
        if (line == null)
          line = obj.ToString();
        else
          line += delimiter + obj.ToString();
      } else {
        if (line == null)
          line = "";
        else
          line += delimiter + "";
      }
    }

    public static void ReadBinFileInBuffer(ref byte[] buffer, String fileName) {
      if (File.Exists(fileName)) {
        FileInfo vFileInf = new FileInfo(fileName);
        long vFSize = vFileInf.Length;
        buffer = new byte[vFSize];
        FileStream vFStr = new FileStream(fileName, FileMode.Open);
        try {
          BinaryReader vBr = new BinaryReader(vFStr);
          int vFSizeInt = (int)vFSize;
          vBr.Read(buffer, 0, vFSizeInt);
        } finally {
          vFStr.Close();
        }
      }
    }

    public static void WriteBuffer2BinFile(String fileName, byte[] buffer) {
      if (File.Exists(fileName))
        File.Delete(fileName);
      FileStream vFStr = new FileStream(fileName, FileMode.CreateNew);
      try {
        BinaryWriter vBr = new BinaryWriter(vFStr);
        vBr.Write(buffer, 0, buffer.Length);
      } finally {
        vFStr.Close();
      }
    }


    public static long StrTimePeriodToMilliseconds(String period) {
      Int64 vRslt = 0;
      if (period != null) {
        int vMult = 1;
        String vPeriod = period.ToUpper();
        Char vPeriodType = vPeriod[vPeriod.Length - 1];
        if (vPeriodType == 'S') {
          vMult = 1000;
          vPeriod = vPeriod.Substring(0, vPeriod.Length - 1);
        } else if (vPeriodType == 'M') {
          vMult = 1000 * 60;
          vPeriod = vPeriod.Substring(0, vPeriod.Length - 1);
        } else if (vPeriodType == 'H') {
          vMult = 1000 * 3600;
          vPeriod = vPeriod.Substring(0, vPeriod.Length - 1);
        } else if (vPeriodType == 'D') {
          vMult = 1000 * 3600 * 24;
          vPeriod = vPeriod.Substring(0, vPeriod.Length - 1);
        }
        vRslt = Int64.Parse(vPeriod) * vMult;
      }
      return vRslt;
    }

    public static void TruncArrayLeft(ref String[] arr, int cnt) {
      String[] vList1 = new String[arr.Length - 1];
      for (int i = cnt; i < arr.Length; i++)
        vList1[i - cnt] = arr[i];
      arr = vList1;
    }

    public static String CutElementFromDelimitedLine(ref String line, String delimiter) {
      String vResult = null;
      String[] vList = Utl.SplitString(line, delimiter);
      if (vList.Length > 0) {
        vResult = vList[0];
        if (vList.Length > 1) {
          TruncArrayLeft(ref vList, 1);
          line = CombineString(vList, delimiter);
        } else {
          line = null;
        }
      }
      return vResult;
    }

    public static bool IsElementInDelimitedLine(String line, String elem, Char delimiter) {
      bool vRslt = false;
      String[] vList = Utl.SplitString(line, delimiter);
      for (int i = 0; i < vList.Length; i++) {
        if (vList[i].Equals(elem)) {
          vRslt = true;
          break;
        }
      }
      return vRslt;
    }

    public static bool IsElementInDelimitedLine(String line, String elem, Char[] delimeter) {
      String[] lst = Utl.SplitString(line, delimeter);
      for (int i = 0; i < lst.Length; i++)
        if (lst[i].Equals(elem))
          return true;
      return false;
    }

    public static bool DelimitedLineHasCommonTags(String line1, String line2, Char[] delimeter) {
      String[] lst = Utl.SplitString(line1, delimeter);
      for (int i = 0; i < lst.Length; i++)
        if (IsElementInDelimitedLine(line2, lst[i], delimeter))
          return true;
      return false;
    }

    public static bool CheckRoles(String objRoles, String userRoles, Char[] delimeter) {
      String vObjectRoles = objRoles;
      if ((vObjectRoles == null) || (vObjectRoles.Equals("")))
        vObjectRoles = "*";

      bool vResult = false;
      // Проверяем наличие * в pObjectRoles
      vResult = IsElementInDelimitedLine(vObjectRoles, "*", delimeter);

      if (!vResult) {
        //* нет в pObjectRoles проверяем пересечение
        vResult = DelimitedLineHasCommonTags(vObjectRoles, userRoles, delimeter);
      }

      if (vResult) {
        //Проверяем наличие исключающих ролей
        String[] lst = userRoles.Split(delimeter);
        for (int i = 0; i < lst.Length; i++)
          if (IsElementInDelimitedLine(vObjectRoles, "!" + lst[i], delimeter))
            vResult = false;
      }

      return vResult;
    }

    public static String ForceDirectory(String path) {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      return path;
    }

    public static bool CheckBoolValue(String value) {
      return (value != null) && ((value.ToUpper().Equals("true")) || (value.ToUpper().Equals("TRUE")));
    }


    public static String ArrayToHTML(String[] array) {
      String res = "";
      for (int i = 0; i < array.Length; i++) {
        if (res.Equals(""))
          res += "\"" + array[i] + "\"";
        else
          res += ",\"" + array[i] + "\"";
      }
      return res;
    }

    public static String NormalizeDir(String dir) {
      if (!String.IsNullOrEmpty(dir)) {
        String Result = dir.Trim();
        if (Result.Substring(Result.Length - 1, 1) != "\\")
          Result = Result + "\\";
        return Result;
      } else
        return dir;
    }

    public static String FullPath(String path, String rootPath) {
      String Result = path.Trim();
      Regex vr = new Regex("^\\D[:].*");
      if (!vr.IsMatch(Result)) {
        if (!Result.Substring(0, 3).Equals("..\\")) {
          if (!Result.Substring(0, 1).Equals("\\"))
            Result = NormalizeDir(rootPath) + Result;
          else
            Result = NormalizeDir(rootPath) + Result.Substring(1);
        } else
          Result = NormalizeDir(Path.GetFullPath(rootPath));
      }
      Result = NormalizeDir(Result);
      return Result;
    }

    public static String genBioLocalPath(String bioCode) {
      int fLstIndx = bioCode.LastIndexOf(".");
      if (fLstIndx >= 0)
        return bioCode.Substring(0, fLstIndx + 1).Replace(".", "\\");
      else
        return null;
    }

    /// <summary>
    /// Сравнивает две версии
    /// </summary>
    /// <param name="verLeft"></param>
    /// <param name="verRight"></param>
    /// <returns>[-1]-меньше; [0]-равно; [1]-больше</returns>
    public static int compareVer(String verLeft, String verRight) {
      String[] vVerLeft = SplitString(verLeft, '.');
      String[] vVerRight = SplitString(verRight, '.');
      int vUpIndex = Math.Max(vVerLeft.Length, vVerRight.Length);
      for (int i = 0; i < vUpIndex; i++) {
        int vIntLeft = (i < vVerLeft.Length) ? Int32.Parse(vVerLeft[i]) : 0;
        int vIntRight = (i < vVerRight.Length) ? Int32.Parse(vVerRight[i]) : 0;
        if (vIntLeft < vIntRight) return -1;
        else if (vIntLeft > vIntRight) return 1;
      }
      return 0;
    }


    /// <summary>
    /// Удаляет содержимое папки
    /// </summary>
    /// <param name="path"></param>
    public static void clearDir(String path) {
      try {
        if (Directory.Exists(path))
          Directory.Delete(path, true);
        Directory.CreateDirectory(path);
      } catch { }
    }

    /// <summary>
    /// Разбирает логин вида [имя пользователя]/[пароль]
    /// </summary>
    /// <param name="login"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    public static void parsLogin(String login, ref String user, ref String password) {
      user = null;
      password = null;
      String[] lst = Utl.SplitString(login, '/');
      if (lst.Length > 0)
        user = lst[0];
      if (lst.Length > 1)
        password = lst[1];
    }

    /// <summary>
    /// Проверяет регулярное вырожение для строки. Возвращает Match.Success.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static Boolean regexMatch(String line, String regex, Boolean ignoreCase) {
      Regex vr = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      Match vM = vr.Match(line);
      return vM.Success;
    }

    /// <summary>
    /// Проверяет регулярное вырожение для строки. Возвращает Match.Value.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static String regexFind(String line, String regex, Boolean ignoreCase) {
      Regex vr = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      Match vM = vr.Match(line);
      return vM.Value;
    }

    /// <summary>
    /// Ищет позицию вхождения строки с помощью регулярного вырожения
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static Int32 regexPos(String line, String regex, Boolean ignoreCase) {
      Regex vr = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      Match vM = vr.Match(line);
      if (vM.Success)
        return vM.Index;
      else
        return -1;
    }

    /// <summary>
    /// Заменяет подстроку с помощью регулярного вырожения
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="rplcmnt"></param>
    /// <param name="ignoreCase"></param>
    public static void regexReplace(ref String line, String regex, String rplcmnt, Boolean ignoreCase) {
      Regex vr = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      line = vr.Replace(line, rplcmnt);
    }

    public static IDictionary<String, String> parsConnectionStr(String connStr) {
      IDictionary<String, String> vRslt = new Dictionary<String, String>();
      if (connStr != null) {
        Char[] spr = new Char[] { ';' };
        String[] lst = connStr.Split(spr);
        for (int i = 0; i < lst.Length; i++) {
          String fpar = lst[i];
          spr[0] = '=';
          String[] ppp = fpar.Split(spr);
          String pName = null;
          String pValue = null;
          if (ppp.Length > 0)
            pName = ppp[0];
          if (ppp.Length > 1)
            pValue = ppp[1];
          vRslt.Add(pName, pValue);
        }
      }
      return vRslt;
    }

    public static String buildConnectionStr(IDictionary<String, String> connStr) {
      String vRslt = null;
      if (connStr != null) {
        foreach (KeyValuePair<String, String> vItem in connStr) {
          Utl.appendStr(ref vRslt, vItem.Key + "=" + vItem.Value, ";");
        }
      }
      return vRslt;
    }


    public static String getFncName(String eveName) {
      if (eveName != null) {
        int vFncNamEnd = eveName.IndexOf('(');
        if (vFncNamEnd >= 0)
          return eveName.Substring(0, eveName.Length - (eveName.Length - vFncNamEnd));
      }
      return null;
    }

    public static void appendStr(ref String line, String str, String delimiter) {
      if (String.IsNullOrEmpty(line))
        line = str;
      else
        line += delimiter + str;
    }

    public static String bldURL(String serverHost, String requestType) {
      String vURL = String.Format(SrvURLTemplate, serverHost);
      if (requestType != null)
        vURL = vURL + "?mtp=" + requestType;
      return vURL;
    }

    public static String extractUsrNameFromLogin(String login) {
      String usrName = null; String psswrd = null;
      parsLogin(login, ref usrName, ref psswrd);
      return usrName;
    }

    public static String extractUrlQueryString(String url) {
      String[] lst = Utl.SplitString(url, '?');
      if (lst.Length > 1)
        return lst[1];
      else
        return null;
    }

    public static String extractUsrPwdFromLogin(String login) {
      String usrName = null; String psswrd = null;
      parsLogin(login, ref usrName, ref psswrd);
      return psswrd;
    }

#if !SILVERLIGHT 
    public static Boolean columnExists(DataColumnCollection cols, String fieldName) {
      if (!String.IsNullOrEmpty(fieldName)) {
        foreach (DataColumn vCol in cols) {
          if (String.Equals(vCol.ColumnName, fieldName, StringComparison.CurrentCultureIgnoreCase)) {
            return true;
          }
        }
      }
      return false;
    }

    public static void dataRowSetValue(DataRow pRow, String pFieldName, Object pValue) {
      foreach (DataColumn vCol in pRow.Table.Columns) {
        if (vCol.ColumnName.ToUpper().Equals(pFieldName.ToUpper())) {
          pRow[vCol.ColumnName] = Convert2Type(pValue, vCol.DataType);
        }
      }
    }

    public static Object dataRowGetValue(DataRow pRow, String pFieldName, Object pIfNull) {
      Object vResult = null;
      if (pRow != null && pRow.RowState != DataRowState.Detached) {
        foreach (DataColumn vCol in pRow.Table.Columns)
          if (vCol.ColumnName.ToUpper().Equals(pFieldName.ToUpper())) {
            vResult = pRow[vCol.ColumnName];
            break;
          }
      }
      return (vResult == null) ? pIfNull : vResult;
    }
#endif

    public static bool typeIsNumeric(Type pType) {
      return pType.Equals(typeof(Double)) || pType.Equals(typeof(Double?)) ||
             pType.Equals(typeof(float)) || pType.Equals(typeof(float?)) ||
             pType.Equals(typeof(Decimal)) || pType.Equals(typeof(Decimal?)) ||
             pType.Equals(typeof(Int16)) || pType.Equals(typeof(Int16?)) ||
             pType.Equals(typeof(Int32)) || pType.Equals(typeof(Int32?)) ||
             pType.Equals(typeof(Int64)) || pType.Equals(typeof(Int64?)) ||
             pType.Equals(typeof(UInt16)) || pType.Equals(typeof(UInt16?)) ||
             pType.Equals(typeof(UInt32)) || pType.Equals(typeof(UInt32?)) ||
             pType.Equals(typeof(UInt64)) || pType.Equals(typeof(UInt64?));
    }

    public static String ObjectAsString(Object pObject) {
      return Convert2Type<String>(pObject);
    }

    private static String _objectAsString(Object pObject) {
      String rslt = null;
      if (pObject != null) {
        Type tp = pObject.GetType();
        if (tp.Equals(typeof(System.DBNull)))
          rslt = null;
        else if (tp.Equals(typeof(System.String)))
          rslt = pObject.ToString();
        else {
#if !SILVERLIGHT
          CultureInfo culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true);
#else
          CultureInfo culture = new CultureInfo(CultureInfo.CurrentCulture.Name);
#endif
          culture.NumberFormat.NumberDecimalSeparator = ".";
#if !SILVERLIGHT
          culture.DateTimeFormat.DateSeparator = "-";
#else
          //culture.DateTimeFormat. = "-";
#endif
          culture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy H:mm:ss";
          culture.DateTimeFormat.LongTimePattern = "";
          rslt = Convert.ToString(pObject, culture);
          if (tp.Equals(typeof(System.DateTime)))
            rslt = rslt.Trim();
        }
      }
      return EncodeANSII2UTF(rslt);
    }

    private static Object convertFromNullable(Object value) {
      if (value == null)
        return null;
      Type conversionType = value.GetType();
      if (conversionType.IsGenericType &&
        conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
        Type nnType = Nullable.GetUnderlyingType(conversionType);
        if (nnType.Equals(typeof(Boolean))) return (Boolean)value;
        else if (nnType.Equals(typeof(Int16))) return (Int16)value;
        else if (nnType.Equals(typeof(Int32))) return (Int32)value;
        else if (nnType.Equals(typeof(Int64))) return (Int64)value;
        else if (nnType.Equals(typeof(Decimal))) return (Decimal)value;
        else if (nnType.Equals(typeof(Double))) return (Double)value;
        else if (nnType.Equals(typeof(Single))) return (Single)value;
        else if (nnType.Equals(typeof(DateTime))) return (DateTime)value;
        else
          throw new Exception(String.Format("Невозможно конвертировать {0} в {1}", conversionType.Name, nnType.Name));
      } else
        return value;
    }

    private static Object convertToNullable(Object value) {
      if (value == null)
        return null;
      Type conversionType = value.GetType();
      if (conversionType.IsGenericType &&
        !conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
        if (conversionType.Equals(typeof(Boolean))) return (Boolean?)value;
        else if (conversionType.Equals(typeof(Int16))) return (Int16?)value;
        else if (conversionType.Equals(typeof(Int32))) return (Int32?)value;
        else if (conversionType.Equals(typeof(Int64))) return (Int64?)value;
        else if (conversionType.Equals(typeof(Decimal))) return (Decimal?)value;
        else if (conversionType.Equals(typeof(Double))) return (Double?)value;
        else if (conversionType.Equals(typeof(Single))) return (Single?)value;
        else if (conversionType.Equals(typeof(DateTime))) return (DateTime?)value;
        else
          return value;
      } else
        return value;
    }

    /// <summary>
    /// Преобразует значение к конкретному типу
    /// </summary>
    /// <param name="inValue"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static Object Convert2Type(Object inValue, Type targetType) {
      if ((inValue != null) && (targetType != null) && inValue.GetType().Equals(targetType))
        return inValue;
      if ((targetType != null) && (targetType != typeof(Object))) {
        Type tp = ((inValue == null) || (inValue == DBNull.Value)) ? null : inValue.GetType();

        Type v_out_type = Nullable.GetUnderlyingType(targetType);
        Boolean v_out_isNullable = v_out_type != null;
        v_out_type = v_out_type ?? targetType;
        Boolean v_out_isClass = v_out_type.IsClass;
        if (tp == null) {
          if (v_out_isNullable || v_out_isClass)
            return null;
          else {
            if (v_out_type.Equals(typeof(String)) || v_out_type.Equals(typeof(Object)))
              return null;
            else if (v_out_type.Equals(typeof(DateTime)))
              return DateTime.MinValue;
            else if (v_out_type.Equals(typeof(Boolean)))
              return false;
            else if (typeIsNumeric(v_out_type)) {
              IFormatProvider ifp = CultureInfo.CurrentCulture.NumberFormat;
              return Convert.ChangeType(0, v_out_type, ifp);
            } else
              throw new Exception("Значение null не может быть представлено как " + v_out_type.Name + "!!! ", null);
          }
        }

        Type v_in_type = Nullable.GetUnderlyingType(tp);
        Boolean v_in_isNullable = v_in_type != null;
        v_in_type = v_in_type ?? tp;

        if ((inValue == null) && (v_out_isNullable))
          return null;

        if (v_in_isNullable)
          inValue = convertFromNullable(inValue);

        Object v_rslt = null;
        if (v_out_type.Equals(typeof(DateTime))) {
          if (inValue == null) {
            v_rslt = DateTime.MinValue;
          } else if (v_in_type.Equals(typeof(DateTime))) {
            v_rslt = inValue;
          } else if (v_in_type.Equals(typeof(String))) {
            v_rslt = DateTimeParser.Instance.ParsDateTime((String)inValue);
          } else {
            throw new Exception("Значение типа " + tp + " не может быть представлено как DateTime!!! ", null);
          }

        } else if (v_out_type.Equals(typeof(Boolean))) {
          if (inValue == null)
            v_rslt = false;
          else if (v_in_type.Equals(typeof(Boolean)))
            v_rslt = inValue;
          else if (typeIsNumeric(v_in_type)) {
            Decimal v_invalDec = (Decimal)Convert.ChangeType(inValue, typeof(Decimal), CultureInfo.CurrentCulture.NumberFormat);
            v_rslt = (!v_invalDec.Equals(new Decimal(0)));
          } else if (v_in_type.Equals(typeof(String))) {
            String vValStr = ((String)inValue).ToUpper();
            v_rslt = (vValStr.Equals("1") || vValStr.Equals("Y") || vValStr.Equals("T") || vValStr.ToUpper().Equals("TRUE") || vValStr.ToUpper().Equals("ON"));
          } else {
            throw new Exception("Значение типа " + tp + " не может быть представлено как boolean!!! ", null);
          }
        } else if (typeIsNumeric(v_out_type)) {
          IFormatProvider ifp = CultureInfo.CurrentCulture.NumberFormat;//new NumberFormatInfo();
          if (inValue == null)
            v_rslt = Convert.ChangeType(0, v_out_type, ifp);
          if (typeIsNumeric(v_in_type)) {
            v_rslt = Convert.ChangeType(inValue, v_out_type, ifp);
          } else if (v_in_type.Equals(typeof(Boolean))) {
            v_rslt = ((Boolean)inValue) ? 1 : 0;
          } else if (v_in_type.Equals(typeof(String))) {
            String vValStr = (String)inValue;
            vValStr = String.IsNullOrEmpty(vValStr) ? "0" : vValStr;
            String vDecSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            vValStr = vValStr.Replace(",", vDecSep);
            vValStr = vValStr.Replace(".", vDecSep);
            try {
              v_rslt = Convert.ChangeType(vValStr, v_out_type, ifp);
            } catch (Exception ex) {
              throw new Exception("Значение [" + vValStr + "] типа " + v_in_type.Name + " не может быть представлено как Numeric!!! ", null);
            }
          } else {
            throw new Exception("Значение типа " + tp + " не может быть представлено как Numeric!!! ", null);
          }
          if (v_out_isNullable)
            v_rslt = convertToNullable(v_rslt);

        } else if (v_out_type.Equals(typeof(String))) {
          v_rslt = _objectAsString(inValue);
        }

        return v_rslt;
      }

      return inValue;
    }

    /// <summary>
    /// Преобразует значение к конкретному типу
    /// </summary>
    /// <typeparam name="pTargetType">Тип</typeparam>
    /// <param name="pValue">Значение</param>
    /// <returns></returns>
    public static pTargetType Convert2Type<pTargetType>(Object pValue) {
      return (pTargetType)Convert2Type(pValue, typeof(pTargetType));
    }

    /// <summary>
    /// Уменьшает период на 1
    /// </summary>
    /// <param name="pPeriod">Период в формате YYYYMM</param>
    /// <returns>Период в формате YYYYMM</returns>
    public static String periodDec(String pPeriod) {
      if (pPeriod != null) {
        int vYear = Int32.Parse(pPeriod.Substring(0, 4));
        int vMonth = Int32.Parse(pPeriod.Substring(4, 2));
        vMonth--;
        if (vMonth == 0) {
          vYear--;
          vMonth = 12;
        }
        return String.Format("{0:0000}{1:00}", vYear, vMonth);
      } else
        return null;
    }

    /// <summary>
    /// Увеличивает период на 1
    /// </summary>
    /// <param name="pPeriod">Период в формате YYYYMM</param>
    /// <returns>Период в формате YYYYMM</returns>
    public static String periodInc(String pPeriod) {
      int vYear = Int32.Parse(pPeriod.Substring(0, 4));
      int vMonth = Int32.Parse(pPeriod.Substring(4, 2));
      vMonth++;
      if (vMonth == 13) {
        vYear++;
        vMonth = 1;
      }
      return String.Format("{0:0000}{1:00}", vYear, vMonth);
    }

    /// <summary>
    /// Загружает текстовый файл (Windows-1251) в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadWINFile(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, Enc_Windows_1251, ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл (cp866) в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadDOSFile(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, Enc_Cp866, ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл (UTF-8) в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadUTF8File(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, Enc_UTF_8, ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pEcoding">Имя кодировки</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadStrFile(String pFileName, String pEcoding, ref String pBuff) {
      LoadStrFile(pFileName, System.Text.Encoding.GetEncoding(pEcoding), ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pEcoding">Кодировка</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadStrFile(String pFileName, Encoding pEcoding, ref String pBuff) {
      if (File.Exists(pFileName)) {
        FileStream fs = new FileStream(pFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        StreamReader fFile = new StreamReader(fs, pEcoding);
        String vLine = null;
        StringWriter bfr = new StringWriter();
        while ((vLine = fFile.ReadLine()) != null)
          bfr.WriteLine(vLine);
        pBuff = bfr.ToString();
        fs.Close();
      }

    }

    public static List<String> LoadStrFile(String pFileName, Encoding pEcoding) {
      var v_rslt = new List<String>();
      if (File.Exists(pFileName)) {
        FileStream fs = new FileStream(pFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        try {
          StreamReader fFile = new StreamReader(fs, pEcoding);
          String vLine = null;
          while ((vLine = fFile.ReadLine()) != null)
            v_rslt.Add(vLine);
        } finally {
          fs.Close();
        }
      }
      return v_rslt;
    }

#if !SILVERLIGHT
    /// <summary>
    /// Выполняет dlg в потоке pControl
    /// </summary>
    /// <param name="pControl"></param>
    /// <param name="dlg"></param>
    public static void runDelegateOnControl(Control pControl, Action dlg) {
      if(pControl.IsHandleCreated && !pControl.IsDisposed && !pControl.Disposing) {
        try {
          if(pControl.InvokeRequired)
            pControl.Invoke(dlg);
          else
            dlg();
        } catch(ObjectDisposedException) { }
      }
    }


#endif

    public static PropertyInfo GetPropertyInfo(Type type, String propertyName, Boolean caseSensetive) {
      if (type != null) {
        PropertyInfo prop = type.GetProperties().Where((p) => { return p.Name.Equals(propertyName, (caseSensetive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)); }).FirstOrDefault();
        return prop;
      } else
        return null;
    }

    public static PropertyInfo GetPropertyInfo(Type type, String propertyName) {
      return GetPropertyInfo(type, propertyName, true);
    }

    public static AttrType GetPropertyAttr<AttrType>(PropertyInfo prop) where AttrType : Attribute {
      if (prop != null) {
        Object[] attrs = prop.GetCustomAttributes(typeof(AttrType), true);
        if (attrs.Length > 0) {
          return attrs[0] as AttrType;
        } else
          return null;
      } else
        return null;
    }

    public static AttrType GetPropertyAttr<AttrType>(Type type, String propertyName) where AttrType : Attribute {
      PropertyInfo prop = GetPropertyInfo(type, propertyName);
      return GetPropertyAttr<AttrType>(prop);
    }

    public static void SetPropertyAttr<AttrType>(Type type, String propertyName, String attrPropertyName, Object attrPropValue) where AttrType : Attribute {
      PropertyInfo prop = GetPropertyInfo(type, propertyName);
      var attr = GetPropertyAttr<AttrType>(prop);
      if (attr != null)
        Utl.SetPropertyValue(attr, attrPropertyName, attrPropValue);
    }

#if !SILVERLIGHT
    public static A GetAttributeOfProp<T, A>(String propName) where A:Attribute {
      PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(T));
      foreach (PropertyDescriptor pd in pdc) {
        if (String.Equals(pd.Name, propName, StringComparison.CurrentCulture)) {
          A attr = (A)pd.Attributes[typeof(A)];
          if (attr != null)
            return attr;
        }
      }
      return null;
    }

    /// <summary>
    /// Прикручивает к имени файла индекс если такой фай существует. 
    /// Например: 1) Если файл "some_file.ext" существует, то функция вернет "some_file(1).ext"
    ///           2) Если файл "some_file(1).ext" существует, то функция вернет "some_file(2).ext"
    ///           и т.д.
    /// </summary>
    /// <param name="pFileName"></param>
    /// <returns></returns>
    public static String incFileNameIndexIfExists(String pFileName) {
      String vResult = pFileName;
      if(File.Exists(pFileName)) {
        String vExt = Path.GetExtension(vResult);
        Regex vr = new Regex("[(]\\d+[)][\\.]", RegexOptions.IgnoreCase);
        Match vMatch = vr.Match(vResult);
        if(vMatch.Success) {
          String vNumStr = vMatch.Value.Substring(1, vMatch.Value.Length - 3);
          Int16 vNum = Int16.Parse(vNumStr); vNum++;
          vResult = vr.Replace(vResult, "(" + vNum + ").");
        } else
          vResult = Utl.NormalizeDir(Path.GetDirectoryName(vResult)) + Path.GetFileNameWithoutExtension(vResult) + "(1)" + vExt;
      }
      return vResult;
    }

    /// <summary>
    /// Проверяет является ли данный vText именем файла, если да, 
    /// то загружает содержимое этого файла а vText
    /// </summary>
    /// <param name="pCurrentPath"></param>
    /// <param name="vText"></param>
    public static void tryLoadTextAsFile(String pCurrentPath, ref String vText) {
      String vCurrentDirectory = Directory.GetCurrentDirectory();
      String vCurrentPath = Path.GetFullPath(pCurrentPath);
      Directory.SetCurrentDirectory(vCurrentPath);
      try {
        String vSQLFileFN = vText;
        if(File.Exists(vSQLFileFN)) {
          try {
            Bio.Helpers.Common.Utl.LoadWINFile(vSQLFileFN, ref vText);
          } catch(Exception ex) {
            throw new Exception("Ошибка при загрузке файла [" + vSQLFileFN + "]. Сообщение: " + ex.Message);
          }
        }
      } finally {
        Directory.SetCurrentDirectory(vCurrentDirectory);
      }
    }

    /// <summary>
    /// Ищет в vText переменные вида {text-file:..\ftw.sql} c именем файла, если находит, 
    /// то загружает содержимое соответствующего файла в соответствующую позицию vText
    /// </summary>
    /// <param name="pCurrentPath"></param>
    /// <param name="vText"></param>
    public static void tryLoadMappedFiles(String pCurrentPath, ref String vText) {
      String csTemplate = "{text-file:[^{]+?}";
      Regex vr = new Regex(csTemplate, RegexOptions.IgnoreCase);
      Match vMatch = vr.Match(vText);
      if(vMatch.Success) {
        String vFileContext = vMatch.Value.Substring(11, vMatch.Value.Length - 12);
        tryLoadTextAsFile(pCurrentPath, ref vFileContext);
        vText = vText.Replace(vMatch.Value, vFileContext);
      } 
    }

    public static void DrawAnSeldCell(Rectangle cellBounds, Boolean focused, Graphics gra, AnchorStyles borders) {
      Color col1 = Color.RoyalBlue;
      Color col2 = Color.Blue;
      if (!focused) {
        col1 = Color.LightGray;
        col2 = Color.Gray;
      }

      Pen vPen = null;
      //left  
      if ((borders & AnchorStyles.Left) == AnchorStyles.Left) {
        vPen = new Pen(col1, 2);
        gra.DrawLine(vPen, new Point(cellBounds.X, cellBounds.Y), new Point(cellBounds.X, cellBounds.Y + cellBounds.Height));
      }
      //right
      if ((borders & AnchorStyles.Right) == AnchorStyles.Right) {
        vPen = new Pen(col2, 2);
        gra.DrawLine(vPen, new Point(cellBounds.X + cellBounds.Width, cellBounds.Y), new Point(cellBounds.X + cellBounds.Width, cellBounds.Y + cellBounds.Height));
      }
      //top
      if ((borders & AnchorStyles.Top) == AnchorStyles.Top) {
        vPen = new Pen(col1, 2);
        gra.DrawLine(vPen, new Point(cellBounds.X, cellBounds.Y), new Point(cellBounds.X + cellBounds.Width, cellBounds.Y));
      }
      //bottom
      if ((borders & AnchorStyles.Bottom) == AnchorStyles.Bottom) {
        vPen = new Pen(col2, 2);
        gra.DrawLine(vPen, new Point(cellBounds.X, cellBounds.Y + cellBounds.Height), new Point(cellBounds.X + cellBounds.Width, cellBounds.Y + cellBounds.Height));
      }
    }
#endif

#if !SILVERLIGHT
    private static void drawSeldCell(DataGridView grid, DataGridViewCellPaintingEventArgs a, Boolean focused) {
      if((a.State & DataGridViewElementStates.Selected) ==
                DataGridViewElementStates.Selected) {
        Boolean vCellSelection = (grid.SelectionMode == DataGridViewSelectionMode.CellSelect) ||
                                  (grid.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect) ||
                                   (grid.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect);
        Rectangle rct = new Rectangle(a.CellBounds.Location, a.CellBounds.Size);
        rct.X = vCellSelection || (a.ColumnIndex == 0) ? rct.X + 1 : rct.X;
        rct.Y = rct.Y + 1;
        rct.Width = vCellSelection || (a.ColumnIndex == grid.Columns.Count - 1) ? rct.Width - 3 : rct.Width;
        rct.Height = rct.Height - 3;

        AnchorStyles vBorders = AnchorStyles.Bottom | AnchorStyles.Top;
        if (vCellSelection || (a.ColumnIndex == 0))
          vBorders = vBorders | AnchorStyles.Left;
        if (vCellSelection || (a.ColumnIndex == grid.Columns.Count - 1))
          vBorders = vBorders | AnchorStyles.Right;

        DrawAnSeldCell(rct, focused, a.Graphics, vBorders);

      }
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// Рисует рамку вокруг выбранных ячеек
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="a"></param>
    /// <param name="focused"></param>
    public static void DrawGridSelectionAlt(DataGridView grid, DataGridViewCellPaintingEventArgs a, Boolean focused) {
      DataGridViewPaintParts vPrts = DataGridViewPaintParts.All;
      vPrts &= ~DataGridViewPaintParts.SelectionBackground;
      vPrts &= ~DataGridViewPaintParts.Focus;
      a.Paint(a.CellBounds, vPrts);
      drawSeldCell(grid, a, focused);
      a.Handled = true;
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// Рисует рамку вокруг выбранного элемента дерева
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="a"></param>
    /// <param name="focused"></param>
    public static void DrawTreeSelectionAlt(TreeView tree, DrawTreeNodeEventArgs a, Boolean focused) {

      Rectangle nodeBounds = a.Node.Bounds;
      nodeBounds.Width += 10;
      a.Graphics.FillRectangle(Brushes.White, nodeBounds);
      DrawAnSeldCell(nodeBounds, focused, a.Graphics, AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom);

      Font nodeFont = a.Node.TreeView.Font;
      RectangleF nodeBoundsF = Rectangle.Inflate(nodeBounds, 0, 0);
      a.Graphics.DrawString(a.Node.Text, nodeFont, Brushes.Black, nodeBoundsF);
      a.DrawDefault = false;
    }
#endif

#if !SILVERLIGHT
    private static RegistryValueKind getRegistryType(Type type) {
      if (type != null) {
        if (type.Equals(typeof(Int16)) || type.Equals(typeof(Int32)) || type.Equals(typeof(Double)) || type.Equals(typeof(Decimal)))
          return RegistryValueKind.DWord;
        if (type.Equals(typeof(Int64)))
          return RegistryValueKind.QWord;
        if (type.Equals(typeof(String)))
          return RegistryValueKind.String;
      }
      return RegistryValueKind.Unknown;
    }
#endif


#if !SILVERLIGHT
    /// <summary>
    /// Записывает значение реестр
    /// </summary>
    /// <param name="pRegKeyName">Путь в реестре.</param>
    /// <param name="valName">Имя параметра.</param>
    /// <param name="value"></param>
    public static void RegistryCUSetValue(String regKeyName, String valName, Object value, Type type) {
      if(type == null)
        type = (value != null) ? value.GetType() : null;
      Object val = value;
      RegistryValueKind knd = getRegistryType(type);
      if (knd == RegistryValueKind.Unknown) {
        if (type != null) {
          TypeConverter tc = TypeDescriptor.GetConverter(type);
          if (tc != null && tc.CanConvertTo(typeof(System.String))) {
            val = tc.ConvertToString(val);
            knd = getRegistryType(typeof(System.String));
          } else
            return;
        } else
          return;
      }

      if (val == null)
        val = (knd == RegistryValueKind.String) ? (Object)String.Empty : 0;
      RegistryKey key = Registry.CurrentUser.CreateSubKey(regKeyName);
      key.SetValue(valName, val, knd);
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// Загружает параметр из реестра.
    /// </summary>
    /// <param name="regKeyName">Путь в реестре.</param>
    /// <param name="valName">Имя параметра.</param>
    /// <param name="defVal">Значение по умолчанию.</param>
    /// <returns>Значение параметра.</returns>
    public static Object RegistryCUGetValue(String regKeyName, String valName, object defVal) {
      RegistryKey key = Registry.CurrentUser.CreateSubKey(regKeyName);
      Object vResult = key.GetValue(valName, defVal) ?? defVal;
      return vResult;
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// Создает путь в реестре относительно каталога CU || LM || ...
    /// </summary>
    /// <param name="pProducerCompany"></param>
    /// <param name="pAppName"></param>
    /// <param name="pAppVersion"></param>
    /// <returns></returns>
    public static String GenerateRegistryKeyName(String pProducerCompany, String pAppName, String pAppVersion) {
      return "Software\\" + pProducerCompany + "\\" + pAppName + "\\" + pAppVersion;
    }
#endif

    /// <summary>
    /// Создает строку, которую можно подставить в URL
    /// </summary>
    /// <returns></returns>
    public static String buidQueryStrParams(Dictionary<String, String> prms) {
      String rslt = null;
      foreach (String k in prms.Keys) {
        String vParamStr = k + "=" + HttpUtility.UrlEncode(prms[k] as String);
        Utl.appendStr(ref rslt, vParamStr, "&");
      }
      return rslt;
    }

    /// <summary>
    /// Создает строку, которую можно подставить в URL
    /// </summary>
    /// <param name="baseURL"></param>
    /// <returns></returns>
    public static String buidQueryStrParams(String baseURL, Dictionary<String, String> prms) {
      String rslt = buidQueryStrParams(prms);
      return (baseURL.IndexOf('?') >= 0) ? baseURL + "&" + rslt : baseURL + "?" + rslt;
    }

    /// <summary>
    /// Возвращает true если параметер TRUE|T|1|Y иначе false
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Boolean parsBoolean(String val) {
      if (String.IsNullOrEmpty(val))
        return false;
      else {
        val = val.ToUpper();
        if (val.Equals("TRUE") || val.Equals("T") || val.Equals("1") || val.Equals("Y"))
          return true;
        else
          return false;
      }

    }

    /// <summary>
    /// Вытаскивает из ораклового сообщения об ошибке текст в случае если это ошибка создана через
    /// raise_application_error(2xxxx, 'message');, тогда вернет "message" иначе null.
    /// </summary>
    /// <param name="pMessage"></param>
    /// <returns></returns>
    public static void extractOracleApplicationError(String exMessage, out Int32 errCode, out String errMsg) {
      errCode = 0;
      errMsg = null;
      if (exMessage != null) {
        int vStrtIndx = exMessage.IndexOf("ORA-2");
        if (vStrtIndx >= 0) {
          int vEndIndx = exMessage.IndexOf("ORA-", vStrtIndx + 5);
          int vLen = vEndIndx - vStrtIndx;
          if (vLen < 0)
            vLen = exMessage.Length - vStrtIndx;
          String vMsg = exMessage;
          if (vLen > 0)
            vMsg = exMessage.Substring(vStrtIndx, vLen);
          errCode = Int32.Parse(vMsg.Substring(4, 5));
          errMsg = vMsg.Substring(11);
        }
      }
    }

    /// <summary>
    /// Достраивает путь к папке до абсолютного и нормализует
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static String resolvePath(String path) {
      return NormalizeDir(String.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : Path.GetFullPath(path));
    }

#if !SILVERLIGHT
    public static Boolean WriteMessageLog(String logFileName, String msg) {
      try {
        FileStream fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        Encoding vEncode = Encoding.Default;
        if (vEncode == null)
          vEncode = Encoding.Default;
        DateTime v_point = DateTime.Now;
        using (StreamWriter sw = new StreamWriter(fs, vEncode)) {
          String vLine = String.Format("[{0} {1}] - {2}", v_point.ToShortDateString(), v_point.ToLongTimeString(), msg);
          sw.WriteLine(vLine);
          sw.Flush();
          sw.Close();
        }
        fs.Close();
        return true;
      } catch (Exception) {
        return false;
      }
    }


    public static String buildErrorLogMsg(Exception ex, DateTime v_point) {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("^^-ERROR-BEGIN---------------------------------------------------------------------------^^");
      sb.AppendLine("Source        : " + ex.Source.ToString().Trim());
      sb.AppendLine("Method        : " + ex.TargetSite.Name.ToString());
      sb.AppendLine("Date          : " + v_point.ToShortDateString());
      sb.AppendLine("Time          : " + v_point.ToLongTimeString());
      sb.AppendLine("Computer      : " + Dns.GetHostName().ToString());
      sb.AppendLine("Error         : " + ex.Message.ToString().Trim());
      sb.AppendLine("Stack Trace   : " + ex.StackTrace.ToString().Trim());
      sb.AppendLine("^^-ERROR-END-----------------------------------------------------------------------------^^");
      return sb.ToString();
    }

    public static Boolean WriteErrorLog(String logFileName, Exception ex) {
      try {
        FileStream fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        Encoding vEncode = Encoding.Default;
        if (vEncode == null)
          vEncode = Encoding.Default;
        DateTime v_point = DateTime.Now;
        using (StreamWriter sw = new StreamWriter(fs, vEncode)) {
          String vLine = String.Format("[{0} {1}] - {2}", v_point.ToShortDateString(), v_point.ToLongTimeString(), "Ошибка!!!");
          sw.WriteLine(vLine);
          sw.WriteLine(buildErrorLogMsg(ex, v_point));
          sw.Flush();
          sw.Close();
        }
        fs.Close();
        return true;
      } catch (Exception) {
        return false;
      }
    }

    public static String getLocalHost(){
      return Dns.GetHostName();
    }
    public static String getLocalIP() {
      String localHost = Dns.GetHostName();
      IPAddress[] addrss = Dns.GetHostAddresses(Dns.GetHostName());
      String localeIP = null;
      foreach (IPAddress a in addrss) {
        if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
          localeIP = a.ToString();
          break;
        }
      }
      return localeIP;
    }
    public static void initDirectory(String path) {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
    }

    public static CommandType detectCommandType(String sqlText) {
      Boolean hasSelect = Utl.regexMatch(sqlText, @"\bSELECT\b", true);
      hasSelect = hasSelect && Utl.regexMatch(sqlText, @"\bFROM\b", true);
      if (hasSelect)
        return CommandType.Text;
      else
        return CommandType.StoredProcedure;
    }

#endif

#if SILVERLIGHT
    public static void loadRemoteAssembly(String xapName, Action<OpenReadCompletedEventArgs, Assembly> onLoadAction) {
      if (!String.IsNullOrEmpty(xapName)) {
        WebClient wc = new WebClient();
        wc.OpenReadCompleted += new OpenReadCompletedEventHandler((sndr, atrs) => {
          String appManifest = new StreamReader(Application.GetResourceStream(new StreamResourceInfo(atrs.Result, null),
                               new Uri("AppManifest.xaml", UriKind.Relative)).Stream).ReadToEnd();


          XElement deploymentRoot = XDocument.Parse(appManifest).Root;
          List<XElement> deploymentParts = (from assemblyParts in deploymentRoot.Elements().Elements()
                                            select assemblyParts).ToList();
          foreach (XElement xElement in deploymentParts.Reverse<XElement>()) {
            String source = xElement.Attribute("Source").Value;
            StreamResourceInfo streamInfo = Application.GetResourceStream(new StreamResourceInfo(atrs.Result,
                                              "application/binary"), new Uri(source, UriKind.Relative));

            AssemblyPart asmPart = new AssemblyPart();
            Assembly asm = asmPart.Load(streamInfo.Stream);
            if (onLoadAction != null)
              onLoadAction(atrs, asm);
          }
        });
        wc.OpenReadAsync(new Uri(xapName, UriKind.Relative));
      }
    }

    public static Boolean DesignTime {
      get {
        return DesignerProperties.IsInDesignTool;
      }
    }

    private static String csCookieValTypeSplitter = "|value-type|";
    public static void SetCookie(String key, Object value, Int32 expireDays, Boolean silent) {
      DateTime expireDate = DateTime.Now + TimeSpan.FromDays(expireDays);
      Type newCookieValueType = value != null ? value.GetType() : typeof(String);
      String newCookieValue = value + csCookieValTypeSplitter + newCookieValueType.FullName;
      String newCookie = key.Trim() + "=" + newCookieValue + ";expires=" + expireDate.ToString("R");
      try {
        HtmlPage.Document.SetProperty("cookie", newCookie);
      } catch (Exception ex) {
        if (!silent)
          throw ex;
      }
    }

    public static Object GetCookie(String key, Object defaultValue, Boolean silent) {
      String[] cookies = null;
      try {
        cookies = HtmlPage.Document.Cookies.Split(';');
      } catch (Exception ex) {
        if (!silent)
          throw ex;
      }
      if (cookies != null) {
        foreach (String cookie in cookies) {
          String[] keyValue = cookie.Trim().Split('=');
          if (keyValue.Length == 2) {
            if (String.Equals(keyValue[0], key)) {
              String valFullStr = keyValue[1];
              String[] valueParts = Utl.SplitString(valFullStr, csCookieValTypeSplitter);
              if (valueParts.Length == 2) {
                String valStr = valueParts[0];
                String valTypeName = valueParts[1];
                Type valType = Type.GetType(valTypeName);
                Object valObj = Utl.Convert2Type(valStr, valType);
                return valObj;
              } else
                return null;
            }
          }
        }
      }
      return defaultValue;

    }

    public static Boolean IsUiThread {
      get {
        return Deployment.Current.Dispatcher.CheckAccess();
      }
    }

    public static void UiThreadInvoke(Action a) {
      Deployment.Current.Dispatcher.BeginInvoke(a);
    }
    public static void UiThreadInvoke(Delegate d, params Object[] args) {
      Deployment.Current.Dispatcher.BeginInvoke(d, args);
    }


#endif

    public static Object GetPropertyValue(Object obj, String propertyName) {
      return GetPropertyValue<Object>(obj, propertyName);
    }
    public static T GetPropertyValue<T>(Object obj, String propertyName) {
      if (obj != null) {
        PropertyInfo propertyRnum = GetPropertyInfo(obj.GetType(), propertyName, false);
        if (propertyRnum != null) {
          var v_val = propertyRnum.GetValue(obj, null);
          return Convert2Type<T>(v_val);
        } else
          return default(T);
      } else
        return default(T);
    }
    public static void SetPropertyValue(Object obj, String propertyName, Object value) {
      if (obj != null) {
        PropertyInfo propertyRnum = GetPropertyInfo(obj.GetType(), propertyName, false);
        if (propertyRnum != null)
          propertyRnum.SetValue(obj, value, null);
      }
    }

    public static TimeSpan Duration(DateTime from) {
      if (from < (DateTime.Now.AddDays(-7)))
        return TimeSpan.MinValue;
      else
        return DateTime.Now.Subtract(from);
    }
    public static String FormatDuration(TimeSpan duration) {
      return String.Format("{0:00}:{1:00}:{2:00}", duration.Hours + (duration.Days * 24), duration.Minutes, duration.Seconds);
    }

    /// <summary>
    /// Возвращает номер дня недели, где 1-пон...7-вос
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Int32 DayOfWeekRu(DateTime date) {
      String[] v_days = new String[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
      String v_day_str = date.ToString("ddd", new CultureInfo("ru-RU"));
      int v_day_int = Array.IndexOf<String>(v_days, v_day_str);
      return v_day_int + 1;
    }

#if SILVERLIGHT
    public static void StoreUserObjectStrg(String objName, Object obj) {
      if (!String.IsNullOrEmpty(objName)) {
        try {
          var userSettings = IsolatedStorageSettings.ApplicationSettings;
          if (userSettings.Contains(objName))
            userSettings.Remove(objName);
          if (obj != null)
            userSettings.Add(objName, obj);
          userSettings.Save();
          userSettings = null;
        } catch (IsolatedStorageException ex) {
          // Isolated storage not enabled or an error occurred
          throw new EBioException(String.Format("Ошибка при сохранении объекта {0} в IsolatedStorageSettings.ApplicationSettings, значение: {1}\n Сообщение: {2}", objName, "" + obj, ex.Message), ex);
        }
      }
    }

    public static void StoreUserObjectCookie0(String objName, Object obj) {
      if ((obj != null) && !String.IsNullOrEmpty(objName)) {
        var jsonObj = jsonUtl.encode(obj, null);
        browserUtl.SetCookie(objName, jsonObj);
      }
    }

    public static T RestoreUserObjectStrg<T>(String objName, T defObj) {
      T rslt;
      try {
        var userSettings = IsolatedStorageSettings.ApplicationSettings;
        if (userSettings.TryGetValue<T>(objName, out rslt))
          return rslt;
        userSettings = null;
      } catch (IsolatedStorageException ex) {
        //return defObj;
        throw new EBioException(String.Format("Ошибка при восстановлении объекта {0} из IsolatedStorageSettings.ApplicationSettings.", objName), ex);
      }
      return defObj;
    }

    public static T RestoreUserObjectCookie0<T>(String objName, T defObj) {
      var jsonObj = browserUtl.GetCookie(objName);
      if (!String.IsNullOrEmpty(jsonObj)) {
        try {
          return jsonUtl.decode<T>(jsonObj);
        } catch (Newtonsoft.Json.JsonSerializationException) {
          return defObj;
        }
      } else
        return defObj;
    }

    //public static void SetDebug(Boolean debug) {
    //  Utl.StoreUserObject("debug", debug);
    //}
    //public static Boolean GetDebug() {
    //  return Utl.RestoreUserObject<Boolean>("debug", false);
    //}

    //public static void SetCurUsrIsDebugger(Boolean debug) {
    //  Utl.StoreUserObject("cur_usr_is_debugger", debug);
    //}
    //public static Boolean CurUsrIsDebugger() {
    //  return Utl.RestoreUserObject<Boolean>("cur_usr_is_debugger", false);
    //}
    //public static void SetCurSessionIsLoggedOn(Boolean debug) {
    //  Utl.StoreUserObject("cur_sess_is_loggedon", debug);
    //}
    //public static Boolean CurSessionIsLoggedOn() {
    //  return Utl.RestoreUserObject<Boolean>("cur_sess_is_loggedon", false);
    //}

    public static String GetAssemblyVersion(Assembly assembly) {
      AssemblyName assemblyName = new AssemblyName(assembly.FullName);
      return assemblyName.Version.ToString();
    }

    public static void SetCurrentClientVersion(String version) {
      Utl.StoreUserObjectStrg("BioMainClientAssemblyVersion", version);
    }
    public static String GetCurrentClientVersion() {
      return Utl.RestoreUserObjectStrg<String>("BioMainClientAssemblyVersion", "0");
    }
#endif

    public static String PopFirstItemFromList(ref String path, String delimeter) {
      if (!String.IsNullOrEmpty(path)) {
        String[] v_nodes = Utl.SplitString(path, delimeter);
        if (v_nodes.Length > 0) {
          var v_nodes_new = v_nodes.Where((v, i) => i > 0).ToArray();
          path = Utl.CombineString(v_nodes_new, delimeter);
        } else
          path = null;
        return (v_nodes.Length > 0) ? v_nodes[0] : null;
      } else
        return null;
    }

    public static String PopLastItemFromList(ref String path, String delimeter) {
      if (!String.IsNullOrEmpty(path)) {
        String[] v_nodes = Utl.SplitString(path, delimeter);
        if (v_nodes.Length > 0) {
          var v_nodes_new = v_nodes.Where((v, i) => i < v_nodes.Length - 1).ToArray();
          path = Utl.CombineString(v_nodes_new, delimeter);
        } else
          path = null;
        return (v_nodes.Length > 0) ? v_nodes[v_nodes.Length - 1] : null;
      } else
        return null;
    }

    private static int CHUNKSIZE = 40000;
    public static void LoadFileToBuffer(Stream stream, out Byte[] buffer) {
      buffer = new Byte[stream.Length];
      int chunkSize;
      int chunkPos = 0;
      while (stream.Position > -1 && stream.Position < stream.Length) {
        if (stream.Length - stream.Position >= CHUNKSIZE)
          chunkSize = CHUNKSIZE;
        else
          chunkSize = (int)(stream.Length - stream.Position);
        //byte[] fileBytes = new byte[chunkSize];
        int byteCount = stream.Read(buffer, chunkPos, chunkSize);
        chunkPos += chunkSize;
      }

    }

    public static T[] combineArrays<T>(T[] array1, T[] array2) {
      int array1OriginalLength = array1.Length;
      Array.Resize<T>(ref array1, array1OriginalLength + array2.Length);
      Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
      return array1;
    }

#if SILVERLIGHT
    public static T FindParentElem<T>(FrameworkElement elem) where T : FrameworkElement {
      if (elem != null) {
        if (elem.GetType().IsSubclassOf(typeof(T)) || (elem is T))
          return (T)elem;
        else
          return FindParentElem<T>(elem.Parent as FrameworkElement);
      } else
        return default(T);
    }
    public static FrameworkElement FindParentElem1<T>(FrameworkElement elem) {
      if (elem != null) {
        if (elem.GetType().IsSubclassOf(typeof(T)) || (elem is T))
          return elem;
        else
          return FindParentElem1<T>(elem.Parent as FrameworkElement);
      } else
        return null;
    }
#endif

  }
}