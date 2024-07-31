using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class ControlUI : MonoBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        private InputActionReference _inputAction;

        [SerializeField]
        private string _textTemplate = "{}";

        [SerializeField, EndGroup]
        private Color _textInputColor = Color.white;


        private void Start()
        {
            if (_inputAction == null)
            {
                Debug.LogError("ACTION NULL", gameObject);
                return;
            }

            SetText();

#if !UNITY_EDITOR
            Destroy(this);
#endif
        }

        private void SetText()
        {
            if (TryGetComponent<TextMeshProUGUI>(out var text))
            {
                text.text = _textTemplate.Replace("{}",
                    $"<color={ColorToHex(_textInputColor)}>{FormatBinding(_inputAction.action.bindings[0])}</color>");
            }
        }

        private static string FormatBinding(in InputBinding binding)
        {
            var displayString = binding.ToDisplayString();
            if (displayString == "Escape")
                return "Esc";

            return displayString;
        }

        private static string ColorToHex(Color32 color)
        {
            string hex = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_textTemplate.Contains("{}"))
                _textTemplate += "{}";

            if (_inputAction != null)
                SetText();
        }
#endif
    }
}