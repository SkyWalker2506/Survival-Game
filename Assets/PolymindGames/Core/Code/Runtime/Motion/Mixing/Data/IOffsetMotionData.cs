using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public interface IOffsetMotionData : IMotionData
    {
        public SpringSettings PositionSettings { get; }
        public SpringSettings RotationSettings { get; }
        public Vector3 PositionOffset { get; }
        public Vector3 RotationOffset { get; }
        public SpringForce3D EnterForce { get; }
        public SpringForce3D ExitForce { get; }
    }
}