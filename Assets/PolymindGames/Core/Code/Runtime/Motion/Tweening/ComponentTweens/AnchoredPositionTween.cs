using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector2> TweenAnchoredPosition(this RectTransform self, Vector2 to, float duration) =>
            Tweens.Get(self.anchoredPosition, to, duration, (Vector2 value) => self.anchoredPosition = value);
    }

    [Serializable]
    public sealed class AnchoredPositionTween : ComponentTween<Vector2, RectTransform>
    {
        protected override Vector2 GetValueFromComponent() => _component.anchoredPosition;
        protected override void OnUpdate(Vector2 value) => _component.anchoredPosition = value;
    }
}