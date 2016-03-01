using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Primitive
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += DragThumb_DragDelta;
            base.DragCompleted += DragThumb_DragCompleted;
        }

        /*/// <summary>
        /// Gets or sets the control subject to resizing.
        /// </summary>
        public FrameworkElement DestControl
        {
            get { return (DesignerCanvasItem) GetValue(DestControlProperty); }
            set { SetValue(DestControlProperty, value); }
        }

        public static readonly DependencyProperty DestControlProperty =
            DependencyProperty.Register("DestControl", typeof (FrameworkElement), typeof (ResizeThumb),
                new PropertyMetadata(null));*/

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var destControl = DataContext as DependencyObject;
            if (destControl == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            var minLeft = double.MaxValue;
            var minTop = double.MaxValue;
            // we only move DesignerItems
            foreach (var item in designer.SelectedItems)
            {
                var left = item.Left;
                var top = item.Top;
                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
            }
            var deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
            var deltaVertical = Math.Max(-minTop, e.VerticalChange);
            foreach (var item in designer.SelectedItems)
            {
                item.Left += deltaHorizontal;
                item.Top += deltaVertical;
            }
            e.Handled = true;
        }

        private void DragThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var destControl = DataContext as DependencyObject;
            if (destControl == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(destControl);
            designer?.InvalidateMeasure();
        }
    }
}