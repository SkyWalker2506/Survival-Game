using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [CreateAssetMenu(menuName = MOTION_DATA_MENU_PATH + "Retraction", fileName = "Retraction_")]
    public sealed class RetractionMotionData : MotionData
    {
        [SerializeField, Range(0.1f, 5f)]
        private float _retractionDistance = 0.55f;

        [SerializeField, SpaceArea]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField]
        private Vector3 _positionOffset;

        [SerializeField, SpaceArea]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField]
        private Vector3 _rotationOffset;
        
        
        public float RetractionDistance => _retractionDistance;
        public SpringSettings PositionSettings => _positionSpring;
        public SpringSettings RotationSettings => _rotationSpring;
        public Vector3 PositionOffset => _positionOffset;
        public Vector3 RotationOffset => _rotationOffset;
    }
}