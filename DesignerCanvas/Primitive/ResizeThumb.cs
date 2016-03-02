using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Primitive
{
    public class ResizeThumb : Thumb
    {
        private SizeAdorner sizeAdorner;

        public ResizeThumb()
        {
            DragDelta += ResizeThumb_DragDelta;
            DragStarted += ResizeThumb_DragStarted;
            DragCompleted += ResizeThumb_DragCompleted;
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            //var canvas = DesignerCanvas.FindDesignerCanvas(this);
            var destControl = DataContext as FrameworkElement;
            if (destControl != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    Debug.Assert(sizeAdorner == null);
                    sizeAdorner = new SizeAdorner(destControl);
                    adornerLayer.Add(sizeAdorner);
                }
            }
        }

        private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sizeAdorner != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                adornerLayer?.Remove(sizeAdorner);
                sizeAdorner = null;
            }
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var destControl = DataContext as DependencyObject;
            if (destControl == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
            // only resize DesignerItems
            CalculateDragLimits(designer.SelectedItems, out minLeft, out minTop,
                out minDeltaHorizontal, out minDeltaVertical);
            foreach (var item in designer.SelectedItems)
            {
                Debug.Assert(item != null);
                double dragDeltaVertical;
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                        item.Height = item.Height - dragDeltaVertical;
                        break;
                    case VerticalAlignment.Top:
                        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                        item.Top += dragDeltaVertical;
                        item.Height = item.Height - dragDeltaVertical;
                        break;
                }
                double dragDeltaHorizontal;
                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                        item.Left += dragDeltaHorizontal;
                        item.Width = item.Width - dragDeltaHorizontal;
                        break;
                    case HorizontalAlignment.Right:
                        dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                        item.Width = item.Width - dragDeltaHorizontal;
                        break;
                }
            }
            e.Handled = true;
        }

        private static void CalculateDragLimits(IEnumerable<GraphicalObject> items, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (var item in items)
            {
                var left = item.Left;
                var top = item.Top;
                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                minDeltaVertical = Math.Min(minDeltaVertical, item.Height - 10);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.Width - 10);
            }
        }
    }
}
