using System;
using System.Windows;

namespace Undefined.DesignerCanvas
{
    public interface ICanvasItem
    {
        double Left { get; set; }

        double Top { get; set; }

        /// <summary>
        /// Gets the bounding rectangle of the object.
        /// </summary>
        Rect Bounds { get; }

        /// <summary>
        /// Fires when <see cref="Bounds"/> has been changed.
        /// </summary>
        event EventHandler BoundsChanged;

        /// <summary>
        /// Determines whether the object is in the specified region.
        /// </summary>
        HitTestResult HitTest(Rect testRectangle);

    }

    public interface IBoxCanvasItem : ICanvasItem
    {

        double Width { get; set; }

        double Height { get; set; }

        bool Resizeable { get; }

        /// <summary>
        /// Angle of rotation, in degrees.
        /// </summary>
        double Angle { get; set; }

    }
}