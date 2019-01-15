//////////////////////////////////////////////
// Apache 2.0  - 2018
// Author : Janus Tida
// Modified by : Derek Tremblay
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfHexaEditor
{
    public static class ToolTipExtension
    {
        public static UIElement GetOperatableToolTip(DependencyObject obj) => 
            (UIElement) obj.GetValue(OperatableToolTipProperty);

        public static void SetOperatableToolTip(DependencyObject obj, FrameworkElement value) => 
            obj.SetValue(OperatableToolTipProperty, value);

        public static void SetToolTipOpen(this FrameworkElement elem, bool open, Point? point = null)
        {
            var toolPopup = GetToolTipPopup(elem);
            if (toolPopup == null)
                return;

            if (point != null)
            {
                toolPopup.VerticalOffset = point.Value.Y;
                toolPopup.HorizontalOffset = point.Value.X;
            }

            toolPopup.IsOpen = open;
        }

        // Using a DependencyProperty as the backing store for OperatableTool.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OperatableToolTipProperty =
            DependencyProperty.RegisterAttached("OperatableToolTip", typeof(FrameworkElement),
                typeof(ToolTipExtension), new PropertyMetadata(null, OperatableToolProperty_Changed));

        private static void OperatableToolProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is UIElement newElem)) return;
            if (!(d is FrameworkElement elem)) return;

            
            var toolPop = new Popup
            {
                Child = newElem,
                PopupAnimation = PopupAnimation.Fade,
                PlacementTarget = elem,
                Placement = PlacementMode.Relative
            };

            toolPop.MouseLeave += ToolPop_MouseLeave;

            elem.MouseDown += FrameworkElem_MouseDown;
            elem.MouseUp += FrameworkElem_MouseUp;
            elem.MouseEnter += FrameworkElem_MouseEnter;
            elem.MouseLeave += FrameworkElem_MouseLeave;

            toolPop.SetBinding(FrameworkElement.DataContextProperty,
                new Binding(nameof(FrameworkElement.DataContext))
                {
                    Source = elem
                }
            );

            SetToolTipPopup(elem, toolPop);
        }

        private static void ToolPop_MouseLeave(object sender, MouseEventArgs e) {
            if (!(sender is Popup pop)) return;

            if ((pop.Child as FrameworkElement)?.ContextMenu?.IsOpen ?? false) return;

            pop.IsOpen = false;
        }

        private static void FrameworkElem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement elem)) return;

            var toolPopup = GetToolTipPopup(elem);
            if (toolPopup == null)
                return;

            if (GetAutoHide(elem))
                SetToolTipOpen(elem, false);
        }

        private static void FrameworkElem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (!(sender is FrameworkElement elem)) return;

            //if (!_toolTipDics.ContainsKey(elem)) return;

            //var pop = _toolTipDics[elem];
            //_toolTipDics[elem] = pop;
        }

        private static void FrameworkElem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement elem)) return;

            var toolPopup = GetToolTipPopup(elem);
            if (toolPopup == null)
                return;
            
            
            if (toolPopup.IsMouseOver)
                return;

            if (GetAutoHide(elem))
                SetToolTipOpen(elem, false);

            
        }

        private static void FrameworkElem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement elem)) return;

            var toolPopup = GetToolTipPopup(elem);
            if (toolPopup == null)
                return;
            
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                return;

            try
            {
                var position = Mouse.GetPosition(elem);
                toolPopup.VerticalOffset = position.Y;
                toolPopup.HorizontalOffset = position.X;

                if (GetAutoShow(elem))
                    SetToolTipOpen(elem, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        

        //This dp show the popup while the mouse entering the targetElem if set to true;
        public static bool GetAutoShow(DependencyObject obj) => 
            (bool) obj.GetValue(AutoShowProperty);

        public static void SetAutoShow(DependencyObject obj, bool value) => obj.SetValue(AutoShowProperty, value);

        // Using a DependencyProperty as the backing store for AutoShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoShowProperty =
            DependencyProperty.RegisterAttached("AutoShow", typeof(bool), typeof(ToolTipExtension),
                new PropertyMetadata(true));


        //This dp hide the popup while the mouse leaving the targetElem if set to true;
        public static bool GetAutoHide(DependencyObject obj) => 
            (bool) obj.GetValue(AutoHideProperty);

        public static void SetAutoHide(DependencyObject obj, bool value) => 
            obj.SetValue(AutoHideProperty, value);

        // Using a DependencyProperty as the backing store for AutoHide.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoHideProperty =
            DependencyProperty.RegisterAttached("AutoHide", typeof(bool), typeof(ToolTipExtension),
                new PropertyMetadata(true));



        private static Popup GetToolTipPopup(DependencyObject obj) {
            return (Popup)obj.GetValue(ToolTipPopupProperty);
        }

        private static void SetToolTipPopup(DependencyObject obj, Popup value) {
            obj.SetValue(ToolTipPopupProperty, value);
        }

        // Using a DependencyProperty as the backing store for ToolTipPopup.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty ToolTipPopupProperty =
            DependencyProperty.RegisterAttached("ToolTipPopup", typeof(Popup), typeof(ToolTipExtension), new PropertyMetadata(null));


    }

    //Simplified tooltip that loaded in visual tree.
    //Plz make sure the targetElem and tooltip are in the same container;
    //public class OperatableToolTip : Popup {
    //    public OperatableToolTip() {
    //        this.HorizontalAlignment = HorizontalAlignment.Left;
    //        this.VerticalAlignment = VerticalAlignment.Top;

    //    }


    //    //public static readonly DependencyProperty PlacementTargetProperty =
    //    //        DependencyProperty.Register(
    //    //                nameof(PlacementTarget),
    //    //                typeof(FrameworkElement),
    //    //                typeof(OperatableToolTip),
    //    //                new FrameworkPropertyMetadata(
    //    //                    null,
    //    //                    OnPlacementTargetChanged));

    //    ///// <summary>
    //    ///// The FrameworkElement relative to which the Popup will be displayed. If PlacementTarget is null (which
    //    ///// it is by default), the Popup is displayed relative to its visual parent.
    //    ///// </summary>
    //    //[Bindable(true), Category("Layout")]
    //    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //    //public FrameworkElement PlacementTarget {
    //    //    get { return (FrameworkElement)GetValue(PlacementTargetProperty); }
    //    //    set { SetValue(PlacementTargetProperty, value); }
    //    //}

    //    //private static void OnPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    //    //    if (!(d is OperatableToolTip tTip)) return;

    //    //    if (e.OldValue is FrameworkElement oldElem) {
    //    //        oldElem.MouseDown  -= tTip.Target_MouseDown;
    //    //        oldElem.MouseLeave -= tTip.Target_MouseLeave;
    //    //        oldElem.MouseMove  -= tTip.Target_MouseMove;
    //    //        oldElem.MouseUp    -= tTip.Target_MouseUp;
    //    //        oldElem.MouseEnter -= tTip.Target_MouseEnter;
    //    //    }

    //    //    if (e.NewValue is FrameworkElement newElem) {
    //    //        newElem.MouseDown  += tTip.Target_MouseDown;
    //    //        newElem.MouseLeave += tTip.Target_MouseLeave;
    //    //        newElem.MouseMove  += tTip.Target_MouseMove;
    //    //        newElem.MouseUp    += tTip.Target_MouseUp;
    //    //        newElem.MouseEnter += tTip.Target_MouseEnter;
    //    //    }

    //    //}

    //    private bool _targetPressing;
    //    public bool TargetPressing => _targetPressing;

    //    private void Target_MouseDown(object d, MouseButtonEventArgs e) {
    //        _targetPressing = true;
    //        this.Visibility = Visibility.Hidden;
    //    }

    //    private void Target_MouseMove(object d, MouseEventArgs e) {

    //        if(PlacementTarget != null) {
    //            var position = Mouse.GetPosition(PlacementTarget);
    //            this.Margin = new Thickness(position.X, position.Y, 0, 0);
    //            this.Visibility = Visibility.Visible;
    //        }

    //    }

    //    private void Target_MouseEnter(object d, MouseEventArgs e) {
    //        if (e.LeftButton != MouseButtonState.Pressed) {
    //            _targetPressing = false;
    //        }
    //    }

    //    private void Target_MouseLeave(object d, MouseEventArgs e) {
    //        if (!this.IsMouseOver) {
    //            _targetPressing = false;
    //            this.Visibility = Visibility.Hidden;
    //        }
    //    }

    //    private void Target_MouseUp(object d, MouseButtonEventArgs e) {
    //        _targetPressing = false;
    //    }
    //}

}
