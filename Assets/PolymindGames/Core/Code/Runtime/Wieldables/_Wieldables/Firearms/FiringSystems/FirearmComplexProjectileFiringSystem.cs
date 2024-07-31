using PolymindGames.PoolingSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Shooters/Complex Projectile Firing-System")]
    public sealed class FirearmComplexProjectileFiringSystem : FirearmFiringSystemBehaviour
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

        [SerializeField]
        private Vector3 _spawnPositionOffset = Vector3.zero;

        [SerializeField]
        private Vector3 _spawnRotationOffset = Vector3.zero;

        [SerializeField, Range(0f, 10f), SpaceArea(3f)]
        private float _inheritedSpeed;

        [SerializeField, Range(1f, 1000f)]
        private float _speed = 75f;

        [SerializeField, Range(0f, 100f), EndGroup]
        [Tooltip("The gravity for the projectile.")]
        private float _gravity = 9.8f;


        public override void Shoot(float accuracy, IFirearmProjectileEffect effect, float value)
        {
            var headTransform = Wieldable.Character.GetTransformOfBodyPoint(BodyPoint.Head);

            // Spawn Projectile(s).
            float spread = Mathf.Lerp(_minSpread, _maxSpread, 1f - accuracy);
            for (int i = 0; i < _count; i++)
            {
                Ray ray = PhysicsUtils.GenerateRay(headTransform, spread);

                var user = Wieldable.Character;
                Vector3 position = ray.origin + headTransform.TransformVector(_spawnPositionOffset);
                Quaternion rotation = Quaternion.LookRotation(ray.direction) * Quaternion.Euler(_spawnRotationOffset);

                // Calculate the launch velocity
                Vector3 launchVelocity = ray.direction * (_speed * value);

                if (user.TryGetCC(out IMotorCC motor))
                    launchVelocity += motor.Velocity * _inheritedSpeed;

                IProjectile projectile = ScenePools.GetObject(_projectile, position, rotation);
                projectile.Launch(user, position, launchVelocity, effect, _gravity);
            }

            Wieldable.Animation.SetTrigger(WieldableAnimationConstants.SHOOT);
        }

        public override void DryFire() { }

        protected override void Awake()
        {
            base.Awake();
            ScenePools.CreatePool(_projectile, 3, 10, "Projectiles", 120f);
        }
    }
}