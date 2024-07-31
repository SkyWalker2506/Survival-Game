using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class QuaternionTween : ValueTween<Quaternion>
    {
        public override Quaternion CombineValues(Quaternion value1, Quaternion value2) => value1 * value2;
        
        protected override void OnUpdate(float easedTime)
        {
            ValueAt = Quaternion.LerpUnclamped(ValueFrom, ValueTo, easedTime);
        }
    }
}