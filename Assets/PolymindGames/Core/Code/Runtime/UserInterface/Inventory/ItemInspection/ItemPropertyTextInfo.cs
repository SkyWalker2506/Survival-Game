using System;
using System.Globalization;
using PolymindGames.InventorySystem;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ItemPropertyTextInfo : DataInfo
    {
        [Serializable]
        private struct FillBarProperty
        {
            public DataIdReference<ItemPropertyDefinition> Property;

            [SpaceArea, NotNull]
            public TextMeshProUGUI Text;

            [Range(0, 10)]
            public int Decimals;
        }

        [SerializeField, ReorderableList(ListStyle.Lined, elementLabel: "Text Property")]
        private FillBarProperty[] _properties;


        public void UpdateInfo(IItem data)
        {
            if (data != null)
            {
                foreach (var txtProperty in _properties)
                    txtProperty.Text.enabled = true;

                foreach (var txtProperty in _properties)
                {
                    if (data.TryGetPropertyWithId(txtProperty.Property, out var prop))
                    {
                        if (prop.Type != ItemPropertyType.Integer && prop.Type != ItemPropertyType.Float)
                            continue;

                        string str = Math.Round(prop.Float, txtProperty.Decimals).ToString(CultureInfo.InvariantCulture);
                        txtProperty.Text.text = str;
                    }
                    else
                        txtProperty.Text.text = string.Empty;
                }
            }
            else
            {
                foreach (var txtProperty in _properties)
                    txtProperty.Text.enabled = false;
            }
        }
    }
}