using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(PanelUI))]
    public sealed class GraphicsOptionsUI : UserOptionsUI<GraphicsOptions>
    {
        [SerializeField, BeginGroup("Dropdowns")]
        private TMP_Dropdown _qualityDropdown; 
        
        [SerializeField]
        private TMP_Dropdown _fullscreenModeDropdown;

        [SerializeField, EndGroup]
        private TMP_Dropdown _resolutionDropdown;

        [SerializeField, BeginGroup("Sliders")]
        private SliderWidget _fieldOfView;
        
        [SerializeField, EndGroup]
        private SliderWidget _frameRateCap;
        
        [SerializeField, BeginGroup("Toggles"), EndGroup]
        private Toggle _vSyncToggle;

        private List<Resolution> _filteredResolutions;


        protected override void InitializeWidgets()
        {
            InitializeFullscreenModeDropdown();
            InitializeFrameRateCapSlider();
            InitializeResolutionDropdown();
            InitializeFieldOfViewSlider();
            InitializeQualityDropdown();
            InitializeVSyncToggle();
        }

        protected override void ResetWidgets()
        {
            _fullscreenModeDropdown.value = (int)Settings.FullscreenMode.Value.Value;
            _frameRateCap.Slider.value = Settings.FrameRateCap.Value;
            _resolutionDropdown.value = _filteredResolutions.FindIndex(resolution => resolution.width == Settings.Resolution.Value.x);
            _fieldOfView.Slider.value = Settings.FieldOfView.Value;
            _qualityDropdown.value = Settings.Quality.Value;
        }

        private void InitializeFullscreenModeDropdown()
        {
            var options = Enum.GetNames(typeof(FullScreenMode)).Select(str => str.AddSpaceBeforeCapitalLetters()).ToList();
            var dropdown = _fullscreenModeDropdown;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(UpdateFullscreenMode);
        }
        
        private void InitializeFrameRateCapSlider()
        {
            var slider = _frameRateCap.Slider;
            slider.maxValue = GraphicsOptions.MAX_FRAMERATE;
            slider.minValue = 0f;
            slider.onValueChanged.AddListener(UpdateFrameRateCap);
        }

        private void InitializeResolutionDropdown()
        {
            var resolutions = Screen.resolutions;
            var maxRefreshRate = resolutions.Max(resolution => resolution.refreshRateRatio).value;
            _filteredResolutions = resolutions.Where(resolution => resolution.refreshRateRatio.value == maxRefreshRate).ToList();
            
            var options = _filteredResolutions.Select(resolution => $"{resolution.width} x {resolution.height}").ToList();
            var dropdown = _resolutionDropdown;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(UpdateResolution);
        }

        private void InitializeFieldOfViewSlider()
        {
            var slider = _fieldOfView.Slider;
            slider.minValue = GraphicsOptions.MIN_FOV;
            slider.maxValue = GraphicsOptions.MAX_FOV;
            slider.onValueChanged.AddListener(UpdateFieldOfView);
        }

        private void InitializeQualityDropdown()
        {
            var dropdown = _qualityDropdown;
            dropdown.ClearOptions();
            dropdown.AddOptions(QualitySettings.names.ToList());
            dropdown.onValueChanged.AddListener(UpdateQuality);
        }

        private void InitializeVSyncToggle()
        {
            _vSyncToggle.onValueChanged.AddListener(UpdateVSync);
        }

        private void UpdateFullscreenMode(int value) =>
            Settings.FullscreenMode.Value = (FullScreenMode)_fullscreenModeDropdown.value;

        private void UpdateResolution(int value)
        {
            var resolution = _filteredResolutions[value];
            Settings.Resolution.Value = new Vector2Int(resolution.width, resolution.height);
        }
        
        private void UpdateFrameRateCap(float value)
        {
            int frameRateCap = (int)_frameRateCap.Slider.value;
            Settings.FrameRateCap.Value = frameRateCap == 0 ? -1 : frameRateCap;
        }

        private void UpdateFieldOfView(float value) => Settings.FieldOfView.Value = value;
        private void UpdateQuality(int value) => Settings.Quality.Value = value;
        private void UpdateVSync(bool value) => Settings.VSyncMode.Value = value ? 1 : 0;
    }
}