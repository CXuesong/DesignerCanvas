using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Undefined.DesignerCanvas.Controls.Primitives
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
            var destObject = DataContext as ICanvasItem;
            if (destObject == null) return;
            var designer = Controls.DesignerCanvas.FindDesignerCanvas(this);
            if (designer == null) return;
            var container = designer.ItemContainerGenerator.ContainerFromItem(destObject) as FrameworkElement;
            if (container == null) return;
            centerPoint = container.TranslatePoint(new Point(destObject.Width*0.5, destObject.Height*0.5), null);
            initialAngle = destObject.Angle;
            var startPoint = Mouse.GetPosition(null);
            startVector = Point.Subtract(startPoint, centerPoint);
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var destObject = DataContext as ICanvasItem;
            if (destObject == null) return;
            var designer = Controls.DesignerCanvas.FindDesignerCanvas(this);
            if (designer == null) return;
            var mod = Keyboard.Modifiers;
            destObject.Angle = initialAngle + EvalAngle((mod & ModifierKeys.Shift) == ModifierKeys.Shift);
        }

        private void RotateThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var destObject = DataContext as ICanvasItem;
            if (destObject == null) return;
            var designer = Controls.DesignerCanvas.FindDesignerCanvas(this);
            if (designer == null) return;
            var mod = Keyboard.Modifiers;
            var deltaAngle = EvalAngle((mod & ModifierKeys.Shift) == ModifierKeys.Shift);
            foreach (var item in designer.SelectedItems.OfType<ICanvasItem>())
            {
                if (item != destObject)
                    item.Angle += deltaAngle;
            }
            designer.InvalidateMeasure();
        }

        private double EvalAngle(bool makeRegular)
        {
            const double regularAngleStep = 15;
            var currentPoint = Mouse.GetPosition(null);
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
