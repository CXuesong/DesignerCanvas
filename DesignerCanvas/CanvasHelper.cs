using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// Provides helper functions for Customizing <see cref="ICanvasItem"/>.
    /// </summary>
    public static class CanvasHelper
    {
        public static Rect GetBounds(this IEnumerable<ICanvasItem> objects)
        {
            return objects.AsParallel().Aggregate(() => Rect.Empty,
                (r, i) => Rect.Union(r, i.Bounds), Rect.Union, r => r);
        }

        /// <summary>
        /// Calculates the distance from Point p0 to the line passing
        /// through p1 and p2.
        /// </summary>
        public static double DistanceToLine(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            //https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points
            var dy = y2 - y1;
            var dx = x2 - x1;
            return Math.Abs((dy*x0) - dx*y0 + x2*y1 - y2*x1)/Math.Sqrt(dy*dy + dx*dx);
        }

        /// <summary>
        /// Calculates the distance from Point p0 to the line passing
        /// through p1 and p2.
        /// </summary>
        public static double DistanceToLine(Point p0, Point p1, Point p2)
        {
            return DistanceToLine(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y);
        }

        /// <summary>
        /// Determines whether p0 is between p1 and p2.
        /// </summary>
        public static bool IsBetweenPoints(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            double x10 = x1 - x0, x20 = x2 - x0, x12 = x1 - x2;
            double y10 = y1 - y0, y20 = y2 - y0, y12 = y1 - y2;
            var l12 = x12*x12 + y12*y12;
            var l10 = x10*x10 + y10*y10;
            var l20 = x20*x20 + y20*y20;
            if ((l12*l12 + l10*l10 - l20*l20)*l12*l10 < 0) return false;
            if ((l12*l12 + l20*l20 - l10*l10)*l12*l20 < 0) return false;
            return true;
        }

        /// <summary>
        /// Determines whether p0 is between p1 and p2.
        /// </summary>
        public static bool IsBetweenPoints(Point p0, Point p1, Point p2)
        {
            return IsBetweenPoints(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y);
        }

        public static PathFigure GenerstePathFigure(IList<Point> points)
        {
            // Currently we connect two connectors with a straight line.
            if (points.Count > 0)
            {
                var figure = new PathFigure { StartPoint = points[0] };
                figure.Segments.Add(new PolyLineSegment(points, true));
                return figure;
            }
            return new PathFigure();
        }

    }
}
