using PolymindGames.WorldManagement;
using UnityEngine;

namespace PolymindGames.Demo
{
    public sealed class ToggleTimeProgression : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.1f)]
        private float _dayTimeIncrementPerSecond;
        
        
        public void ToggleTimeProgress()
        {
            var time = World.Instance.Time;
            time.DayTimeIncrementPerSecond = time.DayTimeIncrementPerSecond == 0f ? _dayTimeIncrementPerSecond : 0f;
        }
    }
}