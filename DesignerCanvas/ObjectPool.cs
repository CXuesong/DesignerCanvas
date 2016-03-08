using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Undefined.DesignerCanvas
{
    internal class ObjectPool<T>
    {
        private Func<T> _OnCreateObject;
        private int _Capacity = 10;
        private List<T> pool = new List<T>();

        public ObjectPool(Func<T> onCreateObject)
        {
            if (onCreateObject == null) throw new ArgumentNullException(nameof(onCreateObject));
            _OnCreateObject = onCreateObject;
        }

        /// <summary>
        /// Specifies the maximum number of objects that can exist in the pool.
        /// </summary>
        public int Capacity
        {
            get { return _Capacity; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException();
                Shrink();
                _Capacity = value;
                pool.Capacity = value;
            }
        }

        public int PooledCount => pool.Count;

        private void Shrink()
        {
            var exceededItems = pool.Count - Capacity;
            if (exceededItems > 0)
                pool.RemoveRange(pool.Count - exceededItems, exceededItems);
        }

        public T Take()
        {
            if (pool.Count == 0) return _OnCreateObject();
            var item = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            return item;
        }

        public bool PutBack(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (pool.Count < _Capacity)
            {
                pool.Add(obj);
                return true;
            }
            return false;
        }
    }
}
