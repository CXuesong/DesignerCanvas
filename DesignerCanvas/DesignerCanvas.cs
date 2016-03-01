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
    public class DesignerCanvas : Canvas
    {
        private readonly GraphicalObjectCollection _Items = new GraphicalObjectCollection();
        private readonly GraphicalObjectCollection _SelectedItems = new GraphicalObjectCollection();
        private readonly GraphicalObjectContainerFactory containerFactory = new GraphicalObjectContainerFactory();

        public GraphicalObjectCollection Items => _Items;

        public GraphicalObjectCollection SelectedItems => _SelectedItems;

        public DesignerCanvas()
        {
            _Items.CollectionChanged += _Items_CollectionChanged;
            _SelectedItems.CollectionChanged += _SelectedItems_CollectionChanged;
        }

        private void _SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            var container = containerFactory.ContainerFromItem((GraphicalObject) item);
                            if (container != null) this.Children.Remove(container);
                            containerFactory.Recycle(container);
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            this.Children.Add(containerFactory.GetContainer((GraphicalObject) item));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in this.Children)
                    {
                        containerFactory.Recycle((DesignerCanvasItem) item);
                    }
                    this.Children.Clear();
                    break;
            }
        }

        static DesignerCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvas), new FrameworkPropertyMetadata(typeof(DesignerCanvas)));
        }
    }

    internal class GraphicalObjectContainerFactory
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

        private DesignerCanvasItem GetContainer()
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
        public DesignerCanvasItem GetContainer(GraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var container = GetContainer();
            PrepareContainer(container, item);
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
