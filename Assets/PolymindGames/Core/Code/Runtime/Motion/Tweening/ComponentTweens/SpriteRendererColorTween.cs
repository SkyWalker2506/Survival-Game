using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Color> TweenSpriteRendererColor(this SpriteRenderer self, Color to, float duration) =>
            Tweens.Get(self.color, to, duration, (Color value) => self.color = value);
    }

    [Serializable]
    public sealed class SpriteRendererColorTween : ComponentTween<Color, SpriteRenderer>
    {
        protected override Color GetValueFromComponent() => _component.color;
        protected override void OnUpdate(Color value) => _component.color = value;
    }
}