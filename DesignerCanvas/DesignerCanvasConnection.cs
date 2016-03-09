using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Undefined.DesignerCanvas.ObjectModel;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// Used for rendering <see cref="Connection"/> in <see cref="DesignerCanvas" />.
    /// </summary>
    public class DesignerCanvasConnection : Control
    {
        private Adorner connectionAdorner;

        public static readonly DependencyProperty SourcePointProperty =
            DependencyProperty.Register("SourcePoint", typeof (Point), typeof (DesignerCanvasConnection),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsArrange,
                    (d, e) => ((DesignerCanvasConnection) d).UpdatePathGeometry()));

        public static readonly DependencyProperty SinkPointProperty =
            DependencyProperty.Register("SinkPoint", typeof (Point), typeof (DesignerCanvasConnection),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsArrange,
                    (d, e) => ((DesignerCanvasConnection) d).UpdatePathGeometry()));

        public static readonly DependencyProperty PathGeometryProperty =
            DependencyProperty.Register("PathGeometry", typeof (PathGeometry), typeof (DesignerCanvasConnection),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty;

        public Point SourcePoint
        {
            get { return (Point)GetValue(SourcePointProperty); }
            set { SetValue(SourcePointProperty, value); }
        }

        public Point SinkPoint
        {
            get { return (Point)GetValue(SinkPointProperty); }
            set { SetValue(SinkPointProperty, value); }
        }

        public PathGeometry PathGeometry
        {
            get { return (PathGeometry)GetValue(PathGeometryProperty); }
            set { SetValue(PathGeometryProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private void UpdatePathGeometry()
        {
            // Currently we connect two connectors with a straight line.
            var geometry = PathGeometry;
            if (geometry == null)
            {
                PathGeometry = geometry = new PathGeometry();
            }
            var points = new[] {SourcePoint, SinkPoint};
            PathGeometry.Figures.Clear();
            var figure = new PathFigure {StartPoint = points[0]};
            figure.Segments.Add(new PolyLineSegment(points, true));
            geometry.Figures.Add(figure);
        }

        public DesignerCanvas ParentDesigner => DesignerCanvas.FindDesignerCanvas(this);

        static DesignerCanvasConnection()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvasConnection), new FrameworkPropertyMetadata(typeof(DesignerCanvasConnection)));
            Selector.IsSelectedProperty.OverrideMetadata(typeof(DesignerCanvasConnection),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (sender, e) =>
                    {
                        var s = (DesignerCanvasConnection)sender;
                        s.ParentDesigner?.NotifyItemIsSelectedChanged(s);
                    }));
        }

        public DesignerCanvasConnection()
        {

        }
    }
}
