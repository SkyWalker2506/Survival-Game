using PolymindGames.PoolingSystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(Rigidbody), typeof(PoolableObject))]
    public abstract class PhysicsProjectileBehaviour : ProjectileBehaviour
    {
        [SerializeField, MinMaxSlider(0, 100f), BeginGroup, EndGroup]
        private Vector2 _torque = new(1, 2f);

        private PoolableObject _poolableObject;
        private IFirearmProjectileEffect _effect;
        private UnityAction _hitCallback;
        private Rigidbody _rigidbody;
        private Vector3 _gravity;
        private Vector3 _origin;

        private const float MAX_AIR_LIFE_TIME = 5f;
        
        
        protected ICharacter Character { get; private set; }
        protected bool InAir { get; private set; }
        protected Rigidbody Rigidbody => _rigidbody;
        
        public sealed override void Launch(ICharacter character, Vector3 origin, Vector3 velocity, IFirearmProjectileEffect effect, float gravity = 9.81f, bool instantStart = false, UnityAction hitCallback = null)
        {
            InAir = true;
            Character = character;
            _hitCallback = hitCallback;

            transform.position = origin;
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.linearVelocity = velocity;

            Vector3 torque = new(_torque.GetRandomFromRange(), _torque.GetRandomFromRange(), _torque.GetRandomFromRange());

            _rigidbody.angularVelocity = torque;
            _rigidbody.maxAngularVelocity = 10000f;

            bool hasCustomGravity = Mathf.Abs(gravity - Physics.gravity.y) > 0.01f;
            _gravity = new Vector3(0f, -Mathf.Abs(gravity), 0f);
            _rigidbody.useGravity = !hasCustomGravity;
            enabled = hasCustomGravity;

            _effect = effect;
            _origin = origin;

            _poolableObject.ReleaseObject(MAX_AIR_LIFE_TIME);

            OnLaunched();
        }

        protected virtual void OnLaunched() { }
        protected virtual void OnHit(Collision hit) { }

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _poolableObject = GetComponent<PoolableObject>();
        }

        protected virtual void FixedUpdate() => _rigidbody.AddForce(_gravity * _rigidbody.mass);

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!InAir)
                return;

            _poolableObject.ResetReleaseDelay();

            float travelledDistance = Vector3.Distance(_origin, transform.position);
            _effect?.DoHitEffect(collision, travelledDistance);

            InAir = false;
            _rigidbody.interpolation = RigidbodyInterpolation.None;

            _hitCallback?.Invoke();
            OnHit(collision);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _rigidbody = GetComponent<Rigidbody>();
        }
#endif
    }
}