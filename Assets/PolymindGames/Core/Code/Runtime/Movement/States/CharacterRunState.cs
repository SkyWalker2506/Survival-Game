using UnityEngine;

namespace PolymindGames.MovementSystem
{
    [NestedObjectPath(MenuName = "Run State")]
    public sealed class CharacterRunState : CharacterGroundedState
    {
        [SerializeField, Range(0f, 20f), SpaceArea]
        private float _minRunSpeed = 3f;
        
        
        public override MovementStateType StateType => MovementStateType.Run;

        public override bool IsValid()
        {
            Vector2 rawMoveInput = Input.RawMovementInput;
            bool noInput = rawMoveInput == Vector2.zero;
            bool wantsToMoveBack = rawMoveInput.y < 0f;
            bool wantsToMoveOnlySideways = Mathf.Abs(rawMoveInput.x) > 0.9f;

            if (noInput || wantsToMoveBack || wantsToMoveOnlySideways)
                return false;

            var velocity = Motor.Velocity;
            var hVelocity = new Vector2(velocity.x, velocity.z);

            bool canRun = rawMoveInput.sqrMagnitude > 0.1f && hVelocity.magnitude > _minRunSpeed
                                                           && Motor.CanSetHeight(Motor.DefaultHeight) && Motor.IsGrounded;

            return canRun;
        }

        public override void OnEnter(MovementStateType prevStateType)
        {
            base.OnEnter(prevStateType);

            Motor.Height = Motor.DefaultHeight;
            Input.UseCrouchInput();
        }

        public override void UpdateLogic()
        {
            if (enabled)
            {
                // Transition to an airborne state.
                if (Controller.TrySetState(MovementStateType.Airborne)) return;

                // Transition to a walk state.
                bool runInput = Input.RunInput || InputOptions.Instance.AutoRun;
                if ((!runInput || !IsValid()) && Controller.TrySetState(MovementStateType.Walk)) return;

                // Transition to a slide or crouch state.
                if (Input.CrouchInput && (Controller.TrySetState(MovementStateType.Slide) || Controller.TrySetState(MovementStateType.Crouch))) return;

                // Transition to a jumping state.
                if (Input.JumpInput)
                    Controller.TrySetState(MovementStateType.Jump);
            }
            else
            {
                // Transition to a walk state.
                if (Controller.TrySetState(MovementStateType.Walk)) return;

                // Transition to an idle state.
                Controller.TrySetState(MovementStateType.Idle);
            }
        }
    }
}