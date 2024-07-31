using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public struct SpringForce2D
    {
        public Vector2 Force;

        [Range(0, 100)]
        public float Duration;


        public static readonly SpringForce2D Default = new(Vector3.zero, 0.125f);

        public SpringForce2D(Vector2 force, float frames)
        {
            Force = force;
            Duration = Mathf.Max(0, frames);
        }

        public bool IsEmpty() => Duration == 0f;

        public static SpringForce2D operator *(SpringForce2D springForce, float mod)
        {
            springForce.Force *= mod;
            return springForce;
        }
    }

    [Serializable]
    public struct DelayedSpringForce2D
    {
        [Range(0f, 10f)]
        public float Delay;

        public SpringForce2D SpringForce;


        public static readonly DelayedSpringForce2D Default = new(SpringForce2D.Default, 0f);

        public DelayedSpringForce2D(SpringForce2D force, float delay)
        {
            Delay = delay;
            SpringForce = force;
        }
    }
}