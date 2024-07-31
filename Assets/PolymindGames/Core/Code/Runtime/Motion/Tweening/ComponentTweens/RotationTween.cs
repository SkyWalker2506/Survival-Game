using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector3> TweenRotation(this Transform self, Vector3 to, float duration) =>
            Tweens.Get(self.eulerAngles, to, duration, (Vector3 value) => self.eulerAngles = value);

        public static ValueTween<Quaternion> TweenRotation(this Transform self, Quaternion to, float duration) =>
            Tweens.Get(self.rotation, to, duration, (Quaternion value) => self.rotation = value);
    }

    [Serializable]
    public sealed class RotationTween : ComponentTween<Vector3, Transform>
    {
        protected override Vector3 GetValueFromComponent() => _component.eulerAngles;
        protected override void OnUpdate(Vector3 value) => _component.eulerAngles = value;
    }
}