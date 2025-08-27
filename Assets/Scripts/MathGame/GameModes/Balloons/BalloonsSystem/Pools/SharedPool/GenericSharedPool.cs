using System.Collections.Generic;
using VContainer;
using UnityEngine;
using VContainer.Unity;

namespace Pools.SharedPool
{
    public class GenericSharedPool<T> where T : Component
    {
        private readonly Dictionary<T, Pool<T>> _pools = new();
        private readonly IObjectResolver _container;

        public GenericSharedPool(IObjectResolver container)
        {
            _container = container;
        }

        public T Take(T prefab, Transform parent = null, bool shouldUseInject = true)
        {
            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Pool<T>(prefab, _container, parent, shouldUseInject);
            }

            return _pools[prefab].Take();
        }

        public void Release(T instance)
        {
            if (instance is IPoolable poolable)
            {
                poolable.Release();
            }
            else
            {
                Debug.LogError("Released object does not implement IPoolable interface.");
            }
        }
    }

    public class Pool<T> where T : Component
    {
        private readonly IObjectResolver _container;
        private readonly T _prefab;
        private readonly Queue<T> _items = new();
        private readonly Transform _parent;
        private readonly bool _shouldUseInject;

        public Pool(T prefab, IObjectResolver container, Transform parent, bool shouldUseInject)
        {
            _container = container;
            _prefab = prefab;
            _parent = parent;
            _shouldUseInject = shouldUseInject;
        }

        public T Take()
        {
            if (_items.Count == 0)
            {
                CreateNewItemInPool();
            }

            T item = _items.Dequeue();
            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return item;
        }

        public void Release(T item)
        {
            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
                _items.Enqueue(item);
            }
            else
            {
                Debug.LogError("Released object does not implement IPoolable interface.");
            }
        }

        private void CreateNewItemInPool()
        {
            var newInstance = _shouldUseInject
                ? _container.Instantiate(_prefab, _parent)
                : Object.Instantiate(_prefab, _parent);
            
            if (newInstance is IPoolable poolable)
            {
                poolable.InitializePool(this);
                _items.Enqueue(newInstance);
            }
            else
            {
                Debug.LogError("Created object does not implement IPoolable interface.");
            }
        }
    }

    public interface IPoolable
    {
        void InitializePool(object parentPool);
        void OnSpawn();
        void OnDespawn();
        void Release();
    }
}