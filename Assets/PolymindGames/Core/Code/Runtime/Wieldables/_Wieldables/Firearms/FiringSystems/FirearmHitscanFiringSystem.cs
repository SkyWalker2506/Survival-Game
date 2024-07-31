using PolymindGames.PoolingSystem;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Shooters/Hitscan Firing-System")]
    public class FirearmHitscanFiringSystem : FirearmFiringSystemBehaviour
    {
        [SerializeField, Range(1, 30), BeginGroup("Ray")]
        [Tooltip("The amount of rays that will be sent in the world")]
        private int _rayCount = 1;

        [SerializeField, Range(0f, 100f)]
        private float _minSpread = 1f;

        [SerializeField, Range(0f, 100f)]
        private float _maxSpread = 2f;

        [SerializeField, Range(0f, 10000f), EndGroup]
        private float _maxDistance = 300f;

        [SerializeField, PrefabObjectOnly, BeginGroup("Visual Effects")]
        private ProjectileTracer _tracerPrefab;

        [SerializeField, Range(0f, 1000f), EndGroup]
        private float _tracerSpeed = 100f;

        [SerializeField, BeginGroup("Dry Fire")]
        private bool _dryFireAnimation;

        [SerializeField]
        private ShakeData _dryFireShake;

        [SerializeField, InLineEditor, EndGroup]
        private AudioDataSO _dryFireAudio;

        private const float MAX_TRACER_DISTANCE = 2000f;


        public override void Shoot(float accuracy, IFirearmProjectileEffect effect, float triggerValue)
        {
            var character = Wieldable.Character;

            float spread = Mathf.Lerp(_minSpread, _maxSpread, 1f - accuracy);
            for (int i = 0; i < _rayCount; i++)
            {
                var headTransform = character.GetTransformOfBodyPoint(BodyPoint.Head);
                Ray ray = PhysicsUtils.GenerateRay(headTransform, spread);

                var tracer = _tracerPrefab != null
                    ? ScenePools.GetObject(_tracerPrefab, ray.origin, Quaternion.LookRotation(ray.direction))
                    : null;

                if (PhysicsUtils.RaycastOptimized(ray, _maxDistance, out RaycastHit hit, LayerConstants.ALL_SOLID_OBJECTS_MASK, Wieldable.Character.transform, QueryTriggerInteraction.UseGlobal))
                {
                    effect.DoHitEffect(ref hit, ray.direction, float.PositiveInfinity, hit.distance);

                    if (tracer != null)
                        tracer.DoTracer(ray.origin, hit.point, _tracerSpeed);
                }
                else if (tracer != null)
                    tracer.DoTracer(ray.origin, ray.GetPoint(MAX_TRACER_DISTANCE), _tracerSpeed);
            }

            Wieldable.Animation.SetTrigger(WieldableAnimationConstants.SHOOT);
        }

        public override void DryFire()
        {
            Wieldable.AudioPlayer.PlaySafe(_dryFireAudio);
            
            if (_dryFireShake.IsPlayable)
                Wieldable.Motion.HandsMotionMixer.GetMotionOfType<AdditiveShakeMotion>().AddShake(_dryFireShake);

            if (_dryFireAnimation)
                Wieldable.Animation.SetTrigger(WieldableAnimationConstants.DRY_FIRE);
        }

        protected override void Awake()
        {
            base.Awake();

            if (_tracerPrefab != null)
                ScenePools.CreatePool(_tracerPrefab, 8, 16, "Tracers");
        }
    }
}