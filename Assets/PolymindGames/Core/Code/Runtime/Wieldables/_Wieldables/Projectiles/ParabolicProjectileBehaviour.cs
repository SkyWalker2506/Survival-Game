using PolymindGames.WieldableSystem;
using PolymindGames.PoolingSystem;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    [RequireComponent(typeof(PoolableObject))]
    public abstract class ParabolicProjectileBehaviour : ProjectileBehaviour
    {
        [SerializeField, BeginGroup]
        private bool _matchRotation;

        [SerializeField, Range(0.1f, 100f), EndGroup]
        [Tooltip("If the projectile remains in the air for this specified amount of time without a collision, it will be returned to the pool or destroyed.")]
        private float _maxAirTime = 5f;
        
        private IFirearmProjectileEffect _effect;
        private PoolableObject _poolableObject;
        private UnityAction _hitCallback;
        private Vector3 _lerpStartPos;
        private Vector3 _lerpTargetPos;
        private Vector3 _startDirection;
        private Vector3 _startPosition;
        private RaycastHit _hit;
        private float _gravity;
        private float _speed;
        private float _startTime;


        protected Transform CachedTransform { get; private set; }
        protected ICharacter Character { get; private set; }
        protected bool InAir { get; private set; }

        public sealed override void Launch(ICharacter character, Vector3 origin, Vector3 velocity, IFirearmProjectileEffect effect,
            float gravity = 9.81f, bool instantStart = false, UnityAction hitCallback = null)
        {
            _startPosition = origin;
            _lerpStartPos = origin;
            _lerpTargetPos = origin;
            _startDirection = velocity.normalized;
            _speed = velocity.magnitude;
            _gravity = gravity;
            _hitCallback = hitCallback;

            Character = character;
            _effect = effect;

            _startTime = -1f;

            _poolableObject.ReleaseObject(_maxAirTime);

            InAir = true;
            OnLaunched();

            if (instantStart)
                FixedUpdate();
        }

        protected virtual void Awake()
        {
            CachedTransform = transform;
            _poolableObject = GetComponent<PoolableObject>();
        }

        protected virtual void FixedUpdate()
        {
            if (!InAir)
                return;

            if (_startTime < 0f)
                _startTime = Time.time;

            float currentTime = Time.time - _startTime;
            float nextTime = currentTime + Time.fixedDeltaTime;

            Vector3 currentPoint = EvaluateParabola(currentTime);
            Vector3 nextPoint = EvaluateParabola(nextTime);

            Vector3 direction = nextPoint - currentPoint;
            float distance = direction.magnitude;
            var ray = new Ray(currentPoint, direction);

            if (PhysicsUtils.RaycastOptimized(ray, distance, out _hit, LayerConstants.ALL_SOLID_OBJECTS_MASK, Character.transform, QueryTriggerInteraction.UseGlobal))
            {
                _effect?.DoHitEffect(ref _hit, ray.direction, direction.magnitude, (_startPosition - _hit.point).magnitude);
                _poolableObject.ResetReleaseDelay();

                InAir = false;
                _hitCallback?.Invoke();
                OnHit(ref _hit);
            }
            else
            {
                _lerpStartPos = currentPoint;
                _lerpTargetPos = nextPoint;
            }
        }

        protected virtual void Update()
        {
            if (!InAir)
                return;

            float delta = Time.time - Time.fixedTime;
            if (delta < Time.fixedDeltaTime)
            {
                float t = delta / Time.fixedDeltaTime;
                CachedTransform.localPosition = Vector3.Lerp(_lerpStartPos, _lerpTargetPos, t);
            }
            else
                CachedTransform.localPosition = _lerpTargetPos;

            if (_matchRotation)
            {
                var velocity = _lerpTargetPos - _lerpStartPos;

                if (velocity != Vector3.zero)
                    CachedTransform.rotation = Quaternion.LookRotation(velocity);
            }
        }

        protected virtual void OnLaunched() { }
        protected virtual void OnHit(ref RaycastHit hit) { }

        private Vector3 EvaluateParabola(float time)
        {
            Vector3 point = _startPosition + _startDirection * (_speed * time);
            Vector3 gravity = Vector3.down * (_gravity * time * time);
            return point + gravity;
        }
    }
}