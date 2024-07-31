using PolymindGames.ProceduralMotion;
using PolymindGames.WieldableSystem;
using PolymindGames.InventorySystem;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class WieldableDurabilityUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Settings")]
        [Tooltip("The UI text component used for displaying wieldable durability.")]
        private TextMeshProUGUI _text;

        [SerializeField]
        [Tooltip("The gradient used to represent the durability color.")]
        private Gradient _color;

        [SerializeField, Range(0f, 1f), EndGroup]
        [Tooltip("The percentage at which the durability color should change to indicate low durability.")]
        private float _activatePercent = 0.35f;

        [SerializeField, BeginGroup("Animation")]
        [Tooltip("The animation sequence for enabling the durability text.")]
        private TweenAnimation _enableTextAnimation;

        [SerializeField, EndGroup]
        [Tooltip("The animation sequence for changing the durability text.")]
        private TweenAnimation _changeTextAnimation;
        
        private const string NO_DURABILITY = "No Durability";
        private const string LOW_DURABILITY = "Low Durability";
        
        private ItemProperty _durabilityProperty;
        private bool _isVisible;


        protected override void OnCharacterAttached(ICharacter character)
        {
            _text.gameObject.SetActive(false);
            character.GetCC<IWieldableControllerCC>().EquippingStopped += OnWieldableEquipped;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            character.GetCC<IWieldableControllerCC>().EquippingStopped -= OnWieldableEquipped;
        }
        
        private void OnWieldableEquipped(IWieldable wieldable)
        {
            // Unsubscribe from previous firearm
            if (_durabilityProperty != null)
                _durabilityProperty.Changed -= OnDurabilityPropertyOnChanged;

            if (wieldable is MeleeWeapon or Polearm or WieldableTool)
            {
                var wieldableItem = wieldable.gameObject.GetComponent<IWieldableItem>();
                if (wieldableItem != null && wieldableItem.AttachedItem.TryGetPropertyWithId(WieldableItemConstants.DURABILITY, out var durabilityProperty))
                {
                    _durabilityProperty = durabilityProperty;
                    durabilityProperty.Changed += OnDurabilityPropertyOnChanged;
                    OnDurabilityPropertyOnChanged(durabilityProperty);
                }
            }
            else
                SetVisibility(false);
        }

        private void OnDurabilityPropertyOnChanged(ItemProperty property)
        {
            float durabilityPercent = property.Float / WieldableItemConstants.MAX_DURABILITY_VALUE;
            bool isVisible = durabilityPercent < _activatePercent;

            SetVisibility(isVisible);
            
            if (isVisible)
                UpdateText(durabilityPercent);
        }

        private void UpdateText(float percent)
        {
            _text.color = _color.Evaluate(percent);
            _text.text = percent == 0f ? NO_DURABILITY : LOW_DURABILITY;
            _changeTextAnimation.PlayAnimation();
        }

        private void SetVisibility(bool value)
        {
            if (_isVisible == value)
                return;

            if (value)
                _enableTextAnimation.PlayAnimation();
            else
                _enableTextAnimation.CancelAnimation();

            _text.gameObject.SetActive(value);
            _isVisible = value;
        }
    }
}
