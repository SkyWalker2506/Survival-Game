using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Panels/Tween Panel")]
    public sealed class TweenPanelUI : AudioPanelUI
    {
        [SerializeField, BeginGroup("Animation")]
        private bool _useUnscaledTime = false;

        [SerializeField]
        private TweenSequence _showAnimation;

        [SerializeField, EndGroup]
        private TweenSequence _hideAnimation;


        protected override void OnShowPanel(bool show)
        {
            base.OnShowPanel(show);

            if (show)
            {
                _hideAnimation.Cancel();
                _showAnimation.Play(_useUnscaledTime);
            }
            else
            {
                _showAnimation.Cancel();
                _hideAnimation.Play(_useUnscaledTime);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _showAnimation.Cancel();
            _hideAnimation.Cancel();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _showAnimation?.Validate(gameObject);
            _hideAnimation?.Validate(gameObject);
        }
#endif
    }
}