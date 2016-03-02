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
    public class GraphicalObjectCollection : ICollection<GraphicalObject>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private LinkedList<GraphicalObject> myCollection = new LinkedList<GraphicalObject>();

        /// <summary>
        /// 获取指定区域内的所有对象。
        /// </summary>
        /// <param name="bounds">要返回其内部对象的选框。</param>
        public IEnumerable<GraphicalObject> ObjectsInRegion(Rect bounds)
        {
            return ObjectsInRegion(bounds, false);
        }

        /// <summary>
        /// 获取指定区域内的所有对象。
        /// </summary>
        /// <param name="bounds">要返回其内部对象的选框。</param>
        /// <param name="includePartialSelection">在返回的集合中包括与选框相交的对象。</param>
        public IEnumerable<GraphicalObject> ObjectsInRegion(Rect bounds, bool includePartialSelection)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return Enumerable.Empty<GraphicalObject>();
            if (includePartialSelection)
            {
                return myCollection.Where(obj =>
                {
                    var b = obj.Bounds;
                    return bounds.Contains(obj.Bounds) || bounds.IntersectsWith(obj.Bounds);
                });
            }
            else
            {
                return myCollection.Where(obj => bounds.Contains(obj.Bounds));
            }
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
        public IEnumerator<GraphicalObject> GetEnumerator()
            => myCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => myCollection.GetEnumerator();

        /// <summary>
        /// 向集合添加一个新项目。
        /// </summary>
        /// <param name="item">不可为<c>null</c>。</param>
        public void Add(GraphicalObject item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            myCollection.AddLast(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
        }

        public void AddRange(IEnumerable<GraphicalObject> items)
        {
            const double batchNotificationItems = 128;
            var tempList = new List<GraphicalObject>();
            foreach (var obj in items)
            {
                myCollection.AddLast(obj);
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

        public bool Contains(GraphicalObject item)
            => myCollection.Contains(item);

        public void CopyTo(GraphicalObject[] array, int arrayIndex)
            => myCollection.CopyTo(array, arrayIndex);

        public bool Remove(GraphicalObject item)
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
}
