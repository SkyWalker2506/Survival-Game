using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector2> TweenAnchoredScale(this RectTransform self, Vector2 to, float duration) =>
            Tweens.Get(self.sizeDelta, to, duration, (Vector2 value) => self.sizeDelta = value);
    }

    [Serializable]
    public sealed class AnchoredScaleTween : ComponentTween<Vector2, RectTransform>
    {
        protected override Vector2 GetValueFromComponent() => _component.sizeDelta;
        protected override void OnUpdate(Vector2 value) => _component.sizeDelta = value;
    }
}