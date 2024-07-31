using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [CreateAssetMenu(menuName = MOTION_DATA_MENU_PATH + "Basic Bob", fileName = "Bob_")]
    public sealed class BasicBobMotionData : MotionData, IBobMotionData
    {
        [SerializeField]
        private BobMode _bobType = BobMode.StepCycleBased;

        [SerializeField, Range(0.01f, 10f)]
        [ShowIf(nameof(_bobType), BobMode.TimeBased)]
        private float _bobSpeed = 1f;

        [SerializeField, SpaceArea]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField]
        private SpringForce3D _positionStepForce = SpringForce3D.Default;

        [SerializeField]
        private Vector3 _positionAmplitude = Vector3.zero;

        [SerializeField, SpaceArea]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField]
        private SpringForce3D _rotationStepForce = SpringForce3D.Default;

        [SerializeField]
        private Vector3 _rotationAmplitude = Vector3.zero;
        
        
        public BobMode BobType => _bobType;
        public float BobSpeed => _bobSpeed;
        public SpringSettings PositionSettings => _positionSpring;
        public SpringSettings RotationSettings => _rotationSpring;
        public SpringForce3D PositionStepForce => _positionStepForce;
        public SpringForce3D RotationStepForce => _rotationStepForce;
        public Vector3 PositionAmplitude => _positionAmplitude;
        public Vector3 RotationAmplitude => _rotationAmplitude;
    }
}