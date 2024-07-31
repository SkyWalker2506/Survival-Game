using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public sealed class ParabolicProjectile : ParabolicProjectileBehaviour
    {
        [SerializeField, Range(0, 1f), BeginGroup]
        [Tooltip("The delay before enabling the trail renderer after the projectile is fired.")]
        private float _trailEnableDelay = 0.1f;

        [SerializeField]
        [Tooltip("The TrailRenderer component to be used for the projectile.")]
        private TrailRenderer _trailRenderer;

        [SerializeField, EndGroup]
        [Tooltip("The ParticleSystem component to be used for the projectile.")]
        private ParticleSystem _particleSystem;

        private float _trailEnableTimer;


        protected override void OnLaunched()
        {
            _trailRenderer.Clear();
            _trailEnableTimer = Time.fixedTime + _trailEnableDelay;
        }

        protected override void OnHit(ref RaycastHit hit)
        {
            _trailEnableTimer = float.MaxValue;
            _trailRenderer.emitting = false;

            _particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (_trailEnableTimer <= Time.fixedTime)
            {
                _trailRenderer.emitting = true;
                if (_particleSystem != null)
                    _particleSystem.Play(false);

                _trailEnableTimer = float.MaxValue;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _trailRenderer.emitting = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_trailRenderer == null)
            {
                _trailRenderer = GetComponentInChildren<TrailRenderer>();
                if (_trailRenderer == null)
                    _trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }
        }
#endif
    }
}