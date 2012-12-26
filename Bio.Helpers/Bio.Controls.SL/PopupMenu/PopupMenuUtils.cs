// Copyright (c) 2009 Ziad Jeeroburkhan. All Rights Reserved.
// GNU General Public License version 2 (GPLv2)
// (http://sl4popupmenu.codeplex.com/license)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Reflection;

namespace Bio.Helpers.Controls.SL {
  public class PopupMenuUtils {
    public static StackPanel GenerateStackPanelWithUnderlinedText(string text, char underliningChar) {
      var sp = new StackPanel { Orientation = Orientation.Horizontal };
      var headerParts = text.Split('^');
      sp.Tag = headerParts[1].Substring(0, 1);
      sp.Children.Add(new TextBlock { Text = headerParts[0] });
      sp.Children.Add(new TextBlock { Text = sp.Tag.ToString(), TextDecorations = TextDecorations.Underline });
      sp.Children.Add(new TextBlock { Text = headerParts[1].Substring(1) });
      return sp;
    }

    public static bool IsKeyPressed(Key key, ModifierKeys keyModifier1, ModifierKeys keyModifier2, KeyEventArgs e) {
      if (e.Key == key)
        if (keyModifier1 == ModifierKeys.None || (Keyboard.Modifiers & keyModifier1) == keyModifier1)
          if (keyModifier2 == ModifierKeys.None || (Keyboard.Modifiers & keyModifier2) == keyModifier2)
            return true;
      return false;
    }

    public static string GetShortcutKeyDisplayText(Key key, ModifierKeys keyModifier1, ModifierKeys keyModifier2) {
      string text = "";

      if (keyModifier1 != ModifierKeys.None)
        text += keyModifier1.ToString() + "+";

      if (keyModifier2 != ModifierKeys.None)
        text += keyModifier2.ToString() + "+";

      return (text + key.ToString())
        .Replace(ModifierKeys.Control.ToString(), "Ctrl");
      //.Replace(ModifierKeys.Windows.ToString(), "Win");
    }

    /// <summary>
    /// Get the first parent of Type T for the specified element. A special case is included
    /// so as to also identify item containers if the child element is an ItemsControl.
    /// </summary>
    /// <typeparam name="T">The type of the parent element</typeparam>
    /// <param name="item">The element to start from</param>
    /// <returns>The parent element</returns>
    public static T GetContainer<T>(FrameworkElement item) where T : class {
      T container = item.GetVisualAncestors().OfType<T>().FirstOrDefault();

      if (container == null) {
        FrameworkElement elem = item;
        while (!(elem is T) && elem != null && elem.Parent != null) {
          var containerFromItem = elem.Parent is ItemsControl
            ? (elem.Parent as ItemsControl).ItemContainerGenerator.ContainerFromItem(elem)
            : null;

          elem = (containerFromItem ?? elem.Parent) as FrameworkElement;
        }

        if (elem != null)
          container = elem as T;
      }
      return container;
    }

    public static void SetPosition(FrameworkElement element, Point newPosition, bool keepElementWithinLayoutBounds) {
      if (keepElementWithinLayoutBounds) {
        if (newPosition.X + element.ActualWidth > Application.Current.Host.Content.ActualWidth)
          newPosition.X = Application.Current.Host.Content.ActualWidth - element.ActualWidth;

        if (newPosition.Y + element.ActualHeight > Application.Current.Host.Content.ActualHeight)
          newPosition.Y = Application.Current.Host.Content.ActualHeight - element.ActualHeight;

        if (newPosition.X < 0)
          newPosition.X = 0;

        if (newPosition.Y < 0)
          newPosition.Y = 0;
      }
      //element.MaxHeight = Application.Current.MainWindow.Height;
      //element.MaxWidth = Application.Current.MainWindow.Width;
      Canvas.SetLeft(element, newPosition.X);
      Canvas.SetTop(element, newPosition.Y);
      //element.Margin = new Thickness(coordinates.X, coordinates.Y, 0, 0);
    }

    /// <summary>
    /// Make a FrameworkElement the child of a Panel control.
    /// </summary>
    /// <param name="parentGrid">The parent control</param>
    /// <param name="childElement">The child element</param>
    /// <param name="addDefaultShadowEffect">When true the default shadow effect is also added to the child element</param>
    public static void RelocateElement(Panel parentGrid, FrameworkElement childElement, bool addDefaultShadowEffect) {
      if (parentGrid == childElement.Parent)
        return;

      RemoveParentReference(childElement);
      parentGrid.Children.Clear();
      parentGrid.Children.Add(childElement);

      // Add the default shadow effect if the child element doesn't have any
      if (childElement.Effect == null && addDefaultShadowEffect)
        childElement.Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 4, Opacity = 0.5, ShadowDepth = 2 };
    }

    public static void RemoveParentReference(FrameworkElement childElement) {
      if (childElement.Parent != null) {
        if (childElement.Parent is ItemsControl) // If the control lies inside an ItemsControl
          (childElement.Parent as ItemsControl).Items.Remove(childElement); // dissociate it the ItemsControl.
        else if (childElement.Parent is ContentControl) // If the control lies inside a ContentControl or the current PopupMenu content
          (childElement.Parent as ContentControl).Content = null; // dissociate it from the current ContentControl or PopupMenu content.
        else if (childElement.Parent is Panel) // If the control lies inside a Panel
          (childElement.Parent as Panel).Children.Remove(childElement); // dissociate it from the Panel.
        else
          throw new Exception("The content element must be placed in a container that inherits from the Panel or ContentControl class. "
                    + "The actual parent type is " + childElement.Parent.GetType());
      }
    }

    public static FrameworkElement FindApplicationElementByName(string elementName, string elementQualifierForErrorMsg) {
      object obj = (Application.Current.RootVisual as FrameworkElement).FindName(elementName.Trim());
      if (obj == null) // Object not found. Use the more thorough but also more costly method.
        obj = Application.Current.RootVisual.GetVisualDescendants()
          .OfType<FrameworkElement>()
          .Where(elem => elem.Name == elementName).FirstOrDefault();

      if (obj != null) {
        if (obj is UIElement) {
          return obj as FrameworkElement;
        } else {
          if (elementQualifierForErrorMsg != null && !DesignerProperties.IsInDesignTool) // Error messages are disabled at design time
            throw new ArgumentException("The " + elementQualifierForErrorMsg + " is referenced to " + elementName + " which is not a UIElement.");
        }
      } else {
        if (elementQualifierForErrorMsg != null && !DesignerProperties.IsInDesignTool) // Error messages are disabled at design time
          throw new ArgumentException("Could not find any element named " + elementName + " for " + elementQualifierForErrorMsg + " in the visual tree.");
      }
      return null;
    }

    /// <summary>
    /// Determine if the mouse is over any of the FrameworkElements specified.
    /// </summary>
    /// <param name="e">The mouse-related arguments</param>
    /// <param name="isPopupChild">Set the value to true if the elements specified are within a Popup control
    /// to get around the fact that the zoom factor for Popup children is not reflected at all in the coordinate system.</param>
    /// <param name="autoMapHoverBoundsToParentOfTextBlocks">
    /// Switch to the parent for bounds info if the element under the mouse is a Textblock to avoid dealing with widths
    /// that can change with the text length.</param>
    /// <param name="valueToReturnOnNullElements">The value to return when the element to HitTest is null</param>
    /// <param name="elements">The FrameworkElements being HitTested.</param>
    public static bool HitTestAny(MouseEventArgs e, bool isPopupChild, bool autoMapHoverBoundsToParentOfTextBlocks, bool valueToReturnOnNullElements, params FrameworkElement[] elements) {
      if (elements.Contains(null))
        return valueToReturnOnNullElements;
      else
        return GetFirstElementUnderMouse(e, isPopupChild, autoMapHoverBoundsToParentOfTextBlocks, elements) != null;
    }

    /// <summary>
    /// Determine if the mouse is over any of the FrameworkElements specified.
    /// </summary>
    /// <param name="e">The mouse-related arguments</param>
    /// <param name="isPopupChild">Set the value to true if the elements specified are within a Popup control
    /// to get around the fact that the zoom factor for Popup children is not reflected at all in the coordinate system.</param>
    /// <param name="autoMapHoverBoundsToParentOfTextBlocks">
    /// Switch to the parent for bounds info if the element under the mouse is a Textblock to avoid dealing with widths
    /// that can change with the text length.</param>
    /// <param name="elements">The FrameworkElements being HitTested.</param>
    public static FrameworkElement GetFirstElementUnderMouse(MouseEventArgs e, bool isPopupChild, bool autoMapHoverBoundsToParentOfTextBlocks, params FrameworkElement[] elements) {
      foreach (FrameworkElement elem in elements) {
        // Textblocks have a variable width, depending on the text length, when none is specified.
        bool isVariableWidthTextBlock = autoMapHoverBoundsToParentOfTextBlocks
                       && elem is TextBlock
                       && double.IsNaN((elem as TextBlock).Width)
                       && double.IsInfinity((elem as TextBlock).MaxWidth);
        // In this case the parent element is used for hit testing to avoid limiting the hover region to the TextBlock whose size varies with its content.
        if (HitTest(e, isPopupChild, isVariableWidthTextBlock ? elem.Parent as FrameworkElement : elem))
          return elem;
      }
      return null;
    }

    /// <summary>
    /// Get the item under the mouse
    /// </summary>
    /// <param name="visualTreeElementsUnderMouse">All the elements in the visual tree hierarchy where the mouse was clicked or hovered.</param>
    /// <param name="senderElement">The containing element</param>
    /// <param name="returnSelectableItemIfAny">Return the clicked or hovered item inside the trigger element if the latter is a DataGrid, a ListBox or a TreeView</param>
    /// <param name="selectItemIfSelectable">Select the item if it lies in a ListBox, Datagrid or TreeView</param>
    /// <returns>The item under the mouse</returns>
    public static FrameworkElement GetItemUnderMouse(IEnumerable<UIElement> visualTreeElementsUnderMouse, FrameworkElement senderElement, bool returnSelectableItemIfAny, bool selectItemIfSelectable) {
      FrameworkElement elem = visualTreeElementsUnderMouse.OfType<ListBoxItem>().FirstOrDefault();
      if (senderElement is ListBox || elem != null) {
        if (senderElement is ListBox && elem == null) // element is a ListBox with no selected item
          return null;
        if (selectItemIfSelectable)
          (elem as ListBoxItem).IsSelected = true;
        if (returnSelectableItemIfAny)
          senderElement = elem;
      } else if (senderElement is DataGrid) {
        if ((elem = visualTreeElementsUnderMouse.OfType<DataGridRow>().FirstOrDefault()) == null) // no DataGridRow was clicked upon
          return null;
        if (selectItemIfSelectable)
          (senderElement as DataGrid).SelectedIndex = (elem as DataGridRow).GetIndex();
        if (returnSelectableItemIfAny)
          senderElement = elem;
      } else if (senderElement is TreeView) {
        if ((elem = visualTreeElementsUnderMouse.OfType<TreeViewItem>().FirstOrDefault()) == null) // no TreeViewItem was clicked upon
          return null;
        if (selectItemIfSelectable)
          (senderElement as TreeView).GetContainerFromItem(elem).IsSelected = true;
        if (returnSelectableItemIfAny)
          senderElement = elem;
      }
      //else
      //{
      //    foreach(var element in visualTreeElementsUnderMouse.OfType<FrameworkElement>())
      //    {
      //        var isSelectedProperty = element.GetType().GetProperty("IsSelected");
      //        if (isSelectedProperty != null)
      //        {
      //            if (selectItemIfSelectable)
      //                isSelectedProperty.SetValue(element, true, null);
      //            if (returnSelectableItemIfAny)
      //                senderElement = element;
      //        }
      //    }
      //}
      return senderElement;
    }

    /// <summary>
    /// Determine if the mouse is over any of the FrameworkElements specified
    /// </summary>
    /// <param name="e">The mouse-related arguments</param>
    /// <param name="isPopupChild">Set the value to true if the elements specified are within a Popup control
    /// to get around the fact that the zoom factor for Popup children is not reflected at all in the coordinate system.</param>
    /// <param name="elements">The FrameworkElements being HitTested.</param>
    public static bool HitTestAny(MouseEventArgs e, bool isPopupChild, params FrameworkElement[] elements) {
      foreach (var element in elements)
        if (HitTest(e, isPopupChild, element))
          return true;
      return false;
    }

    /// <summary>
    /// Determine if the mouse is over a FrameworkElement
    /// </summary>
    /// <param name="e">The mouse-related arguments</param>
    /// <param name="isPopupChild">Set the value to true if the elements specified are within a Popup control.
    /// This will compensate for the fact that the Silverlight coordinate system for elements inside a Popup control does not take into account the zoom factor.</param>
    /// <param name="element">The FrameworkElement being HitTested.</param>
    public static bool HitTest(MouseEventArgs e, bool isPopupChild, FrameworkElement element) {
      Rect? rect = element.GetBoundsRelativeTo(Application.Current.RootVisual);
      if (!rect.HasValue)
        return false;

      Rect box = rect.Value;
      Point pt = e.GetSafePosition(null);
      if (isPopupChild)
        pt = MapToZoomValue(pt);
      // Determine if the mouse lies within the element bounds
      return (pt.X > box.Left && pt.X < box.Right
         && pt.Y > box.Top && pt.Y < box.Bottom);
    }

    public static Point GetAbsoluteElementPos(bool isPopupChild, FrameworkElement element) {

      Point pt = new Point();
      try { pt = element.TransformToVisual(null).Transform(new Point()); } catch { }
      if (isPopupChild)
        pt = MapToZoomValue(pt);
      return pt;
      //Rect rect = element.GetBoundsRelativeTo(Application.Current.RootVisual).Value;
      //return new Point(rect.Left, rect.Top);
    }

    //public static Point GetAbsoluteMousePos(MouseEventArgs e)
    //{
    //    // This will not work for MouseLeave events
    //    Point pt = e.GetSafePosition(null);// Application.Current.RootVisual.TransformToVisual(null).Transform(e.GetPosition(Application.Current.RootVisual));
    //    return pt;
    //}

    //public static Point GetAbsoluteMousePos(MouseEventArgs e, FrameworkElement element)
    //{
    //    Point pt = element.TransformToVisual(null).Transform(e.GetPosition(element));
    //    return MapToZoomLevel(pt);
    //}

    private static Point MapToZoomValue(Point pt) {
      double zoomFactor = Application.Current.Host.Content.ZoomFactor;
      pt.X /= zoomFactor;
      pt.Y /= zoomFactor;
      return pt;
    }

    public static LinearGradientBrush MakeColorGradient(Color startColor, Color endColor, double angle) {
      GradientStopCollection gradientStopCollection = new GradientStopCollection();
      gradientStopCollection.Add(new GradientStop { Color = startColor, Offset = 0 });
      gradientStopCollection.Add(new GradientStop { Color = endColor, Offset = 1 });
      LinearGradientBrush brush = new LinearGradientBrush(gradientStopCollection, angle);
      return brush;
    }

    //public static Storyboard CreateStoryBoard(int beginTime, int duration, FrameworkElement element, string targetProperty, double? from, double? to)
    //{
    //    return CreateStoryBoard(beginTime, duration, element, targetProperty, from, to, false);
    //}

    //public static Storyboard CreateStoryBoard(int beginTime, int duration, FrameworkElement element, string targetProperty, double? from, double? to, bool beginNow)
    //{
    //    DoubleAnimation da = new DoubleAnimation
    //    {
    //        From = from,
    //        To = to,
    //        Duration = new TimeSpan(0, 0, 0, 0, duration < 0 ? 0 : duration)
    //    };

    //    if (element != null)
    //        Storyboard.SetTarget(da, element);
    //    Storyboard.SetTargetProperty(da, new PropertyPath(targetProperty));

    //    Storyboard sb = new Storyboard();
    //    sb.Children.Add(da);
    //    sb.BeginTime = new TimeSpan(0, 0, 0, 0, beginTime);
    //    if (beginNow)
    //        sb.Begin();
    //    RegisterStoryBoardTarget(sb, element);
    //    return sb;
    //}

    public static Storyboard CreateStoryBoard(int beginTime, int duration, FrameworkElement element, string targetProperty, double value, EasingFunctionBase easingFunction) {
      return CreateStoryBoard(beginTime, duration, element, targetProperty, value, easingFunction, false);
    }

    public static Storyboard CreateStoryBoard(int beginTime, int duration, FrameworkElement element, string targetProperty, double value, EasingFunctionBase easingFunction, bool beginNow) {
      EasingDoubleKeyFrame edkf = new EasingDoubleKeyFrame {
        KeyTime = new TimeSpan(0, 0, 0, 0, duration < 0 ? 0 : duration),
        Value = value,
        EasingFunction = easingFunction
      };

      DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
      da.KeyFrames.Add(edkf);

      if (element != null)
        Storyboard.SetTarget(da, element);

      if (targetProperty != null)
        Storyboard.SetTargetProperty(da, new PropertyPath(targetProperty));

      Storyboard sb = new Storyboard();
      sb.Children.Add(da);
      sb.BeginTime = new TimeSpan(0, 0, 0, 0, beginTime);

      if (beginNow)
        sb.Begin();

      RegisterStoryBoardTarget(sb, element);
      return sb;
    }

    public static void RegisterVisualStateGroupTargets(VisualStateGroup visualStateGroup, params FrameworkElement[] targetElements) {
      foreach (VisualState state in visualStateGroup.States)
        RegisterStoryBoardTargets(state.Storyboard, targetElements);
    }

    public static void RegisterStoryBoardTargets(Storyboard storyBoard, params FrameworkElement[] targetElements) {
      foreach (FrameworkElement targetElement in targetElements)
        RegisterStoryBoardTarget(storyBoard, targetElement);
    }

    public static void RegisterStoryBoardTarget(Storyboard storyBoard, FrameworkElement targetElement) {
      storyBoard.Stop();
      targetElement.Dispatcher.BeginInvoke(delegate {
        foreach (Timeline tl in storyBoard.Children.OfType<Timeline>()
            .Where(tl => Storyboard.GetTargetName(tl) == targetElement.Name))
          Storyboard.SetTarget(tl, targetElement);
      });
    }

    public static T GetStyleValue<T>(Style style, DependencyProperty dp) {
      Setter setter = GetStyleSetter(style, dp);
      return setter.Value == null ? default(T) : (T)setter.Value;
    }

    public static Setter GetStyleSetter(Style style, DependencyProperty dp) {
      return style.Setters.OfType<Setter>().Where(s => s.Property == dp).FirstOrDefault();
    }

    //private static bool HitTest(Point point, FrameworkElement element)
    //{
    //    List<UIElement> hits = System.Windows.Media.VisualTreeHelper.FindElementsInHostCoordinates(point, element) as List<UIElement>;
    //    return (hits.Contains(element));
    //}



    //        private static bool? _isInDesignMode;
    //        public static bool IsInDesignModeStatic
    //        {
    //            get
    //            {
    //                if (!_isInDesignMode.HasValue)
    //                {
    //#if DEBUG
    //                    _isInDesignMode = DesignerProperties.IsInDesignTool;
    //#else
    //                    _isInDesignMode = false;
    //#endif
    //                }
    //                return _isInDesignMode.Value;
    //            }
    //        }

    ///// <summary>
    ///// Provides a custom implementation of DesignerProperties.GetIsInDesignMode
    ///// to work around an issue.
    ///// </summary>
    //internal static class DesignerProperties
    //{
    //    /// <summary>
    //    /// Returns whether the control is in design mode (running under Blend
    //    /// or Visual Studio).
    //    /// </summary>
    //    /// <param name="element">The element from which the property value is
    //    /// read.</param>
    //    /// <returns>True if in design mode.</returns>
    //    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "element", Justification =
    //        "Matching declaration of System.ComponentModel.DesignerProperties.GetIsInDesignMode (which has a bug and is not reliable).")]
    //    public static bool GetIsInDesignMode(DependencyObject element)
    //    {
    //        if (!_isInDesignMode.HasValue)
    //        {
    //            _isInDesignMode =
    //                (null == Application.Current) ||
    //                Application.Current.GetType() == typeof(Application);
    //        }
    //        return _isInDesignMode.Value;
    //    }

    //    /// <summary>
    //    /// Stores the computed InDesignMode value.
    //    /// </summary>
    //    private static bool? _isInDesignMode;
    //}

    //public static void InvokeMultiDispatcher(List<UIElement> dispatcherElements, Action a)
    //{
    //    var elems = dispatcherElements.Where(el => el != null);
    //    if (elems.Count() > 1)
    //        elems.First().Dispatcher.BeginInvoke(delegate { InvokeMultiDispatcher(dispatcherElements.Skip(1).ToList(), a); });
    //    else
    //        elems.First().Dispatcher.BeginInvoke(a);
    //}
  }


  /*public static class ContextMenuService
  {
    public static PopupMenu GetContextMenu(DependencyObject obj)
    {
      return (PopupMenu)obj.GetValue(ContextMenuProperty);
    }
    public static void SetContextMenu(DependencyObject obj, PopupMenu value)
    {
      obj.SetValue(ContextMenuProperty, value);
    }

    public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.RegisterAttached(
      "ContextMenu",
      typeof(PopupMenu),
      typeof(ContextMenuService),
      new PropertyMetadata(null, OnContextMenuChanged));

    /// <summary>
    /// Handles changes to the ContextMenu DependencyProperty.
    /// </summary>
    /// <param name="o">DependencyObject that changed.</param>
    /// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
    private static void OnContextMenuChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement element = o as FrameworkElement;
      if (null != element)
      {
        PopupMenu oldContextMenu = e.OldValue as PopupMenu;
        if (null != oldContextMenu)
        {
          //oldContextMenu.LeftClickElements = null;
        }
        PopupMenu newContextMenu = e.NewValue as PopupMenu;
        if (null != newContextMenu)
        {
          newContextMenu.AddRightClickElements(element);
        }
      }
    }
  }*/
}