using System;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Color> TweenGraphicColor(this Graphic self, Color to, float duration) =>
            Tweens.Get(self.color, to, duration, (Color value) => self.color = value);
    }

    [Serializable]
    public sealed class GraphicColorTween : ComponentTween<Color, Graphic>
    {
        protected override Color GetValueFromComponent() => _component.color;
        protected override void OnUpdate(Color color) => _component.color = color;
    }
}