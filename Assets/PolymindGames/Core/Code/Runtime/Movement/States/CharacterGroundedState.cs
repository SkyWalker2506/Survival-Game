using UnityEngine;

namespace PolymindGames.MovementSystem
{
    public abstract class CharacterGroundedState : CharacterMovementState
    {
        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("How much distance does this character need to cover to be considered a step.")]
        protected float _stepLength = 1.2f;

        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("The forward speed of this character.")]
        protected float _forwardSpeed = 2.5f;

        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("The backward speed of this character.")]
        protected float _backSpeed = 2.5f;

        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("The sideways speed of this character.")]
        protected float _sideSpeed = 2.5f;
        
        
        public override bool ApplyGravity => false;
        public override bool SnapToGround => true;
        public override float StepCycleLength => _stepLength;

        public override bool IsValid() => Motor.IsGrounded;

        public override Vector3 UpdateVelocity(Vector3 currentVelocity, float deltaTime)
        {
            Vector3 targetVelocity = GetTargetVelocity(Input.MovementInput, currentVelocity);

            float targetAcceleration;

            // Calculate the rate at which the current speed should increase / decrease. 
            if (targetVelocity.sqrMagnitude > 0.001f)
            {
                // Get the velocity mod.
                float velocityMod = Controller.SpeedModifier.EvaluateValue() * Motor.GetSlopeSpeedMultiplier();

                // Finally multiply the target velocity with the velocity modifier.
                targetVelocity *= velocityMod;

                targetAcceleration = Controller.AccelerationModifier.EvaluateValue();
            }
            else
                targetAcceleration = Controller.DecelerationModifier.EvaluateValue();

            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, targetAcceleration * deltaTime);

            return currentVelocity;
        }

        protected virtual Vector3 GetTargetVelocity(Vector3 moveDirection, Vector3 currentVelocity)
        {
            bool wantsToMove = moveDirection.sqrMagnitude > 0f;
            moveDirection = wantsToMove ? moveDirection : currentVelocity.normalized;

            float desiredSpeed = 0f;

            if (wantsToMove)
            {
                // Set the default speed (forward)
                desiredSpeed = _forwardSpeed;

                // Sideways movement
                if (Mathf.Abs(Input.RawMovementInput.x) > 0.01f)
                    desiredSpeed = _sideSpeed;

                // Back movement
                if (Input.RawMovementInput.y < 0f)
                    desiredSpeed = _backSpeed;
            }

            return moveDirection * desiredSpeed;
        }
    }
}