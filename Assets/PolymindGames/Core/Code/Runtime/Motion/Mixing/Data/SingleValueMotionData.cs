using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public sealed class SingleValueMotionData : IMotionData
    {
        [SerializeField]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField, Range(-100f, 100f), SpaceArea]
        private float _positionValue;

        [SerializeField, Range(-100f, 100f)]
        private float _rotationValue;
        
        
        public SpringSettings PositionSettings => _positionSpring;
        public SpringSettings RotationSettings => _rotationSpring;
        public float PositionValue => _positionValue;
        public float RotationValue => _rotationValue;
    }
}