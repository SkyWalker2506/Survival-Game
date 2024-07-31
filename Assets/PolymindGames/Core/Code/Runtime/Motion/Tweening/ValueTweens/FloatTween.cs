using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class FloatTween : ValueTween<float>
    {
        public override float CombineValues(float value1, float value2) => value1 + value2;
        
        protected override void OnUpdate(float easedTime)
        {
            ValueAt = Mathf.LerpUnclamped(ValueFrom, ValueTo, easedTime);
        }
    }
}