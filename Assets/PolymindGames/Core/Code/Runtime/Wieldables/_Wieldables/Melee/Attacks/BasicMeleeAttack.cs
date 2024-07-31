using System.Collections;
using PolymindGames.SurfaceSystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Attacks/Melee Basic Attack")]
    public sealed class BasicMeleeAttack : MeleeAttackBehaviour
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Attacking")]
        private float _attackDelay = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float _attackRadius = 0.1f;

        [SerializeField, Range(0f, 5f)]
        private float _attackDistance = 0.5f;

        [SerializeField, EndGroup]
        private DelayedAudioData _attackAudio = DelayedAudioData.Default;

        [SerializeField, BeginGroup("Hitting")]
        private DamageType _hitDamageType = DamageType.Hit;

        [SerializeField, Range(0f, 1000f)]
        private float _hitDamage = 15f;

        [SerializeField, Range(0f, 1000f)]
        private float _hitForce = 30f;

        [SerializeField, EndGroup]
        private AudioData _hitAudio = AudioData.Default;

        private Coroutine _attackRoutine;


        public override bool CanAttack() => true;
        public override void CancelAttack() => CoroutineUtils.StopCoroutine(this, ref _attackRoutine);
        
        public override void Attack(float accuracy, UnityAction hitCallback = null)
        {
            PlayAttackAnimation();
            Wieldable.AudioPlayer.PlayDelayed(_attackAudio);

            _attackRoutine = StartCoroutine(C_Hit(accuracy, hitCallback));
        }

        private IEnumerator C_Hit(float accuracy, UnityAction hitCallback)
        {
            float endTime = Time.time + _attackDelay;
            while (endTime > Time.time)
                yield return null;
            
            Ray ray = GetUseRay(accuracy);
            if (PhysicsUtils.SphereCastOptimized(ray, _attackRadius, _attackDistance, out var hitInfo, LayerConstants.ALL_SOLID_OBJECTS_MASK, Wieldable.Character.transform, QueryTriggerInteraction.UseGlobal))
            {
                bool hitRigidbody = hitInfo.rigidbody != null;

                // Apply an impact impulse
                if (hitRigidbody)
                    hitInfo.rigidbody.AddForceAtPosition(ray.direction * _hitForce, hitInfo.point, ForceMode.Impulse);

                if (hitInfo.collider.TryGetComponent(out IDamageReceiver receiver))
                {
                    var args = new DamageArgs(_hitDamageType, hitInfo.point, ray.direction * _hitForce, Wieldable.Character);
                    receiver.ReceiveDamage(_hitDamage, args);
                }

                SurfaceManager.Instance.SpawnEffectFromHit(ref hitInfo, _hitDamageType.GetSurfaceEffectType(), 1f, hitRigidbody);

                if (_hitAudio.IsPlayable)
                    Wieldable.AudioPlayer.Play(_hitAudio);

                PlayHitAnimation();
                    
                hitCallback?.Invoke();
            }

            _attackRoutine = null;
        }
    }
}