using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [NestedObjectPath(MenuName = "Highlight Graphic Feedback")]
    public sealed class HighlightGraphicFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField, NotNull]
        private Image _selectionGraphic;

        [SerializeField]
        private Color _selectedColor;

        [SerializeField]
        private Color _highlightedColor;


        public override void OnNormal(bool instant)
        {
            _selectionGraphic.enabled = false;
        }

        public override void OnHighlighted(bool instant)
        {
            _selectionGraphic.enabled = true;
            _selectionGraphic.color = _highlightedColor;
        }

        public override void OnSelected(bool instant)
        {
            _selectionGraphic.enabled = true;
            _selectionGraphic.color = _selectedColor;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_selectionGraphic == null)
                _selectionGraphic = GetComponentInChildren<Image>();
        }
#endif
    }
}