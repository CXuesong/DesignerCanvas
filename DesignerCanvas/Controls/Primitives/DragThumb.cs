using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    /// <summary>
    /// Used for drag &amp; moving objects on the canvas.
    /// </summary>
    public class DragThumb : Thumb
    {
        /// <summary>
        /// If the count of selected items exceeds this value,
        /// all the selected object except this one will be moved
        /// only when user releases the thumb.
        /// </summary>
        public const int InstantPreviewItemsThreshold = 200;

        public DragThumb()
        {
            DragStarted += DragThumb_DragStarted;
            DragDelta += DragThumb_DragDelta;
            DragCompleted += DragThumb_DragCompleted;
        }

        private bool instantPreview;

        private void DragThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var destControl = DataContext as DependencyObject;
            if (destControl == null) return;
            var designer = Controls.DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            instantPreview = designer.SelectedItems.Count < InstantPreviewItemsThreshold;
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var destControl = DataContext as FrameworkElement;
            if (destControl == null) return;
            var designer = Controls.DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            var minLeft = double.MaxValue;
            var minTop = double.MaxValue;
            //var tf = TransformToAncestor(designer);
            var delta = new Point(e.HorizontalChange, e.VerticalChange);
            var tf = destControl.RenderTransform;
            if (tf != null) delta = tf.Transform(delta);
            foreach (var item in designer.SelectedItems.OfType<ICanvasItem>())
            {
                minLeft = Math.Min(item.Left, minLeft);
                minTop = Math.Min(item.Top, minTop);
            }
            var deltaX = Math.Max(-minLeft, delta.X);
            var deltaY = Math.Max(-minTop, delta.Y);
            if (instantPreview)
            {
                // This operation may be slow.
                foreach (var item in designer.SelectedItems.OfType<ICanvasItem>())
                {
                    item.Left += deltaX;
                    item.Top += deltaY;
                }
            }
            else
            {
                var thisItem = (ICanvasItem)designer.ItemContainerGenerator.ItemFromContainer(destControl);
                thisItem.Left += deltaX;
                thisItem.Top += deltaY;
            }
            e.Handled = true;
        }

        private void DragThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var destControl = DataContext as FrameworkElement;
            if (destControl == null) return;
            var designer = Controls.DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            if (!instantPreview)
            {
                var tf = TransformToAncestor((Visual)Parent);
                var delta = new Point(e.HorizontalChange, e.VerticalChange);
                delta = tf.Transform(delta);
                var thisItem = designer.ItemContainerGenerator.ItemFromContainer(destControl);
                var minLeft = double.MaxValue;
                var minTop = double.MaxValue;
                foreach (var item in designer.SelectedItems.OfType<ICanvasItem>())
                {
                    var left = item.Left;
                    var top = item.Top;
                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }
                var deltaHorizontal = Math.Max(-minLeft, delta.X);
                var deltaVertical = Math.Max(-minTop, delta.Y);
                foreach (var item in designer.SelectedItems.OfType<ICanvasItem>())
                {
                    if (item == thisItem) continue;
                    item.Left += deltaHorizontal;
                    item.Top += deltaVertical;
                }
            }
            designer.InvalidateMeasure();
        }
    }
}