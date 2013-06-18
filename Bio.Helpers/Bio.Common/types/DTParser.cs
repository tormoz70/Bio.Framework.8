namespace Bio.Helpers.Common.Types {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Globalization;
  using System.Text.RegularExpressions;


  public class CDTFmt {
    public String fmt { get; set; }
    public String regex { get; set; }
  }
  /// <summary>
  /// Класс для преобразований из строки в дату.
  /// </summary>
  public class DateTimeParser {
    /// <summary>
    /// Единственный экземпляр класса.
    /// </summary>
    public static DateTimeParser Instance;

    static DateTimeParser() {
      Instance = new DateTimeParser();
      Instance.Fmts = new CDTFmt[] {
        new CDTFmt {
          fmt = "yyyyMMddHHmmss",
          regex  = "^[012]\\d{3}[01]\\d{1}[0123]\\d{1}[012]\\d{1}[012345]\\d{1}[012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "dd.MM.yyyy HH:mm:ss",
          regex  = "^[0123]\\d{1}[.][01]\\d{1}[.][012]\\d{3}\\s[012]\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "yyyy.MM.dd HH:mm:ss",
          regex  = "^[012]\\d{3}[.][01]\\d{1}[.][0123]\\d{1}\\s[012]\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "yyyy.MM.dd",
          regex  = "^[012]\\d{3}[.][01]\\d{1}[.][0123]\\d{1}$"
        },
        new CDTFmt {
          fmt = "dd.MM.yyyy",
          regex  = "^[0123]\\d{1}[.][01]\\d{1}[.][012]\\d{3}$"
        },
        new CDTFmt {
          fmt = "yyyyMMdd",
          regex  = "^[012]\\d{3}[01]\\d{1}[0123]\\d{1}$"
        },
        new CDTFmt {
          fmt = "yyyyMM",
          regex  = "^[012]\\d{3}[01]\\d{1}$"
        },
        new CDTFmt {
          fmt = "ddMMyyyy",
          regex  = "^[0123]\\d{1}[01]\\d{1}[012]\\d{3}$"
        },
        //2005-10-31T00:00:00
        new CDTFmt {
          fmt = "yyyy-MM-ddTHH:mm:ss",
          regex  = "^[012]\\d{3}[-][01]\\d{1}[-][0123]\\d{1}[T][012]\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        },
        //2005-10-31 00:00:00
        new CDTFmt {
          fmt = "yyyy-MM-dd HH:mm:ss",
          regex  = "^[012]\\d{3}[-][01]\\d{1}[-][0123]\\d{1}\\s[012]\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "dd.MM.yyyy H:mm:ss",
          regex  = "^[0123]\\d{1}[.][01]\\d{1}[.][012]\\d{3}\\s[012]?\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "yyyy.MM.dd HH:mm",
          regex  = "^[012]\\d{3}[.][01]\\d{1}[.][0123]\\d{1}\\s[012]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "yyyyMMdd HH:mm:ss",
          regex  = "^[012]\\d{3}[01]\\d{1}[0123]\\d{1}\\s[012]\\d{1}[:][012345]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "yyyyMMdd HH:mm",
          regex  = "^[012]\\d{3}[01]\\d{1}[0123]\\d{1}\\s[012]\\d{1}[:][012345]\\d{1}$"
        },
        new CDTFmt {
          fmt = "dd.MM.yyyy H:mm",
          regex  = "^[0123]\\d{1}[.][01]\\d{1}[.][012]\\d{3}\\s[012]?\\d{1}[:][012345]\\d{1}$"
        }
      };
    }

    protected DateTimeParser() { }

    public CDTFmt[] Fmts { get; set; }

    /// <summary>
    /// Подбирает нужный формат даты по значению даты
    /// </summary>
    /// <param name="pDTValue">Дата в виде строки</param>
    /// <returns>Строка фрмата</returns>
    public string DetectDateTimeFmt(String pDTValue) {
      Regex vr;
      foreach (CDTFmt f in this.Fmts) {
        vr = new Regex(f.regex);
        if (vr.IsMatch(pDTValue))
          return f.fmt;
      }
      return String.Empty;
    }

    public DateTime ParsDateTime(String pValue) {
      String vDTFmt = DetectDateTimeFmt(pValue);
      if (String.IsNullOrEmpty(vDTFmt))
        throw new Exception("Не верная дата: [" + pValue + "]. Невозможно определить формат даты.");
      return ParsDateTime(pValue, vDTFmt);
    }

    public DateTime ParsDateTime(String pValue, String pFormat) {
      if (pValue != null) {
        if (pValue.ToUpper().Equals("NOW"))
          return DateTime.Now;
        if (pValue.ToUpper().Equals("MAX"))
          return DateTime.MaxValue;
        if (pValue.ToUpper().Equals("MIN"))
          return DateTime.MinValue;
#if !SILVERLIGHT
        System.IFormatProvider formatProvider = new System.Globalization.CultureInfo(CultureInfo.CurrentCulture.Name, true);
#else
        System.IFormatProvider formatProvider = new System.Globalization.CultureInfo(CultureInfo.CurrentCulture.Name);
#endif
        DateTime vRslt = new DateTime(1900, 01, 01);
        try {
          vRslt = DateTime.ParseExact(pValue, pFormat, formatProvider);
        } catch (Exception ex) {
          throw new Exception("Ошибка разбора даты. DateTime.ParseExact(" + pValue + ", " + pFormat + "). Сообщение: " + ex.ToString());
        }
        return vRslt;
      }
      return DateTime.MinValue;
    }
  }
}
