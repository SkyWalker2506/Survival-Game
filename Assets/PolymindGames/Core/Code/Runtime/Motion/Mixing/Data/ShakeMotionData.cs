using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public struct ShakeData
    {
        [Range(0f, 10f)]
        public float Multiplier;

        [InLineEditor]
        public ShakeMotionData Shake;
        
        
        public readonly bool IsPlayable => Shake != null && Multiplier > 0.01f;
    }

    [CreateAssetMenu(menuName = MOTION_DATA_MENU_PATH + "Shake", fileName = "Shake_")]
    public sealed class ShakeMotionData : MotionData
    {
        [SerializeField]
        private ShakeSettings3D _positionShake = ShakeSettings3D.Default;

        [SerializeField]
        private ShakeSettings3D _rotationShake = ShakeSettings3D.Default;
        
        
        public ShakeSettings3D PositionShake => _positionShake;
        public ShakeSettings3D RotationShake => _rotationShake;
    }
}