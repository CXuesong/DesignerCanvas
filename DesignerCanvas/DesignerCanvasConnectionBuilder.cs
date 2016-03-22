using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Undefined.DesignerCanvas
{

    internal enum ConnectorDirection
    {
        Left,
        Top,
        Right,
        Bottom
    }

    internal static class DesignerCanvasConnectionBuilder
    {
        public static IEnumerable<Point> BuildGeomotryPoints(Point startPoint, ConnectorDirection startDirection,
            Point endPoint, ConnectorDirection endDirection)
        {
            // for now we just ignore the direction.
            yield return startPoint;
            yield return new Point(startPoint.X, endPoint.Y);
            yield return endPoint;
        }
    }
}
