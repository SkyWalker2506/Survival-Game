using PolymindGames.ResourceGathering;
using PolymindGames.SurfaceSystem;
using PolymindGames.UserInterface;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Attacks/Melee Gathering Attack ")]
    public sealed class MeleeGatheringAttack : MeleeAttackBehaviour
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Gathering")]
        private float _gatherDelay = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float _gatherRadius = 0.1f;

        [SerializeField, Range(0f, 5f)]
        private float _gatherDistance = 0.55f;

        [SerializeField, Range(0f, 1000f), SpaceArea]
        private float _hitDamage = 16f;

        [SerializeField, Range(0f, 1000f)]
        private float _hitForce = 30f;

        [SerializeField, SpaceArea]
        private DelayedAudioData _attackAudio = DelayedAudioData.Default;

        [SerializeField, EndGroup]
        private AudioData _gatherAudio = AudioData.Default;

        [SerializeField]
        [ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private GatherableDefinition[] _validGatherables;

        private Coroutine _attackRoutine;


        public override bool CanAttack()
        {
            Ray ray = GetUseRay(1f);
            return TryGetGatherable(ray, out _, out _);
        }

        public override void Attack(float accuracy, UnityAction hitCallback = null)
        {
            PlayAttackAnimation();
            Wieldable.AudioPlayer.PlayDelayed(_attackAudio);
            _attackRoutine = StartCoroutine(C_Hit(accuracy, hitCallback));
        }

        private IEnumerator C_Hit(float accuracy, UnityAction hitCallback)
        {
            float endTime = Time.time + _gatherDelay;
            while (endTime > Time.time)
                yield return null;
            
            Ray ray = GetUseRay(accuracy);
            if (TryGetGatherable(ray, out var hit, out var gatherable))
            {
                DamageArgs args = new(ray.origin + ray.direction * 0.5f + Vector3.Cross(Vector3.up, ray.direction) * 0.25f, ray.direction * _hitForce, Wieldable.Character);
                gatherable.ReceiveDamage(_hitDamage, args);

                SurfaceManager.Instance.SpawnEffectFromHit(ref hit, SurfaceEffectType.Cut);
                Wieldable.AudioPlayer.Play(_gatherAudio);
                PlayHitAnimation();
                
                hitCallback?.Invoke();
            }

            _attackRoutine = null;
        }

        public override void CancelAttack() => CoroutineUtils.StopCoroutine(this, ref _attackRoutine);

        private bool TryGetGatherable(Ray ray, out RaycastHit hit, out IGatherable gatherable)
        {
            var rootTrs = Wieldable.Character.transform;
            if (PhysicsUtils.SphereCastOptimized(ray, _gatherRadius, _gatherDistance, out hit, LayerConstants.SIMPLE_SOLID_OBJECTS_MASK, rootTrs))
            {
                if (hit.collider.TryGetComponent(out gatherable))
                {
                    hit.point = gatherable.GetGatherPosition();
                    return ((IList)_validGatherables).Contains(gatherable.Definition)
                           && IsWithinGatherRange(gatherable, ray);
                }
            }

            gatherable = null;
            return false;
        }

        private bool IsWithinGatherRange(IGatherable gatherable, Ray ray)
        {
            return Mathf.Abs((ray.origin + ray.direction).y - (gatherable.transform.position + gatherable.Definition.GatherOffset).y) < _gatherDistance;
        }

        private void OnEnable()
        {
            if (TreeHealthIndicatorUI.Instance != null)
                TreeHealthIndicatorUI.Instance.ShowIndicator(_validGatherables);
        }

        private void OnDisable()
        {
            if (TreeHealthIndicatorUI.Instance != null)
                TreeHealthIndicatorUI.Instance.HideIndicator();
        }
    }
}