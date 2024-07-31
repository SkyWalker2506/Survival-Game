using TMPro;
using UnityEngine;

namespace PolymindGames
{
    public sealed class SelectableTextUI : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private TextMeshProUGUI _text;

        [SerializeField, Range(0, 100)]
        private int _selectedFontSize = 15;

        [SerializeField, EndGroup]
        private Color _selectedTextColor = Color.white;

        private float _originalFontSize;
        private FontStyles _originalFontStyle;
        private Color _originalTextColor;


        private void Awake()
        {
            InitializeNameText();
        }

        public void SelectText()
        {
            _text.fontStyle = FontStyles.Bold;
            _text.fontSize = _selectedFontSize;
            _text.color = _selectedTextColor;
        }

        public void DeselectText()
        {
            _text.fontStyle = _originalFontStyle;
            _text.fontSize = _originalFontSize;
            _text.color = _originalTextColor;
        }

        private void InitializeNameText()
        {
            _originalFontSize = _text.fontSize;
            _originalTextColor = _text.color;
            _originalFontStyle = _text.fontStyle;
        }
    }
}