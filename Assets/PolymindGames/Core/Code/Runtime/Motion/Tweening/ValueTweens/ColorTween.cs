using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class ColorTween : ValueTween<Color>
    {
        public override Color CombineValues(Color value1, Color value2) => value1 + value2;
        
        protected override void OnUpdate(float easedTime)
        {
            ValueAt.r = Mathf.LerpUnclamped(ValueFrom.r, ValueTo.r, easedTime);
            ValueAt.g = Mathf.LerpUnclamped(ValueFrom.g, ValueTo.g, easedTime);
            ValueAt.b = Mathf.LerpUnclamped(ValueFrom.b, ValueTo.b, easedTime);
            ValueAt.a = Mathf.LerpUnclamped(ValueFrom.a, ValueTo.a, easedTime);
        }
    }
}