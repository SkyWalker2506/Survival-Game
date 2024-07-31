using UnityEngine;

namespace PolymindGames.MovementSystem
{
    public interface IMovementInputProvider
    {
        Vector2 RawMovementInput { get; }
        Vector3 MovementInput { get; }
        bool RunInput { get; }
        bool CrouchInput { get; }
        bool JumpInput { get; }

        void UseCrouchInput();
        void UseRunInput();
        void UseJumpInput();
    }
}