using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Undefined.DesignerCanvas.Primitive;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// 用于承载绘图图面。
    /// </summary>
    [TemplatePart(Name = "PART_Canvas", Type = typeof (Canvas))]
    public class DesignerCanvas : Control, IScrollInfo
    {
        private readonly GraphicalObjectCollection _Items = new GraphicalObjectCollection();
        private readonly GraphicalObjectCollection _SelectedItems = new GraphicalObjectCollection();

        private readonly GraphicalObjectContainerGenerator _ItemContainerGenerator =
            new GraphicalObjectContainerGenerator();

        #region Items & States

        public GraphicalObjectCollection Items => _Items;

        public GraphicalObjectCollection SelectedItems => _SelectedItems;

        public GraphicalObjectContainerGenerator ItemContainerGenerator => _ItemContainerGenerator;

        // Indicates whether containers' IsSelected property is beging changed to
        // correspond with SelectedItems collection.
        private bool isSelectedContainersSynchronizing = false;

        private void _SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (isSelectedContainersSynchronizing)
                    throw new InvalidOperationException("此函数不支持递归调用。");
                isSelectedContainersSynchronizing = true;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                var container = _ItemContainerGenerator.ContainerFromItem((GraphicalObject) item);
                                container?.SetValue(Selector.IsSelectedProperty, false);
                            }
                        }
                        if (e.NewItems != null)
                        {
                            foreach (var item in e.NewItems)
                            {
                                var container = _ItemContainerGenerator.ContainerFromItem((GraphicalObject) item);
                                container?.SetValue(Selector.IsSelectedProperty, true);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        var unselectedContainers = partCanvas.Children.Cast<DependencyObject>().ToList();
                        foreach (var item in SelectedItems)
                        {
                            var container = _ItemContainerGenerator.ContainerFromItem(item);
                            if (container != null)
                            {
                                container.SetValue(Selector.IsSelectedProperty, true);
                                unselectedContainers.Remove(container);
                            }
                        }
                        foreach (var item in unselectedContainers)
                        {
                            item.SetValue(Selector.IsSelectedProperty, false);
                        }
                        break;
                }
            }
            finally
            {
                isSelectedContainersSynchronizing = false;
            }
        }

        private void _Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            var container =
                                (UIElement) _ItemContainerGenerator.ContainerFromItem((GraphicalObject) item);
                            if (container != null) partCanvas.Children.Remove(container);
                            _ItemContainerGenerator.Recycle(container);
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            partCanvas.Children.Add(
                                (UIElement) _ItemContainerGenerator.CreateContainer((GraphicalObject) item));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in partCanvas.Children)
                    {
                        _ItemContainerGenerator.Recycle((DesignerCanvasItem) item);
                    }
                    partCanvas.Children.Clear();
                    foreach (var item in _Items)
                    {
                        partCanvas.Children.Add((UIElement) _ItemContainerGenerator.CreateContainer(item));
                    }
                    break;
            }
            this.InvalidateMeasure();
        }

        #endregion

        #region UI

        private Canvas partCanvas;
        private TranslateTransform canvasTransform = new TranslateTransform();

        internal static DesignerCanvas FindDesignerCanvas(DependencyObject childContainer)
        {
            while (childContainer != null)
            {
                childContainer = VisualTreeHelper.GetParent(childContainer);
                var dc = childContainer as DesignerCanvas;
                if (dc != null) return dc;
            }
            return null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            partCanvas = (Canvas) GetTemplateChild("PART_Canvas");
            if (partCanvas == null)
            {
                partCanvas = new Canvas();
                this.AddVisualChild(partCanvas);
            }
            partCanvas.RenderTransform = canvasTransform;
            partCanvas.MouseDown += PartCanvas_MouseDown;
            partCanvas.MouseMove += PartCanvas_MouseMove;
            partCanvas.MouseUp += PartCanvas_MouseUp;
        }

        private Point? RubberbandStartPoint = null;

        private void PartCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == partCanvas)
            {
                RubberbandStartPoint = e.GetPosition(partCanvas);
            }
        }

        private void PartCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (RubberbandStartPoint != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    if (adornerLayer != null)
                    {
                        var adorner = new RubberbandAdorner(this, RubberbandStartPoint.Value, Rubberband_Callback);
                        adornerLayer.Add(adorner);
                        RubberbandStartPoint = null;
                    }
                }
            }
        }

        private void PartCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RubberbandStartPoint = null;
            if (e.Source == partCanvas)
            {
                _SelectedItems.Clear();
            }
        }

        private void Rubberband_Callback(object o, Rect rect)
        {
            var mod = Keyboard.Modifiers;
            if ((mod & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None)
            {
                _SelectedItems.Clear();
                _SelectedItems.AddRange(Items.ObjectsInRegion(rect));
            }
            else
            {
                var newItems = new HashSet<GraphicalObject>(Items.ObjectsInRegion(rect));
                if ((mod & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    // Switch
                    var intersection = _SelectedItems.Where(i => newItems.Contains(i)).ToList();
                    foreach (var item in intersection)
                    {
                        _SelectedItems.Remove(item);
                        newItems.Remove(item);
                    }
                    _SelectedItems.AddRange(newItems);
                }
                else if ((mod & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    // Merge
                    foreach (var item in _SelectedItems) newItems.Remove(item);
                    _SelectedItems.AddRange(newItems);
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            const double measurementMargin = 10;
            var bounds = _Items.Bounds;
            if (bounds.IsEmpty) bounds = new Rect(0, 0, 0, 0);
            bounds.Width += measurementMargin;
            bounds.Height += measurementMargin;
            _ExtendRect = bounds;
            // Right now we do not support scrolling to the left / top of the origin.
            _ExtendRect.X = _ExtendRect.Y = 0;
            _ViewPortSize = constraint;
            // Readjust the scroll offset.
            SetHorizontalOffset(_ScrollOffset.X);
            SetVerticalOffset(_ScrollOffset.Y);
            ScrollOwner?.InvalidateScrollInfo();
            //partCanvas.Measure(); // Seems no use.
            if (double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height))
            {
                return new Size(Math.Max(bounds.Right, 0), Math.Max(bounds.Bottom, 0));
            }
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        #endregion

        #region Notifications from Items

        internal void NotifyItemMouseDown(DesignerCanvasItem container)
        {
            Debug.Assert(container != null);
            //Debug.Print("NotifyItemMouseDown");
            if (container.IsSelected == false)
            {
                // Left click to selecte an object.
                if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == ModifierKeys.None)
                {
                    // Unselect other items first.
                    _SelectedItems.Clear();
                }
                _SelectedItems.Add(_ItemContainerGenerator.ItemFromContainer(container));
            }
        }

        internal void NotifyItemIsSelectedChanged(DesignerCanvasItem container)
        {
            Debug.Assert(container != null);
            // Do not update SelectedItems when SelectedItems are being updated.
            if (isSelectedContainersSynchronizing) return;
            var item = _ItemContainerGenerator.ItemFromContainer(container);
            if (item == null) return;
            if (container.IsSelected)
            {
                Debug.Assert(!SelectedItems.Contains(item));
                SelectedItems.Add(item);
            }
            else
            {
                var reuslt = SelectedItems.Remove(item);
                Debug.Assert(reuslt);
            }
        }

        #endregion

        public DesignerCanvas()
        {
            _Items.CollectionChanged += _Items_CollectionChanged;
            _SelectedItems.CollectionChanged += _SelectedItems_CollectionChanged;
            // Note PartCanvas property will return null here.
        }

        static DesignerCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (DesignerCanvas),
                new FrameworkPropertyMetadata(typeof (DesignerCanvas)));
        }

        #region IScrollInfo

        private Rect _ExtendRect;   // Boundary of virtual canvas.
        private Size _ViewPortSize;
        private Point _ScrollOffset; // Offset is non-negative, relative to the top-left corner of _ExtendRect.

        /// <summary>
        /// Gets the boundary of view port relative to the virtual canvas.
        /// </summary>
        public Rect ViewPortBounds
        {
            get
            {
                var tl = _ExtendRect.TopLeft;
                tl.Offset(_ScrollOffset.X, _ScrollOffset.Y);
                return new Rect(tl, _ViewPortSize);
            }
        }

        private const double ScrollStepIncrement = 10;
        private const double ScrollPageStepPreservation = 10;
        private const double ScrollWheelStepIncrementRel = 1.0/3;

        /// <summary>
        /// Scrolls up within content by one logical unit. 
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - ScrollStepIncrement);
        }

        /// <summary>
        /// Scrolls down within content by one logical unit. 
        /// </summary>
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + ScrollStepIncrement);
        }

        /// <summary>
        /// Scrolls left within content by one logical unit.
        /// </summary>
        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ScrollStepIncrement);
        }

        /// <summary>
        /// Scrolls right within content by one logical unit.
        /// </summary>
        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + ScrollStepIncrement);
        }

        /// <summary>
        /// Scrolls up within content by one page.
        /// </summary>
        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - _ViewPortSize.Height);
        }

        /// <summary>
        /// Scrolls down within content by one page.
        /// </summary>
        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + _ViewPortSize.Height);
        }

        /// <summary>
        /// Scrolls left within content by one page.
        /// </summary>
        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - _ViewPortSize.Width);
        }

        /// <summary>
        /// Scrolls right within content by one page.
        /// </summary>
        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + _ViewPortSize.Width);
        }

        /// <summary>
        /// Scrolls up within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - _ViewPortSize.Height*ScrollWheelStepIncrementRel);
        }

        /// <summary>
        /// Scrolls down within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + _ViewPortSize.Height*ScrollWheelStepIncrementRel);
        }

        /// <summary>
        /// Scrolls left within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - _ViewPortSize.Width*ScrollWheelStepIncrementRel);
        }

        /// <summary>
        /// Scrolls right within content after a user clicks the wheel button on a mouse.
        /// </summary>
        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + _ViewPortSize.Width*ScrollWheelStepIncrementRel);
        }

        /// <summary>
        /// Sets the amount of horizontal offset.
        /// </summary>
        /// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
        public void SetHorizontalOffset(double offset)
        {
            if (offset > _ExtendRect.Width - _ViewPortSize.Width) offset = _ExtendRect.Width - _ViewPortSize.Width;
            if (offset < 0) offset = 0;
            _ScrollOffset.X = offset;
            canvasTransform.X = -_ExtendRect.Left - offset;
            ScrollOwner?.InvalidateScrollInfo();
        }

        /// <summary>
        /// Sets the amount of vertical offset.
        /// </summary>
        /// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
        public void SetVerticalOffset(double offset)
        {
            if (offset > _ExtendRect.Height - _ViewPortSize.Height) offset = _ExtendRect.Height - _ViewPortSize.Height;
            if (offset < 0) offset = 0;
            _ScrollOffset.Y = offset;
            canvasTransform.Y = -_ExtendRect.Top - offset;
            ScrollOwner?.InvalidateScrollInfo();
        }

        /// <summary>
        /// Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual"/> object is visible. 
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Rect"/> that is visible.
        /// </returns>
        /// <param name="visual">A <see cref="T:System.Windows.Media.Visual"/> that becomes visible.</param><param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (visual == null) throw new ArgumentNullException(nameof(visual));
            var ltPoint = visual.PointToScreen(new Point(0, 0));
            ltPoint = partCanvas.PointFromScreen(ltPoint);
            var fe = visual as FrameworkElement;
            if (fe != null)
            {
                // Make sure the center of the visual will be shown.
                ltPoint.Offset(fe.ActualWidth/2, fe.ActualHeight/2);
            }
            // Now the coordinate of visual is relative to the canvas.
            var vb = ViewPortBounds;
            if (!vb.Contains(ltPoint))
            {
                SetHorizontalOffset(ltPoint.X);
                SetVerticalOffset(ltPoint.Y);
            }
            return rectangle;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the vertical axis is possible. 
        /// </summary>
        /// <returns>
        /// true if scrolling is possible; otherwise, false. This property has no default value.
        /// </returns>
        public bool CanVerticallyScroll { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.
        /// </summary>
        /// <returns>
        /// true if scrolling is possible; otherwise, false. This property has no default value.
        /// </returns>
        public bool CanHorizontallyScroll { get; set; }

        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double"/> that represents, in device independent pixels, the horizontal size of the extent. This property has no default value.
        /// </returns>
        public double ExtentWidth => _ExtendRect.Width;

        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double"/> that represents, in device independent pixels, the vertical size of the extent.This property has no default value.
        /// </returns>
        public double ExtentHeight => _ExtendRect.Height;

        /// <summary>
        /// Gets the horizontal size of the viewport for this content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double"/> that represents, in device independent pixels, the horizontal size of the viewport for this content. This property has no default value.
        /// </returns>
        public double ViewportWidth => _ViewPortSize.Width;

        /// <summary>
        /// Gets the vertical size of the viewport for this content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double"/> that represents, in device independent pixels, the vertical size of the viewport for this content. This property has no default value.
        /// </returns>
        public double ViewportHeight => _ViewPortSize.Height;

        /// <summary>
        /// Gets the horizontal offset of the scrolled content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double"/> that represents, in device independent pixels, the horizontal offset. This property has no default value.
        /// </returns>
        public double HorizontalOffset => _ScrollOffset.X;

        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Double"/> that represents, in device independent pixels, the vertical offset of the scrolled content. Valid values are between zero and the <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ExtentHeight"/> minus the <see cref="P:System.Windows.Controls.Primitives.IScrollInfo.ViewportHeight"/>. This property has no default value.
        /// </returns>
        public double VerticalOffset => _ScrollOffset.Y;

        /// <summary>
        /// Gets or sets a <see cref="T:System.Windows.Controls.ScrollViewer"/> element that controls scrolling behavior.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ScrollViewer"/> element that controls scrolling behavior. This property has no default value.
        /// </returns>
        public ScrollViewer ScrollOwner { get; set; }

        #endregion
    }

    /// <summary>
    /// Generates UIElements for GraphicalObjects.
    /// Note <see cref="GraphicalObjectCollection"/> has no indexer.
    /// </summary>
    public class GraphicalObjectContainerGenerator // aka. Factory
    {
        /// <summary>
        /// An attached property for item container, set to the corrspoinding source item.
        /// </summary>
        private static readonly DependencyProperty DataItemProperty =
            DependencyProperty.RegisterAttached("DataItem", typeof (object), typeof (GraphicalObjectContainerGenerator),
                new FrameworkPropertyMetadata(null));

        private readonly Dictionary<GraphicalObject, DependencyObject> itemContainerDict =
            new Dictionary<GraphicalObject, DependencyObject>();

        #region Container Pool

        private List<DependencyObject> containerPool = new List<DependencyObject>();
        private int _MaxPooledContainers;

        /// <summary>
        /// Specifies the maximum number of containers that can exist in the pool.
        /// </summary>
        public int MaxPooledContainers
        {
            get { return _MaxPooledContainers; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException();
                ShrinkContainerPool();
                _MaxPooledContainers = value;
            }
        }

        private void ShrinkContainerPool()
        {
            var exceededItems = containerPool.Count - MaxPooledContainers;
            if (exceededItems > 0)
                containerPool.RemoveRange(containerPool.Count - exceededItems, exceededItems);
        }

        #endregion

        private DependencyObject CreateContainer()
        {
            DependencyObject container;
            if (containerPool.Count > 0)
            {
                container = containerPool[containerPool.Count - 1];
                containerPool.RemoveAt(containerPool.Count - 1);
            }
            else
            {
                container = new DesignerCanvasItem();
            }
            return container;
        }

        /// <summary>
        /// Gets a new or pooled container for a specific GraphicalObject.
        /// </summary>
        public DependencyObject CreateContainer(GraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var container = CreateContainer();
            PrepareContainer(container, item);
            itemContainerDict.Add(item, container);
            return container;
        }

        private void PrepareContainer(DependencyObject container, GraphicalObject item)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.SetValue(DataItemProperty, item);
            container.SetValue(FrameworkElement.DataContextProperty, item);
        }

        /// <summary>
        /// Declares a container no longer be used and should be pooled or discarded.
        /// </summary>
        public void Recycle(DependencyObject container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.ClearValue(DataItemProperty);
            container.ClearValue(FrameworkElement.DataContextProperty);
            if (containerPool.Count < MaxPooledContainers)
            {
                containerPool.Add(container);
            }
        }

        /// <summary>
        /// Gets the container, if generated, for a specific item.
        /// </summary>
        /// <returns></returns>
        public DependencyObject ContainerFromItem(GraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            DependencyObject container;
            if (itemContainerDict.TryGetValue(item, out container))
                return container;
            return null;
        }

        /// <summary>
        /// Gets the corresponding item, if exists, for a specific container.
        /// </summary>
        /// <returns></returns>
        public GraphicalObject ItemFromContainer(DependencyObject container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return (GraphicalObject) container.ReadLocalValue(DataItemProperty);
        }
    }
}
