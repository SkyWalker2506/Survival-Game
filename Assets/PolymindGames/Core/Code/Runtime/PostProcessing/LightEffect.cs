using UnityEngine;
using System;

#if POLYMIND_GAMES_FPS_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace PolymindGames
{
    [RequireComponent(typeof(Light))]
    public sealed class LightEffect : MonoBehaviour
    {
        private enum PlayMode
        {
            Once, Loop
        }

        [BeginGroup("Settings")]
        [SerializeField, Range(0f, 5f), Delayed]
        [Tooltip("Intensity of the light effect.")]
        private float _intensity = 1f;

        [SerializeField, Range(0f, 100f), Delayed]
        [Tooltip("Range of the light effect.")]
        private float _range = 1f;

        [SerializeField, EndGroup]
        [Tooltip("Color of the light effect.")]
        private Color _color = Color.yellow;

        [SerializeField, Range(0f, 2f), BeginGroup("Fade")]
        [Tooltip("Duration of the fade-in effect.")]
        private float _fadeInDuration = 0.5f;

        [SerializeField, Range(0f, 2f), EndGroup]
        [Tooltip("Duration of the fade-out effect.")]
        private float _fadeOutDuration = 0.1f;

        [SerializeField, BeginGroup("Pulse"), EndGroup]
        [Tooltip("Settings for pulsing effect.")]
        private PulseSettings _pulse;

        [SerializeField, BeginGroup("Noise"), EndGroup]
        [Tooltip("Settings for noise effect.")]
        private NoiseSettings _noise;

        private bool _isOn = true;
        private bool _pulseActive = true;
        private float _multiplier = 1f;
        private float _pulseTimer;
        private float _weight;
        
#if POLYMIND_GAMES_FPS_HDRP
        private HDAdditionalLightData _hdLight;
#elif UNITY_POST_PROCESSING_STACK_V2
        private Light _light;
#endif

        
        public float Multiplier
        {
            get => _multiplier;
            set => _multiplier = value;
        }

        public void Play(bool fadeIn = true)
        {
            enabled = true;

            _isOn = true;
            _pulseActive = true;
            _pulseTimer = 0f;

            if (!fadeIn)
                _weight = 1f;
        }

        public void Stop(bool fadeOut = true)
        {
            _isOn = false;
            _pulseActive = false;

            if (!fadeOut)
                enabled = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            UpdateWeight(deltaTime);
            UpdateLight(deltaTime);

            if (_weight < 0.0001f)
                enabled = false;
        }

        private void UpdateWeight(float deltaTime)
        {
            float targetWeight = _isOn ? 1f : 0f;
            float fadeDelta = deltaTime * (1f / (_isOn ? _fadeInDuration : _fadeOutDuration));
            _weight = Mathf.MoveTowards(_weight, targetWeight, fadeDelta);
        }

        private void UpdateLight(float deltaTime)
        {
            float intensity = _intensity;
            float range = _range;
            Color color = _color;

            // Pulse handling
            if (_pulse.Enabled && _pulseActive)
            {
                float time = _pulseTimer / Mathf.Max(_pulse.Duration, 0.001f);
                float param = (Mathf.Sin(Mathf.PI * (2f * time - 0.5f)) + 1f) * 0.5f;

                intensity += _intensity * param * _pulse.IntensityAmplitude;
                range += _range * param * _pulse.RangeAmplitude;
                color = Color.Lerp(color, _pulse.Color, param * _pulse.ColorWeight);

                _pulseTimer += deltaTime;

                if (_pulseTimer > _pulse.Duration)
                {
                    if (_pulse.Mode == PlayMode.Once)
                        _pulseActive = false; 

                    _pulseTimer = 0f;
                }

                // Auto stop when all effects finished playing
                if (!_pulseActive)
                    _isOn = false;
            }

            // Noise
            if (_noise.Enabled)
            {
                float noise = Mathf.PerlinNoise(Time.time * _noise.Speed, 0f) * _noise.Intensity;
                intensity += _intensity * noise;
                range += _range * noise;
            }

#if POLYMIND_GAMES_FPS_HDRP
            _hdLight.SetIntensity(intensity * _weight * _multiplier * 500f);
            _hdLight.range = range * _weight * _multiplier;
            _hdLight.color = color;
#elif UNITY_POST_PROCESSING_STACK_V2
            _light.intensity = intensity * _weight * _multiplier;
            _light.range = range * _weight * _multiplier;
            _light.color = color;
#endif
        }

        private void Awake()
        {
#if POLYMIND_GAMES_FPS_HDRP
            _hdLight = GetComponent<HDAdditionalLightData>();
#elif UNITY_POST_PROCESSING_STACK_V2
            _light = GetComponent<Light>();
#endif
        }

        private void OnEnable()
        {
#if POLYMIND_GAMES_FPS_HDRP
            GetComponent<Light>().enabled = true;
#elif UNITY_POST_PROCESSING_STACK_V2
            _light.enabled = true;
#endif
        }

        private void OnDisable()
        {
#if POLYMIND_GAMES_FPS_HDRP
            GetComponent<Light>().enabled = false;
#elif UNITY_POST_PROCESSING_STACK_V2
            _light.enabled = false;
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
#if POLYMIND_GAMES_FPS_HDRP
            if (_hdLight != null || TryGetComponent(out _hdLight) && )
            {
                if (TryGetComponent(out Light light))
                    light.enabled = enabled;

                _hdLight.SetIntensity(_intensity * 500f);
                _hdLight.range = _range;
                _hdLight.color = _color;
            }
#elif UNITY_POST_PROCESSING_STACK_V2
            if (_light != null || TryGetComponent(out _light))
            {
                _light.enabled = enabled;
                _light.intensity = _intensity;
                _light.range = _range;
                _light.color = _color;
            }
#endif
        }
#endif
        
        #region Internal
        [Serializable]
        private struct PulseSettings
        {
            public bool Enabled;

            public PlayMode Mode;

            [Range(0f, 100f), SpaceArea]
            public float Duration;
            
            public Color Color;

            [Range(0f, 1f), SpaceArea]
            public float IntensityAmplitude;

            [Range(0f, 1f)]
            public float RangeAmplitude;

            [Range(0f, 1f)]
            public float ColorWeight;
        }

        [Serializable]
        private struct NoiseSettings
        {
            public bool Enabled;

            [Range(0f, 1f)]
            public float Intensity;

            [Range(0f, 10f)]
            public float Speed;
        }
        #endregion
    }
}