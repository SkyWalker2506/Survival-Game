using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [CreateAssetMenu(menuName = MOTION_DATA_MENU_PATH + "Offset", fileName = "Offset_")]
    public sealed class OffsetMotionData : MotionData, IOffsetMotionData
    {
        [SerializeField]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField, SpaceArea]
        private SpringForce3D _enterForce;

        [SerializeField]
        private SpringForce3D _exitForce;

        [SerializeField, SpaceArea]
        private Vector3 _positionOffset;

        [SerializeField]
        private Vector3 _rotationOffset;
        
        
        public SpringSettings PositionSettings => _positionSpring;
        public SpringSettings RotationSettings => _rotationSpring;
        public SpringForce3D EnterForce => _enterForce;
        public SpringForce3D ExitForce => _exitForce;
        public Vector3 PositionOffset => _positionOffset;
        public Vector3 RotationOffset => _rotationOffset;
    }
}