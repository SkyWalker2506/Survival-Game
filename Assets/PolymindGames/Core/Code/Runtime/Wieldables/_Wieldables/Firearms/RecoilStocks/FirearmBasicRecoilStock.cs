using System;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Recoil/Basic Recoil-Stock")]
    public class FirearmBasicRecoilStock : FirearmRecoilStockBehaviour
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

            DoRecoil(recoilMod);
        }

        private void DoRecoil(float recoilMod)
        {
            // Head recoil
            var headMixer = Wieldable.Motion.HeadMotionMixer;
            if (headMixer.TryGetMotionOfType<AdditiveForceMotion>(out var headForceMotion))
                headForceMotion.AddRotationForce(_headRecoil.HeadForce, recoilMod, SpringType.FastSpring);

            if (headMixer.TryGetMotionOfType<AdditiveShakeMotion>(out var headShakeMotion))
            {
                headShakeMotion.AddPositionShake(_headRecoil.HeadPositionShake, recoilMod);
                headShakeMotion.AddRotationShake(_headRecoil.HeadRotationShake, recoilMod);
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
            public RandomSpringForce3D HeadForce;
            public ShakeSettings3D HeadPositionShake;
            public ShakeSettings3D HeadRotationShake;

            public static HeadRecoil Default =>
                new()
                {
                    HeadForce = RandomSpringForce3D.Default,
                    HeadPositionShake = ShakeSettings3D.Default,
                    HeadRotationShake = ShakeSettings3D.Default
                };
        }
        #endregion
    }
}