using UnityEngine;
using System;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public sealed class CurvesMotionData : IMotionData
    {
        [SerializeField]
        private SpringSettings _positionSpring = SpringSettings.Default;

        [SerializeField]
        private AnimCurves3D _positionCurves;

        [SerializeField, SpaceArea]
        private SpringSettings _rotationSpring = SpringSettings.Default;

        [SerializeField]
        private AnimCurves3D _rotationCurves;
        
        
        public SpringSettings PositionSettings => _positionSpring;
        public SpringSettings RotationSettings => _rotationSpring;
        public AnimCurves3D PositionCurves => _positionCurves;
        public AnimCurves3D RotationCurves => _rotationCurves;
    }
}