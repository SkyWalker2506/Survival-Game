using System;
using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [Serializable]
    public sealed class ItemPropertyFillBarInfo : DataInfo
    {
        [Serializable]
        private struct FillBarProperty
        {
            public DataIdReference<ItemPropertyDefinition> Property;

            [SpaceArea]
            public FillBarUI FillBar;

            [MinMaxSlider(0f, 1000f)]
            public Vector2 Range;
        }

        [SerializeField, ReorderableList(ListStyle.Lined, elementLabel: "Property")]
        private FillBarProperty[] _properties;


        public void UpdateInfo(IItem data)
        {
            if (data != null)
            {
                foreach (var fillProperty in _properties)
                    fillProperty.FillBar.SetActive(true);

                foreach (var fillProperty in _properties)
                {
                    if (data.TryGetPropertyWithId(fillProperty.Property, out var prop))
                    {
                        if (prop.Type != ItemPropertyType.Integer && prop.Type != ItemPropertyType.Float)
                            continue;

                        float value = prop.Float;
                        value = Mathf.Clamp(value, fillProperty.Range.x, fillProperty.Range.y);
                        float fillAmount = value / fillProperty.Range.y;
                        fillProperty.FillBar.SetFillAmount(fillAmount);
                        fillProperty.FillBar.SetActive(true);
                    }
                    else
                        fillProperty.FillBar.SetActive(false);
                }
            }
            else
            {
                foreach (var fillProperty in _properties)
                    fillProperty.FillBar.SetActive(false);
            }
        }
    }
}