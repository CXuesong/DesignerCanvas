using System.Windows;

namespace Undefined.DesignerCanvas
{
    public interface IGraphicalObject
    {
        /// <summary>
        /// Gets the bounding rectangle of the object.
        /// </summary>
        Rect Bounds { get; }

        /// <summary>
        /// Determines whether the object is in the specified region.
        /// </summary>
        HitTestResult HitTest(Rect testRectangle);
    }

    public enum HitTestResult
    {
        None = 0,
        Intersects,
        /// <summary>
        /// The test region is inside the object.
        /// </summary>
        Contains,
        /// <summary>
        /// The object is inside the test region.
        /// </summary>
        Inside,
    }
}