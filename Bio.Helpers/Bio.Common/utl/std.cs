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
  /// Структура описывающая пару - "Имя пользователя"/"Пароль"
  /// </summary>
  public struct Login {
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public String user;

    /// <summary>
    /// Пароль
    /// </summary>
    public String password;
  }

  /// <summary>
  /// Утилиты общего назначения
  /// </summary>
  public class Utl {

    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String EncIso88591 = "ISO-8859-1";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String EncUtf8 = "UTF-8";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String EncCp866 = "Cp866";
    /// <summary>
    /// Константа - имя кодировки
    /// </summary>
    public const String EncIso88595 = "ISO-8859-5";
    /// <summary>
    /// Константа - имя кодировки
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
    /// Кодировка по умолчанию
    /// </summary>
    public static readonly Encoding DefaultEncoding = Encoding.Default;
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
              return v_rslt.Trim().Equals("") ? null : v_rslt;
            }
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
        var v_enc = new UTF8Encoding();
        var v_bfr = v_enc.GetBytes(msg);
        v_bfr = Encoding.Convert(Encoding.GetEncoding(EncWindows1251), Encoding.GetEncoding(SYS_ENCODING), v_bfr);
        return v_enc.GetString(v_bfr);
      }
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
      return value ?? String.Empty;
    }

    /// <summary>
    /// Если на NullOrEmpty, то возвращает строку "NULL" иначе возвращает значение value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static String NullToNULL(String value) {
      return String.IsNullOrEmpty(value) ? "NULL" : value;
    }

    /// <summary>
    /// Если на входе пустая строка или null, то возвращает ноль (нуна для перевода в double)
    /// </summary>
    /// <param name="str"></param>
    /// <returns>0 или входная строка</returns>
    public static String BlankTo0(String str) {
      return String.IsNullOrEmpty(str) ? "0" : str;
    }

    /// <summary>
    /// Ковертирует строку в double
    /// </summary>
    /// <param name="prm"></param>
    /// <returns></returns>
    public static Double ToDbl(String prm) {
      var v_provider = new NumberFormatInfo {NumberDecimalSeparator = "."};
      return Double.Parse(BlankTo0(prm.Replace(",", ".")), v_provider);
    }

    /// <summary>
    /// Если true, тогда возвращает "Y" иначе "N"
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
    /// Возвращяет source ?? target
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    static public String Nvl(String source, String target) {
      return source ?? target;
    }

    /// <summary>
    /// Возвращяет source ?? target
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    static public Int32 Nvl(Int32? source, Int32 target) {
      return source ?? target;
    }

#if !SILVERLIGHT
    /// <summary>
    /// Строит сообщение администратору
    /// </summary>
    /// <param name="vAdmName"></param>
    /// <param name="vAdmCntcts"></param>
    /// <returns></returns>
    static public String GetMsg_ToAdminPlease(String vAdmName, XmlNodeList vAdmCntcts) {
      var v_rslt = "Обратитесь к администратору системы.<br>";
      v_rslt += "Контактная информация:<br>";
      for (var i = 0; i < vAdmCntcts.Count; i++) {
        var v_cntct = (XmlElement)vAdmCntcts[i];
        var v_cc = "";
        if (v_cntct.GetAttribute("type").Equals("office"))
          v_cc = "ком. №";
        else if (v_cntct.GetAttribute("type").Equals("phone"))
          v_cc = "телефон";
        else if (v_cntct.GetAttribute("type").Equals("mail"))
          v_cc = "эл. почта";
        v_rslt += v_cc + " : " + v_cntct.InnerText + "<br>";
      }
      v_rslt += "имя : " + vAdmName + "<br>";
      return v_rslt;
    }
#endif

    /// <summary>
    /// Подставляет appURL вместо "SYS_APP_URL"
    /// </summary>
    /// <param name="appURL"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static String NormalizeURL(String appURL, String url) {
      return url != null ? url.Replace("SYS_APP_URL", appURL) : null;
    }

    /// <summary>
    /// Разбивает строку str на подстроки String[] с разделителями delimiter[]
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, Char[] delimeter) {
      return str == null ? new String[0] : str.Split(delimeter);
    }

    /// <summary>
    /// Разбивает строку str на подстроки String[] с разделителем delimiter
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, Char delimiter) {
      return SplitString(str, new[] { delimiter });
    }


    /// <summary>
    /// Разбивает строку str на подстроки String[] с разделителями delimiter[]
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
    /// Разбивает строку str на подстроки String[] с разделителем delimiter
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static String[] SplitString(String str, String delimiter) {
      return SplitString(str, new[] { delimiter });
    }

    /// <summary>
    /// Объединяет строки lines[] в одну строку через разделитель delimiter
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
    /// Заменяет "&" на "&amp;"
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static String UrlEncode(String line) {
      return line.Replace("&", "&amp;");
    }

    /// <summary>
    /// Заменяет "&amp;" на "&"
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static String UrlDecode(String line) {
      return line.Replace("&amp;", "&");
    }

    /// <summary>
    /// Возвращает x - (x / y) * y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Mod(int x, int y) {
      return x - (x / y) * y;
    }

    private static readonly Mutex FMutexOfLogFile = new Mutex();
    /// <summary>
    /// Добавляет строку line в файл fileName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="line"></param>
    /// <param name="encoding">Кодировка для записи</param>
    /// <param name="createPath">Создать путь если не сужествует</param>
    public static void AppendStringToFile(String fileName, String line, Encoding encoding, Boolean createPath) {
      FMutexOfLogFile.WaitOne();
      try {
        var v_dir = Path.GetDirectoryName(fileName);
        if (v_dir != null && (!Directory.Exists(v_dir) && createPath))
          Directory.CreateDirectory(v_dir);
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
    /// Добавляет строку line в файл fileName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="line"></param>
    /// <param name="encoding"></param>
    public static void AppendStringToFile(String fileName, String line, Encoding encoding) {
      AppendStringToFile(fileName, line, encoding, false);
    }

    /// <summary>
    /// Сохраняет строку line в файл fileName
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
    /// Добавляет obj.ToString() в строку  через разделители
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

    private const int _CHUNKSIZE = 40000;
    /// <summary>
    /// Читает поток в массив байтов
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    public static void ReadBinStreamInBuffer(Stream stream, out Byte[] buffer) {
      buffer = new Byte[stream.Length];
      var v_chunkPos = 0;
      while (stream.Position > -1 && stream.Position < stream.Length) {
        int v_chunkSize;
        if (stream.Length - stream.Position >= _CHUNKSIZE)
          v_chunkSize = _CHUNKSIZE;
        else
          v_chunkSize = (int)(stream.Length - stream.Position);
        stream.Read(buffer, v_chunkPos, v_chunkSize);
        v_chunkPos += v_chunkSize;
      }

    }

    /// <summary>
    /// Загружает файл в массив байтов
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
    /// Пишет массив байтов в файл
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
    /// Ковертирует строку вида ###0(S|M|H|D) в кол-во милисекунд, где S-сек, M-мин, H-час, D-дней
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
    /// Сдвигает массив влево на указанное кол-во элементов 
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
    /// Проверяет содержит ли строка line элемент elem
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
    /// Проверяет содержит ли строка line элемент elem
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
    /// Проверяет имеют ли два списка общие элементы
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
    /// Проверяет есть ли хоть одна роль из userRoles в objRoles
    /// </summary>
    /// <param name="objRoles"></param>
    /// <param name="userRoles"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static Boolean CheckRoles(String objRoles, String userRoles, Char[] delimeter) {
      var v_objectRoles = objRoles;
      if ((v_objectRoles == null) || (v_objectRoles.Equals("")))
        v_objectRoles = "*";

      // Проверяем наличие * в pObjectRoles
      Boolean v_result = IsElementInDelimitedLine(v_objectRoles, "*", delimeter);

      if (!v_result) {
        //* нет в pObjectRoles проверяем пересечение
        v_result = DelimitedLineHasCommonTags(v_objectRoles, userRoles, delimeter);
      }

      if (v_result) {
        //Проверяем наличие исключающих ролей
        var v_lst = userRoles.Split(delimeter);
        foreach (var t in v_lst)
          if (IsElementInDelimitedLine(v_objectRoles, "!" + t, delimeter))
            v_result = false;
      }

      return v_result;
    }

    /// <summary>
    /// Создает папку если ее не существует
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static String ForceDirectory(String path) {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      return path;
    }

    /// <summary>
    /// Проверяет value == "true", не чуствительно к регистру
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Boolean CheckBoolValue(String value) {
      return String.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Преобразует массив строк в список строк разделенных запятой
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
    /// Нормализует путь к папке - добавляет в конец "\" если нужно
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
    /// Строит полный путь из относительного
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
    /// Возвращаяет путь к информационному объекту по коду
    /// </summary>
    /// <param name="bioCode"></param>
    /// <returns></returns>
    public static String GenBioLocalPath(String bioCode) {
      var v_fLstIndx = bioCode.LastIndexOf(".", StringComparison.Ordinal);
      return v_fLstIndx >= 0 ? bioCode.Substring(0, v_fLstIndx + 1).Replace(".", "\\") : null;
    }

    /// <summary>
    /// Сравнивает две версии
    /// </summary>
    /// <param name="verLeft"></param>
    /// <param name="verRight"></param>
    /// <returns>[-1]-меньше; [0]-равно; [1]-больше</returns>
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
    /// Удаляет содержимое папки
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
    /// Разбирает логин
    /// </summary>
    /// <param name="login">логин вида [имя пользователя]/[пароль]</param>
    /// <returns><see cref="Login"/></returns>
    public static Login ParsLogin(String login) {
      var v_result = new Login {
        user = null,
        password = null
      };
      var v_lst = SplitString(login, '/');
      if (v_lst.Length > 0)
        v_result.user = v_lst[0];
      if (v_lst.Length > 1)
        v_result.password = v_lst[1];
      return v_result;
    }

    /// <summary>
    /// Проверяет регулярное вырожение для строки. Возвращает Match.Success.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="regex"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static Match RegexMatch(String line, String regex, Boolean ignoreCase) {
      var v_regex = new Regex(regex, ((ignoreCase) ? RegexOptions.IgnoreCase : RegexOptions.None));
      return v_regex.Match(line);
    }

    /// <summary>
    /// Проверяет регулярное вырожение для строки. Возвращает Match.Value.
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
    /// Ищет позицию вхождения строки с помощью регулярного вырожения
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
    /// Заменяет подстроку с помощью регулярного вырожения
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
    /// Разбирает Connection String
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    public static IDictionary<String, String> ParsConnectionStr(String connStr) {
      IDictionary<String, String> v_rslt = new Dictionary<String, String>();
      if (connStr != null) {
        var v_spr = new[] { ';' };
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
    /// Строит Connection String
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    public static String BuildConnectionStr(IDictionary<String, String> connStr) {
      String v_rslt = null;
      if (connStr != null) {
        foreach (var v_item in connStr) {
          AppendStr(ref v_rslt, v_item.Key + "=" + v_item.Value, ";");
        }
      }
      return v_rslt;
    }

    /// <summary>
    /// Вытаскивает имя функции из строки типа "functionName(param, ...)"
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
    /// Добавляет строку str в конец строки line через разделитель delimiter
    /// </summary>
    /// <param name="line"></param>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <param name="ignoreNull">если str - null or empty, то ничего не добавляется</param>
    public static void AppendStr(ref String line, String str, String delimiter, Boolean ignoreNull) {
      if (String.IsNullOrEmpty(str) && ignoreNull)
        return;
      if (String.IsNullOrEmpty(line))
        line = str;
      else
        line += delimiter + str;
    }

    /// <summary>
    /// Добавляет строку str в конец строки line через разделитель delimiter
    /// </summary>
    /// <param name="line"></param>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    public static void AppendStr(ref String line, String str, String delimiter) {
      AppendStr(ref line, str, delimiter, false);
    }

    /// <summary>
    /// Вовращает строку - URL для запроса к серверу Bio
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
    /// Вытаскивает из строки вида "username/password" имя пользователя
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public static String ExtractUsrNameFromLogin(String login) {
      return ParsLogin(login).user;
    }

    /// <summary>
    /// Вытаскивает из URL подстроку с параметрами запроса (все что после ?)
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static String ExtractUrlQueryString(String url) {
      var v_lst = SplitString(url, '?');
      return v_lst.Length > 1 ? v_lst[1] : null;
    }

    /// <summary>
    /// Вытаскивает из строки вида "username/password" пароль
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public static String ExtractUsrPwdFromLogin(String login) {
      return ParsLogin(login).password;
    }

#if !SILVERLIGHT 
    /// <summary>
    /// Проверяет наличие колонки в коллекции DataColumnCollection
    /// </summary>
    /// <param name="cols"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static Boolean ColumnExists(DataColumnCollection cols, String fieldName) {
      if (!String.IsNullOrEmpty(fieldName)) {
        foreach (DataColumn col in cols) {
          if (String.Equals(col.ColumnName, fieldName, StringComparison.CurrentCultureIgnoreCase)) {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Устанавливает значение value в поле fieldName строки row
    /// </summary>
    /// <param name="row"></param>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    public static void DataRowSetValue(DataRow row, String fieldName, Object value) {
      foreach (DataColumn col in row.Table.Columns) {
        if (col.ColumnName.ToUpper().Equals(fieldName.ToUpper())) {
          row[col.ColumnName] = Convert2Type(value, col.DataType);
        }
      }
    }

    /// <summary>
    /// Вытаскивает значение поля fieldName из строки row
    /// </summary>
    /// <param name="row"></param>
    /// <param name="fieldName"></param>
    /// <param name="ifNull"></param>
    /// <returns></returns>
    public static Object DataRowGetValue(DataRow row, String fieldName, Object ifNull) {
      Object v_result = null;
      if (row != null && row.RowState != DataRowState.Detached) {
        foreach (DataColumn col in row.Table.Columns)
          if (col.ColumnName.ToUpper().Equals(fieldName.ToUpper())) {
            v_result = row[col.ColumnName];
            break;
          }
      }
      return v_result ?? ifNull;
    }
#endif

    /// <summary>
    /// Проверяет является ли тип числовым
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
    /// Преобразует объект в строку
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
        if (v_tp == typeof(String))
          v_rslt = @object.ToString();
        else {
#if !SILVERLIGHT
          var v_culture = new CultureInfo(CultureInfo.CurrentCulture.Name, true);
          v_culture.DateTimeFormat.DateSeparator = "-";
#else
          var v_culture = new CultureInfo(CultureInfo.CurrentCulture.Name);
#endif
          v_culture.NumberFormat.NumberDecimalSeparator = ".";
          v_culture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy H:mm:ss";
          v_culture.DateTimeFormat.LongTimePattern = "";
          v_rslt = Convert.ToString(@object, v_culture);
          if (v_tp == typeof(DateTime))
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
        throw new Exception(String.Format("Невозможно конвертировать {0} в {1}", v_conversionType.Name, v_type.Name));
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
    /// Преобразует значение к конкретному типу
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
          if (v_outType == typeof(String) || v_outType == typeof(Object))
            return null;
          if (v_outType == typeof(DateTime))
            return DateTime.MinValue;
          if (v_outType == typeof(Boolean))
            return false;
          if (TypeIsNumeric(v_outType)) {
            IFormatProvider v_format = CultureInfo.CurrentCulture.NumberFormat;
            return Convert.ChangeType(0, v_outType, v_format);
          }
          throw new Exception("Значение null не может быть представлено как " + v_outType.Name + "!!! ", null);
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
            throw new Exception("Значение типа " + v_tp + " не может быть представлено как DateTime!!! ", null);
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
            throw new Exception("Значение типа " + v_tp + " не может быть представлено как boolean!!! ", null);
          }
        } else if (TypeIsNumeric(v_outType)) {
          IFormatProvider v_numberFormat = CultureInfo.CurrentCulture.NumberFormat;//new NumberFormatInfo();
          if (inValue == null)
            v_rslt = Convert.ChangeType(0, v_outType, v_numberFormat);
          else {
            if (TypeIsNumeric(v_inType)) {
              v_rslt = Convert.ChangeType(inValue, v_outType, v_numberFormat);
            } else if (v_inType == typeof (Boolean)) {
              v_rslt = ((Boolean) inValue) ? 1 : 0;
            } else if (v_inType == typeof (String)) {
              var v_valStr = (String) inValue;
              v_valStr = String.IsNullOrEmpty(v_valStr) ? "0" : v_valStr;
              var v_decSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
              v_valStr = v_valStr.Replace(",", v_decSep);
              v_valStr = v_valStr.Replace(".", v_decSep);
              try {
                v_rslt = Convert.ChangeType(v_valStr, v_outType, v_numberFormat);
              } catch (Exception ex) {
                throw new Exception("Значение [" + v_valStr + "] типа " + v_inType.Name + " не может быть представлено как Numeric!!! Сообщение: " + ex.Message, null);
              }
            } else {
              throw new Exception("Значение [" + inValue + "] типа " + v_tp + " не может быть представлено как Numeric!!! ", null);
            }
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
    /// Преобразует значение к конкретному типу
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    /// <param name="pValue">Значение</param>
    /// <returns></returns>
    public static T Convert2Type<T>(Object pValue) {
      return (T)Convert2Type(pValue, typeof(T));
    }

    /// <summary>
    /// Уменьшает период на 1
    /// </summary>
    /// <param name="pPeriod">Период в формате YYYYMM</param>
    /// <returns>Период в формате YYYYMM</returns>
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
      }
      return null;
    }

    /// <summary>
    /// Увеличивает период на 1
    /// </summary>
    /// <param name="period">Период в формате YYYYMM</param>
    /// <returns>Период в формате YYYYMM</returns>
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
    /// Загружает текстовый файл (Windows-1251) в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadWINFile(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, EncWindows1251, ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл (cp866) в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadDOSFile(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, EncCp866, ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл (UTF-8) в буфер
    /// </summary>
    /// <param name="pFileName">Имя файла</param>
    /// <param name="pBuff">Буфер</param>
    public static void LoadUTF8File(String pFileName, ref String pBuff) {
      LoadStrFile(pFileName, EncUtf8, ref pBuff);
    }

    /// <summary>
    /// Загружает текстовый файл в буфер
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="pEcoding">Имя кодировки</param>
    /// <param name="buff">Буфер</param>
    public static void LoadStrFile(String fileName, String pEcoding, ref String buff) {
      LoadStrFile(fileName, Encoding.GetEncoding(pEcoding), ref buff);
    }

    /// <summary>
    /// Загружает текстовый файл в буфер
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="encoding">Кодировка</param>
    /// <param name="buff">Буфер</param>
    public static void LoadStrFile(String fileName, Encoding encoding, ref String buff) {
      if (File.Exists(fileName)) {
        var v_fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        var v_file = new StreamReader(v_fs, encoding);
        String v_line;
        var v_bfr = new StringWriter();
        while ((v_line = v_file.ReadLine()) != null)
          v_bfr.WriteLine(v_line);
        buff = v_bfr.ToString();
        v_fs.Close();
      }

    }

    /// <summary>
    /// Загружает текстовый файл в List
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
          String v_line;
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
    /// Выполняет dlg в потоке pControl
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
    /// Возвращает PropertyInfo по propertyName типа type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <param name="caseSensetive">С учето регистра</param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyInfo(Type type, String propertyName, Boolean caseSensetive) {
      return type != null ? type.GetProperties().FirstOrDefault(
          p => p.Name.Equals(propertyName, (caseSensetive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
        ) : null;
    }

    /// <summary>
    /// Возвращает PropertyInfo по propertyName типа type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyInfo(Type type, String propertyName) {
      return GetPropertyInfo(type, propertyName, true);
    }

    /// <summary>
    /// Возвращает атрибут типа AttrType для prop
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static T GetPropertyAttr<T>(PropertyInfo prop) where T : Attribute {
      if (prop != null) {
        var attrs = prop.GetCustomAttributes(typeof(T), true);
        if (attrs.Length > 0) {
          return attrs[0] as T;
        }
        return null;
      }
      return null;
    }

    /// <summary>
    /// Возвращает атрибут типа AttrType для propertyName типа type
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
    /// Устанавливает значение атрибута типа T для свойства propertyName
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
        SetPropertyValue(attr, attrPropertyName, attrPropValue);
    }

#if !SILVERLIGHT
    /// <summary>
    /// Возвращает значение атрибута типа A свойства propName для типа T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TA"></typeparam>
    /// <param name="propName"></param>
    /// <returns></returns>
    public static TA GetAttributeOfProp<T, TA>(String propName) where TA:Attribute {
      PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(T));
      foreach (PropertyDescriptor pd in pdc) {
        if (String.Equals(pd.Name, propName, StringComparison.CurrentCulture)) {
          var attr = (TA)pd.Attributes[typeof(TA)];
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
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static String IncFileNameIndexIfExists(String fileName) {
      var v_result = fileName;
      if(File.Exists(fileName)) {
        var v_ext = Path.GetExtension(v_result);
        var vr = new Regex("[(]\\d+[)][\\.]", RegexOptions.IgnoreCase);
        var v_match = vr.Match(v_result);
        if(v_match.Success) {
          var v_numStr = v_match.Value.Substring(1, v_match.Value.Length - 3);
          var v_num = Int16.Parse(v_numStr); v_num++;
          v_result = vr.Replace(v_result, "(" + v_num + ").");
        } else
          v_result = NormalizeDir(Path.GetDirectoryName(v_result)) + Path.GetFileNameWithoutExtension(v_result) + "(1)" + v_ext;
      }
      return v_result;
    }

    /// <summary>
    /// Проверяет является ли данный text именем файла, если да, 
    /// то загружает содержимое этого файла а text
    /// </summary>
    /// <param name="pCurrentPath"></param>
    /// <param name="vText"></param>
    public static void TryLoadTextAsFile(String pCurrentPath, ref String vText) {
      var v_currentDirectory = Directory.GetCurrentDirectory();
      var v_currentPath = Path.GetFullPath(pCurrentPath);
      Directory.SetCurrentDirectory(v_currentPath);
      try {
        var v_sqlFileFN = vText;
        if(File.Exists(v_sqlFileFN)) {
          try {
            LoadWINFile(v_sqlFileFN, ref vText);
          } catch(Exception ex) {
            throw new Exception("Ошибка при загрузке файла [" + v_sqlFileFN + "]. Сообщение: " + ex.Message);
          }
        }
      } finally {
        Directory.SetCurrentDirectory(v_currentDirectory);
      }
    }

    /// <summary>
    /// Ищет в text переменные вида {text-file:..\ftw.sql} c именем файла, если находит, 
    /// то загружает содержимое соответствующего файла в соответствующую позицию text
    /// </summary>
    /// <param name="currentPath"></param>
    /// <param name="text"></param>
    public static void TryLoadMappedFiles(String currentPath, ref String text) {
      var v_fileContent = RegexFind(text, "(?<={text-file:).+(?=})", true);
      TryLoadTextAsFile(currentPath, ref v_fileContent);
      RegexReplace(ref text, "{text-file:.+}", v_fileContent, true);
    }

    /// <summary>
    /// Отрисовывает на канве выбранную ячейку
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

      Pen v_pen;
      //left  
      if ((borders & AnchorStyles.Left) == AnchorStyles.Left) {
        v_pen = new Pen(col1, 2);
        gra.DrawLine(v_pen, new Point(cellBounds.X, cellBounds.Y), new Point(cellBounds.X, cellBounds.Y + cellBounds.Height));
      }
      //right
      if ((borders & AnchorStyles.Right) == AnchorStyles.Right) {
        v_pen = new Pen(col2, 2);
        gra.DrawLine(v_pen, new Point(cellBounds.X + cellBounds.Width, cellBounds.Y), new Point(cellBounds.X + cellBounds.Width, cellBounds.Y + cellBounds.Height));
      }
      //top
      if ((borders & AnchorStyles.Top) == AnchorStyles.Top) {
        v_pen = new Pen(col1, 2);
        gra.DrawLine(v_pen, new Point(cellBounds.X, cellBounds.Y), new Point(cellBounds.X + cellBounds.Width, cellBounds.Y));
      }
      //bottom
      if ((borders & AnchorStyles.Bottom) == AnchorStyles.Bottom) {
        v_pen = new Pen(col2, 2);
        gra.DrawLine(v_pen, new Point(cellBounds.X, cellBounds.Y + cellBounds.Height), new Point(cellBounds.X + cellBounds.Width, cellBounds.Y + cellBounds.Height));
      }
    }
#endif

#if !SILVERLIGHT
    private static void drawSeldCell(DataGridView grid, DataGridViewCellPaintingEventArgs a, Boolean focused) {
      if((a.State & DataGridViewElementStates.Selected) ==
                DataGridViewElementStates.Selected) {
        var v_cellSelection = (grid.SelectionMode == DataGridViewSelectionMode.CellSelect) ||
                                  (grid.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect) ||
                                   (grid.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect);
        var rct = new Rectangle(a.CellBounds.Location, a.CellBounds.Size);
        rct.X = v_cellSelection || (a.ColumnIndex == 0) ? rct.X + 1 : rct.X;
        rct.Y = rct.Y + 1;
        rct.Width = v_cellSelection || (a.ColumnIndex == grid.Columns.Count - 1) ? rct.Width - 3 : rct.Width;
        rct.Height = rct.Height - 3;

        var v_borders = AnchorStyles.Bottom | AnchorStyles.Top;
        if (v_cellSelection || (a.ColumnIndex == 0))
          v_borders = v_borders | AnchorStyles.Left;
        if (v_cellSelection || (a.ColumnIndex == grid.Columns.Count - 1))
          v_borders = v_borders | AnchorStyles.Right;

        DrawAnSelctedCell(rct, focused, a.Graphics, v_borders);

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
      var v_parts = DataGridViewPaintParts.All;
      v_parts &= ~DataGridViewPaintParts.SelectionBackground;
      v_parts &= ~DataGridViewPaintParts.Focus;
      a.Paint(a.CellBounds, v_parts);
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

      var v_nodeBounds = a.Node.Bounds;
      v_nodeBounds.Width += 10;
      a.Graphics.FillRectangle(Brushes.White, v_nodeBounds);
      DrawAnSelctedCell(v_nodeBounds, focused, a.Graphics, AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom);

      var v_nodeFont = a.Node.TreeView.Font;
      RectangleF v_nodeBoundsF = Rectangle.Inflate(v_nodeBounds, 0, 0);
      a.Graphics.DrawString(a.Node.Text, v_nodeFont, Brushes.Black, v_nodeBoundsF);
      a.DrawDefault = false;
    }
#endif

#if !SILVERLIGHT
    private static RegistryValueKind getRegistryType(Type type) {
      if (type != null) {
        if (type == typeof(Int16) || type == typeof(Int32) || type == typeof(Double) || type == typeof(Decimal))
          return RegistryValueKind.DWord;
        if (type == typeof(Int64))
          return RegistryValueKind.QWord;
        if (type == typeof(String))
          return RegistryValueKind.String;
      }
      return RegistryValueKind.Unknown;
    }
#endif


#if !SILVERLIGHT
    /// <summary>
    /// Записывает значение реестр
    /// </summary>
    /// <param name="regKeyName"></param>
    /// <param name="valName">Имя параметра.</param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public static void RegistryCUSetValue(String regKeyName, String valName, Object value, Type type) {
      if (regKeyName == null) throw new ArgumentNullException("regKeyName");
      if(type == null)
        type = (value != null) ? value.GetType() : null;
      var val = value;
      var knd = getRegistryType(type);
      if (knd == RegistryValueKind.Unknown) {
        if (type != null) {
          var tc = TypeDescriptor.GetConverter(type);
          if (tc.CanConvertTo(typeof(String))) {
            val = tc.ConvertToString(val);
            knd = getRegistryType(typeof(String));
          } else
            return;
        } else
          return;
      }

      if (val == null)
        val = (knd == RegistryValueKind.String) ? (Object)String.Empty : 0;
      var key = Registry.CurrentUser.CreateSubKey(regKeyName);
      if (key != null) key.SetValue(valName, val, knd);
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
      var key = Registry.CurrentUser.CreateSubKey(regKeyName);
      if (key != null) 
        return key.GetValue(valName, defVal) ?? defVal;
      return null;
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
    public static String BuidQueryStrParams(Dictionary<String, String> prms) {
      String rslt = null;
      foreach (var k in prms.Keys) {
        var v_paramStr = k + "=" + HttpUtility.UrlEncode(prms[k]);
        AppendStr(ref rslt, v_paramStr, "&");
      }
      return rslt;
    }

    /// <summary>
    /// Создает строку, которую можно подставить в URL
    /// </summary>
    /// <param name="baseURL"></param>
    /// <param name="prms"></param>
    /// <returns></returns>
    public static String BuidQueryStrParams(String baseURL, Dictionary<String, String> prms) {
      var rslt = BuidQueryStrParams(prms);
      return (baseURL.IndexOf('?') >= 0) ? baseURL + "&" + rslt : baseURL + "?" + rslt;
    }

    /// <summary>
    /// Возвращает true если параметер TRUE|T|1|Y иначе false
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static Boolean ParsBoolean(String val) {
      if (String.IsNullOrEmpty(val))
        return false;
      val = val.ToUpper();
      return val.Equals("TRUE") || val.Equals("T") || val.Equals("1") || val.Equals("Y");
    }

    /// <summary>
    /// Вытаскивает из ораклового сообщения об ошибке текст в случае если это ошибка создана через
    /// raise_application_error(2xxxx, 'message');, тогда вернет "message" иначе null.
    /// </summary>
    /// <returns></returns>
    public static void ExtractOracleApplicationError(String exMessage, out Int32 errCode, out String errMsg) {
      errCode = 0;
      errMsg = null;
      if (exMessage != null) {
        var v_strtIndx = exMessage.IndexOf("ORA-2", StringComparison.Ordinal);
        if (v_strtIndx >= 0) {
          var v_endIndx = exMessage.IndexOf("ORA-", v_strtIndx + 5, StringComparison.Ordinal);
          var v_len = v_endIndx - v_strtIndx;
          if (v_len < 0)
            v_len = exMessage.Length - v_strtIndx;
          var v_msg = exMessage;
          if (v_len > 0)
            v_msg = exMessage.Substring(v_strtIndx, v_len);
          errCode = Int32.Parse(v_msg.Substring(4, 5));
          errMsg = v_msg.Substring(11);
        }
      }
    }

    /// <summary>
    /// Достраивает путь к папке до абсолютного и нормализует
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static String ResolvePath(String path) {
      return NormalizeDir(String.IsNullOrEmpty(path) ? Directory.GetCurrentDirectory() : Path.GetFullPath(path));
    }

#if !SILVERLIGHT
    /// <summary>
    /// Записывает сообщение в лог-файл
    /// </summary>
    /// <param name="logFileName"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static Boolean WriteMessageLog(String logFileName, String msg) {
      try {
        var fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        var v_encode = DefaultEncoding;
        var v_point = DateTime.Now;
        using (var sw = new StreamWriter(fs, v_encode)) {
          var v_line = String.Format("[{0} {1}] - {2}", v_point.ToShortDateString(), v_point.ToLongTimeString(), msg);
          sw.WriteLine(v_line);
          sw.Flush();
          sw.Close();
        }
        fs.Close();
        return true;
      } catch (Exception) {
        return false;
      }
    }


    /// <summary>
    /// Строит сообщение об ошибке
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="dateTimePoint"></param>
    /// <returns></returns>
    public static String BuildErrorLogMsg(Exception ex, DateTime dateTimePoint) {
      var sb = new StringBuilder();
      sb.AppendLine("^^-ERROR-BEGIN---------------------------------------------------------------------------^^");
      sb.AppendLine("Source        : " + ex.Source.Trim());
      sb.AppendLine("Method        : " + ex.TargetSite.Name);
      sb.AppendLine("Date          : " + dateTimePoint.ToShortDateString());
      sb.AppendLine("Time          : " + dateTimePoint.ToLongTimeString());
      sb.AppendLine("Computer      : " + Dns.GetHostName());
      sb.AppendLine("Error         : " + ex.Message.Trim());
      sb.AppendLine("Stack Trace   : " + ex.StackTrace.Trim());
      sb.AppendLine("^^-ERROR-END-----------------------------------------------------------------------------^^");
      return sb.ToString();
    }

    /// <summary>
    /// Записывает в лог-файл ошибку
    /// </summary>
    /// <param name="logFileName"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Boolean WriteErrorLog(String logFileName, Exception ex) {
      try {
        var fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        var v_encoding = DefaultEncoding;
        var v_point = DateTime.Now;
        using (var sw = new StreamWriter(fs, v_encoding)) {
          var v_line = String.Format("[{0} {1}] - {2}", v_point.ToShortDateString(), v_point.ToLongTimeString(), "Ошибка!!!");
          sw.WriteLine(v_line);
          sw.WriteLine(BuildErrorLogMsg(ex, v_point));
          sw.Flush();
          sw.Close();
        }
        fs.Close();
        return true;
      } catch (Exception) {
        return false;
      }
    }

    /// <summary>
    /// Возвращает имя компьютера, на котором запущена программа
    /// </summary>
    /// <returns></returns>
    public static String GetLocalHost(){
      return Dns.GetHostName();
    }

    /// <summary>
    /// Возвращает ip компьютера, на котором запущена программа
    /// </summary>
    /// <returns></returns>
    public static String GetLocalIP() {
      var addrss = Dns.GetHostAddresses(Dns.GetHostName());
      String v_localeIP = null;
      foreach (var a in addrss) {
        if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
          v_localeIP = a.ToString();
          break;
        }
      }
      return v_localeIP;
    }

    /// <summary>
    /// Определяет тип SQL-команды
    /// </summary>
    /// <param name="sqlText"></param>
    /// <returns></returns>
    public static CommandType DetectCommandType(String sqlText) {
      var v_hasSelect = RegexMatch(sqlText, @"\bSELECT\b", true).Success;
      v_hasSelect = v_hasSelect && RegexMatch(sqlText, @"\bFROM\b", true).Success;
      return v_hasSelect ? CommandType.Text : CommandType.StoredProcedure;
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
    /// Сохраняет значение в Cookie
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
    /// Вытаскивает значение из Cookie
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
    /// Проверяет является
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

    /// <summary>
    /// Возвращает значение свойства объекта
    /// </summary>
    /// <param name="obj">Экземпляр объекта</param>
    /// <param name="propertyName">Имя запрашиваемого свойства</param>
    /// <returns></returns>
    public static Object GetPropertyValue(Object obj, String propertyName) {
      return GetPropertyValue<Object>(obj, propertyName);
    }

    /// <summary>
    /// Возвращает значение свойства объекта
    /// </summary>
    /// <param name="obj">Экземпляр объекта</param>
    /// <param name="propertyName">Имя запрашиваемого свойства</param>
    /// <typeparam name="T">Тип значения, которой ожидается получить на выходе</typeparam>
    /// <returns></returns>
    public static T GetPropertyValue<T>(Object obj, String propertyName) {
      if (obj != null) {
        var v_propertyRnum = GetPropertyInfo(obj.GetType(), propertyName, false);
        if (v_propertyRnum != null) {
          var v_val = v_propertyRnum.GetValue(obj, null);
          return Convert2Type<T>(v_val);
        }
        return default(T);
      }
      return default(T);
    }

    /// <summary>
    /// Устанавливает свойство для объекта
    /// </summary>
    /// <param name="obj">Экземпляр объекта</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <param name="value">Значение</param>
    public static void SetPropertyValue(Object obj, String propertyName, Object value) {
      if (obj != null) {
        var v_propertyRnum = GetPropertyInfo(obj.GetType(), propertyName, false);
        if (v_propertyRnum != null)
          v_propertyRnum.SetValue(obj, value, null);
      }
    }

    /// <summary>
    /// Вычисляет интервал, до текущего момента
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    public static TimeSpan Duration(DateTime from) {
      return @from < (DateTime.Now.AddDays(-7)) ? TimeSpan.MinValue : DateTime.Now.Subtract(@from);
    }

    /// <summary>
    /// Возвращает интервал в формате HH:MM:SS
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static String FormatDuration(TimeSpan duration) {
      return String.Format("{0:00}:{1:00}:{2:00}", duration.Hours + (duration.Days * 24), duration.Minutes, duration.Seconds);
    }

    /// <summary>
    /// Возвращает номер дня недели, где 1-пон...7-вос
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Int32 DayOfWeekRu(DateTime date) {
      var v_days = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
      var v_dayStr = date.ToString("ddd", new CultureInfo("ru-RU"));
      var v_dayInt = Array.IndexOf(v_days, v_dayStr);
      return v_dayInt + 1;
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

    /// <summary>
    /// Вытаскивает из списка первый элемент
    /// </summary>
    /// <param name="path"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static String PopFirstItemFromList(ref String path, String delimeter) {
      if (!String.IsNullOrEmpty(path)) {
        var v_nodes = SplitString(path, delimeter);
        if (v_nodes.Length > 0) {
          var v_nodesNew = v_nodes.Where((v, i) => i > 0).ToArray();
          path = CombineString(v_nodesNew, delimeter);
        } else
          path = null;
        return (v_nodes.Length > 0) ? v_nodes[0] : null;
      }
      return null;
    }

    /// <summary>
    /// Вытаскивает из списка последний элемент
    /// </summary>
    /// <param name="path"></param>
    /// <param name="delimeter"></param>
    /// <returns></returns>
    public static String PopLastItemFromList(ref String path, String delimeter) {
      if (!String.IsNullOrEmpty(path)) {
        var v_nodes = SplitString(path, delimeter);
        if (v_nodes.Length > 0) {
          var v_nodesNew = v_nodes.Where((v, i) => i < v_nodes.Length - 1).ToArray();
          path = CombineString(v_nodesNew, delimeter);
        } else
          path = null;
        return (v_nodes.Length > 0) ? v_nodes[v_nodes.Length - 1] : null;
      }
      return null;
    }

    /// <summary>
    /// Объединяет два массива
    /// </summary>
    /// <param name="array1">Массив 1</param>
    /// <param name="array2">Массив 2</param>
    /// <typeparam name="T">Тип элементов в массиве</typeparam>
    /// <returns></returns>
    public static T[] combineArrays<T>(T[] array1, T[] array2) {
      var v_array1OriginalLength = array1.Length;
      Array.Resize(ref array1, v_array1OriginalLength + array2.Length);
      Array.Copy(array2, 0, array1, v_array1OriginalLength, array2.Length);
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