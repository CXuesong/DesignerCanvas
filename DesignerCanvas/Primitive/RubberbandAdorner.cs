using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Primitive
{
    public class RubberbandAdorner : Adorner
    {
        private readonly DesignerCanvas _Canvas;
        private readonly Action<object, Rect> _Callback;
        private readonly RubberbandChrome chrome;
        // Note the adorner layer will not scroll.
        // However, we don't count in the render transformation of canvas,
        // which will be handled in the callback function.
        private Point _StartPoint;
        private Point _EndPoint;

        /// <param name="startPoint">Relative to the adorner layer. Note the adorner layer will not scroll.</param>
        public RubberbandAdorner(DesignerCanvas canvas, Point startPoint, Action<object, Rect> callback) : base(canvas)
        {
            _Canvas = canvas;
            _Callback = callback;
            _StartPoint = _EndPoint = startPoint;
            chrome = new RubberbandChrome()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(_StartPoint.X , _StartPoint.Y, 0, 0)
            };
            ResizeChrome();
            this.AddVisualChild(chrome);
            CaptureMouse();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            var result = base.HitTestCore(hitTestParameters);
            if (result == null) return new PointHitTestResult(this, hitTestParameters.HitPoint);
            return result;
        }

        private void ResizeChrome()
        {
            var rect = new Rect(_StartPoint, _EndPoint);
            chrome.Margin = new Thickness(rect.X, rect.Y, 0, 0);
            chrome.Width = rect.Width;
            chrome.Height = rect.Height;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0) return chrome;
            throw new IndexOutOfRangeException();
        }

        protected override int VisualChildrenCount => 1;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
                _EndPoint = e.GetPosition(this);
                ResizeChrome();
            }
            else
            {
                ReleaseMouseCapture();
            }
            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            ReleaseMouseCapture();
            _Callback?.Invoke(this, new Rect(_StartPoint, _EndPoint));
            var adornerLayer = Parent as AdornerLayer;
            adornerLayer?.Remove(this);
            e.Handled = true;
        }
    }

    public class RubberbandChrome : Control
    {
        static RubberbandChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (RubberbandChrome),
                new FrameworkPropertyMetadata(typeof (RubberbandChrome)));
        }
    }
}
