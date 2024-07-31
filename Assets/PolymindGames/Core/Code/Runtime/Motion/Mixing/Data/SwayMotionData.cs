using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public sealed class SwayMotionData : IMotionData
    {
        [SerializeField, Range(0f, 100f)]
        private float _maxSwayLength = 10f;

        [SerializeField, SpaceArea]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField]
        private Vector3 _positionSway;

        [SerializeField, SpaceArea]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField]
        private Vector3 _rotationSway;
        
        
        public SpringSettings PositionSettings => _positionSpring;
        public SpringSettings RotationSettings => _rotationSpring;
        public float MaxSwayLength => _maxSwayLength;
        public Vector3 PositionSway => _positionSway;
        public Vector3 RotationSway => _rotationSway;
    }
}