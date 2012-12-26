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
using System.Collections.Generic;
using System.Linq;

namespace Bio.Helpers.Controls.SL {
  public class MenuTriggerElement {
    public PopupMenu PopupMenu { get; set; }

    public TriggerTypes TriggerType { get; set; }

    public FrameworkElement TriggerElement { get; set; }

    public bool IsTriggerAssigned { get; set; }

    //public string MenuName { get { return PopupMenu.Name; } }
    //public string triggerName { get { return TriggerElement.Name; } }
  }

  public static class PopupMenuManager {
    public static bool KeyboardCaptureOn;

    public static List<MenuTriggerElement> ApplicationMenus = new List<MenuTriggerElement>();

    public static List<PopupMenu> OpenMenus = new List<PopupMenu>();

    public static RoutedEventHandler ItemClicked { get; set; }

    public static MouseEventArgs CapturedMouseEventArgs { get; set; }

    /// <summary>
    /// The neighbouring left click element being hovered, if any.
    /// This is only used after a left click menu has already been fired.
    /// </summary>
    public static FrameworkElement NeighbouringLeftClickElementUnderMouse;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="triggerElement"></param>
    /// <param name="triggerType"></param>
    /// <param name="targetMenu"></param>
    /// <returns>Если менюшка создана заново, то возвращает true, если обновлена существующая, то false</returns>
    public static void RegisterMenu(FrameworkElement triggerElement, TriggerTypes triggerType, PopupMenu targetMenu) {
      UnregisterMenu(triggerElement);
      var menuTriggerElement = new MenuTriggerElement {
        PopupMenu = targetMenu,
        TriggerType = triggerType,
        TriggerElement = triggerElement
      };
      if (targetMenu.AccessKeyElement == null)
        ApplicationMenus.Add(menuTriggerElement);
      else
        ApplicationMenus.Insert(0, menuTriggerElement);
    }

    public static void UnregisterMenu(FrameworkElement triggerElement) {
      var menuTriggerElement = ApplicationMenus.Where((e) => {
        return e.TriggerElement == triggerElement;
      }).FirstOrDefault();
      if (menuTriggerElement != null) {
        menuTriggerElement.PopupMenu.UnassignTriggerElement(triggerElement);
        ApplicationMenus.Remove(menuTriggerElement);
      }
    }

    public static void AppRoot_KeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Escape && OpenMenus.Count > 0) {
        CloseHangingMenus(OpenMenus.First().CloseDuration, false, null);
      } else if (KeyboardCaptureOn) {
        foreach (PopupMenu menu in ApplicationMenus.Select(m => m.PopupMenu)) {
          if (menu.AccessKeyElement != null
            && PopupMenuUtils.IsKeyPressed(menu.AccessKey, menu.AccessKeyModifier1, menu.AccessKeyModifier2, e)) {
            e.Handled = true;

            if (menu.AccesskeyPressed != null)
              menu.AccesskeyPressed(menu, e);

            if (menu.OpenOnAccessKeyPressed)
              menu.OpenNextTo(menu.AccessKeyElement, menu.Orientation, true, true);
            else if (menu.AccessKeyElement is Control)
              (menu.AccessKeyElement as Control).Focus();

            break;
          }
        }
      }
    }

    public static void AppRoot_KeyUp(object sender, KeyEventArgs e) {
      if (PopupMenuManager.OpenMenus.Count > 0 && KeyboardCaptureOn) {
        if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
          if (e.Key != OpenMenus.First().SubmenuLaunchKey)
            CloseTopMenu(e);
      }
    }

    public static void CloseTopMenu(KeyEventArgs e) {
      if (OpenMenus.Count > 0) {
        e.Handled = true;
        OpenMenus.First().Close(0);
      }
    }

    /// <summary>
    /// Closes all open menus except for the one being hovered(or whose trigger element is being hovered) and its parents.
    /// </summary>
    /// <param name="closeDuration">The closing animation duration.</param>
    /// <param name="closeOnlyHoverMenus">Close hover menus as only.</param>
    /// <param name="e">The MouseEventArgs value used to get the mouse position. A null value can be used to close all the menus.</param>
    public static void CloseHangingMenus(int closeDuration, bool closeOnlyHoverMenus, MouseEventArgs e) {
      foreach (PopupMenu menu in OpenMenus.ToList()) {
        if ((e == null || !PopupMenuUtils.HitTestAny(e, true, false, true, menu.ContentRoot, menu.ActualTriggerElement)) && !menu.IsPinned) {
          if (!menu.IsModal && (menu.ActualTriggerType == TriggerTypes.Hover || !closeOnlyHoverMenus))
            menu.Close(closeDuration);
        } else {
          break;
        }
      }
    }

  }
}
