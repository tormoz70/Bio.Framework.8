using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bio.Helpers.Common.Types {
  public abstract class VSelection {
    public const String csNotSeldText = "<не выбрано>";
    public const String csMultiSeldText = "<выбор сделан>";
    public String DisplayField { get; set; }
    public String ValueField { get; set; }
    public abstract Object Value { get; set; }
    protected String _display = null;
    public virtual String Display { 
      get {
        return this.IsEmpty() ? csNotSeldText : this._display;
      }
      set {
        this._display = value;
      }
    }
    public abstract Boolean IsEmpty();
    public T Cast<T>() where T : VSelection {
      return this as T;
    }
    public Boolean IsMultiSelection() {
      return this is VMultiSelection;
    }

    public Params filterParams { get; set; }
  }
}
