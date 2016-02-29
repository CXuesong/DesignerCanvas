using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class GraphicalObjectCollection : ICollection<GraphicalObject>
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

        #region 标准集合操作
        public IEnumerator<GraphicalObject> GetEnumerator()
            => myCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => myCollection.GetEnumerator();

        /// <summary>
        /// 向集合添加一个新项目。
        /// </summary>
        /// <param name="item">不可为<c>null</c>。</param>
        public void Add(GraphicalObject item)
            => myCollection.AddLast(item);

        public void Clear()
            => myCollection.Clear();

        public bool Contains(GraphicalObject item)
            => myCollection.Contains(item);

        public void CopyTo(GraphicalObject[] array, int arrayIndex)
            => myCollection.CopyTo(array, arrayIndex);

        public bool Remove(GraphicalObject item)
            => myCollection.Remove(item);

        public int Count => myCollection.Count;

        public bool IsReadOnly => false;
        #endregion
    }
}
