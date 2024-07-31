using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public sealed class AnimCurves2D
    {
        [SerializeField, Range(-25f, 25f)]
        private float _multiplier = 1f;

        [SerializeField, Header("Curves")]
        private AnimationCurve _xCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private AnimationCurve _yCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private float? _duration;
        
        
        public float Duration => _duration ??= GetDuration();

        public Vector2 Evaluate(float time)
        {
            return new Vector2
            {
                x = _xCurve.Evaluate(time) * _multiplier,
                y = _yCurve.Evaluate(time) * _multiplier
            };
        }

        public Vector2 Evaluate(float xTime, float yTime)
        {
            return new Vector2
            {
                x = _xCurve.Evaluate(xTime) * _multiplier,
                y = _yCurve.Evaluate(yTime) * _multiplier
            };
        }

        public Vector3 EvaluateVec3(float xTime, float zTime)
        {
            return new Vector3
            {
                x = _xCurve.Evaluate(xTime) * _multiplier,
                y = 0f,
                z = _yCurve.Evaluate(zTime) * _multiplier
            };
        }

        private float GetDuration()
        {
            float curvesDuration = 0f;

            curvesDuration = GetKeyTimeLargerThan(_xCurve, curvesDuration);
            curvesDuration = GetKeyTimeLargerThan(_yCurve, curvesDuration);

            return curvesDuration;
        }

        private static float GetKeyTimeLargerThan(AnimationCurve animCurve, float largerThan)
        {
            foreach (var key in animCurve.keys)
            {
                if (key.time > largerThan)
                    largerThan = key.time;
            }

            return largerThan;
        }
    }
}