using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public interface IMotionData
    { }

    public abstract class MotionData : ScriptableObject, IMotionData
    {
        protected const string MOTION_DATA_MENU_PATH = "Polymind Games/Motion/Data/";
    }
}