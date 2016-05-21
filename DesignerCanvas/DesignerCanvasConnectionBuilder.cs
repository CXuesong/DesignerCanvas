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
                    yield return new Point(startPoint.X*0.9 + endPoint.X*0.1, startPoint.Y);
                    yield return new Point(startPoint.X*0.9 + endPoint.X*0.1, endPoint.Y);
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
                    yield return new Point(startPoint.X, startPoint.Y*0.9 + endPoint.Y*0.1);
                    yield return new Point(endPoint.X, startPoint.Y*0.9 + endPoint.Y*0.1);
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
