using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolution
{
    public class Pool<T> where T : IPoolable, new()
    {
        private Stack<T> _items = new Stack<T>();
        private Dictionary<int, T> componentsInUse = new Dictionary<int, T>();
        private object _sync = new object();

        public T Get(int entity)
        {
            lock(_sync)
            {
                T item;
                if (componentsInUse.TryGetValue(entity, out item))
                {
                    return item;
                }
                if (_items.Count == 0)
                {
                    return new T();
                }
                return _items.Pop();
            }
        }

        public void Free(int entity)
        {
            lock(_sync)
            {
                T item;
                if (componentsInUse.TryGetValue(entity, out item))
                {
                    item.Release();
                    _items.Push(item);
                    componentsInUse.Remove(entity);
                }
            }
        }
    }
}
