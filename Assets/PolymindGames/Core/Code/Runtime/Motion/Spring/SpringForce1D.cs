using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public struct SpringForce1D
    {
        public float Force;

        [Range(0, 100)]
        public float Duration;


        public static readonly SpringForce1D Default = new(0f, 0.125f);

        public SpringForce1D(float force, float duration)
        {
            Force = force;
            Duration = Mathf.Max(0, duration);
        }

        public static SpringForce1D operator *(SpringForce1D springForce, float mod)
        {
            springForce.Force *= mod;
            return springForce;
        }
    }

    [Serializable]
    public struct DelayedSpringForce1D
    {
        [Range(0f, 2f)]
        public float Delay;

        public SpringForce1D SpringForce;


        public static readonly DelayedSpringForce1D Default = new(SpringForce1D.Default, 0f);

        public DelayedSpringForce1D(SpringForce1D force, float delay)
        {
            Delay = delay;
            SpringForce = force;
        }
    }
}