using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_2)]
    public sealed class MotionMixer : MonoBehaviour, IMotionMixer
    {
        [SerializeField, InLineEditor, BeginGroup]
        private Transform _targetTransform;

        [SerializeField, EndGroup]
        private MixMode _mixMode = MixMode.FixedLerpUpdate;

        [SerializeField, BeginGroup("Offset")]
        private Vector3 _pivotOffset;

        [SerializeField]
        private Vector3 _positionOffset;

        [SerializeField, EndGroup]
        private Vector3 _rotationOffset;
        
        private readonly List<IMixedMotion> _motions = new();
        private readonly Dictionary<Type, IMixedMotion> _motionsDict = new();

        private Vector3 _startPosition = Vector3.zero;
        private Vector3 _targetPosition = Vector3.zero;
        private Quaternion _startRotation = Quaternion.identity;
        private Quaternion _targetRotation = Quaternion.identity;
        private float _weightMultiplier = 1f;
        

        public Transform TargetTransform => _targetTransform;

        public float WeightMultiplier
        {
            get => _weightMultiplier;
            set
            {
                value = Mathf.Clamp01(value);
                _weightMultiplier = value;

                for (var i = 0; i < _motions.Count; i++)
                    _motions[i].Multiplier = value;
            }
        }

        public void ResetMixer(Transform targetTransform, Vector3 pivotOffset, Vector3 positionOffset, Vector3 rotationOffset)
        {
            _targetTransform = targetTransform;
            _pivotOffset = pivotOffset;
            _positionOffset = positionOffset;
            _rotationOffset = rotationOffset;
        }

        public bool TryGetMotionOfType<T>(out T motion) where T : class, IMixedMotion
        {
            if (_motionsDict.TryGetValue(typeof(T), out var mixedMotion))
            {
                motion = (T)mixedMotion;
                return true;
            }

            motion = null;
            return false;
        }

        public T GetMotionOfType<T>() where T : class, IMixedMotion
        {
            if (_motionsDict.TryGetValue(typeof(T), out var mixedMotion))
                return (T)mixedMotion;

            Debug.LogError($"No motion of type ''{nameof(T)}'' found, use ''{nameof(TryGetMotionOfType)}'' instead if the expected motion can be null.");
            return null;
        }

        public void AddMixedMotion(IMixedMotion mixedMotion)
        {
            if (mixedMotion == null)
                return;

            var motionType = mixedMotion.GetType();
            if (!_motionsDict.ContainsKey(motionType))
            {
                _motionsDict.Add(motionType, mixedMotion);
                _motions.Add(mixedMotion);
                mixedMotion.Multiplier = _weightMultiplier;
            }
        }

        public void RemoveMixedMotion(IMixedMotion mixedMotion)
        {
            if (mixedMotion == null)
                return;

            if (_motionsDict.Remove(mixedMotion.GetType()))
                _motions.Remove(mixedMotion);
        }

        private void Update()
        {
            switch (_mixMode)
            {
                case MixMode.FixedLerpUpdate:
                    UpdateInterpolation();
                    return;
                case MixMode.Update:
                    UpdateMotions(false, Time.deltaTime);
                    break;
            }
        }

        private void LateUpdate()
        {
            switch (_mixMode)
            {
                case MixMode.FixedLerpLateUpdate:
                    UpdateInterpolation();
                    return;
                case MixMode.LateUpdate:
                    UpdateMotions(false, Time.deltaTime);
                    break;
            }
        }

        private void FixedUpdate()
        {
            switch (_mixMode)
            {
                case MixMode.FixedUpdate:
                    UpdateMotions(false, Time.fixedDeltaTime);
                    break;
                case MixMode.FixedLerpUpdate:
                    UpdateMotions(true, Time.fixedDeltaTime);
                    break;
                case MixMode.FixedLerpLateUpdate:
                    UpdateMotions(true, Time.fixedDeltaTime);
                    break;
            }
        }

        private void UpdateInterpolation()
        {
            float delta = Time.time - Time.fixedTime;
            if (delta < Time.fixedDeltaTime)
            {
                float t = delta / Time.fixedDeltaTime;
                Vector3 targetPosition = Vector3.Lerp(_startPosition, _targetPosition, t);
                Quaternion targetRotation = Quaternion.Lerp(_startRotation, _targetRotation, t);
                _targetTransform.SetLocalPositionAndRotation(targetPosition, targetRotation);
            }
            else
                _targetTransform.SetLocalPositionAndRotation(_targetPosition, _targetRotation);
        }

        private void UpdateMotions(bool lerp, float deltaTime)
        {
            Vector3 targetPos = _pivotOffset;
            Quaternion targetRot = Quaternion.identity;

            for (int i = 0; i < _motions.Count; i++)
            {
                var motion = _motions[i];
                motion.UpdateMotion(deltaTime);

                targetPos += targetRot * motion.GetPosition(deltaTime);
                targetRot *= motion.GetRotation(deltaTime);
            }

            targetPos = targetPos - targetRot * _pivotOffset + _positionOffset;
            targetRot *= Quaternion.Euler(_rotationOffset);

            if (lerp)
            {
                _startPosition = _targetPosition;
                _startRotation = _targetRotation;
                _targetPosition = targetPos;
                _targetRotation = targetRot;
            }
            else
                _targetTransform.SetLocalPositionAndRotation(targetPos, targetRot);
        }

        #region Internal
        private enum MixMode
        {
            Update = 1,
            LateUpdate = 2,
            FixedUpdate = 3,
            FixedLerpUpdate = 4,
            FixedLerpLateUpdate = 5
        }
        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && _targetTransform != null)
                _targetTransform.SetLocalPositionAndRotation(_positionOffset, Quaternion.Euler(_rotationOffset));
        }

        private void Reset() => _targetTransform = transform;

        private void OnDrawGizmos()
        {
            Color pivotColor = new(0.1f, 1f, 0.1f, 0.5f);
            const float PIVOT_RADIUS = 0.08f;

            var prevColor = Handles.color;
            Handles.color = pivotColor;
            Handles.SphereHandleCap(0, transform.TransformPoint(_pivotOffset), Quaternion.identity, PIVOT_RADIUS, EventType.Repaint);
            Handles.color = prevColor;
        }
#endif
    }
}