using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class AudioOptionsUI : UserOptionsUI<AudioOptions>
    {
        [SerializeField, BeginGroup]
        private SliderWidget _masterVolumeSlider;
        
        [SerializeField]
        private SliderWidget _effectsVolumeSlider;
        
        [SerializeField]
        private SliderWidget _ambienceVolumeSlider;
        
        [SerializeField] 
        private SliderWidget _musicVolumeSlider;
        
        [SerializeField, EndGroup]
        private SliderWidget _uiVolumeSlider;
        

        protected override void InitializeWidgets()
        {
            _masterVolumeSlider.Slider.onValueChanged.AddListener(MasterVolumeChanged);
            _effectsVolumeSlider.Slider.onValueChanged.AddListener(EffectsVolumeChanged);
            _ambienceVolumeSlider.Slider.onValueChanged.AddListener(AmbienceVolumeChanged);
            _musicVolumeSlider.Slider.onValueChanged.AddListener(MusicVolumeChanged);
            _uiVolumeSlider.Slider.onValueChanged.AddListener(UIVolumeChanged);
        }

        protected override void ResetWidgets()
        {
            _masterVolumeSlider.Slider.value = Settings.MasterVolume * 100f;
            _effectsVolumeSlider.Slider.value = Settings.EffectsVolume * 100f;
            _ambienceVolumeSlider.Slider.value = Settings.AmbienceVolume * 100f;
            _musicVolumeSlider.Slider.value = Settings.MusicVolume * 100f;
            _uiVolumeSlider.Slider.value = Settings.UIVolume * 100f;
        }

        private void MasterVolumeChanged(float value) => Settings.MasterVolume.Value = value * 0.01f;
        private void EffectsVolumeChanged(float value) => Settings.EffectsVolume.Value = value * 0.01f;
        private void AmbienceVolumeChanged(float value) => Settings.AmbienceVolume.Value = value * 0.01f;
        private void MusicVolumeChanged(float value) => Settings.MusicVolume.Value = value * 0.01f;
        private void UIVolumeChanged(float value) => Settings.UIVolume.Value = value * 0.01f;
    }
}