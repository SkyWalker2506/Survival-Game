using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static partial class TweenExtensions
    {
        public static ValueTween<Vector3> TweenLocalRotation(this Transform self, Vector3 to, float duration) =>
            Tweens.Get(self.localEulerAngles, to, duration, (Vector3 value) => self.localEulerAngles = value);

        public static ValueTween<Quaternion> TweenLocalRotation(this Transform self, Quaternion to, float duration) =>
            Tweens.Get(self.localRotation, to, duration, (Quaternion value) => self.localRotation = value);
    }

    [Serializable]
    public sealed class LocalRotationTween : ComponentTween<Vector3, Transform>
    {
        protected override Vector3 GetValueFromComponent()
        {
            Vector3 localEulerAngles = _component.localEulerAngles;
            return localEulerAngles;
        }

        protected override void OnUpdate(Vector3 value) => _component.localEulerAngles = value;
    }
}