using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    public class PolyLineVertexThumb : Thumb
    {
        private DesignerCanvas parentCanvas;

        public IPolyLineCanvasItem PolyLine { get; }

        public int VertexIndex { get; }

        internal PolyLineVertexThumb(IPolyLineCanvasItem polyLine, int vertexIndex)
        {
            if (polyLine == null) throw new ArgumentNullException(nameof(polyLine));
            PolyLine = polyLine;
            VertexIndex = vertexIndex;
            DragDelta += PolyLineVertexThumb_DragDelta;
            DragStarted += PolyLineVertexThumb_DragStarted;
            DragCompleted += PolyLineVertexThumb_DragCompleted;
        }

        private void PolyLineVertexThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            parentCanvas = DesignerCanvas.FindDesignerCanvas(this);
            //vertexPos = VertexCollection[VertexIndex];
        }

        private void PolyLineVertexThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (parentCanvas == null) return;
            var z = parentCanvas.Zoom / 100.0;
            var hc = e.HorizontalChange / z;
            var vc = e.VerticalChange / z;
            var mod = Keyboard.Modifiers;
            PolyLine.Points[VertexIndex] += new Vector(hc, vc);
        }

        private void PolyLineVertexThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            PolyLine.NormalizePositions();
        }

        internal void RefreshPosition()
        {
            var p = PolyLine.Points[VertexIndex];
            SetValue(Canvas.LeftProperty, p.X);
            SetValue(Canvas.TopProperty, p.Y);
        }
    }

    [TemplatePart(Name = "PART_Canvas", Type = typeof (Canvas))]
    public class PolyLineVerticesAdorner : CanvasAdorner
    {
        private List<PolyLineVertexThumb> vertexThumbs = new List<PolyLineVertexThumb>();

        private Canvas canvas;

        public new IPolyLineCanvasItem AdornedObject => (IPolyLineCanvasItem) base.AdornedObject;

        public PolyLineVerticesAdorner(IPolyLineCanvasItem adornedObject) : base(adornedObject)
        {
            CollectionChangedEventManager.AddHandler(adornedObject.Points, (sender, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Reset:
                        ResetThumbs();
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                        Debug.Assert(e.NewItems.Count == 1);
                        vertexThumbs[e.OldStartingIndex].RefreshPosition();
                        break;
                }
            });
        }

        static PolyLineVerticesAdorner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PolyLineVerticesAdorner), new FrameworkPropertyMetadata(typeof(PolyLineVerticesAdorner)));
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            canvas = (Canvas) GetTemplateChild("PART_Canvas");
            ResetThumbs();
        }

        private void ResetThumbs()
        {
            if (canvas == null) return;
            vertexThumbs.Clear();
            canvas.Children.Clear();
            for (int i = 0; i < AdornedObject.Points.Count; i++)
            {
                //var p = AdornedObject.Points[i];
                var thumb = new PolyLineVertexThumb(AdornedObject, i);
                thumb.Width = thumb.Height = 8;
                thumb.Margin = new Thickness(-4, -4, 0, 0);
                thumb.Cursor = Cursors.Cross;
                thumb.RefreshPosition();
                vertexThumbs.Add(thumb);
                canvas.Children.Add(thumb);
            }
        }

        protected override void OnUpdateLayout()
        {
            base.OnUpdateLayout();
            var z = ParentCanvas.Zoom/100.0;
            Left = AdornedObject.Left*z;
            Top = AdornedObject.Top*z;
            if (vertexThumbs.Count != AdornedObject.Points.Count) ResetThumbs();
        }
    }
}
