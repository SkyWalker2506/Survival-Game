using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.WieldableSystem
{
    using Random = UnityEngine.Random;
    
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public sealed class ParabolicStickyProjectile : ParabolicProjectileBehaviour, ISaveableComponent
    {
        [SerializeField, Range(0, 1f), BeginGroup("Trail")]
        private float _trailEnableDelay = 0.1f;

        [SerializeField, Disable, EndGroup]
        private TrailRenderer _trailRenderer;

        [SerializeField, Range(0, 10f), BeginGroup("Penetration")]
        private float _penetrationOffset = 0.45f;

        [SerializeField, EndGroup]
        private TwangSettings _twangSettings;

        private Collider _collider;
        private SurfaceHitType _hitSurface;
        private Transform _penetratedTransform;
        private Vector3 _penetrationPositionOffset;
        private Quaternion _penetrationRotationOffset;
        private Rigidbody _rigidbody;
        private Twang? _twang;
        private float _trailEnableTimer;


        protected override void OnLaunched()
        {
            // Enable the trail.
            _trailEnableTimer = Time.time + _trailEnableDelay;

            _trailRenderer.Clear();

            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            _collider.isTrigger = true;

            _penetratedTransform = null;
            _hitSurface = SurfaceHitType.None;

            _twang = null;
        }

        protected override void OnHit(ref RaycastHit hit)
        {
            // Clean the trail.
            _trailRenderer.emitting = false;
            _trailEnableTimer = float.MaxValue;

            _collider.enabled = true;

            CachedTransform.position = hit.point + CachedTransform.forward * -_penetrationOffset;

            // Stick the projectile in the object.
            _penetratedTransform = hit.collider.transform;
            _penetrationPositionOffset = _penetratedTransform.InverseTransformPoint(CachedTransform.position);
            _penetrationRotationOffset = Quaternion.Inverse(_penetratedTransform.rotation) * CachedTransform.rotation;

            _hitSurface = _penetratedTransform.gameObject.isStatic ? SurfaceHitType.Static : SurfaceHitType.Dynamic;

            if (_penetratedTransform.gameObject.TryGetComponent<ParabolicStickyProjectile>(out var hitProjectile))
            {
                _penetratedTransform = null;
                Unstick();

                hitProjectile.Unstick();
            }
            else
            {
                // Animate the projectile.
                if (_twangSettings.Duration > 0.01f)
                    _twang = new Twang(ref _twangSettings, CachedTransform);

            }

            Update();
        }

        protected override void Awake()
        {
            base.Awake();

            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            _trailRenderer.emitting = false;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (_trailEnableTimer <= Time.fixedTime)
            {
                _trailRenderer.emitting = true;
                _trailEnableTimer = float.MaxValue;
            }
        }

        protected override void Update()
        {
            base.Update();

            switch (_hitSurface)
            {
                case SurfaceHitType.None: break;
                case SurfaceHitType.Dynamic:
                    UpdateDynamicHit();
                    break;
                case SurfaceHitType.Static:
                    UpdateStaticHit();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// While the penetrated transform is not null this projectile will stick to it and apply a twang animation.
        /// </summary>
        private void UpdateDynamicHit()
        {
            if (_penetratedTransform != null && _penetratedTransform.gameObject.activeSelf)
            {
                Vector3 position = _penetratedTransform.position + _penetratedTransform.TransformVector(_penetrationPositionOffset);
                Quaternion rotation = _penetratedTransform.rotation * _penetrationRotationOffset;

                if (_twang != null)
                {
                    var (twangPos, twangRot) = _twang.Value.GetPositionAndRotation();

                    position += twangPos;
                    rotation *= twangRot;

                    if (_twang.Value.IsFinished())
                        _twang = null;
                }

                CachedTransform.SetPositionAndRotation(position, rotation);
            }
            else
                Unstick();
        }

        /// <summary>
        /// Apply a twang animation.
        /// </summary>
        private void UpdateStaticHit()
        {
            if (_twang == null)
                return;

            Vector3 position = _penetratedTransform.position + _penetratedTransform.TransformVector(_penetrationPositionOffset);
            Quaternion rotation = _penetratedTransform.rotation * _penetrationRotationOffset;

            var (twangPos, twangRot) = _twang.Value.GetPositionAndRotation();

            position += twangPos;
            rotation *= twangRot;

            if (_twang.Value.IsFinished())
                _twang = null;

            CachedTransform.SetPositionAndRotation(position, rotation);
        }

        private void Unstick()
        {
            _collider.isTrigger = false;
            _rigidbody.isKinematic = false;
            _hitSurface = SurfaceHitType.None;
        }
        
        #region Save & Load
        void ISaveableComponent.LoadMembers(object data)
        {
            
        }

        // TODO: Implement
        object ISaveableComponent.SaveMembers()
        {
            return null;
        }
        #endregion

        #region Internal
        [Serializable]
        private struct TwangSettings
        {
            public Vector3 PivotOffset;

            [Range(0f, 10f)]
            public float Duration;

            [Range(0f, 500f)]
            public float Range;

            public Vector2 Rotation;

            public AudioDataSO Audio;
        }

        private struct Twang
        {
            private readonly Quaternion _randomRotation;
            private readonly Vector3 _pivotOffset;
            private readonly float _stopTime;
            private float _currentVelocity;
            private float _rotationRange;


            public Twang(ref TwangSettings settings, Transform transform)
            {
                if (settings.Audio != null)
                    AudioManager.Instance.PlayClipAtPoint(settings.Audio.Clip, transform.position, settings.Audio.Volume);

                _stopTime = Time.time + settings.Duration;

                Vector2 randomRotationRange = settings.Rotation;
                _randomRotation = Quaternion.Euler(new Vector2(
                    Random.Range(-randomRotationRange.x, randomRotationRange.x),
                    Random.Range(-randomRotationRange.y, randomRotationRange.y)));

                _rotationRange = settings.Range;
                _pivotOffset = settings.PivotOffset;

                _currentVelocity = 0f;
            }

            public bool IsFinished() => Time.time >= _stopTime;

            public ValueTuple<Vector3, Quaternion> GetPositionAndRotation()
            {
                Quaternion rotRange = Quaternion.Euler(Random.Range(-_rotationRange, _rotationRange), Random.Range(-_rotationRange, _rotationRange), 0f);
                Quaternion targetRot = _randomRotation * rotRange;
                Vector3 targetPos = _pivotOffset - targetRot * _pivotOffset;
                _rotationRange = Mathf.SmoothDamp(_rotationRange, 0f, ref _currentVelocity, _stopTime - Time.time);

                return (targetPos, targetRot);
            }
        }

        private enum SurfaceHitType
        {
            None,
            Dynamic,
            Static
        }
		#endregion

        #region Editor
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Twang pivot
            if (_twangSettings.Duration > 0.01f)
            {
                Vector3 twangPivotPosition = transform.position + transform.TransformVector(_twangSettings.PivotOffset);
                var cameraTransform = SceneView.currentDrawingSceneView.camera.transform;
                Vector3 sceneCamPosition = cameraTransform.position;
                Vector3 sceneCamForward = cameraTransform.forward;

                // Make sure we don't draw the gizmo when not looking at it
                if (Vector3.Dot(sceneCamForward, twangPivotPosition - sceneCamPosition) >= 0f)
                {
                    Handles.Label(twangPivotPosition, "Twang Pivot");
                    Gizmos.color = new Color(0f, 1f, 0f, 0.85f);
                    Gizmos.DrawSphere(twangPivotPosition, 0.03f);
                }
            }
        }

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
    }
}