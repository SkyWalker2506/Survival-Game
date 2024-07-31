using PolymindGames.PoolingSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Bullet Effects/Explosion Projectile Effect (Bullet)")]
    public sealed class FirearmExplosionProjectileEffect : FirearmProjectileEffectBehaviour, IFirearmProjectileEffect
    {
        [SerializeField, PrefabObjectOnly, BeginGroup, EndGroup]
        [Tooltip("Pooled explosion prefab.")]
        private Explosion _explosionPrefab;


        public override void DoHitEffect(ref RaycastHit hit, Vector3 hitDirection, float speed, float travelledDistance)
        {
            var explosion = ScenePools.GetObject(_explosionPrefab, hit.point, Quaternion.identity);
            explosion.Detonate(Wieldable.Character);
        }

        public override void DoHitEffect(Collision collision, float travelledDistance)
        {
            var explosion = ScenePools.GetObject(_explosionPrefab, collision.GetContact(0).point, Quaternion.identity);
            explosion.Detonate(Wieldable.Character);
        }

        protected override void Awake()
        {
            base.Awake();
            ScenePools.CreatePool(_explosionPrefab, 2, 8, "ProjectileEffects");
        }
    }
}