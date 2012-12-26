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

namespace Bio.Helpers.Common.Types {
  public class HeaderContentAttribute : Attribute {
    public String Text { get; private set; }
    public HeaderContentAttribute(String headerContent)
      : base() {
      this.Text = headerContent;
    }
  }
  public class HeaderHideAttribute : Attribute {
  }

  public sealed class RequiredAttribute : Attribute {
    public RequiredAttribute(Boolean isRequired) {
      this.IsRequired = isRequired;
    }
    public Boolean IsRequired { get; private set; }
  }

}
