using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [NestedObjectPath(MenuName = "Tween Feedback")]
    public sealed class TweenFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField]
        private bool _useUnscaledTime = false;

        [SerializeField]
        private TweenSequence _normalTween = new();

        [SerializeField]
        private TweenSequence _highlightedTween = new();

        [SerializeField]
        private TweenSequence _selectedTween = new();

        [SerializeField]
        private TweenSequence _pressedTween = new();


        public override void OnNormal(bool instant) => _normalTween.Play(_useUnscaledTime);
        public override void OnHighlighted(bool instant) => _highlightedTween.Play(_useUnscaledTime);
        public override void OnSelected(bool instant) => _selectedTween.Play(_useUnscaledTime);
        public override void OnPressed(bool instant) => _pressedTween.Play(_useUnscaledTime);

#if UNITY_EDITOR
        private void OnValidate()
        {
            _normalTween.Validate(gameObject);
            _highlightedTween.Validate(gameObject);
            _selectedTween.Validate(gameObject);
            _pressedTween.Validate(gameObject);
        }
#endif
    }
}