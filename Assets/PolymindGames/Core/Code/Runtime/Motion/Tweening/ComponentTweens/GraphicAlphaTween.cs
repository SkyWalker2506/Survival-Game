using System;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<float> TweenGraphicAlpha(this Graphic self, float to, float duration) =>
            Tweens.Get(self.color.a, to, duration, (float value) =>
            {
                Color color = self.color;
                color.a = value;
                self.color = color;
            });
    }

    [Serializable]
    public sealed class GraphicAlphaTween : ComponentTween<float, Graphic>
    {
        protected override float GetValueFromComponent() => _component.color.a;

        protected override void OnUpdate(float value)
        {
            Color color = _component.color;
            color.a = value;
            _component.color = color;
        }
    }
}