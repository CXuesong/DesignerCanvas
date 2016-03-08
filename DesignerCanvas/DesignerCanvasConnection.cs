using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Undefined.DesignerCanvas
{
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

        /*  #region Properties


          // connection path geometry
          private PathGeometry pathGeometry;
          public PathGeometry PathGeometry
          {
              get { return pathGeometry; }
              set
              {
                  if (pathGeometry != value)
                  {
                      pathGeometry = value;
                      UpdateAnchorPosition();
                      OnPropertyChanged("PathGeometry");
                  }
              }
          }

          // between source connector position and the beginning 
          // of the path geometry we leave some space for visual reasons; 
          // so the anchor position source really marks the beginning 
          // of the path geometry on the source side
          private Point anchorPositionSource;
          public Point AnchorPositionSource
          {
              get { return anchorPositionSource; }
              set
              {
                  if (anchorPositionSource != value)
                  {
                      anchorPositionSource = value;
                      OnPropertyChanged("AnchorPositionSource");
                  }
              }
          }

          // slope of the path at the anchor position
          // needed for the rotation angle of the arrow
          private double anchorAngleSource;
          public double AnchorAngleSource
          {
              get { return anchorAngleSource; }
              set
              {
                  if (anchorAngleSource != value)
                  {
                      anchorAngleSource = value;
                      OnPropertyChanged("AnchorAngleSource");
                  }
              }
          }

          // analogue to source side
          private Point anchorPositionSink;
          public Point AnchorPositionSink
          {
              get { return anchorPositionSink; }
              set
              {
                  if (anchorPositionSink != value)
                  {
                      anchorPositionSink = value;
                      OnPropertyChanged("AnchorPositionSink");
                  }
              }
          }
          // analogue to source side
          private double anchorAngleSink;
          public double AnchorAngleSink
          {
              get { return anchorAngleSink; }
              set
              {
                  if (anchorAngleSink != value)
                  {
                      anchorAngleSink = value;
                      OnPropertyChanged("AnchorAngleSink");
                  }
              }
          }

          private ArrowSymbol sourceArrowSymbol = ArrowSymbol.None;
          public ArrowSymbol SourceArrowSymbol
          {
              get { return sourceArrowSymbol; }
              set
              {
                  if (sourceArrowSymbol != value)
                  {
                      sourceArrowSymbol = value;
                      OnPropertyChanged("SourceArrowSymbol");
                  }
              }
          }

          public ArrowSymbol sinkArrowSymbol = ArrowSymbol.Arrow;
          public ArrowSymbol SinkArrowSymbol
          {
              get { return sinkArrowSymbol; }
              set
              {
                  if (sinkArrowSymbol != value)
                  {
                      sinkArrowSymbol = value;
                      OnPropertyChanged("SinkArrowSymbol");
                  }
              }
          }

          // specifies a point at half path length
          private Point labelPosition;
          public Point LabelPosition
          {
              get { return labelPosition; }
              set
              {
                  if (labelPosition != value)
                  {
                      labelPosition = value;
                      OnPropertyChanged("LabelPosition");
                  }
              }
          }

          // pattern of dashes and gaps that is used to outline the connection path
          private DoubleCollection strokeDashArray;
          public DoubleCollection StrokeDashArray
          {
              get
              {
                  return strokeDashArray;
              }
              set
              {
                  if (strokeDashArray != value)
                  {
                      strokeDashArray = value;
                      OnPropertyChanged("StrokeDashArray");
                  }
              }
          }
          // if connected, the ConnectionAdorner becomes visible
          private bool isSelected;
          public bool IsSelected
          {
              get { return isSelected; }
              set
              {
                  if (isSelected != value)
                  {
                      isSelected = value;
                      OnPropertyChanged("IsSelected");
                      if (isSelected)
                          ShowAdorner();
                      else
                          HideAdorner();
                  }
              }
          }

          #endregion
      */
        public DesignerCanvasConnection()
        {

        }

       /* protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // usual selection business
            var designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
            if (designer != null)
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (IsSelected)
                    {
                        IsSelected = false;
                        designer.SelectedItems.Remove(this);
                    }
                    else
                    {
                        IsSelected = true;
                        designer.SelectedItems.Add(this);
                    }
                else if (!IsSelected)
                {
                    foreach (ISelectable item in designer.SelectedItems)
                        item.IsSelected = false;

                    designer.SelectedItems.Clear();
                    IsSelected = true;
                    designer.SelectedItems.Add(this);
                }
            e.Handled = false;
        }

        void OnConnectorPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            // whenever the 'Position' property of the source or sink Connector 
            // changes we must update the connection path geometry
            if (e.PropertyName.Equals("Position"))
            {
                UpdatePathGeometry();
            }
        }

        private void UpdatePathGeometry()
        {
            if (Source != null && Sink != null)
            {
                var geometry = new PathGeometry();
                List<Point> linePoints = PathFinder.GetConnectionLine(Source.GetInfo(), Sink.GetInfo(), true);
                if (linePoints.Count > 0)
                {
                    var figure = new PathFigure();
                    figure.StartPoint = linePoints[0];
                    linePoints.Remove(linePoints[0]);
                    figure.Segments.Add(new PolyLineSegment(linePoints, true));
                    geometry.Figures.Add(figure);

                    PathGeometry = geometry;
                }
            }
        }

        private void UpdateAnchorPosition()
        {
            Point pathStartPoint, pathTangentAtStartPoint;
            Point pathEndPoint, pathTangentAtEndPoint;
            Point pathMidPoint, pathTangentAtMidPoint;

            // the PathGeometry.GetPointAtFractionLength method gets the point and a tangent vector 
            // on PathGeometry at the specified fraction of its length
            PathGeometry.GetPointAtFractionLength(0, out pathStartPoint, out pathTangentAtStartPoint);
            PathGeometry.GetPointAtFractionLength(1, out pathEndPoint, out pathTangentAtEndPoint);
            PathGeometry.GetPointAtFractionLength(0.5, out pathMidPoint, out pathTangentAtMidPoint);

            // get angle from tangent vector
            AnchorAngleSource = Math.Atan2(-pathTangentAtStartPoint.Y, -pathTangentAtStartPoint.X) * (180 / Math.PI);
            AnchorAngleSink = Math.Atan2(pathTangentAtEndPoint.Y, pathTangentAtEndPoint.X) * (180 / Math.PI);

            // add some margin on source and sink side for visual reasons only
            pathStartPoint.Offset(-pathTangentAtStartPoint.X * 5, -pathTangentAtStartPoint.Y * 5);
            pathEndPoint.Offset(pathTangentAtEndPoint.X * 5, pathTangentAtEndPoint.Y * 5);

            AnchorPositionSource = pathStartPoint;
            AnchorPositionSink = pathEndPoint;
            LabelPosition = pathMidPoint;
        }

        private void ShowAdorner()
        {
            // the ConnectionAdorner is created once for each Connection
            if (connectionAdorner == null)
            {
                var designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                var adornerLayer = AdornerLayer.GetAdornerLayer(designer);
                if (adornerLayer != null)
                {
                    connectionAdorner = new ConnectionAdorner(designer, this);
                    adornerLayer.Add(connectionAdorner);
                }
            }
            connectionAdorner.Visibility = Visibility.Visible;
        }

        internal void HideAdorner()
        {
            if (connectionAdorner != null)
                connectionAdorner.Visibility = Visibility.Collapsed;
        }

        void Connection_Unloaded(object sender, RoutedEventArgs e)
        {
            // do some housekeeping when Connection is unloaded

            // remove event handler
            source.PropertyChanged -= OnConnectorPositionChanged;
            sink.PropertyChanged -= OnConnectorPositionChanged;

            // remove adorner
            if (connectionAdorner != null)
            {
                var designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                var adornerLayer = AdornerLayer.GetAdornerLayer(designer);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(connectionAdorner);
                    connectionAdorner = null;
                }
            }
        }*/
    }
}
