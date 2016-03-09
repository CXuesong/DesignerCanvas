namespace Undefined.DesignerCanvas.ObjectModel
{
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