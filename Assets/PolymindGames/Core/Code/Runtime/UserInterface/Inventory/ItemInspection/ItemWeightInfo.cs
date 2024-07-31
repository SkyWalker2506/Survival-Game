using System;
using PolymindGames.InventorySystem;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ItemWeightInfo : DataInfo
    {
        [SerializeField, NotNull]
        private TextMeshProUGUI _weightTxt;


        public void UpdateInfo(IItem data)
        {
            if (data != null)
                _weightTxt.text = $"{Math.Round(data.Definition.Weight * data.StackCount, 3)} {ItemDefinition.WEIGHT_UNIT}";
            else
                _weightTxt.text = string.Empty;
        }
    }
}