﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    public class RelativePositionPanel : Panel
    {
        public static readonly DependencyProperty RelativePositionProperty =
            DependencyProperty.RegisterAttached("RelativePosition", typeof (Point), typeof (RelativePositionPanel),
                new FrameworkPropertyMetadata(new Point(0, 0), OnRelativePositionChanged));

        public static Point GetRelativePosition(UIElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            return (Point)element.GetValue(RelativePositionProperty);
        }

        public static void SetRelativePosition(UIElement element, Point value)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            element.SetValue(RelativePositionProperty, value);
        }

        private static void OnRelativePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reference = d as UIElement;
            if (reference != null)
            {
                var parent = VisualTreeHelper.GetParent(reference) as RelativePositionPanel;
                if (parent != null)
                {
                    parent.InvalidateArrange();
                }
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement element in base.InternalChildren)
            {
                if (element != null)
                {
                    var relPosition = GetRelativePosition(element);
                    var x = (arrangeSize.Width - element.DesiredSize.Width) * relPosition.X;
                    var y = (arrangeSize.Height - element.DesiredSize.Height) * relPosition.Y;

                    if (double.IsNaN(x)) x = 0;
                    if (double.IsNaN(y)) y = 0;

                    element.Arrange(new Rect(new Point(x, y), element.DesiredSize));
                }
            }
            return arrangeSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            // SDK docu says about InternalChildren Property: 'Classes that are derived from Panel 
            // should use this property, instead of the Children property, for internal overrides 
            // such as MeasureCore and ArrangeCore.

            foreach (UIElement element in InternalChildren)
            {
                if (element != null)
                    element.Measure(size);
            }

            return base.MeasureOverride(availableSize);
        }
    }
}
