using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public sealed class AnimCurve1D
    {
        [SerializeField, Range(-10f, 10f)]
        private float _multiplier = 1f;

        [SerializeField]
        private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        
        public float Duration => _curve[_curve.length - 1].time;

        public float Evaluate(float time) => _curve.Evaluate(time) * _multiplier;
    }
}