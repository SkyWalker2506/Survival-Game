using UnityEngine.UI;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class InputOptionsUI : UserOptionsUI<InputOptions>
    {
        [SerializeField, BeginGroup("Toggles")]
        private Toggle _invertMouseToggle; 
        
        [SerializeField, EndGroup]
        private Toggle _autoRunToggle;
        
        [SerializeField, BeginGroup("Sliders"), EndGroup]
        private SliderWidget _mouseSensitivity;
        

        protected override void InitializeWidgets()
        {
            _autoRunToggle.onValueChanged.AddListener(UpdateAutoRun);
            _invertMouseToggle.onValueChanged.AddListener(UpdateMouseInvert);
            _mouseSensitivity.Slider.onValueChanged.AddListener(UpdateMouseSensitivity);
        }

        protected override void ResetWidgets()
        {
            _autoRunToggle.isOn = Settings.AutoRun;
            _invertMouseToggle.isOn = Settings.InvertMouse;
            _mouseSensitivity.Slider.value = Settings.MouseSensitivity;
        }

        private void UpdateMouseInvert(bool value) => Settings.InvertMouse.Value = value;
        private void UpdateAutoRun(bool value) => Settings.AutoRun.Value = value;
        private void UpdateMouseSensitivity(float value) => Settings.MouseSensitivity.Value = value;
    }
}
