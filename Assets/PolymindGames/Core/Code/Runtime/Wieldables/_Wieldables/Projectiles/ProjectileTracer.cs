using PolymindGames.PoolingSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(PoolableObject))]
    public sealed class ProjectileTracer : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 100f), BeginGroup("Settings")]
        [Tooltip("The maximum amount of time the projectile can remain in the air without colliding. After this time, it will be returned to the pool or destroyed.")]
        private float _maxAirTime = 2f;

        [SerializeField, Range(0, 1f), EndGroup]
        [Tooltip("The delay before enabling the tracer effect after the projectile is fired.")]
        private float _tracerEnableDelay = 0.1f;

        [SerializeField, NotNull, BeginGroup("References")]
        [Tooltip("The TrailRenderer component used to visualize the projectile's path.")]
        private TrailRenderer _trailRenderer;

        [SerializeField, NotNull, EndGroup]
        [Tooltip("The ParticleSystem component used to visualize the projectile's path.")]
        private ParticleSystem _particleSystem;

        private PoolableObject _poolableObject;
        private Vector3 _currentPosition;
        private Vector3 _endPosition;
        private float _speed;
        private float _startTime;
        private float _trailEnableTimer;


        public void DoTracer(Vector3 start, Vector3 end, float speed)
        {
            _trailEnableTimer = Time.fixedTime + _tracerEnableDelay;

            transform.position = start;
            _currentPosition = start;
            _endPosition = end;

            _startTime = Time.fixedTime;
            _speed = speed;

            _trailRenderer.Clear();
            _poolableObject.ReleaseObject(_maxAirTime);

            enabled = true;
        }

        private void FixedUpdate()
        {
            float time = Time.fixedTime;

            if (_trailEnableTimer <= time)
            {
                _trailRenderer.emitting = true;
                if (_particleSystem != null)
                    _particleSystem.Play(false);

                _trailEnableTimer = float.MaxValue;
            }

            if (time - _startTime > _maxAirTime)
            {
                _trailRenderer.emitting = false;
                _particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                enabled = false;
            }
        }

        private void Update()
        {
            _currentPosition = Vector3.MoveTowards(_currentPosition, _endPosition, _speed * Time.deltaTime);
            transform.position = _currentPosition;

            if (Vector3.Distance(_currentPosition, _endPosition) < 0.05f)
            {
                _trailRenderer.Clear();
                _particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                _poolableObject.ReleaseObject(0.1f);
                enabled = false;
            }
        }

        private void Awake()
        {
            _trailRenderer.emitting = false;
            _poolableObject = GetComponent<PoolableObject>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_particleSystem == null)
                _particleSystem = GetComponent<ParticleSystem>();

            if (_trailRenderer == null)
                _trailRenderer = GetComponent<TrailRenderer>();
        }
#endif
    }
}