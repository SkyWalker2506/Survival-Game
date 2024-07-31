using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<float> TweenCanvasGroupAlpha(this CanvasGroup self, float to, float duration)
            => Tweens.Get(self.alpha, to, duration, (float value) => self.alpha = value);
    }

    [Serializable]
    public sealed class CanvasGroupAlphaTween : ComponentTween<float, CanvasGroup>
    {
        protected override float GetValueFromComponent() => _component.alpha;
        protected override void OnUpdate(float value) => _component.alpha = value;
    }
}