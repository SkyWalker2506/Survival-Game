using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class Vector2Tween : ValueTween<Vector2>
    {
        public override Vector2 CombineValues(Vector2 value1, Vector2 value2) => value1 + value2;
        
        protected override void OnUpdate(float easedTime)
        {
            ValueAt.x = Mathf.LerpUnclamped(ValueFrom.x, ValueTo.x, easedTime);
            ValueAt.y = Mathf.LerpUnclamped(ValueFrom.y, ValueTo.y, easedTime);
        }
    }
}