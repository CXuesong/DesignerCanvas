using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Undefined.DesignerCanvas.ObjectModel;

namespace Undefined.DesignerCanvas
{
    internal static class DesignerCanvasConnectionBuilder
    {
        public static IEnumerable<Point> BuildGeomotryPoints(Point startPoint, ConnectorDirection startDirection,
            Point endPoint, ConnectorDirection endDirection)
        {
            // for now we just ignore the direction.
            yield return startPoint;
            if (startDirection == ConnectorDirection.Horizontal)
            {
                if (endDirection == ConnectorDirection.Horizontal)
                {
                    yield return new Point(startPoint.X, startPoint.Y);
                    yield return new Point(startPoint.X, endPoint.Y);
                }
                else
                {
                    yield return new Point(startPoint.X, endPoint.Y);
                }
            }
            else
            {
                if (endDirection == ConnectorDirection.Vertical)
                {
                    yield return new Point(startPoint.X, startPoint.Y);
                    yield return new Point(endPoint.X, startPoint.Y);
                }
                else
                {
                    yield return new Point(startPoint.X, endPoint.Y);
                }
            }
            yield return endPoint;
        }
    }
}
