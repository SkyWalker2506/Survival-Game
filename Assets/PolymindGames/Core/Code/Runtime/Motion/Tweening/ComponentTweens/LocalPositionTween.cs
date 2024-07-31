using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector3> TweenLocalPosition(this Transform self, Vector3 to, float duration) =>
            Tweens.Get(self.localPosition, to, duration, (Vector3 value) => self.localPosition = value);
    }

    [Serializable]
    public sealed class LocalPositionTween : ComponentTween<Vector3, Transform>
    {
        protected override Vector3 GetValueFromComponent() => _component.localPosition;
        protected override void OnUpdate(Vector3 value) => _component.localPosition = value;
    }
}