using System;
using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Shake Animator")]
    public sealed class WieldableShakeAnimator : AnimatorEffectTranslator<WieldableShakeAnimator.AnimationData>
    {
        private IShakeHandler _shakeHandler;


        protected override void PlayAnimation(AnimationData anim)
        {
            if (anim.HeadShake.IsPlayable)
                _shakeHandler.AddShake(anim.HeadShake, BodyPoint.Head);
            
            if (anim.HandsShake.IsPlayable)
                _shakeHandler.AddShake(anim.HandsShake, BodyPoint.Hands);
        }

        protected override void Awake()
        {
            base.Awake();
            var wieldable = GetComponentInParent<IWieldable>();
            _shakeHandler = wieldable.Motion.ShakeHandler;
        }
        
        #region Internal
        [Serializable]
        public sealed class AnimationData : AnimatorTranslatorData
        {
            [SpaceArea]
            public ShakeData HeadShake;
            public ShakeData HandsShake;
        }
        #endregion
    }
}