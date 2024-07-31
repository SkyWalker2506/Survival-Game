using UnityEngine;

namespace PolymindGames.MovementSystem
{
    [NestedObjectPath(MenuName = "Walk State")]
    public sealed class CharacterWalkState : CharacterGroundedState
    {
        public override MovementStateType StateType => MovementStateType.Walk;

        public override bool IsValid() => Motor.IsGrounded && Motor.CanSetHeight(Motor.DefaultHeight);
        public override void OnEnter(MovementStateType prevStateType) => Motor.Height = Motor.DefaultHeight;

        public override void UpdateLogic()
        {
            // Transition to an idle state.
            if ((Input.RawMovementInput.sqrMagnitude < 0.1f && Motor.SimulatedVelocity.sqrMagnitude < 0.01f || !enabled)
                && Controller.TrySetState(MovementStateType.Idle)) return;

            // Transition to a run state.
            if ((Input.RunInput || InputOptions.Instance.AutoRun) && Controller.TrySetState(MovementStateType.Run)) return;

            // Transition to a crouch state.
            if (Input.CrouchInput && Controller.TrySetState(MovementStateType.Crouch)) return;

            // Transition to an airborne state.
            if (!Motor.IsGrounded && Controller.TrySetState(MovementStateType.Airborne)) return;

            // Transition to a jumping state.
            if (Input.JumpInput)
                Controller.TrySetState(MovementStateType.Jump);
        }
    }
}