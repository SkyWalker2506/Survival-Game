using System.Collections;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Attacks/Melee Throw Attack")]
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_1)]
    public sealed class MeleeThrowAttack : MeleeAttackBehaviour
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Throwing")]
        [Tooltip("Time to spawn the projectile")]
        private float _throwDelay = 0.5f;

        [SerializeField, Range(0f, 100f)]
        private float _throwMinSpread = 1f;

        [SerializeField, Range(0f, 100f)]
        private float _throwMaxSpread = 1f;

        [SerializeField]
        private Vector3 _throwPositionOffset = Vector3.zero;

        [SerializeField]
        private Vector3 _throwRotationOffset = Vector3.zero;

        [SerializeField, SpaceArea]
        private DelayedAudioData _throwStartAudio;

        [SerializeField]
        private AudioData _throwAudio;

        [SerializeField, Range(0f, 5f), SpaceArea]
        private float _visualsDisableDelay;

        [SerializeField, EndGroup]
        private Renderer _projectileVisuals;

        [SerializeField, PrefabObjectOnly, BeginGroup("Projectile")]
        private ProjectileBehaviour _projectile;

        [SerializeField, SceneObjectOnly]
        private FirearmProjectileEffectBehaviour _projectileEffect;

        [SerializeField, Range(0f, 1000f)]
        private float _projectileSpeed = 50f;

        [SerializeField, Range(0f, 100f)]
        [Tooltip("The gravity for the projectile.")]
        private float _projectileGravity = 9.81f;

        [SerializeField, EndGroup]
        private bool _linkItemToProjectile = true;

        private IAimInputHandler _aimInputHandler;
        private Coroutine _throwRoutine;


        public override bool CanAttack() => _aimInputHandler == null || _aimInputHandler.IsAiming;

        public override void Attack(float accuracy, UnityAction throwCallback = null)
        {
            var animator = Wieldable.Animation;
            animator.SetBool(WieldableAnimationConstants.IS_THROWN, true);
            PlayAttackAnimation();

            _aimInputHandler?.Aim(WieldableInputPhase.End);
            Wieldable.AudioPlayer.PlayDelayed(_throwStartAudio);

            _throwRoutine = StartCoroutine(C_Hit(accuracy, throwCallback));
        }

        private IEnumerator C_Hit(float accuracy, UnityAction hitCallback)
        {
            float endTime = Time.time + _visualsDisableDelay;
            while (endTime > Time.time)
                yield return null;
            
            // Disable the visuals projectile visuals. 
            _projectileVisuals.enabled = false;

            endTime = Time.time + _throwDelay - _visualsDisableDelay;
            while (endTime > Time.time)
                yield return null;
            
            // Calculate the launch state
            Ray ray = GetUseRay(accuracy, _throwMinSpread, _throwMaxSpread, _throwPositionOffset);
            Vector3 velocity = ray.direction * _projectileSpeed + GetCharacterVelocity();
            Quaternion rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(_throwRotationOffset);

            // Instantiate and launch the projectile
            var projectile = Instantiate(_projectile, ray.origin, rotation);
            projectile.Launch(Wieldable.Character, ray.origin, velocity, _projectileEffect, _projectileGravity, false, hitCallback);

            if (_throwAudio.IsPlayable)
                Wieldable.AudioPlayer.Play(_throwAudio);

            // Remove Item from the user's inventory.
            if (_linkItemToProjectile)
                LinkItemToProjectile(projectile);
            
            PlayHitAnimation();

            _throwRoutine = null;
        }

        public override void CancelAttack()
        {
            CoroutineUtils.StopCoroutine(this, ref _throwRoutine);
        }

        private void LinkItemToProjectile(ProjectileBehaviour projectile)
        {
            var attachedItem = GetComponent<IWieldableItem>()?.AttachedItem;
            if (attachedItem == null)
                return;

            attachedItem.StackCount--;

            // If the stack count is 0 it means the item has been fully removed from the inventory.
            // This enables us to link the same item to the prefab instead of creating a new one.
            if (projectile.gameObject.TryGetComponent(out ItemPickup pickup))
            {
                if (attachedItem.StackCount == 0)
                {
                    attachedItem.StackCount = 1;
                    pickup.LinkWithItem(attachedItem);
                }
                else
                {
                    if (pickup.AttachedItem == null)
                        pickup.LinkWithItem(new Item(attachedItem, 1));
                    else
                        pickup.AttachedItem.StackCount = 1;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _aimInputHandler = GetComponent<IAimInputHandler>();
        }

        private void OnEnable()
        {
            var animator = Wieldable.Animation;
            animator.ResetTrigger(WieldableAnimationConstants.HOLSTER);
            animator.SetBool(WieldableAnimationConstants.IS_THROWN, false);
            _projectileVisuals.enabled = true;
        }

        private void OnValidate()
        {
            _visualsDisableDelay = Mathf.Min(_visualsDisableDelay, _throwDelay);
        }
    }
}