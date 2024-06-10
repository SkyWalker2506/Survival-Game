//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;
using System;

namespace HQFPSTemplate
{
    [CreateAssetMenu(fileName = "Player Movement Profile", menuName = "HQ FPS Template/Player/Movement Profile")]
    public class PlayerMovementProfile : ScriptableObject
    {
        #region Internal
        [Serializable]
        public class MovementStateModule
        {
            public bool Enabled = true;

            [ShowIf("Enabled", true)]
            [Range(1f, 10f)]
            public float SpeedMultiplier = 4.5f;

            [ShowIf("Enabled", true)]
            [Range(0f, 3f)]
            public float StepLength = 1.9f;
        }

        [Serializable]
        public class CoreMovementModule
        {
            public float Gravity = 20f;

            [Range(0f, 20f)]
            public float Acceleration = 5f;

            [Range(0f, 20f)]
            public float Damping = 8f;

            [Range(0f, 1f)]
            public float AirborneControl = 0.15f;

            [Range(0f, 3f)]
            public float StepLength = 1.2f;

            [Range(0f, 10f)]
            public float ForwardSpeed = 2.5f;

            [Range(0f, 10f)]
            public float BackSpeed = 2.5f;

            [Range(0f, 10f)]
            public float SideSpeed = 2.5f;

            public AnimationCurve SlopeSpeedMult = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

            public float AntiBumpFactor = 1f;

            [Range(0f, 1f)]
            public float HeadBounceFactor = 0.65f;
        }

        [Serializable]
        public class JumpModule
        {
            public bool Enabled = true;

            [ShowIf("Enabled", true)]
            [Range(0f, 3f)]
            public float JumpHeight = 1f;

            [ShowIf("Enabled", true)]
            [Range(0f, 1.5f)]
            public float JumpTimer = 0.3f;
        }

        [Serializable]
        public class LowerHeightStateModule : MovementStateModule
        {
            [ShowIf("Enabled", true)]
            [Range(0f, 2f)]
            public float ControllerHeight = 1f;

            [ShowIf("Enabled", true)]
            [Range(0f, 1f)]
            public float TransitionDuration = 0.3f;
        }

        [Serializable]
        public class SlidingModule
        {
            public bool SlopeSlide = true;

            [ShowIf("SlopeSlide", true, 3f)]
            [Range(20f, 90f)]
            public float SlopeSlideTreeshold = 35f;

            [ShowIf("SlopeSlide", true, 3f)]
            [Range(0f, 50f)]
            public float SlopeSlideSpeed = 1f;

            [Space(3f)]

            public bool CrouchSlide = true;

            [ShowIf("CrouchSlide", true, 3f)]
            public float CrouchSlideSpeedThreshold = 5f;
        }
        #endregion

        [Group] public CoreMovementModule CoreMovement;

        [BHeader("States", true)]

        [Group] public MovementStateModule RunState;
        [Group] public LowerHeightStateModule CrouchState;
        [Group] public LowerHeightStateModule ProneState;

        [BHeader("Actions", true)]

        [Group] public JumpModule Jumping;
        [Group] public SlidingModule Sliding;
    }
}
