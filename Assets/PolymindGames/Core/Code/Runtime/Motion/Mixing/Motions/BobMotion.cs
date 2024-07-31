using UnityEngine;
using System;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AdditiveForceMotion))]
    [AddComponentMenu("Polymind Games/Motion/Bob Motion")]
    [RequireCharacterComponent(typeof(IMovementControllerCC), typeof(IMotorCC))]
    public sealed class BobMotion : DataMotionBehaviour<IBobMotionData>
    {
        [BeginGroup("Interpolation")]
        [SerializeField, Range(0.1f, 10f)]
        private float _resetSpeed = 1.5f;

        [SerializeField, Range(0f, 3.14f), EndGroup]
        private float _rotationDelay = 0.5f;
        
        private AdditiveForceMotion _additiveForce;
        private IMovementControllerCC _movement;
        private float _bobParam;
        private bool _inversed;
        private IMotorCC _motor;
        private Vector3 _posBob;
        private Vector3 _rotBob;

        private const float POSITION_BOB_MOD = 0.01f;
        private const float ROTATION_BOB_MOD = 0.5f;


        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();
            _movement = character.GetCC<IMovementControllerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _movement.StepCycleEnded += OnStepped;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _movement.StepCycleEnded -= OnStepped;
        }

        protected override void OnDataChanged(IBobMotionData data)
        {
            if (data != null)
            {
                PositionSpring.Adjust(data.PositionSettings);
                RotationSpring.Adjust(data.RotationSettings);
            }
        }

        public override void UpdateMotion(float deltaTime)
        {
            bool canUpdateBob = CanUpdateBob();
            if (RotationSpring.IsIdle && !canUpdateBob)
                return;

            // Update the bob parameter.
            if (canUpdateBob)
            {
                TickBobParam();

                // Calculate the position bob.
                Vector3 posAmplitude = Data.PositionAmplitude;
                _posBob = new Vector3
                {
                    x = Mathf.Cos(_bobParam) * posAmplitude.x,
                    y = Mathf.Cos(_bobParam * 2) * posAmplitude.y,
                    z = Mathf.Cos(_bobParam) * posAmplitude.z
                };

                // Calculate the rotation bob.
                Vector3 rotAmplitude = Data.RotationAmplitude;
                _rotBob = new Vector3
                {
                    x = Mathf.Cos(_bobParam * 2 + _rotationDelay) * rotAmplitude.x,
                    y = Mathf.Cos(_bobParam + _rotationDelay) * rotAmplitude.y,
                    z = Mathf.Cos(_bobParam + _rotationDelay) * rotAmplitude.z
                };
            }

            // Reset bob...
            else
            {
                float resetSpeed = Data == null ? deltaTime * 50f : deltaTime * _resetSpeed;
                _bobParam = Mathf.MoveTowards(_bobParam, 0f, resetSpeed);

                _posBob = Mathf.Abs(_posBob.x + _posBob.y + _posBob.y) > 0.001f
                    ? Vector3.MoveTowards(_posBob, Vector3.zero, resetSpeed)
                    : Vector3.zero;

                _rotBob = Mathf.Abs(_rotBob.x + _rotBob.y + _rotBob.y) > 0.001f
                    ? Vector3.MoveTowards(_rotBob, Vector3.zero, resetSpeed)
                    : Vector3.zero;
            }

            SetTargetPosition(_posBob, POSITION_BOB_MOD);
            SetTargetRotation(_rotBob, ROTATION_BOB_MOD);
        }

        private void TickBobParam()
        {
            switch (Data.BobType)
            {
                case BobMode.StepCycleBased:
                    _bobParam = _movement.StepCycle * Mathf.PI;
                    _bobParam += _inversed ? 0f : Mathf.PI;
                    break;
                case BobMode.TimeBased:
                    _bobParam = Time.time * Data.BobSpeed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnStepped()
        {
            if (!CanUpdateBob())
                return;

            if (_additiveForce == null)
                _additiveForce = MotionMixer.GetMotionOfType<AdditiveForceMotion>();

            float stepFactor = _motor.Velocity.sqrMagnitude < 0.1f ? 0f : 1f;

            _additiveForce.AddPositionForce(Data.PositionStepForce, 0.05f * stepFactor * FinalMultiplier);
            _additiveForce.AddRotationForce(Data.RotationStepForce, stepFactor * FinalMultiplier);

            _inversed = !_inversed;
        }

        private bool CanUpdateBob()
        {
            if (Data == null)
                return false;

            return Data.BobType == BobMode.TimeBased ||
                   _motor.IsGrounded && (_motor.Velocity.sqrMagnitude > 0.1f || _motor.TurnSpeed > 0.1f);
        }
    }
}