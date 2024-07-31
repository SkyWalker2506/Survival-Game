using System;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Recoil/Pattern Recoil-Stock")]
    public class FirearmPatternRecoilStock : FirearmRecoilStockBehaviour
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Motion")]
        private float _hipfireRecoilMod = 1f;

        [SerializeField, Range(0f, 5f)]
        private float _aimRecoilMod = 0.7f;

        [SerializeField]
        private HeadRecoil _headRecoil = HeadRecoil.Default;

        [SerializeField, EndGroup]
        private HandsRecoil _handsRecoil = HandsRecoil.Default;


        public override void DoRecoil(bool isAiming, float heatValue, float triggerValue)
        {
            float recoilMod = triggerValue * RecoilMultiplier;
            recoilMod *= isAiming ? _aimRecoilMod : _hipfireRecoilMod;

            DoRecoil(heatValue, recoilMod);
        }

        private void DoRecoil(float heatValue, float recoilMod)
        {
            // Head recoil
            var headMixer = Wieldable.Motion.HeadMotionMixer;

            var headRecoilMotion = headMixer.GetMotionOfType<CameraRecoilMotion>();
            Vector2 patternRecoil = _headRecoil.RecoilCurve.Evaluate(heatValue);
            headRecoilMotion.AddRecoil(patternRecoil, _headRecoil.RecoveryDelay);

            if (headMixer.TryGetMotionOfType<AdditiveForceMotion>(out var headForceMotion))
                headForceMotion.AddRotationForce(_headRecoil.RandomForce, recoilMod);

            if (headMixer.TryGetMotionOfType<AdditiveShakeMotion>(out var headShakeMotion))
            {
                headShakeMotion.AddPositionShake(_headRecoil.PositionShake, recoilMod);
                headShakeMotion.AddRotationShake(_headRecoil.RotationShake, recoilMod);
            }

            // Wieldable recoil
            var wieldableMixer = Wieldable.Motion.HandsMotionMixer;

            if (wieldableMixer.TryGetMotionOfType<AdditiveForceMotion>(out var wieldableForceMotion))
            {
                wieldableForceMotion.AddPositionForce(_handsRecoil.PositionForce, recoilMod, SpringType.FastSpring);
                wieldableForceMotion.AddRotationForce(_handsRecoil.RotationForce, recoilMod, SpringType.FastSpring);
            }

            if (wieldableMixer.TryGetMotionOfType<AdditiveShakeMotion>(out var wieldableShakeMotion))
            {
                wieldableShakeMotion.AddPositionShake(_handsRecoil.PositionShake, recoilMod);
                wieldableShakeMotion.AddRotationShake(_handsRecoil.RotationShake, recoilMod);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyRecoilSprings();
        }

        private void ApplyRecoilSprings()
        {
            var headMixer = Wieldable.Motion.HeadMotionMixer;
            if (!headMixer.TryGetMotionOfType(out CameraRecoilMotion headRecoilMotion))
            {
                headRecoilMotion = new CameraRecoilMotion(Wieldable.Character);
                headMixer.AddMixedMotion(headRecoilMotion);
            }

            headRecoilMotion.SetRecoilSprings(ref _headRecoil.RecoilSpring, _headRecoil.RecoilRecoverySpring);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && Wieldable != null)
                ApplyRecoilSprings();
        }
#endif
        
        #region Internal
        [Serializable]
        protected struct HandsRecoil
        {
            public RandomSpringForce3D PositionForce;
            public ShakeSettings3D PositionShake;
            public RandomSpringForce3D RotationForce;
            public ShakeSettings3D RotationShake;

            public static HandsRecoil Default =>
                new()
                {
                    PositionForce = RandomSpringForce3D.Default,
                    PositionShake = ShakeSettings3D.Default,
                    RotationForce = RandomSpringForce3D.Default,
                    RotationShake = ShakeSettings3D.Default
                };
        }

        [Serializable]
        protected struct HeadRecoil
        {
            public RandomSpringForce3D RandomForce;
            public ShakeSettings3D PositionShake;
            public ShakeSettings3D RotationShake;

            [Title("Pattern")]
            public AnimCurves2D RecoilCurve;
            public SpringSettings RecoilSpring;

            [SpaceArea(3f)]
            [Range(0.05f, 0.3f)]
            public float RecoveryDelay;
            public SpringSettings RecoilRecoverySpring;


            public static HeadRecoil Default =>
                new()
                {
                    RandomForce = RandomSpringForce3D.Default,
                    PositionShake = ShakeSettings3D.Default,
                    RotationShake = ShakeSettings3D.Default,
                    RecoveryDelay = 0.1f,
                    RecoilSpring = new SpringSettings(50f, 150f, 1f, 1.5f),
                    RecoilRecoverySpring = new SpringSettings(12, 100, 1, 1),
                    RecoilCurve = new AnimCurves2D()
                };
        }
        #endregion
    }
}