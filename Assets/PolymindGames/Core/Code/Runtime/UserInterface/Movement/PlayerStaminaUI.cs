using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_vitals#player-stamina-ui")]
    public sealed class PlayerStaminaUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup("References")]
        [Tooltip("The canvas group used to fade the stamina bar in & out.")]
        private CanvasGroup _canvasGroup;

        [SerializeField, EndGroup]
        [Tooltip("The stamina bar image, the fill amount will be modified based on the current stamina value.")]
        private Image _staminaBar;

        [SerializeField, Range(1f, 10f), BeginGroup("Settings")]
        [Tooltip("Represents how much time it takes for the stamina bar to start fading after not decreasing.")]
        private float _hideDuration = 4f;

        [SerializeField, Range(0f, 25f), EndGroup]
        [Tooltip("How fast will the stamina bar alpha fade in & out.")]
        private float _alphaLerpSpeed = 4f;

        private IStaminaManagerCC _stamina;
        private float _lastStaminaValue = 1f;
        private float _hideTime;
        private bool _show;


        protected override void OnCharacterAttached(ICharacter character)
        {
            _stamina = Character.GetCC<IStaminaManagerCC>();
            enabled = true;
        }

        protected override void OnCharacterDetached(ICharacter character) => enabled = false;

        protected override void Awake()
        {
            base.Awake();
            enabled = false;
        }

        private void FixedUpdate()
        {
            float stamina = _stamina.Stamina;

            float targetAlpha = _show ? 1f : 0f;
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, _alphaLerpSpeed * Time.deltaTime);
            _staminaBar.fillAmount = stamina / _stamina.MaxStamina;

            if (stamina < _lastStaminaValue)
            {
                _show = true;
                _hideTime = Time.time + _hideDuration;
            }
            else if (Time.time > _hideTime)
                _show = false;

            _lastStaminaValue = stamina;
        }
    }
}