using System;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Camera Curves Animator")]
    public sealed class WieldableCameraCurvesAnimator : AnimatorEffectTranslator<WieldableCameraCurvesAnimator.AnimationData>
    {
        private AdditiveForceMotion _forceMotion;

       
        protected override void PlayAnimation(AnimationData anim)
        {
            _forceMotion.AddRotationCurve(anim.Curves);
        }

        private void OnEnable()
        {
            if (_forceMotion == null)
            {
                var wieldable = GetComponentInParent<IWieldable>();
                _forceMotion = wieldable.Motion.HeadMotionMixer?.GetMotionOfType<AdditiveForceMotion>();
            }
        }

        #region Internal
        [Serializable]
        public sealed class AnimationData : AnimatorTranslatorData
        {
            [SpaceArea(3f)] 
            public AnimCurves3D Curves;
        }
        #endregion
    }
}