using System;
using PolymindGames.InventorySystem;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class InventoryContainerWeightUI : CharacterUIBehaviour
    {
        [SerializeField, Range(0, 5), BeginGroup]
        private int _decimals = 1;

        [SerializeField]
        private FillBarUI _weightBar;

        [SerializeField, EndGroup]
        private TextMeshProUGUI _weightText;

        private ItemWeightRestriction _weightRestriction;


        protected override void OnCharacterAttached(ICharacter character)
        {
            if (character.Inventory.TryGetRestriction(out _weightRestriction))
            {
                _weightRestriction.WeightChanged += OnWeightChanged;
                OnWeightChanged(_weightRestriction.TotalWeight);
            }
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            if (_weightRestriction != null)
                _weightRestriction.WeightChanged -= OnWeightChanged;
        }

        private void OnWeightChanged(float totalWeight)
        {
            float maxWeight = _weightRestriction.MaxWeight;
            _weightText.text = $"{(float)Math.Round(totalWeight, _decimals)} / {maxWeight} {ItemDefinition.WEIGHT_UNIT}";
            _weightBar.SetFillAmount(totalWeight / maxWeight);
        }
    }
}