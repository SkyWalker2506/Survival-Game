using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames
{
    [AddComponentMenu("Polymind Games/Damage/Explosion")]
    public sealed class Explosion : MonoBehaviour
    {
        [BeginGroup]
        [SerializeField, Range(0f, 1000f)]
        private float _force = 20;

        [SerializeField, Range(0f, 1000f)]
        private float _damage = 90;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _radius = 7;

        [BeginGroup("Effects")]
        [SerializeField, InLineEditor, NotNull]
        private AudioDataSO _explosionAudio;

        [SerializeField]
        private ShakeData _cameraShake;

        [SerializeField, NotNull]
        private LightEffect _lightEffect;

        [SerializeField, NotNull, EndGroup]
        private ParticleSystem _particles;

        private const float CHARACTER_EXTRA_FORCE_MOD = 30f;


        public void Detonate() => Detonate(null);

        public void Detonate(IDamageSource source)
        {
            Vector3 position = transform.position;
            int colsCount = PhysicsUtils.OverlapSphereOptimized(position, _radius, out var colliders, LayerConstants.DAMAGEABLE_MASK, QueryTriggerInteraction.Collide);

            for (var i = 0; i < colsCount; i++)
            {
                var col = colliders[i];

                if (col.TryGetComponent<ICharacter>(out var character) && character.TryGetCC(out IMotorCC motor))
                {
                    var characterPos = character.transform.position;
                    float forceMultiplier = (1 - Vector3.Distance(characterPos, position) / _radius) * _force * CHARACTER_EXTRA_FORCE_MOD;
                    motor.AddForce((characterPos - position + Vector3.up).normalized * forceMultiplier,
                        ForceMode.Impulse);
                }

                if (col.TryGetComponent(out IDamageReceiver receiver))
                {
                    (float damage, var context) = GetDamageForHit(col.transform, source);
                    receiver.ReceiveDamage(damage, context);
                }

                if (col.attachedRigidbody != null)
                {
                    col.attachedRigidbody.AddExplosionForce(_force, position, _radius, 1f,
                        ForceMode.Impulse);
                }
            }

            StartEffects();

            AudioManager.Instance.PlayClipAtPoint(_explosionAudio.Clip, position, _explosionAudio.Volume);
            ShakeEvents.DoShake(position, _cameraShake, _radius);
        }

        private void OnEnable() => StopEffects();

        private (float, DamageArgs) GetDamageForHit(Transform hit, IDamageSource source)
        {
            var hitPosition = hit.position;
            var trsPosition = transform.position;

            float distToObject = (trsPosition - hitPosition).sqrMagnitude;
            float explosionRadiusSqr = _radius * _radius;

            float distanceFactor = 1f - Mathf.Clamp01(distToObject / explosionRadiusSqr);

            float damage = _damage * distanceFactor;
            DamageArgs args = new(DamageType.Explosion, hitPosition, (hitPosition - trsPosition).normalized * _force, source);

            return (damage, args);
        }

        private void StartEffects()
        {
            _lightEffect.Play();
            _particles.Play(true);
        }

        private void StopEffects()
        {
            _lightEffect.Stop();
            _particles.Stop(true);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
            Gizmos.color = Color.white;
        }
#endif
    }
}