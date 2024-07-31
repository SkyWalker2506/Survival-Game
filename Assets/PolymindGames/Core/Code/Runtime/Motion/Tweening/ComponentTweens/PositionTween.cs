using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector3> TweenPosition(this Transform self, Vector3 to, float duration) =>
            Tweens.Get(self.position, to, duration, (Vector3 value) => self.position = value);
    }

    [Serializable]
    public sealed class PositionTween : ComponentTween<Vector3, Transform>
    {
        protected override Vector3 GetValueFromComponent() => _component.position;
        protected override void OnUpdate(Vector3 value) => _component.position = value;
    }
}