using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Undefined.DesignerCanvas.Controls.Primitives
{
    public class PolyLineCanvasItemPresenter : Control
    {
        public static readonly DependencyProperty PathGeometryProperty =
            DependencyProperty.Register("PathGeometry", typeof (PathGeometry), typeof (PolyLineCanvasItemPresenter),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange));

        public PathGeometry PathGeometry
        {
            get { return (PathGeometry)GetValue(PathGeometryProperty); }
            set { SetValue(PathGeometryProperty, value); }
        }

        private void UpdatePathGeometry()
        {
            // Currently we connect two connectors with a straight line.
            var geometry = PathGeometry;
            if (geometry == null)
            {
                PathGeometry = geometry = new PathGeometry();
            }
            PathGeometry.Figures.Clear();
            var item = DataContext as IPolyLineCanvasItem;
            if (item?.Points.Count > 0)
            {
                geometry.Figures.Add(CanvasHelper.GenerstePathFigure(item.Points));
            }
        }

        public PolyLineCanvasItemPresenter()
        {
            this.DataContextChanged += PolyLineCanvasItemPresenter_DataContextChanged;
            DesignerCanvasItemContainer.AddBeforeDraggingStartedHandler(this, PolyLineCanvasItemPresenter_BeforeDraggingStarted);
        }

        private void PolyLineCanvasItemPresenter_BeforeDraggingStarted(object sender, BeforeDraggingStartedEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var item = DataContext as IPolyLineCanvasItem;
                if (item != null)
                {
                    // Use Control + Click on the segment to add a vertex
                    var seg = GetSegmentIndex(item.Points, Mouse.GetPosition(this));
                    //MessageBox.Show(seg.ToString());
                    if (seg >= 0)
                        item.Points.Insert(seg + 1, Mouse.GetPosition(this));
                }
            }
        }

        private void PolyLineCanvasItemPresenter_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldItem = e.OldValue as IPolyLineCanvasItem;
            if (oldItem != null)
                CollectionChangedEventManager.RemoveHandler(oldItem.Points, CanvasItem_PointCollectionChanged);
            var newItem = e.NewValue as IPolyLineCanvasItem;
            if (newItem != null)
                CollectionChangedEventManager.AddHandler(newItem.Points, CanvasItem_PointCollectionChanged);
            UpdatePathGeometry();
        }

        private void CanvasItem_PointCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdatePathGeometry();
        }

        /// <summary>
        /// Gets the nearest line segment index from the given point.
        /// </summary>
        /// <returns>The smaller index of the segment vertex, or -1 if the point is too far from the segments.</returns>
        protected static int GetSegmentIndex(IList<Point> vertices, Point point, double maxDistance = 5)
        {
            if (vertices.Count < 2) return -1;
            var nearestIndex = -1;
            var nearestDist = double.PositiveInfinity;
            for (int i = 0, j = vertices.Count - 1; i < j; i++)
            {
                if (!CanvasHelper.IsBetweenPoints(point, vertices[i], vertices[i + 1])) continue;
                var dist = CanvasHelper.DistanceToLine(point, vertices[i], vertices[i + 1]);
                if (dist > maxDistance) continue;
                if (dist < nearestDist)
                {
                    nearestIndex = i;
                    nearestDist = dist;
                }
            }
            return nearestIndex;
        }

        static PolyLineCanvasItemPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PolyLineCanvasItemPresenter), new FrameworkPropertyMetadata(typeof(PolyLineCanvasItemPresenter)));
        }
    }
}
