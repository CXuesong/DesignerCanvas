using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
                var figure = new PathFigure {StartPoint = item.Points[0]};
                figure.Segments.Add(new PolyLineSegment(item.Points, true));
                geometry.Figures.Add(figure);
            }
        }

        public PolyLineCanvasItemPresenter()
        {
            this.DataContextChanged += PolyLineCanvasItemPresenter_DataContextChanged;
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

        static PolyLineCanvasItemPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PolyLineCanvasItemPresenter), new FrameworkPropertyMetadata(typeof(PolyLineCanvasItemPresenter)));
        }
    }
}
