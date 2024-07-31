using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [CreateAssetMenu(menuName = MOTION_DATA_MENU_PATH + "Simple Bob", fileName = "Bob_")]
    public sealed class SimpleBobMotionData : MotionData, IBobMotionData
    {
        [SerializeField]
        private BobMode _bobType = BobMode.StepCycleBased;

        [SerializeField, Range(0.01f, 10f)]
        [ShowIf(nameof(_bobType), BobMode.TimeBased)]
        private float _bobSpeed = 1f;

        [SerializeField, SpaceArea]
        private Vector3 _positionAmplitude = Vector3.zero;

        [SerializeField]
        private Vector3 _rotationAmplitude = Vector3.zero;
        
        
        public BobMode BobType => _bobType;
        public float BobSpeed => _bobSpeed;
        public SpringSettings PositionSettings => SpringSettings.Default;
        public SpringSettings RotationSettings => SpringSettings.Default;
        public SpringForce3D PositionStepForce => SpringForce3D.Default;
        public SpringForce3D RotationStepForce => SpringForce3D.Default;
        public Vector3 PositionAmplitude => _positionAmplitude;
        public Vector3 RotationAmplitude => _rotationAmplitude;
    }
}