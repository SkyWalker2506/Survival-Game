using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(Image))]
    [NestedObjectPath(MenuName = "Sprite Swap Feedback")]
    public sealed class SpriteSwapFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField, NotNull]
        private Image _image;

        [SerializeField, HideInInspector]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _highlightedSprite;

        [SerializeField]
        private Sprite _selectedSprite;

        [SerializeField]
        private Sprite _pressedSprite;


        public override void OnNormal(bool instant) => _image.sprite = _normalSprite;
        public override void OnHighlighted(bool instant) => _image.sprite = _highlightedSprite;
        public override void OnSelected(bool instant) => _image.sprite = _selectedSprite;
        public override void OnPressed(bool instant) => _image.sprite = _pressedSprite;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_image == null) _image = GetComponent<Image>();
        }
#endif
    }
}