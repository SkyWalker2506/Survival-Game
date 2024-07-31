using System;
using PolymindGames.InventorySystem;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{

    [Serializable]
    public sealed class ItemNameInfo : DataInfo
    {
        [SerializeField, NotNull]
        private TextMeshProUGUI _nameTxt;


        public void UpdateInfo(IItem data)
        {
            if (data != null)
            {
                var def = data.Definition;

                _nameTxt.text = def.Name;
                _nameTxt.color = def.Color;
            }
            else
                _nameTxt.text = string.Empty;
        }
    }
}