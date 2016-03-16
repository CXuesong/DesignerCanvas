using System;
using System.Windows;

namespace Undefined.DesignerCanvas.ObjectModel
{
    public interface IGraphicalObject
    {
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
}