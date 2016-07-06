using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    public class ResizeThumb : Thumb
    {
        private SizeAdorner sizeAdorner;

        private Controls.DesignerCanvas parentCanvas;

        public ResizeThumb()
        {
            DragDelta += ResizeThumb_DragDelta;
            DragStarted += ResizeThumb_DragStarted;
            DragCompleted += ResizeThumb_DragCompleted;
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            parentCanvas = DesignerCanvas.FindDesignerCanvas(this);
            var destControl = DataContext as IBoxCanvasItem;
            if (parentCanvas != null && destControl != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    Debug.Assert(sizeAdorner == null);
                    sizeAdorner = new SizeAdorner(destControl);
                    parentCanvas.AddAdorner(sizeAdorner);
                }
            }
        }

        private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sizeAdorner != null)
            {
                ((Canvas) sizeAdorner.Parent).Children.Remove(sizeAdorner);
                sizeAdorner = null;
            }
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (parentCanvas == null) return;
            var z = parentCanvas.Zoom/100.0;
            var hc = e.HorizontalChange/z;
            var vc = e.VerticalChange/z;
            var mod = Keyboard.Modifiers;
            double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
            // only resize DesignerItems
            CalculateDragLimits(parentCanvas.SelectedItems.OfType<IBoxCanvasItem>(), out minLeft, out minTop,
                out minDeltaHorizontal, out minDeltaVertical);
            var isResizingWidthAndHeight = HorizontalAlignment != HorizontalAlignment.Stretch &&
                                           VerticalAlignment != VerticalAlignment.Stretch;
            foreach (var item in parentCanvas.SelectedItems.OfType<IBoxCanvasItem>())
            {
                Debug.Assert(item != null);
                double ratio = double.NaN; // Width / Height
                if (isResizingWidthAndHeight && (mod & ModifierKeys.Shift) == ModifierKeys.Shift)
                    ratio = item.Width/item.Height;
                double dragDeltaVertical;
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        dragDeltaVertical = Math.Min(-vc, minDeltaVertical);
                        item.Height = item.Height - dragDeltaVertical;
                        break;
                    case VerticalAlignment.Top:
                        dragDeltaVertical = Math.Min(Math.Max(-minTop, vc), minDeltaVertical);
                        item.Top += dragDeltaVertical;
                        item.Height = item.Height - dragDeltaVertical;
                        break;
                }
                if (double.IsNaN(ratio))
                {
                    double dragDeltaHorizontal;
                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, hc), minDeltaHorizontal);
                            item.Left += dragDeltaHorizontal;
                            item.Width -= dragDeltaHorizontal;
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-hc, minDeltaHorizontal);
                            item.Width -= dragDeltaHorizontal;
                            break;
                    }
                }
                else
                {
                    var delta = item.Height*ratio - item.Width;
                    item.Width += delta;
                    if (HorizontalAlignment == HorizontalAlignment.Left)
                        item.Left -= delta;
                }
            }
            e.Handled = true;
        }

        private static void CalculateDragLimits(IEnumerable<IBoxCanvasItem> items, out double minLeft, out double minTop,
            out double minDeltaHorizontal, out double minDeltaVertical)
        {
            const double DefaultMinSize = 10;

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
                var sc = item as ISizeConstraint;
                var minWidth = Math.Max(sc?.MinWidth ?? DefaultMinSize, 0);
                var minHeight = Math.Max(sc?.MinHeight ?? DefaultMinSize, 0);
                minDeltaVertical = Math.Min(minDeltaVertical, item.Height - minHeight);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.Width - minWidth);
            }
        }
    }
}
