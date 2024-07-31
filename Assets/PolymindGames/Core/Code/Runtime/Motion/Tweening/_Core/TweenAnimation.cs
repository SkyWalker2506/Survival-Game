using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public sealed class TweenAnimation : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private bool _playOnEnable;

        [SerializeField, EndGroup]
        private bool _useUnscaledTime;

        [SerializeField, SpaceArea, BeginGroup, EndGroup]
        private TweenSequence _tweenSequence = new();


        public void PlayAnimation()
        {
            if (UnityUtils.IsPlayMode)
                _tweenSequence.Play(_useUnscaledTime);
        }

        public void CancelAnimation()
        {
            if (UnityUtils.IsPlayMode)
                _tweenSequence.Cancel();
        }

        private void OnEnable()
        {
            if (_playOnEnable)
                PlayAnimation();
        }

        private void OnDisable()
        {
            CancelAnimation();
        }

#if UNITY_EDITOR
        private void OnValidate() => _tweenSequence.Validate(gameObject);
#endif
    }
}