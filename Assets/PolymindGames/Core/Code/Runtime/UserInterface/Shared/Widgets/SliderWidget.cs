using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class SliderWidget : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private Slider _slider;

        [SerializeField]
        private bool _valueDecimals = false;
        
        [SerializeField, EndGroup]
        private TextMeshProUGUI _valueText;

        [SerializeField, Range(0f, 100f), BeginGroup]
        private float _increment;
        
        [SerializeField]
        [ShowIf(nameof(_increment), 0.001f, Comparison = UnityComparisonMethod.Greater)]
        private ButtonUI _incrementButton;

        [SerializeField, EndGroup]
        [ShowIf(nameof(_increment), 0.001f, Comparison = UnityComparisonMethod.Greater)]
        private ButtonUI _decrementButton;


        public Slider Slider => _slider;
        
        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(UpdateText);
            
            if (_increment > 0.001f)
            {
                _incrementButton.OnSelected += IncrementSlider;
                _decrementButton.OnSelected += DecrementSlider;
            }
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(UpdateText);
            
            if (_increment > 0.001f)
            {
                _incrementButton.OnSelected -= IncrementSlider;
                _decrementButton.OnSelected -= DecrementSlider;
            }
        }

        private void UpdateText(float value) => _valueText.text = value.ToString(_valueDecimals ? "F2" : "F0");
        private void IncrementSlider() => _slider.value += _increment;
        private void DecrementSlider() => _slider.value -= _increment;
    }
}
