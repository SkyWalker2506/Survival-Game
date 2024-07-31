using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [NestedObjectPath(MenuName = "Color Tint Feedback")]
    public sealed class ColorTintFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField, NotNull]
        private Graphic _targetGraphic;

        [SerializeField]
        private Color _normalColor = Color.grey;

        [SerializeField]
        private Color _highlightedColor = Color.grey;

        [SerializeField]
        private Color _pressedColor = Color.grey;

        [SerializeField]
        private Color _selectedColor = Color.grey;

        [SerializeField]
        private Color _disabledColor = Color.grey;

        [SerializeField, Range(0.01f, 1f)]
        private float _fadeDuration = 0.1f;


        public override void OnNormal(bool instant)
        {
            _targetGraphic.CrossFadeColor(_normalColor, instant
                ? 0f
                : _fadeDuration, true, true);
        }

        public override void OnHighlighted(bool instant)
        {
            _targetGraphic.CrossFadeColor(_highlightedColor, instant
                ? 0f
                : _fadeDuration, true, true);
        }

        public override void OnSelected(bool instant)
        {
            _targetGraphic.CrossFadeColor(_selectedColor, instant
                ? 0f
                : _fadeDuration, true, true);
        }

        public override void OnPressed(bool instant)
        {
            _targetGraphic.CrossFadeColor(_pressedColor, instant
                ? 0f
                : _fadeDuration, true, true);
        }

        public override void OnDisabled(bool instant)
        {
            _targetGraphic.CrossFadeColor(_disabledColor, instant
                ? 0f
                : _fadeDuration, true, true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_targetGraphic == null || !_targetGraphic.transform.IsChildOf(transform))
                _targetGraphic = GetComponentInChildren<Graphic>();

            if (TryGetComponent(out CanvasRenderer canRenderer))
            {
                if (TryGetComponent<SelectableUI>(out var selectable))
                    canRenderer.SetColor(selectable.IsSelectable ? _normalColor : _disabledColor);
                else
                    canRenderer.SetColor(_normalColor);
            }
        }
#endif
    }
}