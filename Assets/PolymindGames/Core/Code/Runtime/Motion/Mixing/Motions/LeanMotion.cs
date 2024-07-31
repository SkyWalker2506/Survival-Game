using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AdditiveForceMotion))]
    [AddComponentMenu("Polymind Games/Motion/Lean Motion")]
    public sealed class LeanMotion : MotionBehaviour
    {
        [SerializeField, BeginGroup("Interpolation")]
        [Tooltip("Settings for rotation spring.")]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField, EndGroup]
        [Tooltip("Settings for position spring.")]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField, BeginGroup("Forces")]
        [Tooltip("Force applied on position when entering lean motion.")]
        private SpringForce3D _positionEnterForce = SpringForce3D.Default;

        [SerializeField, EndGroup]
        [Tooltip("Force applied on rotation when entering lean motion.")]
        private SpringForce3D _rotationEnterForce = SpringForce3D.Default;

        [SerializeField, Range(-90, 90f), BeginGroup("Offsets")]
        [Tooltip("Angle at which the object will lean.")]
        private float _leanAngle = 13f;

        [SerializeField, Range(-5f, 5f)]
        [Tooltip("Side offset applied during leaning.")]
        private float _leanSideOffset = 0.35f;

        [SerializeField, Range(-5f, 5f), EndGroup]
        [Tooltip("Height offset applied during leaning.")]
        private float _leanHeightOffset = 0.2f;

        private AdditiveForceMotion _additiveForce;
        private BodyLeanState _leanState;
        private float _maxLeanPercent = 1f;
        
        
        public float LeanAngle => _leanAngle;
        public float LeanSideOffset => _leanSideOffset;
        public float LeanHeightOffset => _leanHeightOffset;

        public float MaxLeanPercent
        {
            get => _maxLeanPercent;
            set
            {
                var leanPercent = Mathf.Clamp01(value);
                _maxLeanPercent = leanPercent;
            }
        }

        public void SetLeanState(BodyLeanState leanState)
        {
            if (_additiveForce == null)
                _additiveForce = MotionMixer.GetMotionOfType<AdditiveForceMotion>();

            float posFactor = _maxLeanPercent * 0.02f;
            _additiveForce.AddPositionForce(_positionEnterForce, posFactor, SpringType.FastSpring);

            float rotFactor = (leanState == BodyLeanState.Center ? -1 : 1f) * _maxLeanPercent;
            _additiveForce.AddRotationForce(_rotationEnterForce, rotFactor, SpringType.FastSpring);

            _leanState = leanState;
        }

        protected override void Awake()
        {
            base.Awake();
            IgnoreParentMultiplier = true;
        }

        protected override SpringSettings GetDefaultPositionSpringSettings() => _positionSpring;
        protected override SpringSettings GetDefaultRotationSpringSettings() => _rotationSpring;

        public override void UpdateMotion(float deltaTime)
        {
            if (_leanState == BodyLeanState.Center && RotationSpring.IsIdle)
                return;
            
            Vector3 targetPos;
            Vector3 targetRot;
            
            switch (_leanState)
            {
                case BodyLeanState.Left:
                    targetPos = new Vector3(-_leanSideOffset * _maxLeanPercent, -_leanHeightOffset * _maxLeanPercent, 0f);
                    targetRot = new Vector3(0f, 0f, _leanAngle * _maxLeanPercent);
                    break;
                case BodyLeanState.Right:
                    targetPos = new Vector3(_leanSideOffset * _maxLeanPercent, -_leanHeightOffset * _maxLeanPercent, 0f);
                    targetRot = new Vector3(0f, 0f, -_leanAngle * _maxLeanPercent);
                    break;
                case BodyLeanState.Center:
                    targetPos = Vector3.zero;
                    targetRot = Vector3.zero;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetTargetPosition(targetPos);
            SetTargetRotation(targetRot);
        }

        private (Vector3, Vector3) Get()
        {
            return (Vector3.zero, Vector3.zero);
        }
    }
}