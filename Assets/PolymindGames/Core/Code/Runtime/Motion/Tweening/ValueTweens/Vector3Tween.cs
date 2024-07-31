using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class Vector3Tween : ValueTween<Vector3>
    {
        public override Vector3 CombineValues(Vector3 value1, Vector3 value2) => value1 + value2;
        
        protected override void OnUpdate(float easedTime)
        {
            ValueAt.x = Mathf.LerpUnclamped(ValueFrom.x, ValueTo.x, easedTime);
            ValueAt.y = Mathf.LerpUnclamped(ValueFrom.y, ValueTo.y, easedTime);
            ValueAt.z = Mathf.LerpUnclamped(ValueFrom.z, ValueTo.z, easedTime);
        }
    }
}