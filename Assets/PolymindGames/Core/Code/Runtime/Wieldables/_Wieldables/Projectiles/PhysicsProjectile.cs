using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public sealed class PhysicsProjectile : PhysicsProjectileBehaviour
    {
        [SerializeField, BeginGroup("Settings")]
        private DetonationMode _detonationMode;

        [SerializeField, Range(0f, 100f)]
        [HideIf(nameof(_detonationMode), DetonationMode.OnImpact)]
        private float _detonateDelay = 5f;

        [SerializeField]
        private RigidbodyDetonateAction _rigidbodyImpactAction;

        [SerializeField]
        private RigidbodyDetonateAction _rigidbodyDetonateAction;

        [SerializeField, SpaceArea]
        private UnityEvent<ICharacter> _launchEvent;

        [SerializeField, EndGroup]
        private UnityEvent<ICharacter> _detonateEvent;

        [SerializeField, BeginGroup("Effects")]
        private TrailRenderer _trailRenderer;

        [SerializeField, EndGroup]
        private AudioDataSO _impactAudio;


        protected override void OnLaunched()
        {
            _trailRenderer.Clear();
            _trailRenderer.emitting = true;

            _launchEvent.Invoke(Character);

            if (_detonationMode == DetonationMode.AfterDelay)
                CoroutineUtils.InvokeDelayed(this, Detonate, _detonateDelay);
        }

        protected override void OnHit(Collision hit)
        {
            if (_impactAudio != null)
                AudioManager.Instance.PlayClipAtPoint(_impactAudio.Clip, hit.GetContact(0).point, _impactAudio.Volume);

            UpdateRigidbody(_rigidbodyImpactAction);
            _trailRenderer.emitting = false;

            switch (_detonationMode)
            {
                case DetonationMode.OnImpact:
                    Detonate();
                    break;
                case DetonationMode.OnImpactAfterDelay:
                    CoroutineUtils.InvokeDelayed(this, Detonate, 0.05f);
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _trailRenderer.emitting = false;
        }

        private void Detonate()
        {
            UpdateRigidbody(_rigidbodyDetonateAction);
            _detonateEvent.Invoke(Character);
        }

        private void UpdateRigidbody(RigidbodyDetonateAction action)
        {
            switch (action)
            {
                case RigidbodyDetonateAction.None:
                    break;
                case RigidbodyDetonateAction.Freeze:
                    Rigidbody.isKinematic = true;
                    break;
                case RigidbodyDetonateAction.ClearVelocity:
                    Rigidbody.linearVelocity = Vector3.zero;
                    Rigidbody.angularVelocity = Vector3.zero;
                    break;
                case RigidbodyDetonateAction.HalfTheVelocity:
                    Rigidbody.linearVelocity *= 0.5f;
                    Rigidbody.angularVelocity *= 0.5f;
                    break;
                case RigidbodyDetonateAction.QuarterTheVelocity:
                    Rigidbody.linearVelocity *= 0.25f;
                    Rigidbody.angularVelocity *= 0.25f;
                    break;
                case RigidbodyDetonateAction.RemoveAQuarterVelocity:
                    Rigidbody.linearVelocity *= 0.75f;
                    Rigidbody.angularVelocity *= 0.75f;
                    break;
                case RigidbodyDetonateAction.Unfreeze:
                    Rigidbody.isKinematic = false;
                    break;
            }
        }

        #region Editor
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
        #endregion
        
        #region Internal
        private enum DetonationMode : byte
        {
            AfterDelay,
            OnImpact,
            OnImpactAfterDelay
        }

        private enum RigidbodyDetonateAction : byte
        {
            None,
            Freeze,
            ClearVelocity,
            HalfTheVelocity,
            QuarterTheVelocity,
            RemoveAQuarterVelocity,
            Unfreeze
        }
        #endregion
    }
}