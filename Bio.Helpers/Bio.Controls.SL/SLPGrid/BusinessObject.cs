using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using Bio.Helpers.Common.Types;

namespace Bio.Helpers.Controls.SL.SLPropertyGrid {
  public class Car : INotifyPropertyChanged {
    private string _Brand;
    private CarType _CarType;

    [Category("Car Info")]
    public string Brand {
      get { return _Brand; }
      set {
        if (_Brand == value) return;
        _Brand = value;
        OnPropertyChanged("Brand");
      }
    }

    [Category("Car Info")]
    public CarType Type {
      get { return _CarType; }
      set {
        if (_CarType == value) return;
        _CarType = value;
        OnPropertyChanged("CarType");
      }
    }

    public enum CarType {
      Sport,
      StationWagon,
      SUV
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
      if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }

  public class Person : INotifyPropertyChanged {
    #region Fields
    private double _Double;
    private decimal _Decimal;
    private int _Int;
    private short _Short;
    private long _Long;
    private uint _Uint;
    private ushort _Ushort;
    private ulong _Ulong;
    private string _String;
    private char _Char;
    private DateTime _DateTime;
    private TimeSpan _TimeSpan;
    private bool _Boolean;
    private Car _Car;
    private DayOfWeek _Enum;
    #endregion

    [Category("Numeric Float")]
    public double Double {
      get { return _Double; }
      set {
        if (_Double == value) return;
        _Double = value;
        OnPropertyChanged("Double");
      }
    }

    [Category("Numeric Float")]
    public decimal Decimal {
      get { return _Decimal; }
      set {
        if (_Decimal == value) return;
        _Decimal = value;
        OnPropertyChanged("Decimal");
      }
    }

    [Category("Numeric Signed")]
    public int Int {
      get { return _Int; }
      set {
        if (_Int == value) return;
        _Int = value;
        OnPropertyChanged("Int");
      }
    }

    [Category("Numeric Signed")]
    public short Short {
      get { return _Short; }
      set {
        if (_Short == value) return;
        _Short = value;
        OnPropertyChanged("Short");
      }
    }

    [Category("Numeric Signed")]
    public long Long {
      get { return _Long; }
      set {
        if (_Long == value) return;
        _Long = value;
        OnPropertyChanged("Long");
      }
    }

    [Category("Numeric Unsigned")]
    public uint Uint {
      get { return _Uint; }
      set {
        if (_Uint == value) return;
        _Uint = value;
        OnPropertyChanged("Uint");
      }
    }

    [Category("Numeric Unsigned")]
    public ushort Ushort {
      get { return _Ushort; }
      set {
        if (_Ushort == value) return;
        _Ushort = value;
        OnPropertyChanged("Ushort");
      }
    }

    [Category("Numeric Unsigned")]
    public ulong Ulong {
      get { return _Ulong; }
      set {
        if (_Ulong == value) return;
        _Ulong = value;
        OnPropertyChanged("Ulong");
      }
    }

    [Category("Read Only")]
    [HeaderContent("The answer to everything")]
    public double Answer {
      get { return 42; }
    }

    [Category("Read Only")]
    public DateTime Now {
      get { return DateTime.Now; }
    }

    [Category("Read Only")]
    public bool True {
      get { return true; }
    }

    [Category("Read Only")]
    public DayOfWeek Today {
      get { return DateTime.Now.DayOfWeek; }
    }

    [Category("Strings")]
    public string String {
      get { return _String; }
      set {
        if (_String == value) return;
        _String = value;
        OnPropertyChanged("String");
      }
    }

    [Category("Strings")]
    public char Char {
      get { return _Char; }
      set {
        if (_Char == value) return;
        _Char = value;
        OnPropertyChanged("Char");
      }
    }

    [Category("Date & Time")]
    public DateTime Datetime {
      get { return _DateTime; }
      set {
        if (_DateTime == value) return;
        _DateTime = value;
        OnPropertyChanged("DateTime");
      }
    }

    [Category("Date & Time")]
    public TimeSpan TimeSpan {
      get { return _TimeSpan; }
      set {
        if (_TimeSpan == value) return;
        _TimeSpan = value;
        OnPropertyChanged("TimeSpan");
      }
    }

    [Category("Others")]
    public bool Boolean {
      get { return _Boolean; }
      set {
        if (_Boolean == value) return;
        _Boolean = value;
        OnPropertyChanged("Boolean");
      }
    }

    [Category("Others")]
    public DayOfWeek Enum {
      get { return _Enum; }
      set {
        if (_Enum == value) return;
        _Enum = value;
        OnPropertyChanged("Enum");
      }
    }

    //[Category("Others")]
    public Car Car {
      get { return _Car; }
      set {
        if (_Car == value) return;
        _Car = value;
        OnPropertyChanged("Car");
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
      if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
