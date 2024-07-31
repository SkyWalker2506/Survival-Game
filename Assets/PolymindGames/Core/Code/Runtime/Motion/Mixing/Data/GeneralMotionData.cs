using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [CreateAssetMenu(menuName = MOTION_DATA_MENU_PATH + "General Motion", fileName = "GeneralMotion")]
    public sealed class GeneralMotionData : MotionData
    {
        [SerializeField, BeginIndent]
        private SwayMotionData _look;

        [SerializeField]
        private SwayMotionData _strafe;

        [SerializeField]
        private CurvesMotionData _jump;

        [SerializeField]
        private CurvesMotionData _land;

        [SerializeField, EndIndent]
        private SingleValueMotionData _fall;
        
        
        public SwayMotionData Look => _look;
        public SwayMotionData Strafe => _strafe;
        public CurvesMotionData Jump => _jump;
        public CurvesMotionData Land => _land;
        public SingleValueMotionData Fall => _fall;
    }
}