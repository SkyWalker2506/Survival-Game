using UnityEngine;

namespace PolymindGames.PoolingSystem
{
    public sealed class PoolableObject : MonoBehaviour
    {
        [SerializeField, Range(0f, 1000f), BeginGroup, EndGroup]
        [Tooltip("The time it takes for this object to return to the pool or self-destruct if it's not part of one.")]
        private float _autoReleaseDelay = 10f;

        private Component _cachedComponent;
        private ObjectPool _parentPool;
        private float _releaseTimer;

        
        public float AutoReleaseDelay
        {
            get => _autoReleaseDelay;
            set
            {
                _autoReleaseDelay = value;
                _releaseTimer = Time.fixedTime + value;
            }
        }

        internal void Init<T>(ObjectPool pool) where T : Component
        {
            if (_parentPool != null)
            {
                Debug.LogError("You are attempting to initialize a poolable object, but it's already initialized!!");
                return;
            }

            _parentPool = pool;
            _cachedComponent = GetComponent<T>();
        }

        internal T GetCachedComponent<T>() where T : Component
        {
            return (T)_cachedComponent;
        }

        public void ResetReleaseDelay() => _releaseTimer = Time.fixedTime + _autoReleaseDelay;

        /// <summary>
        /// Release the object after a delay.
        /// </summary>
        public void ReleaseObject(float delay) => _releaseTimer = Time.fixedTime + delay;

        /// <summary>
        /// Send the object back to the pool or destroy it if it's not part of one.
        /// </summary>
        public void ReleaseObject()
        {
            if (_parentPool != null)
                _parentPool.ReleaseInstance(this);
            else
                Destroy(gameObject);
        }

        private void OnEnable() => _releaseTimer = Time.fixedTime + _autoReleaseDelay;

        private void FixedUpdate()
        {
            if (_releaseTimer < Time.fixedTime)
                ReleaseObject();
        }
    }
}