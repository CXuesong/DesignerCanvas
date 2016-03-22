using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Undefined.DesignerCanvas.ObjectModel;

namespace Undefined.DesignerCanvas.Primitive
{
    public class RotateThumb : Thumb
    {
        private double initialAngle;
        private Vector startVector;
        private Point centerPoint;
        private Canvas canvas;

        public RotateThumb()
        {

            DragDelta += RotateThumb_DragDelta;
            DragStarted += RotateThumb_DragStarted;
            DragCompleted += RotateThumb_DragCompleted;
        }

        private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            var destControl = DataContext as FrameworkElement;
            if (destControl == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            var item = designer.ItemContainerGenerator.ItemFromContainer(destControl) as IEntity;
            if (item == null) return;
            centerPoint = destControl.TranslatePoint(
                new Point(destControl.Width*destControl.RenderTransformOrigin.X,
                    destControl.Height*destControl.RenderTransformOrigin.Y),
                canvas);
            initialAngle = item.Angle;
            var startPoint = Mouse.GetPosition(canvas);
            startVector = Point.Subtract(startPoint, centerPoint);
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var destControl = DataContext as DependencyObject;
            if (destControl == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            var item = designer.ItemContainerGenerator.ItemFromContainer(destControl) as IEntity;
            if (item == null) return;
            var mod = Keyboard.Modifiers;
            item.Angle = initialAngle + EvalAngle((mod & ModifierKeys.Shift) == ModifierKeys.Shift);
            (destControl as UIElement)?.InvalidateArrange();
        }

        private void RotateThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var destControl = DataContext as DependencyObject;
            if (destControl == null) return;
            var designer = DesignerCanvas.FindDesignerCanvas(destControl);
            if (designer == null) return;
            var destItem = designer.ItemContainerGenerator.ItemFromContainer(destControl);
            var mod = Keyboard.Modifiers;
            var deltaAngle = EvalAngle((mod & ModifierKeys.Shift) == ModifierKeys.Shift);
            foreach (var item in designer.SelectedItems.OfType<IEntity>())
            {
                if (item != destItem)
                {
                    item.Angle += deltaAngle;
                }
            }
            designer.InvalidateMeasure();
        }

        private double EvalAngle(bool makeRegular)
        {
            const double regularAngleStep = 15;
            var currentPoint = Mouse.GetPosition(canvas);
            var currentVector = Point.Subtract(currentPoint, centerPoint);
            var angle = Vector.AngleBetween(startVector, currentVector);
            if (makeRegular)
            {
                angle = Math.Round((initialAngle + angle)/regularAngleStep)*regularAngleStep - initialAngle;
            }
            return angle;
        }
    }
}
