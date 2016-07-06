using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    /// <summary>
    /// Represents a adorder to a specific CanvasItem.
    /// </summary>
    /// <remarks>This  class is used becaused it won't zoom as CanvasEntityItem is being zoomed.</remarks>
    public class CanvasAdorner : Control
    {
        public ICanvasItem AdornedObject { get; }

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(double), typeof(CanvasAdorner), new PropertyMetadata(0.0));

        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(double), typeof(CanvasAdorner), new PropertyMetadata(0.0));

        private DesignerCanvas _ParentCanvas;

        public DesignerCanvas ParentCanvas => _ParentCanvas;

        internal void SetCanvas(Controls.DesignerCanvas canvas)
        {
            if (_ParentCanvas != null) _ParentCanvas.ZoomChanged -= ParentCanvas_ZoomChanged;
            _ParentCanvas = canvas;
            if (canvas != null) canvas.ZoomChanged += ParentCanvas_ZoomChanged;
            if (canvas != null) OnUpdateLayout();
        }

        private void ParentCanvas_ZoomChanged(object sender, EventArgs e)
        {
            Debug.Assert(sender == _ParentCanvas);
            OnUpdateLayout();
        }

        public CanvasAdorner(ICanvasItem adornedObject)
        {
            if (adornedObject == null) throw new ArgumentNullException(nameof(adornedObject));
            AdornedObject = adornedObject;
            var npc = adornedObject as INotifyPropertyChanged;
            if (npc != null)
            {
                PropertyChangedEventManager.AddHandler(npc, AdornedObject_PropertyChanged, "");
            }
            SetBinding(Canvas.LeftProperty, new Binding("Left") {RelativeSource = RelativeSource.Self});
            SetBinding(Canvas.TopProperty, new Binding("Top") {RelativeSource = RelativeSource.Self});
        }

        static CanvasAdorner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasAdorner), new FrameworkPropertyMetadata(typeof(CanvasAdorner)));
        }

        private void AdornedObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(sender == AdornedObject);
            OnAdornedObjectPropertyChanged(e.PropertyName);
        }

        protected virtual void OnAdornedObjectPropertyChanged(string propertyName)
        {
            if (_ParentCanvas != null)
            {
                if (propertyName == nameof(AdornedObject.Bounds)) OnUpdateLayout();
            }
        }

        protected virtual void OnUpdateLayout()
        {
            
        }
    }

    /// <summary>
    /// Used for rendering a handle for resizing &amp; rotation.
    /// </summary>
    public class ResizeRotateAdorner : CanvasAdorner
    {
        private RotateTransform rotateTransform = new RotateTransform();

        public new ICanvasBoxItem AdornedObject => (ICanvasBoxItem) base.AdornedObject;

        public ResizeRotateAdorner(ICanvasBoxItem adornedObject) : base(adornedObject)
        {
            SnapsToDevicePixels = true;
            this.DataContext = adornedObject;
            this.RenderTransform = rotateTransform;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        static ResizeRotateAdorner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeRotateAdorner), new FrameworkPropertyMetadata(typeof(ResizeRotateAdorner)));
        }

        protected override void OnAdornedObjectPropertyChanged(string propertyName)
        {
            base.OnAdornedObjectPropertyChanged(propertyName);
            if (ParentCanvas == null) return;
            if (propertyName == nameof(ICanvasBoxItem.Angle)) OnUpdateLayout();
        }

        protected override void OnUpdateLayout()
        {
            base.OnUpdateLayout();
            var zoom = ParentCanvas.Zoom/100.0;
            Left = AdornedObject.Left*zoom;
            Top = AdornedObject.Top * zoom;
            Width = AdornedObject.Width*zoom;
            Height = AdornedObject.Height*zoom;
            rotateTransform.Angle = AdornedObject.Angle;
        }
    }

    public class SizeAdorner : CanvasAdorner
    {
        private RotateTransform rotateTransform = new RotateTransform();

        public new ICanvasBoxItem AdornedObject => (ICanvasBoxItem)base.AdornedObject;

        public SizeAdorner(ICanvasBoxItem adornedObject) : base(adornedObject)
        {
            SnapsToDevicePixels = true;
            this.DataContext = adornedObject;
            this.RenderTransform = rotateTransform;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        protected override void OnAdornedObjectPropertyChanged(string propertyName)
        {
            base.OnAdornedObjectPropertyChanged(propertyName);
            if (ParentCanvas == null) return;
            if (propertyName == nameof(ICanvasBoxItem.Angle)) OnUpdateLayout();
        }

        protected override void OnUpdateLayout()
        {
            base.OnUpdateLayout();
            var zoom = ParentCanvas.Zoom / 100.0;
            Left = AdornedObject.Left * zoom;
            Top = AdornedObject.Top * zoom;
            Width = AdornedObject.Width * zoom;
            Height = AdornedObject.Height * zoom;
            rotateTransform.Angle = AdornedObject.Angle;
        }

        static SizeAdorner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SizeAdorner), new FrameworkPropertyMetadata(typeof(SizeAdorner)));
        }
    }
}
