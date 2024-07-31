using UnityEngine;
using System;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<float> TweenCanvasRendererAlpha(this CanvasRenderer self, float to, float duration) =>
            Tweens.Get(self.GetAlpha(), to, duration, self.SetAlpha);
    }

    [Serializable]
    public sealed class CanvasRendererAlphaTween : ComponentTween<float, CanvasRenderer>
    {
        protected override float GetValueFromComponent() => _component.GetAlpha();
        protected override void OnUpdate(float value) => _component.SetAlpha(value);
    }
}