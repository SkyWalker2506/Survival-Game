using PolymindGames.PoolingSystem;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Shooters/Projectile Firing-System")]
    public sealed class FirearmProjectileFiringSystem : FirearmFiringSystemBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Projectile")]
        private ProjectileBehaviour _projectile;

        [SerializeField, Range(1, 30)]
        [Tooltip("The amount of projectiles that will be spawned in the world")]
        private int _count = 1;

        [SerializeField, Range(0f, 100f), SpaceArea(3f)]
        private float _minSpread = 0.75f;

        [SerializeField, Range(0f, 100f)]
        private float _maxSpread = 1.5f;

        [SerializeField, Range(1f, 1000f), SpaceArea(3f)]
        private float _speed = 75f;

        [SerializeField, Range(0f, 100f), EndGroup]
        [Tooltip("The gravity for the projectile.")]
        private float _gravity = 9.8f;

        [SerializeField, BeginGroup("Dry Fire")]
        private bool _dryFireAnimation;

        [SerializeField]
        private ShakeData _dryFireShake;

        [SerializeField, InLineEditor, EndGroup]
        private AudioDataSO _dryFireAudio;
        

        public override void Shoot(float accuracy, IFirearmProjectileEffect effect, float value)
        {
            var headTransform = Wieldable.Character.GetTransformOfBodyPoint(BodyPoint.Head);

            // Spawn Projectile(s).
            float spread = Mathf.Lerp(_minSpread, _maxSpread, 1f - accuracy);
            for (int i = 0; i < _count; i++)
            {
                Ray ray = PhysicsUtils.GenerateRay(headTransform, spread);
                var projectile = ScenePools.GetObject(_projectile, ray.origin, Quaternion.LookRotation(ray.direction));
                projectile.Launch(Wieldable.Character, ray.origin, ray.direction * (_speed * value), effect, _gravity);
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
            ScenePools.CreatePool(_projectile, 5, 10, "Projectiles", 120f);
        }
    }
}