using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Bio.Helpers.Controls.SL;
using Bio.Helpers.Common.Extentions;

namespace Bio.Framework.Client.SL {
  public partial class CPluginViewDialog : FloatableWindow {
    public CPluginViewDialog()
      : base() {
      InitializeComponent();
    }

    private List<Button> _dlgButtons = new List<Button>();
    public List<Button> dlgButtons { get { return this._dlgButtons; } }

    public void AddButtons(params PluginViewDialogButton[] buttons) {
      foreach (var b in buttons) {
        var newBtn = new Button() {
          Name = b.Name,
          Content = b.Caption,
          IsEnabled = b.IsDisabled == false,
          Style = (Style)this.Resources["DialogBtnStyle"]
        };
        newBtn.Click += b.OnClick;
        this.buttonsPanel.Children.Add(newBtn);
        this._dlgButtons.Add(newBtn);
      }
    }

    public void ChangeButton(PluginViewDialogButton button) {
      if (button != null) {
        var vbtn = this.buttonsPanel.Children.Where(b => {
          return (b is Button) && String.Equals((b as Button).Name, button.Name);
        }).FirstOrDefault() as Button;
        if (vbtn != null) {
          if(button.Caption != null)
            vbtn.Content = button.Caption;
          if (button.IsDisabled != null)
            vbtn.IsEnabled = button.IsDisabled == false;
        }
      }
    }

    private CPluginViewBase _viewContent = null;
    public CPluginViewDialog(CPluginViewBase viewContent)
      : base() {
      InitializeComponent();
      this._viewContent = viewContent;
      this.Title = this._viewContent.ownerPlugin.ViewTitle;
      this.OnShow += CPluginViewDialog_OnShow;
    }

    public override void ShowDialog() {
      base.ShowDialog();
    }

    void CPluginViewDialog_OnShow(object sender, EventArgs e) {
      //this.Width = this._viewContent.Width;
      //this.Height = this._viewContent.Height;
      //this.VerticalAlignment = VerticalAlignment.Stretch;
      //this.HorizontalAlignment = HorizontalAlignment.Stretch;
      //this.VerticalContentAlignment = VerticalAlignment.Stretch;
      //this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
      this._viewContent.Show(this.contentControlPart);
    }



  }

  public class PluginViewDialogButtons {

    public static PluginViewDialogButton OKButton {
      get {
        return new PluginViewDialogButton {
          Name = "OkButton",
          Caption = "OK",
          IsDisabled = false,
          OnClick = new RoutedEventHandler((s, a) => {
            var root = (s as Button).GetRoot<CPluginViewDialog>((node) => { return node is CPluginViewDialog; });
            root.DialogResult = true;
          })
        };
      }
    }
    public static PluginViewDialogButton CancelButton {
      get {
        return new PluginViewDialogButton {
          Name = "CancelButton",
          Caption = "Отмена",
          IsDisabled = false,
          OnClick = new RoutedEventHandler((s, a) => {
            var root = (s as Button).GetRoot<CPluginViewDialog>((node) => { return node is CPluginViewDialog; });
            root.DialogResult = false;
          })
        };
      }
    }
    public static PluginViewDialogButton CloseButton {
      get {
        return new PluginViewDialogButton {
          Name = "CloseButton",
          Caption = "Закрыть",
          OnClick = new RoutedEventHandler((s, a) => {
            var root = (s as Button).GetRoot<CPluginViewDialog>((node) => { return node is CPluginViewDialog; });
            root.DialogResult = false;
          })
        };
      }
    }
  }

}
