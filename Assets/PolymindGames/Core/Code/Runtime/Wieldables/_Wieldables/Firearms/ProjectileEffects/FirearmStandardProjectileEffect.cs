using PolymindGames.SurfaceSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Bullet Effects/Standard Projectile-Effect")]
    public sealed class FirearmStandardProjectileEffect : FirearmProjectileEffectBehaviour
    {
        [SerializeField, BeginGroup("Damage")]
        private DamageType _damageType = DamageType.Bullet;

        [SerializeField, Range(0f, 1000f)]
        [Tooltip("The maximum damage at close range.")]
        private float _damage = 15f;

        [SerializeField, Range(0f, 1000f), EndGroup]
        [Tooltip("The impact impulse that will be transferred to the rigidbodies at contact.")]
        private float _force = 15f;

        [SerializeField, BeginGroup("Falloff")]
        private FalloffType _falloffType;

        [SerializeField, Range(0f, 1000f)]
        private float _minFalloffThreshold = 20f;

        [SerializeField, Range(0f, 1000f), EndGroup]
        private float _maxFalloffThreshold = 100f;


        public override void DoHitEffect(ref RaycastHit hit, Vector3 hitDirection, float speed, float travelledDistance)
        {
            (float impulse, float damage) = GetImpulseAndDamage(speed, travelledDistance);

            bool hitRigidbody = hit.rigidbody != null;

            // Apply an impact impulse.
            if (hitRigidbody)
                hit.rigidbody.AddForceAtPosition(hitDirection * impulse, hit.point, ForceMode.Impulse);

            // Apply damage to any found receiver.
            if (hit.collider.TryGetComponent(out IDamageReceiver receiver))
                receiver.ReceiveDamage(damage, new DamageArgs(_damageType, hit.point, hitDirection * impulse, Wieldable.Character));

            SurfaceManager.Instance.SpawnEffectFromHit(ref hit, _damageType.GetSurfaceEffectType(), 1f, hitRigidbody);
        }

        public override void DoHitEffect(Collision collision, float travelledDistance)
        {
            (float impulse, float damage) = GetImpulseAndDamage(collision.relativeVelocity.magnitude, travelledDistance);

            var contact = collision.GetContact(0);
            bool hitRigidbody = collision.rigidbody != null;

            // Apply an impact impulse.
            if (hitRigidbody)
                collision.rigidbody.AddForceAtPosition(-contact.normal * impulse, contact.point, ForceMode.Impulse);

            // Apply damage to any found receiver.
            if (collision.collider.TryGetComponent(out IDamageReceiver receiver))
                receiver.ReceiveDamage(damage, new DamageArgs(_damageType, contact.point, -contact.normal * impulse, Wieldable.Character));

            SurfaceManager.Instance.SpawnEffectFromCollision(collision, _damageType.GetSurfaceEffectType(), 1f, hitRigidbody);
        }

        private (float impulse, float damage) GetImpulseAndDamage(float speed, float distance)
        {
            // Calculate the falloff damage and impulse mod.
            float falloffMod = _falloffType switch
            {
                FalloffType.Distance => (distance - _minFalloffThreshold) / (_maxFalloffThreshold - _minFalloffThreshold),
                FalloffType.Speed => (speed - _minFalloffThreshold) / (_maxFalloffThreshold - _minFalloffThreshold),
                _ => 1f
            };

            falloffMod = 1 - Mathf.Clamp01(falloffMod);

            float impulse = _force * falloffMod;
            float damage = _damage * falloffMod;

            return (impulse, damage);
        }

        #region Internal
        private enum FalloffType : byte
        {
            Distance,
            Speed
        }
        #endregion
    }
}