using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public abstract class MeleeAttackBehaviour : MonoBehaviour
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Settings")]
        [Tooltip("The cooldown time between attacks.")]
        private float _attackCooldown = 0.75f;

        [SerializeField, Range(0, 10)]
        [Tooltip("The index of the attack animation.")]
        private int _attackAnimIndex;

        [SerializeField, Range(0.1f, 2f), EndGroup]
        [Tooltip("The speed of the attack animation.")]
        private float _attackAnimSpeed = 1f;
        
        
        public float AttackCooldown => _attackCooldown;
        protected IWieldable Wieldable { get; private set; }

        public abstract bool CanAttack();
        public abstract void Attack(float accuracy, UnityAction hitCallback = null);
        public abstract void CancelAttack();

        protected virtual void Awake() => Wieldable = GetComponent<IWieldable>();

        protected Ray GetUseRay(float accuracy, Vector3 hitOffset = default(Vector3))
        {
            float spread = Mathf.Lerp(1f, 5f, 1 - accuracy);
            var headTransform = Wieldable.Character.GetTransformOfBodyPoint(BodyPoint.Head);
            return PhysicsUtils.GenerateRay(headTransform, spread, hitOffset);
        }

        protected Ray GetUseRay(float accuracy, float minSpread, float maxSpread, Vector3 hitOffset)
        {
            float spread = Mathf.Lerp(minSpread, maxSpread, 1 - accuracy);
            var headTransform = Wieldable.Character.GetTransformOfBodyPoint(BodyPoint.Head);
            return PhysicsUtils.GenerateRay(headTransform, spread, hitOffset);
        }

        protected Vector3 GetCharacterVelocity()
        {
            return Wieldable.Character.TryGetCC(out IMotorCC motor) ? motor.Velocity : Vector3.zero;
        }

        protected void PlayAttackAnimation()
        {
            var animator = Wieldable.Animation;
            animator.SetFloat(WieldableAnimationConstants.ATTACK_INDEX, _attackAnimIndex);
            animator.SetFloat(WieldableAnimationConstants.ATTACK_SPEED, _attackAnimSpeed);
            animator.SetTrigger(WieldableAnimationConstants.ATTACK);
        }

        protected void PlayHitAnimation()
        {
            var animator = Wieldable.Animation;
            animator.SetTrigger(WieldableAnimationConstants.ATTACK_HIT);
        }
    }
}