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
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace Bio.Helpers.Controls.SL {
  public class CImgButton : ContentControl {
    private static DependencyObject findRootParent(FrameworkElement obj) {
      if ((obj.Parent != null) && (obj.Parent is FrameworkElement))
        return findRootParent(obj.Parent as FrameworkElement);
      else
        return obj;
    }
    public CImgButton() {
      this.DefaultStyleKey = typeof(CImgButton);
      this.MouseEnter += new MouseEventHandler(CImgButton1_MouseEnter);
      this.MouseLeave += new MouseEventHandler(CImgButton1_MouseLeave);
      this.Loaded += new RoutedEventHandler(CImgButton1_Loaded);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(CImgButton1_MouseLeftButtonDown);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(CImgButton1_MouseLeftButtonUp);
    }

    void CImgButton1_Loaded(object sender, RoutedEventArgs e) {
      FrameworkElement vRootParent = findRootParent(this) as FrameworkElement;
      if (vRootParent != null) {
        vRootParent.LostFocus += new RoutedEventHandler(CImgButton1_LostFocus);
        vRootParent.GotFocus += new RoutedEventHandler(CImgButton1_GotFocus);
      }
    }

    void CImgButton1_GotFocus(object sender, RoutedEventArgs e) {
      //this._mouseOver = true;
      //this.SetState();
    }

    void CImgButton1_LostFocus(object sender, RoutedEventArgs e) {
      this._mousePressed = false;
      this._mouseOver = false;
      this.SetState();
    }

    bool _mouseOver = false;
    bool _mousePressed = false;

    private void SetState() {
      if (_mousePressed) {
        VisualStateManager.GoToState(this, "MousePressed", true);
        //System.Diagnostics.Debug.WriteLine("GoToState -> MouseNone!");
      } else if (_mouseOver) {
        VisualStateManager.GoToState(this, "MouseOver", true);
        //System.Diagnostics.Debug.WriteLine("GoToState -> MouseOver!");
      } else {
        VisualStateManager.GoToState(this, "MouseNone", true);
        //System.Diagnostics.Debug.WriteLine("GoToState -> MouseNone!");
      }
    }

    void CImgButton1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      //if (this._mousePressed) {
      //  this.OnClick(e);
      //}
      //System.Diagnostics.Debug.WriteLine("MouseLeftButtonUp!");
      this._mousePressed = false;
      this.SetState();
    }

    void CImgButton1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      //System.Diagnostics.Debug.WriteLine("MouseLeftButtonDown!");
      this._mousePressed = true;
      this.SetState();
    }

    void CImgButton1_MouseLeave(object sender, MouseEventArgs e) {
      this._mousePressed = false;
      this._mouseOver = false;
      this.SetState();
    }

    void CImgButton1_MouseEnter(object sender, MouseEventArgs e) {
      this._mouseOver = true;
      this.SetState();
    }

    public static DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(String), typeof(CImgButton), new PropertyMetadata("ImgButton"));
    public String Caption {
      get { return (String)this.GetValue(CaptionProperty); }
      set { this.SetValue(CaptionProperty, value); }
    }

    public String ImgSource { get; set; }
    public override void OnApplyTemplate() {
      //base.OnApplyTemplate();
      Image vImage = this.GetTemplateChild("image") as Image;
      if (vImage != null) {
        if (String.IsNullOrEmpty(this.ImgSource))
          this.ImgSource = "/Bio.Framework.Client.SL.Controls;component/Images/home_24.png";
        StreamResourceInfo sr = Application.GetResourceStream(new Uri(this.ImgSource, UriKind.Relative));
        BitmapImage image = new BitmapImage();
        image.SetSource(sr.Stream);
        vImage.Source = image;
      }
    }

  }
}
