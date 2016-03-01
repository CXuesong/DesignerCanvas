using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// 用于承载绘图图面。
    /// </summary>
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    public class DesignerCanvas : Control
    {
        private readonly GraphicalObjectCollection _Items = new GraphicalObjectCollection();
        private readonly GraphicalObjectCollection _SelectedItems = new GraphicalObjectCollection();
        private readonly GraphicalObjectContainerGenerator _ItemContainerGenerator = new GraphicalObjectContainerGenerator();

        #region Items & States

        public GraphicalObjectCollection Items => _Items;

        public GraphicalObjectCollection SelectedItems => _SelectedItems;

        //public GraphicalObjectContainerGenerator ItemContainerGenerator => _ItemContainerGenerator;

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
                                var container = _ItemContainerGenerator.ContainerFromItem((GraphicalObject)item);
                                if (container != null) container.IsSelected = false;
                            }
                        }
                        if (e.NewItems != null)
                        {
                            foreach (var item in e.NewItems)
                            {
                                var container = _ItemContainerGenerator.ContainerFromItem((GraphicalObject)item);
                                if (container != null) container.IsSelected = true;
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        var containers = partCanvas.Children.OfType<DesignerCanvasItem>().ToList();
                        foreach (var item in SelectedItems)
                        {
                            var container = _ItemContainerGenerator.ContainerFromItem(item);
                            container.IsSelected = true;
                            containers.Remove(container);
                        }
                        foreach (var item in containers)
                        {
                            item.IsSelected = false;
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
                            var container = _ItemContainerGenerator.ContainerFromItem((GraphicalObject)item);
                            if (container != null) partCanvas.Children.Remove(container);
                            _ItemContainerGenerator.Recycle(container);
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            partCanvas.Children.Add(_ItemContainerGenerator.CreateContainer((GraphicalObject)item));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in partCanvas.Children)
                    {
                        _ItemContainerGenerator.Recycle((DesignerCanvasItem)item);
                    }
                    partCanvas.Children.Clear();
                    foreach (var item in _Items)
                    {
                        partCanvas.Children.Add(_ItemContainerGenerator.CreateContainer(item));
                    }
                    break;
            }
        }
        #endregion

        #region UI

        private Canvas partCanvas;

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
            partCanvas.MouseDown += PartCanvas_MouseDown;
        }

        private void PartCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == partCanvas)
            {
                _SelectedItems.Clear();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            const double measurementMargin = 10;
            var bounds = _Items.Bounds;
            return new Size(Math.Max(0, bounds.Right + measurementMargin),
                Math.Max(0, bounds.Bottom + measurementMargin));
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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvas), new FrameworkPropertyMetadata(typeof(DesignerCanvas)));
        }
    }

    public class GraphicalObjectContainerGenerator    // aka. Factory
    {
        private Dictionary<GraphicalObject, DesignerCanvasItem> itemContainerDict =
            new Dictionary<GraphicalObject, DesignerCanvasItem>();

        #region Container Pool
        private List<DesignerCanvasItem> containerPool = new List<DesignerCanvasItem>();
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

        private DesignerCanvasItem CreateContainer()
        {
            DesignerCanvasItem container;
            if (containerPool.Count > 0)
            {
                container = containerPool[containerPool.Count - 1];
                containerPool.RemoveAt(containerPool.Count - 1);
            }
            else
            {
                container = new DesignerCanvasItem();
                container.SetBinding(Canvas.LeftProperty, new Binding("Left"));
                container.SetBinding(Canvas.TopProperty, new Binding("Top"));
                container.SetBinding(FrameworkElement.WidthProperty, new Binding("Width"));
                container.SetBinding(FrameworkElement.HeightProperty, new Binding("Height"));
                container.SetBinding(DesignerCanvasItem.ImageProperty, new Binding("Image"));
            }
            return container;
        }

        /// <summary>
        /// Gets a new or pooled container for a specific GraphicalObject.
        /// </summary>
        public DesignerCanvasItem CreateContainer(GraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var container = CreateContainer();
            PrepareContainer(container, item);
            itemContainerDict.Add(item, container);
            return container;
        }

        private void PrepareContainer(DesignerCanvasItem container, GraphicalObject item)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.DataContext = item;
        }

        /// <summary>
        /// Declares a container no longer be used and should be pooled or discarded.
        /// </summary>
        public void Recycle(DesignerCanvasItem container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var dc = container.DataContext as GraphicalObject;
            if (dc != null) itemContainerDict.Remove(dc);
            container.DataContext = null;
            if (containerPool.Count < MaxPooledContainers)
            {
                containerPool.Add(container);
            }
        }

        /// <summary>
        /// Gets the container, if generated, for a specific item.
        /// </summary>
        /// <returns></returns>
        public DesignerCanvasItem ContainerFromItem(GraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            DesignerCanvasItem container;
            if (itemContainerDict.TryGetValue(item, out container))
                return container;
            return null;
        }

        /// <summary>
        /// Gets the corresponding item, if exists, for a specific container.
        /// </summary>
        /// <returns></returns>
        public GraphicalObject ItemFromContainer(DesignerCanvasItem container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return container.DataContext as GraphicalObject;
        }
    }
}
