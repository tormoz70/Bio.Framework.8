namespace Bio.Helpers.Common.Types {
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Xml;

  public class EBioDOA:EBioException {
    public EBioDOA()
      : base() {
    }
    public EBioDOA(String pMsg)
      : base(pMsg) {
    }
    public EBioDOA(Exception pInnerExeption)
      : base(pInnerExeption.Message, pInnerExeption) {
    }
  }

  public class EBioDOATooMuchRows:EBioDOA {
    public EBioDOATooMuchRows()
      : base() {
    }
    public EBioDOATooMuchRows(String pMsg)
      : base(pMsg) {
    }
  }

  public class EBioSQLBreaked:EBioDOA {
    public EBioSQLBreaked()
      : base() {
    }
    public EBioSQLBreaked(String pMsg)
      : base(pMsg) {
    }
    public EBioSQLBreaked(Exception pInnerExeption)
      : base(pInnerExeption) {
    }
  }
  public class EBioSQLTimeout : EBioDOA {
    public EBioSQLTimeout()
      : base() {
    }
    public EBioSQLTimeout(String pMsg)
      : base(pMsg) {
    }
    public EBioSQLTimeout(Exception pInnerExeption)
      : base(pInnerExeption) {
        this.ApplicationErrorMessage = "Время ожидания запроса истекло!";
    }
  }

}
