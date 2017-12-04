using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revolution
{
    public enum PoolResults {
        Ok,
        ObjectAlreadyExists
    }


public class ComponentDoesNotExistException<T> : Exception
    {
        public ComponentDoesNotExistException(int entity) : base($"Component {typeof(T).Name} not found on Entity {entity}") { }
    }

    public class Pool<T> where T : IPoolable, new()
    {
        public static Pool<T> Instance { get
            {
                if (_instance == null) _instance = new Pool<T>();
                return _instance;
            }
        }
        private static Pool<T> _instance;
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
                throw new ComponentDoesNotExistException<T>(entity);
            }
        }

        public PoolResults Create(int entity)
        {
            lock (_sync)
            {
                T item;
                if (!componentsInUse.TryGetValue(entity, out item))
                {

                    if (_items.Count == 0)
                    {
                        item = new T();
                    }
                    else
                    {
                        item = _items.Pop();
                    }
                    componentsInUse[entity] = item;
                    return PoolResults.Ok;
                }
                return PoolResults.ObjectAlreadyExists;
            }
        }

        public PoolResults Free(int entity)
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
                return PoolResults.Ok;
            }
        }
    }
}
