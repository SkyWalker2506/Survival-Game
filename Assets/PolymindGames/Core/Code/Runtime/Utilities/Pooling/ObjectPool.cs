using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolymindGames.PoolingSystem
{
    public abstract class ObjectPool
    {
        public abstract PoolableObject GetInstance();
        public abstract void ReleaseInstance(PoolableObject instance);
        public abstract void ReleaseInstances(int keep = 0);
        public abstract void Populate(int amount);
        public abstract void Dispose();
    }

    public sealed class ObjectPool<T> : ObjectPool where T : Component
    {
        private readonly Stack<PoolableObject> _pool;
        private readonly PoolableObject _template;
        private readonly Transform _parent;
        private readonly int _capacity;


        public ObjectPool(T template, int capacity, Transform parent, float autoReleaseDelay = Mathf.Infinity)
        {
#if DEBUG
            if (template == null || capacity < 1)
            {
                Debug.LogError("You want to create an object pool for an object that is null!");
                return;
            }
#endif
            _parent = parent;
            _capacity = capacity;
            _pool = new Stack<PoolableObject>(_capacity);

            var obj = Object.Instantiate(template.gameObject);

            if (obj.TryGetComponent(out _template))
                _template.Init<T>(this);
            else
            {
                _template = obj.AddComponent<PoolableObject>();
                _template.Init<T>(this);
                _template.AutoReleaseDelay = autoReleaseDelay;
            }

            _template.gameObject.SetActive(false);
            _template.transform.parent = _parent;
        }

        public T GetInstanceComponent() => GetInstance().GetCachedComponent<T>();

        public override PoolableObject GetInstance()
        {
            PoolableObject instance = GetPooledInstance();
            if (instance == null)
                instance = CreateInstance();

            instance.gameObject.SetActive(true);
            return instance;
        }

        public override void ReleaseInstance(PoolableObject instance)
        {
#if UNITY_EDITOR
            if (instance == null)
            {
                Debug.LogError("The object you want to return is null!");
                return;
            }
#endif

            if (_pool.Count == _capacity)
            {
                Object.Destroy(instance.gameObject);
                return;
            }

            _pool.Push(instance);

            instance.transform.SetParent(_parent);
            instance.gameObject.SetActive(false);
        }

        public override void ReleaseInstances(int keep = 0)
        {
            if (keep < 0 || keep > _capacity)
                throw new ArgumentOutOfRangeException(nameof(keep));

            if (keep != 0)
            {
                for (int i = _pool.Count - keep; i > 0; i--)
                    Object.Destroy(_pool.Pop().gameObject);
            }
            else
            {
                while (_pool.Count > 0)
                    Object.Destroy(_pool.Pop().gameObject);
            }
        }

        public override void Populate(int amount)
        {
            amount = Mathf.Min(amount, _capacity - 1);
            while (amount > _pool.Count)
                _pool.Push(CreateInstance());
        }

        private PoolableObject GetPooledInstance()
        {
            PoolableObject instance = null;
            while (_pool.Count > 0 && instance == null)
                instance = _pool.Pop();

            return instance;
        }

        private PoolableObject CreateInstance()
        {
            var instance = Object.Instantiate(_template, _parent);
            instance.Init<T>(this);
            return instance;
        }

        public override void Dispose()
        {
            foreach (var item in _pool)
            {
                if (item != null)
                    Object.Destroy(item.gameObject);
            }

            _pool.Clear();
        }
    }
}