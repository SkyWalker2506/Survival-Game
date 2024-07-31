using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public enum BobMode
    {
        StepCycleBased,
        TimeBased
    }

    public interface IBobMotionData : IMotionData
    {
        public BobMode BobType { get; }
        public float BobSpeed { get; }
        public SpringSettings PositionSettings { get; }
        public SpringSettings RotationSettings { get; }
        public SpringForce3D PositionStepForce { get; }
        public SpringForce3D RotationStepForce { get; }
        public Vector3 PositionAmplitude { get; }
        public Vector3 RotationAmplitude { get; }
    }
}