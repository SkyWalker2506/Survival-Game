using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    public enum ItemPropertyType
    {
        Boolean,
        Integer,
        Float,
        Double,
        Item
    }

    /// <summary>
    /// Item properties hold values that can be changed and manipulated at runtime resulting in dynamic behaviour (float, bool and integer).
    /// </summary>
    [Serializable]
    public sealed class ItemProperty
    {
        [SerializeField]
        private int _id;

        [SerializeField]
        private ItemPropertyType _type;

        [SerializeField]
        private double _value;
        
        
        public int Id => _id;
        public string Name => ItemPropertyDefinition.GetWithId(_id).Name;
        public ItemPropertyType Type => _type;

        public bool Boolean
        {
            get => _value > 0f;
            set
            {
                if (_type == ItemPropertyType.Boolean)
                    SetInternalValue(value ? 1 : 0);
            }
        }

        public int Integer
        {
            get => (int)_value;
            set
            {
                if (_type == ItemPropertyType.Integer)
                    SetInternalValue(value);
            }
        }

        public double Double
        {
            get => _value;
            set
            {
                if (_type == ItemPropertyType.Double)
                    SetInternalValue(value);
            }
        }

        public float Float
        {
            get => (float)_value;
            set
            {
                if (_type == ItemPropertyType.Float)
                    SetInternalValue(value);
            }
        }

        public int ItemId
        {
            get => (int)_value;
            set
            {
                if (_type == ItemPropertyType.Item)
                    SetInternalValue(value);
            }
        }


        public ItemProperty(ItemPropertyDefinition definition, double value)
        {
            _id = definition.Id;
            _type = definition.Type;
            _value = value;
        }

        public event PropertyChangedDelegate Changed;

        public ItemProperty Clone() => (ItemProperty)MemberwiseClone();

        private void SetInternalValue(double value)
        {
            double oldValue = _value;
            _value = value;

            if (Math.Abs(oldValue - _value) > 0.001)
                Changed?.Invoke(this);
        }
    }

    public delegate void PropertyChangedDelegate(ItemProperty property);
}