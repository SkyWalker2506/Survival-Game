using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames
{
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Polymind Games/Damage/Fire")]
    public sealed class Fire : MonoBehaviour
    {
        [SerializeField, Range(0f, 1000f), BeginGroup("Settings")]
        [Tooltip("The duration of the fire effect.")]
        private float _duration = 15f;

        [SerializeField, Range(0f, 1000f)]
        [Tooltip("The damage inflicted per tick of the fire.")]
        private float _damagePerTick = 15f;

        [SerializeField, Range(0.01f, 10f), EndGroup]
        [Tooltip("The time interval between each damage tick.")]
        private float _timePerTick = 1f;

        [SerializeField, NotNull, BeginGroup("Effects")]
        [Tooltip("The audio effect to play when the fire is active.")]
        private AudioEffect _audioEffect;

        [SerializeField, NotNull]
        [Tooltip("The light effect of the fire.")]
        private LightEffect _lightEffect;

        [SerializeField, NotNull]
        [Tooltip("The particle system representing the fire.")]
        private ParticleSystem _particles;

        private List<IDamageReceiver> _damageReceivers;
        private SphereCollider _collider;


        public void Detonate() => Detonate(null);

        public void Detonate(IDamageSource source)
        {
            _damageReceivers ??= new List<IDamageReceiver>();
            _damageReceivers.Clear();

            _collider.enabled = true;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f))
                transform.SetPositionAndRotation(hit.point, Quaternion.Euler(hit.normal));

            StartCoroutine(C_UpdateFire(source));
        }

        private void OnTriggerEnter(Collider col)
        {
            if (LayerConstants.ALL_SOLID_OBJECTS_MASK == (LayerConstants.ALL_SOLID_OBJECTS_MASK | 1 << col.gameObject.layer))
            {
                if (col.TryGetComponent<IDamageReceiver>(out var damageReceiver))
                    _damageReceivers.Add(damageReceiver);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (LayerConstants.ALL_SOLID_OBJECTS_MASK == (LayerConstants.ALL_SOLID_OBJECTS_MASK | 1 << col.gameObject.layer))
            {
                if (col.TryGetComponent<IDamageReceiver>(out var damageReceiver))
                    _damageReceivers.Remove(damageReceiver);
            }
        }

        private void OnEnable()
        {
            _collider.enabled = false;
            StopEffects();
        }

        private void Awake() => _collider = GetComponent<SphereCollider>();

        private IEnumerator C_UpdateFire(IDamageSource source)
        {
            StartEffects();

            DamageArgs args = new(DamageType.Fire, transform.position, Vector3.zero, source);
            float damage = _damagePerTick;

            float damageTimer = Time.time + _timePerTick;
            float endTime = Time.time + _duration;
            while (Time.time < endTime)
            {
                if (damageTimer < Time.time)
                {
                    foreach (var receiver in _damageReceivers)
                        receiver.ReceiveDamage(damage, args);

                    damageTimer = Time.time + _timePerTick;
                }

                yield return null;
            }

            _collider.enabled = false;
            StopEffects();
        }

        private void StartEffects()
        {
            _audioEffect.Play();
            _lightEffect.Play();
            _particles.Play(true);
        }

        private void StopEffects()
        {
            _audioEffect.Stop();
            _lightEffect.Stop();
            _particles.Stop(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_collider == null) _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(transform.position, _collider.radius);
        }
#endif
    }
}