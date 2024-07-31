using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class FPSCounterUI : MonoBehaviour
    {
        [SerializeField, Range(30f, 1000f), BeginGroup]
        private float _requiredFPS;

        [SerializeField, EndGroup]
        private Gradient _colorGradient;
        
        private int _currentFps;
        private int _fpsAccumulator;
        private float _fpsNextPeriod;
        private TextMeshProUGUI _text;

        private const float FPS_MEASURE_PERIOD = 0.5f;
        private const string DISPLAY = "{0} FPS";


        private void Start()
        {
            _fpsNextPeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            // Measure average frames per second
            _fpsAccumulator++;

            if (Time.realtimeSinceStartup > _fpsNextPeriod)
            {
                _currentFps = (int)(_fpsAccumulator / FPS_MEASURE_PERIOD);
                _fpsAccumulator = 0;
                _fpsNextPeriod += FPS_MEASURE_PERIOD;

                _text.text = string.Format(DISPLAY, _currentFps);
                _text.color = _colorGradient.Evaluate(_currentFps / _requiredFPS);
            }
        }
    }
}