using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Undefined.DesignerCanvas.ObjectModel
{
    /// <summary>
    /// Provides basic operations for a collection of <see cref="Entity"/>s & <see cref="Connection"/>s.
    /// The implementation of this class may be subject to change for a better performance. (E.g. R-Tree)
    /// Thus for now only foundamental operations are provided, and this class doesn't implement <see cref="IList"/>.
    /// </summary>
    public class GraphicalObjectCollection : ICollection<IGraphicalObject>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private readonly HashSet<IGraphicalObject> myCollection = new HashSet<IGraphicalObject>();

        /// <summary>
        /// Gets all <see cref="IGraphicalObject"/> contained in the specified rectangle region.
        /// </summary>
        public IEnumerable<IGraphicalObject> ObjectsInRegion(Rect bounds)
        {
            return ObjectsInRegion(bounds, ItemSelectionOptions.None);
        }

        /// <summary>
        /// Gets all <see cref="IGraphicalObject"/> contained in or intersecting with the specified rectangle region.
        /// </summary>
        public IEnumerable<IGraphicalObject> ObjectsInRegion(Rect bounds, ItemSelectionOptions options)
        {
            if (bounds.IsEmpty || bounds.Width == 0 || bounds.Height == 0) return Enumerable.Empty<Entity>();
            var query = ((options & ItemSelectionOptions.IncludePartialSelection) == ItemSelectionOptions.IncludePartialSelection)
                ? myCollection.Where(obj => bounds.IntersectsWith(obj.Bounds))
                : myCollection.Where(obj => bounds.Contains(obj.Bounds));
            if ((options & ItemSelectionOptions.PerformHitTest) == ItemSelectionOptions.PerformHitTest)
            {
                if ((options & ItemSelectionOptions.IncludePartialSelection) ==
                    ItemSelectionOptions.IncludePartialSelection)
                    query = query.Where(obj => obj.HitTest(bounds) != HitTestResult.None);
                else
                    query = query.Where(obj => obj.HitTest(bounds) == HitTestResult.Contains);
            }
            return query;
        }

        /// <summary>
        /// Gets the unioned boundary of all items.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                if (myCollection.Count == 0) return Rect.Empty;
                var rect = myCollection.First().Bounds;
                foreach (var obj in myCollection)
                {
                    rect.Union(obj.Bounds);
                }
                return rect;
            }
        }

        #region ICollection
        public IEnumerator<IGraphicalObject> GetEnumerator()
            => myCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => myCollection.GetEnumerator();

        /// <summary>
        /// 向集合添加一个新项目。
        /// </summary>
        /// <param name="item">不可为<c>null</c>。</param>
        public void Add(IGraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            myCollection.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public void AddRange(IEnumerable<IGraphicalObject> items)
        {
            const double batchNotificationItems = 128;
            var tempList = new List<IGraphicalObject>();
            foreach (var obj in items)
            {
                myCollection.Add(obj);
                tempList.Add(obj);
                if (tempList.Count >= batchNotificationItems)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempList));
                    tempList.Clear();
                }
            }
            if (tempList.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tempList));
            }
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public void Clear()
        {
            if (myCollection.Count == 0) return;
            myCollection.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public bool Contains(IGraphicalObject item)
            => myCollection.Contains(item);

        public void CopyTo(IGraphicalObject[] array, int arrayIndex)
            => myCollection.CopyTo(array, arrayIndex);

        public bool Remove(IGraphicalObject item)
        {
            // NOTE: this is an O(n) operation!
            var result = myCollection.Remove(item);
            if (result)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged("Item[]");
            }
            return result;
        }

        public int Count => myCollection.Count;

        public bool IsReadOnly => false;

        #endregion

        #region INotifyPropertyChanged & INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// Used by <see cref="GraphicalObjectCollection.ObjectsInRegion"/>.
    /// </summary>
    [Flags]
    public enum ItemSelectionOptions
    {
        None = 0,
        /// <summary>
        /// Includes the object intersecting with the specified region.
        /// </summary>
        IncludePartialSelection = 1,
        /// <summary>
        /// Performs hittesting for every object intersecting with the specified region.
        /// This operation might be slow.
        /// </summary>
        PerformHitTest = 2,
    }
}