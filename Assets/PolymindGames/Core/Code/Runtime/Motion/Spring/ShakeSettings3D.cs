using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public struct ShakeSettings3D
    {
        [Range(0f, 10f)]
        public float Duration;

        [Range(0f, 100f)]
        public float Speed;

        [Range(-25, 25f), Title("Amplitude")]
        public float XAmplitude;

        [Range(-25f, 25f)]
        public float YAmplitude;

        [Range(-25f, 25f)]
        public float ZAmplitude;


        public static readonly ShakeSettings3D Default =
            new()
            {
                Duration = 0.2f,
                Speed = 20f,
                XAmplitude = 0f,
                YAmplitude = 0f,
                ZAmplitude = 0f

            };
    }
}