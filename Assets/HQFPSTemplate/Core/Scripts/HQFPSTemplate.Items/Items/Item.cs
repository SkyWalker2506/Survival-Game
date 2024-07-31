using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    /// <summary>
    /// Basic inventory item
    /// </summary>
    [Serializable]
    public sealed class Item : IItem, ISerializationCallbackReceiver
    {
        [SerializeField]
        private int _id;

        [SerializeField]
        private int _stackCount;

        [SerializeReference]
        private ItemProperty[] _properties;

        private ItemDefinition _definition;

        
        /// <summary>
        /// Constructor that requires an item definition.
        /// </summary>
        public Item(ItemDefinition itemDef, int count = 1)
        {
            if (itemDef == null)
                throw new NullReferenceException("Cannot create an item from a null item definition.");

            _definition = itemDef;
            _id = itemDef.Id;
            _stackCount = Mathf.Clamp(count, 1, _definition.StackSize);
            _properties = InstantiateProperties(itemDef.GetPropertyGenerators());

            // Listen to the property changed callbacks.
            foreach (var property in _properties)
                property.Changed += _ => PropertyChanged?.Invoke();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Item(IItem item)
        {
            _id = item.Id;
            _definition = item.Definition;
            _stackCount = item.StackCount;
            _properties = CloneProperties(item.Properties);

            // Listen to the property changed callbacks.
            foreach (var property in _properties)
                property.Changed += _ => PropertyChanged?.Invoke();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Item(IItem item, int count)
        {
            _id = item.Id;
            _definition = item.Definition;
            _stackCount = count;
            _properties = CloneProperties(item.Properties);

            // Listen to the property changed callbacks.
            foreach (var property in _properties)
                property.Changed += _ => PropertyChanged?.Invoke();
        }

        public ItemDefinition Definition => _definition;
        public ItemProperty[] Properties => _properties;
        public string Name => Definition.Name;
        public int Id => _id;

        public int StackCount
        {
            get => _stackCount;
            set
            {
                int oldStack = _stackCount;
                _stackCount = Mathf.Clamp(value, 0, _definition.StackSize);

                if (_stackCount == oldStack)
                    return;

                StackCountChanged?.Invoke();
            }
        }

        public float TotalWeight
        {
            get
            {
                if (_properties == Array.Empty<ItemProperty>())
                    return Definition.Weight * _stackCount;

                float weight = Definition.Weight;
                foreach (var prop in _properties)
                {
                    if (prop.Type == ItemPropertyType.Item && prop.ItemId != 0)
                        weight += ItemDefinition.GetWithId(prop.ItemId).Weight;
                }

                return weight * _stackCount;
            }
        }

        public event UnityAction PropertyChanged;
        public event UnityAction StackCountChanged;

        public override string ToString() => "Item Name: " + Name + " | Stack Size: " + _stackCount;

        private ItemProperty[] CloneProperties(ItemProperty[] properties)
        {
            var clonedProperties = new ItemProperty[properties.Length];

            for (int i = 0; i < properties.Length; i++)
                clonedProperties[i] = properties[i].Clone();

            return clonedProperties;
        }

        private ItemProperty[] InstantiateProperties(ItemPropertyGenerator[] propertyGenerators)
        {
            if (propertyGenerators == null || propertyGenerators.Length == 0)
                return Array.Empty<ItemProperty>();

            var properties = new ItemProperty[propertyGenerators.Length];

            for (int i = 0; i < propertyGenerators.Length; i++)
                properties[i] = propertyGenerators[i].GenerateItemProperty();

            return properties;
        }

		#region Save & Load
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() => _definition = ItemDefinition.GetWithId(_id);
		#endregion
    }
}