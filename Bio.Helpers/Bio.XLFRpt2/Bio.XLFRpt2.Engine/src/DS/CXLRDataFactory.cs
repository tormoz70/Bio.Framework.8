using Bio.Helpers.Common;

namespace Bio.Helpers.XLFRpt2.Engine {

  using System;
  using System.Data;
  using System.Xml;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Threading;
  using Bio.Helpers.Common.Types;
  using System.Reflection;
  using Bio.Helpers.Common;
  //using Bio.Helpers.DOA;

  public class CXLRDataRow : Dictionary<String, Object> {
    public CXLRDataRow() : base() { }
    public CXLRDataRow(IDictionary<String, Object> data):base() {
      foreach (KeyValuePair<String, Object> p in data) {
        this.Add(p.Key.ToUpper(), p.Value);
      }
    }
    public new Object this[String fieldName] {
      get {
        if (!String.IsNullOrEmpty(fieldName)) {
          if (this.ContainsKey(fieldName.ToUpper())) {
            return base[fieldName.ToUpper()];
          } else
            return null;
        } else
          return null;
      }
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public delegate void CXLRDTblFactoryOnOpenEveHandler(CXLRDataFactory sender, IDictionary<String, FieldType> cols);
  public delegate void CXLRDTblFactoryOnFetchEveHandler(CXLRDataFactory sender, IList row);
  public delegate void CXLRDTblFactoryOnEOFEveHandler(CXLRDataFactory sender);
  public abstract class CXLRDataFactory : DisposableObject {
    /// <summary>
    /// Происходит сразу после открытия
    /// </summary>
#pragma warning disable 0067
    public event CXLRDTblFactoryOnOpenEveHandler OnOpen;
#pragma warning restore 0067
    /// <summary>
    /// После каждого Next
    /// </summary>
#pragma warning disable 0067
    public event CXLRDTblFactoryOnFetchEveHandler OnFetch;
#pragma warning restore 0067
    /// <summary>
    /// По достижении конца
    /// </summary>
#pragma warning disable 0067
    public event CXLRDTblFactoryOnEOFEveHandler OnEOF;
#pragma warning restore 0067

    public abstract IDbConnection openDbConnection(CXLReportConfig cfg);



    /// <summary>
    /// Реализует инициализацию курсора
    /// </summary>
    /// <returns>Метаданные курсора</returns>
    protected abstract IDictionary<String, FieldType> open(IDbConnection conn, CXLReportDSConfig dsCfg, Int32 timeout);
    /// <summary>
    /// Реализует передвижение на следующую запись курсора
    /// </summary>
    /// <returns>Текущая запись. Если EOF, тогда null</returns>
    protected abstract IList next();

    public virtual Object GetScalarValue(IDbConnection conn, String cmd, Params prms, Int32 timeout) {
      return cmd;
    }

    public abstract IDbCommand PrepareCmd(IDbConnection conn, String cmd, Params prms, Int32 timeout);
    public abstract void ExecCmd(IDbCommand cmd);

    private IDictionary<String, FieldType> FColDefs = null;
    private IList FCurRow = null;
    /// <summary>
    /// Открывает курсор, инициализирует и переходит в начало.
    /// </summary>
    public void Open(IDbConnection conn, CXLReportDSConfig dsCfg, Int32 timeout) {
      //this._conn = conn;
      var v_dsCfg = dsCfg;
      if (String.IsNullOrEmpty(v_dsCfg.sql))
        throw new EBioException("SQL-предложение не определено!");
      this.FColDefs = this.open(conn, v_dsCfg, timeout);
    }
    /// <summary>
    /// считать следующую запись
    /// </summary>
    /// <returns>Если false, тогда это EOF</returns>
    public Boolean Next() {
      this.FCurRow = this.next();
      return this.FCurRow != null;
    }
    
    /// <summary>
    /// Пробежать до конца
    /// </summary>
    public void GoToEOF() {
      while (this.Next()) { }
    }
    public IList Values {
      get {
        return this.FCurRow;
      }
    }

    private String[] getFldNames() {
      String[] keys = new String[this.FColDefs.Keys.Count];
      this.FColDefs.Keys.CopyTo(keys, 0);
      return keys;
    }

    private int indexOfField(String fieldName) {
      String[] keys = this.getFldNames();
      for (int i = 0; i < keys.Length; i++)
        if (String.Equals(keys[i], fieldName, StringComparison.CurrentCultureIgnoreCase))
          return i;
      return -1;
    }

    public Object ValueByName(String fieldName) {
      int colIndex = indexOfField(fieldName);
      if (colIndex >= 0) {
        return this.FCurRow[colIndex];
      } else
        return null;
    }

    public IList CurentRow {
      get {
        return this.FCurRow;
      }
    }
    public CXLRDataRow CurentRowExt {
      get {
        CXLRDataRow row = new CXLRDataRow();
        String[] keys = this.getFldNames();
        foreach (String fn in keys) {
          row.Add(fn, this.ValueByName(fn));
        }
        return row;
      }
    }

    public int ColCount {
      get {
        return this.FColDefs.Keys.Count;
      }
    }
    public String ColName(int index) {
      String[] keys = this.getFldNames();
      return keys[index];
    }

    //private Assembly loadAssembly(String pAssemblyName) {
    //  if (pAssemblyName == null)
    //    return null;
    //  Assembly vAssembly;
    //  try {
    //    vAssembly = Assembly.Load(pAssemblyName);
    //  } catch (Exception ex) {
    //    throw new EBioException("Ошибка загрузки модуля обработки функции \"" + pAssemblyName + "\". Сообщение: " + ex.ToString(), ex);
    //  }
    //  return vAssembly;
    //}

    //private static Type findType(Assembly pAssembly, String pTypeName) {
    //  return pAssembly.GetType(pTypeName, false, false);
    //}

    private static CXLRDataFactory _createDataFactory(Type vType) {
      if (vType != null) {
        Type[] parTypes = new Type[] { };
        Object[] parVals = new Object[] { };
        ConstructorInfo ci = vType.GetConstructor(parTypes);
        CXLRDataFactory obj = (CXLRDataFactory)ci.Invoke(parVals);
        return obj;
      } else
        return null;
    }

    public static CXLRDataFactory createDataFactory(CXLReportConfig cfg) {
      CXLRDataFactory vRslt = null;
      // Тип должен быть описан в виде "<Полное имя типа>, <Имя сборки(без расширения)>, <Версия сборки>", Например: "Bio.Module.CPlugin11, Bio.Module.Plugin11, Version=1.0.0.0"
      Type vType = Type.GetType(cfg.dataFactoryTypeName);
      if (vType != null) {
        vRslt = _createDataFactory(vType);
      } else
        throw new EBioException("Ошибка при инициализации типа " + cfg.dataFactoryTypeName + ". Сообщение: Тип не найден в доступных библиотеках.");
      return vRslt;
    }

  }
}
