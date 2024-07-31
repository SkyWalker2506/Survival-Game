using PolymindGames.WieldableSystem;
using UnityEngine.UI;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_wieldables#charge")]
    public sealed class FirearmChargeUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup]
        [Tooltip("The canvas group used to fade the stamina bar in & out.")]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        [Tooltip("A gradient used in determining the color of the charge image relative to the current charge value.")]
        private Gradient _fillGradient;

        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false), EndGroup]
        [Tooltip("UI images that will have their fill amount value set to the current charge value.")]
        private Image[] _chargeFillImages;

        private IChargeHandler _chargeHandler;
        private IFirearm _firearm;


        protected override void Awake()
        {
            base.Awake();
            _canvasGroup.alpha = 0f;
            enabled = false;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            var wieldables = character.GetCC<IWieldableControllerCC>();
            wieldables.EquippingStopped += OnWieldableEquipped;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var wieldables = character.GetCC<IWieldableControllerCC>();
            wieldables.EquippingStopped -= OnWieldableEquipped;
        }

        private void OnWieldableEquipped(IWieldable wieldable)
        {
            // Unsubscribe from previous firearm
            if (_firearm != null)
            {
                _firearm.RemoveChangedListener(AttachmentType.Trigger, OnTriggerChanged);
                _firearm = null;
                OnTriggerChanged();
            }

            if (wieldable is IFirearm firearm)
            {
                // Subscribe to current firearm
                _firearm = firearm;
                _firearm.AddChangedListener(AttachmentType.Trigger, OnTriggerChanged);
                OnTriggerChanged();
            }
        }

        private void OnTriggerChanged()
        {
            var prevCharge = _chargeHandler;
            _chargeHandler = _firearm?.Trigger as IChargeHandler;

            if (_chargeHandler != null && prevCharge == null)
            {
                enabled = true;
                return;
            }

            if (_chargeHandler == null && prevCharge != null)
            {
                UpdateChargeImages(0f);
                _canvasGroup.alpha = 0f;
                enabled = false;
            }
        }

        private void Update()
        {
            float normalizedCharge = _chargeHandler.GetNormalizedCharge();
            UpdateChargeImages(normalizedCharge);

            bool showCanvas = normalizedCharge > 0.01f;

            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, showCanvas ? 1f : 0f, Time.deltaTime * 5f);
        }

        private void UpdateChargeImages(float fillAmount)
        {
            Color chargeColor = _fillGradient.Evaluate(fillAmount);
            for (int i = 0; i < _chargeFillImages.Length; i++)
            {
                _chargeFillImages[i].fillAmount = fillAmount;
                _chargeFillImages[i].color = chargeColor;
            }
        }
    }
}