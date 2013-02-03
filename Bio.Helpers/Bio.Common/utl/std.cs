namespace Bio.Helpers.Common {
  using System;
  using System.Text.RegularExpressions;
  using System.IO;
#if !SILVERLIGHT
  using System.Web;
  using System.Windows.Forms;
  using System.Drawing;
  using System.Data;
  using Microsoft.Win32;
  using System.Threading;
  using System.Xml;
#else
  using System.IO.IsolatedStorage;
  using System.Windows;
  using System.Windows.Browser;
  using System.Xml.Linq;
  using System.Windows.Resources;
#endif
  using System.Collections.Generic;
  using System.Globalization;
  using System.ComponentModel;
  using System.Reflection;
  using System.Linq;

  using Types;
  using System.Net;
  using System.Text;

  /// <summary>
  /// ������� ������ ����������
  /// </summary>
  public class Utl {

    /// <summary>
    /// ��������� - ��� ���������
    /// </summary>
    public const String EncIso88591 = "ISO-8859-1";
    /// <summary>
    /// ��������� - ��� ���������
    /// </summary>
    public const String EncUtf8 = "UTF-8";
    /// <summary>
    /// ��������� - ��� ���������
    /// </summary>
    public const String EncCp866 = "Cp866";
    /// <summary>
    /// ��������� - ��� ���������
    /// </summary>
    public const String EncIso88595 = "ISO-8859-5";
    /// <summary>
    /// ��������� - ��� ���������
    /// </summary>
    public const String EncWindows1251 = "WINDOWS-1251";
    /// <summary>
    /// 
    /// </summary>
    public const String SYS_ENCODING = EncUtf8;

#if SILVERLIGHT
    public static Encoding DefaultEncoding = Encoding.UTF8;
#else
    /// <summary>
    /// ��������� �� ���������
    /// </summary>
    public static Encoding DefaultEncoding = Encoding.Default;
#endif

    /// <summary>
    /// ��������� - ��� ��������� ������� ��� ������� ����� � �������
    /// </summary>
    public const String QLOGIN_PARNAME = "QLOGIN";
    /// <summary>
    /// ��������� - ��� ��������� ������� ��� ����� � ������� �� ���-����
    /// </summary>
    public const String HASHLOGIN_PARNAME = "HLGN";
    /// <summary>
    /// ��������� - ��� ��������� ������� ��� ������� ����� � �������
    /// </summary>
    public const String FLOGIN_PARNAME = "FLOGIN";
    /// <summary>
    /// ��������� - ��� ��������� ������� cliname
    /// </summary>
    public const String CLINAME_PARNAME = "cliname";
    /// <summary>
    /// ��������� - �������� ��������� ������� cliname
    /// </summary>
    public const String CLINAME_PARVALUE = "dalpha";
    /// <summary>
    /// ��������� - ������ ��� ���������� URL
    /// </summary>
    public const String SrvURLTemplate = "{0}/srv.aspx";

#if !SILVERLIGHT
    /// <summary>
    /// ����������� �� URL ������ ������� ��������� �� �����
    /// </summary>
    /// <param name="queryUri"></param>
    /// <param name="prm"></param>
    /// <returns></returns>
    public static String GetQueryParam(String queryUri, String prm) {
      if (queryUri != null) {
        var v_s = HttpUtility.UrlDecode(queryUri.Substring(queryUri.IndexOf("?", StringComparison.Ordinal) + 1));
        var v_spr = new[] { '&' };
        if (v_s != null) {
          var v_pars = v_s.Split(v_spr);
          foreach (var t in v_pars) {
            var v_name = prm + "=";
            if (t.StartsWith(v_name)) {
              var v_rslt = t.Substring(v_name.Length);
              if (v_rslt.Trim().Equals(""))
                return null;
              return v_rslt;
            }
          }
        }
      }
      return null;
    }
#endif

    /// <summary>
    /// �������� ANSII -> UTF
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static String EncodeANSII2UTF(String msg) {
#if !SILVERLIGHT
      if (msg != null) {
        String v_result;
        var v_enc = new UTF8Encoding();
        var v_bfr = v_enc.GetBytes(msg);
        v_bfr = Encoding.Convert(Encoding.GetEncoding(EncWindows1251), Encoding.GetEncoding(SYS_ENCODING), v_bfr);
        var v_tmps = v_enc.GetString(v_bfr);
        v_result = v_tmps;
        return v_result;
      }
      return null;
#else
      return msg;
#endif
    }

    /// <summary>
    /// ��������� ������� "(value == null) || (value == DBNull.Value)"
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Boolean IsNull(Object value) {
      return (value == null) || (value == DBNull.Value);
    }

    /// <summary>
    /// ���� Null, �� ���������� ������ ������
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static String NullToBlank(String value) {
      return value ?? String.Empty;
    }

    /// <summary>
    /// ���� �� NullOrEmpty, �� ���������� ������ "NULL" ����� ���������� �������� value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static String NullToNULL(String value) {
      return String.IsNullOrEmpty(value) ? "NULL" : value;
    }

    /// <summary>
    /// ���� �� ����� ������ ������ ��� null, �� ���������� ���� (���� ��� �������� � double)
    /// </summary>
    /// <param name="str"></param>
    /// <returns>0 ��� ������� ������</returns>
    public static String BlankTo0(String str) {
      return String.IsNullOrEmpty(str) ? "0" : str;
    }

    /// <summary>
    /// ����������� ������ � double
    /// </summary>
    /// <param name="prm"></param>
    /// <returns></returns>
    public static Double ToDbl(String prm) {
      var v_provider = new NumberFormatInfo {NumberDecimalSeparator = "."};
      return Double.Parse(BlankTo0(prm.Replace(",", ".")), v_provider);
    }

    /// <summary>
    /// ���� true, ����� ���������� "Y" ����� "N"
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static String BoolToStr(Boolean value) {
      return (value) ? "Y" : "N";
    }

#if !SILVERLIGHT
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vcook"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static String GetCookieByName(HttpCookieCollection vcook, String name) {
      foreach (String key in vcook.Keys) {
        if (String.Equals(key, name)) {
          var v_httpCookie = vcook.Get(key);
          if (v_httpCookie != null) return v_httpCookie.Value;
        }
      }
      return "null";
    }
#endif

    /// <summary>
    /// ���������� source ?? target
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    static public String Nvl(String source, String target) {
      return source ?? target;
    }

    /// <summary>
    /// ���������� source ?? target
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    static public Int32 Nvl(Int32? source, Int32 target) {
      return source ?? target;
    }

#if !SILVERLIGHT
    /// <summary>
    /// ������ ��������� ��������������
    /// </summary>
    /// <param name="vAdmName"></param>
    /// <param name="vAdmCntcts"></param>
    /// <returns></returns>
    static public String GetMsg_ToAdminPlease(String vAdmName, XmlNodeList vAdmCntcts) {
      var v_rslt = "���������� � �������������� �������.<br>";
      v_rslt += "���������� ����������:<br>";
      for (var i = 0; i < vAdmCntcts.Count; i++) {
        var v_cntct = (XmlElement)vAdmCntcts[i];
        var v_cc = "";
        if (v_cntct.GetAttribute("type").Equals("office"))
          v_cc = "���. �";
        else if (v_cntct.GetAttribute("type").Equals("phone"))
          v_cc = "�������";
        else if (v_cntct.GetAttribute("type").Equals("mail"))
          v_cc = "��. �����";
        v_rslt += v_cc + " : " + v_cntct.InnerText + "<br>";
      }
      v_rslt += "��� : " + vAdmName + "<br>";
      return v_rslt;
    }
#endif

    /// <summary>
    /// ����������� appURL ������ "SYS_APP_URL"
    /// </summary>
    /// <param name="appURL"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static String NormalizeURL(String appURL, String url) {
      return url != null ? url.Replace("SYS_APP_URL", appURL) : null;
    }

    /// <summary>
    /// ��������� ������ str �� ��������� String[] � ������������� delimiter[]
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, Char[] delimeter) {
      return str == null ? new String[0] : str.Split(delimeter);
    }

    /// <summary>
    /// ��������� ������ str �� ��������� String[] � ������������ delimiter
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, Char delimiter) {
      return SplitString(str, new[] { delimiter });
    }


    /// <summary>
    /// ��������� ������ str �� ��������� String[] � ������������� delimiter[]
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, String[] delimeter) {
      if (!String.IsNullOrEmpty(str)) {
        var v_line = str;
        String v_dlmtr;
        if (delimeter.Length > 1) {
          const String csDlmtrPG = "#inner_pg_delimeter_str#";
          foreach (var d in delimeter)
            v_line = v_line.Replace(d, csDlmtrPG);
          v_dlmtr = csDlmtrPG;
        } else
          v_dlmtr = delimeter.FirstOrDefault();
        if (v_dlmtr != null) {
          IList<String> v_list = new List<String>();
          var v_itemBgn = 0;
          while (v_itemBgn <= v_line.Length) {
            var v_dlmtrPos = v_line.IndexOf(v_dlmtr, v_itemBgn, StringComparison.Ordinal);
            if (v_dlmtrPos == -1)
              v_dlmtrPos = v_line.Length;
            var v_line2Add = v_line.Substring(v_itemBgn, v_dlmtrPos - v_itemBgn);
            v_list.Add(v_line2Add);
            v_itemBgn += v_line2Add.Length + v_dlmtr.Length;
          }
          return v_list.ToArray();
        }
        return new String[0];
      }
      return new String[] {};
    }

    /// <summary>
    /// ��������� ������ str �� ��������� String[] � ������������ delimiter
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, String delimiter) {
      return SplitString(str, new[] { delimiter });
    }

    /// <summary>
    /// ���������� ������ lines[] � ���� ������ ����� ����������� delimiter
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static String CombineString(String[] lines, String delimiter) {
      String v_result = null;
      foreach (var v_line in lines)
        AppendStr(ref v_result, v_line, delimiter);
      return v_result;
    }

    /// <summary>
    /// �������� "&" �� "&amp;"
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static String UrlEncode(String line) {
      return line.Replace("&", "&amp;");
    }

    /// <summary>
    /// �������� "&amp;" �� "&"
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static String UrlDecode(String line) {
      return line.Replace("&amp;", "&");
    }

    /// <summary>
    /// ���������� x - (x / y) * y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Mod(int x, int y) {
      return x - (x / y) * y;
    }

    private static readonly Mutex FMutexOfLogFile = new Mutex();
    /// <summary>
    /// ��������� ������ line � ���� fileName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="line"></param>
    /// <param name="encoding">��������� ��� ������</param>
    /// <param name="createPath">������� ���� ���� �� ����������</param>
    public static void AppendStringToFile(String fileName, String line, Encoding encoding, Boolean createPath) {
      FMutexOfLogFile.WaitOne();
      try {
        if (!Directory.Exists(Path.GetDirectoryName(fileName)) && createPath)
          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        var v_fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        var v_encoding = encoding ?? DefaultEncoding;
        using (var sw = new StreamWriter(v_fs, v_encoding)) {
          sw.WriteLine(line);
          sw.Flush();
          sw.Close();
        }
      } finally {
        FMutexOfLogFile.ReleaseMutex();
      }
    }

    /// <summary>
    /// ��������� ������ line � ���� fileName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="line"></param>
    /// <param name="encoding"></param>
    public static void AppendStringToFile(String fileName, String line, Encoding encoding) {
      AppendStringToFile(fileName, line, encoding, false);
    }

    /// <summary>
    /// ��������� ������ line � ���� fileName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="line"></param>
    /// <param name="encoding"></param>
    public static void SaveStringToFile(String fileName, String line, Encoding encoding) {
      if (File.Exists(fileName))
        File.Delete(fileName);
      AppendStringToFile(fileName, line, encoding);
    }

    /// <summary>
    /// ��������� obj.ToString() � ������  ����� �����������
    /// </summary>
    /// <param name="line"></param>
    /// <param name="delimiter"></param>
    /// <param name="obj"></param>
    public static void AddObjToLine(ref String line, String delimiter, Object obj) {
      if (obj != null) 
        AppendStr(ref line, obj.ToString(), delimiter);
      else
        AppendStr(ref line, String.Empty, delimiter);
    }

    private const int CHUNKSIZE = 40000;
    /// <summary>
    /// ������ ����� � ������ ������
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    public static void ReadBinStreamInBuffer(Stream stream, out Byte[] buffer) {
      buffer = new Byte[stream.Length];
      var v_chunkPos = 0;
      while (stream.Position > -1 && stream.Position < stream.Length) {
        int v_chunkSize;
        if (stream.Length - stream.Position >= CHUNKSIZE)
          v_chunkSize = CHUNKSIZE;
        else
          v_chunkSize = (int)(stream.Length - stream.Position);
        stream.Read(buffer, v_chunkPos, v_chunkSize);
        v_chunkPos += v_chunkSize;
      }

    }

    /// <summary>
    /// ��������� ���� � ������ ������
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="fileName"></param>
    public static void ReadBinFileInBuffer(String fileName, ref Byte[] buffer) {
      if (File.Exists(fileName)) {
        var v_stream = new FileStream(fileName, FileMode.Open);
        ReadBinStreamInBuffer(v_stream, out buffer);
      }
    }

    /// <summary>
    /// ����� ������ ������ � ����
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="buffer"></param>
    public static void WriteBuffer2BinFile(String fileName, Byte[] buffer) {
      if (File.Exists(fileName))
        File.Delete(fileName);
      var v_fileStream = new FileStream(fileName, FileMode.CreateNew);
      try {
        var v_binaryWriter = new BinaryWriter(v_fileStream);
        v_binaryWriter.Write(buffer, 0, buffer.Length);
      } finally {
        v_fileStream.Close();
      }
    }


    /// <summary>
    /// ����������� ������ ���� ###0(S|M|H|D) � ���-�� ����������, ��� S-���, M-���, H-���, D-����
    /// </summary>
    /// <param name="period"></param>
    /// <returns></returns>
    public static long StrTimePeriodToMilliseconds(String period) {
      Int64 v_rslt = 0;
      if (period != null) {
        var v_mult = 1;
        var v_period = period.ToUpper();
        var v_periodType = v_period[v_period.Length - 1];
        if (v_periodType == 'S') {
          v_mult = 1000;
          v_period = v_period.Substring(0, v_period.Length - 1);
        } else if (v_periodType == 'M') {
          v_mult = 1000 * 60;
          v_period = v_period.Substring(0, v_period.Length - 1);
        } else if (v_periodType == 'H') {
          v_mult = 1000 * 3600;
          v_period = v_period.Substring(0, v_period.Length - 1);
        } else if (v_periodType == 'D') {
          v_mult = 1000 * 3600 * 24;
          v_period = v_period.Substring(0, v_period.Length - 1);
        }
        v_rslt = Int64.Parse(v_period) * v_mult;
      }
      return v_rslt;
    }

    /// <summary>
    /// �������� ������ ����� �� ��������� ���-�� ��������� 
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="cnt"></param>
    public static void ShiftArrayLeft(ref String[] arr, int cnt) {
      var v_list1 = new String[arr.Length - 1];
      for (var i = cnt; i < arr.Length; i++)
        v_list1[i - cnt] = arr[i];
      arr = v_list1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="line"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static String CutElementFromDelimitedLine(ref String line, String delimiter) {
      String v_result = null;
      var v_list = SplitString(line, delimiter);
      if (v_list.Length > 0) {
        v_result = v_list[0];
        if (v_list.Length > 1) {
          ShiftArrayLeft(ref v_list, 1);
          line = CombineString(v_list, delimiter);
        } else {
          line = null;
        }
      }
      return v_result;
    }

    /// <summary>
    /// ��������� �������� �� ������ line ������� elem
    /// </summary>
    /// <param name="line"></param>
    /// <param name="elem"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static bool IsElementInDelimitedLine(String line, String elem, Char delimiter) {
      var v_rslt = false;
      var v_list = SplitString(line, delimiter);
      foreach (var t in v_list) {
        if (t.Equals(elem)) {
          v_rslt = true;
          break;
        }
      }
      return v_rslt;
    }

    /// <summary>
    /// ��������� �������� �� ������ line ������� elem
    /// </summary>
    /// <param name="line"></param>
    /// <param name="elem"></param>
    /// <param name="delimeters"></param>
    /// <returns></returns>
    public static bool IsElementInDelimitedLine(String line, String elem, Char[] delimeters) {
      var v_lst = SplitString(line, delimeters);
      foreach (var t in v_lst)
        if (t.Equals(elem))
          return true;
      return false;
    }

    /// <summary>
    /// ��������� ����� �� ��� ������ ����� ��������
    /// </summary>
    /// <param name="line1"></param>
    /// <param name="line2"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static Boolean DelimitedLineHasCommonTags(String line1, String line2, Char[] delimeter) {
      var v_lst = SplitString(line1, delimeter);
      foreach (var t in v_lst)
        if (IsElementInDelimitedLine(line2, t, delimeter))
          return true;
      return false;
    }

    /// <summary>
    /// ��������� ���� �� ���� ���� ���� �� userRoles � objRoles
    /// </summary>
    /// <param name="objRoles"></param>
    /// <param name="userRoles"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static Boolean CheckRoles(String objRoles, String userRoles, Char[] delimeter) {
      var v_objectRoles = objRoles;
      if ((v_objectRoles == null) || (v_objectRoles.Equals("")))
        v_objectRoles = "*";

      // ��������� ������� * � pObjectRoles
      Boolean v_result = IsElementInDelimitedLine(v_objectRoles, "*", delimeter);

      if (!v_result) {
        //* ��� � pObjectRoles ��������� �����������
        v_result = DelimitedLineHasCommonTags(v_objectRoles, userRoles, delimeter);
      }

      if (v_result) {
        //��������� ������� ����������� �����
        var v_lst = userRoles.Split(delimeter);
        foreach (var t in v_lst)
          if (IsElementInDelimitedLine(v_objectRoles, "!" + t, delimeter))
            v_result = false;
      }

      return v_result;
    }

    /// <summary>
    /// ������� ����� ���� �� �� ����������
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static String ForceDirectory(String path) {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      return path;
    }

    /// <summary>
    /// ��������� value == "true", �� ������������ � ��������
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Boolean CheckBoolValue(String value) {
      return String.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// ����������� ������ ����� � ������ ����� ����������� �������
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static String ArrayToHTML(String[] array) {
      var v_res = "";
      foreach (var t in array) {
        if (v_res.Equals(""))
          v_res += "\"" + t + "\"";
        else
          v_res += ",\"" + t + "\"";
      }
      return v_res;
    }

    /// <summary>
    /// ����������� ���� � ����� - ��������� � ����� "\" ���� �����
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static String NormalizeDir(String dir) {
      if (!String.IsNullOrEmpty(dir)) {
        var v_result = dir.Trim();
        if (v_result.Substring(v_result.Length - 1, 1) != "\\")
          v_result = v_result + "\\";
        return v_result;
      }
      return dir;
    }

    /// <summary>
    /// ������ ������ ���� �� ��������������
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rootPath"></param>
    /// <returns></returns>
    public static String FullPath(String path, String rootPath) {
      var v_result = path.Trim();
      var v_vr = new Regex("^\\D[:].*");
      if (!v_vr.IsMatch(v_result)) {
        if (!v_result.Substring(0, 3).Equals("..\\")) {
          if (!v_result.Substring(0, 1).Equals("\\"))
            v_result = NormalizeDir(rootPath) + v_result;
          else
            v_result = NormalizeDir(rootPath) + v_result.Substring(1);
        } else
          v_result = NormalizeDir(Path.GetFullPath(rootPath));
      }
      v_result = NormalizeDir(v_result);
      return v_result;
    }

    /// <summary>
    /// ����������� ���� � ��������������� ������� �� ����
    /// </summary>
    /// <param name="bioCode"></param>
    /// <returns></returns>
    public static String GenBioLocalPath(String bioCode) {
      var v_fLstIndx = bioCode.LastIndexOf(".", StringComparison.Ordinal);
      return v_fLstIndx >= 0 ? bioCode.Substring(0, v_fLstIndx + 1).Replace(".", "\\") : null;
    }

    /// <summary>
    /// ���������� ��� ������
    /// </summary>
    /// <param name="verLeft"></param>
    /// <param name="verRight"></param>
    /// <returns>[-1]-������; [0]-�����; [1]-������</returns>
    public static int CompareVer(String verLeft, String verRight) {
      var v_verLeft = SplitString(verLeft, '.');
      var v_verRight = SplitString(verRight, '.');
      var v_upIndex = Math.Max(v_verLeft.Length, v_verRight.Length);
      for (var i = 0; i < v_upIndex; i++) {
        var v_intLeft = (i < v_verLeft.Length) ? Int32.Parse(v_verLeft[i]) : 0;
        var v_intRight = (i < v_verRight.Length) ? Int32.Parse(v_verRight[i]) : 0;
        if (v_intLeft < v_intRight) return -1;
        if (v_intLeft > v_intRight) return 1;
      }
      return 0;
    }


    /// <summary>
    /// ������� ���������� �����
    /// </summary>
    /// <param name="path"></param>
    public static void ClearDir(String path) {
      try {
        if (Directory.Exists(path))
          Directory.Delete(path, true);
        Directory.CreateDirectory(path);
      } catch { }
    }

    /// <summary>
    /// ��������� ����� ���� [��� ������������]/[������]
    /// </summary>
    /// <param name="login"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    public static void ParsLogin(String login, ref String user, ref String password) {
      user = null;
      password = null;
      var v_lst = Utl.SplitString(login, '/');
      if (v_lst.Length > 0)
        user = v_lst[0];
      if (v_lst.Length > 1)
        password = v_lst[1];
    }

    /// <summary>
    /// ��������� ���������� ��������� ��� ������. ���������� Match.Success.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static Boolean RegexMatch(String line, String regex, Boolean ignoreCase) {
      var v_regex = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      var v_match = v_regex.Match(line);
      return v_match.Success;
    }

    /// <summary>
    /// ��������� ���������� ��������� ��� ������. ���������� Match.Value.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static String RegexFind(String line, String regex, Boolean ignoreCase) {
      var v_regex = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      var v_match = v_regex.Match(line);
      return v_match.Success ? v_match.Value : null;
    }

    /// <summary>
    /// ���� ������� ��������� ������ � ������� ����������� ���������
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static Int32 RegexPos(String line, String regex, Boolean ignoreCase) {
      var v_regex = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      var v_match = v_regex.Match(line);
      return v_match.Success ? v_match.Index : -1;
    }

    /// <summary>
    /// �������� ��������� � ������� ����������� ���������
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="rplcmnt"></param>
    /// <param name="ignoreCase"></param>
    public static void RegexReplace(ref String line, String regex, String rplcmnt, Boolean ignoreCase) {
      var v_regex = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      line = v_regex.Replace(line, rplcmnt);
    }

    /// <summary>
    /// ��������� Connection String
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    public static IDictionary<String, String> ParsConnectionStr(String connStr) {
      IDictionary<String, String> v_rslt = new Dictionary<String, String>();
      if (connStr != null) {
        var v_spr = new Char[] { ';' };
        var v_lst = connStr.Split(v_spr);
        foreach (var fpar in v_lst) {
          v_spr[0] = '=';
          var v_ppp = fpar.Split(v_spr);
          String v_name = null;
          String v_value = null;
          if (v_ppp.Length > 0)
            v_name = v_ppp[0];
          if (v_ppp.Length > 1)
            v_value = v_ppp[1];
          v_rslt.Add(v_name, v_value);
        }
      }
      return v_rslt;
    }

    /// <summary>
    /// ������ Connection String
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    public static String BuildConnectionStr(IDictionary<String, String> connStr) {
      String v_rslt = null;
      if (connStr != null) {
        foreach (var v_item in connStr) {
          Utl.AppendStr(ref v_rslt, v_item.Key + "=" + v_item.Value, ";");
        }
      }
      return v_rslt;
    }

    /// <summary>
    /// ����������� ��� ������� �� ������ ���� "functionName(param, ...)"
    /// </summary>
    /// <param name="eveName"></param>
    /// <returns></returns>
    public static String GetFncName(String eveName) {
      if (eveName != null) {
        var v_fncNamEnd = eveName.IndexOf('(');
        if (v_fncNamEnd >= 0)
          return eveName.Substring(0, eveName.Length - (eveName.Length - v_fncNamEnd)).Trim();
      }
      return null;
    }

    /// <summary>
    /// ��������� ������ str � ����� ������ line ����� ����������� delimiter
    /// </summary>
    /// <param name="line"></param>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <param name="ignoreNull">���� str - null or empty, �� ������ �� �����������</param>
    public static void AppendStr(ref String line, String str, String delimiter, Boolean ignoreNull) {
      if (String.IsNullOrEmpty(str) && ignoreNull)
        return;
      if (String.IsNullOrEmpty(line))
        line = str;
      else
        line += delimiter + str;
    }

    /// <summary>
    /// ��������� ������ str � ����� ������ line ����� ����������� delimiter
    /// </summary>
    /// <param name="line"></param>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    public static void AppendStr(ref String line, String str, String delimiter) {
      AppendStr(ref line, str, delimiter, false);
    }

    /// <summary>
    /// ��������� ������ - URL ��� ������� � ������� Bio
    /// </summary>
    /// <param name="serverHost"></param>
    /// <param name="requestType"></param>
    /// <returns></returns>
    public static String BuildURL(String serverHost, String requestType) {
      var v_url = String.Format(SrvURLTemplate, serverHost);
      if (requestType != null)
        v_url = v_url + "?mtp=" + requestType;
      return v_url;
    }

    /// <summary>
    /// ����������� �� ������ ���� "username/password" ��� ������������
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public static String ExtractUsrNameFromLogin(String login) {
      String v_user = null; String v_psswrd = null;
      ParsLogin(login, ref v_user, ref v_psswrd);
      return v_user;
    }

    /// <summary>
    /// ����������� �� URL ��������� � ����������� ������� (��� ��� ����� ?)
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static String ExtractUrlQueryString(String url) {
      var v_lst = SplitString(url, '?');
      return v_lst.Length > 1 ? v_lst[1] : null;
    }

    /// <summary>
    /// ����������� �� ������ ���� "username/password" ������
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public static String ExtractUsrPwdFromLogin(String login) {
      String v_usrName = null; String v_psswrd = null;
      ParsLogin(login, ref v_usrName, ref v_psswrd);
      return v_psswrd;
    }

#if !SILVERLIGHT 
    /// <summary>
    /// ��������� ������� ������� � ��������� DataColumnCollection
    /// </summary>
    /// <param name="cols"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static Boolean ColumnExists(DataColumnCollection cols, String fieldName) {
      if (!String.IsNullOrEmpty(fieldName)) {
        foreach (DataColumn vCol in cols) {
          if (String.Equals(vCol.ColumnName, fieldName, StringComparison.CurrentCultureIgnoreCase)) {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// ������������� �������� value � ���� fieldName ������ row
    /// </summary>
    /// <param name="row"></param>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    public static void DataRowSetValue(DataRow row, String fieldName, Object value) {
      foreach (DataColumn vCol in row.Table.Columns) {
        if (vCol.ColumnName.ToUpper().Equals(fieldName.ToUpper())) {
          row[vCol.ColumnName] = Convert2Type(value, vCol.DataType);
        }
      }
    }

    /// <summary>
    /// ����������� �������� ���� fieldName �� ������ row
    /// </summary>
    /// <param name="row"></param>
    /// <param name="fieldName"></param>
    /// <param name="ifNull"></param>
    /// <returns></returns>
    public static Object DataRowGetValue(DataRow row, String fieldName, Object ifNull) {
      Object vResult = null;
      if (row != null && row.RowState != DataRowState.Detached) {
        foreach (DataColumn vCol in row.Table.Columns)
          if (vCol.ColumnName.ToUpper().Equals(fieldName.ToUpper())) {
            vResult = row[vCol.ColumnName];
            break;
          }
      }
      return vResult ?? ifNull;
    }
#endif

    /// <summary>
    /// ��������� �������� �� ��� ��������
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool TypeIsNumeric(Type type) {
      return type == typeof(Double) || type == typeof(Double?) ||
             type == typeof(float) || type == typeof(float?) ||
             type == typeof(Decimal) || type == typeof(Decimal?) ||
             type == typeof(Int16) || type == typeof(Int16?) ||
             type == typeof(Int32) || type == typeof(Int32?) ||
             type == typeof(Int64) || type == typeof(Int64?) ||
             type == typeof(UInt16) || type == typeof(UInt16?) ||
             type == typeof(UInt32) || type == typeof(UInt32?) ||
             type == typeof(UInt64) || type == typeof(UInt64?);
    }

    /// <summary>
    /// ����������� ������ � ������
    /// </summary>
    /// <param name="pObject"></param>
    /// <returns></returns>
    public static String ObjectAsString(Object pObject) {
      return Convert2Type<String>(pObject);
    }

    private static String _objectAsString(Object @object) {
      String v_rslt = null;
      if (@object != null) {
        var v_tp = @object.GetType();
        if (v_tp == typeof(System.DBNull))
          v_rslt = null;
        else if (v_tp == typeof(System.String))
          v_rslt = @object.ToString();
        else {
#if !SILVERLIGHT
          var v_culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true);
#else
          var v_culture = new CultureInfo(CultureInfo.CurrentCulture.Name);
#endif
          v_culture.NumberFormat.NumberDecimalSeparator = ".";
#if !SILVERLIGHT
          v_culture.DateTimeFormat.DateSeparator = "-";
#else
          //
#endif
          v_culture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy H:mm:ss";
          v_culture.DateTimeFormat.LongTimePattern = "";
          v_rslt = Convert.ToString(@object, v_culture);
          if (v_tp == typeof(System.DateTime))
            v_rslt = v_rslt.Trim();
        }
      }
      return EncodeANSII2UTF(v_rslt);
    }

    private static Object _convertFromNullable(Object value) {
      if (value == null)
        return null;
      var v_conversionType = value.GetType();
      if (v_conversionType.IsGenericType &&
        v_conversionType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
        var v_type = Nullable.GetUnderlyingType(v_conversionType);
        if (v_type == typeof(Boolean)) return (Boolean)value;
        if (v_type == typeof(Int16)) return (Int16)value;
        if (v_type == typeof(Int32)) return (Int32)value;
        if (v_type == typeof(Int64)) return (Int64)value;
        if (v_type == typeof(Decimal)) return (Decimal)value;
        if (v_type == typeof(Double)) return (Double)value;
        if (v_type == typeof(Single)) return (Single)value;
        if (v_type == typeof(DateTime)) return (DateTime)value;
        throw new Exception(String.Format("���������� �������������� {0} � {1}", v_conversionType.Name, v_type.Name));
      }
      return value;
    }

    private static Object _convertToNullable(Object value) {
      if (value == null)
        return null;
      var v_conversionType = value.GetType();
      if (v_conversionType.IsGenericType &&
        v_conversionType.GetGenericTypeDefinition() != typeof(Nullable<>)) {
        if (v_conversionType == typeof(Boolean)) return (Boolean?)value;
        if (v_conversionType == typeof(Int16)) return (Int16?)value;
        if (v_conversionType == typeof(Int32)) return (Int32?)value;
        if (v_conversionType == typeof(Int64)) return (Int64?)value;
        if (v_conversionType == typeof(Decimal)) return (Decimal?)value;
        if (v_conversionType == typeof(Double)) return (Double?)value;
        if (v_conversionType == typeof(Single)) return (Single?)value;
        if (v_conversionType == typeof(DateTime)) return (DateTime?)value;
        return value;
      }
      return value;
    }

    /// <summary>
    /// ����������� �������� � ����������� ����
    /// </summary>
    /// <param name="inValue"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static Object Convert2Type(Object inValue, Type targetType) {
      if ((inValue != null) && (targetType != null) && inValue.GetType() == targetType)
        return inValue;
      if ((targetType != null) && (targetType != typeof(Object))) {
        var v_tp = ((inValue == null) || (inValue == DBNull.Value)) ? null : inValue.GetType();

        var v_outType = Nullable.GetUnderlyingType(targetType);
        var v_outIsNullable = v_outType != null;
        v_outType = v_outType ?? targetType;
        var v_outIsClass = v_outType.IsClass;
        if (v_tp == null) {
          if (v_outIsNullable || v_outIsClass)
            return null;
          else {
            if (v_outType == typeof(String) || v_outType == typeof(Object))
              return null;
            else if (v_outType == typeof(DateTime))
              return DateTime.MinValue;
            else if (v_outType == typeof(Boolean))
              return false;
            else if (TypeIsNumeric(v_outType)) {
              IFormatProvider v_format = CultureInfo.CurrentCulture.NumberFormat;
              return Convert.ChangeType(0, v_outType, v_format);
            } else
              throw new Exception("�������� null �� ����� ���� ������������ ��� " + v_outType.Name + "!!! ", null);
          }
        }

        var v_inType = Nullable.GetUnderlyingType(v_tp);
        var v_inIsNullable = v_inType != null;
        v_inType = v_inType ?? v_tp;

        if (v_inIsNullable)
          inValue = _convertFromNullable(inValue);

        Object v_rslt = null;
        if (v_outType == typeof(DateTime)) {
          if (inValue == null) {
            v_rslt = DateTime.MinValue;
          } else if (v_inType == typeof(DateTime)) {
            v_rslt = inValue;
          } else if (v_inType == typeof(String)) {
            v_rslt = DateTimeParser.Instance.ParsDateTime((String)inValue);
          } else {
            throw new Exception("�������� ���� " + v_tp + " �� ����� ���� ������������ ��� DateTime!!! ", null);
          }

        } else if (v_outType == typeof(Boolean)) {
          if (inValue == null)
            v_rslt = false;
          else if (v_inType == typeof(Boolean))
            v_rslt = inValue;
          else if (TypeIsNumeric(v_inType)) {
            var v_invalDec = (Decimal)Convert.ChangeType(inValue, typeof(Decimal), CultureInfo.CurrentCulture.NumberFormat);
            v_rslt = (!v_invalDec.Equals(new Decimal(0)));
          } else if (v_inType == typeof(String)) {
            var v_valStr = ((String)inValue).ToUpper();
            v_rslt = (v_valStr.Equals("1") || v_valStr.Equals("Y") || v_valStr.Equals("T") || v_valStr.ToUpper().Equals("TRUE") || v_valStr.ToUpper().Equals("ON"));
          } else {
            throw new Exception("�������� ���� " + v_tp + " �� ����� ���� ������������ ��� boolean!!! ", null);
          }
        } else if (TypeIsNumeric(v_outType)) {
          IFormatProvider v_numberFormat = CultureInfo.CurrentCulture.NumberFormat;//new NumberFormatInfo();
          if (inValue == null)
            v_rslt = Convert.ChangeType(0, v_outType, v_numberFormat);
          if (TypeIsNumeric(v_inType)) {
            v_rslt = Convert.ChangeType(inValue, v_outType, v_numberFormat);
          } else if (v_inType == typeof(Boolean)) {
            v_rslt = (inValue != null) && ((Boolean)inValue) ? 1 : 0;
          } else if (v_inType == typeof(String)) {
            var v_valStr = (String)inValue;
            v_valStr = String.IsNullOrEmpty(v_valStr) ? "0" : v_valStr;
            var v_decSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            v_valStr = v_valStr.Replace(",", v_decSep);
            v_valStr = v_valStr.Replace(".", v_decSep);
            try {
              v_rslt = Convert.ChangeType(v_valStr, v_outType, v_numberFormat);
            } catch (Exception ex) {
              throw new Exception("�������� [" + v_valStr + "] ���� " + v_inType.Name + " �� ����� ���� ������������ ��� Numeric!!! ", null);
            }
          } else {
            throw new Exception("�������� ���� " + v_tp + " �� ����� ���� ������������ ��� Numeric!!! ", null);
          }
          if (v_outIsNullable)
            v_rslt = _convertToNullable(v_rslt);

        } else if (v_outType == typeof(String)) {
          v_rslt = _objectAsString(inValue);
        }

        return v_rslt;
      }

      return inValue;
    }

    /// <summary>
    /// ����������� �������� � ����������� ����
    /// </summary>
    /// <typeparam name="T">���</typeparam>
    /// <param name="pValue">��������</param>
    /// <returns></returns>
    public static T Convert2Type<T>(Object pValue) {
      return (T)Convert2Type(pValue, typeof(T));
    }

    /// <summary>
    /// ��������� ������ �� 1
    /// </summary>
    /// <param name="pPeriod">������ � ������� YYYYMM</param>
    /// <returns>������ � ������� YYYYMM</returns>
    public static String PeriodDec(String pPeriod) {
      if (pPeriod != null) {
        var v_year = Int32.Parse(pPeriod.Substring(0, 4));
        var v_month = Int32.Parse(pPeriod.Substring(4, 2));
        v_month--;
        if (v_month == 0) {
          v_year--;
          v_month = 12;
        }
        return String.Format("{0:0000}{1:00}", v_year, v_month);
      } else
        return null;
    }

    /// <summary>
    /// ����������� ������ �� 1
    /// </summary>
    /// <param name="period">������ � ������� YYYYMM</param>
    /// <returns>������ � ������� YYYYMM</returns>
    public static String PeriodInc(String period) {
      var v_year = Int32.Parse(period.Substring(0, 4));
      var v_month = Int32.Parse(period.Substring(4, 2));
      v_month++;
      if (v_month == 13) {
        v_year++;
        v_month = 1;
      }
      return String.Format("{0:0000}{1:00}", v_year, v_month);
    }

    /// <summary>
    /// ��������� ��������� ���� (Windows-1251) � �����
    /// </summary>
    /// <param name="pFileName">��� �����</param>
    /// <param name="pBuff">�����</param>
    public static void LoadWINFile(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, EncWindows1251, ref pBuff);
    }

    /// <summary>
    /// ��������� ��������� ���� (cp866) � �����
    /// </summary>
    /// <param name="pFileName">��� �����</param>
    /// <param name="pBuff">�����</param>
    public static void LoadDOSFile(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, EncCp866, ref pBuff);
    }

    /// <summary>
    /// ��������� ��������� ���� (UTF-8) � �����
    /// </summary>
    /// <param name="pFileName">��� �����</param>
    /// <param name="pBuff">�����</param>
    public static void LoadUTF8File(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, EncUtf8, ref pBuff);
    }

    /// <summary>
    /// ��������� ��������� ���� � �����
    /// </summary>
    /// <param name="fileName">��� �����</param>
    /// <param name="pEcoding">��� ���������</param>
    /// <param name="buff">�����</param>
    public static void LoadStrFile(String fileName, String pEcoding, ref String buff) {
      LoadStrFile(fileName, Encoding.GetEncoding(pEcoding), ref buff);
    }

    /// <summary>
    /// ��������� ��������� ���� � �����
    /// </summary>
    /// <param name="fileName">��� �����</param>
    /// <param name="encoding">���������</param>
    /// <param name="buff">�����</param>
    public static void LoadStrFile(String fileName, Encoding encoding, ref String buff) {
      if (File.Exists(fileName)) {
        var v_fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var v_file = new StreamReader(v_fs, encoding);
        String v_line = null;
        var v_bfr = new StringWriter();
        while ((v_line = v_file.ReadLine()) != null)
          v_bfr.WriteLine(v_line);
        buff = v_bfr.ToString();
        v_fs.Close();
      }

    }

    /// <summary>
    /// ��������� ��������� ���� � List<String>
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static List<String> LoadStrFile(String fileName, Encoding encoding) {
      var v_result = new List<String>();
      if (File.Exists(fileName)) {
        var v_fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        try {
          var v_file = new StreamReader(v_fs, encoding);
          String v_line = null;
          while ((v_line = v_file.ReadLine()) != null)
            v_result.Add(v_line);
        } finally {
          v_fs.Close();
        }
      }
      return v_result;
    }

#if !SILVERLIGHT
    /// <summary>
    /// ��������� dlg � ������ pControl
    /// </summary>
    /// <param name="pControl"></param>
    /// <param name="dlg"></param>
    public static void RunDelegateOnControl(Control pControl, Action dlg) {
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

    /// <summary>
    /// ���������� PropertyInfo �� propertyName ���� type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="caseSensetive">� ����� ��������</param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyInfo(Type type, String propertyName, Boolean caseSensetive) {
      if (type != null) {
        var prop = type.GetProperties().Where((p) => { return p.Name.Equals(propertyName, (caseSensetive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)); }).FirstOrDefault();
        return prop;
      } else
        return null;
    }

    /// <summary>
    /// ���������� PropertyInfo �� propertyName ���� type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyInfo(Type type, String propertyName) {
      return GetPropertyInfo(type, propertyName, true);
    }

    /// <summary>
    /// ���������� ������� ���� AttrType ��� prop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static T GetPropertyAttr<T>(PropertyInfo prop) where T : Attribute {
      if (prop != null) {
        var attrs = prop.GetCustomAttributes(typeof(T), true);
        if (attrs.Length > 0) {
          return attrs[0] as T;
        } else
          return null;
      } else
        return null;
    }

    /// <summary>
    /// ���������� ������� ���� AttrType ��� propertyName ���� type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static T GetPropertyAttr<T>(Type type, String propertyName) where T : Attribute {
      var prop = GetPropertyInfo(type, propertyName);
      return GetPropertyAttr<T>(prop);
    }

    /// <summary>
    /// ������������� �������� �������� ���� T ��� �������� propertyName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="attrPropertyName"></param>
    /// <param name="attrPropValue"></param>
    public static void SetPropertyAttr<T>(Type type, String propertyName, String attrPropertyName, Object attrPropValue) where T : Attribute {
      var prop = GetPropertyInfo(type, propertyName);
      var attr = GetPropertyAttr<T>(prop);
      if (attr != null)
        Utl.SetPropertyValue(attr, attrPropertyName, attrPropValue);
    }

#if !SILVERLIGHT
    /// <summary>
    /// ���������� �������� �������� ���� A �������� propName ��� ���� T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="A"></typeparam>
    /// <param name="propName"></param>
    /// <returns></returns>
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
    /// ������������ � ����� ����� ������ ���� ����� ��� ����������. 
    /// ��������: 1) ���� ���� "some_file.ext" ����������, �� ������� ������ "some_file(1).ext"
    ///           2) ���� ���� "some_file(1).ext" ����������, �� ������� ������ "some_file(2).ext"
    ///           � �.�.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static String IncFileNameIndexIfExists(String fileName) {
      var vResult = fileName;
      if(File.Exists(fileName)) {
        var vExt = Path.GetExtension(vResult);
        var vr = new Regex("[(]\\d+[)][\\.]", RegexOptions.IgnoreCase);
        var vMatch = vr.Match(vResult);
        if(vMatch.Success) {
          var vNumStr = vMatch.Value.Substring(1, vMatch.Value.Length - 3);
          var vNum = Int16.Parse(vNumStr); vNum++;
          vResult = vr.Replace(vResult, "(" + vNum + ").");
        } else
          vResult = Utl.NormalizeDir(Path.GetDirectoryName(vResult)) + Path.GetFileNameWithoutExtension(vResult) + "(1)" + vExt;
      }
      return vResult;
    }

    /// <summary>
    /// ��������� �������� �� ������ text ������ �����, ���� ��, 
    /// �� ��������� ���������� ����� ����� � text
    /// </summary>
    /// <param name="pCurrentPath"></param>
    /// <param name="vText"></param>
    public static void TryLoadTextAsFile(String pCurrentPath, ref String vText) {
      var vCurrentDirectory = Directory.GetCurrentDirectory();
      var vCurrentPath = Path.GetFullPath(pCurrentPath);
      Directory.SetCurrentDirectory(vCurrentPath);
      try {
        var vSQLFileFN = vText;
        if(File.Exists(vSQLFileFN)) {
          try {
            Bio.Helpers.Common.Utl.LoadWINFile(vSQLFileFN, ref vText);
          } catch(Exception ex) {
            throw new Exception("������ ��� �������� ����� [" + vSQLFileFN + "]. ���������: " + ex.Message);
          }
        }
      } finally {
        Directory.SetCurrentDirectory(vCurrentDirectory);
      }
    }

    /// <summary>
    /// ���� � text ���������� ���� {text-file:..\ftw.sql} c ������ �����, ���� �������, 
    /// �� ��������� ���������� ���������������� ����� � ��������������� ������� text
    /// </summary>
    /// <param name="currentPath"></param>
    /// <param name="text"></param>
    public static void TryLoadMappedFiles(String currentPath, ref String text) {
      var fileContent = RegexFind(text, "(?<={text-file:).+(?=})", true);
      TryLoadTextAsFile(currentPath, ref fileContent);
      RegexReplace(ref text, "{text-file:.+}", fileContent, true);
    }

    /// <summary>
    /// ������������ �� ����� ��������� ������
    /// </summary>
    /// <param name="cellBounds"></param>
    /// <param name="focused"></param>
    /// <param name="gra"></param>
    /// <param name="borders"></param>
    public static void DrawAnSelctedCell(Rectangle cellBounds, Boolean focused, Graphics gra, AnchorStyles borders) {
      var col1 = Color.RoyalBlue;
      var col2 = Color.Blue;
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

        DrawAnSelctedCell(rct, focused, a.Graphics, vBorders);

      }
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// ������ ����� ������ ��������� �����
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
    /// ������ ����� ������ ���������� �������� ������
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="a"></param>
    /// <param name="focused"></param>
    public static void DrawTreeSelectionAlt(TreeView tree, DrawTreeNodeEventArgs a, Boolean focused) {

      Rectangle nodeBounds = a.Node.Bounds;
      nodeBounds.Width += 10;
      a.Graphics.FillRectangle(Brushes.White, nodeBounds);
      DrawAnSelctedCell(nodeBounds, focused, a.Graphics, AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom);

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
    /// ���������� �������� ������
    /// </summary>
    /// <param name="pRegKeyName">���� � �������.</param>
    /// <param name="valName">��� ���������.</param>
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
    /// ��������� �������� �� �������.
    /// </summary>
    /// <param name="regKeyName">���� � �������.</param>
    /// <param name="valName">��� ���������.</param>
    /// <param name="defVal">�������� �� ���������.</param>
    /// <returns>�������� ���������.</returns>
    public static Object RegistryCUGetValue(String regKeyName, String valName, object defVal) {
      RegistryKey key = Registry.CurrentUser.CreateSubKey(regKeyName);
      Object vResult = key.GetValue(valName, defVal) ?? defVal;
      return vResult;
    }
#endif

#if !SILVERLIGHT
    /// <summary>
    /// ������� ���� � ������� ������������ �������� CU || LM || ...
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
    /// ������� ������, ������� ����� ���������� � URL
    /// </summary>
    /// <returns></returns>
    public static String BuidQueryStrParams(Dictionary<String, String> prms) {
      String rslt = null;
      foreach (String k in prms.Keys) {
        String vParamStr = k + "=" + HttpUtility.UrlEncode(prms[k] as String);
        Utl.AppendStr(ref rslt, vParamStr, "&");
      }
      return rslt;
    }

    /// <summary>
    /// ������� ������, ������� ����� ���������� � URL
    /// </summary>
    /// <param name="baseURL"></param>
    /// <returns></returns>
    public static String BuidQueryStrParams(String baseURL, Dictionary<String, String> prms) {
      String rslt = BuidQueryStrParams(prms);
      return (baseURL.IndexOf('?') >= 0) ? baseURL + "&" + rslt : baseURL + "?" + rslt;
    }

    /// <summary>
    /// ���������� true ���� ��������� TRUE|T|1|Y ����� false
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Boolean ParsBoolean(String val) {
      if (String.IsNullOrEmpty(val))
        return false;
      else {
        val = val.ToUpper();
        return val.Equals("TRUE") || val.Equals("T") || val.Equals("1") || val.Equals("Y");
      }
    }

    /// <summary>
    /// ����������� �� ���������� ��������� �� ������ ����� � ������ ���� ��� ������ ������� �����
    /// raise_application_error(2xxxx, 'message');, ����� ������ "message" ����� null.
    /// </summary>
    /// <returns></returns>
    public static void ExtractOracleApplicationError(String exMessage, out Int32 errCode, out String errMsg) {
      errCode = 0;
      errMsg = null;
      if (exMessage != null) {
        var vStrtIndx = exMessage.IndexOf("ORA-2", System.StringComparison.Ordinal);
        if (vStrtIndx >= 0) {
          var vEndIndx = exMessage.IndexOf("ORA-", vStrtIndx + 5, System.StringComparison.Ordinal);
          var vLen = vEndIndx - vStrtIndx;
          if (vLen < 0)
            vLen = exMessage.Length - vStrtIndx;
          var vMsg = exMessage;
          if (vLen > 0)
            vMsg = exMessage.Substring(vStrtIndx, vLen);
          errCode = Int32.Parse(vMsg.Substring(4, 5));
          errMsg = vMsg.Substring(11);
        }
      }
    }

    /// <summary>
    /// ����������� ���� � ����� �� ����������� � �����������
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static String ResolvePath(String path) {
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
          String vLine = String.Format("[{0} {1}] - {2}", v_point.ToShortDateString(), v_point.ToLongTimeString(), "������!!!");
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
      Boolean hasSelect = Utl.RegexMatch(sqlText, @"\bSELECT\b", true);
      hasSelect = hasSelect && Utl.RegexMatch(sqlText, @"\bFROM\b", true);
      if (hasSelect)
        return CommandType.Text;
      else
        return CommandType.StoredProcedure;
    }

#endif

#if SILVERLIGHT
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xapName"></param>
    /// <param name="onLoadAction"></param>
    public static void LoadRemoteAssembly(String xapName, Action<OpenReadCompletedEventArgs, Assembly> onLoadAction) {
      if (!String.IsNullOrEmpty(xapName)) {
        var wc = new WebClient();
        wc.OpenReadCompleted += new OpenReadCompletedEventHandler((sndr, atrs) => {
          var appManifest = new StreamReader(System.Windows.Application.GetResourceStream(new StreamResourceInfo(atrs.Result, null),
                               new Uri("AppManifest.xaml", UriKind.Relative)).Stream).ReadToEnd();


          var deploymentRoot = XDocument.Parse(appManifest).Root;
          var deploymentParts = (from assemblyParts in deploymentRoot.Elements().Elements()
                                            select assemblyParts).ToList();
          foreach (var xElement in deploymentParts.Reverse<XElement>()) {
            var source = xElement.Attribute("Source").Value;
            var streamInfo = System.Windows.Application.GetResourceStream(new StreamResourceInfo(atrs.Result,
                                              "application/binary"), new Uri(source, UriKind.Relative));

            var asmPart = new System.Windows.AssemblyPart();
            var asm = asmPart.Load(streamInfo.Stream);
            if (onLoadAction != null)
              onLoadAction(atrs, asm);
          }
        });
        wc.OpenReadAsync(new Uri(xapName, UriKind.Relative));
      }
    }

    /// <summary>
    /// true - DesignTime
    /// </summary>
    public static Boolean DesignTime {
      get {
        return DesignerProperties.IsInDesignTool;
      }
    }

    private const String csCookieValTypeSplitter = "|value-type|";

    /// <summary>
    /// ��������� �������� � Cookie
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expireDays"></param>
    /// <param name="silent"></param>
    /// <exception cref="Exception"></exception>
    public static void SetCookie(String key, Object value, Int32 expireDays, Boolean silent) {
      var expireDate = DateTime.Now + TimeSpan.FromDays(expireDays);
      var newCookieValueType = value != null ? value.GetType() : typeof(String);
      var newCookieValue = value + csCookieValTypeSplitter + newCookieValueType.FullName;
      var newCookie = key.Trim() + "=" + newCookieValue + ";expires=" + expireDate.ToString("R");
      try {
        HtmlPage.Document.SetProperty("cookie", newCookie);
      } catch (Exception) {
        if (!silent)
          throw;
      }
    }

    /// <summary>
    /// ����������� �������� �� Cookie
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <param name="silent"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Object GetCookie(String key, Object defaultValue, Boolean silent) {
      String[] cookies = null;
      try {
        cookies = HtmlPage.Document.Cookies.Split(';');
      } catch (Exception) {
        if (!silent)
          throw;
      }
      if (cookies != null) {
        foreach (String cookie in cookies) {
          var keyValue = cookie.Trim().Split('=');
          if (keyValue.Length == 2) {
            if (String.Equals(keyValue[0], key)) {
              var valFullStr = keyValue[1];
              var valueParts = Utl.SplitString(valFullStr, csCookieValTypeSplitter);
              if (valueParts.Length == 2) {
                var valStr = valueParts[0];
                var valTypeName = valueParts[1];
                var valType = Type.GetType(valTypeName);
                var valObj = Utl.Convert2Type(valStr, valType);
                return valObj;
              } else
                return null;
            }
          }
        }
      }
      return defaultValue;

    }

    /// <summary>
    /// ��������� ��������
    /// </summary>
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
    /// ���������� ����� ��� ������, ��� 1-���...7-���
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Int32 DayOfWeekRu(DateTime date) {
      String[] v_days = new String[] { "��", "��", "��", "��", "��", "��", "��" };
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
          throw new EBioException(String.Format("������ ��� ���������� ������� {0} � IsolatedStorageSettings.ApplicationSettings, ��������: {1}\n ���������: {2}", objName, "" + obj, ex.Message), ex);
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
        throw new EBioException(String.Format("������ ��� �������������� ������� {0} �� IsolatedStorageSettings.ApplicationSettings.", objName), ex);
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