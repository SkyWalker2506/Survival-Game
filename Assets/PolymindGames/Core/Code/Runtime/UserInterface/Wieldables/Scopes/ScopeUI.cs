using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(CanvasGroup))]
    [DefaultExecutionOrder(-1)]
    public sealed class ScopeUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private ValueTween<float> _tween;

        
        public void Show(float showDuration)
        {
            gameObject.SetActive(true);

            if (showDuration > 0.01f)
            { 
                _tween.SetTo(1f)
                    .SetDuration(showDuration)
                    .SetCompleteCallback(null)
                    .Play(this);
            }
            else
                _canvasGroup.alpha = 1f;
        }

        public void Hide(float hideDuration)
        {
            if (hideDuration > 0.01f)
            {
                _tween.SetTo(0f)
                    .SetDuration(hideDuration)
                    .SetCompleteCallback(() => gameObject.SetActive(false))
                    .Play(this);
            }
            else
            {
                gameObject.SetActive(false);
                _canvasGroup.alpha = 0f;
            }
        }

        public void SetZoomLevel(int level) { }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _tween = _canvasGroup.TweenCanvasGroupAlpha(0f, 0f)
                .SetEase(EaseType.CubicIn);
        }

        private void OnDestroy() => Tweens.Release(_tween);
    }
}