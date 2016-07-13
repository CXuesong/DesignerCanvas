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
            var destObject = DataContext as ICanvasItem;
            if (destObject == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(this);
            if (designer == null) return;
            instantPreview = designer.SelectedItems.Count < InstantPreviewItemsThreshold;
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var destItem = DataContext as ICanvasItem;
            if (destItem == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(this);
            if (designer == null) return;
            var container = designer.ItemContainerGenerator.ContainerFromItem(destItem) as UIElement;
            if (container == null) return;
            var minLeft = double.MaxValue;
            var minTop = double.MaxValue;
            //var tf = TransformToAncestor(designer);
            var delta = new Point(e.HorizontalChange, e.VerticalChange);
            var tf = container.RenderTransform;
            if (tf != null) delta = tf.Transform(delta);
            foreach (var item in designer.SelectedItems)
            {
                minLeft = Math.Min(item.Left, minLeft);
                minTop = Math.Min(item.Top, minTop);
            }
            var deltaX = Math.Max(-minLeft, delta.X);
            var deltaY = Math.Max(-minTop, delta.Y);
            if (instantPreview)
            {
                // This operation may be slow.
                foreach (var item in designer.SelectedItems)
                {
                    item.NotifyUserDragging(deltaX, deltaY);
                    item.Left += deltaX;
                    item.Top += deltaY;
                }
            }
            else
            {
                destItem.NotifyUserDragging(deltaX, deltaY);
                destItem.Left += deltaX;
                destItem.Top += deltaY;
            }
            e.Handled = true;
        }

        private void DragThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var destItem = DataContext as ICanvasItem;
            if (destItem == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(this);
            if (designer == null) return;
            var container = designer.ItemContainerGenerator.ContainerFromItem(destItem) as UIElement;
            if (container == null) return;
            if (!instantPreview)
            {
                var tf = TransformToAncestor((Visual)Parent);
                var delta = new Point(e.HorizontalChange, e.VerticalChange);
                delta = tf.Transform(delta);
                var minLeft = double.MaxValue;
                var minTop = double.MaxValue;
                foreach (var item in designer.SelectedItems.OfType<IBoxCanvasItem>())
                {
                    var left = item.Left;
                    var top = item.Top;
                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
                }
                var deltaHorizontal = Math.Max(-minLeft, delta.X);
                var deltaVertical = Math.Max(-minTop, delta.Y);
                foreach (var item in designer.SelectedItems.OfType<IBoxCanvasItem>())
                {
                    if (item == destItem) continue;
                    item.NotifyUserDragging(deltaHorizontal, deltaVertical);
                    item.Left += deltaHorizontal;
                    item.Top += deltaVertical;
                }
            }
            designer.InvalidateMeasure();
        }
    }
}