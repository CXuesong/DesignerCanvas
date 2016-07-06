using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Undefined.DesignerCanvas
{
    static class CanvasHelper
    {
        public static Rect GetBounds(this IEnumerable<ICanvasItem> objects)
        {
            return objects.AsParallel().Aggregate(() => Rect.Empty,
                (r, i) => Rect.Union(r, i.Bounds), Rect.Union, r => r);
        }
    }
}
