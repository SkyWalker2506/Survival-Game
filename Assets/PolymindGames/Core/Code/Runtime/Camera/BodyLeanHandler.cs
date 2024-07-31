using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames
{
    [OptionalCharacterComponent(typeof(IMotorCC))]
    public sealed class BodyLeanHandler : CharacterBehaviour, IBodyLeanHandlerCC
    {
        [SerializeField, Range(0f, 3f), BeginGroup("Settings")]
        private float _leanCooldown = 0.2f;

        [SerializeField]
        private LayerMask _obstructionMask;

        [SerializeField, Range(0f, 1f)]
        private float _obstructionPadding = 0.15f;

        [SerializeField, Range(0f, 1f)]
        private float _maxLeanObstructionCutoff = 0.35f;

        [SerializeField, Range(1f, 100f), EndGroup]
        private float _maxAllowedCharacterSpeed = 4f;

        [SerializeField, NotNull, BeginGroup("References")]
        private Transform _referenceTransform;

        [SerializeField, NotNull]
        private LeanMotion _bodyLeanMotion;

        [SerializeField, NotNull, EndGroup]
        private LeanMotion _wieldableLeanMotion;

        [SerializeField, BeginGroup("Audio"), EndGroup]
        private AudioData _leanAudio = AudioData.Default;
        
        private float _leanTimer;
        private float _maxLeanPercent;
        private IMotorCC _motor;
        private RaycastHit _raycastHit;
        private BodyLeanState _leanState;


        public BodyLeanState LeanState => _leanState;
        
        public void SetLeanState(BodyLeanState leanState)
        {
            if (Time.time < _leanTimer)
                return;

            if (CanLean(leanState))
            {
                SetLeanState_Internal(leanState);
                _leanTimer = Time.time + _leanCooldown;
            }
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();
            SetLeanPercent(1f);
        }

        private void Update()
        {
            if (_leanState == BodyLeanState.Center)
                return;

            if (!CanLean(_leanState))
                SetLeanState_Internal(BodyLeanState.Center);
        }

        private void SetLeanState_Internal(BodyLeanState leanState)
        {
            if (leanState == _leanState)
                return;

            _leanState = leanState;
            Character.AudioPlayer.Play(_leanAudio, Mathf.Max(_maxLeanPercent, 0.3f), BodyPoint.Torso);

            if (leanState == BodyLeanState.Center)
                SetLeanPercent(1f);

            _bodyLeanMotion.SetLeanState(leanState);
            _wieldableLeanMotion.SetLeanState(leanState);
        }

        private bool CanLean(BodyLeanState leanState)
        {
            if (leanState == BodyLeanState.Center)
                return true;

            if (_motor != null && (_motor.Velocity.magnitude > _maxAllowedCharacterSpeed || !_motor.IsGrounded))
                return false;

            Vector3 position = _referenceTransform.position;
            Vector3 targetPos = new Vector3(leanState == BodyLeanState.Left ? -_bodyLeanMotion.LeanSideOffset : _bodyLeanMotion.LeanSideOffset,
                -_bodyLeanMotion.LeanHeightOffset, 0f);

            targetPos = _bodyLeanMotion.transform.TransformPoint(targetPos);

            Ray ray = new Ray(position, targetPos - position);
            float distance = Vector3.Distance(position, targetPos) + _obstructionPadding;

            if (PhysicsUtils.SphereCastOptimized(ray, 0.2f, distance, out _raycastHit, _obstructionMask, Character.transform))
            {
                // Lower the max lean value.
                SetLeanPercent(_raycastHit.distance / distance);
                return _maxLeanPercent > _maxLeanObstructionCutoff;
            }

            // Reset the max lean value.
            _bodyLeanMotion.MaxLeanPercent = 1f;
            _wieldableLeanMotion.MaxLeanPercent = 1f;
            return true;
        }

        private void SetLeanPercent(float percent)
        {
            _maxLeanPercent = percent;
            _bodyLeanMotion.MaxLeanPercent = percent;
            _wieldableLeanMotion.MaxLeanPercent = percent;
        }
    }
}