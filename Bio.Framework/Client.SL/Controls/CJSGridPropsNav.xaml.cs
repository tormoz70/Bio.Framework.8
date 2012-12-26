using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Bio.Framework.Client.SL;
using Bio.Helpers.Common.Types;
using Bio.Helpers.Common;
using Bio.Helpers.Controls.SL;
using System.Collections;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Bio.Framework.Client.SL {
  public partial class CJSGridPropsNav : UserControl {
    public CJSGridPropsNav() {
      InitializeComponent();
    }



    public CJSGridPropsNav(CJSGridConfig cfg)
      : this() {
        this.DataContext = cfg;
      //this.lbxList.ItemsSource = cfg.Items;
    }

  }

}

