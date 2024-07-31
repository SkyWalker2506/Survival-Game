using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector3> TweenLocalScale(this Transform self, Vector3 to, float duration) =>
            Tweens.Get(self.localScale, to, duration, (Vector3 value) => self.localScale = value);
    }

    [Serializable]
    public sealed class LocalScaleTween : ComponentTween<Vector3, Transform>
    {
        protected override Vector3 GetValueFromComponent() => _component.localScale;
        protected override void OnUpdate(Vector3 value) => _component.localScale = value;
    }
}