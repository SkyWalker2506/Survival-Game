using PolymindGames.SurfaceSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Bullet Effects/Basic Projectile Effect (Bullet)")]
    public sealed class FirearmBasicProjectileEffect : FirearmProjectileEffectBehaviour
    {
        [SerializeField, BeginGroup("Damage")]
        private DamageType _damageType = DamageType.Bullet;

        [SerializeField, Range(0f, 1000f)]
        [Tooltip("The damage at close range.")]
        private float _damage = 15f;

        [SerializeField, Range(0f, 1000f), EndGroup]
        [Tooltip("The impact impulse that will be transferred to the rigidbodies at contact.")]
        private float _force = 15f;


        public override void DoHitEffect(ref RaycastHit hit, Vector3 hitDirection, float speed, float travelledDistance)
        {
            bool hitRigidbody = hit.rigidbody != null;

            // Apply an impact impulse
            if (hitRigidbody)
                hit.rigidbody.AddForceAtPosition(hitDirection * _force, hit.point, ForceMode.Impulse);

            if (hit.collider.TryGetComponent(out IDamageReceiver receiver))
            {
                IDamageSource source = Wieldable.Character;
                DamageArgs args = new(_damageType, hit.point, hitDirection * _force, source);
                receiver.ReceiveDamage(_damage, args);
            }

            SurfaceManager.Instance.SpawnEffectFromHit(ref hit, _damageType.GetSurfaceEffectType(), 1f, hitRigidbody);
        }

        public override void DoHitEffect(Collision collision, float travelledDistance)
        {
            var contact = collision.GetContact(0);

            bool hitRigidbody = collision.rigidbody != null;

            // Apply an impact impulse
            if (hitRigidbody)
                collision.rigidbody.AddForceAtPosition(-contact.normal * _force, contact.point, ForceMode.Impulse);

            if (collision.collider.TryGetComponent(out IDamageReceiver receiver))
                receiver.ReceiveDamage(_damage, new DamageArgs(_damageType, contact.point, -contact.normal * _force, Wieldable.Character));

            SurfaceManager.Instance.SpawnEffectFromCollision(collision, _damageType.GetSurfaceEffectType(), 1f, hitRigidbody);
        }
    }
}