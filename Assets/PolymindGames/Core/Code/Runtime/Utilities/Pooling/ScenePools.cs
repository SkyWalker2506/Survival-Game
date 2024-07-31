using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.PoolingSystem
{
    public static class ScenePools
    {
        private static ScenePoolHandler s_ScenePoolHandler;
        private static bool s_HasScenePoolHandler;
        
        private const int DEFAULT_POOL_CAPACITY = 16;


        public static Transform ScenePoolHandlerRoot => PoolHandler.transform;
        
        private static ScenePoolHandler PoolHandler
        {
            get
            {
                if (!s_HasScenePoolHandler)
                {
                    var poolHandlerObject = new GameObject("ScenePools");
                    s_ScenePoolHandler = poolHandlerObject.AddComponent<ScenePoolHandler>();
                    s_HasScenePoolHandler = true;
                }

                return s_ScenePoolHandler;
            }
        }
        
        public static event UnityAction ScenePoolHandlerDestroyed;

        public static ObjectPool<T> CreatePool<T>(T template, int initialInstances, int capacity, string poolCategoryName = "", float autoReleaseDelay = Mathf.Infinity)
            where T : Component
        {
#if DEBUG
            if (template == null)
            {
                Debug.LogError("You want to create an object pool for an object that is null!");
                return null;
            }
#endif

            var poolHandler = PoolHandler;
            if (poolHandler.Pools.TryGetValue(template.GetHashCode(), out var pool))
                return (ObjectPool<T>)pool;
            
            Transform poolCategoryRoot;

            if (string.IsNullOrEmpty(poolCategoryName))
                poolCategoryRoot = poolHandler.transform;
            else if (!poolHandler.PoolCategories.TryGetValue(poolCategoryName, out poolCategoryRoot))
            {
                var poolCategoryObj = new GameObject(poolCategoryName);
                poolCategoryRoot = poolCategoryObj.transform;
                poolCategoryRoot.SetParent(poolHandler.transform);
                poolHandler.PoolCategories.Add(poolCategoryName, poolCategoryRoot);
            }

            pool = new ObjectPool<T>(template, capacity, poolCategoryRoot, autoReleaseDelay);
            pool.Populate(initialInstances);
            poolHandler.Pools.Add(template.GetHashCode(), pool);

            return (ObjectPool<T>)pool;
        }

        public static bool ContainsPool<T>(T template) where T : Component =>
            PoolHandler.Pools.TryGetValue(template.GetHashCode(), out _);

        public static bool TryGetPool<T>(T template, out ObjectPool<T> pool) where T : Component
        {
            if (PoolHandler.Pools.TryGetValue(template.GetHashCode(), out var foundPool))
            {
                pool = (ObjectPool<T>)foundPool;
                return true;
            }

            pool = null;
            return false;
        }

        public static ObjectPool<T> GetPool<T>(T template) where T : Component
        {
            return PoolHandler.Pools.TryGetValue(template.GetHashCode(), out var pool) ? (ObjectPool<T>)pool : null;
        }

        public static T GetObject<T>(T template, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            var instance = GetObject(template);

            instance.transform.SetPositionAndRotation(position, rotation);
            if (parent is not null)
                instance.transform.SetParent(parent, true);

            return instance;
        }

        public static T GetObject<T>(T template)
            where T : Component
        {
            if (PoolHandler.Pools.TryGetValue(template.GetHashCode(), out var pool))
                return pool.GetInstance().GetCachedComponent<T>();

            var newPool = CreatePool(template, 0, DEFAULT_POOL_CAPACITY);
            PoolHandler.Pools.Add(template.GetHashCode(), newPool);

            return newPool.GetInstance().GetCachedComponent<T>();
        }
        
        private sealed class ScenePoolHandler : MonoBehaviour
        {
            public Dictionary<string, Transform> PoolCategories = new();
            public Dictionary<int, ObjectPool> Pools = new();

            
            private void OnDestroy()
            {
                s_HasScenePoolHandler = false;
                ScenePoolHandlerDestroyed?.Invoke();

                foreach (var pool in Pools.Values)
                    pool.Dispose();

                Pools = null;
                PoolCategories = null;
            }
        }
    }
}