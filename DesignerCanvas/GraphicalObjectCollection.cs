using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Undefined.DesignerCanvas
{
    /// <summary>
    /// 为设计图面上的对象集合提供基本操作。
    /// 为了优化性能，此集合的内部实现可能会在未来进行调整。
    /// 因此目前仅公开必要的集合操作。
    /// </summary>
    public class GraphicalObjectCollection : ICollection<IGraphicalObject>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private readonly HashSet<IGraphicalObject> myCollection = new HashSet<IGraphicalObject>();

        /// <summary>
        /// 获取指定区域内的所有对象。
        /// </summary>
        /// <param name="bounds">要返回其内部对象的选框。</param>
        public IEnumerable<IGraphicalObject> ObjectsInRegion(Rect bounds)
        {
            return ObjectsInRegion(bounds, ItemSelectionOptions.None);
        }

        /// <summary>
        /// 获取指定区域内的所有对象。
        /// </summary>
        /// <param name="bounds">要返回其内部对象的选框。</param>
        /// <param name="includePartialSelection">在返回的集合中包括与选框相交的对象。</param>
        public IEnumerable<IGraphicalObject> ObjectsInRegion(Rect bounds, ItemSelectionOptions options)
        {
            if (bounds.IsEmpty || bounds.Width == 0 || bounds.Height == 0) return Enumerable.Empty<GraphicalObject>();
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

    [Flags]
    public enum ItemSelectionOptions
    {
        None = 0,
        IncludePartialSelection = 1,
        PerformHitTest = 2,
    }
}
