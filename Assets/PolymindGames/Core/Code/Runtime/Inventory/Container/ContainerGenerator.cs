using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    /// <summary>
    /// Generates an item container instance based on a few parameters.
    /// </summary>
    [Serializable]
    public struct ContainerGenerator : ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("The name of the item container.")]
        public string Name;

        [SerializeField, Range(1, 100)]
        [Tooltip("Number of item slots that this container has (e.g. Holster 8, Backpack 25 etc.).")]
        public int MaxSize;

        [SerializeField, SpaceArea]
        [DataReferenceDetails(HasNullElement = false)]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Tooltip("Only items that are tagged with the specified tag can be added.")]
        public DataIdReference<ItemTagDefinition>[] RequiredTags;

        [SerializeField, SpaceArea(1f)]
        [DataReferenceDetails(HasNullElement = false)]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Tooltip("Only items with the specified properties can be added.")]
        public DataIdReference<ItemPropertyDefinition>[] RequiredProperties;


        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() => MaxSize = Mathf.Max(1, MaxSize);
        
        public readonly ItemContainer GenerateContainer(IInventory inventory)
        {
            var container = new ItemContainer(inventory,
                Name,
                MaxSize,
                GetAllRestrictions()
            );

            return container;
        }

        private readonly ItemRestriction[] GetAllRestrictions()
        {
            var restrictions = new List<ItemRestriction>();

            if (RequiredTags.Length > 0)
                restrictions.Add(new ItemTagRestriction(ItemTagRestriction.AllowType.OnlyWithTags, RequiredTags));

            if (RequiredProperties.Length > 0)
                restrictions.Add(new ItemPropertyRestriction(RequiredProperties));

            return restrictions.ToArray();
        }
    }
}