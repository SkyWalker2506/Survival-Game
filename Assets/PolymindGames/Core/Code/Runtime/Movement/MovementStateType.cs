using System;
using System.Linq;

namespace PolymindGames
{
    public enum MovementStateType
    {
        None = 0,
        Idle = 1,
        Walk = 2,
        Run = 3,
        Slide = 4,
        Crouch = 5,
        Jump = 6,
        Airborne = 7

        // Mantle = 7,       // Not Implemented
        // Swim = 8,         // Not Implemented
        // LadderClimb = 9,   // Not Implemented
        // Here you can add more state types.
    }

    public enum MovementStateTransitionType
    {
        Enter = 0,
        Exit = 1,
        Both = 2
    }

    public static class MovementStateTypeUtils
    {
        public static readonly MovementStateType[] MovementStateTypes = Enum.GetValues(typeof(MovementStateType)).Cast<MovementStateType>().ToArray();
        public static readonly int MovementStateTypesCount = MovementStateTypes.Length;

        public static int GetValue(this MovementStateType stateType) => (int)stateType;
    }
}